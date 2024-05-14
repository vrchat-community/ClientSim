
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for listening to Input Events and sending them to UdonBehaviours.
    /// </summary>
    /// <remarks>
    /// Listens to Events:
    /// - ClientSimMenuStateChangedEvent
    /// Listens to Input Events:
    /// - Jump
    /// - Grab
    /// - Use
    /// - Drop
    /// </remarks>
    public class ClientSimUdonInput : IDisposable
    {
        private const float MINIMUM_MOVE_INPUT_EPS = 1e-3f;
        private const float MINIMUM_LOOK_INPUT_EPS = 1e-5f;
        
        private readonly IClientSimInput _input;
        private readonly IClientSimEventDispatcher _eventDispatcher;
        /// <summary>
        /// A wrapper for sending events to all UdonBehaviours to create unit tests not dependent on UdonManager.
        /// </summary>
        private readonly IClientSimUdonInputEventSender _udonInputEventSender;

        private Vector2 _prevInput;
        private Vector2 _prevLookInput = Vector2.zero;
        private Vector2 _prevMoveAxes = Vector2.zero;

        private bool _isMenuOpen;
        private int _lastMenuUpdateFrame; // TODO update based on processing tick instead of time to allow for better testing.

        // All button based events are queued until process time. This is done to ensure that all udon input events
        // happen at the same time in the frame and not mixed between when unity's update for the new Input Manager
        // sends events. Without this, input events would happen before UdonBehaviour.Update, causing strange out of
        // order issues. 
        private readonly Queue<Action> _queuedEvents = new Queue<Action>();

        public ClientSimUdonInput(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimInput input,
            IClientSimUdonInputEventSender udonInputEventSender)
        {
            _input = input;
            _eventDispatcher = eventDispatcher;
            _udonInputEventSender = udonInputEventSender;

            _eventDispatcher.Subscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            
            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeJump(JumpInput);
            _input?.SubscribeUse(UseInput);
            _input?.SubscribeGrab(GrabInput);
            _input?.SubscribeDrop(DropInput);
            _input?.SubscribeInputChangedEvent(SendInputChangedEvent);
        }

        public void Dispose()
        {
            _eventDispatcher?.Unsubscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            
            _input?.UnsubscribeJump(JumpInput);
            _input?.UnsubscribeUse(UseInput);
            _input?.UnsubscribeGrab(GrabInput);
            _input?.UnsubscribeDrop(DropInput);
            _input?.UnsubscribeInputChangedEvent(SendInputChangedEvent);
        }

        #region ClientSim Events
        
        private void SetMenuOpen(ClientSimMenuStateChangedEvent stateChangedEvent)
        {
            _lastMenuUpdateFrame = Time.frameCount;
            _isMenuOpen = stateChangedEvent.isMenuOpen;
        }

        #endregion

        #region ClientSim Input

        private void JumpInput(bool value, HandType hand)
        {
            QueueButtonInputEvent(value, hand, UdonManager.UDON_INPUT_JUMP);
        }
        
        private void UseInput(bool value, HandType hand)
        {
            QueueButtonInputEvent(value, hand, UdonManager.UDON_INPUT_USE);
        }
        
        private void GrabInput(bool value, HandType hand)
        {
            QueueButtonInputEvent(value, hand, UdonManager.UDON_INPUT_GRAB);
        }
        
        private void DropInput(bool value, HandType hand)
        {
            QueueButtonInputEvent(value, hand, UdonManager.UDON_INPUT_DROP);
        }
        
        private void SendInputChangedEvent(VRCInputMethod inputMethod)
        {
            _queuedEvents.Enqueue(() =>
            {
                UdonManager.Instance.RunEvent(UdonManager.UDON_EVENT_ONINPUTMETHODCHANGED, ("inputMethod", inputMethod));
            });
        }

        #endregion

        private void SendUdonInputBoolEvent(bool value, HandType hand, string eventName)
        {
            var args = new UdonInputEventArgs(value, hand);
            _udonInputEventSender.RunInputAction(eventName, args);
        }
        
        private void SendUdonInputFloatEvent(float value, HandType hand, string eventName)
        {
            var args = new UdonInputEventArgs(value, hand);
            _udonInputEventSender.RunInputAction(eventName, args);
        }

        private void QueueButtonInputEvent(bool value, HandType hand, string eventName)
        {
            // Do not queue event if the menu is open.
            if (IsMenuOpen())
            {
                return;
            }

            _queuedEvents.Enqueue(() =>
            {
                SendUdonInputBoolEvent(value, hand, eventName);
            });
        }
        
        public void ProcessInputEvents()
        {
            // If the menu is open or the menu was updated on this frame, skip sending input events. 
            if (IsMenuOpen())
            {
                _queuedEvents.Clear();
                return;
            }

            // Ensure that all udon input events happen at the same time in the frame and not mixed between when unity's
            // update for the new Input Manager sends events. Without this, input events would happen before
            // UdonBehaviour.Update, causing strange out of order issues. 
            while (_queuedEvents.Count > 0)
            {
                Action inputEvent = _queuedEvents.Dequeue();
                inputEvent?.Invoke();
            }

            Vector2 lookAxes = _input.GetLookAxes();
            if (Mathf.Abs(lookAxes.x - _prevLookInput.x) > MINIMUM_LOOK_INPUT_EPS)
            {
                SendUdonInputFloatEvent(lookAxes.x, HandType.RIGHT, UdonManager.UDON_LOOK_HORIZONTAL);
            }
            if (Mathf.Abs(lookAxes.y - _prevLookInput.y) > MINIMUM_LOOK_INPUT_EPS)
            {
                SendUdonInputFloatEvent(lookAxes.y, HandType.RIGHT, UdonManager.UDON_LOOK_VERTICAL);
            }
            _prevLookInput = lookAxes;

            
            Vector2 moveAxes = _input.GetMovementAxes();
            if (Mathf.Abs(_prevMoveAxes.x - moveAxes.x) > MINIMUM_MOVE_INPUT_EPS)
            {
                SendUdonInputFloatEvent(moveAxes.x, HandType.LEFT, UdonManager.UDON_MOVE_HORIZONTAL);
            }
            if (Mathf.Abs(_prevMoveAxes.y - moveAxes.y) > MINIMUM_MOVE_INPUT_EPS)
            {
                SendUdonInputFloatEvent(moveAxes.y, HandType.LEFT, UdonManager.UDON_MOVE_VERTICAL);
            }
            _prevMoveAxes = moveAxes;
        }

        private bool IsMenuOpen()
        {
            return _isMenuOpen || _lastMenuUpdateFrame == Time.frameCount;
        }
    }
}