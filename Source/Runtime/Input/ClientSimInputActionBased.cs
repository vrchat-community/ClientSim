
#if ENABLE_INPUT_SYSTEM
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using VRC.SDKBase;
#endif

using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    // TODO refactor Input system to queue events from Unity's input system instead of firing events right away.
    // Unity's input events are processed and sent before ALL monobehaviours. In test environment, this will happen
    // right after modifying the input device, which will happen at the end of the frame instead. In order to properly
    // process input events the same in runtime and tests, the events will need to be queued and processed in mono behaviours.
    // This would also allow for other event types to be included that does not use the Input System (eg vr controls).
    
    /// <summary>
    /// An implementation of ClientSimInput that receives input events from Unity's Input System using InputActions.
    /// Events from the action system are then sent to systems listening to input events.
    /// </summary>
    public class ClientSimInputActionBased : ClientSimInputBase
    {
        private readonly ClientSimSettings _settings;
        
/* Items need to be wrapped in this define to prevent compiler errors on initial import
 when the Input System has not yet been imported or enabled.*/
#if ENABLE_INPUT_SYSTEM
        private readonly InputAction _movementHorizontal;
        private readonly InputAction _movementVertical;
        private readonly InputAction _lookHorizontal;
        private readonly InputAction _lookVertical;
        
        private readonly InputAction _jumpLeft;
        private readonly InputAction _jumpRight;
        private readonly InputAction _useLeft;
        private readonly InputAction _useRight;
        private readonly InputAction _grabLeft;
        private readonly InputAction _grabRight;
        private readonly InputAction _dropLeft;
        private readonly InputAction _dropRight;
        private readonly InputAction _toggleMenuLeft;
        private readonly InputAction _toggleMenuRight;
        
        #region NonVR Only

        private readonly InputAction _run;
        private readonly InputAction _toggleCrouch;
        private readonly InputAction _toggleProne;
        private readonly InputAction _releaseMouse;

        private readonly InputAction _pickupRotateUpDown;
        private readonly InputAction _pickupRotateLeftRight;
        private readonly InputAction _pickupRotateCwCcw;
        private readonly InputAction _pickupManipulateForwardBack;

        #endregion

        private InputActionAsset InputActions;
        
        public ClientSimInputActionBased(InputActionAsset actionAsset, ClientSimSettings settings)
        {
            InputActions = actionAsset;
            
            _settings = settings;
            
            _lookHorizontal = actionAsset["LookHorizontal"];
            _lookVertical = actionAsset["LookVertical"];
            _movementHorizontal = actionAsset["MovementHorizontal"];
            _movementVertical = actionAsset["MovementVertical"];
            
            _jumpLeft = actionAsset["JumpLeft"];
            _jumpRight = actionAsset["JumpRight"];
            _useLeft = actionAsset["UseLeft"];
            _useRight = actionAsset["UseRight"];
            _grabLeft = actionAsset["GrabLeft"];
            _grabRight = actionAsset["GrabRight"];
            _dropLeft = actionAsset["DropLeft"];
            _dropRight = actionAsset["DropRight"];
            _toggleMenuLeft = actionAsset["ToggleMenuLeft"];
            _toggleMenuRight = actionAsset["ToggleMenuRight"];
            
            // Desktop only input options.
            _run = actionAsset["Run"];
            _toggleCrouch = actionAsset["ToggleCrouch"];
            _toggleProne = actionAsset["ToggleProne"];
            _releaseMouse = actionAsset["ReleaseMouse"];

            _pickupRotateUpDown = actionAsset["PickupRotateUpDown"];
            _pickupRotateLeftRight = actionAsset["PickupRotateLeftRight"];
            _pickupRotateCwCcw = actionAsset["PickupRotateCwCcw"];
            _pickupManipulateForwardBack = actionAsset["PickupManipulateForwardBack"];


            _jumpLeft.performed += HandleJumpLeft;
            _jumpLeft.canceled += HandleJumpLeft;
            _jumpRight.performed += HandleJumpRight;
            _jumpRight.canceled += HandleJumpRight;
            
            _useLeft.performed += HandleUseLeft;
            _useLeft.canceled += HandleUseLeft;
            _useRight.performed += HandleUseRight;
            _useRight.canceled += HandleUseRight;
            
            _grabLeft.performed += HandleGrabLeft;
            _grabLeft.canceled += HandleGrabLeft;
            _grabRight.performed += HandleGrabRight;
            _grabRight.canceled += HandleGrabRight;
            
            _dropLeft.performed += HandleDropLeft;
            _dropLeft.canceled += HandleDropLeft;
            _dropRight.performed += HandleDropRight;
            _dropRight.canceled += HandleDropRight;
            
            _toggleMenuLeft.performed += HandleToggleMenuLeft;
            _toggleMenuLeft.canceled += HandleToggleMenuLeft;
            _toggleMenuRight.performed += HandleToggleMenuRight;
            _toggleMenuRight.canceled += HandleToggleMenuRight;

            _run.performed += HandleRun;
            _run.canceled += HandleRun;
            _toggleCrouch.performed += HandleToggleCrouch;
            _toggleCrouch.canceled += HandleToggleCrouch;
            _toggleProne.performed += HandleToggleProne;
            _toggleProne.canceled += HandleToggleProne;
            _releaseMouse.performed += HandleReleaseMouse;
            _releaseMouse.canceled += HandleReleaseMouse;
            
            foreach (InputAction action in actionAsset)
            {
                action.performed += SetInputDevice;
            }
        }
        
        private VRCInputMethod _lastInputMethod = VRCInputMethod.Count;

        public VRCInputMethod LastInputMethod
        {
            get => _lastInputMethod;
            set
            {
                // Only send events for changes
                if(_lastInputMethod == value) return;
                
                _lastInputMethod = value;
                SendInputMethodChangedEvent(_lastInputMethod);
            }
        }

        private const string MOUSE_LOOK_PATTERN = @"Player/Look.*Mouse/delta";
        bool isTouchActive = false;
        private void SetInputDevice(InputAction.CallbackContext ctx)
        {
            if(Regex.IsMatch(ctx.action.ToString(), MOUSE_LOOK_PATTERN)) return; // This is to avoid changing the tooltip icon when looking with a mouse
            isTouchActive = (ctx.control.device.description.empty || Touchscreen.current != null && ctx.control.device.GetType().Equals(Touchscreen.current.GetType())); // On screen controls
            if (!isTouchActive)
            {
                if (Keyboard.current != null && ctx.control.device.GetType().Equals(Keyboard.current.GetType()))
                {
#if VRC_MOBILE && UNITY_ANDROID
                //This prevents the android back button from switching to keyboard and disabling touch UI
                if (ctx.control.device.description.interfaceName == "Android")
                    return;
#endif
                    LastInputMethod = VRCInputMethod.Keyboard;
                    return;
                }
                if (Mouse.current != null && ctx.control.device.GetType().Equals(Mouse.current.GetType()))
                {
                    LastInputMethod = VRCInputMethod.Mouse;
                    return;
                }

                if (Gamepad.current != null && ctx.control.device.GetType().Equals(Gamepad.current.GetType()))
                {
                    LastInputMethod = VRCInputMethod.Controller;
                    return;
                }
            }
            else
            {
                LastInputMethod = VRCInputMethod.Touch;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            // Go through and unsubscribe from all input actions
            _jumpLeft.performed -= HandleJumpLeft;
            _jumpLeft.canceled -= HandleJumpLeft;
            _jumpRight.performed -= HandleJumpRight;
            _jumpRight.canceled -= HandleJumpRight;
            
            _useLeft.performed -= HandleUseLeft;
            _useLeft.canceled -= HandleUseLeft;
            _useRight.performed -= HandleUseRight;
            _useRight.canceled -= HandleUseRight;
            
            _grabLeft.performed -= HandleGrabLeft;
            _grabLeft.canceled -= HandleGrabLeft;
            _grabRight.performed -= HandleGrabRight;
            _grabRight.canceled -= HandleGrabRight;
            
            _dropLeft.performed -= HandleDropLeft;
            _dropLeft.canceled -= HandleDropLeft;
            _dropRight.performed -= HandleDropRight;
            _dropRight.canceled -= HandleDropRight;
            
            _toggleMenuLeft.performed -= HandleToggleMenuLeft;
            _toggleMenuLeft.canceled -= HandleToggleMenuLeft;
            _toggleMenuRight.performed -= HandleToggleMenuRight;
            _toggleMenuRight.canceled -= HandleToggleMenuRight;

            _run.performed -= HandleRun;
            _run.canceled -= HandleRun;
            _toggleCrouch.performed -= HandleToggleCrouch;
            _toggleCrouch.canceled -= HandleToggleCrouch;
            _toggleProne.performed -= HandleToggleProne;
            _toggleProne.canceled -= HandleToggleProne;
            _releaseMouse.performed -= HandleReleaseMouse;
            _releaseMouse.canceled -= HandleReleaseMouse;
        }


        private void HandleJumpLeft(InputAction.CallbackContext context)
        {
            SendJumpEvent(context.ReadValueAsButton(), HandType.LEFT);
        }
        
        private void HandleJumpRight(InputAction.CallbackContext context)
        {
            SendJumpEvent(context.ReadValueAsButton(), HandType.RIGHT);
        }
        
        private void HandleUseLeft(InputAction.CallbackContext context)
        {
            SendUseEvent(context.ReadValueAsButton(), HandType.LEFT);
        }
        
        private void HandleUseRight(InputAction.CallbackContext context)
        {
            SendUseEvent(context.ReadValueAsButton(), HandType.RIGHT);
        }
        
        private void HandleGrabLeft(InputAction.CallbackContext context)
        {
            SendGrabEvent(context.ReadValueAsButton(), HandType.LEFT);
        }
        
        private void HandleGrabRight(InputAction.CallbackContext context)
        {
            SendGrabEvent(context.ReadValueAsButton(), HandType.RIGHT);
        }
        
        private void HandleDropLeft(InputAction.CallbackContext context)
        {
            SendDropEvent(context.ReadValueAsButton(), HandType.LEFT);
        }
        
        private void HandleDropRight(InputAction.CallbackContext context)
        {
            SendDropEvent(context.ReadValueAsButton(), HandType.RIGHT);
        }
        
        private void HandleToggleMenuLeft(InputAction.CallbackContext context)
        {
            SendToggleMenuEvent(context.ReadValueAsButton(), HandType.LEFT);
        }
        
        private void HandleToggleMenuRight(InputAction.CallbackContext context)
        {
            SendToggleMenuEvent(context.ReadValueAsButton(), HandType.RIGHT);
        }
        
        private void HandleRun(InputAction.CallbackContext context)
        {
            SendRunEvent(context.ReadValueAsButton());
        }
        
        private void HandleToggleCrouch(InputAction.CallbackContext context)
        {
            SendToggleCrouchEvent(context.ReadValueAsButton());
        }
        
        private void HandleToggleProne(InputAction.CallbackContext context)
        {
            SendToggleProneEvent(context.ReadValueAsButton());
        }
        
        private void HandleReleaseMouse(InputAction.CallbackContext context)
        {
            SendReleaseMouseEvent(context.ReadValueAsButton());
        }
#endif
        
        public override float GetMovementHorizontal()
        {
#if ENABLE_INPUT_SYSTEM
            return _movementHorizontal.ReadValue<float>();
#else
            return 0;
#endif
        }
        
        public override float GetMovementVertical()
        {
#if ENABLE_INPUT_SYSTEM
            return _movementVertical.ReadValue<float>();
#else
            return 0;
#endif
        }
        
        public override float GetLookHorizontal()
        {
#if ENABLE_INPUT_SYSTEM
            if (_pickupRotateLeftRight.ReadValue<float>() != 0)
            {
                return 0;
            }
            return _lookHorizontal.ReadValue<float>();
#else
            return 0;
#endif
        }
        
        public override float GetLookVertical()
        {
#if ENABLE_INPUT_SYSTEM
            if (_pickupRotateUpDown.ReadValue<float>() != 0)
            {
                return 0;
            }
            return _lookVertical.ReadValue<float>() * (_settings.invertMouseLook ? -1 : 1);
#else
            return 0;
#endif
        }

        public override float GetPickupRotateUpDown()
        {
#if ENABLE_INPUT_SYSTEM
            return _pickupRotateUpDown.ReadValue<float>();
#else
            return 0;
#endif
        }

        public override float GetPickupRotateLeftRight()
        {
#if ENABLE_INPUT_SYSTEM
            return _pickupRotateLeftRight.ReadValue<float>();
#else
            return 0;
#endif
        }
        
        public override float GetPickupRotateCwCcw()
        {
#if ENABLE_INPUT_SYSTEM
            return _pickupRotateCwCcw.ReadValue<float>();
#else
            return 0;
#endif
        }
        
        public override float GetPickupManipulateDistance()
        {
#if ENABLE_INPUT_SYSTEM
            return _pickupManipulateForwardBack.ReadValue<float>();
#else
            return 0;
#endif
        }
    }
}