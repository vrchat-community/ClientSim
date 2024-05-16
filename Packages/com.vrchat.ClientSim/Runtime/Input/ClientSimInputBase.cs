using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Base method for handling subscribing to and sending input events.
    /// Class is abstract to allow for Test versions to mock sending input events.
    /// </summary>
    public abstract class ClientSimInputBase : IClientSimInput, IDisposable
    {
        private Action<bool, HandType> _jumpEvent;
        private Action<bool, HandType> _useEvent;
        private Action<bool, HandType> _grabEvent;
        private Action<bool, HandType> _dropEvent;
        private Action<bool, HandType> _toggleMenuEvent;
        
        private Action<bool> _runEvent;
        private Action<bool> _toggleCrouchEvent;
        private Action<bool> _toggleProneEvent;
        private Action<bool> _releaseMouseEvent;
        
        private Action<VRCInputMethod> _inputMethodChangedEvent;

        public virtual void Dispose()
        {
            // Clear all subscriptions
            _jumpEvent = null;
            _useEvent = null;
            _grabEvent = null;
            _dropEvent = null;
            _toggleMenuEvent = null;
            _inputMethodChangedEvent = null;
            
            _runEvent = null;
            _toggleCrouchEvent = null;
            _toggleProneEvent = null;
            _releaseMouseEvent = null;
        }

        #region Event Subscriptions

        public void SubscribeJump(Action<bool, HandType> handler)
        {
            _jumpEvent += handler;
        }
        
        public void UnsubscribeJump(Action<bool, HandType> handler)
        {
            _jumpEvent -= handler;
        }


        public void SubscribeUse(Action<bool, HandType> handler)
        {
            _useEvent += handler;
        }
        
        public void UnsubscribeUse(Action<bool, HandType> handler)
        {
            _useEvent -= handler;
        }

        
        public void SubscribeGrab(Action<bool, HandType> handler)
        {
            _grabEvent += handler;
        }

        public void UnsubscribeGrab(Action<bool, HandType> handler)
        {
            _grabEvent -= handler;
        }

        
        public void SubscribeDrop(Action<bool, HandType> handler)
        {
            _dropEvent += handler;
        }

        public void UnsubscribeDrop(Action<bool, HandType> handler)
        {
            _dropEvent -= handler;
        }

        
        public void SubscribeToggleMenu(Action<bool, HandType> handler)
        {
            _toggleMenuEvent += handler;
        }

        public void UnsubscribeToggleMenu(Action<bool, HandType> handler)
        {
            _toggleMenuEvent -= handler;
        }
        

        public void SubscribeRun(Action<bool> handler)
        {
            _runEvent += handler;
        }

        public void UnsubscribeRun(Action<bool> handler)
        {
            _runEvent -= handler;
        }

        
        public void SubscribeToggleCrouch(Action<bool> handler)
        {
            _toggleCrouchEvent += handler;
        }

        public void UnsubscribeToggleCrouch(Action<bool> handler)
        {
            _toggleCrouchEvent -= handler;
        }

        
        public void SubscribeToggleProne(Action<bool> handler)
        {
            _toggleProneEvent += handler;
        }

        public void UnsubscribeToggleProne(Action<bool> handler)
        {
            _toggleProneEvent -= handler;
        }

        
        public void SubscribeReleaseMouse(Action<bool> handler)
        {
            _releaseMouseEvent += handler;
        }

        public void UnsubscribeReleaseMouse(Action<bool> handler)
        {
            _releaseMouseEvent -= handler;
        }

        public void SubscribeInputChangedEvent(Action<VRCInputMethod> handler)
        {
            _inputMethodChangedEvent += handler;
        }
        
        public void UnsubscribeInputChangedEvent(Action<VRCInputMethod> handler)
        {
            _inputMethodChangedEvent -= handler;
        }
        
        #endregion

        #region GetAxisData
        
        public Vector2 GetMovementAxes()
        {
            float x = GetMovementHorizontal();
            float y = GetMovementVertical();

            return new Vector2(x, y);
        }

        public Vector2 GetLookAxes()
        {
            float x = GetLookHorizontal();
            float y = GetLookVertical();

            return new Vector2(x, y);
        }

        public abstract float GetMovementHorizontal();

        public abstract float GetMovementVertical();

        public abstract float GetLookHorizontal();

        public abstract float GetLookVertical();

        public abstract float GetPickupRotateUpDown();

        public abstract float GetPickupRotateLeftRight();

        public abstract float GetPickupRotateCwCcw();

        public abstract float GetPickupManipulateDistance();

        #endregion
        
        
        public void SendJumpEvent(bool value, HandType handType)
        {
            _jumpEvent?.Invoke(value, handType);
        }
        
        public void SendUseEvent(bool value, HandType handType)
        {
            _useEvent?.Invoke(value, handType);
        }
        
        public void SendGrabEvent(bool value, HandType handType)
        {
            _grabEvent?.Invoke(value, handType);
        }
        
        public void SendDropEvent(bool value, HandType handType)
        {
            _dropEvent?.Invoke(value, handType);
        }
        
        public void SendToggleMenuEvent(bool value, HandType handType)
        {
            _toggleMenuEvent?.Invoke(value, handType);
        }
        
        public void SendRunEvent(bool value)
        {
            _runEvent?.Invoke(value);
        }
        
        public void SendToggleCrouchEvent(bool value)
        {
            _toggleCrouchEvent?.Invoke(value);
        }
        
        public void SendToggleProneEvent(bool value)
        {
            _toggleProneEvent?.Invoke(value);
        }
        
        public void SendReleaseMouseEvent(bool value)
        {
            _releaseMouseEvent?.Invoke(value);
        }
        
        public void SendInputMethodChangedEvent(VRCInputMethod value)
        {
            _inputMethodChangedEvent?.Invoke(value);
        }
    }
}