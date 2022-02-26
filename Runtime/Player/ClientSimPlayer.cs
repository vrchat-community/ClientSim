using UnityEngine;
using VRC.SDKBase;

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
        private ClientSimInteractManager _interactManager;
        private IClientSimSceneManager _sceneManager;
        private IClientSimProxyObjectProvider _proxyProvider;
        private ClientSimSettings _settings;
        
        public VRCPlayerApi Player { get; private set; }
        public bool IsUserVR { get; private set; }
        public bool isInstanceOwner;

        // Public to allow users to edit values in editor.
        public ClientSimPlayerLocomotionData locomotionData = new ClientSimPlayerLocomotionData();
        public ClientSimPlayerPickupData pickupData = new ClientSimPlayerPickupData();
        public ClientSimPlayerAudioData audioData = new ClientSimPlayerAudioData();
        public ClientSimPlayerTagsData tagData = new ClientSimPlayerTagsData();
        
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
            IClientSimProxyObjectProvider proxyProvider)
        {
            _eventDispatcher = eventDispatcher;
            _settings = settings;
            _sceneManager = sceneManager;
            _proxyProvider = proxyProvider;
            
            
            // TODO take settings and spawn desktop vs vr tracking data
            playerTrackingData.Initialize(eventDispatcher, input, settings);
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

        public void InitializeCombat()
        {
            if (_combatSystemHelper != null)
            {
                return;
            }

            _combatSystemHelper = gameObject.AddComponent<ClientSimCombatSystemHelper>();
            _combatSystemHelper.Initialize(Player, _eventDispatcher, _proxyProvider, playerController);
        }

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
    }
}