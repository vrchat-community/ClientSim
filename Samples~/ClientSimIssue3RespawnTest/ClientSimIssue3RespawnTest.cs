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

namespace ClientSimTest.Tests.WorldTests
{
    public class ClientSimIssue3RespawnTest : ClientSimWorldTestBase
    {
        private static readonly string _sceneName = "ClientSimIssue3RespawnTest";
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
        public IEnumerator Player_WalkingIntoRespawnArea_ShouldRespawnAndTriggerRespawnEvent()
        {
            yield return WaitForClientSimStartup();
            
            // Verify initial state of objects in the scene.
            var testHelpers = Object.FindObjectOfType<ClientSimIssue3RespawnTestObjectReferences>();
            Assert.IsNotNull(testHelpers, "Could not find Test helper reference.");
            
            // Begin test walkthrough for the world

            // Close the menu before doing anything.
            Helper.CloseMenu();
            
            // Walk into the respawn cube.
            Helper.TestInput.SetInputRun(true);

            yield return Helper.WalkToPoint(testHelpers.respawnCube.gameObject.transform, "Didn't reach RespawnCube");

            // Wait for the same amount of time the UdonBehaviour uses to delay the Respawn call
            yield return new WaitForSeconds(testHelpers.respawnCube.GetProgramVariable<float>("delay"));

            //Verify that the player is very close to Spawn
            Vector3 playerPosition = Helper.GetLastJoinPlayer().GetPosition();
            float distance = Vector3.Distance(playerPosition, testHelpers.spawn.position);
            Assert.IsTrue(distance < 0.25f, "Player failed to respawn.");
            
            // Ensure OnPlayerRespawn event fired by checking variable set by this event
            Assert.IsTrue(testHelpers.respawnCube.GetProgramVariable<bool>("respawned"));
        }
    }
}

