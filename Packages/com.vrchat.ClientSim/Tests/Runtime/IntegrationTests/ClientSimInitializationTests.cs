using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimInitializationTests : ClientSimTestBase
    {
        // This test is for checking if the ClientSim will prevent starting if there is no VRC_SceneDescriptor in the scene.
        [UnityTest]
        public IEnumerator TestNoWorldDescriptor()
        {
            // Load an empty scene with no VRC components.
            yield return LoadEmptyScene();

            Assert.IsFalse(ClientSimMain.HasInstance());
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
            };
            
            // Try to start ClientSim, but expect an exception that there is no world descriptor. 
            Assert.Throws<ClientSimException>(() => ClientSimRuntimeLoader.StartClientSim(settings));

            // Wait one frame for ClientSimMain to start.
            yield return null;
            
            Assert.IsFalse(Helper.HasReadyEventSent(), "ClientSim ready event sent when ClientSim shouldn't start.");
        }
        
        // This test is for checking if the ClientSim will prevent starting if there is a PipelineSaver in the scene
        // which is added when the user tries to upload.
        [UnityTest]
        public IEnumerator TestWithPipelineSaver()
        {
            // Load a basic world with only a floor and the VRC_SceneDescriptor. This is just enough the ClientSim to
            // properly initialize.
            yield return LoadBasicScene();

            Assert.IsFalse(ClientSimMain.HasInstance());

            // Adding this component should prevent ClientSim from starting due to thinking the user is uploading content.
            VRC_SceneDescriptor.Instance.gameObject.AddComponent<PipelineSaver>();
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
            };
            
            ClientSimRuntimeLoader.StartClientSim(settings);

            yield return null;
            
            Assert.IsFalse(ClientSimMain.HasInstance());
            Assert.IsFalse(Helper.HasReadyEventSent(), "ClientSim ready event sent when ClientSim shouldn't start!");
        }

        // Test that editor only objects are removed in the scene when DeleteEditorOnly is set to true on startup.
        [UnityTest]
        public IEnumerator TestDeleteEditorOnly()
        {
            yield return LoadBasicScene();

            GameObject editorOnlyObject = new GameObject {tag = "EditorOnly"};
            Assert.IsNotNull(editorOnlyObject);
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                deleteEditorOnly = true,
            };
            
            yield return StartClientSim(settings);

            // Must compare with null as UnityObjects override null comparison. 
            Assert.IsTrue(editorOnlyObject == null, "Editor only object is not null.");
        }
        
        // Test that editor only objects remain in the scene when the DeleteEditorOnly is set to false on startup.
        [UnityTest]
        public IEnumerator TestDeleteEditorOnlyDisabled()
        {
            yield return LoadBasicScene();

            GameObject editorOnlyObject = new GameObject {tag = "EditorOnly"};
            Assert.IsNotNull(editorOnlyObject);
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                deleteEditorOnly = false,
            };
            
            yield return StartClientSim(settings);

            // Must compare with null as UnityObjects override null comparison. 
            Assert.IsFalse(editorOnlyObject == null, "Editor only object is null.");
        }

        // Test if ClientSim will initialize properly and not spawn a controllable player for the user.
        [UnityTest]
        public IEnumerator TestLocalPlayerDisabled()
        {
            // Load a basic world with only a floor and the VRC_SceneDescriptor. This is just enough the ClientSim to
            // properly initialize.
            yield return LoadBasicScene();
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = false,
            };
            
            yield return StartClientSim(settings);
            
            VRCPlayerApi player = Networking.LocalPlayer;
            Assert.IsNotNull(player, "Local player is null after starting ClientSim.");
            Assert.IsNotNull(player.gameObject, "Local player does not have a GameObject.");
            Assert.IsFalse(player.gameObject.activeInHierarchy, "Local player is active when ClientSim settings request no player.");
        }
        
        // Test if ClientSim will initialize properly and spawn a controllable player for the user.
        // The local player should be master, instance master, and have the specified custom name.
        [UnityTest]
        public IEnumerator TestLocalPlayerEnabled()
        {
            // Load a basic world with only a floor and the VRC_SceneDescriptor. This is just enough the ClientSim to
            // properly initialize.
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                isInstanceOwner = true,
                localPlayerIsMaster = true,
                customLocalPlayerName = "Custom Player Name",
            };
            
            yield return StartClientSim(settings);
            
            VRCPlayerApi player = Networking.LocalPlayer;
            Assert.IsNotNull(player, "Local player is null after starting ClientSim.");
            Assert.IsNotNull(player.gameObject, "Local player does not have a GameObject.");
            Assert.IsTrue(player.gameObject.activeInHierarchy, "Local player is not active when ClientSim settings request player.");
            
            Assert.IsTrue(VRCPlayerApi.GetPlayerByGameObject(player.gameObject) == player, "Getting player by GameObject did not return the local player");

            Assert.IsTrue(settings.customLocalPlayerName.Equals(player.displayName), "Local player display name does not match settings.");
            
            Assert.IsTrue(Networking.IsMaster, "Local player is not master when ClientSim settings request player to be master.");
            Assert.IsTrue(player.isMaster, "Local player is not master when ClientSim settings request player to be master.");
            
            Assert.IsTrue(Networking.IsInstanceOwner, "Local player is not instance owner when ClientSim settings request player to be instance owner.");
            Assert.IsTrue(player.isInstanceOwner, "Local player is not instance owner when ClientSim settings request player to be instance owner.");
            
            Assert.IsTrue(player.isLocal, "Local player is not local.");
            Assert.IsTrue(player.playerId == 1, "Local player does not have id 1.");
            Assert.IsTrue(player.IsValid(), "Local player is not valid.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerById(1) == player, "Getting player by Id 1 did not return the local player");
            
            Assert.IsTrue(VRCPlayerApi.GetPlayerCount() == 1, "Player count after ClientSim start is not 1.");
            
            Assert.IsFalse(player.IsUserInVR(), "Local player is considered in VR when it should not be.");
        }
        
        // Test if ClientSim will initialize properly and spawn a controllable player for the user.
        // The local player should not be master or instance owner. A remote player will be spawned first that is master.
        [UnityTest]
        public IEnumerator TestLocalPlayerNonMaster()
        {
            // Load a basic world with only a floor and the VRC_SceneDescriptor. This is just enough the ClientSim to
            // properly initialize.
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                isInstanceOwner = false,
                localPlayerIsMaster = false,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi player = Networking.LocalPlayer;
            Assert.IsNotNull(player, "Local player is null after starting ClientSim.");
            Assert.IsNotNull(player.gameObject, "Local player does not have a GameObject.");
            Assert.IsTrue(player.gameObject.activeInHierarchy, "Local player is not active when ClientSim settings request player.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerByGameObject(player.gameObject) == player, "Getting player by GameObject did not return the local player");

            Assert.IsFalse(Networking.IsMaster, "Local player is master when ClientSim settings request player to not be master.");
            Assert.IsFalse(player.isMaster, "Local player is master when ClientSim settings request player to not be master.");
            
            Assert.IsFalse(Networking.IsInstanceOwner, "Local player is instance owner when ClientSim settings request player to not be instance owner.");
            Assert.IsFalse(player.isInstanceOwner, "Local player is instance owner when ClientSim settings request player to not be instance owner.");
            
            Assert.IsTrue(player.isLocal, "Local player is not local.");
            Assert.IsTrue(player.playerId == 2, "Local player does not have id 2.");
            Assert.IsTrue(player.IsValid(), "Local player is not valid.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerById(2) == player, "Getting player by Id 2 did not return the local player");
            
            Assert.IsTrue(VRCPlayerApi.GetPlayerCount() == 2, "Player count after ClientSim start is not 2.");

            VRCPlayerApi remotePlayer = VRCPlayerApi.AllPlayers[0];
            Assert.IsTrue(remotePlayer.IsValid(), "Remote player is not valid.");
            Assert.IsTrue(remotePlayer.playerId == 1, "Remote player does not have id 1.");
            Assert.IsFalse(remotePlayer.isLocal, "Remote player is local.");
            Assert.IsTrue(remotePlayer.isMaster, "Remote player is not master.");
            Assert.IsFalse(remotePlayer.isInstanceOwner, "Remote player is instance owner.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerById(1) == remotePlayer, "Getting player by Id 1 did not return the remote player");
            Assert.IsTrue(VRCPlayerApi.GetPlayerByGameObject(remotePlayer.gameObject) == remotePlayer, "Getting player by GameObject did not return the remote player");
        }
    }
}

