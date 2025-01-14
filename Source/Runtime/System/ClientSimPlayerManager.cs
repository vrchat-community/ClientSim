using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using Object = UnityEngine.Object;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System that manages all the players currently in ClientSim.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimOnNewMasterEvent
    /// - ClientSimOnPlayerJoinedEvent
    /// - ClientSimOnPlayerLeftEvent
    /// </remarks>
    public class ClientSimPlayerManager : IClientSimPlayerManager, IDisposable
    {
        private int _masterID = -1;
        private int _localPlayerID = -1;
        private int _nextPlayerID = 1;
        private readonly Dictionary<VRCPlayerApi, int> _playerIDs = new Dictionary<VRCPlayerApi, int>();
        private readonly Dictionary<int, VRCPlayerApi> _players = new Dictionary<int, VRCPlayerApi>();

        // List of players that have joined before ClientSim has finished initializing.
        private readonly List<VRCPlayerApi> _waitingPlayers = new List<VRCPlayerApi>();
        private bool _networkReady = false;
        
        private VRCPlayerObject[] _playerObjectList;
        
        private VRCPlayerApi _localPlayer;

        private IClientSimEventDispatcher _eventDispatcher;
        private static IClientSimPlayerHeightManager _heightManager;

        public ClientSimPlayerManager(IClientSimEventDispatcher eventDispatcher, IClientSimPlayerHeightManager heightManager)
        {
            _eventDispatcher = eventDispatcher;
            _heightManager = heightManager;
            
            // Ensure no other players have been added to the list. 
            VRCPlayerApi.AllPlayers.Clear();
        }

        ~ClientSimPlayerManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            // Prevent sending new events as players are cleared.
            _eventDispatcher = null;
            
            // Dispose of all players added by this manager.
            List<VRCPlayerApi> players = new List<VRCPlayerApi>(_playerIDs.Keys);
            foreach (VRCPlayerApi player in players)
            {
                RemovePlayer(player);
            }
            
            _heightManager?.Dispose();
        }

        private int GetNextPlayerId()
        {
            int id = _nextPlayerID;
            ++_nextPlayerID;
            return id;
        }
        
        private void InitializePlayer(ClientSimPlayer clientSimPlayer, VRCPlayerApi player, int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                throw new ClientSimException($"PlayerId already assigned to player! {playerId}");
            }
            
            this.Log($"Assigning player id {playerId}");

            _playerIDs[player] = playerId;
            _players[playerId] = player;

            // Adding player to the list makes them valid. This should happen before network ready has been sent. 
            player.AddToList();
            
            if (_masterID == -1)
            {
                this.Log($"Player {playerId} is now master");
                _masterID = playerId;
                
                _eventDispatcher?.SendEvent(new ClientSimOnNewMasterEvent
                {
                    oldMasterPlayer = null, 
                    newMasterPlayer = player
                });
            }

#if VRC_ENABLE_PLAYER_PERSISTENCE 
            clientSimPlayer.SetupPlayerPersistence(
                ClientSimMain.GetInstance().GetEventDispatcher(),
                ClientSimMain.GetInstance().GetUdonEventSender(), 
                ClientSimMain.GetInstance().GetBlacklistManager(),
                ClientSimMain.GetInstance().GetUdonManager(),
                ClientSimMain.GetInstance().GetSyncedObjectManager(),
                ClientSimMain.GetInstance().GetPlayerManager()
            );
