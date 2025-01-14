using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Utility;
using Object = UnityEngine.Object;
using VRCStation = VRC.SDK3.Components.VRCStation;

#if VRC_ENABLE_PLAYER_PERSISTENCE
using VRC.SDK3.ClientSim.Persistence;
using VRC.SDKBase.Network;
#endif

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// ClientSimPlayer is the container class for all the player related systems.
    /// </summary>
    /// <remarks>
    /// When the user clicks on the player in the inspector, this class also allows you to edit settings,
    /// such as locomotion and audio values.
    /// </remarks>
    // TODO split into local and remote versions
    [AddComponentMenu("")]
    [SelectionBase]
    public class ClientSimPlayer : ClientSimBehaviour, IClientSimPlayerApiProvider
    {
        [SerializeField]
        private ClientSimPlayerController playerController;
        [SerializeField] 
        private ClientSimPlayerStationManager stationManager;
        [SerializeField]
        private ClientSimPlayerRaycaster playerRaycaster;
        [SerializeField]
        private ClientSimTrackingProviderBase playerTrackingData;
        [SerializeField]
        private ClientSimPlayerAvatarManager playerAvatar;
        [SerializeField]
        private ClientSimReticle reticle;

        private ClientSimCombatSystemHelper _combatSystemHelper;

        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimPlayerManager _playerManager;
        private ClientSimInteractManager _interactManager;
        private IClientSimSceneManager _sceneManager;
        private IClientSimProxyObjectProvider _proxyProvider;
        private IClientSimUdonEventSender _udonEventSender;
        private ClientSimSettings _settings;
        
        public VRCPlayerApi Player { get; private set; }
        public bool IsUserVR { get; private set; }
        public bool isInstanceOwner;
        public bool isSuspended;

        // Public to allow users to edit values in editor.
        public ClientSimPlayerLocomotionData locomotionData = new();
        public ClientSimPlayerPickupData pickupData = new();
        public ClientSimPlayerAudioData audioData = new();
        public ClientSimPlayerTagsData tagData = new();

#if VRC_ENABLE_PLAYER_PERSISTENCE
        public GameObject[] PlayerPersistenceObjects = Array.Empty<GameObject>();
        public GameObject[] PlayerPersistenceRootObjects = Array.Empty<GameObject>();
        public ClientSimPlayerDataStorage PlayerDataPrefab;
        internal ClientSimPlayerDataStorage PlayerDataObject;
        internal ClientSimPlayerObjectStorage PlayerObjectData;
        private ClientSimPlayerRestoredStatus playerRestoredStatus = new();
#endif
        
        public void SetPlayer(VRCPlayerApi player)
        {
            Player = player;
        }

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimInput input,
            ClientSimSettings settings,
            IClientSimHighlightManager highlightManager,
            IClientSimTooltipManager tooltipManager,
            IClientSimInteractiveLayerProvider interactiveLayerProvider,
            IClientSimMousePositionProvider mousePositionProvider,
            IClientSimSceneManager sceneManager,
            IClientSimProxyObjectProvider proxyProvider,
            IClientSimUdonEventSender udonEventSender,
            IClientSimBlacklistManager blacklistManager,
            IClientSimUdonManager udonManager,
            IClientSimSyncedObjectManager syncedObjectManager,
            IClientSimPlayerManager playerManager,
            IClientSimPlayerHeightManager heightManager)
        {
            _eventDispatcher = eventDispatcher;
            _settings = settings;
            _sceneManager = sceneManager;
            _proxyProvider = proxyProvider;
            _playerManager = playerManager;
            _udonEventSender = udonEventSender;
            
            // TODO take settings and spawn desktop vs vr tracking data
            playerTrackingData.Initialize(eventDispatcher, input, settings, heightManager);
            IsUserVR = playerTrackingData.IsVR();
            
            _interactManager = new ClientSimInteractManager(playerTrackingData, pickupData);

            playerRaycaster.Initialize(
                eventDispatcher,
                input, 
                this,
                pickupData,
                highlightManager,
                tooltipManager,
                interactiveLayerProvider,
                playerTrackingData, 
                mousePositionProvider,
                _interactManager,
                playerTrackingData,
                stationManager);
            
            stationManager.Initialize(eventDispatcher, this);
            
            playerController.Initialize(
                eventDispatcher, 
                input, 
                this, 
                locomotionData,
                sceneManager, 
                proxyProvider, 
                playerTrackingData,
                stationManager);
            
            playerAvatar.Initialize(eventDispatcher);

            if (settings.spawnPlayer)
            {
                if (!IsUserVR)
                {
                    reticle.Initialize(_eventDispatcher, _settings, mousePositionProvider);
                }
                // TODO initialize VR raycast visualizers
            }
        }

        public void SetEventDispatcher(IClientSimEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        public void InitializeCombat()
        {
            if (_combatSystemHelper != null)
            {
                return;
            }

            _combatSystemHelper = gameObject.AddComponent<ClientSimCombatSystemHelper>();
            _combatSystemHelper.Initialize(Player, _eventDispatcher, _proxyProvider, playerController);
        }

#if VRC_ENABLE_PLAYER_PERSISTENCE

        internal void SetupPlayerPersistence(IClientSimEventDispatcher eventDispatcher, IClientSimUdonEventSender udonEventSender, IClientSimBlacklistManager blacklistManager, IClientSimUdonManager udonManager, IClientSimSyncedObjectManager syncedObjectManager,IClientSimPlayerManager playerManager)
        {
            _eventDispatcher = eventDispatcher;
            _udonEventSender = udonEventSender;
            
            eventDispatcher.Subscribe<ClientSimOnPlayerDataDecodedEvent>(OnPlayerDataDecoded);
            eventDispatcher.Subscribe<ClientSimOnPlayerObjectsDecodedEvent>(OnPlayerObjectsDecoded);
            
            GameObject dataObject = Instantiate(PlayerDataPrefab.gameObject);
            int playerId = playerManager.GetPlayerID(Player);
            
            PlayerDataObject = dataObject.GetComponent<ClientSimPlayerDataStorage>();
            PlayerDataObject.Init(Player, udonEventSender, eventDispatcher);
            PlayerDataObject.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            
            PlayerObjectData = dataObject.GetComponent<ClientSimPlayerObjectStorage>();
            PlayerObjectData.Init(Player, udonEventSender, eventDispatcher);
            
            blacklistManager.AddObjectAndChildrenToBlackList(PlayerDataObject.gameObject);
            
            // spawning PlayerObjects
            VRCPlayerObject[] playerObjects = ClientSimNetworkingUtilities.GetPlayerObjectList();
            
            VRCSceneDescriptor sdk3Descriptor = (VRCSceneDescriptor)VRC_SceneDescriptor.Instance;
            
            List<GameObject> playerObjectInstances = new List<GameObject>();
            
            int baseId = (playerId * ClientSimNetworkingUtilities.MaxID) + 
                         ClientSimNetworkingUtilities.FirstPlayerPersistenceID;
            
            for (int i = 0; i < playerObjects.Length; i++)
            {
                VRCPlayerObject playerObject = playerObjects[i];
                GameObject playerObjectGameObject = playerObject.gameObject;
                Transform playerObjectTransform = playerObjectGameObject.transform;
                playerObjectGameObject.SetActive(false);
            
                GameObject instance = Object.Instantiate(playerObjectGameObject, playerObjectTransform.parent, true);
                instance.transform.localScale = playerObjectTransform.localScale;
                instance.transform.localPosition = playerObjectTransform.localPosition;
                instance.transform.localRotation = playerObjectTransform.localRotation;
                
                playerObjectInstances.Add(instance);
                
                instance.transform.name = playerObjectTransform.name + " [" + playerId + "]"; 
                
                INetworkID[] networkIds = instance.GetComponentsInChildren<INetworkID>(true);

                foreach (INetworkID networkId in networkIds)
                {
                    Component component = networkId as Component;
                    string path = component.transform.Path(instance.transform);
                    
                    GameObject ppOriginal = playerObjectTransform.Find(path).gameObject;
                    
                    int indexNetworkObject = sdk3Descriptor.NetworkIDCollection.FindIndex((x) => x.gameObject == ppOriginal);
                
                    if (indexNetworkObject == -1)
                    {
                        this.LogError($"Failed to locate player persistence view ID for {playerObjectTransform.name}/{path}");
                        continue;
                    }
                    
                    NetworkIDPair networkIdPair = sdk3Descriptor.NetworkIDCollection[indexNetworkObject];

                    int viewId = ClientSimNetworkingUtilities.FlattenPlayerViewId(networkIdPair.ID) + baseId;
                    
                    ConfigureObject(component.gameObject, viewId, playerId, networkId, null, udonManager, syncedObjectManager);
                    
                    viewId++;
                    
                    if (viewId >= (playerId * ClientSimNetworkingUtilities.MaxID) +ClientSimNetworkingUtilities.MaxPlayerPersistenceID)
                    {
                        this.LogError("Ran out of player persistence view IDs.");
                        break;
                    }
                }
                
                foreach (VRCStation station in instance.GetComponentsInChildren<VRCStation>()){
                    ClientSimStationHelper.InitializeStations(station);
                }
                
                instance.SetActive(true);
            }
            
            IEnumerable<GameObject> withChildren = playerObjectInstances.SelectMany(obj => obj.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject);
            PlayerPersistenceObjects = withChildren.ToArray();
            PlayerPersistenceRootObjects = playerObjectInstances.ToArray();
        }
        
        private static void ConfigureObject(
            GameObject obj,
            int viewId, 
            int playerId,
            INetworkID networkId,
            string objectName = null,
            IClientSimUdonManager udonManager = null,
            IClientSimSyncedObjectManager syncedObjectManager = null)
        {
            VRCEnablePersistence enablePersistence = obj.GetComponentInParent<VRCEnablePersistence>(true);
            bool SavePersistence = enablePersistence != null;

            if (!obj.TryGetComponent(out ClientSimNetworkingView MainView))
            {
                MainView = obj.AddComponent<ClientSimNetworkingView>();
            }

            MainView.SetNetworkId(viewId);
            MainView.SetPlayerId(playerId);
            MainView.SetPersist(SavePersistence);
            
            InitilizeNetworkHolder(obj, MainView);

            switch (networkId)
            {
                case VRCObjectPool vrcop:
                {
                    if (vrcop)
                    {
                        vrcop.NetworkConfigure();
                        syncedObjectManager.InitializeObjectPool(vrcop);
                    }
                    break;
                }
                case VRCObjectSync vrcos:
                {
                    if (vrcos)
                    {
                        vrcos.NetworkConfigure();
                        // the player initialization happens before the sdk has set the synced object callbacks so we need to do it here
                        if(VRCObjectSync.OnAwake == null)
                            syncedObjectManager.InitializeObjectSync(vrcos);
                    }
                    break;
                }
                case VRC.SDK3.Network.VRCNetworkBehaviour vrcnb3:
                {
                    if (vrcnb3)
                    {
                        vrcnb3.NetworkConfigure();  
                    }

                    break;
                }
                case UdonBehaviour udon:
                {
                    if (udon)
                    {

                        if (UdonManager.Instance.HasLoaded)
                        {
                            udon.IsNetworkingSupported = true;
                            UdonManager.Instance.RegisterUdonBehaviour(udon);
                        }
                    }

                    break;
                }
                case VRCPickup vrcPickup:
                {
                    if (vrcPickup)
                    {
                        ClientSimPickupHelper.InitializePickup(vrcPickup);
                    }

                    break;
                }
            }

            IClientSimSyncable[] syncables = obj.GetComponentsInChildren<IClientSimSyncable>(true);
            foreach (IClientSimSyncable syncable in syncables)
            {
                syncable.SetOwner(playerId);
            }
        }
        
        private static void InitilizeNetworkHolder(GameObject obj, ClientSimNetworkingView mainView)
        {
            if (!obj.TryGetComponent<ClientSimNetworkIdHolder>(out var networkIdHolder))
            {
                networkIdHolder = obj.AddComponent<ClientSimNetworkIdHolder>();
                networkIdHolder.SetNetworkView(mainView);
                networkIdHolder.SetNetworkComponents();
                
                mainView.AddNetworkedObject(networkIdHolder);
            }
        }

        private void RemovePlayerPersistenceObjects()
        {
            for (int i = 0; i < PlayerPersistenceObjects.Length; i++)
            {
                Object.Destroy(PlayerPersistenceObjects[i]);
            }
        }

        private void CheckPlayerRestored()
        {
            if (playerRestoredStatus.HasDecodedPlayerData && 
                playerRestoredStatus.HasDecodedPlayerObjects && 
                !playerRestoredStatus.PlayerRestored)
            {
                playerRestoredStatus.PlayerRestored = true;
                _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONPLAYERRESTORED, ("player", Player));
                _eventDispatcher.SendEvent(new ClientSimOnPlayerRestoredEvent
                {
                    player = Player
                });
            }
        }
        
        private void OnPlayerDataDecoded(ClientSimOnPlayerDataDecodedEvent payload)
        {
            if (payload.player.playerId != Player.playerId) return; 
            
            playerRestoredStatus.HasDecodedPlayerData = true;
            CheckPlayerRestored();
        }

        private void OnPlayerObjectsDecoded(ClientSimOnPlayerObjectsDecodedEvent payload)
        {
            if (payload.player.playerId != Player.playerId) return; 

            playerRestoredStatus.HasDecodedPlayerObjects = true;
            CheckPlayerRestored();
        }
        
        private void OnDestroy()
        {
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerDataDecodedEvent>(OnPlayerDataDecoded);
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerObjectsDecodedEvent>(OnPlayerObjectsDecoded);

            RemovePlayerPersistenceObjects();
        }
        
        public void EnablePlayerObjects()
        {
            for (int i = 0; i < PlayerPersistenceRootObjects.Length; i++)
            {
                PlayerPersistenceRootObjects[i].SetActive(true);
            }
        }
