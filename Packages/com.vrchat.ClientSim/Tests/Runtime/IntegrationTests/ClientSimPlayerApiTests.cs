using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    public class ClientSimPlayerApiTests : ClientSimTestBase
    {
        [UnityTest]
        public IEnumerator TestPlayerLocomotionSettings()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            ClientSimPlayer localClientSimPlayer = localPlayer.GetClientSimPlayer();
            ClientSimPlayerLocomotionData localLocomotionData = localClientSimPlayer.locomotionData;
            
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            ClientSimPlayer remoteClientSimPlayer = remotePlayer.GetClientSimPlayer();
            ClientSimPlayerLocomotionData remoteLocomotionData = remoteClientSimPlayer.locomotionData;
            
            // Saving these values to compare later after modifying the local player to verify remote player was not modified.
            float remoteWalkSpeed = remoteLocomotionData.GetWalkSpeed();
            float remoteStrafeSpeed = remoteLocomotionData.GetStrafeSpeed();
            float remoteRunSpeed = remoteLocomotionData.GetRunSpeed();
            float remoteJumpImpulse = remoteLocomotionData.GetJump();
            float remoteGravityStrength = remoteLocomotionData.GetGravityStrength();
            bool remoteImmobile = remoteLocomotionData.GetImmobilized();
            bool remoteLegacyLocomotion = remoteLocomotionData.GetUseLegacyLocomotion();
            
            
            // Get the values and verify with locomotion data source.
            float walkSpeed = localPlayer.GetWalkSpeed();
            float strafeSpeed = localPlayer.GetStrafeSpeed();
            float runSpeed = localPlayer.GetRunSpeed();
            float jumpImpulse = localPlayer.GetJumpImpulse();
            float gravityStrength = localPlayer.GetGravityStrength();
            
            // No way using Player API to get these values
            bool immobilized = localLocomotionData.immobilized;
            bool useLegacyLocomotion = localLocomotionData.useLegacyLocomotion;
            
            Assert.IsTrue(Mathf.Approximately(walkSpeed, localLocomotionData.walkSpeed));
            Assert.IsTrue(Mathf.Approximately(strafeSpeed, localLocomotionData.strafeSpeed));
            Assert.IsTrue(Mathf.Approximately(runSpeed, localLocomotionData.runSpeed));
            Assert.IsTrue(Mathf.Approximately(jumpImpulse, localLocomotionData.jumpSpeed));
            Assert.IsTrue(Mathf.Approximately(gravityStrength, localLocomotionData.gravityStrength));
            
            // Verify adding values modifies the locomotion data.
            localPlayer.SetWalkSpeed(walkSpeed + 5);
            localPlayer.SetStrafeSpeed(strafeSpeed + 6);
            localPlayer.SetRunSpeed(runSpeed + 7);
            localPlayer.SetJumpImpulse(jumpImpulse + 8);
            localPlayer.SetGravityStrength(gravityStrength + 9);
            
            Assert.IsTrue(Mathf.Approximately(walkSpeed + 5, localLocomotionData.walkSpeed));
            Assert.IsTrue(Mathf.Approximately(strafeSpeed + 6, localLocomotionData.strafeSpeed));
            Assert.IsTrue(Mathf.Approximately(runSpeed + 7, localLocomotionData.runSpeed));
            Assert.IsTrue(Mathf.Approximately(jumpImpulse + 8, localLocomotionData.jumpSpeed));
            Assert.IsTrue(Mathf.Approximately(gravityStrength + 9, localLocomotionData.gravityStrength));
            
            // Verify getting values are still equal
            walkSpeed = localPlayer.GetWalkSpeed();
            strafeSpeed = localPlayer.GetStrafeSpeed();
            runSpeed = localPlayer.GetRunSpeed();
            jumpImpulse = localPlayer.GetJumpImpulse();
            gravityStrength = localPlayer.GetGravityStrength();
            
            Assert.IsTrue(Mathf.Approximately(walkSpeed, localLocomotionData.walkSpeed));
            Assert.IsTrue(Mathf.Approximately(strafeSpeed, localLocomotionData.strafeSpeed));
            Assert.IsTrue(Mathf.Approximately(runSpeed, localLocomotionData.runSpeed));
            Assert.IsTrue(Mathf.Approximately(jumpImpulse, localLocomotionData.jumpSpeed));
            Assert.IsTrue(Mathf.Approximately(gravityStrength, localLocomotionData.gravityStrength));
            
            // Ensure these values have not been modified when changing other values.
            Assert.IsTrue(immobilized == localLocomotionData.immobilized);
            Assert.IsTrue(useLegacyLocomotion == localLocomotionData.useLegacyLocomotion);
            
            localPlayer.Immobilize(true);
            Assert.IsTrue(localLocomotionData.immobilized);
            
            localPlayer.Immobilize(false);
            Assert.IsFalse(localLocomotionData.immobilized);
            
            localPlayer.UseLegacyLocomotion();
            Assert.IsTrue(localLocomotionData.useLegacyLocomotion);
            
            
            // Verify remote player values have not been modified.
            Assert.IsTrue(Mathf.Approximately(remoteWalkSpeed, remoteLocomotionData.GetWalkSpeed()));
            Assert.IsTrue(Mathf.Approximately(remoteStrafeSpeed, remoteLocomotionData.GetStrafeSpeed()));
            Assert.IsTrue(Mathf.Approximately(remoteRunSpeed, remoteLocomotionData.GetRunSpeed()));
            Assert.IsTrue(Mathf.Approximately(remoteJumpImpulse, remoteLocomotionData.GetJump()));
            Assert.IsTrue(Mathf.Approximately(remoteGravityStrength, remoteLocomotionData.GetGravityStrength()));
            Assert.IsTrue(remoteImmobile == remoteLocomotionData.GetImmobilized());
            Assert.IsTrue(remoteLegacyLocomotion == remoteLocomotionData.GetUseLegacyLocomotion());

            // Getting or setting any of remote player speed items throws exceptions in VRChat
            Assert.Throws<ClientSimException>(() => remotePlayer.GetWalkSpeed());
        }

        [UnityTest]
        public IEnumerator TestPlayerAudioSettings()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            ClientSimPlayer localClientSimPlayer = localPlayer.GetClientSimPlayer();
            ClientSimPlayerAudioData localAudioData = localClientSimPlayer.audioData;
            
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            ClientSimPlayer remoteClientSimPlayer = remotePlayer.GetClientSimPlayer();
            ClientSimPlayerAudioData remoteAudioData = remoteClientSimPlayer.audioData;

            // Save remote audio values to verify they do not change when local player values are changed.
            float voiceVolumetricRadius = remoteAudioData.voiceVolumetricRadius;
            float voiceDistanceNear = remoteAudioData.voiceDistanceNear;
            float voiceDistanceFar = remoteAudioData.voiceDistanceFar;
            float voiceGain = remoteAudioData.voiceGain;
            bool voiceLowpass = remoteAudioData.voiceLowpass;
            
            // Verify setting audio data for local player.
            SetPlayerAudioSetting(localPlayer, localAudioData);
            
            Assert.IsTrue(Mathf.Approximately(voiceVolumetricRadius, remoteAudioData.voiceVolumetricRadius));
            Assert.IsTrue(Mathf.Approximately(voiceDistanceNear, remoteAudioData.voiceDistanceNear));
            Assert.IsTrue(Mathf.Approximately(voiceDistanceFar, remoteAudioData.voiceDistanceFar));
            Assert.IsTrue(Mathf.Approximately(voiceGain, remoteAudioData.voiceGain));
            Assert.IsTrue(voiceLowpass == remoteAudioData.voiceLowpass);
            
            SetPlayerAudioSetting(remotePlayer, remoteAudioData);
        }

        private void SetPlayerAudioSetting(VRCPlayerApi player, ClientSimPlayerAudioData audioData)
        {
            // There are no getters for audio data, so all values must be checked against the audio data object.
            
            // Verify player voice audio settings
            float voiceVolumetricRadius = audioData.voiceVolumetricRadius;
            float voiceDistanceNear = audioData.voiceDistanceNear;
            float voiceDistanceFar = audioData.voiceDistanceFar;
            float voiceGain = audioData.voiceGain;
            bool voiceLowpass = audioData.voiceLowpass;
            
            player.SetVoiceVolumetricRadius(voiceVolumetricRadius + 5);
            player.SetVoiceDistanceNear(voiceDistanceNear + 6);
            player.SetVoiceDistanceFar(voiceDistanceFar + 7);
            player.SetVoiceGain(voiceGain + 8);
            player.SetVoiceLowpass(!voiceLowpass);
            
            Assert.IsTrue(Mathf.Approximately(voiceVolumetricRadius + 5, audioData.voiceVolumetricRadius));
            Assert.IsTrue(Mathf.Approximately(voiceDistanceNear + 6, audioData.voiceDistanceNear));
            Assert.IsTrue(Mathf.Approximately(voiceDistanceFar + 7, audioData.voiceDistanceFar));
            Assert.IsTrue(Mathf.Approximately(voiceGain + 8, audioData.voiceGain));
            Assert.IsTrue(!voiceLowpass == audioData.voiceLowpass);
            
            
            // Verify player avatar audio settings
            float avatarAudioVolumetricRadius = audioData.avatarAudioVolumetricRadius;
            float avatarAudioNearRadius = audioData.avatarAudioNearRadius;
            float avatarAudioFarRadius = audioData.avatarAudioFarRadius;
            float avatarAudioGain = audioData.avatarAudioGain;
            bool avatarAudioCustomCurve = audioData.avatarAudioCustomCurve;
            bool avatarAudioForceSpatial = audioData.avatarAudioForceSpatial;
            
            player.SetAvatarAudioVolumetricRadius(avatarAudioVolumetricRadius + 15);
            player.SetAvatarAudioNearRadius(avatarAudioNearRadius + 16);
            player.SetAvatarAudioFarRadius(avatarAudioFarRadius + 17);
            player.SetAvatarAudioGain(avatarAudioGain + 18);
            player.SetAvatarAudioCustomCurve(!avatarAudioCustomCurve);
            player.SetAvatarAudioForceSpatial(!avatarAudioForceSpatial);
            
            Assert.IsTrue(Mathf.Approximately(avatarAudioVolumetricRadius + 15, audioData.avatarAudioVolumetricRadius));
            Assert.IsTrue(Mathf.Approximately(avatarAudioNearRadius + 16, audioData.avatarAudioNearRadius));
            Assert.IsTrue(Mathf.Approximately(avatarAudioFarRadius + 17, audioData.avatarAudioFarRadius));
            Assert.IsTrue(Mathf.Approximately(avatarAudioGain + 18, audioData.avatarAudioGain));
            Assert.IsTrue(!avatarAudioCustomCurve == audioData.avatarAudioCustomCurve);
            Assert.IsTrue(!avatarAudioForceSpatial == audioData.avatarAudioForceSpatial);
        }
        
        [UnityTest]
        public IEnumerator TestPlayerTags()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            string tagKey1 = "Key1";
            string tagKey2 = "Key2";
            string tagValue1 = "Value1";
            string tagValue2 = "Value2";
            string tagValue3 = "Value3";
            string tagValue4 = "Value4";
            
            // Test key 1 for local and remote players
            string localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            string remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(string.IsNullOrEmpty(localKey1Value));
            Assert.IsTrue(string.IsNullOrEmpty(remoteKey1Value));
            
            localPlayer.SetPlayerTag(tagKey1, tagValue1);
            
            localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(localKey1Value == tagValue1);
            Assert.IsTrue(string.IsNullOrEmpty(remoteKey1Value));
            
            remotePlayer.SetPlayerTag(tagKey1, tagValue2);
            
            localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(localKey1Value == tagValue1);
            Assert.IsTrue(remoteKey1Value == tagValue2);
            
            // Test changing value for existing key
            localPlayer.SetPlayerTag(tagKey1, tagValue3);
            
            localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(localKey1Value == tagValue3);
            Assert.IsTrue(remoteKey1Value == tagValue2);
            
            
            // Test key 2 for local player.
            string localKey2Value = localPlayer.GetPlayerTag(tagKey2);
            string remoteKey2Value = remotePlayer.GetPlayerTag(tagKey2);
            Assert.IsTrue(string.IsNullOrEmpty(localKey2Value));
            Assert.IsTrue(string.IsNullOrEmpty(remoteKey2Value));
            
            localPlayer.SetPlayerTag(tagKey2, tagValue4);
            
            // Key 1 did not change
            localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(localKey1Value == tagValue3);
            Assert.IsTrue(remoteKey1Value == tagValue2);
            
            // Key 2 only set for local player
            localKey2Value = localPlayer.GetPlayerTag(tagKey2);
            remoteKey2Value = remotePlayer.GetPlayerTag(tagKey2);
            Assert.IsTrue(localKey2Value == tagValue4);
            Assert.IsTrue(string.IsNullOrEmpty(remoteKey2Value));

            // Clear only local player's tags
            LogAssert.Expect(LogType.Error, new Regex(".*Clearing all player tags\\. VRCPlayerApi\\.ClearPlayerTags is a dangerous call, as it will clear all the tags and this might break prefabs that rely on them\\."));
            localPlayer.ClearPlayerTags();
            
            // Verify tags are gone for local player.
            localKey1Value = localPlayer.GetPlayerTag(tagKey1);
            localKey2Value = localPlayer.GetPlayerTag(tagKey2);
            Assert.IsTrue(string.IsNullOrEmpty(localKey1Value));
            Assert.IsTrue(string.IsNullOrEmpty(localKey2Value));
            
            // Verify tag still exists for remote player.
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(remoteKey1Value == tagValue2);
            
            
            // Clear only local player's tags
            LogAssert.Expect(LogType.Error, new Regex(".*Clearing all player tags\\. VRCPlayerApi\\.ClearPlayerTags is a dangerous call, as it will clear all the tags and this might break prefabs that rely on them\\."));
            remotePlayer.ClearPlayerTags();
            
            remoteKey1Value = remotePlayer.GetPlayerTag(tagKey1);
            Assert.IsTrue(string.IsNullOrEmpty(remoteKey1Value));
            
            
            // TODO Unsure what these methods do, but adding to test to verify they do not throw exceptions
            localPlayer.SetSilencedToTagged(1, tagKey1, tagValue1);
            remotePlayer.SetSilencedToTagged(1, tagKey1, tagValue1);
            
            localPlayer.SetSilencedToUntagged(2, tagKey2, tagValue2);
            remotePlayer.SetSilencedToUntagged(2, tagKey2, tagValue2);
            
            localPlayer.ClearSilence();
            remotePlayer.ClearSilence();
            
            
            // TODO method not properly supported
            localPlayer.GetPlayersWithTag(tagKey1, tagValue1);
            remotePlayer.GetPlayersWithTag(tagKey2, tagValue2);
        }

        [UnityTest]
        public IEnumerator TestPlayerOwnership()
        {
            yield return LoadBasicScene();

            VRCPlayerApi oldMaster = null;
            VRCPlayerApi newMaster = null;

            void OnMasterSwitch(ClientSimOnNewMasterEvent masterEvent)
            {
                oldMaster = masterEvent.oldMasterPlayer;
                newMaster = masterEvent.newMasterPlayer;
            }
            
            EventDispatcher.Subscribe<ClientSimOnNewMasterEvent>(OnMasterSwitch);
            
            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                localPlayerIsMaster = false, // Ensure local player is not master
            };

            yield return StartClientSim(settings);
            
            
            VRCPlayerApi remotePlayer = VRCPlayerApi.GetPlayerById(1);
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi otherRemotePlayer = Helper.SpawnRemotePlayer();
            
            // Verify master
            Assert.IsNull(oldMaster);
            Assert.IsTrue(newMaster == remotePlayer);

            GameObject syncedObjectPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestObjectSync");
            
            // Create synced objects to test transferring ownership.
            GameObject syncedObject1 = Object.Instantiate(syncedObjectPrefab);
            GameObject syncedObject2 = Object.Instantiate(syncedObjectPrefab);;
            GameObject syncedObject3 = Object.Instantiate(syncedObjectPrefab);;

            // Allow object manager to pickup new synced objects.
            yield return null;

            
            // By default, owning player should be master, which is the remote player.
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == remotePlayer);
            
            // Set owner to current owner and verify no change.
            Networking.SetOwner(remotePlayer, syncedObject1);
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == remotePlayer);
            
            // Set ownership to the local player
            Networking.SetOwner(localPlayer, syncedObject2);
            Assert.IsTrue(Networking.GetOwner(syncedObject2) == localPlayer);
            Assert.IsTrue(Networking.IsOwner(syncedObject2));
            
            // Verify remote player still owns other object.
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == remotePlayer);
            Assert.IsFalse(Networking.IsOwner(syncedObject1));
            
            
            // Set ownership of 3rd object to other remote player.
            Networking.SetOwner(otherRemotePlayer, syncedObject3);
            Assert.IsTrue(Networking.GetOwner(syncedObject3) == otherRemotePlayer);
            Assert.IsFalse(Networking.IsOwner(syncedObject3));
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == remotePlayer);
            Assert.IsTrue(Networking.GetOwner(syncedObject2) == localPlayer);
            
            // Remove other remote player
            Helper.RemoveRemotePlayer(otherRemotePlayer);
            
            // Verify master has not changed.
            Assert.IsTrue(remotePlayer.isMaster);
            Assert.IsFalse(localPlayer.isMaster);
            Assert.IsNull(oldMaster);
            Assert.IsTrue(newMaster == remotePlayer);
            
            // Verify that master now owns the object the other remote player owned.
            Assert.IsFalse(Networking.GetOwner(syncedObject3) == otherRemotePlayer);
            Assert.IsTrue(Networking.GetOwner(syncedObject3) == remotePlayer);
            Assert.IsFalse(Networking.IsOwner(syncedObject3));
            
            // Verify ownership of other objects.
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == remotePlayer);
            Assert.IsTrue(Networking.GetOwner(syncedObject2) == localPlayer);
            
            
            // Remove current master
            Helper.RemoveRemotePlayer(remotePlayer);
            
            Assert.IsFalse(remotePlayer.isMaster);
            Assert.IsTrue(localPlayer.isMaster);
            Assert.IsTrue(oldMaster == remotePlayer);
            Assert.IsTrue(newMaster == localPlayer);
            
            // Verify local player owns all objects
            Assert.IsTrue(Networking.GetOwner(syncedObject1) == localPlayer);
            Assert.IsTrue(Networking.GetOwner(syncedObject2) == localPlayer);
            Assert.IsTrue(Networking.GetOwner(syncedObject3) == localPlayer);
            
            
            // Verify TakeOwnership and IsOwner Api
            otherRemotePlayer = Helper.SpawnRemotePlayer();
            otherRemotePlayer.TakeOwnership(syncedObject3);
            Assert.IsTrue(otherRemotePlayer.IsOwner(syncedObject3));
            Assert.IsFalse(localPlayer.IsOwner(syncedObject3));
            
            
            EventDispatcher.Unsubscribe<ClientSimOnNewMasterEvent>(OnMasterSwitch);
            
            Object.Destroy(syncedObject1);
            Object.Destroy(syncedObject2);
            Object.Destroy(syncedObject3);
        }

        [UnityTest]
        public IEnumerator TestPlayerTeleport()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            Vector3 localPos = localPlayer.GetPosition();
            Vector3 remotePos = remotePlayer.GetPosition();
            
            // Teleporting remote players does not do anything.
            LogAssert.Expect(LogType.Warning, new Regex(".*\\[VRCPlayerAPI.TeleportTo\\] Teleporting remote players will do nothing.*"));
            remotePlayer.TeleportTo(remotePos + Vector3.one, Quaternion.identity);
            LogAssert.Expect(LogType.Warning, new Regex(".*\\[VRCPlayerAPI.TeleportTo\\] Teleporting remote players will do nothing.*"));
            remotePlayer.TeleportTo(remotePos + Vector3.one, Quaternion.identity, VRC_SceneDescriptor.SpawnOrientation.Default);
            LogAssert.Expect(LogType.Warning, new Regex(".*\\[VRCPlayerAPI.TeleportTo\\] Teleporting remote players will do nothing.*"));
            remotePlayer.TeleportTo(remotePos + Vector3.one, Quaternion.identity, VRC_SceneDescriptor.SpawnOrientation.Default, true);

            // Verify remote player never moved.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(remotePos, remotePlayer.GetPosition()), 0));
            // Verify local player never moved either.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(localPos, localPlayer.GetPosition()), 0));
            
            
            
            Vector3 pos1 = Vector3.one;
            Quaternion rot1 = Quaternion.Euler(0, 75, 0);
            
            // Verify local player is not at this location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(pos1, localPlayer.GetPosition()), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(rot1, localPlayer.GetRotation()), 0));
            
            localPlayer.TeleportTo(pos1, rot1);
            
            // Verify local player is now at this location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pos1, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(rot1, localPlayer.GetRotation()), 0));
            
            
            
            // Verify that player rotation only rotates on y axis and not other axes.
            Vector3 pos2 = new Vector3(-1, 2, -3);
            Quaternion rot2 = Quaternion.Euler(12, 34, 56);
            Quaternion expectedRot2 = Quaternion.Euler(0, rot2.eulerAngles.y, 0);
            
            // Verify local player is not at this location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(pos2, localPlayer.GetPosition()), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(expectedRot2, localPlayer.GetRotation()), 0));
            
            localPlayer.TeleportTo(pos2, rot2);
            
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(rot2, localPlayer.GetRotation()), 0));
            
            // Verify local player is now at this location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pos2, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(expectedRot2, localPlayer.GetRotation()), 0));
            
            
            
            // Verify local player is not at this location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(pos1, localPlayer.GetPosition()), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(rot1, localPlayer.GetRotation()), 0));
            
            // Teleport based on playspace. This should be the same as normal teleportation.
            localPlayer.TeleportTo(pos1, rot1, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint);
            
            // Verify local player is now at this location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pos1, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(rot1, localPlayer.GetRotation()), 0));
            
            
            
            // Verify local player is not at this location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(pos2, localPlayer.GetPosition()), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(expectedRot2, localPlayer.GetRotation()), 0));
            
            // Lerp on remote does nothing locally.
            localPlayer.TeleportTo(pos2, rot2, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
            
            // Verify local player is now at this location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(pos2, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(expectedRot2, localPlayer.GetRotation()), 0));
        }

        [UnityTest]
        public IEnumerator TestPlayerRespawn()
        {
            yield return LoadBasicScene();

            bool playerRespawned = false;
            void OnPlayerRespawn(ClientSimOnPlayerRespawnEvent respawnEvent)
            {
                playerRespawned = true;
            }
            
            EventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);
            
            Assert.IsFalse(playerRespawned);
            // Test respawning from the menu
            Helper.RespawnPlayer();
            
            Assert.IsTrue(playerRespawned);
            playerRespawned = false;
            
            
            // Test going below respawn height
            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNotNull(descriptor);
            
            Vector3 belowRespawn = new Vector3(0, descriptor.RespawnHeightY - 10, 0);
            
            Networking.LocalPlayer.TeleportTo(belowRespawn, Quaternion.identity);

            // Wait a frame for player to detect it is under the respawn height
            yield return null;
            
            Assert.IsTrue(playerRespawned);
            
            // Test calling the Udon Respawn Method
            playerRespawned = false;
            Assert.IsFalse(playerRespawned);
            Networking.LocalPlayer.Respawn();
            Assert.True(playerRespawned);
            
            // Test calling the Udon Respawn Method with an integer
            playerRespawned = false;
            Assert.IsFalse(playerRespawned);
            var player = Networking.LocalPlayer;
            player.Respawn(0);
            Assert.True(playerRespawned);
            
            EventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
        }

        [UnityTest]
        public IEnumerator TestPlayerSetVelocity()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            Assert.IsTrue(localPlayer.IsPlayerGrounded(), "Player is not grounded initially.");
            Assert.IsTrue(localPlayer.GetVelocity().sqrMagnitude < 0.01f, "Player is does not have zero velocity initially.");

            Vector3 expectedVelocity = Vector3.up * 100;
            localPlayer.SetVelocity(expectedVelocity); 
            remotePlayer.SetVelocity(expectedVelocity);

            yield return null;
            
            Assert.IsFalse(localPlayer.IsPlayerGrounded(), "Player is still grounded after setting velocity upwards.");
            Vector3 velocity = localPlayer.GetVelocity();
            float velocityRatio = velocity.y / expectedVelocity.y;
            Assert.IsTrue(Vector3.Angle(velocity, expectedVelocity) < 0.01f, "Player velocity is not the same direction as expected velocity");
            Assert.IsTrue(0.8f <= velocityRatio && velocityRatio < 1.01f, "Player velocity is not within 80% of expected velocity magnitude.");
            
            // You cannot get velocity on remote players.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(remotePlayer.GetVelocity(), Vector3.zero), 0));
        }

        [UnityTest]
        public IEnumerator TestPlayerTrackingData()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();

            // Move the player to some arbitrary location to ensure values update.
            localPlayer.TeleportTo(Vector3.one, Quaternion.Euler(0, 35, 0));
            remotePlayer.gameObject.transform.SetPositionAndRotation(Vector3.back, Quaternion.Euler(0, 15, 0));
            
            
            var trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
            // Tracking data for the origin is equal to the player's current position.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(trackingData.position, localPlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, localPlayer.GetRotation()), 0));
            
            // Tracking data for the head is equal to the camera's location.
            Camera cam = VRC_UiShape.GetEventCamera();
            Transform camTransform = cam.transform;
            trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(trackingData.position, camTransform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, camTransform.rotation), 0));
            
            // Ensure left and right hand are non zero values
            trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(trackingData.position, Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, Quaternion.identity), 0));
            
            trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(trackingData.position, Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, Quaternion.identity), 0));
            
            
            
            // Test remote players
            
            // Origin for remote is their location.
            trackingData = remotePlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(trackingData.position, remotePlayer.GetPosition()), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, remotePlayer.GetRotation()), 0));
            
            // Test that remote player returns something that isn't zero.
            // TODO update this test if remote players can be Generic avatars
            trackingData = remotePlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(trackingData.position, Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, Quaternion.identity), 0));

            trackingData = remotePlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(trackingData.position, Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, Quaternion.identity), 0));
            
            trackingData = remotePlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(trackingData.position, Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(trackingData.rotation, Quaternion.identity), 0));
        }

        [UnityTest]
        public IEnumerator TestPlayerBoneData()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
                playerHeight = 1,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            // Move the player to some arbitrary location that isn't the zero point.
            localPlayer.TeleportTo(Vector3.one, Quaternion.Euler(0, 35, 0));
            remotePlayer.gameObject.transform.SetPositionAndRotation(Vector3.back, Quaternion.Euler(0, 15, 0));

            // Bone data is arbitrary based on the avatar. These tests are to verify that any data is returned and not zero.
            
            // Local player
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.Chest), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(localPlayer.GetBoneRotation(HumanBodyBones.Chest), Quaternion.identity), 0));
            
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.LeftHand), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(localPlayer.GetBoneRotation(HumanBodyBones.LeftHand), Quaternion.identity), 0));
            
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.RightFoot), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(localPlayer.GetBoneRotation(HumanBodyBones.RightFoot), Quaternion.identity), 0));
            
            // Remote player
            // TODO update this test if remote players can be Generic avatars
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(remotePlayer.GetBonePosition(HumanBodyBones.Head), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(remotePlayer.GetBoneRotation(HumanBodyBones.Head), Quaternion.identity), 0));
            
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(remotePlayer.GetBonePosition(HumanBodyBones.RightHand), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(remotePlayer.GetBoneRotation(HumanBodyBones.RightHand), Quaternion.identity), 0));
            
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(remotePlayer.GetBonePosition(HumanBodyBones.LeftFoot), Vector3.zero), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(remotePlayer.GetBoneRotation(HumanBodyBones.LeftFoot), Quaternion.identity), 0));
        }
        
        [UnityTest]
        [Ignore("Log no longer appears")]
        public IEnumerator TestPlayerPlayHaptics()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();

            LogAssert.Expect(LogType.Log, new Regex(".*\\[VRCPlayerAPI.PlayHapticEventInHand\\] Playing haptics for player.*"));
            localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 1, 1, 1);
            
            LogAssert.Expect(LogType.Warning, new Regex(".*\\[VRCPlayerAPI.PlayHapticEventInHand\\] PlayHapticEventInHand for remote players will do nothing.*"));
            remotePlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 1, 1, 1);
        }

        [UnityTest]
        public IEnumerator TestPlayerCombat()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            GameObject localSpawn = new GameObject("LocalSpawn");
            Transform localSpawnPoint = localSpawn.transform;
            localSpawnPoint.position = new Vector3(2, 0, 2);
            
            GameObject remoteSpawn = new GameObject("RemoteSpawn");
            Transform remoteSpawnPoint = remoteSpawn.transform;
            remoteSpawnPoint.position = new Vector3(-2, 0, -2);

            GameObject visualDamagePrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestVisualDamage");
            Assert.IsNotNull(visualDamagePrefab);

            ClientSimProxyObjects proxyObjects = Helper.GetProxyObjects();
            Transform cameraProxy = proxyObjects.CameraProxy();
            Assert.IsTrue(cameraProxy.childCount == 0);

            bool localPlayerDead = false;
            void OnPlayerDeathStatusChanged(ClientSimPlayerDeathStatusChangedEvent deathStatusChangedEvent)
            {
                Assert.IsTrue(deathStatusChangedEvent.player.isLocal, "Only local player should be sending combat events");
                localPlayerDead = deathStatusChangedEvent.isDead;
            }
            EventDispatcher.Subscribe<ClientSimPlayerDeathStatusChangedEvent>(OnPlayerDeathStatusChanged);
            
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();
            
            var pickupData = localPlayer.GetClientSimPlayer().pickupData;
            Assert.IsTrue(pickupData.pickupsEnabled);

            // Local player should not be near the combat respawn point.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(localSpawnPoint.position, localPlayer.GetPosition()), 0));
            // Remote player should not be near the combat respawn point.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(remoteSpawnPoint.position, remotePlayer.GetPosition()), 0));

            // Player health is -1 before combat setup.
            Assert.IsTrue(Mathf.Approximately(-1, localPlayer.CombatGetCurrentHitpoints()));
            Assert.IsTrue(Mathf.Approximately(-1, remotePlayer.CombatGetCurrentHitpoints()));
            Assert.IsFalse(localPlayerDead);
            
            localPlayer.CombatSetup();

            float maxHP = 333;
            float curHP = 111;
            localPlayer.CombatSetMaxHitpoints(maxHP);
            localPlayer.CombatSetCurrentHitpoints(curHP);
            localPlayer.CombatSetRespawn(true, 1e-8f, localSpawnPoint);
            localPlayer.CombatSetDamageGraphic(null);
            Assert.IsFalse(localPlayerDead);

            // provide one frame to allow local combat system to setup.
            yield return null;
            
            Assert.IsNotNull(localPlayer.CombatGetDestructible());
            
            // Verify that "visual damage" has been spawned for the local player.
            Assert.IsTrue(cameraProxy.childCount == 1);
            Transform visualDamageSpawn = cameraProxy.GetChild(0);
            Assert.IsNotNull(visualDamageSpawn);
            Assert.IsNotNull(visualDamageSpawn.GetComponent<Renderer>());
            Assert.IsNull(visualDamageSpawn.GetComponent<VRCVisualDamage>(), "Visual damage script was not destroyed. If this behaviour was fixed in VRC, please update this test.");

            
            // Current hit points are set to max on initialization.
            Assert.IsTrue(Mathf.Approximately(maxHP, localPlayer.CombatGetCurrentHitpoints()));
            Assert.IsTrue(Mathf.Approximately(-1, remotePlayer.CombatGetCurrentHitpoints()));
            
            remotePlayer.CombatSetup();

            remotePlayer.CombatSetMaxHitpoints(maxHP);
            remotePlayer.CombatSetRespawn(true, 1e-8f, remoteSpawnPoint);
            remotePlayer.CombatSetDamageGraphic(visualDamagePrefab);

            // This should do nothing
            localPlayer.CombatSetup();
            
            localPlayer.CombatSetCurrentHitpoints(curHP);
            Assert.IsTrue(Mathf.Approximately(curHP, localPlayer.CombatGetCurrentHitpoints()));
            Assert.IsFalse(localPlayerDead);
            
            // provide one frame to allow remote combat system to setup.
            yield return null;

            Assert.IsNotNull(remotePlayer.CombatGetDestructible());

            Assert.IsTrue(Mathf.Approximately(maxHP, remotePlayer.CombatGetCurrentHitpoints()));
            Assert.IsTrue(Mathf.Approximately(curHP, localPlayer.CombatGetCurrentHitpoints()));
            Assert.IsFalse(localPlayerDead);
            
            // Kill the local player.
            localPlayer.CombatSetCurrentHitpoints(0);
            Assert.IsTrue(localPlayerDead);
            Assert.IsTrue(Mathf.Approximately(0, localPlayer.CombatGetCurrentHitpoints()));
            
            Assert.IsTrue(Mathf.Approximately(maxHP, remotePlayer.CombatGetCurrentHitpoints()));
            
            // Local player cannot hold pickups after dying.
            Assert.IsFalse(pickupData.pickupsEnabled);
            
            yield return ClientSimTestUtils.WaitUntil(() => !localPlayerDead, "Local player never revived!", 0.2f);
            
            Assert.IsFalse(localPlayerDead);
            // Local player can hold pickups after reviving.
            Assert.IsTrue(pickupData.pickupsEnabled);
            
            // Local player should be at the respawn point.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(localSpawnPoint.position, localPlayer.GetPosition()), 0));
            
            // Remote player should not be at the spawn point still.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(remoteSpawnPoint.position, remotePlayer.GetPosition()), 0));
            
            
            EventDispatcher.Unsubscribe<ClientSimPlayerDeathStatusChangedEvent>(OnPlayerDeathStatusChanged);
            
            Object.Destroy(localSpawn);
            Object.Destroy(remoteSpawn);
        }
    }
}