#endif
            
            if (_networkReady)
            {
                DispatchPlayerJoinedEvent(player);
            }
            else
            {
                _waitingPlayers.Add(player);
            }
        }

        public void OnClientSimReady()
        {
            _networkReady = true;
            foreach (var player in _waitingPlayers)
            {
                DispatchPlayerJoinedEvent(player);
            }

            _waitingPlayers.Clear();
        }

        private void DispatchPlayerJoinedEvent(VRCPlayerApi player)
        {
            _eventDispatcher?.SendEvent(new ClientSimOnPlayerJoinedEvent { player = player });
        }

        public VRCPlayerApi CreateNewPlayer(bool local, ClientSimPlayer player, string name = "")
        {
            int playerID = GetNextPlayerId();
            string objectName = $"[{playerID}] {(local ? "Local" : "Remote")} Player";
            player.gameObject.name = objectName;
            VRCPlayerApi playerApi = new VRCPlayerApi
            {
                gameObject = player.gameObject,
                displayName = string.IsNullOrEmpty(name) ? objectName : name,
                isLocal = local
            };

            player.SetPlayer(playerApi);
            
            InitializePlayer(player, playerApi, playerID);

            if (local)
            {
                _localPlayerID = playerID;
                _localPlayer = playerApi;
            }
            Debug.Assert(playerApi.isLocal == local, "ClientSimPlayerManager:CreateNewPlayer New player does not match local settings!");

            return playerApi;
        }

        public void RemovePlayer(VRCPlayerApi player)
        {
            // Master is leaving, pick a new master.
            if (_masterID == player.playerId)
            {
                _masterID = -1;
                VRCPlayerApi newMaster = null;
                Debug.Assert(VRCPlayerApi.AllPlayers[0] == player, "ClientSimPlayerManager:RemovePlayer Removing master player who was not first in the list!");
                // First player is the current master who is leaving.
                // If there is another player, select them as the new master.
                if (VRCPlayerApi.AllPlayers.Count > 1)
                {
                    newMaster = VRCPlayerApi.AllPlayers[1];
                    _masterID = newMaster.playerId;
                }

                _eventDispatcher?.SendEvent(new ClientSimOnNewMasterEvent
                {
                    oldMasterPlayer = player, 
                    newMasterPlayer = newMaster
                });
            }
        
            _eventDispatcher?.SendEvent(new ClientSimOnPlayerLeftEvent { player = player });
            
            _playerIDs.Remove(player);
            _players.Remove(player.playerId);
            player.RemoveFromList();

            if (player.isLocal)
            {
                _localPlayer = null;
                _localPlayerID = -1;
            }
            
            Object.Destroy(player.gameObject);
        }
        
        public int GetMasterID()
        {
            return _masterID;
        }
        
        public VRCPlayerApi GetMaster()
        {
            return GetPlayerByID(_masterID);
        }

        public VRCPlayerApi GetInstanceOwner()
        {
            foreach (var player in _players.Values)
            {
                if (player.GetClientSimPlayer().isInstanceOwner)
                {
                    return player;
                }
            }
            return null;
        }

        public VRCPlayerApi LocalPlayer()
        {
            return _localPlayer;
        }

        public VRCPlayerApi GetPlayerByID(int playerID)
        {
            _players.TryGetValue(playerID, out VRCPlayerApi player);
            return player;
        }

        public int GetPlayerID(VRCPlayerApi player)
        {
            if (player == null)
            {
                return -1;
            }
            
            _playerIDs.TryGetValue(player, out int playerId);
            return playerId;
        }

        public bool IsMaster(VRCPlayerApi player)
        {
            return GetPlayerID(player) == _masterID;
        }

        public bool IsInstanceOwner(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().isInstanceOwner;
        }
        
        public bool IsInstanceOwner()
        {
            return IsInstanceOwner(_localPlayer);
        }
        
        public bool IsLocalPlayerMaster()
        {
            return _localPlayerID == _masterID;
        }
        
        public bool IsSuspended(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().isSuspended;
        }
        
        public VRCPlayerApi GetOwner(GameObject obj)
        {
            // TODO consider SyncMode.None
            IClientSimSyncable sync = obj.GetComponent<IClientSimSyncable>();

            int playerID = sync != null ? sync.GetOwner() : _masterID;
            
            if (!_players.TryGetValue(playerID, out VRCPlayerApi player))
            {
                return null;
            }
            return player;
        }

        public bool IsOwner(VRCPlayerApi player, GameObject obj)
        {
            IClientSimSyncable sync = obj.GetComponent<IClientSimSyncable>();
            int owner = sync == null ? _masterID : sync.GetOwner();
            return owner == player.playerId;
        }

        public static void SetOwner(VRCPlayerApi player, GameObject obj)
        {
            if (Networking.GetOwner(obj) == player)
            {
                return;
            }

            IClientSimSyncable[] syncs = obj.GetComponents<IClientSimSyncable>();
            foreach (IClientSimSyncable sync in syncs)
            {
                sync.SetOwner(player.playerId);
            }

            IClientSimSyncableHandler[] syncHandlers = obj.GetComponents<IClientSimSyncableHandler>();
            foreach (IClientSimSyncableHandler syncHandler in syncHandlers)
            {
                syncHandler.OnOwnershipTransferred(player.playerId);
            }
        }

        public static bool IsUserInVR(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().IsUserVR; 
        }
        
        public static void EnablePickups(VRCPlayerApi player, bool enabled)
        {
            if (!player.isLocal)
            {
                player.LogWarning($"[VRCPlayerAPI.EnablePickups] EnablePickups for remote players will do nothing. PlayerId: {player.playerId}");
                return;
            }
            
            player.GetClientSimPlayer().pickupData.SetPickupsEnabled(enabled);
        }

        public static void Immobilize(VRCPlayerApi player, bool immobilized)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.Immobilize] You cannot set remote players Immobilized");
            }
            
            player.GetClientSimPlayer().locomotionData.SetImmobilized(immobilized);
        }

        public static void TeleportToOrientationLerp(VRCPlayerApi player, Vector3 position, Quaternion rotation, VRC_SceneDescriptor.SpawnOrientation orientation, bool lerp)
        {
            if (!player.isLocal)
            {
                player.LogWarning($"[VRCPlayerAPI.TeleportTo] Teleporting remote players will do nothing. PlayerId: {player.playerId}");
                return;
            }
            
            // Ignore lerp since there is no networking here
            player.GetPlayerController().Teleport(position, rotation, orientation == VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint);
        }

        public static void TeleportToOrientation(VRCPlayerApi player, Vector3 position, Quaternion rotation, VRC_SceneDescriptor.SpawnOrientation orientation)
        {
            TeleportToOrientationLerp(player, position, rotation, orientation, false);
        }

        public static void TeleportTo(VRCPlayerApi player, Vector3 position, Quaternion rotation)
        {
            TeleportToOrientation(player, position, rotation, VRC_SceneDescriptor.SpawnOrientation.Default);
        }
        
        public static void Respawn(VRCPlayerApi playerApi)
        {
            if (!playerApi.isLocal)
            {
                playerApi.LogWarning($"[VRCPlayerApi.Respawn] Respawn for remote players will do nothing.");
                return;
            }
            
            playerApi.GetPlayerController().Respawn();
        }

        public static void RespawnWithIndex(VRCPlayerApi playerApi, int index)
        {
            if (!playerApi.isLocal)
            {
                playerApi.LogWarning($"[VRCPlayerApi.Respawn] Respawn for remote players will do nothing.");
                return;
            }
            
            playerApi.GetPlayerController().Respawn(index);
        }

        public static void PlayHapticEventInHand(VRCPlayerApi player, VRC_Pickup.PickupHand hand, float duration, float amplitude, float frequency)
        {
            if (!player.isLocal)
            {
                player.LogWarning($"[VRCPlayerAPI.PlayHapticEventInHand] PlayHapticEventInHand for remote players will do nothing. PlayerId: {player.playerId}");
                return;
            }
            
            // TODO
            player.Log($"[VRCPlayerAPI.PlayHapticEventInHand] Playing haptics for player. PlayerId: {player.playerId}, Hand: {hand}, Duration: {duration}, Amplitude: {amplitude}, Frequency: {frequency}");
        }

        public static VRCPlayerApi GetPlayerByGameObject(GameObject obj)
        {
            ClientSimPlayer player = obj.GetComponentInParent<ClientSimPlayer>();
            if (player != null)
            {
                return player.Player;
            }
            return null;
        }

        public static VRC_Pickup GetPickupInHand(VRCPlayerApi player, VRC_Pickup.PickupHand hand)
        {
            return player.GetClientSimPlayer().pickupData.GetPickupInHand(hand);
        }

        public static VRCPlayerApi.TrackingData GetTrackingData(VRCPlayerApi player, VRCPlayerApi.TrackingDataType trackingDataType)
        {
            // Remote players do not have tracking data, so get respective bone
            if (!player.isLocal)
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                 
                switch (trackingDataType)
                {
                    case VRCPlayerApi.TrackingDataType.Head:
                        position = GetBonePosition(player, HumanBodyBones.Head);
                        rotation = GetBoneRotation(player, HumanBodyBones.Head);
                        break;
                    case VRCPlayerApi.TrackingDataType.LeftHand:
                        position = GetBonePosition(player, HumanBodyBones.LeftHand);
                        rotation = GetBoneRotation(player, HumanBodyBones.LeftHand);
                        break;
                    case VRCPlayerApi.TrackingDataType.RightHand:
                        position = GetBonePosition(player, HumanBodyBones.RightHand);
                        rotation = GetBoneRotation(player, HumanBodyBones.RightHand);
                        break;
                    case VRCPlayerApi.TrackingDataType.Origin:
                        position = GetPosition(player);
                        rotation = GetRotation(player);
                        break;
                }
                return new VRCPlayerApi.TrackingData(position, rotation);
            }
            
            return player.GetClientSimPlayer().GetTrackingProvider().GetTrackingData(trackingDataType);
        }

        #region Player Locomotion
        
        public static float GetRunSpeed(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.GetRunSpeed] You cannot get run speed for remote clients!");
            }
            return player.GetClientSimPlayer().locomotionData.GetRunSpeed();
        }

        public static void SetRunSpeed(VRCPlayerApi player, float speed)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.SetRunSpeed] You cannot set run speed for remote clients!");
            }
            player.GetClientSimPlayer().locomotionData.SetRunSpeed(speed);
        }

        public static float GetStrafeSpeed(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.GetStrafeSpeed] You cannot get strafe speed for remote clients!");
            }
            return player.GetClientSimPlayer().locomotionData.GetStrafeSpeed();
        }

        public static void SetStrafeSpeed(VRCPlayerApi player, float speed)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.SetStrafeSpeed] You cannot set strafe speed for remote clients!");
            }
            player.GetClientSimPlayer().locomotionData.SetStrafeSpeed(speed);
        }

        public static float GetWalkSpeed(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.GetWalkSpeed] You cannot get walk speed for remote clients!");
            }
            return player.GetClientSimPlayer().locomotionData.GetWalkSpeed();
        }

        public static void SetWalkSpeed(VRCPlayerApi player, float speed)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.SetWalkSpeed] You cannot set walk speed for remote clients!");
            }
            player.GetClientSimPlayer().locomotionData.SetWalkSpeed(speed);
        }

        public static float GetJumpImpulse(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.GetJumpImpulse] You cannot get jump impulse for remote clients!");
            }
            return player.GetClientSimPlayer().locomotionData.GetJump();
        }

        public static void SetJumpImpulse(VRCPlayerApi player, float jump)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.SetJumpImpulse] You cannot set jump impulse for remote clients!");
            }
            player.GetClientSimPlayer().locomotionData.SetJump(jump);
        }

        public static float GetGravityStrength(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.GetGravityStrength] You cannot get gravity strength for remote clients!");
            }
            return player.GetClientSimPlayer().locomotionData.GetGravityStrength();
        }
        
        public static void SetGravityStrength(VRCPlayerApi player, float gravity)
        {
            if (!player.isLocal)
            {
                // VRChatBug: Throw an exception to crash the udon program similar to VRChat Client.
                throw new ClientSimException("[VRCPlayerAPI.SetGravityStrength] You cannot set gravity strength for remote clients!");
            }
            player.GetClientSimPlayer().locomotionData.SetGravityStrength(gravity);
        }
        
        public static void UseLegacyLocomotion(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                return;
            }
            player.GetClientSimPlayer().locomotionData.SetUseLegacyLocomotion(true);
        }

        public static Vector3 GetVelocity(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                return Vector3.zero;
            }
            return player.GetPlayerController().GetVelocity();
        }

        public static void SetVelocity(VRCPlayerApi player, Vector3 velocity)
        {
            if (!player.isLocal)
            {
                return;
            }
            player.GetPlayerController().SetVelocity(velocity);
        }
        
        public static Vector3 GetPosition(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().GetPosition();
        }

        public static Quaternion GetRotation(VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().GetRotation();
        }

        public static bool IsGrounded(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                // TODO verify remote player values when not grounded.
                return false;
            }
            return player.GetPlayerController().IsGrounded();
        }
        

        #endregion

        public static Quaternion GetBoneRotation(VRCPlayerApi player, HumanBodyBones bone)
        {
            return player.GetClientSimPlayer().GetAvatarDataProvider().GetBoneRotation(bone);
        }

        public static Vector3 GetBonePosition(VRCPlayerApi player, HumanBodyBones bone)
        {
            return player.GetClientSimPlayer().GetAvatarDataProvider().GetBonePosition(bone);
        }

        #region Player Tags

        public static List<int> GetPlayersWithTag(string tagName, string tagValue)
        {
            List<int> players = new List<int>();
            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player.GetClientSimPlayer().tagData.HasPlayerTag(tagName, tagValue))
                {
                    players.Add(player.playerId);
                }
            }
            return players;
        }

        public static void ClearPlayerTags(VRCPlayerApi player)
        {
            player.LogError("Clearing all player tags. VRCPlayerApi.ClearPlayerTags is a dangerous call, as it will clear all the tags and this might break prefabs that rely on them.");
            player.GetClientSimPlayer().tagData.ClearPlayerTags();
        }

        public static void SetPlayerTag(VRCPlayerApi player, string tagName, string tagValue)
        {
            player.GetClientSimPlayer().tagData.SetPlayerTag(tagName, tagValue);
        }

        public static string GetPlayerTag(VRCPlayerApi player, string tagName)
        {
            return player.GetClientSimPlayer().tagData.GetPlayerTag(tagName);
        }


        public static void ClearSilence(VRCPlayerApi player)
        {
            // TODO?
        }

        public static void SetSilencedToUntagged(VRCPlayerApi player, int number, string tagName, string tagValue)
        {
            // TODO?
        }

        public static void SetSilencedToTagged(VRCPlayerApi player, int number, string tagName, string tagValue)
        {
            // TODO?
        }

        #endregion

        #region Player Audio

        public static void SetAvatarAudioVolumetricRadius(VRCPlayerApi player, float value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioVolumetricRadius(value);
        }

        public static void SetAvatarAudioNearRadius(VRCPlayerApi player, float value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioNearRadius(value);
        }

        public static void SetAvatarAudioFarRadius(VRCPlayerApi player, float value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioFarRadius(value);
        }

        public static void SetAvatarAudioGain(VRCPlayerApi player, float value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioGain(value);
        }
        
        public static void SetAvatarAudioForceSpatial(VRCPlayerApi player, bool value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioForceSpatial(value);
        }

        public static void SetAvatarAudioCustomCurve(VRCPlayerApi player, bool value)
        {
            player.GetClientSimPlayer().audioData.SetAvatarAudioCustomCurve(value);
        }

        public static void SetVoiceGain(VRCPlayerApi player, float value) =>
            player.GetClientSimPlayer().audioData.SetVoiceGain(value);

        public static void SetVoiceDistanceNear(VRCPlayerApi player, float value) =>
            player.GetClientSimPlayer().audioData.SetVoiceDistanceNear(value);

        public static void SetVoiceDistanceFar(VRCPlayerApi player, float value) =>
            player.GetClientSimPlayer().audioData.SetVoiceDistanceFar(value);

        public static void SetVoiceVolumetricRadius(VRCPlayerApi player, float value) =>
            player.GetClientSimPlayer().audioData.SetVoiceVolumetricRadius(value);

        public static void SetVoiceLowpass(VRCPlayerApi player, bool value) =>
            player.GetClientSimPlayer().audioData.SetVoiceLowpass(value);

        public static float GetVoiceGain(VRCPlayerApi player) =>
            player.GetClientSimPlayer().audioData.GetVoiceGain();

        public static float GetVoiceDistanceNear(VRCPlayerApi player) => 
            player.GetClientSimPlayer().audioData.GetVoiceDistanceNear();

        public static float GetVoiceDistanceFar(VRCPlayerApi player) =>
            player.GetClientSimPlayer().audioData.GetVoiceDistanceFar();

        public static float GetVoiceVolumetricRadius(VRCPlayerApi player) =>
            player.GetClientSimPlayer().audioData.GetVoiceVolumetricRadius();

        public static bool GetVoiceLowpass(VRCPlayerApi player) =>
            player.GetClientSimPlayer().audioData.GetVoiceLowpass();
        
        public static string GetCurrentLanguage()
        {
            return ClientSimSettings.Instance.currentLanguage;
        }
        
        public static string[] GetAvailableLanguages()
        {
            return ClientSimSettings.Instance.availableLanguages;
        }

        #endregion
        
        #region Player Scaling
        
        public static bool GetManualAvatarScalingAllowed(VRCPlayerApi _) => _heightManager.GetManualAvatarScalingAllowed();
        public static void SetManualAvatarScalingAllowed(VRCPlayerApi _, bool value) => _heightManager.SetManualAvatarScalingAllowed(value);
        public static float GetAvatarEyeHeightMinimumAsMeters(VRCPlayerApi _) => _heightManager.GetAvatarEyeHeightMinimumAsMeters();
        public static float GetAvatarEyeHeightMaximumAsMeters(VRCPlayerApi _) => _heightManager.GetAvatarEyeHeightMaximumAsMeters();
        public static float GetAvatarEyeHeightAsMeters(VRCPlayerApi _) => _heightManager.GetAvatarEyeHeightAsMeters();
        public static void SetAvatarEyeHeightMinimumByMeters(VRCPlayerApi _, float value) => _heightManager.SetAvatarEyeHeightMinimumByMeters(value);
        public static void SetAvatarEyeHeightMaximumByMeters(VRCPlayerApi _, float value) => _heightManager.SetAvatarEyeHeightMaximumByMeters(value);
        public static void SetAvatarEyeHeightByMeters(VRCPlayerApi _, float value) => _heightManager.SetAvatarEyeHeightByMeters(value);
        public static void SetAvatarEyeHeightByMultiplier(VRCPlayerApi _, float value) => _heightManager.SetAvatarEyeHeightByMultiplier(value);
        
        #endregion

    }
}
 