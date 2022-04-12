using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.ClientSim.Tests;
using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Editor.Tests
{
    public class ClientSimUdonInputTests
    {
        private ClientSimEventDispatcher _eventDispatcher;
        private ClientSimTestUdonInputEventListener _udonEventListener;
        private ClientSimTestInputMock _testInputMock;
        private ClientSimUdonInput _udonInput;

        [SetUp]
        public void SetUp()
        {
            _eventDispatcher = new ClientSimEventDispatcher();
            _udonEventListener = new ClientSimTestUdonInputEventListener();
            _testInputMock = new ClientSimTestInputMock();
            _udonInput = new ClientSimUdonInput(_eventDispatcher, _testInputMock, _udonEventListener);
        }

        [TearDown]
        public void TearDown()
        {
            _udonInput.Dispose();
            _eventDispatcher.Dispose();
        }
        
        [Test]
        public void TestUdonButtonInput()
        {
            // Jump events for true and false/left and right.
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, true, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, false, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(true, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, true, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(false, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, false, HandType.RIGHT);
            
            
            // Use events for true and false/left and right.
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendUseEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_USE, true, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendUseEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_USE, false, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendUseEvent(true, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_USE, true, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendUseEvent(false, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_USE, false, HandType.RIGHT);
            
            
            // Grab events for true and false/left and right.
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendGrabEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_GRAB, true, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendGrabEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_GRAB, false, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendGrabEvent(true, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_GRAB, true, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendGrabEvent(false, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_GRAB, false, HandType.RIGHT);
            
            
            // Drop events for true and false/left and right.
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendDropEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_DROP, true, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendDropEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_DROP, false, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendDropEvent(true, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_DROP, true, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendDropEvent(false, HandType.RIGHT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_DROP, false, HandType.RIGHT);
        }

        [Test]
        public void TestUdonAxisInput()
        {
            // Test movement Horizontal
            _testInputMock.SetMovementHorizontal(0);
            _udonInput.ProcessInputEvents();
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_HORIZONTAL, 1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_HORIZONTAL, -1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            
            // Test movement Vertical
            _testInputMock.SetMovementVertical(0);
            _udonInput.ProcessInputEvents();
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementVertical(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_VERTICAL, 1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementVertical(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_VERTICAL, -1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);

            
            // Test Look Horizontal
            _testInputMock.SetLookHorizontal(0);
            _udonInput.ProcessInputEvents();
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetLookHorizontal(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEventSign(UdonManager.UDON_LOOK_HORIZONTAL, 1, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetLookHorizontal(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEventSign(UdonManager.UDON_LOOK_HORIZONTAL, -1, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            
            
            // Test Look Vertical
            _testInputMock.SetLookVertical(0);
            _udonInput.ProcessInputEvents();
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetLookVertical(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEventSign(UdonManager.UDON_LOOK_VERTICAL, 1, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetLookVertical(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEventSign(UdonManager.UDON_LOOK_VERTICAL, -1, HandType.RIGHT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
        }

        // Test that when the menu is opened, no events are sent.
        [UnityTest]
        public IEnumerator TestOpeningMenuPreventsEvents()
        {
            // Jump events for true and false
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, true, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_INPUT_JUMP, false, HandType.LEFT);
            
            // Test Movement Horizontal
            _testInputMock.SetMovementHorizontal(0);
            _udonInput.ProcessInputEvents();
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_HORIZONTAL, 1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 1);
            AssertNextEvent(UdonManager.UDON_MOVE_HORIZONTAL, -1, HandType.LEFT);
            
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            
            
            // Must test valid events before opening the menu as Time.frameCount does not update in Editor tests.
            yield return null;
            _eventDispatcher.SendEvent(new ClientSimMenuStateChangedEvent {isMenuOpen = true});
            
            // Jump events for true and false, but all events should fail since menu is open.
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(true, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SendJumpEvent(false, HandType.LEFT);
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            
            // Test Movement Horizontal does not send event due to menu opened on this frame.
            _testInputMock.SetMovementHorizontal(0);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            _testInputMock.SetMovementHorizontal(-1);
            _udonInput.ProcessInputEvents();
            Assert.IsTrue(_udonEventListener.EventQueueCount() == 0);
            
            yield return null;
        }

        private void AssertNextEvent(
            string eventName, 
            bool value, 
            HandType handType)
        {
            (string, UdonInputEventArgs) udonEvent = _udonEventListener.DequeueEvent();
            
            Assert.IsTrue(udonEvent.Item1 == eventName);
            Assert.IsTrue(udonEvent.Item2.boolValue == value);
            Assert.IsTrue(udonEvent.Item2.handType == handType);
        }

        private void AssertNextEvent(
            string eventName, 
            float value,
            HandType handType)
        {
            (string, UdonInputEventArgs) udonEvent = _udonEventListener.DequeueEvent();
            
            Assert.IsTrue(udonEvent.Item1 == eventName);
            Assert.IsTrue(Mathf.Approximately(udonEvent.Item2.floatValue, value));
            Assert.IsTrue(udonEvent.Item2.handType == handType);
        }
        
        // Compare the sign of the value instead of comparing the value itself.
        private void AssertNextEventSign(
            string eventName, 
            float value,
            HandType handType)
        {
            (string, UdonInputEventArgs) udonEvent = _udonEventListener.DequeueEvent();
            
            Assert.IsTrue(udonEvent.Item1 == eventName);
            Assert.IsTrue(Mathf.Approximately(Mathf.Sign(udonEvent.Item2.floatValue), Mathf.Sign(value)));
            Assert.IsTrue(udonEvent.Item2.handType == handType);
        }
    }
}