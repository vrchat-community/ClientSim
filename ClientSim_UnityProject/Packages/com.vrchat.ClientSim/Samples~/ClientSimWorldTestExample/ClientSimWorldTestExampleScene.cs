using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VRC.SDK3.ClientSim;
using VRC.SDK3.ClientSim.Tests.WorldTests;
using VRC.Udon.Common;

namespace ClientSimTest.Examples
{
    public class ClientSimWorldTestExampleScene : ClientSimWorldTestBase
    {
        private static readonly string _sceneName = "ClientSimWorldTestExample";
        private static readonly string _samplesDirectory = Path.Combine("Assets", "Samples", "VRChat Client Simulator");
        private static bool _sceneExists;
        
        protected override ClientSimSettings GetTestSettings()
        {
            return new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                deleteEditorOnly = false, // Ensure test helpers still exist in the scene.
                localPlayerIsMaster = true,
            };
        }

        protected override void SetupScene()
        {
            string[] guids = AssetDatabase.FindAssets($"t:scene {_sceneName}", new[] { _samplesDirectory });

            if (guids.Length == 0)
            {
                _sceneExists = false;
                return;
            }

            string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            
            _sceneExists = sceneAsset != null;
            if (_sceneExists)
            {
                LoadSceneFromPath(scenePath);
            }
        }

        [SetUp]
        public void CheckIfSceneExists()
        {
            Assert.IsTrue(_sceneExists, $"Failed to find Scene {_sceneName}! Please re-import the ClientSimWorldTestExample.");
        }
        
