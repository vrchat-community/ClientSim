
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.InputSystem;

namespace VRC.SDK3.ClientSim.Editor
{
    public class ClientSimSettingsWindow : EditorWindow
    {
        // General content
        private readonly GUIContent _generalFoldoutGuiContent = new GUIContent("General Settings", "");
        private readonly GUIContent _enableToggleGuiContent = new GUIContent("Enable ClientSim", "If enabled, all triggers will function similarly to VRChat. Note that behavior may be different than the actual game!");
        private readonly GUIContent _displayLogsToggleGuiContent = new GUIContent("Enable Console Logging", "Enabling logging will print messages to the console when certain events happen. Examples include trigger execution, pickup grabbed, station entered, etc.");
        private readonly GUIContent _deleteEditorOnlyToggleGuiContent = new GUIContent("Remove \"EditorOnly\"", "Enabling this setting will ensure that all objects with the tag \"EditorOnly\" are deleted when in playmode. This can be helpful in finding objects that will not be uploaded with your world. Enable console logging to see which objects are deleted.");
        private readonly GUIContent _startupDelayGuiContent = new GUIContent("Startup Delay", "The duration that the Client Sim will wait to simulate the VRChat client loading before spawning the player and initializing Udon. This is useful to test when Unity components behave differently at startup compared to VRChat.");
        private readonly GUIContent _stopOnScriptChangesToggleGuiContent = new GUIContent("Stop On Script Changes", "If enabled, the editor will stop if script changes are detected while in play mode. This will override the Unity Editor setting 'Preferences > General > Script Changes While Playing'.");

        private readonly GUIContent _setTargetFrameRateGuiContent = new GUIContent("Set Target FrameRate", "Should ClientSim set the target framerate on startup? This will automatically set the physics delta time to match expected framerate. Disabling this setting is useful when profiling.");
        private readonly GUIContent _targetFrameRateGuiContent = new GUIContent("Target FrameRate", "The target framerate unity should aim for. Default is 90 fps.");
        
        
        // Player Controller content
        private readonly GUIContent _playerControllerFoldoutGuiContent = new GUIContent("Player Controller Settings", "");
        private readonly GUIContent _playerControllerToggleGuiContent = new GUIContent("Spawn Player Controller", "If enabled, a player controller will spawn and allow you to move around your world as if in desktop mode. Supports interacts and pickups.");

        private readonly GUIContent _showDesktopReticleGuiContent = new GUIContent("Show Desktop Reticle", "Show or hide the center Desktop reticle image.");
        private readonly GUIContent _showTooltipsGuiContent = new GUIContent("Show Tooltips", "If enabled, hovering over an interactable object or pickup will display a tooltip above the object.");
        private readonly GUIContent _invertMouseLookGuiContent = new GUIContent("Invert Mouse Look", "If enabled, moving the mouse up or down will invert the direction the player will look up and down.");
        private readonly GUIContent _playerHeightGuiContent = new GUIContent("Player Height", "How tall should the player be in meters. Default height is 1.9. Note that the player's collision capsule is 1.6 and never changes.");
        private readonly GUIContent _currentLanguageGuiContent = new GUIContent("Current Language", "The language the player is currently using. Available languages include English, French, German, Italian, Japanese, Korean, and Spanish.");
        private int selectedLanguageIndex;
        
        // Player settings
        private readonly GUIContent _playerButtonsFoldoutGuiContent = new GUIContent("Player Settings", "");
        private readonly GUIContent _localPlayerCustomNameGuiContent = new GUIContent("Local Player Name", "Set a custom name for the local player. Useful for testing udon script name detection");
        private readonly GUIContent _isMasterGuiContent = new GUIContent("Local Player Is Master", "Set whether the local player starts off as the master of the instance. Setting this to false and starting Client Sim will spawn a remote player before the local player.");
        private readonly GUIContent _isInstanceOwnerGuiContent = new GUIContent("Is Instance Owner", "Set whether the local player is considered the instance owner");
        private readonly GUIContent _remotePlayerCustomNameGuiContent = new GUIContent("Remote Player Name", "Set a custom name for the next spawned remote player. Useful for testing udon script name detection");
        
        private const int WARNING_ICON_SIZE = 60;
        
        private static Texture2D _warningIcon;
        private static ClientSimSettings _settings;
        
        private Vector2 _scrollPosition;
        private GUIStyle _boxStyle;
        private GUIStyle _multilineLabel;
        private bool _showGeneralSettings = true;
        private bool _showPlayerControllerSettings = true;
        private bool _showPlayerButtons = true;

