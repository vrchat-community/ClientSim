using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Components;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimInteractTests : ClientSimTestBase
    {
        // TODO
        // Displayed tooltip with multiple interacts
        
        // Current tests:
        // Test Interact with all collider types (Box/Mesh/Sphere/Capsule)
        // Interact range (proximity vs scale)
        // Interact with UIShape can't interact
        // Interact on different layers (UI only when menu open, MirrorReflection never)
        // Interact through colliders (isTrigger and not isTrigger) (trigger colliders do not increase range)
        // Multiple interacts on an object with different proximities (test with some out of range)
        // Interact with released mouse (UI closed)
        // Interact with released mouse (UI open)

        public enum InteractType
        {
            BOX,
            SPHERE,
            CAPSULE,
            MESH
        }
        private static InteractType[] _interactTypeValues =
        {
            InteractType.BOX, 
            InteractType.SPHERE, 
            InteractType.CAPSULE, 
            InteractType.MESH
        };

        private static float[] _testTrackingScaleValues = { 0.2f, 1, 2 };
        private static float[] _testProximityValues = { 0, 2, 5 };

        private GameObject _interactObj;
        private ClientSimTestInteract _testInteractHandler;
        private bool _wasInteractedWith = false;
        
        public override void TestSetUp()
        {
            base.TestSetUp();

            _testInteractHandler = null;
            _wasInteractedWith = false;
        }
        
        [TearDown]
        public void InteractTestTearDown()
        {
            if (_interactObj != null)
            {
                Object.Destroy(_interactObj);
            }
        }
        
        private GameObject SpawnInteract(InteractType type = InteractType.BOX)
        {
            string interactPrefabName = "ClientSimTestInteract";
            switch (type)
            {
                case InteractType.BOX:
                    interactPrefabName += "Box";
                    break;
                case InteractType.SPHERE:
                    interactPrefabName += "Sphere";
                    break;
                case InteractType.CAPSULE:
                    interactPrefabName += "Capsule";
                    break;
                case InteractType.MESH:
                    interactPrefabName += "Mesh";
                    break;
            }
            GameObject interactPrefab = ClientSimTestPrefabSpawner.GetTestPrefab(interactPrefabName);
            return Object.Instantiate(interactPrefab, Vector3.forward, Quaternion.identity);
        }

        private IEnumerator CreateInteract(InteractType type = InteractType.BOX)
        {
            _interactObj = SpawnInteract(type);

            // Give time to initialize
            yield return null;

            // Note that since Udon programs can't properly be in test packages, using proxy TestInteract for
            // something being interactable
            _testInteractHandler = _interactObj.GetComponent<ClientSimTestInteract>();
            Assert.IsNotNull(_testInteractHandler);
            
            _testInteractHandler.onInteract += () => _wasInteractedWith = true;
        }
        
        private IEnumerator StartClientSimAndCreateInteract(InteractType interactType = InteractType.BOX)
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                initializationDelay = 0f
            };

            yield return StartClientSim(settings);

            yield return CreateInteract(interactType);
            
            // Ensure menu is closed, allowing the player to walk and interact
            Helper.CloseMenu();

            yield return null;
        }

        // Given an object, wait until it is hovered, then press the use action, verify interact occured, and release use action.
        // This does not move the object or move the player to verify it should interact. 
        private IEnumerator InteractWithObject(GameObject obj, bool clearLastResults = true)
        {
            Assert.IsFalse(_wasInteractedWith);
            yield return Helper.WaitUntilObjectHovered(obj, HandType.RIGHT);
            yield return null; // Wait an extra frame in case object was already hovered before this method call.
            
            Assert.IsFalse(_wasInteractedWith);
            Assert.IsNull(Helper.GetLastInteractResults(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsTrue(Helper.GetLastInteractResults(HandType.RIGHT, clearLastResults).interactObject == obj);
            Assert.IsTrue(_wasInteractedWith);
            _wasInteractedWith = false;
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
        }
        
        // Try to interact, but expect no interact.
        private IEnumerator TryInteractWithObject(GameObject obj)
        {
            Assert.IsFalse(_wasInteractedWith);
            yield return Helper.WaitUntilObjectHovered(obj, HandType.RIGHT);
            yield return null; // Wait an extra frame in case object was already hovered before this method call.
            
            Assert.IsFalse(_wasInteractedWith);
            Assert.IsNull(Helper.GetLastInteractResults(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;

            Assert.IsNull(Helper.GetLastInteractResults(HandType.RIGHT, true));
            Assert.IsFalse(_wasInteractedWith);
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestInteractColliderTypes([ValueSource(nameof(_interactTypeValues))] InteractType interactType)
        {
            yield return StartClientSimAndCreateInteract(interactType);

            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform);

            Assert.IsFalse(_wasInteractedWith);
            Assert.IsNull(Helper.GetLastRaycastResults(HandType.RIGHT).interactable);
            yield return Helper.WaitUntilObjectHovered(_interactObj, HandType.RIGHT);
            
            Assert.IsFalse(_wasInteractedWith);
            Assert.IsNull(Helper.GetLastInteractResults(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;

            var interactEvents = Helper.GetLastInteractResults(HandType.RIGHT, true);
            Assert.IsTrue(interactEvents.interactObject == _interactObj);
            Assert.IsTrue(interactEvents.interacts.Contains(_testInteractHandler));
            Assert.IsTrue(_wasInteractedWith);
            _wasInteractedWith = false;
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestInteractProximity(
            [ValueSource(nameof(_testTrackingScaleValues))]float trackingScale, 
            [ValueSource(nameof(_testProximityValues))]float proximity)
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);
            
            // Position the box collider so that it is aligned on the object's z position.
            // This is to ensure proximity properly based on the object's distance to the player.
            var collider = _interactObj.GetComponent<BoxCollider>();
            collider.center = new Vector3(0, 0, 0.5f);
            
            Helper.SetTrackingScale(trackingScale);

            _testInteractHandler.proximity = proximity;

            // Current proximity formula based on ClientSimInteractHandler:
            // (player tracking scale * 1.25f + interact.proximity)
            float expectedInteractReach =
                trackingScale * ClientSimInteractManager.INTERACT_SCALE + _testInteractHandler.proximity;
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, expectedInteractReach - 0.1f);
            
            yield return InteractWithObject(_interactObj);
            
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, expectedInteractReach + 0.1f);

            yield return TryInteractWithObject(_interactObj);
        }

        [UnityTest]
        public IEnumerator TestInteractUIShapePreventsInteract()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _testInteractHandler.proximity = 2;
            
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, 1);
            
            yield return InteractWithObject(_interactObj);

            // Adding UI Shape prevents interact events
            _interactObj.AddComponent<VRCUiShape>();
            yield return null;

            yield return TryInteractWithObject(_interactObj);
        }

        [UnityTest]
        public IEnumerator TestInteractLayerMirrorReflection()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _testInteractHandler.proximity = 2;

            float objectDistance = 1;
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, objectDistance);
            
            // Verify can interact with the object.
            yield return InteractWithObject(_interactObj);

            // Change layer of object to mirror reflection, which can never be interacted with.
            _interactObj.layer = LayerMask.NameToLayer("MirrorReflection");

            yield return null;

            // Verify can't interact if object is never hovered.
            var raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            Assert.IsTrue(raycastResult.distance > objectDistance);
            
            // Open the menu, and disable the object to prevent the ui canvas from blocking our test objects.
            Helper.OpenAndDisableMenu();
            
            yield return null;
            
            // Verify can't interact if object is never hovered.
            raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            Assert.IsTrue(raycastResult.distance > objectDistance);
        }
        
        [UnityTest]
        public IEnumerator TestInteractLayerUI()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _testInteractHandler.proximity = 2;
            
            float objectDistance = 1;
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, objectDistance);
            
            yield return InteractWithObject(_interactObj);

            // Change layer of object to UI, which can only be interacted with while the menu is open.
            _interactObj.layer = LayerMask.NameToLayer("UI");
            
            yield return null;
            
            // Verify can't interact if object is never hovered.
            var raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            Assert.IsTrue(raycastResult.distance > objectDistance);

            // Open the menu, and disable the object to prevent the ui canvas from blocking our test objects.
            Helper.OpenAndDisableMenu();
            
            yield return null;

            // Verify that object can interact when menu is open.
            yield return InteractWithObject(_interactObj);
        }

        [UnityTest]
        public IEnumerator TestInteractWithColliderInFront()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _testInteractHandler.proximity = 5;

            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, _testInteractHandler.proximity);

            yield return InteractWithObject(_interactObj);

            GameObject cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Collider primCollider = cubePrimitive.GetComponent<BoxCollider>();
            Helper.MoveObjectInFrontOfPlayer(primCollider.transform, _testInteractHandler.proximity / 2f);

            
            // If a collider is trigger and does not have an interact, you can still interact through it.
            primCollider.isTrigger = true;
        
            yield return InteractWithObject(_interactObj);
            
            // If a collider is not trigger, you cannot interact through it.
            primCollider.isTrigger = false;

            yield return TryInteractWithObject(_interactObj);
            
            
            // Verify that you cannot interact through trigger colliders when out of range
            // This would work in VRChat, even though it does not make sense.
            // https://feedback.vrchat.com/sdk-bug-reports/p/incorrect-proximity-calculation-when-aiming-through-trigger-colliders
            
            primCollider.isTrigger = true;
            _testInteractHandler.proximity = 5;

            // Should not be able to reach this far.
            float distance = _testInteractHandler.proximity * 2;
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, distance);
            Helper.MoveObjectInFrontOfPlayer(primCollider.transform, distance - 1.5f);
            
            yield return TryInteractWithObject(_interactObj);

            Object.Destroy(cubePrimitive);
        }
        
        [UnityTest]
        public IEnumerator TestInteractMultipleComponents()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            // Allow the object to always be interacted with.
            _testInteractHandler.proximity = 50;

            ClientSimTestInteract testInteract1 = _interactObj.AddComponent<ClientSimTestInteract>();
            ClientSimTestInteract testInteract2 = _interactObj.AddComponent<ClientSimTestInteract>();
            ClientSimTestInteract testInteract3 = _interactObj.AddComponent<ClientSimTestInteract>();

            // Set interact distances out of order to ensure all are processed and not just the last found.
            testInteract1.proximity = 2;
            testInteract1.canInteract = true;
            
            testInteract2.proximity = 0;
            testInteract2.canInteract = true;

            testInteract3.proximity = 4;
            testInteract3.canInteract = true;
            
            
            // Move object far enough that nothing interacts.
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, 6);
            
            yield return InteractWithObject(_interactObj, false);
            
            ClientSimInteractEvent results = Helper.GetLastInteractResults(HandType.RIGHT, true);
            
            // Only interact of proximity 4 was interacted with.
            Assert.IsFalse(results.interacts.Contains(testInteract1), "Interact of proximity 2 was interacted from distance of 6!");
            Assert.IsFalse(results.interacts.Contains(testInteract2), "Interact of proximity 0 was interacted from distance of 6!");
            Assert.IsFalse(results.interacts.Contains(testInteract3), "Interact of proximity 4 was interacted from distance of 6!");
            
            
            // Move object closer so only one can interact. 
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, 4);
            
            yield return InteractWithObject(_interactObj, false);

            results = Helper.GetLastInteractResults(HandType.RIGHT, true);
            
            // Only interact of proximity 4 was interacted with.
            Assert.IsFalse(results.interacts.Contains(testInteract1), "Interact of proximity 2 was interacted from distance of 4!");
            Assert.IsFalse(results.interacts.Contains(testInteract2), "Interact of proximity 0 was interacted from distance of 4!");
            Assert.IsTrue(results.interacts.Contains(testInteract3), "Interact of proximity 4 was not interacted from distance of 4!");
            
            
            // Move object closer so only two can interact. 
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, 2);
            
            yield return InteractWithObject(_interactObj, false);

            results = Helper.GetLastInteractResults(HandType.RIGHT, true);
            
            // Only interact of proximity 4 and 2 was interacted with.
            Assert.IsTrue(results.interacts.Contains(testInteract1), "Interact of proximity 2 was not interacted from distance of 2!");
            Assert.IsFalse(results.interacts.Contains(testInteract2), "Interact of proximity 0 was interacted from distance of 2!");
            Assert.IsTrue(results.interacts.Contains(testInteract3), "Interact of proximity 4 was not interacted from distance of 2!");

            
            // Move object closer so all can interact. 
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, 0.5f);
            
            yield return InteractWithObject(_interactObj, false);

            results = Helper.GetLastInteractResults(HandType.RIGHT, true);
            
            // All should have been interacted.
            Assert.IsTrue(results.interacts.Contains(testInteract1), "Interact of proximity 2 was not interacted from distance of 0.5!");
            Assert.IsTrue(results.interacts.Contains(testInteract2), "Interact of proximity 0 was not interacted from distance of 0.5!");
            Assert.IsTrue(results.interacts.Contains(testInteract3), "Interact of proximity 4 was not interacted from distance of 0.5!");
        }

        [UnityTest]
        public IEnumerator TestInteractMouseReleasedMenuClosed()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _testInteractHandler.proximity = 5;
            
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, _testInteractHandler.proximity);
            
            // Move the object slightly to the right to not be directly in front of the player.
            _interactObj.transform.position += new Vector3(_interactObj.transform.transform.localScale.x + 0.1f, 0, 0);

            TestInput.SetInputReleaseMouse(true);
            TestInput.SetInputLook(ClientSimBaseInput.GetScreenCenter());

            yield return null;
            
            // Verify can't interact if object is never hovered.
            var raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            
            Helper.PutMouseOverObject(_interactObj.transform);
            
            // Verify can now interact with object with mouse over it.
            yield return InteractWithObject(_interactObj);

            // Move object to opposite x and redo test.
            Vector3 objPos = _interactObj.transform.position;
            objPos.x *= -1;
            _interactObj.transform.position = objPos;
            
            yield return null;
            
            // Verify can't interact if object is never hovered.
            raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            
            Helper.PutMouseOverObject(_interactObj.transform);
            
            // Verify can now interact with object with mouse over it.
            yield return InteractWithObject(_interactObj);
            
            TestInput.SetInputReleaseMouse(false);
        }
        
        [UnityTest]
        public IEnumerator TestInteractMouseReleasedMenuOpen()
        {
            yield return StartClientSimAndCreateInteract(InteractType.BOX);

            _interactObj.layer = LayerMask.NameToLayer("UI");
            _testInteractHandler.proximity = 5;
            
            Helper.MoveObjectInFrontOfPlayer(_interactObj.transform, _testInteractHandler.proximity);
            
            // Move the object slightly to the right to not be directly in front of the player.
            _interactObj.transform.position += new Vector3(_interactObj.transform.transform.localScale.x + 0.1f, 0, 0);

            Helper.OpenAndDisableMenu();

            yield return null;
            
            // Verify can't interact if object is never hovered.
            var raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            
            Helper.PutMouseOverObject(_interactObj.transform);
            
            // Verify can now interact with object with mouse over it.
            yield return InteractWithObject(_interactObj);

            // Move object to opposite x and redo test.
            Vector3 objPos = _interactObj.transform.position;
            objPos.x *= -1;
            _interactObj.transform.position = objPos;
            
            yield return null;
            
            // Verify can't interact if object is never hovered.
            raycastResult = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsFalse(raycastResult.hitObject == _interactObj);
            
            Helper.PutMouseOverObject(_interactObj.transform);
            
            // Verify can now interact with object with mouse over it.
            yield return InteractWithObject(_interactObj);
        }
    }
}