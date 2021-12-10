
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using System.Collections.Generic;

namespace VRC.SDK3.ClientSim.Editor
{
    public class ClientSimSettingsWindow : EditorWindow
    {
        private const string PACKAGE_PATH = "Packages/com.vrchat.clientsim";

        // General content
        private readonly GUIContent generalFoldoutGuiContent = new GUIContent("General Settings", "");
        private readonly GUIContent enableToggleGuiContent = new GUIContent("Enable ClientSim", "If enabled, all triggers will function similarly to VRChat. Note that behavior may be different than the actual game!");
        private readonly GUIContent displayLogsToggleGuiContent = new GUIContent("Enable Console Logging", "Enabling logging will print messages to the console when certain events happen. Examples include trigger execution, pickup grabbed, station entered, etc.");
        private readonly GUIContent deleteEditorOnlyToggleGuiContent = new GUIContent("Remove \"EditorOnly\"", "Enabling this setting will ensure that all objects with the tag \"EditorOnly\" are deleted when in playmode. This can be helpful in finding objects that will not be uploaded with your world. Enable console logging to see which objects are deleted.");

        // Player Controller content
        private readonly GUIContent playerControllerFoldoutGuiContent = new GUIContent("Player Controller Settings", "");
        private readonly GUIContent playerControllerToggleGuiContent = new GUIContent("Spawn Player Controller", "If enabled, a player controller will spawn and allow you to move around your world as if in desktop mode. Supports interacts and pickups.");
        private readonly GUIContent playerControllerRunKeyGuiContent = new GUIContent("Run Key", "The button used to run. Running will make the player move faster.");
        private readonly GUIContent playerControllerCrouchKeyGuiContent = new GUIContent("Crouch Key", "The button used to crouch. Crouching will lower the camera midway to the floor and slow down the player.");
        private readonly GUIContent playerControllerProneKeyGuiContent = new GUIContent("Prone Key", "The button used to go prone. Going prone will lower the camera closer to the floor and slow down the player.");
        
        // Player settings
        private readonly GUIContent playerButtonsFoldoutGuiContent = new GUIContent("Player Settings", "");
        private readonly GUIContent localPlayerCustomNameGuiContent = new GUIContent("Local Player Name", "Set a custom name for the local player. Useful for testing udon script name detection");
        private readonly GUIContent isInstanceOwnerGuiContent = new GUIContent("Is Instance Owner", "Set whether the local player is considered the instance owner");
        private readonly GUIContent remotePlayerCustomNameGuiContent = new GUIContent("Remote Player Name", "Set a custom name for the next spawned remote player. Useful for testing udon script name detection");
        private readonly GUIContent showDesktopReticleGuiContent = new GUIContent("Show Desktop Reticle", "Show or hide the center Desktop reticle image.");


        private static ClientSimSettings settings_;
        private Vector2 scrollPosition_;
        private GUIStyle boxStyle_;
        private bool showGeneralSettings_ = true;
        private bool showPlayerControllerSettings_ = true;
        private bool showPlayerButtons_ = true;

        private string version_;
        private string remotePlayerCustomName = "";

        public static void TryInitOnLoad()
        {
            settings_ = ClientSimSettings.Instance;
            if (settings_.displaySettingsWindowAtLaunch)
            {
                Init();
            }
        }

        [MenuItem("VRChat SDK/Utilities/ClientSim")]
        static void Init()
        {
            ClientSimSettingsWindow window = (ClientSimSettingsWindow)EditorWindow.GetWindow(typeof(ClientSimSettingsWindow), false, "ClientSim Settings");
            window.Show();
        }

        private void OnEnable()
        {
            if (settings_ == null)
            {
                settings_ = ClientSimSettings.Instance;
            }

            version_ = "";
            var packageInfo = 
                UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PACKAGE_PATH);
            if (packageInfo != null)
            {
                version_ = packageInfo.version;
            }

            if (settings_.displaySettingsWindowAtLaunch)
            {
                settings_.displaySettingsWindowAtLaunch = false;
                ClientSimSettings.SaveSettings(settings_);
            }
        }

