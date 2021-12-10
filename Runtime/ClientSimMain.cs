using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using Random = UnityEngine.Random;


namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimMain : ClientSimBehaviour
    {
        private const string CLIENT_SIM_GAMEOBJECT_NAME_ = "__ClientSim";
        private const string EDITOR_ONLY_TAG_ = "EditorOnly";

        private static ClientSimMain instance_;

        private IClientSimSDKManager sdkManager_;

        private ClientSimSettings settings_;
        private ClientSimPlayerController playerController_;
        private VRC_SceneDescriptor descriptor_;
        private Transform proxyObjectParents_;

        private bool shouldVerifySyncedObjectList_;
        private readonly Queue<ClientSimSyncedObjectHelper> toBeAddedSync_ = new Queue<ClientSimSyncedObjectHelper>();
        private readonly Queue<ClientSimSyncedObjectHelper> toBeRemovedSync_ = new Queue<ClientSimSyncedObjectHelper>();
        private HashSet<ClientSimSyncedObjectHelper> allSyncedObjects_ = new HashSet<ClientSimSyncedObjectHelper>();

        private bool networkReady_;
        private int spawnOrder_ = 0;

        // Dummy method to get the static initializer to be called early on.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            if (!ClientSimEnabled())
            {
                return;
            }

            DestroyEditorOnly();
        }

        static ClientSimMain()
        {
            if (!ClientSimEnabled())
            {
                return;
            }

            LinkAPI();
            CreateInstance();
        }

        private static bool ClientSimEnabled()
        {
            return 
                ClientSimSettings.Instance.enableClientSim &&
                FindObjectOfType<PipelineSaver>() == null && 
                Application.isPlaying;
        }

        private static void LinkAPI()
        {
            VRCStation.Initialize += ClientSimStationHelper.InitializeStations;
            VRCStation.useStationDelegate = ClientSimStationHelper.UseStation;
            VRCStation.exitStationDelegate = ClientSimStationHelper.ExitStation;

            VRC_UiShape.GetEventCamera = ClientSimPlayerController.GetPlayerCamera;
            VRC_Pickup.OnAwake = ClientSimPickupHelper.InitializePickup;
            VRC_Pickup.ForceDrop = ClientSimPickupHelper.ForceDrop;
            VRC_Pickup._GetCurrentPlayer = ClientSimPickupHelper.GetCurrentPlayer;
            VRC_Pickup._GetPickupHand = ClientSimPickupHelper.GetPickupHand;

            VRC.Udon.UdonBehaviour.OnInit = ClientSimUdonHelper.OnInit;
            
            VRC.SDK3.Components.VRCObjectPool.OnInit = ClientSimObjectPoolHelper.OnInit;
            VRC.SDK3.Components.VRCObjectPool.OnReturn = ClientSimObjectPoolHelper.OnReturn;
            VRC.SDK3.Components.VRCObjectPool.OnSpawn = ClientSimObjectPoolHelper.OnSpawn;
            
            VRC.SDK3.Components.VRCObjectSync.FlagDiscontinuityHook = ClientSimObjectSyncHelper.FlagDiscontinuityHook;
            VRC.SDK3.Components.VRCObjectSync.OnAwake = ClientSimObjectSyncHelper.InitializeObjectSync;
            VRC.SDK3.Components.VRCObjectSync.RespawnHandler = ClientSimObjectSyncHelper.RespawnObject;
            VRC.SDK3.Components.VRCObjectSync.TeleportHandler = ClientSimObjectSyncHelper.TeleportTo;
            VRC.SDK3.Components.VRCObjectSync.SetGravityHook = ClientSimObjectSyncHelper.SetUseGravity;
            VRC.SDK3.Components.VRCObjectSync.SetKinematicHook = ClientSimObjectSyncHelper.SetIsKinematic;

            Networking._IsMaster = ClientSimPlayerManager.IsLocalPlayerMaster;
            Networking._LocalPlayer = ClientSimPlayerManager.LocalPlayer;
            Networking._GetOwner = ClientSimPlayerManager.GetOwner;
            Networking._IsOwner = ClientSimPlayerManager.IsOwner;
            Networking._SetOwner = ClientSimPlayerManager.TakeOwnership;
            Networking._GetUniqueName = VRC.Tools.GetGameObjectPath;
            Networking._IsInstanceOwner = ClientSimPlayerManager.IsInstanceOwner;
            Networking._IsObjectReady = obj => IsNetworkReady();
            Networking._IsNetworkSettled = IsNetworkReady;

            VRCPlayerApi._GetPlayerId = ClientSimPlayerManager.GetPlayerID;
            VRCPlayerApi._GetPlayerById = ClientSimPlayerManager.GetPlayerByID;
            VRCPlayerApi._isMasterDelegate = ClientSimPlayerManager.IsMaster;
            VRCPlayerApi._EnablePickups = ClientSimPlayerManager.EnablePickups;
            VRCPlayerApi._Immobilize = ClientSimPlayerManager.Immobilize;
            VRCPlayerApi._TeleportTo = ClientSimPlayerManager.TeleportTo;
            VRCPlayerApi._TeleportToOrientation = ClientSimPlayerManager.TeleportToOrientation;
            VRCPlayerApi._TeleportToOrientationLerp = ClientSimPlayerManager.TeleportToOrientationLerp;
            VRCPlayerApi._PlayHapticEventInHand = ClientSimPlayerManager.PlayHapticEventInHand;
            VRCPlayerApi._GetPlayerByGameObject = ClientSimPlayerManager.GetPlayerByGameObject;
            VRCPlayerApi._GetPickupInHand = ClientSimPlayerManager.GetPickupInHand;
            VRCPlayerApi._GetTrackingData = ClientSimPlayerManager.GetTrackingData;
            VRCPlayerApi._GetBonePosition = ClientSimPlayerManager.GetBonePosition;
            VRCPlayerApi._GetBoneRotation = ClientSimPlayerManager.GetBoneRotation;
            VRCPlayerApi._TakeOwnership = ClientSimPlayerManager.TakeOwnership;
            VRCPlayerApi._IsOwner = ClientSimPlayerManager.IsOwner;
            VRCPlayerApi._isInstanceOwnerDelegate = ClientSimPlayerManager.IsInstanceOwner;

            VRCPlayerApi._ClearPlayerTags = ClientSimPlayerManager.ClearPlayerTag;
            VRCPlayerApi._SetPlayerTag = ClientSimPlayerManager.SetPlayerTag;
            VRCPlayerApi._GetPlayerTag = ClientSimPlayerManager.GetPlayerTag;
            VRCPlayerApi._GetPlayersWithTag = ClientSimPlayerManager.GetPlayersWithTag;
            VRCPlayerApi._SetSilencedToTagged = ClientSimPlayerManager.SetSilencedToTagged;
            VRCPlayerApi._SetSilencedToUntagged = ClientSimPlayerManager.SetSilencedToUntagged;
            VRCPlayerApi._ClearSilence = ClientSimPlayerManager.ClearSilence;

            VRCPlayerApi._IsUserInVR = (VRCPlayerApi _) => false; // TODO one day...
            VRCPlayerApi._GetRunSpeed = ClientSimPlayerManager.GetRunSpeed;
            VRCPlayerApi._SetRunSpeed = ClientSimPlayerManager.SetRunSpeed;
            VRCPlayerApi._GetWalkSpeed = ClientSimPlayerManager.GetWalkSpeed;
            VRCPlayerApi._SetWalkSpeed = ClientSimPlayerManager.SetWalkSpeed;
            VRCPlayerApi._GetJumpImpulse = ClientSimPlayerManager.GetJumpImpulse;
            VRCPlayerApi._SetJumpImpulse = ClientSimPlayerManager.SetJumpImpulse;
            VRCPlayerApi._GetStrafeSpeed = ClientSimPlayerManager.GetStrafeSpeed;
            VRCPlayerApi._SetStrafeSpeed = ClientSimPlayerManager.SetStrafeSpeed;
            VRCPlayerApi._GetVelocity = ClientSimPlayerManager.GetVelocity;
            VRCPlayerApi._SetVelocity = ClientSimPlayerManager.SetVelocity;
            VRCPlayerApi._GetPosition = ClientSimPlayerManager.GetPosition;
            VRCPlayerApi._GetRotation = ClientSimPlayerManager.GetRotation;
            VRCPlayerApi._GetGravityStrength = ClientSimPlayerManager.GetGravityStrength;
            VRCPlayerApi._SetGravityStrength = ClientSimPlayerManager.SetGravityStrength;
            VRCPlayerApi.IsGrounded = ClientSimPlayerManager.IsGrounded;
            VRCPlayerApi._UseAttachedStation = ClientSimPlayerManager.UseAttachedStation;
            VRCPlayerApi._UseLegacyLocomotion = ClientSimPlayerManager.UseLegacyLocomotion;

            VRCPlayerApi._CombatSetup = ClientSimCombatSystemHelper.CombatSetup;
            VRCPlayerApi._CombatSetMaxHitpoints = ClientSimCombatSystemHelper.CombatSetMaxHitpoints;
            VRCPlayerApi._CombatGetCurrentHitpoints = ClientSimCombatSystemHelper.CombatGetCurrentHitpoints;
            VRCPlayerApi._CombatSetRespawn = ClientSimCombatSystemHelper.CombatSetRespawn;
            VRCPlayerApi._CombatSetDamageGraphic = ClientSimCombatSystemHelper.CombatSetDamageGraphic;
            VRCPlayerApi._CombatGetDestructible = ClientSimCombatSystemHelper.CombatGetDestructible;
            VRCPlayerApi._CombatSetCurrentHitpoints = ClientSimCombatSystemHelper.CombatSetCurrentHitpoints;
            
            VRCPlayerApi._SetAvatarAudioVolumetricRadius = ClientSimPlayerManager.SetAvatarAudioVolumetricRadius;
            VRCPlayerApi._SetAvatarAudioNearRadius = ClientSimPlayerManager.SetAvatarAudioNearRadius;
            VRCPlayerApi._SetAvatarAudioFarRadius = ClientSimPlayerManager.SetAvatarAudioFarRadius;
            VRCPlayerApi._SetAvatarAudioGain = ClientSimPlayerManager.SetAvatarAudioGain;
            VRCPlayerApi._SetAvatarAudioForceSpatial = ClientSimPlayerManager.SetAvatarAudioForceSpatial;
            VRCPlayerApi._SetAvatarAudioCustomCurve = ClientSimPlayerManager.SetAvatarAudioCustomCurve;
            
            VRCPlayerApi._SetVoiceLowpass = ClientSimPlayerManager.SetVoiceLowpass;
            VRCPlayerApi._SetVoiceVolumetricRadius = ClientSimPlayerManager.SetVoiceVolumetricRadius;
            VRCPlayerApi._SetVoiceDistanceFar = ClientSimPlayerManager.SetVoiceDistanceFar;
            VRCPlayerApi._SetVoiceDistanceNear = ClientSimPlayerManager.SetVoiceDistanceNear;
            VRCPlayerApi._SetVoiceGain = ClientSimPlayerManager.SetVoiceGain;
            
            VRC_SpatialAudioSource.Initialize = ClientSimSpatialAudioHelper.InitializeAudio;

            VRCAVProVideoPlayer.Initialize = player => new ClientSimAVProVideoStub(player);
        }

        private static void CreateInstance()
        {
            GameObject executor = new GameObject(CLIENT_SIM_GAMEOBJECT_NAME_);
            executor.tag = EDITOR_ONLY_TAG_;
            instance_ = executor.AddComponent<ClientSimMain>();
        }

        private static void DestroyEditorOnly()
        {
            if (!ClientSimSettings.Instance.deleteEditorOnly)
            {
                return;
            }

            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            Queue<GameObject> queue = new Queue<GameObject>(rootObjects);
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj.tag == EDITOR_ONLY_TAG_)
                {
                    obj.Log("Deleting editor only object: " + VRC.Tools.GetGameObjectPath(obj));
                    DestroyImmediate(obj);
                }
                else
                {
                    for (int child = 0; child < obj.transform.childCount; ++child)
                    {
                        queue.Enqueue(obj.transform.GetChild(child).gameObject);
                    }
                }
            }
        }

        public static bool HasInstance()
        {
            return instance_ != null;
        }

        public static bool IsNetworkReady()
        {
            return instance_.networkReady_;
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (instance_ != null)
            {
                this.LogError("Already have an instance of ClientSim!");
                DestroyImmediate(gameObject);
                return;
            }
            
            settings_ = ClientSimSettings.Instance;

            instance_ = this;
            DontDestroyOnLoad(this);

            proxyObjectParents_ = new GameObject(CLIENT_SIM_GAMEOBJECT_NAME_ + "ProxyObjects").transform;
            DontDestroyOnLoad(proxyObjectParents_);

            ClientSimInputModule.DisableOtherInputModules();
            gameObject.AddComponent<ClientSimBaseInput>();
            gameObject.AddComponent<ClientSimInputModule>();

            descriptor_ = FindObjectOfType<VRC_SceneDescriptor>();
            if (descriptor_ == null)
            {
                Debug.LogWarning("There is no VRC_SceneDescriptor in the scene.");
            }

            sdkManager_ = gameObject.AddComponent<ClientSimUdonManager>();

            StartCoroutine(OnNetworkReady());
        }

        private IEnumerator OnNetworkReady()
        {
            // VRChatBug: VRChat does not initialize SDK components immediately. This delay is to provide Unity
            // components time to work before Udon starts to simulate how it is in client.
            yield return new WaitForSeconds(0.5f);

            this.Log("Sending OnNetworkReady");
            sdkManager_.OnNetworkReady();

            if (settings_.spawnPlayer)
            {
                // TODO add option to allow for spawning remote players first to have data on not being master
                SpawnLocalPlayer();
            }
            networkReady_ = true;
            
            yield return new WaitForSeconds(0.1f);
            ClientSimPlayerManager.OnNetworkReady();
        }

        public static Transform GetProxyObjectTransform()
        {
            return instance_.proxyObjectParents_;
        }


        public static void SpawnPlayer(bool local, string name = null)
        {
            if (local)
            {
                instance_?.SpawnLocalPlayer();
            }
            else
            {
                instance_?.SpawnRemotePlayer(name);
            }
        }

        private void SpawnLocalPlayer()
        {
            if (descriptor_ == null)
            {
                Debug.LogError("Cannot spawn player if there is no world descriptor!");
                return;
            }

            GameObject player = new GameObject("Local Player");
            player.transform.parent = transform;

            // Force move the player initially to the spawn point to prevent enter triggers at the origin
            Transform spawn = GetSpawnPoint();
            player.transform.position = spawn.position;
            player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0); 

            playerController_ = player.AddComponent<ClientSimPlayerController>();
            playerController_.Teleport(spawn, false);

            ClientSimPlayer playerObj = player.AddComponent<ClientSimPlayer>();
            // TODO initialize players better as this will send player join event before the object has finished setting up.
            VRCPlayerApi playerAPI = ClientSimPlayerManager.CreateNewPlayer(true, player, settings_.customLocalPlayerName);
            playerObj.SetPlayer(playerAPI);
            player.name = $"[{playerAPI.playerId}] {player.name}";
        }

        private void SpawnRemotePlayer(string name = null)
        {
            if (descriptor_ == null)
            {
                Debug.LogError("Cannot spawn player if there is no world descriptor!");
                return;
            }

            GameObject player = new GameObject("Remote Player");
            player.transform.parent = transform;
            player.layer = LayerMask.NameToLayer("Player");
            // TODO do this better
            Transform spawn = GetSpawnPoint(true);
            player.transform.position = spawn.position;
            player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);

            GameObject playerVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerVis.layer = player.layer;
            playerVis.transform.SetParent(player.transform, false);

            ClientSimPlayer playerObj = player.AddComponent<ClientSimPlayer>();
            // TODO initialize players better as this will send player join event before the object has finished setting up.
            VRCPlayerApi playerAPI = ClientSimPlayerManager.CreateNewPlayer(false, player, name);
            playerObj.SetPlayer(playerAPI);
            player.name = $"[{playerAPI.playerId}] {player.name}";

            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        public static Transform GetNextSpawnPoint()
        {
            if (instance_ != null)
            {
                return instance_.GetSpawnPoint();
            }
            return null;
        }
        
        private Transform GetSpawnPoint(bool remote = false)
        {
            if (descriptor_.spawns.Length == 0 || descriptor_.spawns[0] == null)
            {
                throw new Exception("[ClientSimMain] Cannot spawn player when descriptor does not have a spawn set!");
            }

            // Remote players always restart the list, so for now, only first spawn
            if (descriptor_.spawnOrder == VRC_SceneDescriptor.SpawnOrder.First || 
                descriptor_.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Demo || 
                remote)
            {
                return descriptor_.spawns[0];
            }
            if (descriptor_.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Random)
            {
                int spawn = Random.Range(0, descriptor_.spawns.Length);
                return descriptor_.spawns[spawn];
            }
            if (descriptor_.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Sequential)
            {
                Transform spawn = descriptor_.spawns[spawnOrder_];
                spawnOrder_ = (spawnOrder_ + 1) % descriptor_.spawns.Length;
                return spawn;
            }
            
            // Fallback to first spawn point
            return descriptor_.spawns[0];
        }

        public static void RemovePlayer(VRCPlayerApi player)
        {
            ClientSimPlayerManager.RemovePlayer(player);
            Destroy(player.gameObject);
        }

        public static void PlayerJoined(VRCPlayerApi player)
        {
            instance_?.OnPlayerJoined(player);
        }

        private void OnPlayerJoined(VRCPlayerApi player)
        {
            sdkManager_.OnPlayerJoined(player);
        }

        public static void PlayerLeft(VRCPlayerApi player)
        {
            instance_?.OnPlayerLeft(player);
        }

        private void OnPlayerLeft(VRCPlayerApi player)
        {
            int masterID = ClientSimPlayerManager.GetMasterID();
            VRCPlayerApi masterPlayer = VRCPlayerApi.GetPlayerById(masterID);

            foreach (ClientSimSyncedObjectHelper sync in allSyncedObjects_)
            {
                if (sync == null)
                {
                    continue;
                }
                
                GameObject syncObj = sync.gameObject;
                if (Networking.GetOwner(syncObj)?.playerId == player.playerId)
                {
                    Networking.SetOwner(masterPlayer, syncObj);
                }
            }

            sdkManager_.OnPlayerLeft(player);
        }

        public static void PlayerRespawned(VRCPlayerApi player)
        {
            instance_?.OnPlayerRespawn(player);
        }

        private void OnPlayerRespawn(VRCPlayerApi player)
        {
            sdkManager_.OnPlayerRespawn(player);
        }

        private void LateUpdate()
        {
            if (descriptor_ == null)
            {
                return;
            }
            
            ProcessAddedAndRemovedSyncedObjects();
            ProcessSyncedObjectsBelowRespawn();
        }

        private void ProcessSyncedObjectsBelowRespawn()
        {
            if (playerController_ != null && playerController_.transform.position.y < descriptor_.RespawnHeightY)
            {
                playerController_.Respawn();
            }
            
            // TODO space this out so that there are only x number per frame instead of all every time? 
            List<GameObject> objsToDestroy = new List<GameObject>();
            foreach (ClientSimSyncedObjectHelper sync in allSyncedObjects_)
            {
                if (sync == null)
                {
                    shouldVerifySyncedObjectList_ = true;
                    Debug.LogWarning("Null Synced Object!");
                    continue;
                }
                
                if (!sync.SyncPosition)
                {
                    continue;
                }
                
                if (sync.transform.position.y < descriptor_.RespawnHeightY)
                {
                    if (descriptor_.ObjectBehaviourAtRespawnHeight == VRC_SceneDescriptor.RespawnHeightBehaviour.Respawn)
                    {
                        sync.Respawn();
                    }
                    else
                    {
                        objsToDestroy.Add(sync.gameObject);
                    }
                }
            }

            foreach (var obj in objsToDestroy)
            {
                Destroy(obj);
            }
        }
        
        public static void AddSyncedObject(ClientSimSyncedObjectHelper sync)
        {
            if (instance_ == null || sync == null)
            {
                return;
            }

            instance_.QueueAddSyncedObject(sync);
        }

        public static void RemoveSyncedObject(ClientSimSyncedObjectHelper sync)
        {
            if (instance_ == null)
            {
                return;
            }

            instance_.QueueRemoveSyncedObject(sync);
        }
        
        private void QueueAddSyncedObject(ClientSimSyncedObjectHelper syncedObject)
        {
            if (syncedObject == null)
            {
                return;
            }
            toBeAddedSync_.Enqueue(syncedObject);
        }
        
        private void QueueRemoveSyncedObject(ClientSimSyncedObjectHelper syncedObject)
        {
            shouldVerifySyncedObjectList_ = true;
            toBeRemovedSync_.Enqueue(syncedObject);
        }
        
        private void ProcessAddedAndRemovedSyncedObjects()
        {
            if (toBeAddedSync_.Count > 0)
            {
                foreach (var sync in toBeAddedSync_)
                {
                    if (sync == null)
                    {
                        shouldVerifySyncedObjectList_ = true;
                        continue;
                    }
                    allSyncedObjects_.Add(sync);
                }
                toBeAddedSync_.Clear();
            }
            if (toBeRemovedSync_.Count > 0)
            {
                foreach (var udon in toBeRemovedSync_)
                {
                    if (udon == null)
                    {
                        shouldVerifySyncedObjectList_ = true;
                        continue;
                    }
                    allSyncedObjects_.Remove(udon);
                }
                toBeRemovedSync_.Clear();
            }

            if (shouldVerifySyncedObjectList_)
            {
                HashSet<ClientSimSyncedObjectHelper> allSyncs = new HashSet<ClientSimSyncedObjectHelper>();
                foreach (var sync in allSyncedObjects_)
                {
                    if (sync == null)
                    {
                        continue;
                    }
                    allSyncs.Add(sync);
                }

                allSyncedObjects_ = allSyncs;
            }
        }
    }
}
