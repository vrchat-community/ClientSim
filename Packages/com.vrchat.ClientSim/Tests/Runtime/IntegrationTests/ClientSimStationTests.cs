using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimStationTests : ClientSimTestBase
    {
        // TODO VRCStation.CanUseStationFromStation does not block the player from entering the station, but only blocks
        // if the user can interact with objects that have VRCStation component on them. You can still force players to
        // enter a station. Creating a test for this case requires checking if interact is possible.

        // TODO other tests:
        // Player look rotation limits
        // Player is allowed to be on other rotations
        // Moving stations (Player's position follows the station)
        // Crouch/Prone does not move camera

        private GameObject _stationObj;
        private VRCStation _station;
        
        
        private ClientSimTestStationHandler _testStationHandler;

        private GameObject SpawnStation()
        {
            GameObject stationPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestStation");
            return Object.Instantiate(stationPrefab);
        }
        
        private IEnumerator CreateStation()
        {
            _stationObj = SpawnStation();
            
            // Give time to initialize
            yield return null;
            
            _station = _stationObj.GetComponent<VRCStation>();
            Assert.IsNotNull(_station);
            Assert.IsNotNull(_stationObj.GetComponent<ClientSimStationHelper>());

            _testStationHandler = _stationObj.GetComponent<ClientSimTestStationHandler>();
            Assert.IsNotNull(_testStationHandler);
        }

        [TearDown]
        public void StationTestTearDown()
        {
            if (_stationObj != null)
            {
                Object.Destroy(_stationObj);
            }
        }
        
        private IEnumerator StartClientSimAndCreateStation()
        {
            yield return LoadBasicScene();
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                initializationDelay = 0f
            };

            yield return StartClientSim(settings);

            yield return CreateStation();
            
            // Ensure menu is closed, allowing the player to walk and interact
            Helper.CloseMenu();

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestStationEnterExit()
        {
            yield return StartClientSimAndCreateStation();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            // UseAttachedStation is tested in the sample world test: ClientSimWorldTestExampleScene
            // It cannot be tested here due to needing udon to be running for it to work. ClientSim Tests have no udon
            // programs due to package limitations. 
            // localPlayer.UseAttachedStation();
            
            // Note that in VRChat, entering and exiting a station does not actually move the player to the expected
            // location within one frame like this. 
            
            // Try enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);

            // Verify player is now at the enter location
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));

            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            // Verify player is now at the exit location
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationExitPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationExitPlayerLocation.rotation, localPlayer.GetRotation()), 0));
        }
        
        [UnityTest]
        public IEnumerator TestStationEnterExitWhenMoving()
        {
            yield return StartClientSimAndCreateStation();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            
            _station.disableStationExit = false;
            
            // Try enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Force player to move forward to exit the station.
            Assert.IsNull(Helper.GetLastExitedStation(true));
            TestInput.SetInputMoveForward(true);

            yield return ClientSimTestUtils.WaitUntil(() => Helper.GetLastExitedStation(false) == _station, "Player never left the station!", 1f);
            
            // Player should have moved, forcing them to exit the station. 
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            TestInput.SetInputMoveForward(false);
            
            yield return null;

            // Prevent exiting the station on movement.
            _station.disableStationExit = true;

            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            TestInput.SetInputMoveForward(true);
            
            yield return null;
            
            TestInput.SetInputMoveForward(false);
            
            // Player should not have exited
            Assert.IsNull(Helper.GetLastExitedStation(true));
            
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
        }
        
        [UnityTest]
        public IEnumerator TestStationRemotePlayerTryEnterExit()
        {
            yield return StartClientSimAndCreateStation();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            // Try forcing remote player in station
            LogAssert.Expect(LogType.Warning, new Regex(".*Trying to force a remote player to enter a station\\. Force enter a station can only be done for the local player.*"));
            _station.UseStation(remotePlayer);
            
            // Try forcing remote player to exit station, when not in the station
            LogAssert.Expect(LogType.Warning, new Regex(".*Trying to force a remote player to exit a station\\. Force exit a station can only be done for the local player.*"));
            _station.ExitStation(remotePlayer);
            
            
            // Try enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);

            
            // Try forcing remote player in station while local player is in the station
            LogAssert.Expect(LogType.Warning, new Regex(".*Trying to force a remote player to enter a station\\. Force enter a station can only be done for the local player.*"));
            _station.UseStation(remotePlayer);

            // Try forcing remote player to exit while player is in the station.
            LogAssert.Expect(LogType.Warning, new Regex(".*Trying to force a remote player to exit a station\\. Force exit a station can only be done for the local player.*"));
            _station.ExitStation(remotePlayer);

            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
        }
        
        [UnityTest]
        public IEnumerator TestStationTeleportExit()
        {
            yield return StartClientSimAndCreateStation();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            
            // Enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);

            // Teleport the player to exit the station.
            Assert.IsNull(Helper.GetLastExitedStation(true));
            localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            
            // Enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);

            // Respawn the player to exit the station.
            Assert.IsNull(Helper.GetLastExitedStation(true));
            Helper.RespawnPlayer();
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
        }

        [UnityTest]
        public IEnumerator TestStationMoveStationBelowRespawnHeight()
        {
            bool playerRespawned = false;
            void OnPlayerRespawned(ClientSimOnPlayerRespawnEvent respawnEvent)
            {
                playerRespawned = true;
            }
            EventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawned);
            
            yield return StartClientSimAndCreateStation();

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            
            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNotNull(descriptor);
            
            Vector3 belowRespawn = new Vector3(0, descriptor.RespawnHeightY - 10, 0);
            
            // Enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);

            Assert.IsFalse(playerRespawned);
            
            // Teleport the station below respawn height to respawn the player and exit the station.
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _stationObj.transform.position = belowRespawn;

            // Wait a frame for player controller to see it is below respawn height.
            yield return null;
            
            // Verify player has respawned
            Assert.IsTrue(playerRespawned);
            
            // Verify player has exited the station.
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            EventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawned);
        }
        
        // Test the timing for when the OnStationEntered and OnStationExited events fire relative to the player moving in the station.
        [UnityTest]
        public IEnumerator TestStationEnterExitTiming()
        {
            yield return StartClientSimAndCreateStation();
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            _station.transform.position = Vector3.zero;
            
            Vector3 stationEnteredLocation = new Vector3(2, 0, 0);
            Vector3 stationExitedLocation = new Vector3(-2, 0, 0);

            // On enter, move the station to the right.
            // Assert that player has already moved to the enter location of the station.
            _testStationHandler.onEnterStation += station =>
            {
                // Verify player is now at the enter location
                Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
                Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));

                station.transform.position = stationEnteredLocation;
            };
            
            // On exit, move the station to the left.
            // On exit event is handled before the player is moved to the station exit location.
            _testStationHandler.onExitStation += station =>
            {
                // Verify player is still at the enter location
                Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
                Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));

                station.transform.position = stationExitedLocation;
            };
            
            
            
            // Try enter station
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Verify station has moved.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.transform.position, stationEnteredLocation), 0));

            // Verify player is now at the old enter location
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position - stationEnteredLocation, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));

            // Wait one frame for the player's position to update.
            yield return null;
            
            // Verify player is now at the expected enter location
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));

            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            // Verify station has moved.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.transform.position, stationExitedLocation), 0));
            
            // Verify player is now at the exit location
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationExitPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationExitPlayerLocation.rotation, localPlayer.GetRotation()), 0));
        }

        // Test the 4 options between seated and immobility of a station.
        [UnityTest]
        public IEnumerator TestStationSeatedMobileSettings()
        {
            yield return StartClientSimAndCreateStation();

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            ClientSimPlayerLocomotionData locomotionData = localPlayer.GetClientSimPlayer().locomotionData;

            // Disable station exit to properly test mobile stations.
            _station.disableStationExit = true;
            
            
            
            // Test standard station with mobility settings.
            _station.seated = true;
            _station.PlayerMobility = VRCStation.Mobility.Immobilize;
            
            // Enter the station.
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Sitting in a non-mobile station sets player immobile
            Assert.IsTrue(locomotionData.GetImmobilized());
            
            // Try to move forward.
            TestInput.SetInputMoveForward(true);

            yield return new WaitForSeconds(0.3f);
            
            TestInput.SetInputMoveForward(false);
            
            // Verify player never moved out of the station.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));
            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            Assert.IsFalse(locomotionData.GetImmobilized());

            yield return null;
            
            
            // Test not seated immobile station
            // Expectation is that player will still stick to the station, but exiting the station will prevent movement.
            // TODO change this test when the bug is fixed in VRChat.
            _station.seated = false;
            _station.PlayerMobility = VRCStation.Mobility.Immobilize;
            
            // Enter the station.
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Sitting in a non-mobile station sets player immobile
            Assert.IsTrue(locomotionData.GetImmobilized());
            
            // Try to move forward.
            TestInput.SetInputMoveForward(true);

            yield return new WaitForSeconds(0.3f);
            
            TestInput.SetInputMoveForward(false);
            
            // Verify player never moved out of the station.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));
            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            Assert.IsTrue(locomotionData.GetImmobilized());
            localPlayer.Immobilize(false);
            Assert.IsFalse(locomotionData.GetImmobilized());

            yield return null;
            
            
            // Test seated mobile station
            // Expectation is that player will still stick to the station as with immobile station.
            _station.seated = true;
            _station.PlayerMobility = VRCStation.Mobility.Mobile;
            
            // Enter the station.
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Sitting in a non-mobile station sets player immobile
            Assert.IsTrue(locomotionData.GetImmobilized());
            
            // Try to move forward.
            TestInput.SetInputMoveForward(true);

            yield return new WaitForSeconds(0.3f);
            
            TestInput.SetInputMoveForward(false);
            
            // Verify player never moved out of the station.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(_station.stationEnterPlayerLocation.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(_station.stationEnterPlayerLocation.rotation, localPlayer.GetRotation()), 0));
            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            Assert.IsFalse(locomotionData.GetImmobilized());

            yield return null;
            
            
            // Test non-seated mobile station
            // Expectation is that player will be able to move around while in the station.
            _station.seated = false;
            _station.PlayerMobility = VRCStation.Mobility.Mobile;

            Vector3 stationEnterPoint = _station.stationEnterPlayerLocation.position;
            localPlayer.TeleportTo(stationEnterPoint, Quaternion.identity);

            // Enter the station.
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            _station.UseStation(localPlayer);
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            
            // Sitting in a mobile station does not set player immobile, allowing them to move while in the station.
            Assert.IsFalse(locomotionData.GetImmobilized());
            
            // Verify player has not moved yet.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(stationEnterPoint, localPlayer.GetPosition()), 0));

            
            // Try to move forward.
            TestInput.SetInputMoveForward(true);

            bool PlayerMoved()
            {
                float playerDistance = Vector3.Distance(stationEnterPoint, localPlayer.GetPosition());
                return playerDistance > 0.2f;
            }

            yield return ClientSimTestUtils.WaitUntil(PlayerMoved, "Player never moved away from the station.", 0.4f);
            
            TestInput.SetInputMoveForward(false);
            
            Assert.IsNull(Helper.GetLastExitedStation(true));
            _station.ExitStation(localPlayer);
            Assert.IsTrue(Helper.GetLastExitedStation(true) == _station);
            
            Assert.IsFalse(locomotionData.GetImmobilized());
        }
        
        [UnityTest]
        public IEnumerator TestStationDestroyed()
        {
            yield return StartClientSimAndCreateStation();

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            localPlayer.TeleportTo(Vector3.zero, Quaternion.identity);
            
            // Try enter station
            Assert.IsFalse(Helper.IsLocalPlayerInStation());
            Assert.IsNull(Helper.GetLastEnteredStation(true));
            
            _station.UseStation(localPlayer);
            
            Assert.IsTrue(Helper.GetLastEnteredStation(true) == _station);
            Assert.IsTrue(Helper.IsLocalPlayerInStation());
            Assert.IsNull(Helper.GetLastExitedStation(true));
            
            // Destroy the station to force exit the player.
            Object.DestroyImmediate(_stationObj);

            // Destroying a Unity Object requires comparison with null rather than Assert.IsNull.
            Assert.IsTrue(Helper.GetLastExitedStation(true) == null);
            Assert.IsFalse(Helper.IsLocalPlayerInStation());
        }
    }
}