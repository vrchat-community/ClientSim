#if ENABLE_INPUT_SYSTEM

using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests.ComponentTests
{
    public class ClientSimActionInputTests : ClientSimTestBase
    {
        private const string INPUT_PREFAB_NAME = "ClientSimInputManager";

        private InputTestFixture _inputTestFixture;
        private Keyboard _keyboard;
        private Mouse _mouse;

        private GameObject _inputInstance;
        private ClientSimInputManager _inputManager;
        private ClientSimSettings _settings;
        private ClientSimTestInputListener _inputListener;
        private IClientSimInput _input;
        
        [SetUp]
        public void InputTestSetUp()
        {
            _inputTestFixture = new InputTestFixture();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();

            _inputInstance = SpawnInputPrefab();
            _inputManager = _inputInstance.GetComponent<ClientSimInputManager>();
            Assert.IsNotNull(_inputManager);
            
            _settings = new ClientSimSettings();
            _inputManager.Initialize(_settings);

            _inputListener = new ClientSimTestInputListener();
            _input = _inputManager.GetInput();
            _inputListener.Subscribe(_input);
        }

        [TearDown]
        public void InputTestTearDown()
        {
            _inputListener.Unsubscribe(_input);
            
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.RemoveDevice(_mouse);
            Object.Destroy(_inputInstance);
        }
        
        private GameObject SpawnInputPrefab()
        {
            GameObject inputPrefab = ClientSimTestPrefabSpawner.GetRuntimePrefab(INPUT_PREFAB_NAME);
            Assert.IsNotNull(inputPrefab);

            GameObject inputInstance = Object.Instantiate(inputPrefab);
            Assert.IsNotNull(inputInstance);
            
            return inputInstance;
        }
        
        // Verify that for the given input mouse or keyboard action the corresponding ClientSim input event is sent.
        [Test]
        public void TestKeyboardMouseInputEvents()
        {
            #region Handed Keyboard Actions

            // Keyboard Jump action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.jump);
            
            _inputTestFixture.Press(_keyboard.spaceKey);
            
            Assert.IsTrue(_inputListener.jump);
            Assert.IsTrue(_inputListener.jumpHandType == HandType.LEFT);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.spaceKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.jump);
            
            
            // Keyboard Toggle Menu action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleMenu);
            
            _inputTestFixture.Press(_keyboard.escapeKey);
            
            Assert.IsTrue(_inputListener.toggleMenu);
            Assert.IsTrue(_inputListener.toggleMenuHandType == HandType.LEFT);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.escapeKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleMenu);

            #endregion

            #region Non-handed keyboard actions

            // Keyboard Run action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.run);
            
            _inputTestFixture.Press(_keyboard.leftShiftKey);
            
            Assert.IsTrue(_inputListener.run);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.leftShiftKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.run);
            
            
            // Keyboard Crouch action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleCrouch);
            
            _inputTestFixture.Press(_keyboard.cKey);
            
            Assert.IsTrue(_inputListener.toggleCrouch);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.cKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleCrouch);
            
            
            // Keyboard Prone action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleProne);
            
            _inputTestFixture.Press(_keyboard.zKey);

            Assert.IsTrue(_inputListener.toggleProne);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.zKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.toggleProne);
            
            
            // Keyboard Release Mouse action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.releaseMouse);
            
            _inputTestFixture.Press(_keyboard.tabKey);
            
            Assert.IsTrue(_inputListener.releaseMouse);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_keyboard.tabKey);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.releaseMouse);

            #endregion

            #region Handed Mouse Actions

            // Mouse Use and Grab actions
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.use);
            Assert.IsFalse(_inputListener.grab);
            
            _inputTestFixture.Press(_mouse.leftButton);
            
            Assert.IsTrue(_inputListener.use);
            Assert.IsTrue(_inputListener.useHandType == HandType.RIGHT);
            Assert.IsTrue(_inputListener.grab);
            Assert.IsTrue(_inputListener.grabHandType == HandType.RIGHT);
            Assert.IsTrue(_inputListener.CountTrue() == 2);
            
            _inputTestFixture.Release(_mouse.leftButton);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.use);
            Assert.IsFalse(_inputListener.grab);
            
            
            // Mouse Drop action
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.drop);
            
            _inputTestFixture.Press(_mouse.rightButton);
            
            Assert.IsTrue(_inputListener.drop);
            Assert.IsTrue(_inputListener.dropHandType == HandType.RIGHT);
            Assert.IsTrue(_inputListener.CountTrue() == 1);
            
            _inputTestFixture.Release(_mouse.rightButton);
            
            Assert.IsTrue(_inputListener.AllOff());
            Assert.IsFalse(_inputListener.drop);

            #endregion
        }
        
        // Verify that for the given input mouse or keyboard action the corresponding ClientSim input axis is non zero.
        [Test]
        public void TestKeyboardMouseAxisInput()
        {
            #region Test movement options
            
            InputSystem.Update();

            // W key to move forward
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Press(_keyboard.wKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 1));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 1));
            
            _inputTestFixture.Release(_keyboard.wKey);
            
            // S Key to move backward
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Press(_keyboard.sKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), -1));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 1));
            
            _inputTestFixture.Release(_keyboard.sKey);
            
            // A Key to move left
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Press(_keyboard.aKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), -1));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 1));
            
            _inputTestFixture.Release(_keyboard.aKey);
            
            // D Key to move right
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Press(_keyboard.dKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 1));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 1));
            
            _inputTestFixture.Release(_keyboard.dKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetMovementAxes().sqrMagnitude, 0));

            #endregion
            
            #region Pickup Manipulation Rotation
            
            InputSystem.Update();
            
            // L key to rotate pickups right
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.lKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), -1));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Release(_keyboard.lKey);
            
            // J key to rotate pickups right
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.jKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 1));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Release(_keyboard.jKey);
            
            // I key to rotate pickups up
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.iKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), -1));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Release(_keyboard.iKey);
            
            // K key to rotate pickups down
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.kKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 1));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Release(_keyboard.kKey);
            
            // O key to rotate pickups clockwise
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.oKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 1));
            
            _inputTestFixture.Release(_keyboard.oKey);
            
            // U key to rotate pickups counterclockwise
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));
            
            _inputTestFixture.Press(_keyboard.uKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), -1));
            
            _inputTestFixture.Release(_keyboard.uKey);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateLeftRight(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateUpDown(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupRotateCwCcw(), 0));

            #endregion
            
            #region Pickup Manipulation Distance
            
            _inputTestFixture.Move(_mouse.scroll, Vector2.zero);
            InputSystem.Update();
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupManipulateDistance(), 0));
            
            _inputTestFixture.Move(_mouse.scroll, Vector2.up);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupManipulateDistance(), 1));
            
            _inputTestFixture.Move(_mouse.scroll, Vector2.down);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupManipulateDistance(), -1));
            
            _inputTestFixture.Move(_mouse.scroll, Vector2.zero);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetPickupManipulateDistance(), 0));
            
            #endregion

            #region Mouse movement

            _settings.invertMouseLook = false;
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);
            InputSystem.Update();
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetLookAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Move(_mouse.delta, Vector2.up);
            
            // Invert off means moving up is positive.
            Assert.IsTrue(_input.GetLookVertical() > 0);
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);

            _inputTestFixture.Move(_mouse.delta, Vector2.down);
            
            // Invert off means moving down is negative.
            Assert.IsTrue(_input.GetLookVertical() < 0);
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);
            InputSystem.Update();
            
            _inputTestFixture.Move(_mouse.delta, Vector2.left);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(_input.GetLookHorizontal() < 0);
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.right);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(_input.GetLookHorizontal() > 0);
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);
            InputSystem.Update();
            
            
            _settings.invertMouseLook = true;
            
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(Mathf.Approximately(_input.GetLookAxes().sqrMagnitude, 0));
            
            _inputTestFixture.Move(_mouse.delta, Vector2.up);
            
            // Invert on means moving up is negative.
            Assert.IsTrue(_input.GetLookVertical() < 0);
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);

            _inputTestFixture.Move(_mouse.delta, Vector2.down);
            
            // Invert on means moving down is positive.
            Assert.IsTrue(_input.GetLookVertical() > 0);
            Assert.IsTrue(Mathf.Approximately(_input.GetLookHorizontal(), 0));
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);
            InputSystem.Update();
            
            _inputTestFixture.Move(_mouse.delta, Vector2.left);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(_input.GetLookHorizontal() < 0);
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.right);
            
            Assert.IsTrue(Mathf.Approximately(_input.GetLookVertical(), 0));
            Assert.IsTrue(_input.GetLookHorizontal() > 0);
            Assert.IsTrue(_input.GetLookAxes().sqrMagnitude > 0);
            
            _inputTestFixture.Move(_mouse.delta, Vector2.zero);

            #endregion
        }
    }
}
#endif