        void OnGUI()
        {
            float tempLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 175;
            
            boxStyle_ = new GUIStyle(EditorStyles.helpBox);
            scrollPosition_ = EditorGUILayout.BeginScrollView(scrollPosition_);

            DrawHeader();

            EditorGUI.BeginChangeCheck();

            // Disables UI if ClientSim is disabled
            DrawGeneralSettings();

            DrawPlayerControllerSettings();
            
            DrawPlayerButtons();
            
            // Disable group from General settings
            EditorGUI.EndDisabledGroup();
            

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                ClientSimSettings.SaveSettings(settings_);
            }
            EditorGUIUtility.labelWidth = tempLabelWidth;
        }

        private void DrawHeader()
        {
            DrawTitle();
            GUILayout.Space(5);
        }

        private void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("ClientSim Settings", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version: " + version_);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.BeginVertical(boxStyle_);
            showGeneralSettings_ = EditorGUILayout.Foldout(showGeneralSettings_, generalFoldoutGuiContent, true);

            if (showGeneralSettings_)
            {
                AddIndent();
                
                if (settings_.enableClientSim && FindObjectOfType<VRC_SceneDescriptor>() == null)
                {
                    EditorGUILayout.HelpBox("No VRC_SceneDescriptor in scene. Please add one to enable ClientSim.", MessageType.Warning);
                }
                if (settings_.enableClientSim && Application.isPlaying && !ClientSimMain.HasInstance())
                {
                    EditorGUILayout.HelpBox("Please exit and reenter play mode to enable ClientSim!", MessageType.Warning);
                }

                settings_.enableClientSim = EditorGUILayout.Toggle(enableToggleGuiContent, settings_.enableClientSim);

                EditorGUI.BeginDisabledGroup(!settings_.enableClientSim);

                settings_.displayLogs = EditorGUILayout.Toggle(displayLogsToggleGuiContent, settings_.displayLogs);

                settings_.deleteEditorOnly = EditorGUILayout.Toggle(deleteEditorOnlyToggleGuiContent, settings_.deleteEditorOnly);
                
                RemoveIndent();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerControllerSettings()
        {
            EditorGUILayout.BeginVertical(boxStyle_);

            showPlayerControllerSettings_ = EditorGUILayout.Foldout(showPlayerControllerSettings_, playerControllerFoldoutGuiContent, true);
            if (showPlayerControllerSettings_)
            {
                AddIndent();

                settings_.spawnPlayer = EditorGUILayout.Toggle(playerControllerToggleGuiContent, settings_.spawnPlayer);

                EditorGUI.BeginDisabledGroup(!settings_.spawnPlayer);
                
                settings_.showDesktopReticle = EditorGUILayout.Toggle(showDesktopReticleGuiContent, settings_.showDesktopReticle);

                // key bindings
                settings_.runKey = (KeyCode)EditorGUILayout.EnumPopup(playerControllerRunKeyGuiContent, settings_.runKey);
                settings_.crouchKey = (KeyCode)EditorGUILayout.EnumPopup(playerControllerCrouchKeyGuiContent, settings_.crouchKey);
                settings_.proneKey = (KeyCode)EditorGUILayout.EnumPopup(playerControllerProneKeyGuiContent, settings_.proneKey);
                
                EditorGUI.EndDisabledGroup();

                RemoveIndent();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerButtons()
        {
            EditorGUILayout.BeginVertical(boxStyle_);
            showPlayerButtons_ = EditorGUILayout.Foldout(showPlayerButtons_, playerButtonsFoldoutGuiContent, true);
            if (showPlayerButtons_)
            {
                AddIndent();
                
                // custom name
                settings_.customLocalPlayerName = EditorGUILayout.TextField(localPlayerCustomNameGuiContent, settings_.customLocalPlayerName);
                
                settings_.isInstanceOwner = EditorGUILayout.Toggle(isInstanceOwnerGuiContent, settings_.isInstanceOwner);
                
                // TODO have setting for spawning players in the room before you

                EditorGUI.BeginDisabledGroup(!ClientSimMain.HasInstance() || !Application.isPlaying);

                remotePlayerCustomName = EditorGUILayout.TextField(remotePlayerCustomNameGuiContent, remotePlayerCustomName);

                if (GUILayout.Button("Spawn Remote Player"))
                {
                    ClientSimMain.SpawnPlayer(false, remotePlayerCustomName);
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
    }
}
