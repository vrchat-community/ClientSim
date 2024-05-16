using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// The system responsible for managing the mouse position for both ClientSim raycasting
    /// and for Unity's EventSystem to know where to raycast for UI elements.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimMouseReleasedEvent
    /// - ClientSimCurrentHandEvent
    /// Listens to Events:
    /// - ClientSimMenuStateChangedEvent
    /// - ClientSimRaycastHitResultsEvent
    /// Listens to Input Events:
    /// - Use
    /// - ReleaseMouse
    /// </remarks>
    [AddComponentMenu("")]
    // Update FrameTick at the end of frame so that Input from playmode and test can be processed in the same order.
    [DefaultExecutionOrder(10000)] 
    class ClientSimBaseInput : BaseInput, IClientSimMousePositionProvider, IDisposable
    {
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimInput _input;
        private ClientSimSettings _settings;

        private bool _menuIsOpen;
        private bool _mouseReleaseKeyIsDown;
        private bool _prevMouseReleased;

        // Used for interacting with UI
        private int _frameTick = 0;
        private bool _rightUseDown = false;
        private int _rightUseChangeTick = -1;
        private bool _leftUseDown = false;
        private int _leftUseChangeTick = -1;
        
        private HandType _lastHandUsed = HandType.RIGHT;
        private Camera _playerCamera = null;
        private bool _uiShapeHit = false;
        private Vector3 _raycastMousePosition;

        public static Vector2 GetScreenCenter()
        {
            return new Vector2(Screen.width, Screen.height) * 0.5f;
        }

        public Vector2 GetMousePosition()
        {
            if (IsMouseFree())
            {
                // Due to having multiple inputs enabled or disabled, this method ensures no errors are thrown even
                // if setup is incorrect.
#if ENABLE_INPUT_SYSTEM
                // TODO if gamepad input, emulate mouse position to allow clicking on menus.
                return UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return base.mousePosition;
#else
                return Vector2.zero;
#endif
            }
            return GetScreenCenter();
        }
        
        protected override void Awake()
        {
            base.Awake();
            this.PreventComponentFromSaving();
        }
        
        public void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimInput input, 
            ClientSimSettings settings)
        {
            // Do not lock mouse if the player is never spawned.
            if (!settings.spawnPlayer)
            {
                enabled = false;
                return;
            }
            
            _eventDispatcher = eventDispatcher;
            _input = input;
            _settings = settings;
            
            _eventDispatcher.Subscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            _eventDispatcher.Subscribe<ClientSimRaycastHitResultsEvent>(OnRaycastHit);
            
            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeReleaseMouse(InputMouseReleased);
            _input?.SubscribeUse(InputUse);
        }

        protected override void Start()
        {
            base.Start();

            // TODO properly pass in the camera provider instead of using this method.
            _playerCamera = VRC_UiShape.GetEventCamera?.Invoke(this.gameObject);

            foreach (var canvas in FindObjectsOfType<Canvas>())
            {
                if ((canvas.renderMode == RenderMode.WorldSpace) && (canvas.worldCamera == null))
                    canvas.worldCamera = _playerCamera;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }

        public void Dispose()
        {
            _eventDispatcher?.Unsubscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            _eventDispatcher?.Unsubscribe<ClientSimRaycastHitResultsEvent>(OnRaycastHit);
            
            _input?.UnsubscribeReleaseMouse(InputMouseReleased);
            _input?.UnsubscribeUse(InputUse);
        }

        private void Update()
        {
            // Update mouse lock every frame to ensure it is always locked when needed.
            InternalLockUpdate();
            
            // TODO Move this to input system and support checking when Use input is down or up on the current frame.
            ++_frameTick;
        }

        #region Overrides

        // Use the screenspace value of the last raycast hit position as the current mouse position.
        // Using the raycast position decouples Desktop and VR's input source, allowing both to interact with UI without
        // knowing the source of the raycast (mouse vs controller position)
        public override Vector2 mousePosition => _raycastMousePosition;

        public override bool GetMouseButton(int button)
        {
            return _lastHandUsed == HandType.RIGHT ? _rightUseDown : _leftUseDown;
        }
        
        public override bool GetMouseButtonUp(int button)
        {
            return 
                _lastHandUsed == HandType.RIGHT 
                    ? (!_rightUseDown && _rightUseChangeTick == _frameTick)
                    : (!_leftUseDown && _leftUseChangeTick == _frameTick);
        }
        
        public override bool GetMouseButtonDown(int button)
        {
            return 
                _lastHandUsed == HandType.RIGHT 
                ? (_rightUseDown && _rightUseChangeTick == _frameTick)
                : (_leftUseDown && _leftUseChangeTick == _frameTick);
        }
        
        // Override mouse scroll method to prevent errors when input settings are incorrectly setup on first import.
        public override Vector2 mouseScrollDelta
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return UnityEngine.InputSystem.Mouse.current?.scroll.ReadValue() ?? Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return base.mouseScrollDelta;
#else
                return Vector2.zero;
#endif
            }
        }

        // Override generic axis method to prevent errors when input settings are incorrectly setup on first import.
        public override float GetAxisRaw(string axisName)
        {
            if (axisName == "Horizontal")
            {
                return _input?.GetMovementHorizontal() ?? 0;
            }
            if (axisName == "Vertical")
            {
                return _input?.GetMovementVertical() ?? 0;
            }
            
            return 0f;
        }

        // Override generic button method to prevent errors when input settings are incorrectly setup on first import.
        public override bool GetButtonDown(string buttonName)
        {
            if (buttonName == "Horizontal")
            {
                return Mathf.Abs(_input?.GetMovementHorizontal() ?? 0) > 0.5;
            }
            if (buttonName == "Vertical")
            {
                return Mathf.Abs(_input?.GetMovementVertical() ?? 0) > 0.5;
            }
            
            return false;
        }

        #endregion
        
        #region ClientSim Events

        private void SetMenuOpen(ClientSimMenuStateChangedEvent stateChangedEvent)
        {
            _menuIsOpen = stateChangedEvent.isMenuOpen;
            CheckMouseRelease();
        }
        
        private void OnRaycastHit(ClientSimRaycastHitResultsEvent hitEvent)
        {
            if (_lastHandUsed == hitEvent.handType)
            {
                var hitResults = hitEvent.raycastResults;
                _uiShapeHit = hitResults != null && hitResults.uiShape != null;

                _raycastMousePosition = GetScreenCenter();
                // If there is a player camera and there was a hit point, convert the world point to screen space.
                // Transforming it now instead of when requested to ensure that player position updates do not affect
                // interacting with the menu.
                if (hitResults != null && _playerCamera != null)
                {
                    _raycastMousePosition = _playerCamera.WorldToScreenPoint(hitResults.hitPoint);
                }
            }
        }
        
        #endregion

        #region ClientSim Input Events
        
        private void InputMouseReleased(bool value)
        {
            _mouseReleaseKeyIsDown = value;
            CheckMouseRelease();
        }
        
        private void InputUse(bool value, HandType handType)
        {
            if (value)
            {
                if (_lastHandUsed != handType)
                {
                    _lastHandUsed = handType;
                    _eventDispatcher.SendEvent(new ClientSimCurrentHandEvent { currentUsedHand = _lastHandUsed });
                }
            }

            if (handType == HandType.RIGHT)
            {
                _rightUseDown = value;
                _rightUseChangeTick = _frameTick;
            }
            else
            {
                _leftUseDown = value;
                _leftUseChangeTick = _frameTick;
            }
        }

        #endregion

        public bool HitUIShape()
        {
            return _uiShapeHit; 
        }
        
        private bool IsMouseFree()
        {
            return _mouseReleaseKeyIsDown || _menuIsOpen;
        }

        private void CheckMouseRelease()
        {
            bool released = IsMouseFree();
            if (released != _prevMouseReleased)
            {
                _prevMouseReleased = released;
                _eventDispatcher.SendEvent(new ClientSimMouseReleasedEvent { isReleased = released });
            }
            InternalLockUpdate();
        }
        
        private void InternalLockUpdate()
        {
            // If the menu is open or the tab key is held down, do not lock the mouse and show the cursor.
            // TODO Check if TrackingProvider is VR and do not lock mouse.
            if (IsMouseFree() || ClientSimRuntimeLoader.IsInUnityTest())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Else hide the cursor and lock the cursor to the center of the screen.
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}