        private string _version;
        private string _remotePlayerCustomName = "";

        private bool _needsInputSetup = false;
        private bool _needsInputManagerSetup = false;
        private bool _needsAudioSetup = false;
        private bool _needsLayerSetup = false;

        [MenuItem("VRChat SDK/Utilities/ClientSim")]
        public static void Init()
        {
            ClientSimSettingsWindow window = GetWindow<ClientSimSettingsWindow>(false, "ClientSim Settings");
            window.Show();
        }

        private void OnEnable()
        {
            if (_settings == null)
            {
                _settings = ClientSimSettings.Instance;
            }

            if (_warningIcon == null)
            {
                // Reuse VRChat's warning icon.
                _warningIcon = Resources.Load<Texture2D>("2FAIcons/SDK_Warning_Triangle_icon");
            }
            
            _version = ClientSimResourceLoader.GetVersion();
        }

        private void OnFocus()
        {
            // Verify settings to know if we need to display "Do It" buttons
            _needsInputSetup = !ClientSimProjectSettingsSetup.IsUsingCorrectInputAxesSettings();
            _needsInputManagerSetup = !ClientSimProjectSettingsSetup.IsUsingCorrectInputTypeSettings();
            _needsAudioSetup = !ClientSimProjectSettingsSetup.IsUsingCorrectAudioSettings();
            
            // VRChat layer setup
            _needsLayerSetup = !UpdateLayers.AreLayersSetup() || !UpdateLayers.IsCollisionLayerMatrixSetup();
        }

        void OnGUI()
        {
            float tempLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 175;
            
            _boxStyle = new GUIStyle(EditorStyles.helpBox);
            _multilineLabel = new GUIStyle(EditorStyles.label);
            _multilineLabel.wordWrap = true;
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();

            DrawWindow();
            
            DrawFooter();
            
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.labelWidth = tempLabelWidth;
        }

        private void DrawWindow()
        {
            if (_needsAudioSetup || _needsInputSetup || _needsInputManagerSetup || _needsLayerSetup)
            {
                DrawDoItButtons();
                return;
            }
            
            EditorGUI.BeginChangeCheck();

            // Disables UI if ClientSim is disabled
            DrawGeneralSettings();

            DrawPlayerControllerSettings();
            
            DrawPlayerButtons();
            
            // Disable group from General settings
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck())
            {
                ClientSimSettings.SaveSettings(_settings);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            DrawVersion();
        }
        
        private void DrawVersion()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Version: " + _version);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDoItButtons()
        {
            EditorGUILayout.BeginVertical(_boxStyle);

            // Display a warning icon informing them of project setting issues.
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            EditorGUILayout.BeginHorizontal();
            
            string content = "You must address the following issues before you can test this content using ClientSim!";
            if (Application.isPlaying)
            {
                content += "\nPlease exit playmode before applying these settings.";
            }
            
            GUILayout.Label(new GUIContent(_warningIcon), GUILayout.Width(WARNING_ICON_SIZE), GUILayout.Height(WARNING_ICON_SIZE));
            EditorGUILayout.LabelField(content, labelStyle, GUILayout.Height(WARNING_ICON_SIZE));
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            
            DrawAudioSettingsDoIt();

            DrawInputAxesSettingsDoIt();

            DrawInputManagerSettingsDoIt();

            DrawLayerSettingsSection();
            
            EditorGUI.EndDisabledGroup();
        }

        private void DrawAudioSettingsDoIt()
        {
            if (!_needsAudioSetup)
            {
                return;
            }

            BeginWarningArea();
            
            GUILayout.Label("Audio Spatializer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label("VRChat uses an audio spatializer that is different from the default Unity spatializer. Clicking this button will modify the project's audio settings to use this audio spatializer.", _multilineLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Set Audio Spatializer"))
            {
                bool doIt = EditorUtility.DisplayDialog("Set Audio Spatializer for ClientSim",
                    "This will modify the project's audio settings to use the  audio spatializer. Are you sure you want to continue?",
                    "Do it!", "Don't do it");
                if (doIt)
                {
                    ClientSimProjectSettingsSetup.SetAudioSettings();
                    _needsAudioSetup = false;
                }
            }
            
            EndWarningArea();
        }
        
        private void DrawInputAxesSettingsDoIt()
        {
            if (!_needsInputSetup)
            {
                return;
            }
            
            BeginWarningArea();

            GUILayout.Label("Input Axes Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label("VRChat uses a custom list of Input Axes. This will allow you to test these Input Axes in Udon. Clicking this button will replace this project's input axes with VRChat's and remove any custom axes added by the user.", _multilineLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Apply VRChat Input Axes"))
            {
                bool doIt = EditorUtility.DisplayDialog("Set Input Axes for ClientSim",
                    "This will replace this project's input axes with VRChat's. Any custom input axes will be removed. Are you sure you want to continue?",
                    "Do it!", "Don't do it");
                if (doIt)
                {
                    ClientSimProjectSettingsSetup.ApplyClientSimInputAxes();
                    _needsInputSetup = false;
                }
            }

            EndWarningArea();
        }
        
