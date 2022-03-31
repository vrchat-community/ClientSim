using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.ClientSim;
using VRC.SDK3.ClientSim.Tests.WorldTests;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace ClientSimTest.Tests.WorldTests
{
    public class ClientSimIssue3RespawnTest : ClientSimWorldTestBase
    {
        private static readonly string _sceneName = "ClientSimIssue3RespawnTest";
        private static readonly string _samplesDirectory = Path.Combine("Assets", "Samples", "VRChat Client Simulator");
        private static readonly string _programVariableRespawned = "respawned";
        private static readonly string _programVariableDelay = "delay";
        private static bool _sceneExists;
        
        protected override ClientSimSettings GetTestSettings()
        {
            return new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0.5f, // Helps catch Udon errors that run in Start()
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
        
        // Test will have the Player walk into a trigger which calls Respawn
        [UnityTest]
        public IEnumerator Player_WalkingIntoRespawnObject_ShouldRespawnAndTriggerRespawnEvent()
        {
            yield return WaitForClientSimStartup();
            
            // Verify initial state of objects in the scene.
            var testHelpers = Object.FindObjectOfType<ClientSimIssue3RespawnTestObjectReferences>();
            Assert.IsNotNull(testHelpers, "Could not find Test helper reference.");

            // Listen for OnPlayerRespawn event through dispatcher
            bool respawnEventFromDispatcherTriggered = false;
            void OnPlayerRespawn(ClientSimOnPlayerRespawnEvent respawnEvent)
            {
                respawnEventFromDispatcherTriggered = true;
            }
            EventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);

            // Close the menu before doing anything.
            Helper.CloseMenu();
            
            // Walk into the respawn cube.
            Helper.TestInput.SetInputRun(true);

            yield return Helper.WalkToPoint(testHelpers.respawnCube.gameObject.transform, "Didn't reach RespawnCube");

            // Wait for the same amount of time the UdonBehaviour uses to delay the Respawn call
            yield return new WaitForSeconds(testHelpers.respawnCube.GetProgramVariable<float>(_programVariableDelay));

            // Verify that the player is very close to Spawn
            Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
            float distance = Vector3.Distance(playerPosition, testHelpers.spawn1.position);
            Assert.IsTrue(distance < 0.25f, "Player failed to respawn.");
            
            // Ensure OnPlayerRespawn event fired by checking variable set by this event
            Assert.IsTrue(testHelpers.respawnCube.GetProgramVariable<bool>(_programVariableRespawned));
            
            // Ensure OnPlayerRespawn fired by EventDispatcher
            Assert.IsTrue(respawnEventFromDispatcherTriggered);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
        }
        
        // Test will have the Player walk into a trigger which calls Respawn with Index
        [UnityTest]
        public IEnumerator Player_WalkingIntoRespawnWithIndexObject_ShouldRespawnAndTriggerRespawnEvent()
        {
            yield return WaitForClientSimStartup();
            
            // Verify initial state of objects in the scene.
            var testHelpers = Object.FindObjectOfType<ClientSimIssue3RespawnTestObjectReferences>();
            Assert.IsNotNull(testHelpers, "Could not find Test helper reference.");
            
            // Listen for OnPlayerRespawn event through dispatcher
            bool respawnEventFromDispatcherTriggered = false;
            void OnPlayerRespawn(ClientSimOnPlayerRespawnEvent respawnEvent)
            {
                respawnEventFromDispatcherTriggered = true;
            }
            EventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);

            // Close the menu before doing anything.
            Helper.CloseMenu();
            
            // Walk into the respawn cube.
            Helper.TestInput.SetInputRun(true);

            yield return Helper.WalkToPoint(testHelpers.respawnWithIndexCube.gameObject.transform, "Didn't reach RespawnWithIndexCube");

            // Wait for the same amount of time the UdonBehaviour uses to delay the Respawn call
            yield return new WaitForSeconds(testHelpers.respawnWithIndexCube.GetProgramVariable<float>(_programVariableDelay));

            // Verify that the player is very close to Spawn
            Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
            float distance = Vector3.Distance(playerPosition, testHelpers.spawn2.position);
            Assert.IsTrue(distance < 0.25f, "Player failed to respawn.");
            
            // Ensure OnPlayerRespawn event fired by checking variable set by this event
            Assert.IsTrue(testHelpers.respawnWithIndexCube.GetProgramVariable<bool>(_programVariableRespawned));
            
            // Ensure OnPlayerRespawn fired by EventDispatcher
            Assert.IsTrue(respawnEventFromDispatcherTriggered);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
        }

        [UnityTest]
        public IEnumerator Player_RespawnedWithInvalidIndex_RespawnsAtIndex0()
        {
            yield return WaitForClientSimStartup();
            
            // Verify initial state of objects in the scene.
            var testHelpers = Object.FindObjectOfType<ClientSimIssue3RespawnTestObjectReferences>();
            Assert.IsNotNull(testHelpers, "Could not find Test helper reference.");
            
            // Listen for OnPlayerRespawn event through dispatcher
            bool respawnEventFromDispatcherTriggered = false;
            void OnPlayerRespawn(ClientSimOnPlayerRespawnEvent respawnEvent)
            {
                respawnEventFromDispatcherTriggered = true;
            }
            EventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);

            // Call Respawn with invalid index
            ClientSimPlayerManager.RespawnWithIndex(Networking.LocalPlayer, -1);
            
            // Ensure OnPlayerRespawn fired by EventDispatcher
            Assert.IsTrue(respawnEventFromDispatcherTriggered);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
            
            // Check if Player is near spawn 0
            Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
            float distance = Vector3.Distance(playerPosition, testHelpers.spawn1.position);
            Assert.IsTrue(distance < 0.25f, "Player failed to respawn.");
        }
    }
}

