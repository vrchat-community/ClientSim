#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimSettings
    {
        private static ClientSimSettings instance_;
        public static ClientSimSettings Instance
        {
            get
            {
                if (instance_ == null)
                {
#if UNITY_EDITOR
                    instance_ = LoadSettings();
#endif
                }
                return instance_;
            }
        }

        private const string CLIENT_SIM_SETTINGS_PREFS_STRING = "com.vrchat.clientsim.settings";

        [SerializeField] public bool displaySettingsWindowAtLaunch = true;

        [SerializeField] public KeyCode crouchKey = KeyCode.C;
        [SerializeField] public KeyCode proneKey = KeyCode.Z;
        [SerializeField] public KeyCode runKey = KeyCode.LeftShift;

        [SerializeField] public string customLocalPlayerName = "";

        // TODO move settings to be per project instead of global to all
        [SerializeField] public bool enableClientSim = true;
        [SerializeField] public bool displayLogs = true;
        [SerializeField] public bool deleteEditorOnly = true;
        [SerializeField] public bool spawnPlayer = true;
        
        [SerializeField] public bool isInstanceOwner = true;
        
        [SerializeField] public bool showDesktopReticle = true;

#if UNITY_EDITOR
        private static ClientSimSettings LoadSettings()
        {
            ClientSimSettings settings = new ClientSimSettings();

            string data = EditorPrefs.GetString(CLIENT_SIM_SETTINGS_PREFS_STRING, JsonUtility.ToJson(settings, false));

            JsonUtility.FromJsonOverwrite(data, settings);
            return settings;
        }

        public static void SaveSettings(ClientSimSettings settings)
        {
            string data = JsonUtility.ToJson(settings, false);
            EditorPrefs.SetString(CLIENT_SIM_SETTINGS_PREFS_STRING, data);
        }
#endif

    }
}