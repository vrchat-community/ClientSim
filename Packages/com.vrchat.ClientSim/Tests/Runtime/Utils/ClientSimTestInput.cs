using System;
using JetBrains.Annotations;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace VRC.SDK3.ClientSim.Tests
{
    public class ClientSimTestInput : IDisposable
    {
#if ENABLE_INPUT_SYSTEM
        private readonly InputTestFixture _inputTestFixture;
        private readonly Keyboard _keyboard;
        private readonly Mouse _mouse;
#endif

        private bool _initialized = false;
        
        public ClientSimTestInput()
        {
#if ENABLE_INPUT_SYSTEM
            _inputTestFixture = new InputTestFixture();
            _inputTestFixture.Setup();
            
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
#endif
            _initialized = true;
        }

        ~ClientSimTestInput()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_initialized)
            {
                return;
            }
            _initialized = false;
            
#if ENABLE_INPUT_SYSTEM
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.RemoveDevice(_mouse);
            
            _inputTestFixture.TearDown();
#endif
        }
        
#if ENABLE_INPUT_SYSTEM
        private void SetControlKey(bool value, ButtonControl key)
        {
            if (value)
            {
                _inputTestFixture.Press(key);
            }
            else
            {
                _inputTestFixture.Release(key);
            }
        }
#endif

        [PublicAPI]
        public void SetInputJump(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.spaceKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputUseGrab(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _mouse.leftButton);
#endif
        }
        
        [PublicAPI]
        public void SetInputDrop(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _mouse.rightButton);
#endif
        }

        [PublicAPI]
        public void SetInputToggleMenu(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.escapeKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputRun(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.leftShiftKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputToggleCrouch(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.cKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputToggleProne(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.zKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputReleaseMouse(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.tabKey);
#endif
        }

        #region Movement Input Helpers
        
        [PublicAPI]
        public void SetInputMoveForward(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.wKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputMoveBackward(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.sKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputMoveRight(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.dKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputMoveLeft(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.aKey);
#endif
        }

        #endregion
        
        #region Look Input Helpers
        
        [PublicAPI]
        public void SetInputLook(Vector2 value)
        {
#if ENABLE_INPUT_SYSTEM
            _inputTestFixture.Move(_mouse.position, value);
#endif
        }
        
        [PublicAPI]
        public void SetInputLookDelta(Vector2 value)
        {
#if ENABLE_INPUT_SYSTEM
            SetInputLook(_mouse.position.ReadValue() + value);
#endif
        }

        #endregion
        
        #region Pickup Manipulation

        [PublicAPI]
        public void SetInputPickupManipulateRotateRight(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.lKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateRotateLeft(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.jKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateRotateUp(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.iKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateRotateDown(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.kKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateRotateClockwise(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.oKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateRotateCounterClockwise(bool value)
        {
#if ENABLE_INPUT_SYSTEM
            SetControlKey(value, _keyboard.uKey);
#endif
        }
        
        [PublicAPI]
        public void SetInputPickupManipulateMove(float value)
        {
#if ENABLE_INPUT_SYSTEM
            _inputTestFixture.Set(_mouse.scroll, new Vector2(0, value));
#endif
        }

        #endregion
        
    }
}