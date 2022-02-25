using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Components;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimUITests : ClientSimTestBase
    {
        // TODO:
        // UI on MirrorReflection layer Menu Open
        // Test Dropdowns
        // Test Toggle
        
        // Current Tests:
        // UI on default layer Menu Closed
        // UI on default layer Menu Open
        // UI on MirrorReflection layer Menu Closed
        // UI on UI layer Menu Closed
        // UI on UI layer Menu Open
        // UI without UIShape on default layer
        // UI on default layer Mouse released
        // UI with items offset
        // Trigger Interact blocking UI (near and far)
        // Non Trigger Interact blocking UI (near and far)

        private GameObject _canvasObj;
        private ClientSimTestUIHandler _testUIHandler;

        private bool _buttonClicked = false;
        private bool _sliderChanged = false;
        private float _sliderValue = 0;
        
        public override void TestSetUp()
        {
            base.TestSetUp();

            _testUIHandler = null;
            _buttonClicked = false;
            _sliderChanged = false;
            _sliderValue = 0;
        }
        
        [TearDown]
        public void InteractTestTearDown()
        {
            if (_canvasObj != null)
            {
                Object.Destroy(_canvasObj);
                _canvasObj = null;
            }
        }
        
        private GameObject SpawnUICanvas()
        {
            string canvasPrefabName = "ClientSimTestCanvas";
            GameObject interactPrefab = ClientSimTestPrefabSpawner.GetTestPrefab(canvasPrefabName);
            return Object.Instantiate(interactPrefab, Vector3.forward, Quaternion.identity);
        }

        private IEnumerator CreateUICanvas()
        {
            _canvasObj = SpawnUICanvas();

            // Give time to initialize
            yield return null;

            _testUIHandler = _canvasObj.GetComponent<ClientSimTestUIHandler>();
            Assert.IsNotNull(_testUIHandler);
            Assert.IsNotNull(_canvasObj.GetComponent<BoxCollider>());
            Assert.IsNotNull(_canvasObj.GetComponent<Canvas>().worldCamera); // This is set through UIShape
            
            _testUIHandler.button.onClick.AddListener(() => _buttonClicked = true);
            _testUIHandler.slider.onValueChanged.AddListener(
                value =>
                {
                    _sliderChanged = true;
                    _sliderValue = value;
                });
            _sliderValue = _testUIHandler.slider.value;
        }
        
        private IEnumerator StartClientSimAndCreateUICanvas()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                initializationDelay = 0f
            };

            yield return StartClientSim(settings);

            yield return CreateUICanvas();
            
            // Ensure menu is closed, allowing the player to walk and interact
            Helper.CloseMenu();

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestInteractWithUIButton()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);

            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            // Button clicks need press down and up while looking at the object.
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsTrue(_buttonClicked);
            _buttonClicked = false;
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestInteractWithUISlider()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.sliderHandle);
            
            Assert.IsFalse(_sliderChanged);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);
            
            Assert.IsFalse(_sliderChanged);
            TestInput.SetInputUseGrab(true);

            float value = _sliderValue;
            
            yield return null;
            
            Helper.LookAtObject(_testUIHandler.sliderLeftAnchor);

            yield return null;
            
            // Slider value should have changed.
            Assert.IsTrue(_sliderChanged);
            _sliderChanged = false;
            Assert.IsFalse(Mathf.Approximately(value, _sliderValue));
            value = _sliderValue;
            
            Helper.LookAtObject(_testUIHandler.sliderRightAnchor);
            
            yield return null;
            
            Assert.IsTrue(_sliderChanged);
            _sliderChanged = false;
            Assert.IsFalse(Mathf.Approximately(value, _sliderValue));
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
        }
        
        // When the mouse is released, you can interact with UI elements that are no longer in the center of the screen.
        [UnityTest]
        public IEnumerator TestInteractWithUISliderMouseReleased()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            // Make sure to look at an unrelated object.
            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_sliderChanged);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);

            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);
            
            yield return null;

            TestInput.SetInputReleaseMouse(true);
            
            yield return null;
            
            Helper.PutMouseOverObject(_testUIHandler.sliderHandle);
            
            yield return null;
            
            Assert.IsFalse(_sliderChanged);
            TestInput.SetInputUseGrab(true);

            float value = _sliderValue;
            
            yield return null;
            
            Helper.PutMouseOverObject(_testUIHandler.sliderLeftAnchor);

            yield return null;
            
            // Slider value should have changed.
            Assert.IsTrue(_sliderChanged);
            _sliderChanged = false;
            Assert.IsFalse(Mathf.Approximately(value, _sliderValue));
            value = _sliderValue;
            
            Helper.PutMouseOverObject(_testUIHandler.sliderRightAnchor);
            
            yield return null;
            
            Assert.IsTrue(_sliderChanged);
            _sliderChanged = false;
            Assert.IsFalse(Mathf.Approximately(value, _sliderValue));
            
            TestInput.SetInputUseGrab(false);
            TestInput.SetInputReleaseMouse(false);
            
            yield return null;
        }
        
        // UI on the MirrorReflection layer cannot ever be interacted with.
        [UnityTest]
        public IEnumerator TestMirrorReflectionLayer()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);

            // Following Test for UI button other than changing the layer.
            _canvasObj.layer = LayerMask.NameToLayer("MirrorReflection");
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
        }
        
        // UI on the UI and UIMenu layer cannot be interacted with while the menu is closed.
        [UnityTest]
        public IEnumerator TestUILayerMenuClosed()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);

            // Following Test for UI button other than changing the layer.
            _canvasObj.layer = LayerMask.NameToLayer("UI");
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
        }
        
        // UI on the UI and UIMenu layer can be interacted with while the menu is open.
        [UnityTest]
        public IEnumerator TestUILayerMenuOpen()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);
            
            _canvasObj.layer = LayerMask.NameToLayer("UI");

            // Open the menu, and disable the object to prevent the ui canvas from blocking our test objects.
            Helper.OpenAndDisableMenu();
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            // Button clicks need press down and up while looking at the object.
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsTrue(_buttonClicked);
            _buttonClicked = false;
            
            yield return null;
        }
        
        // UI on the Default layer cannot be interacted with while the menu is Open.
        [UnityTest]
        public IEnumerator TestDefaultLayerMenuOpen()
        {
            yield return StartClientSimAndCreateUICanvas();

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);

            // Open the menu, and disable the object to prevent the ui canvas from blocking our test objects.
            Helper.OpenAndDisableMenu();
            Helper.PutMouseOverObject(_testUIHandler.button.transform);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
        }
        
        // UI elements without a UI Shape cannot be interacted with.
        [UnityTest]
        public IEnumerator TestDefaultLayerNoUIShape()
        {
            yield return StartClientSimAndCreateUICanvas();
            
            // Destroy the UI shape to prevent it from being interacted with.
            Object.Destroy(_canvasObj.GetComponent<VRCUiShape>());

            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNull(raycastResults.uiShape);

            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
        }
        
        // UI elements must be within the canvas collider to be interacted with.
        [UnityTest]
        public IEnumerator TestDefaultLayerUIElementsOutsideOfCollider()
        {
            yield return StartClientSimAndCreateUICanvas();

            Transform canvasTransform = _canvasObj.transform;
            Helper.MoveObjectInFrontOfPlayer(canvasTransform);
            
            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);

            Vector3 buttonPosition = _testUIHandler.button.transform.position;
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);

            // Offset the object and put the children back. This way the collider will not cover the ui elements.
            Vector3 offset = new Vector3(0, 1, 0);
            canvasTransform.position += offset;
            for (int child = 0; child < canvasTransform.childCount; ++child)
            {
                canvasTransform.GetChild(child).position -= offset;
            }
            
            // Ensure that the button is in the same position.
            Assert.IsTrue(Vector3.Distance(buttonPosition, _testUIHandler.button.transform.position) < 1e-3f);
            
            yield return null;
            
            // There should be nothing to raycast against since the collider was moved.
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
        }
        
        
        // Interact objects in front of UI will block interacting with UI. If the interact is trigger, then outside of
        // the proximity range will allow UI interactions again.
        [UnityTest]
        public IEnumerator TestInteractWithUIButtonWithBlockingInteractTrigger()
        {
            yield return StartClientSimAndCreateUICanvas();
            
            // Spawn an interatable object and place it at the same location as the button.
            string interactPrefabName = "ClientSimTestInteractBox";
            GameObject interactPrefab = ClientSimTestPrefabSpawner.GetTestPrefab(interactPrefabName);
            GameObject interactInstance = 
                Object.Instantiate(interactPrefab, _testUIHandler.button.transform);
            interactInstance.transform.localScale = 
                Vector3.one * interactPrefab.transform.localScale.x / _canvasObj.transform.localScale.x;
            interactInstance.transform.localPosition = Vector3.zero;
            ClientSimTestInteract testInteract = interactInstance.GetComponent<ClientSimTestInteract>();
            testInteract.proximity = 1;
            bool interacted = false;
            testInteract.onInteract += () => interacted = true;
            // Set the collider to isTrigger, meaning interacts can pass through it when not within range.
            Collider collider = interactInstance.GetComponent<Collider>();
            collider.isTrigger = true;

            // Move the object far enough away that it will not trigger the interact.
            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform, testInteract.proximity + 5);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(_canvasObj, HandType.RIGHT);
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;

            // Button clicks need press down and up while looking at the object.
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsTrue(_buttonClicked);
            _buttonClicked = false;
            
            yield return null;
            
            
            // Move the canvas close enough so that the Interact can be used, which blocks the UI interaction
            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform, testInteract.proximity);
            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            yield return Helper.WaitUntilObjectHovered(interactInstance, HandType.RIGHT);
            
            raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.interactable);
            Assert.IsNull(raycastResults.uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(interacted);
            interacted = false;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
            
            Object.Destroy(interactInstance);
        }
        
        // Interact objects in front of UI will block interacting with UI. If the interact is non trigger, then the UI
        // should never be usable through the interact object, even outside of proximity range.
        [UnityTest]
        public IEnumerator TestInteractWithUIButtonWithBlockingInteractNonTrigger()
        {
            yield return StartClientSimAndCreateUICanvas();
            
            // Spawn an interatable object and place it at the same location as the button.
            string interactPrefabName = "ClientSimTestInteractBox";
            GameObject interactPrefab = ClientSimTestPrefabSpawner.GetTestPrefab(interactPrefabName);
            GameObject interactInstance = 
                Object.Instantiate(interactPrefab, _testUIHandler.button.transform);
            interactInstance.transform.localScale = 
                Vector3.one * interactPrefab.transform.localScale.x / _canvasObj.transform.localScale.x;
            interactInstance.transform.localPosition = Vector3.zero;
            ClientSimTestInteract testInteract = interactInstance.GetComponent<ClientSimTestInteract>();
            testInteract.proximity = 1;
            bool interacted = false;
            testInteract.onInteract += () => interacted = true;
            // Set the collider to not isTrigger, meaning interacts cannot pass through.
            Collider collider = interactInstance.GetComponent<Collider>();
            collider.isTrigger = false;

            // Move the object far enough away that it will not trigger the interact.
            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform, testInteract.proximity + 5);

            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).uiShape);
            yield return Helper.WaitUntilObjectHovered(interactInstance, HandType.RIGHT);
            
            
            var raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNull(raycastResults.interactable);
            Assert.IsNull(raycastResults.uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsFalse(interacted);
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
            
            
            // Move the canvas close enough so that the Interact can be used, which blocks the UI interaction
            Helper.MoveObjectInFrontOfPlayer(_canvasObj.transform, testInteract.proximity);
            Helper.LookAtObject(_testUIHandler.button.transform);
            
            Assert.IsFalse(_buttonClicked);
            yield return Helper.WaitUntilObjectHovered(interactInstance, HandType.RIGHT);
            
            raycastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsNotNull(raycastResults);
            Assert.IsNotNull(raycastResults.interactable);
            Assert.IsNull(raycastResults.uiShape);
            
            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(interacted);
            interacted = false;

            Assert.IsFalse(_buttonClicked);
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsFalse(_buttonClicked);
            
            yield return null;
            
            Object.Destroy(interactInstance);
        }
    }
}