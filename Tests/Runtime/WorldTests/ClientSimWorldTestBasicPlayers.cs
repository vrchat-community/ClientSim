using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.WorldTests
{
    public class ClientSimWorldTestBasicPlayers : ClientSimWorldTestBase
    {
        protected override ClientSimSettings GetTestSettings()
        {
            return new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                isInstanceOwner = true,
                localPlayerIsMaster = true,
                customLocalPlayerName = "Custom Player Name",
            };
        }

        protected override void SetupScene()
        {
            // Load an basic scene with just a Scene Descriptor.
            LoadSceneFromPath(ClientSimTestSceneLoader.GetBasicScenePath());
        }
        
        [UnityTest]
        public IEnumerator TestTwoPlayersJoinLocalMaster()
        {
            yield return WaitForClientSimStartup();
            
            VRCPlayerApi player = Networking.LocalPlayer;
            Assert.IsTrue(VRCPlayerApi.GetPlayerCount() == 1, "Player count after ClientSim start is not 1.");
            
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            Assert.IsTrue(VRCPlayerApi.GetPlayerCount() == 2, "Player count after ClientSim start with remote player is not 2.");
            
            
            Assert.IsNotNull(player, "Local player is null after starting ClientSim.");
            Assert.IsNotNull(player.gameObject, "Local player does not have a GameObject.");
            Assert.IsTrue(player.gameObject.activeInHierarchy, "Local player is not active when ClientSim settings request player.");
            
            Assert.IsTrue(VRCPlayerApi.GetPlayerByGameObject(player.gameObject) == player, "Getting player by GameObject did not return the local player");

            Assert.IsTrue(Settings.customLocalPlayerName.Equals(player.displayName), "Local player display name does not match settings.");
            
            Assert.IsTrue(Networking.IsMaster, "Local player is not master when ClientSim settings request player to be master.");
            Assert.IsTrue(player.isMaster, "Local player is not master when ClientSim settings request player to be master.");
            
            Assert.IsTrue(Networking.IsInstanceOwner, "Local player is not instance owner when ClientSim settings request player to be instance owner.");
            Assert.IsTrue(player.isInstanceOwner, "Local player is not instance owner when ClientSim settings request player to be instance owner.");
            
            Assert.IsTrue(player.isLocal, "Local player is not local.");
            Assert.IsTrue(player.playerId == 1, "Local player does not have id 1.");
            Assert.IsTrue(player.IsValid(), "Local player is not valid.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerById(1) == player, "Getting player by Id 1 did not return the local player");
            
            Assert.IsFalse(player.IsUserInVR(), "Local player is considered in VR when it should not be.");
            
            
            // Verify Remote Player
            Assert.IsTrue(remotePlayer.IsValid(), "Remote player is not valid.");
            Assert.IsTrue(remotePlayer.playerId == 2, "Remote player does not have id 2.");
            Assert.IsFalse(remotePlayer.isLocal, "Remote player is local.");
            Assert.IsFalse(remotePlayer.isMaster, "Remote player is not master.");
            Assert.IsFalse(remotePlayer.isInstanceOwner, "Remote player is instance owner.");
            Assert.IsTrue(VRCPlayerApi.GetPlayerById(2) == remotePlayer, "Getting player by Id 2 did not return the remote player");
            Assert.IsTrue(VRCPlayerApi.GetPlayerByGameObject(remotePlayer.gameObject) == remotePlayer, "Getting player by GameObject did not return the remote player");
        }
    }
}

