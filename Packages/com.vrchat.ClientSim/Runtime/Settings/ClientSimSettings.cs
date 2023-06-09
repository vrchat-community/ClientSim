#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimSettings
    {
        private static ClientSimSettings _instance;
        public static ClientSimSettings Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    _instance = LoadSettings();
#endif
                }
                return _instance;
            }
        }

        private const string SETTINGS_PREFS_STRING = "com.vrchat.clientsim.settings";

        public string customLocalPlayerName = "";

        // TODO move settings to be per project instead of global to all
        public bool enableClientSim = true;
        public bool displayLogs = true;
        public bool deleteEditorOnly = true;
        public bool spawnPlayer = true;

        public bool setTargetFrameRate = true;
        public int targetFrameRate = 90;

        public bool stopOnScriptChanges = true;

        public bool isInstanceOwner = true;
        public bool localPlayerIsMaster = true;
        public float initializationDelay = 0.0f;
        
        public bool showDesktopReticle = true;
        public bool showTooltips = true;
        public bool invertMouseLook = false;
        public float playerHeight = ClientSimTrackingProviderBase.AVATAR_HEIGHT; // Default avatar height is 1.9 units tall
        public string currentLanguage = "English";
        public readonly string[] availableLanguages =
            { "English", "French", "German", "Italian", "Japanese", "Korean", "Spanish" };

#if UNITY_EDITOR
        private static ClientSimSettings LoadSettings()
        {
            ClientSimSettings settings = new ClientSimSettings();

            string data = EditorPrefs.GetString(SETTINGS_PREFS_STRING, JsonUtility.ToJson(settings, false));

            JsonUtility.FromJsonOverwrite(data, settings);
            return settings;
        }

        public static void SaveSettings(ClientSimSettings settings)
        {
            string data = JsonUtility.ToJson(settings, false);
            EditorPrefs.SetString(SETTINGS_PREFS_STRING, data);
        }
#endif

    }
}