        private void DrawInputManagerSettingsDoIt()
        {
            if (!_needsInputManagerSetup)
            {
                return;
            }
            
            BeginWarningArea();

            GUILayout.Label("Input Manager Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label("VRChat and ClientSim use both the legacy Input Manager and the new Input System package. Without this setting, input will not work in playmode for ClientSim or Udon. Clicking this button will update the project settings to use both input systems and then *RESTART* Unity to apply the changes.", _multilineLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Set Input Manager"))
            {
                bool doIt = EditorUtility.DisplayDialog("Set Input Manager for ClientSim",
                    "This will update the project settings to use both the legacy Input Manager and the new Input System package. Clicking \"Do it!\" will also *RESTART* Unity to apply the changes. Are you sure you want to continue?",
                    "Do it!", "Don't do it");
                if (doIt)
                {
                    ClientSimProjectSettingsSetup.SetInputTypeSettings();
                    _needsInputManagerSetup = false;

                    // After importing the new input system, a dialog is displayed to enable it and disable the old.
                    // This method is then called after to restart unity and recompile code.
                    // Since the class is internal, Reflection is needed to call the method.
                    var inputAssembly = typeof(InputSystem).Assembly;
                    var editorHelpersType = inputAssembly.GetType("UnityEngine.InputSystem.Editor.EditorHelpers");
                    var restartEditorMethod = editorHelpersType.GetMethod("RestartEditorAndRecompileScripts",
                        BindingFlags.Public | BindingFlags.Static);
                    restartEditorMethod.Invoke(null, new object[] { false });
                }
            }
            
            EndWarningArea();
        }

