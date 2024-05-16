using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for displaying the menu to the players and updating settings.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimMenuStateChangedEvent
    /// - ClientSimMenuRespawnClickedEvent
    /// - ClientSimOnPlayerHeightUpdateEvent
    /// Listens to Events:
    /// - ClientSimReadyEvent
    /// - ClientSimOnPlayerMovedEvent
    /// - ClientSimOnNewMasterEvent
    /// - ClientSimOnPlayerJoinedEvent
    /// Listens to Input Events:
    /// - ToggleMenu
    /// </remarks>
    [AddComponentMenu("")]
    public class ClientSimMenu : ClientSimBehaviour, IDisposable
    {
        // Property name on UI shaders to set the ZTest mode. Used to make the menu appear on top of everything.
        private const string GUI_ZTEST_MODE_PROPERTY_NAME = "unity_GUIZTestMode";

        private const string HAS_USER_ACCEPTED_WARNING = "accepted_warning";
        
        public enum ClientSimDisplayedPage
        { 
            PAUSE_MENU,
            WARNING_PAGE,
            INVALID_SETTINGS_PAGE,
            DELAYED_START_PAGE,
        }
        
        // The method to open the settings window is set from Editor context.
        // This hook is set on playmode start in ClientSimEditorRuntimeLinker.cs
        internal static Action openSettingsHook;
        // This method allows the menu to check the editor only method if ClientSim has all settings properly set
        internal static Func<bool> checkValidSettingsHook;
        
        [SerializeField]
        private GameObject menu;

        public float menuScaleFactor = 0.0035f;
        
        [SerializeField]
        private GameObject pauseMenu;
        [SerializeField]
        private GameObject warningsPage;
        [SerializeField]
        private GameObject invalidSettingsPage;
        [SerializeField]
        private GameObject delayStartPage;


        [SerializeField]
        private Toggle tooltipsToggle;
        [SerializeField]
        private Toggle reticleToggle;
        [SerializeField]
        private Toggle invertMouseToggle;
        [SerializeField]
        private Toggle consoleLoggingToggle;
        [SerializeField]
        private Slider playerHeightSlider;
        [SerializeField]
        private Text playerHeightText;

        [SerializeField]
        private Text playerNameText;
        [SerializeField]
        private Text playerIdText;
        [SerializeField]
        private Toggle isMasterToggle;
        [SerializeField]
        private Toggle isInstanceOwnerToggle;

        
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimInput _input;
        private ClientSimSettings _settings;
        private IClientSimSessionState _sessionState;
        private IClientSimPlayerHeightManager _heightManager;

        private ClientSimDisplayedPage _displayedPage = ClientSimDisplayedPage.WARNING_PAGE;
        private bool _menuIsActive;

        private float _playerHeightOriginalMaxvalue;

        private Canvas _menuCanvas;

        public void SetCanvasCamera(Camera cam)
        {
            _menuCanvas.worldCamera = cam;
        }
        
        protected override void Awake()
        {
            base.Awake();

            _menuCanvas = menu.GetComponent<Canvas>();
            _playerHeightOriginalMaxvalue = playerHeightSlider.maxValue;

        }

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimInput input, 
            ClientSimSettings settings,
            IClientSimSessionState sessionState,
            IClientSimPlayerHeightManager heightManager)
        {
            _eventDispatcher = eventDispatcher;
            _input = input;
            _settings = settings;
            _sessionState = sessionState;
            _heightManager = heightManager;

            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeToggleMenu(HandleInputMenuToggle);
            _eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Subscribe<ClientSimOnNewMasterEvent>(OnMasterChange);
            _eventDispatcher.Subscribe<ClientSimReadyEvent>(OnReady);
            _eventDispatcher.Subscribe<ClientSimOnPlayerHeightUpdateEvent>(OnPlayerHeightUpdate);
            _eventDispatcher.Subscribe<ClientSimOnToggleManualScalingEvent>(OnManualScalingToggled);

            playerNameText.text = "";
            playerIdText.text = "";
            isMasterToggle.isOn = false;
            isInstanceOwnerToggle.isOn = settings.isInstanceOwner;

            UpdateValuesFromSettings();
#if UNITY_EDITOR
            UnityEditor.SceneVisibilityManager.instance.Hide(gameObject, true);
#endif
        }

        public ClientSimDisplayedPage GetDisplayedPage()
        {
            return _displayedPage;
        }
        
        private void Start()
        {
            SetUIOverlayMaterial();
            
            bool shouldShowMenu = true;
            if (checkValidSettingsHook?.Invoke() == false)
            {
                // Force open the ClientSim settings window.
                // When first importing ClientSim and disabling legacy input, all UI menus fail to react to the mouse,
                // preventing users from clicking the settings window button.
                OpenSettings();
                SetDisplayedPage(ClientSimDisplayedPage.INVALID_SETTINGS_PAGE);
            }
            else if (_settings.initializationDelay > 0)
            {
                SetDisplayedPage(ClientSimDisplayedPage.DELAYED_START_PAGE);
            }
            else if (_settings.spawnPlayer)
            {
                DisplayInitialPageForPlayer();
            }
            else
            {
                shouldShowMenu = false;
            }
            
            ToggleMenu(shouldShowMenu);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _input?.UnsubscribeToggleMenu(HandleInputMenuToggle);
            
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher?.Unsubscribe<ClientSimOnNewMasterEvent>(OnMasterChange);
            _eventDispatcher?.Unsubscribe<ClientSimReadyEvent>(OnReady);
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerMovedEvent>(OnPlayerMoved);
            _eventDispatcher?.Unsubscribe<ClientSimOnToggleManualScalingEvent>(OnManualScalingToggled);
        }

        private void Update()
        {
            // Only update the menu settings while the menu is displayed.
            if (_menuIsActive)
            {
                UpdateValuesFromSettings();
            }
        }

        private void SetUIOverlayMaterial()
        {
            int propertyId = Shader.PropertyToID(GUI_ZTEST_MODE_PROPERTY_NAME);
            
            // Solution provided from Unity forums:
            // https://answers.unity.com/questions/878667/world-space-canvas-on-top-of-everything.html
            Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
            Dictionary<Material, Material> newMaterialMapping = new Dictionary<Material, Material>();
            foreach (var graphic in graphics)
            {
                Material mat = graphic.materialForRendering;
                if (mat == null)
                {
                    continue;
                }
                if (!newMaterialMapping.TryGetValue(mat, out Material updatedMaterial))
                {
                    updatedMaterial = new Material(mat);
                    newMaterialMapping.Add(mat, updatedMaterial);
                }
                updatedMaterial.SetInt(propertyId, (int)UnityEngine.Rendering.CompareFunction.Always);
                graphic.material = updatedMaterial;
            }
        }

        private void SetDisplayedPage(ClientSimDisplayedPage page)
        {
            _displayedPage = page;
            
            pauseMenu.SetActive(_displayedPage == ClientSimDisplayedPage.PAUSE_MENU);
            warningsPage.SetActive(_displayedPage == ClientSimDisplayedPage.WARNING_PAGE);
            delayStartPage.SetActive(_displayedPage == ClientSimDisplayedPage.DELAYED_START_PAGE);
            invalidSettingsPage.SetActive(_displayedPage == ClientSimDisplayedPage.INVALID_SETTINGS_PAGE);
        }

        private void DisplayInitialPageForPlayer()
        {
            SetDisplayedPage(_sessionState.GetBool(HAS_USER_ACCEPTED_WARNING)
                ? ClientSimDisplayedPage.PAUSE_MENU
                : ClientSimDisplayedPage.WARNING_PAGE);
        }
        
        private void UpdateValuesFromSettings()
        {
            tooltipsToggle.isOn = _settings.showTooltips;
            reticleToggle.isOn = _settings.showDesktopReticle;
            invertMouseToggle.isOn = _settings.invertMouseLook;
            consoleLoggingToggle.isOn = _settings.displayLogs;

            float playerHeight = _heightManager.GetAvatarEyeHeightAsMetersClamped();
            if (!Mathf.Approximately(playerHeight, playerHeightSlider.value))
            {
                // the player height slider obeys manual scale restrictions as if through the radial menu
                ClampPlayerHeightSliderBounds();
                
                playerHeightSlider.value = playerHeight;
                playerHeightText.text = playerHeight.ToString("F2");
                
                _eventDispatcher.SendEvent(new ClientSimOnPlayerHeightUpdateEvent { playerHeight = playerHeight });
            }
        }

        private void ClampPlayerHeightSliderBounds()
        {
            playerHeightSlider.minValue = _heightManager.GetAvatarEyeHeightMinimumAsMeters();
            playerHeightSlider.maxValue = _heightManager.GetAvatarEyeHeightMaximumAsMeters();
        }
        
        private void SaveSettings()
        {
#if UNITY_EDITOR
            ClientSimSettings.SaveSettings(_settings);
#endif
        }
        
        private void ToggleMenu(bool isActive)
        {
            _menuIsActive = isActive;
            menu.SetActive(isActive);

            // toggle internal UI camera stack to improve menu performance
            if (_menuCanvas.worldCamera != null)
                _menuCanvas.worldCamera.enabled = isActive;
            
            _eventDispatcher.SendEvent(new ClientSimMenuStateChangedEvent { isMenuOpen = _menuIsActive });

            if (_menuIsActive)
            {
                _eventDispatcher.Subscribe<ClientSimOnPlayerMovedEvent>(OnPlayerMoved);
                UpdateCanvasLocation();
                
                // If the user sets the player height value too large through the Settings window,
                // toggling the menu will clamp the max range to make it more usable again.
                ClampPlayerHeightSliderBounds();
            }
            else
            {
                _eventDispatcher.Unsubscribe<ClientSimOnPlayerMovedEvent>(OnPlayerMoved);
            }
        }

        // TODO update position based on tracking type. Desktop should always be in front of the camera and VR
        // should be stationary relative to the playspace position.
        private void UpdateCanvasLocation()
        {
            Camera cam = _menuCanvas.worldCamera;
            
            // Always use main camera to position the menu if the camera is missing or not enabled.
            if (cam == null || !cam.enabled || !cam.gameObject.activeInHierarchy)
            {
                cam = Camera.main;
            }

            // TODO handle missing camera better
            if (cam == null)
            {
                return;
            }
            
            Transform camTransform = cam.transform;
            menu.transform.localScale = camTransform.lossyScale * menuScaleFactor;
            
            Vector3 position = camTransform.TransformPoint(Vector3.forward * 2);
            menu.transform.SetPositionAndRotation(position, camTransform.rotation);
        }

