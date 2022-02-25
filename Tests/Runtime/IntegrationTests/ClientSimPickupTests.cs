using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimPickupTests : ClientSimTestBase
    {
        // TODO
        // Pickup events while menu is open (OnUseDown, OnUseUp, Drop) (Events still fire while menu is open)

        private GameObject _pickupObj;
        private VRC_Pickup _pickup;

        private ClientSimTestPickupHandler _testPickupHandler;
        
        [TearDown]
        public void PickupTestTearDown()
        {
            if (_pickupObj != null)
            {
                Object.Destroy(_pickupObj);
            }
        }
        
        private GameObject SpawnPickup(bool synced = false)
        {
            string pickupPrefabName = 
                synced
                ? "ClientSimTestPickupSynced"
                : "ClientSimTestPickup";
            
            GameObject pickupPrefab = ClientSimTestPrefabSpawner.GetTestPrefab(pickupPrefabName);
            return Object.Instantiate(pickupPrefab, Vector3.forward, Quaternion.identity);
        }

        private IEnumerator CreatePickup(bool synced = false)
        {
            _pickupObj = SpawnPickup(synced);

            // Give time to initialize
            yield return null;

            _pickup = _pickupObj.GetComponent<VRC_Pickup>();
            Assert.IsNotNull(_pickup);
            Assert.IsNotNull(_pickupObj.GetComponent<ClientSimPickupHelper>());

            _testPickupHandler = _pickupObj.GetComponent<ClientSimTestPickupHandler>();
            Assert.IsNotNull(_testPickupHandler);
        }
        
        
        private IEnumerator StartClientSimAndCreatePickup(bool syncedPickup = false)
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                initializationDelay = 0f
            };

            yield return StartClientSim(settings);

            yield return CreatePickup(syncedPickup);
            
            // Ensure menu is closed, allowing the player to walk and interact
            Helper.CloseMenu();

            yield return null;
        }

        private IEnumerator GrabPickup()
        {
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);

            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);

            if (_pickup.AutoHold == VRC_Pickup.AutoHoldMode.Yes)
            {
                TestInput.SetInputUseGrab(false);
                
                yield return null;
            }
        }

        private IEnumerator DropPickup()
        {
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));

            if (_pickup.AutoHold == VRC_Pickup.AutoHoldMode.Yes)
            {
                TestInput.SetInputDrop(true);
                
                yield return null;
                
                TestInput.SetInputDrop(false);
            }
            else
            {
                TestInput.SetInputUseGrab(false);
            }
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPickupGrabDrop()
        {
            yield return StartClientSimAndCreatePickup();

            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left));
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right));
            Assert.IsTrue(_pickup.currentHand == VRC_Pickup.PickupHand.None);
            Assert.IsNull(_pickup.currentPlayer);
            
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);

            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left));
            Assert.IsTrue(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right) == _pickup);
            Assert.IsTrue(_pickup.currentHand == VRC_Pickup.PickupHand.Right);
            Assert.IsTrue(_pickup.currentPlayer == localPlayer);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left));
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right));
            Assert.IsTrue(_pickup.currentHand == VRC_Pickup.PickupHand.None);
            Assert.IsNull(_pickup.currentPlayer);
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestPickupNotPickupable()
        {
            yield return StartClientSimAndCreatePickup();

            // Test that disabling pickup.pickupable prevents the object from being picked up.
            _pickup.pickupable = false;
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);
            
            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);

            ClientSimRaycastResults lastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsTrue(lastResults.hitObject == _pickupObj);
            Assert.IsNull(lastResults.uiShape);
            Assert.IsNull(lastResults.interactable);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));

            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
        }
        
        [UnityTest]
        public IEnumerator TestPickupDisabledPickups()
        {
            yield return StartClientSimAndCreatePickup();
            
            // Test that the player can't pickup an object when pickups are disabled for the player.
            Networking.LocalPlayer.EnablePickups(false);
            _pickup.pickupable = true;
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);
            
            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            ClientSimRaycastResults lastResults = Helper.GetLastRaycastResults(HandType.RIGHT);
            Assert.IsTrue(lastResults.hitObject == _pickupObj);
            Assert.IsNull(lastResults.uiShape);
            Assert.IsNull(lastResults.interactable);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));

            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
        }
        
        [UnityTest]
        public IEnumerator TestPickupGrabDropWithAutoHold()
        {
            yield return StartClientSimAndCreatePickup();

            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);

            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);

            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(true);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(false);
            
            yield return null;
            
            // Drop event only happens after drop input has been released.
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
        }

        [UnityTest]
        public IEnumerator TestPickupOwnershipTransfer()
        {
            yield return StartClientSimAndCreatePickup(true);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();

            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            
            Networking.SetOwner(remotePlayer, _pickupObj);
            
            // Remote player is the current owner.
            Assert.IsTrue(remotePlayer.IsOwner(_pickupObj));
            Assert.IsFalse(localPlayer.IsOwner(_pickupObj));
            
            yield return GrabPickup();
            
            // After picking up the object, local player should now be the owner.
            Assert.IsFalse(remotePlayer.IsOwner(_pickupObj));
            Assert.IsTrue(localPlayer.IsOwner(_pickupObj));

            yield return DropPickup();
        }
        
        [UnityTest]
        public IEnumerator TestPickupUseEventsHoldInitialGrab()
        {
            yield return StartClientSimAndCreatePickup();
            
            
            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;

            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);
            
            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            
            TestInput.SetInputUseGrab(true);
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);
            
            yield return null;
            
            // OnUseDown event only fires after holding the pickup for half a second
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));

            yield return ClientSimTestUtils.WaitUntil(
                () => Helper.GetLastPickupUseDown(HandType.RIGHT, false) != null,
                "Pickup Use Down event never fired",
                0.6f);
            
            Assert.IsTrue(Helper.GetLastPickupUseDown(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupUseUp(HandType.RIGHT, true) == _pickup);;
            
            
            // Now that initial hold delay has passed, check that trying to use fires event right away.
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupUseDown(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupUseUp(HandType.RIGHT, true) == _pickup);
            
            
            // Drop the pickup and ensure use events do not fire.
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(true);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(false);
            
            yield return null;
            
            // Drop event only happens after drop input has been released.
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
        }
        
        // This test checks that after picking up the object, releasing the use button, and waiting the initial delay, 
        // using the object will fire the use event right away.
        // This test also verifies that dropping the object will fire OnUseUp if use button still held.
        [UnityTest]
        public IEnumerator TestPickupUseEventsReleaseInitialGrab()
        {
            yield return StartClientSimAndCreatePickup();
            
            
            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);

            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);
            
            yield return new WaitForSeconds(0.3f);
            
            // OnUseDown event only fires after holding the pickup for half a second
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));

            TestInput.SetInputUseGrab(false);

            // Combined wait at this point should fire use down right away.
            yield return new WaitForSeconds(0.3f);
            
            TestInput.SetInputUseGrab(true);
            
            yield return null;

            Assert.IsTrue(Helper.GetLastPickupUseDown(HandType.RIGHT, true) == _pickup);
            
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupUseUp(HandType.RIGHT, true) == _pickup);
            
            
            // Now that initial hold delay has passed, check that trying to use fires event right away.
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(true);
            
            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupUseDown(HandType.RIGHT, true) == _pickup);
            
            // Check to ensure that use up fires before drop
            _testPickupHandler.onPickupUseUpAction += () =>
            {
                Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, false), "Pickup dropped before PickupUseUp fired!");
            };
            _testPickupHandler.onDropAction += () =>
            {
                Assert.IsTrue(Helper.GetLastPickupUseUp(HandType.RIGHT, false) == _pickup, "Pickup use up did not fire before pickup drop!");
            };
            
            // Drop the pickup and ensure use up event fires.
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(true);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputDrop(false);
            
            yield return null;
            
            // Drop event only happens after drop input has been released.
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            Assert.IsTrue(Helper.GetLastPickupUseUp(HandType.RIGHT, true) == _pickup);
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestPickupUseEventsNotAutoHold()
        {
            yield return StartClientSimAndCreatePickup();
            
            
            // Test can grab pickup
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            
            Helper.MoveObjectInFrontOfPlayer(_pickupObj.transform);

            yield return Helper.WaitUntilObjectHovered(_pickupObj, HandType.RIGHT);
            
            Assert.IsNull(Helper.GetLastPickupPickedUp(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            
            TestInput.SetInputUseGrab(true);

            yield return null;
            
            Assert.IsTrue(Helper.GetLastPickupPickedUp(HandType.RIGHT, true) == _pickup);

            // Wait long enough to pass the initial pickup use delay.
            yield return new WaitForSeconds(0.6f);
            
            // No use event should fire.
            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            TestInput.SetInputUseGrab(false);

            yield return null;

            Assert.IsNull(Helper.GetLastPickupUseDown(HandType.RIGHT, true));
            Assert.IsNull(Helper.GetLastPickupUseUp(HandType.RIGHT, true));
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPickupForceDrop()
        {
            yield return StartClientSimAndCreatePickup();
            
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();

            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;

            yield return GrabPickup();
            
            // Try having the remote player drop.
            LogAssert.Expect(LogType.Error, new Regex(".*Cannot Drop.*"));
            _pickup.Drop(remotePlayer);
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            
            _pickup.Drop();
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == _pickup);
            
            TestInput.SetInputUseGrab(false);
            
            yield return null;
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPickupDropOnDestroy()
        {
            yield return StartClientSimAndCreatePickup();

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            
            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;

            yield return GrabPickup();
            
            Assert.IsNull(Helper.GetLastPickupDropped(HandType.RIGHT, true));
            Assert.IsTrue(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right) == _pickup);
            
            // Destroy the pickup and verify it "dropped" properly.
            Object.DestroyImmediate(_pickupObj);
            
            // Destroying a Unity Object requires comparison with null rather than Assert.IsNull.
            Assert.IsTrue(Helper.GetLastPickupDropped(HandType.RIGHT, true) == null);
            Assert.IsNull(localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right));
            
            TestInput.SetInputUseGrab(false);

            yield return null;
        }
        
        
        [UnityTest]
        [Ignore("Log no longer appears")]
        public IEnumerator TestPickupHaptics()
        {
            yield return StartClientSimAndCreatePickup();

            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;

            yield return GrabPickup();
            
            LogAssert.Expect(LogType.Log, new Regex(".*\\[VRCPlayerAPI.PlayHapticEventInHand\\] Playing haptics for player.*"));
            _pickup.GenerateHapticEvent(1, 1, 1);

            yield return DropPickup();
        }
        
        [UnityTest]
        public IEnumerator TestPickupManipulation()
        {
            yield return StartClientSimAndCreatePickup();

            _pickup.allowManipulationWhenEquipped = true;
            
            yield return GrabPickup();
            
            // TODO Verify directionality of each motion to be sure they do what they should.
            
            Transform pickupTransform = _pickupObj.transform;
            Vector3 pickupPos1 = pickupTransform.position;
            Quaternion pickupRot1 = pickupTransform.rotation;
            
            // Test Mouse scroll to move forward and back.
            TestInput.SetInputPickupManipulateMove(10);

            yield return new WaitForSeconds(0.2f);

            Vector3 pickupPos2 = pickupTransform.position;
            Assert.IsTrue(Vector3.Distance(pickupPos1, pickupPos2) > 0);
            pickupPos1 = pickupPos2;

            TestInput.SetInputPickupManipulateMove(-10);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupPos2 = pickupTransform.position;
            Assert.IsTrue(Vector3.Distance(pickupPos1, pickupPos2) > 0);
            
            TestInput.SetInputPickupManipulateMove(0);
            
            // Test Rotating Right
            TestInput.SetInputPickupManipulateRotateRight(true);
            
            yield return new WaitForSeconds(0.2f);
            
            Quaternion pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            pickupRot1 = pickupRot2;
            
            TestInput.SetInputPickupManipulateRotateRight(false);
            
            // Test Rotating Left
            TestInput.SetInputPickupManipulateRotateLeft(true);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            pickupRot1 = pickupRot2;
            
            TestInput.SetInputPickupManipulateRotateLeft(false);
            
            // Test Rotating Up
            TestInput.SetInputPickupManipulateRotateUp(true);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            pickupRot1 = pickupRot2;
            
            TestInput.SetInputPickupManipulateRotateUp(false);
            
            // Test Rotating Down
            TestInput.SetInputPickupManipulateRotateDown(true);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            pickupRot1 = pickupRot2;
            
            TestInput.SetInputPickupManipulateRotateDown(false);
            
            // Test Rotating Clockwise
            TestInput.SetInputPickupManipulateRotateClockwise(true);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            pickupRot1 = pickupRot2;
            
            TestInput.SetInputPickupManipulateRotateClockwise(false);
            
            // Test Rotating Counter Clockwise
            TestInput.SetInputPickupManipulateRotateCounterClockwise(true);
            
            yield return new WaitForSeconds(0.2f);
            
            pickupRot2 = pickupTransform.rotation;
            Assert.IsTrue(Quaternion.Angle(pickupRot1, pickupRot2) > 0);
            
            TestInput.SetInputPickupManipulateRotateCounterClockwise(false);
            
            yield return DropPickup();
        }
        
        [UnityTest]
        public IEnumerator TestPickupManipulationDisabled()
        {
            yield return StartClientSimAndCreatePickup();
            
            _pickup.allowManipulationWhenEquipped = false;
            
            yield return GrabPickup();

            Transform pickupTransform = _pickupObj.transform;
            Vector3 pickupPos = pickupTransform.position;
            Quaternion pickupRot = pickupTransform.rotation;
            
            // Test Mouse scroll to move forward and back.
            TestInput.SetInputPickupManipulateMove(10);

            yield return new WaitForSeconds(0.1f);
            
            // Pickup has not moved.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateMove(-10);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateMove(0);
            
            // Test Rotating Right
            TestInput.SetInputPickupManipulateRotateRight(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateRight(false);
            
            // Test Rotating Left
            TestInput.SetInputPickupManipulateRotateLeft(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateLeft(false);
            
            // Test Rotating Up
            TestInput.SetInputPickupManipulateRotateUp(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateUp(false);
            
            // Test Rotating Down
            TestInput.SetInputPickupManipulateRotateDown(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateDown(false);
            
            // Test Rotating Clockwise
            TestInput.SetInputPickupManipulateRotateClockwise(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateClockwise(false);
            
            // Test Rotating Counter Clockwise
            TestInput.SetInputPickupManipulateRotateCounterClockwise(true);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pickupPos, pickupTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(pickupRot, pickupTransform.rotation), 0));
            
            TestInput.SetInputPickupManipulateRotateCounterClockwise(false);
            
            yield return DropPickup();
        }
        
        private static bool[] _pickupFollowValues = { false, true };
        [UnityTest]
        public IEnumerator TestPickupRotateFollow([ValueSource(nameof(_pickupFollowValues))] bool kinematic)
        {
            yield return StartClientSimAndCreatePickup();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            _pickup.pickupable = true;
            _pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            _pickupObj.GetComponent<Rigidbody>().isKinematic = kinematic;

            yield return GrabPickup();
            
            // Player is now holding the pickup. Rotate left and verify pickup moves right relative to the player.
            Vector3 playerPos = localPlayer.GetPosition();
            Vector3 pickupPos1 = _pickupObj.transform.position;
            TestInput.SetInputLookDelta(new Vector2(-100, 0));
            
            yield return new WaitForSeconds(0.2f);
            
            Vector3 pickupPos2 = _pickupObj.transform.position;
            Assert.IsTrue(Vector3.Distance(pickupPos1, pickupPos2) > 0.1f);

            Vector3 fromPlayerToPickup1 = new Vector3(pickupPos1.x, playerPos.y, pickupPos1.z) - playerPos;
            Vector3 fromPlayerToPickup2 = new Vector3(pickupPos2.x, playerPos.y, pickupPos2.z) - playerPos;
            // Moving left should be negative angle
            Assert.IsTrue(Vector3.SignedAngle(fromPlayerToPickup1, fromPlayerToPickup2, Vector3.up) < 0);
            
            // Rotate right
            TestInput.SetInputLookDelta(new Vector2(100, 0));
            pickupPos1 = pickupPos2;
            
            yield return new WaitForSeconds(0.2f);
            
            pickupPos2 = _pickupObj.transform.position;
            Assert.IsTrue(Vector3.Distance(pickupPos1, pickupPos2) > 0.1f);
            
            fromPlayerToPickup1 = new Vector3(pickupPos1.x, playerPos.y, pickupPos1.z) - playerPos;
            fromPlayerToPickup2 = new Vector3(pickupPos2.x, playerPos.y, pickupPos2.z) - playerPos;
            // Moving right should be positive angle
            Assert.IsTrue(Vector3.SignedAngle(fromPlayerToPickup1, fromPlayerToPickup2, Vector3.up) > 0);
            
            yield return DropPickup();
        }
    }
}