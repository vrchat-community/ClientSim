using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;
using VRC.Economy;
using VRC.SDK3.Network;
using VRC.SDK3.Platform;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.SDKBase.Platform;
using VRC.Udon;
using VRC.Economy;
using VRC.SDKBase.Network;
using VRC.Economy;
using VRCNetworkBehaviour = VRC.SDK3.Network.VRCNetworkBehaviour;
using System;
using VRC.SDK3.Components;
using VRCStation = VRC.SDKBase.VRCStation;
#if VRC_ENABLE_PLAYER_PERSISTENCE
using VRC.SDK3.ClientSim.Persistence;
using System.Linq;
#endif

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// The main system for ClientSim. This handles the spawning of the ClientSim system and initialization.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimReadyEvent
    /// </remarks>
    [AddComponentMenu("")]
    public class ClientSimMain : ClientSimBehaviour
    {
        private static readonly string _systemPrefabPath = Path.Combine("ClientSim", "Prefabs", "ClientSimSystem");

        private static ClientSimMain _instance;
        
        [SerializeField]
        private ClientSimMenu menu;
        [SerializeField]
        private ClientSimBaseInput baseInput;
        [SerializeField]
        private ClientSimInputModule inputModule;
        [SerializeField]
        private ClientSimSyncedObjectManager syncedObjectManager;
        [SerializeField]
        private ClientSimInputManager inputManager;
        [SerializeField]
        private ClientSimUdonInputBehaviour udonInput;
        [SerializeField]
        private ClientSimHighlightManager highlightManager;
        [SerializeField]
        private ClientSimTooltipManager tooltipManager;
        [SerializeField]
        private ClientSimPlayerSpawner playerSpawner;
        [SerializeField]
        private ClientSimStackedVRCameraSystem stackedCameraSystem;
        [SerializeField] 
        private ClientSimStoreManager storeManager;

        [SerializeField]
        private GameObject proxyObjectPrefab;

        private ClientSimProxyObjects _proxyObject;
        
        private IClientSimEventDispatcher _eventDispatcher;
        private ClientSimPlayerManager _playerManager;
        private ClientSimUdonManager _udonManager;
        private ClientSimSceneManager _sceneManager;
        private ClientSimBlacklistManager _blacklistManager;
        private ClientSimInteractiveLayerProvider _interactiveLayerProvider;
        private IClientSimSessionState _sessionState;
        private IClientSimUdonEventSender _udonEventSender;

        // public so that height can be adjusted via the settings window
        public ClientSimPlayerHeightManager HeightManager { get; private set; }
        
        private ClientSimSettings _settings;
        private ClientSimPlayer _player;

        private bool _isReady;

        public static void CreateInstance(
            ClientSimSettings settings,
            IClientSimEventDispatcher eventDispatcher = null)
        {
            if (HasInstance())
            {
                throw new ClientSimException("Cannot create an instance of ClientSim while one already exists.");
            }
            
            // Configure Delegates for Allowlisted URL Validation
            VRCUrl.DomainExplicitAllowlistDelegate = () => UrlAllowlistConfig.DomainExplicitAllowlist;
            VRCUrl.DomainWildcardAllowlistDelegate = () => UrlAllowlistConfig.DomainWildcardAllowlist;

            GameObject systemPrefab = Resources.Load<GameObject>(_systemPrefabPath);

            if (systemPrefab == null)
            {
                throw new ClientSimException("Failed to start Client Sim! Main system prefab was not found.");
            }

            GameObject systemInstance = Instantiate(systemPrefab);
            systemInstance.name = $"__{systemPrefab.name}";

            ClientSimMain main = systemInstance.GetComponent<ClientSimMain>();
            if (main == null)
            {
                throw new ClientSimException("Failed to start Client Sim! Main system component not found.");
            }

            try
            {
                main.Initialize(settings, eventDispatcher);
            }
            catch (ClientSimException e)
            {
#if UNITY_EDITOR
                // Tests expect certain exceptions, don't exit play mode
                if (ClientSimRuntimeLoader.IsInUnityTest())
                {
                    throw e;
                }
                else
                {
                    Debug.LogError($"Play mode Stopped because: {e.Message}");
                    UnityEditor.EditorApplication.isPlaying = false;
                }
#else
                Debug.LogError($"Tried to initialize ClientSim outside of the Unity Editor: {e.Message}");
#endif
            }
        }

        public static bool HasInstance()
        {
            return _instance;
        }

        internal static ClientSimMain GetInstance()
        {
            return _instance;
        }
        
        public static void SpawnRemotePlayer(string name = null)
        {
            if (HasInstance())
            {
                _instance.SpawnRemotePlayerAndInitialize(name);
            }
        }
        
        public static void RemovePlayer(VRCPlayerApi player)
        {
            if (HasInstance())
            {
                _instance._playerManager.RemovePlayer(player);
            }
        }

        // Used in tests
        public static void RemoveInstance()
        {
            if (HasInstance())
            {
                DestroyImmediate(_instance.gameObject);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (_instance)
            {
                DestroyImmediate(this);
                throw new ClientSimException("Multiple instances of ClientSim running!");
            }
            _instance = this;
            
            this.Log("Starting ClientSim");

            DontDestroyOnLoad(this);
            
            SpawnProxyObjects();
        }

        private void OnDestroy()
        {
            if (_proxyObject)
            {
                _proxyObject.DestroyProxy();    
            }

            _playerManager?.Dispose();

            if (_instance != this)
            {
                return;
            }

            RemoveSDKLinks();
            _instance = null;
        }

        private void Initialize(
            ClientSimSettings settings, 
            IClientSimEventDispatcher eventDispatcher)
        {
            _sceneManager = new ClientSimSceneManager();
            if (!_sceneManager.HasSceneDescriptor())
            {
                _instance = null;
                Destroy(gameObject);
                Debug.LogWarning("Cannot start ClientSim if there is no scene descriptor!");
                return;
            }
            
            ClientSimNetworkingUtilities.ConfigureNetworkOnScene(VRC_SceneDescriptor.Instance);

            _settings = settings;

            // Event Dispatcher is provided during tests to listen and send events before everything has been initialized.
            _eventDispatcher = eventDispatcher ?? new ClientSimEventDispatcher();
            
            _blacklistManager = new ClientSimBlacklistManager();
            _blacklistManager.AddObjectAndChildrenToBlackList(gameObject);

            _interactiveLayerProvider = new ClientSimInteractiveLayerProvider(_eventDispatcher);
            _sessionState = new ClientSimSessionState();
            
            inputManager.Initialize(_settings);
            IClientSimInput input = inputManager.GetInput();
            
            _udonEventSender = new ClientSimUdonManagerEventSender(UdonManager.Instance);
            HeightManager = new ClientSimPlayerHeightManager(_eventDispatcher, _udonEventSender);

            udonInput.Initialize(_eventDispatcher, input);
            menu.Initialize(_eventDispatcher, input, _settings, _sessionState, HeightManager);
            baseInput.Initialize(_eventDispatcher, input, _settings);
            inputModule.Initialize(_interactiveLayerProvider);
            
            _playerManager = new ClientSimPlayerManager(_eventDispatcher, HeightManager);
            // ObjectManager must be initialized before UdonManager to ensure object ownership for leaving players
            // is handled first.
            syncedObjectManager.Initialize(_eventDispatcher, _sceneManager, _playerManager);
            _udonManager = new ClientSimUdonManager(_eventDispatcher, syncedObjectManager, _udonEventSender);
            playerSpawner.Initialize(_sceneManager, _playerManager, _blacklistManager, _eventDispatcher, null);
            
            // Option to allow for spawning remote players first to prevent the local player from being master.
            // TODO replace this with networking test system to save previous player count in the instance.
            if (!_settings.localPlayerIsMaster)
            {
                SpawnRemotePlayerAndInitialize();
            }
            
            // Spawn player controller to ensure that Local Player specific methods initialize properly.
            // This included getting the Player's camera.
            SpawnLocalPlayer(_settings.customLocalPlayerName);
            
            _player.Initialize(
                _eventDispatcher,
                input,
                _settings,
                highlightManager,
                tooltipManager,
                _interactiveLayerProvider,
                baseInput,
                _sceneManager,
                _proxyObject,
                _udonEventSender,
                _blacklistManager,
                _udonManager,
                syncedObjectManager,
                _playerManager,
                HeightManager);

            Camera playerCamera = _player.GetCameraProvider().GetCamera();
            tooltipManager.Initialize(_settings, _player.GetTrackingProvider());
            highlightManager.Initialize(playerCamera);
            stackedCameraSystem.Initialize(playerCamera, menu);
            
            storeManager = new ClientSimStoreManager(_eventDispatcher, new ClientSimUdonManagerEventSender(UdonManager.Instance), _playerManager);

            // Initialize SDK links after everything has been created and initialized.
            SetupSDKLinks();
        }
        
        #region Platform Management

        private Vector2 CurrentResolution;
        private VRCOrientation CurrentOrientation;
        private const int ScreenChangePollRate = 100;
        
        private async UniTaskVoid CheckForScreenChange()
        {
            while (_isReady)
            {
                if ((int)CurrentResolution.x != UnityEngine.Device.Screen.width || (int)CurrentResolution.y != UnityEngine.Device.Screen.height)
                {
                    CurrentResolution = new Vector2(UnityEngine.Device.Screen.width, UnityEngine.Device.Screen.height);
                }
                
                var currentOrientation = DetermineOrientation();
                if (currentOrientation != CurrentOrientation)
                {
                    CurrentOrientation = DetermineOrientation();
                    // send ScreenUpdate event
                    var screenUpdateEvent = new ClientSimScreenUpdateEvent()
                    {
                        data = new ScreenUpdateData()
                        {
                            type = ScreenUpdateType.OrientationChanged,
                            orientation = CurrentOrientation,
                            resolution = CurrentResolution
                        }
                    };
                    _eventDispatcher.SendEvent(screenUpdateEvent);
                }

                await UniTask.Delay(ScreenChangePollRate);
            }
        }
        
        private VRCOrientation DetermineOrientation()
        {
            return Screen.width < Screen.height ? VRCOrientation.Portrait : VRCOrientation.Landscape;
        }
        #endregion

        private void Start()
        {
            if (!_sceneManager.HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot start ClientSim if there is no world descriptor!");
            }

            if (_settings.setTargetFrameRate)
            {
                Application.targetFrameRate = Mathf.Max(_settings.targetFrameRate, 1);
                Time.fixedDeltaTime = 1f / Application.targetFrameRate;
            }
            else
            {
                Application.targetFrameRate = -1;
            }

            StartCoroutine(InitializeClientSim());
        }

        private IEnumerator InitializeClientSim()
        {
            // VRChatBug: VRChat does not initialize SDK components immediately. This delay is to provide Unity
            // components time to work before Udon starts to simulate how it is in client.
            float startDelay = Mathf.Max(_settings.initializationDelay, 0);
            yield return new WaitForSeconds(startDelay);

            _isReady = true;
            
            // Enable the player if set to spawn in the settings.
            if (_settings.spawnPlayer)
            {
                // Player can be invalid if we weren't able to spawn it
                if (_player)
                {
                    _player.isInstanceOwner = _settings.isInstanceOwner;
                    _sceneManager.ResetSpawnOrder(); // Avoids any spawn offsets from pre-initialization of players
                    _player.EnablePlayer(_sceneManager.GetSpawnPoint(false));
                }
            }

            // Notify UdonManager that ClientSim is ready. This will then notify all registered UdonBehaviours that
            // they can begin running. Udon will initialize in the next frame in the next Update call.
            yield return _udonManager.OnClientSimReady();
            
            // Notify PlayerManager to send OnPlayerJoined events.
            // This must happen after Udon has been initialized to ensure UdonBehaviours are active to receive the event.
            // This must happen before ClientSimReadyEvent as some listeners depend on this.
            _playerManager.OnClientSimReady();
            
            // Send event indicating ClientSim is initialized and ready.
            _eventDispatcher.SendEvent(new ClientSimReadyEvent());
            
            stackedCameraSystem.Ready();
            
            CheckForScreenChange().Forget();
            
            this.Log("ClientSim Initialized");
        }

        private void SpawnProxyObjects()
        {
            if (!proxyObjectPrefab)
            {
                throw new ClientSimException("Failed to start Client Sim! Proxy object prefab was not found.");
            }

            GameObject proxyInstance = Instantiate(proxyObjectPrefab);
            proxyInstance.name = "__" + proxyObjectPrefab.name;
            DontDestroyOnLoad(proxyInstance);
            
            _proxyObject = proxyInstance.GetComponent<ClientSimProxyObjects>();
            if (!_proxyObject)
            {
                throw new ClientSimException("Failed to start Client Sim! Proxy object script was not found.");
            }
            
            // Do not add this object to the blacklist since proxy objects are not blacklisted in udon.
        }

        private void SpawnLocalPlayer(string playerName = "")
        {
            _player = playerSpawner.SpawnPlayer(playerName, true);
        }
        
        private void SpawnRemotePlayerAndInitialize(string playerName = "")
        {
            playerSpawner.SpawnPlayer(playerName, false);
        }

        private bool IsObjectReady(GameObject obj)
        {
            return _isReady;
        }
        
        public bool IsNetworkReady()
        {
            return _isReady;
        }

        private string GetUniqueStringForObject(GameObject obj)
        {
            return obj.GetInstanceID().ToString();
        }

#if VRC_ENABLE_PLAYER_PERSISTENCE
        private GameObject[] GetPlayerPersistence(VRCPlayerApi playerApi)
        {
            ClientSimPlayer player = playerApi.GetClientSimPlayer();
            if (player == null || player.PlayerPersistenceRootObjects == null)
                return System.Array.Empty<GameObject>();

            return player.PlayerPersistenceRootObjects;
        }
        
        private Component FindComponentInPlayerObjects(VRCPlayerApi target, Component referenceComponent)
        {
            if (referenceComponent == null)
                throw new ArgumentNullException(nameof(referenceComponent));
            
            VRCPlayerObject sourceObject = referenceComponent.GetComponentInParent<VRC.SDK3.Components.VRCPlayerObject>(true);
            if (sourceObject == null)
                throw new ArgumentException($"{nameof(referenceComponent)} must be on a player object.");
            
            GameObject networkGameObject = ((Component)referenceComponent.gameObject.GetComponentInParent<INetworkID>(true)).gameObject;
            VRCSceneDescriptor sdk3Descriptor = (VRCSceneDescriptor)VRC_SceneDescriptor.Instance;
            int idx = sdk3Descriptor.NetworkIDCollection.FindIndex((x) => x.gameObject == networkGameObject);
            if (idx < 0)
                throw new ArgumentException($"{nameof(referenceComponent)} must be on a player object in the NetworkIDCollection.");
            
            VRC.SDKBase.Network.NetworkIDPair networkIdPair = sdk3Descriptor.NetworkIDCollection[idx];
            // See ClientSimPlayer.SetupPlayerPersistence for how the ID is calculated
            int desiredID = ClientSimNetworkingUtilities.FirstPlayerPersistenceID
                            + networkIdPair.ID;

            GameObject[] objs = GetPlayerPersistence(target);
            if (objs == null)
            {
                Debug.LogError("Player persistence is missing for " + target.displayName);
                return null;
            }

            var possibleMatches = VRC.Tools.FindComponentInPossibleClones(sourceObject.gameObject, referenceComponent, objs);
            
            Debug.Log($"Found {possibleMatches.Count()} possible matches for {referenceComponent.name} in player objects for {target.displayName}");;

            foreach (Component possibleMatch in possibleMatches)
            {
                if (!possibleMatch) continue;
                ClientSimNetworkIdHolder networkIdHolder = possibleMatch.GetComponentInParent<ClientSimNetworkIdHolder>(true);
                if (!networkIdHolder)
                    continue;
                
                int thisID = networkIdHolder.GetNetworkView()?.GetNetworkId() ?? 0;
                if (thisID <= 0)
                {
                    Debug.LogError($"Network ID is invalid for {possibleMatch.name}");
                    continue;
                }
                
                int matchID = ClientSimNetworkingUtilities.FlattenPlayerViewId(thisID);
                if (matchID == desiredID) return possibleMatch;
                Debug.LogError($"Network ID {matchID} for {possibleMatch.name} does not match desired ID {desiredID}");
            }

            Debug.LogError($"Failed to find component {referenceComponent.GetType().Name} in player objects for {target.displayName}");
            
            return null;
        }
        
        public void EnablePlayerObjects()
        {
            _player.EnablePlayerObjects();
        }
#endif

        private static void CallNetworkConfigure(VRCNetworkBehaviour behaviour)
        {
            behaviour.NetworkConfigure();
        }

        #region VRChat SDK Links

        // If adding to this list, be sure to also remove the link in the RemoveSDKLinks method 
        private void SetupSDKLinks()
        {
            UdonBehaviour.OnInit += _udonManager.InitUdon;
            
            Networking._IsMaster += _playerManager.IsLocalPlayerMaster;
            Networking._LocalPlayer += _playerManager.LocalPlayer;
            Networking._GetOwner += _playerManager.GetOwner;
            Networking._IsOwner += _playerManager.IsOwner;
            Networking._SetOwner += ClientSimPlayerManager.SetOwner;
            Networking._GetMaster += _playerManager.GetMaster;
            Networking._GetInstanceOwner += _playerManager.GetInstanceOwner;
            
            Networking._IsInstanceOwner += _playerManager.IsInstanceOwner;
            Networking._IsObjectReady += IsObjectReady;
            Networking._IsNetworkSettled += IsNetworkReady;
            
            Networking._GetUniqueName += GetUniqueStringForObject;
            
#if VRC_ENABLE_PLAYER_PERSISTENCE
            ClientSimPlayerDataWrapper.ConfigureSDK();
            Networking._GetPlayerPersistence += GetPlayerPersistence;
            Networking._FindComponentInPlayerObjects += FindComponentInPlayerObjects;
#endif
            
            VRCNetworkBehaviour.OnNetworkBehaviourAwake += CallNetworkConfigure;
            
            VRCStation.Initialize += ClientSimStationHelper.InitializeStations;
            VRCStation.useStationDelegate += ClientSimStationHelper.UseStation;
            VRCStation.exitStationDelegate += ClientSimStationHelper.ExitStation;
            VRCPlayerApi._UseAttachedStation += ClientSimStationHelper.UseAttachedStation;

            if (_player != null)
            {
                VRC_UiShape.GetEventCamera += _player.GetCameraProvider().GetCameraForObject;
            }

            VRC_Pickup.OnAwake += ClientSimPickupHelper.InitializePickup;
            VRC_Pickup.ForceDrop += ClientSimPickupHelper.ForceDrop;
            VRC_Pickup._GetCurrentPlayer += ClientSimPickupHelper.GetCurrentPlayer;
            VRC_Pickup._GetPickupHand += ClientSimPickupHelper.GetPickupHand;
            VRC_Pickup.OnDestroyed += ClientSimPickupHelper.PickupDestroy;
            VRC_Pickup.HapticEvent += ClientSimPickupHelper.PlayHapticForPickup;
            
            Components.VRCObjectPool.OnInit += syncedObjectManager.InitializeObjectPool;
            Components.VRCObjectPool.OnReturn += ClientSimObjectPoolHelper.OnReturn;
            Components.VRCObjectPool.OnSpawn += ClientSimObjectPoolHelper.OnSpawn;
            
            Components.VRCObjectSync.OnAwake += syncedObjectManager.InitializeObjectSync;
            Components.VRCObjectSync.FlagDiscontinuityHook += ClientSimObjectSyncHelper.FlagDiscontinuityHook;
            Components.VRCObjectSync.RespawnHandler += ClientSimObjectSyncHelper.RespawnObject;
            Components.VRCObjectSync.TeleportHandler += ClientSimObjectSyncHelper.TeleportTo;
            Components.VRCObjectSync.SetGravityHook += ClientSimObjectSyncHelper.SetUseGravity;
            Components.VRCObjectSync.SetKinematicHook += ClientSimObjectSyncHelper.SetIsKinematic;

            VRCPlayerApi._GetPlayerId += _playerManager.GetPlayerID;
            VRCPlayerApi._GetPlayerById += _playerManager.GetPlayerByID;
            VRCPlayerApi._isMasterDelegate += _playerManager.IsMaster;
            VRCPlayerApi._isSuspendedDelegate += _playerManager.IsSuspended;
            VRCPlayerApi._TakeOwnership += ClientSimPlayerManager.SetOwner;
            VRCPlayerApi._IsOwner += _playerManager.IsOwner;
            VRCPlayerApi._isInstanceOwnerDelegate += _playerManager.IsInstanceOwner;
            
            VRCPlayerApi._EnablePickups += ClientSimPlayerManager.EnablePickups;
            VRCPlayerApi._Immobilize += ClientSimPlayerManager.Immobilize;
            VRCPlayerApi._TeleportTo += ClientSimPlayerManager.TeleportTo;
            VRCPlayerApi._TeleportToOrientation += ClientSimPlayerManager.TeleportToOrientation;
            VRCPlayerApi._TeleportToOrientationLerp += ClientSimPlayerManager.TeleportToOrientationLerp;
            VRCPlayerApi._PlayHapticEventInHand += ClientSimPlayerManager.PlayHapticEventInHand;
            VRCPlayerApi._GetPlayerByGameObject += ClientSimPlayerManager.GetPlayerByGameObject;
            VRCPlayerApi._GetPickupInHand += ClientSimPlayerManager.GetPickupInHand;
            VRCPlayerApi._GetTrackingData += ClientSimPlayerManager.GetTrackingData;
            VRCPlayerApi._GetBonePosition += ClientSimPlayerManager.GetBonePosition;
            VRCPlayerApi._GetBoneRotation += ClientSimPlayerManager.GetBoneRotation;

            VRCPlayerApi._ClearPlayerTags += ClientSimPlayerManager.ClearPlayerTags;
            VRCPlayerApi._SetPlayerTag += ClientSimPlayerManager.SetPlayerTag;
            VRCPlayerApi._GetPlayerTag += ClientSimPlayerManager.GetPlayerTag;
            VRCPlayerApi._GetPlayersWithTag += ClientSimPlayerManager.GetPlayersWithTag;
            VRCPlayerApi._SetSilencedToTagged += ClientSimPlayerManager.SetSilencedToTagged;
            VRCPlayerApi._SetSilencedToUntagged += ClientSimPlayerManager.SetSilencedToUntagged;
            VRCPlayerApi._ClearSilence += ClientSimPlayerManager.ClearSilence;

            VRCPlayerApi._IsUserInVR += ClientSimPlayerManager.IsUserInVR;
            VRCPlayerApi._GetRunSpeed += ClientSimPlayerManager.GetRunSpeed;
            VRCPlayerApi._SetRunSpeed += ClientSimPlayerManager.SetRunSpeed;
            VRCPlayerApi._GetWalkSpeed += ClientSimPlayerManager.GetWalkSpeed;
            VRCPlayerApi._SetWalkSpeed += ClientSimPlayerManager.SetWalkSpeed;
            VRCPlayerApi._GetJumpImpulse += ClientSimPlayerManager.GetJumpImpulse;
            VRCPlayerApi._SetJumpImpulse += ClientSimPlayerManager.SetJumpImpulse;
            VRCPlayerApi._GetStrafeSpeed += ClientSimPlayerManager.GetStrafeSpeed;
            VRCPlayerApi._SetStrafeSpeed += ClientSimPlayerManager.SetStrafeSpeed;
            VRCPlayerApi._GetVelocity += ClientSimPlayerManager.GetVelocity;
            VRCPlayerApi._SetVelocity += ClientSimPlayerManager.SetVelocity;
            VRCPlayerApi._GetPosition += ClientSimPlayerManager.GetPosition;
            VRCPlayerApi._GetRotation += ClientSimPlayerManager.GetRotation;
            VRCPlayerApi._GetGravityStrength += ClientSimPlayerManager.GetGravityStrength;
            VRCPlayerApi._SetGravityStrength += ClientSimPlayerManager.SetGravityStrength;
            VRCPlayerApi.IsGrounded += ClientSimPlayerManager.IsGrounded;
            VRCPlayerApi._UseLegacyLocomotion += ClientSimPlayerManager.UseLegacyLocomotion;
            VRCPlayerApi._Respawn += ClientSimPlayerManager.Respawn;
            VRCPlayerApi._RespawnWithIndex += ClientSimPlayerManager.RespawnWithIndex;
            
            VRCPlayerApi._GetManualAvatarScalingAllowed += ClientSimPlayerManager.GetManualAvatarScalingAllowed;
            VRCPlayerApi._SetManualAvatarScalingAllowed += ClientSimPlayerManager.SetManualAvatarScalingAllowed;
            VRCPlayerApi._GetAvatarEyeHeightMinimumAsMeters += ClientSimPlayerManager.GetAvatarEyeHeightMinimumAsMeters;
            VRCPlayerApi._GetAvatarEyeHeightMaximumAsMeters += ClientSimPlayerManager.GetAvatarEyeHeightMaximumAsMeters;
            VRCPlayerApi._SetAvatarEyeHeightMinimumByMeters += ClientSimPlayerManager.SetAvatarEyeHeightMinimumByMeters;
            VRCPlayerApi._SetAvatarEyeHeightMaximumByMeters += ClientSimPlayerManager.SetAvatarEyeHeightMaximumByMeters;
            VRCPlayerApi._GetAvatarEyeHeightAsMeters += ClientSimPlayerManager.GetAvatarEyeHeightAsMeters;
            VRCPlayerApi._SetAvatarEyeHeightByMeters += ClientSimPlayerManager.SetAvatarEyeHeightByMeters;
            VRCPlayerApi._SetAvatarEyeHeightByMultiplier += ClientSimPlayerManager.SetAvatarEyeHeightByMultiplier;

            VRCPlayerApi._CombatSetup += ClientSimCombatSystemHelper.CombatSetup;
            VRCPlayerApi._CombatSetMaxHitpoints += ClientSimCombatSystemHelper.CombatSetMaxHitpoints;
            VRCPlayerApi._CombatGetCurrentHitpoints += ClientSimCombatSystemHelper.CombatGetCurrentHitpoints;
            VRCPlayerApi._CombatSetRespawn += ClientSimCombatSystemHelper.CombatSetRespawn;
            VRCPlayerApi._CombatSetDamageGraphic += ClientSimCombatSystemHelper.CombatSetDamageGraphic;
            VRCPlayerApi._CombatGetDestructible += ClientSimCombatSystemHelper.CombatGetDestructible;
            VRCPlayerApi._CombatSetCurrentHitpoints += ClientSimCombatSystemHelper.CombatSetCurrentHitpoints;
            
            VRCPlayerApi._SetAvatarAudioVolumetricRadius += ClientSimPlayerManager.SetAvatarAudioVolumetricRadius;
            VRCPlayerApi._SetAvatarAudioNearRadius += ClientSimPlayerManager.SetAvatarAudioNearRadius;
            VRCPlayerApi._SetAvatarAudioFarRadius += ClientSimPlayerManager.SetAvatarAudioFarRadius;
            VRCPlayerApi._SetAvatarAudioGain += ClientSimPlayerManager.SetAvatarAudioGain;
            VRCPlayerApi._SetAvatarAudioForceSpatial += ClientSimPlayerManager.SetAvatarAudioForceSpatial;
            VRCPlayerApi._SetAvatarAudioCustomCurve += ClientSimPlayerManager.SetAvatarAudioCustomCurve;
            
            VRCPlayerApi._SetVoiceGain += ClientSimPlayerManager.SetVoiceGain;
            VRCPlayerApi._SetVoiceDistanceNear += ClientSimPlayerManager.SetVoiceDistanceNear;
            VRCPlayerApi._SetVoiceDistanceFar += ClientSimPlayerManager.SetVoiceDistanceFar;
            VRCPlayerApi._SetVoiceVolumetricRadius += ClientSimPlayerManager.SetVoiceVolumetricRadius;
            VRCPlayerApi._SetVoiceLowpass += ClientSimPlayerManager.SetVoiceLowpass;

            VRCPlayerApi._GetVoiceGain += ClientSimPlayerManager.GetVoiceGain;
            VRCPlayerApi._GetVoiceDistanceNear += ClientSimPlayerManager.GetVoiceDistanceNear;
            VRCPlayerApi._GetVoiceDistanceFar += ClientSimPlayerManager.GetVoiceDistanceFar;
            VRCPlayerApi._GetVoiceVolumetricRadius += ClientSimPlayerManager.GetVoiceVolumetricRadius;
            VRCPlayerApi._GetVoiceLowpass += ClientSimPlayerManager.GetVoiceLowpass;

            VRCPlayerApi._GetCurrentLanguage += ClientSimPlayerManager.GetCurrentLanguage;
            VRCPlayerApi._GetAvailableLanguages += ClientSimPlayerManager.GetAvailableLanguages;
            
            VRC_SpatialAudioSource.Initialize += ClientSimSpatialAudioHelper.InitializeAudio;

            VRCAVProVideoPlayer.Initialize += ClientSimAVProVideoStub.InitializePlayer;

            InputManager._EnableObjectHighlight += highlightManager.EnableObjectHighlight;
            InputManager._GetLastUsedInputMethod += inputManager.GetLastUsedInputMethod;
            
            Store._sendProductEvent += ClientSimStoreManager.SendProductEvent;
            Store._listPurchases += ClientSimStoreManager.ListPurchases;
            Store._listAvailableProducts += ClientSimStoreManager.ListAvailableProducts;
            Store._doesPlayerOwnProduct += ClientSimStoreManager.DoesPlayerOwnProduct;
            Store._doesAnyPlayerOwnProduct += ClientSimStoreManager.DoesAnyPlayerOwnProduct;
            Store._getPlayersWhoOwnProduct += ClientSimStoreManager.GetPlayersWhoOwnProduct;
            Store._listProductOwners += ClientSimStoreManager.ListProductOwners;
            
        }

        private void RemoveSDKLinks()
        {
            UdonBehaviour.OnInit -= _udonManager.InitUdon;
            
            Networking._IsMaster -= _playerManager.IsLocalPlayerMaster;
            Networking._LocalPlayer -= _playerManager.LocalPlayer;
            Networking._GetOwner -= _playerManager.GetOwner;
            Networking._IsOwner -= _playerManager.IsOwner;
            Networking._SetOwner -= ClientSimPlayerManager.SetOwner;
            
            Networking._IsInstanceOwner -= _playerManager.IsInstanceOwner;
            Networking._IsObjectReady -= IsObjectReady;
            Networking._IsNetworkSettled -= IsNetworkReady;

            Networking._GetUniqueName -= GetUniqueStringForObject;

#if VRC_ENABLE_PLAYER_PERSISTENCE
            Networking._GetPlayerPersistence -= GetPlayerPersistence;
            Networking._FindComponentInPlayerObjects -= FindComponentInPlayerObjects;
#endif
            
            VRCNetworkBehaviour.OnNetworkBehaviourAwake -= CallNetworkConfigure;

            VRCStation.Initialize -= ClientSimStationHelper.InitializeStations;
            VRCStation.useStationDelegate -= ClientSimStationHelper.UseStation;
            VRCStation.exitStationDelegate -= ClientSimStationHelper.ExitStation;
            VRCPlayerApi._UseAttachedStation -= ClientSimStationHelper.UseAttachedStation;

            // Player can be invalid if we weren't able to spawn
            if (_player)
            {
                VRC_UiShape.GetEventCamera -= _player.GetCameraProvider().GetCameraForObject;
            }

            VRC_Pickup.OnAwake -= ClientSimPickupHelper.InitializePickup;
            VRC_Pickup.ForceDrop -= ClientSimPickupHelper.ForceDrop;
            VRC_Pickup._GetCurrentPlayer -= ClientSimPickupHelper.GetCurrentPlayer;
            VRC_Pickup._GetPickupHand -= ClientSimPickupHelper.GetPickupHand;
            VRC_Pickup.OnDestroyed -= ClientSimPickupHelper.PickupDestroy;
            VRC_Pickup.HapticEvent -= ClientSimPickupHelper.PlayHapticForPickup;
            
            Components.VRCObjectPool.OnInit -= syncedObjectManager.InitializeObjectPool;
            Components.VRCObjectPool.OnReturn -= ClientSimObjectPoolHelper.OnReturn;
            Components.VRCObjectPool.OnSpawn -= ClientSimObjectPoolHelper.OnSpawn;
            
            Components.VRCObjectSync.OnAwake -= syncedObjectManager.InitializeObjectSync;
            Components.VRCObjectSync.FlagDiscontinuityHook -= ClientSimObjectSyncHelper.FlagDiscontinuityHook;
            Components.VRCObjectSync.RespawnHandler -= ClientSimObjectSyncHelper.RespawnObject;
            Components.VRCObjectSync.TeleportHandler -= ClientSimObjectSyncHelper.TeleportTo;
            Components.VRCObjectSync.SetGravityHook -= ClientSimObjectSyncHelper.SetUseGravity;
            Components.VRCObjectSync.SetKinematicHook -= ClientSimObjectSyncHelper.SetIsKinematic;

            VRCPlayerApi._GetPlayerId -= _playerManager.GetPlayerID;
            VRCPlayerApi._GetPlayerById -= _playerManager.GetPlayerByID;
            VRCPlayerApi._isMasterDelegate -= _playerManager.IsMaster;
            VRCPlayerApi._TakeOwnership -= ClientSimPlayerManager.SetOwner;
            VRCPlayerApi._IsOwner -= _playerManager.IsOwner;
            VRCPlayerApi._isInstanceOwnerDelegate -= _playerManager.IsInstanceOwner;
            
            VRCPlayerApi._EnablePickups -= ClientSimPlayerManager.EnablePickups;
            VRCPlayerApi._Immobilize -= ClientSimPlayerManager.Immobilize;
            VRCPlayerApi._TeleportTo -= ClientSimPlayerManager.TeleportTo;
            VRCPlayerApi._TeleportToOrientation -= ClientSimPlayerManager.TeleportToOrientation;
            VRCPlayerApi._TeleportToOrientationLerp -= ClientSimPlayerManager.TeleportToOrientationLerp;
            VRCPlayerApi._PlayHapticEventInHand -= ClientSimPlayerManager.PlayHapticEventInHand;
            VRCPlayerApi._GetPlayerByGameObject -= ClientSimPlayerManager.GetPlayerByGameObject;
            VRCPlayerApi._GetPickupInHand -= ClientSimPlayerManager.GetPickupInHand;
            VRCPlayerApi._GetTrackingData -= ClientSimPlayerManager.GetTrackingData;
            VRCPlayerApi._GetBonePosition -= ClientSimPlayerManager.GetBonePosition;
            VRCPlayerApi._GetBoneRotation -= ClientSimPlayerManager.GetBoneRotation;

            VRCPlayerApi._ClearPlayerTags -= ClientSimPlayerManager.ClearPlayerTags;
            VRCPlayerApi._SetPlayerTag -= ClientSimPlayerManager.SetPlayerTag;
            VRCPlayerApi._GetPlayerTag -= ClientSimPlayerManager.GetPlayerTag;
            VRCPlayerApi._GetPlayersWithTag -= ClientSimPlayerManager.GetPlayersWithTag;
            VRCPlayerApi._SetSilencedToTagged -= ClientSimPlayerManager.SetSilencedToTagged;
            VRCPlayerApi._SetSilencedToUntagged -= ClientSimPlayerManager.SetSilencedToUntagged;
            VRCPlayerApi._ClearSilence -= ClientSimPlayerManager.ClearSilence;

            VRCPlayerApi._IsUserInVR -= ClientSimPlayerManager.IsUserInVR;
            VRCPlayerApi._GetRunSpeed -= ClientSimPlayerManager.GetRunSpeed;
            VRCPlayerApi._SetRunSpeed -= ClientSimPlayerManager.SetRunSpeed;
            VRCPlayerApi._GetWalkSpeed -= ClientSimPlayerManager.GetWalkSpeed;
            VRCPlayerApi._SetWalkSpeed -= ClientSimPlayerManager.SetWalkSpeed;
            VRCPlayerApi._GetJumpImpulse -= ClientSimPlayerManager.GetJumpImpulse;
            VRCPlayerApi._SetJumpImpulse -= ClientSimPlayerManager.SetJumpImpulse;
            VRCPlayerApi._GetStrafeSpeed -= ClientSimPlayerManager.GetStrafeSpeed;
            VRCPlayerApi._SetStrafeSpeed -= ClientSimPlayerManager.SetStrafeSpeed;
            VRCPlayerApi._GetVelocity -= ClientSimPlayerManager.GetVelocity;
            VRCPlayerApi._SetVelocity -= ClientSimPlayerManager.SetVelocity;
            VRCPlayerApi._GetPosition -= ClientSimPlayerManager.GetPosition;
            VRCPlayerApi._GetRotation -= ClientSimPlayerManager.GetRotation;
            VRCPlayerApi._GetGravityStrength -= ClientSimPlayerManager.GetGravityStrength;
            VRCPlayerApi._SetGravityStrength -= ClientSimPlayerManager.SetGravityStrength;
            VRCPlayerApi.IsGrounded -= ClientSimPlayerManager.IsGrounded;
            VRCPlayerApi._UseLegacyLocomotion -= ClientSimPlayerManager.UseLegacyLocomotion;
            VRCPlayerApi._Respawn -= ClientSimPlayerManager.Respawn;
            VRCPlayerApi._RespawnWithIndex -= ClientSimPlayerManager.RespawnWithIndex;
            
            VRCPlayerApi._GetManualAvatarScalingAllowed -= ClientSimPlayerManager.GetManualAvatarScalingAllowed;
            VRCPlayerApi._SetManualAvatarScalingAllowed -= ClientSimPlayerManager.SetManualAvatarScalingAllowed;
            VRCPlayerApi._GetAvatarEyeHeightMinimumAsMeters -= ClientSimPlayerManager.GetAvatarEyeHeightMinimumAsMeters;
            VRCPlayerApi._GetAvatarEyeHeightMaximumAsMeters -= ClientSimPlayerManager.GetAvatarEyeHeightMaximumAsMeters;
            VRCPlayerApi._SetAvatarEyeHeightMinimumByMeters -= ClientSimPlayerManager.SetAvatarEyeHeightMinimumByMeters;
            VRCPlayerApi._SetAvatarEyeHeightMaximumByMeters -= ClientSimPlayerManager.SetAvatarEyeHeightMaximumByMeters;
            VRCPlayerApi._GetAvatarEyeHeightAsMeters -= ClientSimPlayerManager.GetAvatarEyeHeightAsMeters;
            VRCPlayerApi._SetAvatarEyeHeightByMeters -= ClientSimPlayerManager.SetAvatarEyeHeightByMeters;
            VRCPlayerApi._SetAvatarEyeHeightByMultiplier -= ClientSimPlayerManager.SetAvatarEyeHeightByMultiplier;

            VRCPlayerApi._CombatSetup -= ClientSimCombatSystemHelper.CombatSetup;
            VRCPlayerApi._CombatSetMaxHitpoints -= ClientSimCombatSystemHelper.CombatSetMaxHitpoints;
            VRCPlayerApi._CombatGetCurrentHitpoints -= ClientSimCombatSystemHelper.CombatGetCurrentHitpoints;
            VRCPlayerApi._CombatSetRespawn -= ClientSimCombatSystemHelper.CombatSetRespawn;
            VRCPlayerApi._CombatSetDamageGraphic -= ClientSimCombatSystemHelper.CombatSetDamageGraphic;
            VRCPlayerApi._CombatGetDestructible -= ClientSimCombatSystemHelper.CombatGetDestructible;
            VRCPlayerApi._CombatSetCurrentHitpoints -= ClientSimCombatSystemHelper.CombatSetCurrentHitpoints;
            
            VRCPlayerApi._SetAvatarAudioVolumetricRadius -= ClientSimPlayerManager.SetAvatarAudioVolumetricRadius;
            VRCPlayerApi._SetAvatarAudioNearRadius -= ClientSimPlayerManager.SetAvatarAudioNearRadius;
            VRCPlayerApi._SetAvatarAudioFarRadius -= ClientSimPlayerManager.SetAvatarAudioFarRadius;
            VRCPlayerApi._SetAvatarAudioGain -= ClientSimPlayerManager.SetAvatarAudioGain;
            VRCPlayerApi._SetAvatarAudioForceSpatial -= ClientSimPlayerManager.SetAvatarAudioForceSpatial;
            VRCPlayerApi._SetAvatarAudioCustomCurve -= ClientSimPlayerManager.SetAvatarAudioCustomCurve;
            
            VRCPlayerApi._SetVoiceLowpass -= ClientSimPlayerManager.SetVoiceLowpass;
            VRCPlayerApi._SetVoiceVolumetricRadius -= ClientSimPlayerManager.SetVoiceVolumetricRadius;
            VRCPlayerApi._SetVoiceDistanceFar -= ClientSimPlayerManager.SetVoiceDistanceFar;
            VRCPlayerApi._SetVoiceDistanceNear -= ClientSimPlayerManager.SetVoiceDistanceNear;
            VRCPlayerApi._SetVoiceGain -= ClientSimPlayerManager.SetVoiceGain;
            
            VRC_SpatialAudioSource.Initialize -= ClientSimSpatialAudioHelper.InitializeAudio;

            VRCAVProVideoPlayer.Initialize -= ClientSimAVProVideoStub.InitializePlayer;

            InputManager._EnableObjectHighlight -= highlightManager.EnableObjectHighlight;
            
            Store._sendProductEvent -= ClientSimStoreManager.SendProductEvent;
            Store._listPurchases -= ClientSimStoreManager.ListPurchases;
            Store._listAvailableProducts -= ClientSimStoreManager.ListAvailableProducts;
            Store._doesPlayerOwnProduct -= ClientSimStoreManager.DoesPlayerOwnProduct;
            Store._doesAnyPlayerOwnProduct -= ClientSimStoreManager.DoesAnyPlayerOwnProduct;
            Store._getPlayersWhoOwnProduct -= ClientSimStoreManager.GetPlayersWhoOwnProduct;
        }

        #endregion

        #region Test Accessors

        internal ClientSimProxyObjects GetProxyObjects()
        {
            return _proxyObject;
        }

        internal ClientSimMenu GetMenu()
        {
            return menu;
        }

        internal IClientSimEventDispatcher GetEventDispatcher()
        {
            return _eventDispatcher;
        }
        
        internal IClientSimUdonEventSender GetUdonEventSender()
        {
            return _udonEventSender;
        }
        
        internal IClientSimBlacklistManager GetBlacklistManager()
        {
            return _blacklistManager;
        }
        
        internal IClientSimUdonManager GetUdonManager()
        {
            return _udonManager;
        }

        internal IClientSimSyncedObjectManager GetSyncedObjectManager()
        {
            return syncedObjectManager;
        }
        
        internal IClientSimPlayerManager GetPlayerManager()
        {
            return _playerManager;
        }

        #endregion
    }
}