#region ClientSim Input
        
        private void HandleInputMenuToggle(bool value, HandType hand)
        {
            // Only handle menu input down, and not on release.
            if (!value)
            {
                return;
            }
            
            // Users can only change the enabled state when the current page is the normal pause menu.
            if (_displayedPage != ClientSimDisplayedPage.PAUSE_MENU)
            {
                return;
            }
            
            // Player is not active, do not allow changing the state of the menu. 
            if (!_settings.spawnPlayer)
            {
                return;
            }
            
#if ENABLE_INPUT_SYSTEM
            // Ignore pressing escape to toggle the menu off. Due to Unity using the escape key as a special key to
            // remove focus from the game window, it is impossible to recapture it.
            if (_menuIsActive && (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame ?? false))
            {
                return;
            }
#endif
            
            ToggleMenu(!_menuIsActive);
        }

#endregion

#region ClientSim Events

        private void OnReady(ClientSimReadyEvent readyEvent)
        {
            // Disable the menu if the player is not spawned, and the displayed page isn't invalid.
            if (!_settings.spawnPlayer && _displayedPage != ClientSimDisplayedPage.INVALID_SETTINGS_PAGE)
            {
                ToggleMenu(false);
                return;
            }
            
            if (_displayedPage == ClientSimDisplayedPage.DELAYED_START_PAGE)
            {
                DisplayInitialPageForPlayer();
                UpdateCanvasLocation();
            }
        }
        
        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent joinEvent)
        {
            VRCPlayerApi player = joinEvent.player;
            if (!player.isLocal)
            {
                return;
            }

            isMasterToggle.isOn = player.isMaster;
            playerNameText.text = player.displayName;
            playerIdText.text = player.playerId.ToString();
        }

        private void OnMasterChange(ClientSimOnNewMasterEvent masterEvent)
        {
            isMasterToggle.isOn = Networking.IsMaster;
        }

        private void OnPlayerMoved(ClientSimOnPlayerMovedEvent movedEvent)
        {
            UpdateCanvasLocation();
        }

        private void OnPlayerHeightUpdate(ClientSimOnPlayerHeightUpdateEvent heightEvent)
        {
            // if height was set programatically and exceeds a manual scaling limit, set slider to the exceeded limit 
            if (heightEvent.exceedsManualScalingMinimum)
                playerHeightSlider.SetValueWithoutNotify(_heightManager.GetAvatarEyeHeightMinimumAsMeters());
            
            else if (heightEvent.exceedsManualScalingMaximum)
                playerHeightSlider.SetValueWithoutNotify(_heightManager.GetAvatarEyeHeightMaximumAsMeters());
            
            else
                playerHeightSlider.SetValueWithoutNotify(heightEvent.playerHeight);
            
            playerHeightText.text = heightEvent.playerHeight.ToString("F2");
            ClientSimSettings.Instance.SetInitialPlayerHeight(heightEvent.playerHeight);
        }

        private void OnManualScalingToggled(ClientSimOnToggleManualScalingEvent toggleEvent)
        {
            playerHeightSlider.interactable = toggleEvent.manualScalingAllowed;
        }