#endif

        private void Start()
        {
            if (!Player.isLocal)
            {
                return;
            }
            
            Camera playerCamera = playerTrackingData.GetCamera();
            if (playerCamera != null)
            {
                _sceneManager.SetupCamera(playerCamera);
            }
        }

        public void EnablePlayer(Transform spawnPoint)
        {
            playerController.Teleport(spawnPoint, false);
            gameObject.SetActive(true);
        }

        public ClientSimPlayerController GetPlayerController()
        {
            return playerController;
        }
        
        public IClientSimPlayerCameraProvider GetCameraProvider()
        {
            return playerTrackingData;
        }

        public IClientSimTrackingProvider GetTrackingProvider()
        {
            return playerTrackingData;
        }

        public IClientSimPlayerStationManager GetStationHandler()
        {
            return stationManager;
        }
        
        public IClientSimPlayerAvatarDataProvider GetAvatarDataProvider()
        {
            return playerAvatar;
        }

        public ClientSimCombatSystemHelper GetCombatHelper()
        {
            return _combatSystemHelper;
        }

        public Vector3 GetPosition()
        {
            if (playerController != null)
            {
                return playerController.GetPosition();
            }
            return transform.position;
        }
        
        public Quaternion GetRotation()
        {
            if (playerController != null)
            {
                return playerController.GetRotation();
            }
            return transform.rotation;
        }

        public void SimulateVRCPlusGift()
        {
            _eventDispatcher?.SendEvent(new ClientSimOnVRCPlusMassGift
            {
                gifter = Player,
                numGifts = 10,
            });
        }

        private class ClientSimPlayerRestoredStatus
        {
            public bool HasDecodedPlayerData = false;
            public bool HasDecodedPlayerObjects = false;
            public bool PlayerRestored = false;
        }
    }
}