        private void DrawLayerSettingsSection()
        {
            if (!_needsLayerSetup)
            {
                return;
            }
            
            BeginWarningArea();

            GUILayout.Label("Layer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            GUILayout.Label("VRChat scenes must have the same Unity layer configuration as VRChat. Please see the VRChat Build Control Panel to setup the project's layers and collision matrix.", _multilineLabel);
            
            // TODO create button to open build control panel.
            
            EndWarningArea();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            _showGeneralSettings = EditorGUILayout.Foldout(_showGeneralSettings, _generalFoldoutGuiContent, true);

            if (_showGeneralSettings)
            {
                AddIndent();
                
                if (_settings.enableClientSim && FindObjectOfType<VRC_SceneDescriptor>() == null)
                {
                    EditorGUILayout.HelpBox("No VRC_SceneDescriptor in scene. Please add one to enable ClientSim.", MessageType.Warning);
                }
                if (_settings.enableClientSim && Application.isPlaying && !ClientSimMain.HasInstance())
                {
                    EditorGUILayout.HelpBox("Please exit and re-enter playmode to enable ClientSim!", MessageType.Warning);
                }

                _settings.enableClientSim = EditorGUILayout.Toggle(_enableToggleGuiContent, _settings.enableClientSim);

                EditorGUI.BeginDisabledGroup(!_settings.enableClientSim);

                _settings.displayLogs = EditorGUILayout.Toggle(_displayLogsToggleGuiContent, _settings.displayLogs);

                _settings.stopOnScriptChanges = EditorGUILayout.Toggle(_stopOnScriptChangesToggleGuiContent, _settings.stopOnScriptChanges);


                // Settings that cannot be changed at runtime
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                
                _settings.deleteEditorOnly = EditorGUILayout.Toggle(_deleteEditorOnlyToggleGuiContent, _settings.deleteEditorOnly);
                
                _settings.setTargetFrameRate = EditorGUILayout.Toggle(_setTargetFrameRateGuiContent, _settings.setTargetFrameRate);
                
                EditorGUI.BeginDisabledGroup(!_settings.setTargetFrameRate);
                _settings.targetFrameRate = EditorGUILayout.IntField(_targetFrameRateGuiContent, _settings.targetFrameRate);
                _settings.targetFrameRate = Mathf.Max(1, _settings.targetFrameRate);
                EditorGUI.EndDisabledGroup();
                
                _settings.initializationDelay = EditorGUILayout.FloatField(_startupDelayGuiContent, _settings.initializationDelay);
                _settings.initializationDelay = Mathf.Max(0, _settings.initializationDelay);
                
                EditorGUI.EndDisabledGroup();

                RemoveIndent();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawPlayerControllerSettings()
        {
            EditorGUILayout.BeginVertical(_boxStyle);

            _showPlayerControllerSettings = EditorGUILayout.Foldout(_showPlayerControllerSettings, _playerControllerFoldoutGuiContent, true);
            if (_showPlayerControllerSettings)
            {
                AddIndent();

                _settings.spawnPlayer = EditorGUILayout.Toggle(_playerControllerToggleGuiContent, _settings.spawnPlayer);

                EditorGUI.BeginDisabledGroup(!_settings.spawnPlayer);
                
                _settings.showDesktopReticle = EditorGUILayout.Toggle(_showDesktopReticleGuiContent, _settings.showDesktopReticle);
                _settings.showTooltips = EditorGUILayout.Toggle(_showTooltipsGuiContent, _settings.showTooltips);
                _settings.invertMouseLook = EditorGUILayout.Toggle(_invertMouseLookGuiContent, _settings.invertMouseLook);
                _settings.playerHeight = EditorGUILayout.FloatField(_playerHeightGuiContent, _settings.playerHeight);
                _settings.playerHeight = Mathf.Clamp(_settings.playerHeight, 0.2f, 80f); // TODO make consts for these.
                selectedLanguageIndex = EditorGUILayout.Popup(_currentLanguageGuiContent, selectedLanguageIndex, _settings.availableLanguages);
                _settings.currentLanguage = _settings.availableLanguages[selectedLanguageIndex];

                EditorGUI.EndDisabledGroup();

                RemoveIndent();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawPlayerButtons()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            _showPlayerButtons = EditorGUILayout.Foldout(_showPlayerButtons, _playerButtonsFoldoutGuiContent, true);
            if (_showPlayerButtons)
            {
                AddIndent();

                bool hasInstance = ClientSimMain.HasInstance();
                
                // Values cannot change once ClientSim has started.
                EditorGUI.BeginDisabledGroup(hasInstance || Application.isPlaying);
                
                _settings.customLocalPlayerName = EditorGUILayout.TextField(_localPlayerCustomNameGuiContent, _settings.customLocalPlayerName);
                _settings.localPlayerIsMaster = EditorGUILayout.Toggle(_isMasterGuiContent, _settings.localPlayerIsMaster);
                _settings.isInstanceOwner = EditorGUILayout.Toggle(_isInstanceOwnerGuiContent, _settings.isInstanceOwner);
                
                // TODO display desktop/vr option here
                
                EditorGUI.EndDisabledGroup();
                
                
                EditorGUI.BeginDisabledGroup(!hasInstance || !Application.isPlaying);

                _remotePlayerCustomName = EditorGUILayout.TextField(_remotePlayerCustomNameGuiContent, _remotePlayerCustomName);

                if (GUILayout.Button("Spawn Remote Player"))
                {
                    ClientSimMain.SpawnRemotePlayer(_remotePlayerCustomName);
                }

                List<VRCPlayerApi> playersToRemove = new List<VRCPlayerApi>();
                if (Application.isPlaying)
                {
                    foreach (var player in VRCPlayerApi.AllPlayers)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(player.displayName);
                        GUILayout.Space(5);

                        EditorGUI.BeginDisabledGroup(VRCPlayerApi.AllPlayers.Count == 1 || player.isLocal);

                        if (GUILayout.Button("Remove Player"))
                        {
                            playersToRemove.Add(player);
                        }

                        EditorGUI.EndDisabledGroup();

                        GUILayout.EndHorizontal();
                    }

                    for (int i = playersToRemove.Count - 1; i >= 0; --i)
                    {
                        ClientSimMain.RemovePlayer(playersToRemove[i]);
                    }
                    playersToRemove.Clear();
                }

                EditorGUI.EndDisabledGroup();

                RemoveIndent();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void AddIndent()
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 7 + 4);
            EditorGUILayout.BeginVertical();
        }

        private void RemoveIndent()
        {
            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void BeginWarningArea()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
        }

        private void EndWarningArea()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5);
        }
    }
}