#endregion

#region UI Hooks

        [PublicAPI]
        public void WarningAccepted()
        {
            CloseMenu();
            SetDisplayedPage(ClientSimDisplayedPage.PAUSE_MENU);
            _sessionState.SetBool(HAS_USER_ACCEPTED_WARNING, true);
        }

        [PublicAPI]
        public void OpenMenu()
        {
            ToggleMenu(true);
        }
        
        [PublicAPI]
        public void CloseMenu()
        {
            ToggleMenu(false);
        }

        [PublicAPI]
        public void Respawn()
        {
            CloseMenu();
            _eventDispatcher.SendEvent(new ClientSimMenuRespawnClickedEvent());
        }

        [PublicAPI]
        public void OpenSettings()
        {
            openSettingsHook?.Invoke();
        }

        [PublicAPI]
        public void ExitPlaymode()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }

        [PublicAPI]
        public void SpawnRemotePlayer()
        {
            ClientSimMain.SpawnRemotePlayer();
        }

        [PublicAPI]
        public void UpdatePlayerHeight(float playerHeight)
        {
            if (!_heightManager.GetManualAvatarScalingAllowed())
            {
                return;
            }
            
            if (Mathf.Approximately(_heightManager.GetAvatarEyeHeightAsMetersClamped(), playerHeight))
            {
                return;
            }

            _heightManager.SetAvatarEyeHeightByMeters(playerHeight, true);
            SaveSettings();
        }
        
        [PublicAPI]
        public void UpdateShowTooltips(bool showTooltips)
        {
            if (_settings.showTooltips == showTooltips)
            {
                return;
            }
            
            _settings.showTooltips = showTooltips;
            SaveSettings();
        }
        
        [PublicAPI]
        public void UpdateShowReticle(bool showReticle)
        {
            if (_settings.showDesktopReticle == showReticle)
            {
                return;
            }
            
            _settings.showDesktopReticle = showReticle;
            SaveSettings();
        }
        
        [PublicAPI]
        public void UpdateInvertMouseLook(bool invertMouseLook)
        {
            if (_settings.invertMouseLook == invertMouseLook)
            {
                return;
            }
            
            _settings.invertMouseLook = invertMouseLook;
            SaveSettings();
        }
        
        [PublicAPI]
        public void UpdateConsoleLogging(bool consoleLogging)
        {
            if (_settings.displayLogs == consoleLogging)
            {
                return;
            }
            
            _settings.displayLogs = consoleLogging;
            SaveSettings();
        }

#endregion
    }
}