        // Test will go through the example scene. This scene is a small "puzzle" requiring multiple steps to reach the end area. 
        [UnityTest]
        public IEnumerator TestExampleScene()
        {
            yield return WaitForClientSimStartup();

            
            // Verify initial state of objects in the scene.
            var testHelpers = Object.FindObjectOfType<ClientSimWorldTestObjectReferences>();
            Assert.IsNotNull(testHelpers, "Could not find Test helper reference.");
            
            // Assert that some objects are disabled. 
            Assert.IsFalse(testHelpers.menu.activeInHierarchy, "Station Menu is enabled when it should be disabled.");
            Assert.IsFalse(testHelpers.endCredits.activeInHierarchy, "End Credits are enabled when it should be disabled.");
            
            // Assert that some objects are enabled.
            Assert.IsTrue(testHelpers.door1.activeInHierarchy, "Door 1 is disabled when it should be enabled.");
            Assert.IsTrue(testHelpers.door2.activeInHierarchy, "Door 2 is disabled when it should be enabled.");
            Assert.IsTrue(testHelpers.pickup.activeInHierarchy, "Pickup is disabled when it should be enabled.");
            
            bool door1Activated;
            string doorActivatedVariableName = "DoorOpen";
            Assert.IsTrue(testHelpers.doorController1.TryGetProgramVariable(doorActivatedVariableName, out door1Activated), $"Door Controller udon program did not have variable named {doorActivatedVariableName}.");
            Assert.IsFalse(door1Activated, "Door 1 controller has door activated when it should not be.");
            
            
            // Begin test walkthrough for the world

            // Close the menu before doing anything.
            Helper.CloseMenu();
            
            // Look at the station to try to enter it.
            Helper.LookAtObject(testHelpers.station.transform);

            // Wait for the station to be hovered, to know it can be interacted with.
            yield return Helper.WaitUntilObjectHovered(testHelpers.station, HandType.RIGHT);
            
            // Begin interact with station
            Helper.TestInput.SetInputUseGrab(true);
            yield return null;

            // Verify that the player interacted with the station and entered it.
            var lastInteract = Helper.GetLastInteractResults(HandType.RIGHT, true);
            Assert.IsNotNull(lastInteract, "Station was not interacted with.");
            Assert.IsTrue(lastInteract.interactObject == testHelpers.station, $"Last interacted object was not expected station: {lastInteract.interactObject}");

            var enteredStation = Helper.GetLastEnteredStation(true);
            Assert.IsNotNull(enteredStation, "Player did not enter the station.");
            Assert.IsTrue(enteredStation.gameObject == testHelpers.station, $"Player did not enter the expected station: {enteredStation.gameObject}");
            
            // Finish Interact with station
            Helper.TestInput.SetInputUseGrab(false);
            yield return null;
            
            
            // Behaviour after entering the station should be to enable the menu. Check if menu is now enabled.
            Assert.IsTrue(testHelpers.menu.activeInHierarchy, "Station Menu is not enabled after entering the station.");
            
            // Get button object and look at it to begin interact.
            var button = testHelpers.menu.GetComponentInChildren<Button>();
            Helper.LookAtObject(button.transform);
            
            // Wait for the menu canvas to be hovered.
            yield return Helper.WaitUntilObjectHovered(testHelpers.menu, HandType.RIGHT);
            
            // Begin interact with the button
            Helper.TestInput.SetInputUseGrab(true);
            yield return null;
            
            // Finish Interact with the button
            Helper.TestInput.SetInputUseGrab(false);
            yield return null;
            
            // UI objects require use down and use up to interact
            
            // Verify button was pressed. 
            Assert.IsFalse(testHelpers.door1.activeInHierarchy, "Door 1 is still disabled after clicking the button.");
            Assert.IsTrue(testHelpers.door2.activeInHierarchy, "Door 2 should still be disabled after clicking the button.");
            
            // Get the variable from the udon program and verify it is now true.
            Assert.IsTrue(testHelpers.doorController1.TryGetProgramVariable(doorActivatedVariableName, out door1Activated), $"Door Controller udon program did not have variable named {doorActivatedVariableName}.");
            Assert.IsTrue(door1Activated, "Door 1 controller door is not activated after clicking the button.");

            
            // Respawn to exit the station.
            Helper.RespawnPlayer();
            var exitedStation = Helper.GetLastExitedStation(true);
            Assert.IsNotNull(exitedStation, "Player did not exit the station.");
            Assert.IsTrue(exitedStation.gameObject == testHelpers.station, $"Player did not exit the expected station: {enteredStation.gameObject}");

            // Menu should be closed after exiting the station.
            Assert.IsFalse(testHelpers.menu.activeInHierarchy, "Station Menu is still enabled after exiting the station.");
            
            
            // Walk through the door and in front of the pickup using the helper walk locations.
            // Note that walking here is not important for this test and could be replaced with Teleportation. 
            Helper.TestInput.SetInputRun(true);
            Transform[] path1 = 
            {
                testHelpers.walkLocations[0], 
                testHelpers.walkLocations[1], 
                testHelpers.walkLocations[2]
            };
            yield return Helper.WalkThroughPoints(path1, "Player failed to walk through the door and to the pickup.", 2f);
            
            
            // Look at the pickup to grab it. 
            Helper.LookAtObject(testHelpers.pickup.transform);
            
            // Wait for the pickup to be hovered.
            yield return Helper.WaitUntilObjectHovered(testHelpers.pickup, HandType.RIGHT);
            
            // Grab the pickup
            Helper.TestInput.SetInputUseGrab(true);
            yield return null;

            // Verify we grabbed the pickup
            var lastPickup = Helper.GetLastPickupPickedUp(HandType.RIGHT, true);
            Assert.IsNotNull(lastPickup, "Failed to grab pickup.");
            Assert.IsTrue(lastPickup.gameObject == testHelpers.pickup, $"Object picked up was not the expected pickup. {lastPickup.gameObject}");
            
            // Release grab since pickup is auto hold.
            Helper.TestInput.SetInputUseGrab(false);
            yield return null;
            
            // Verify the pickup was not dropped after releasing grab.
            lastPickup = Helper.GetLastPickupDropped(HandType.RIGHT, true);
            Assert.IsNull(lastPickup, "Pickup should not have dropped after releasing grab.");
            
            
            // Walk in front of next door.
            Transform[] path2 = 
            {
                testHelpers.walkLocations[3], 
                testHelpers.walkLocations[4],
            };
            yield return Helper.WalkThroughPoints(path2, "Player failed to walk back from pickup area into next door.", 3f);

            
            // Path should have placed player within range of pickup detector. Expect that the pickup is now disabled and the second door is open.
            lastPickup = Helper.GetLastPickupDropped(HandType.RIGHT, true);
            Assert.IsNotNull(lastPickup, "Pickup was not dropped on entering pickup detection area.");
            Assert.IsFalse(testHelpers.pickup.activeInHierarchy, "Pickup was not disabled after entering detection area.");
            
            // Door should now be disabled.
            Assert.IsFalse(testHelpers.door2.activeInHierarchy, "Door 2 should be disabled after pickup entered detection area.");
            bool door2Activated;
            Assert.IsTrue(testHelpers.doorController2.TryGetProgramVariable(doorActivatedVariableName, out door2Activated), $"Door 2 Controller udon program did not have variable named {doorActivatedVariableName}.");
            Assert.IsTrue(door2Activated, "Door 2 controller door is not activated after pickup entered detection area.");
            
            
            // Verify credits have not yet been enabled.
            Assert.IsFalse(testHelpers.endCredits.activeInHierarchy, "End Credits are enabled after pickup detected when it should be disabled.");
            
            // Walk into player detector to enable end credits
            yield return Helper.WalkToPoint(testHelpers.walkLocations[5], "Player failed to walk to end area.");
            
            Assert.IsTrue(testHelpers.endCredits.activeInHierarchy, "End Credits are not enabled after player walked to end area.");
            
            
            
            // World has now been tested for one test case. It would be good to also verify other cases, such as player
            // walking towards end credits door without the pickup to ensure the door doesn't open and the player can't
            // walk through it.
        }
    }
}

