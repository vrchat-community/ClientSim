#if UNITY_EDITOR
using UnityEditor;
using VRC.Core;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string AVAILABLE_LANGUAGES_CODES_KEY = "availableLanguageCodes";
        private static readonly Dictionary<string, string> LanguageByCode = new()
        {
            { "en", "English" },
            { "fr", "French" },
            { "de", "German" },
            { "it", "Italian" },
            { "ja", "Japanese" },
            { "ko", "Korean" },
            { "es", "Spanish" },
            { "pt", "Portuguese" },
            { "pt-BR", "Brazilian" },
            { "he", "Hebrew" },
            { "pl", "Polish" },
            { "tok", "Toki Pona" },
            { "id", "Indonesian" },
            { "zh-CN", "Chinese Simplified" },
            { "zh-HK", "Chinese Traditional" },
            { "ru", "Russian" },
            { "sv", "Swedish" },
            { "nl", "Dutch" },
            { "uk", "Ukrainian" },
            { "da", "Danish" },
            { "no", "Norwegian" },
            { "th", "Thai" },
            { "fi", "Finnish" },
            { "hu", "Hungarian" },
            { "cs", "Czech" },
            { "tr", "Turkish" },
            { "ar", "Arabic" },
            { "ro", "Romanian" },
            { "vi", "Vietnamese" }
        };
        private static readonly Dictionary<string, string> CodeByLanguage = new()
        {
            { "English", "en" },
            { "French", "fr" },
            { "German", "de" },
            { "Italian", "it" },
            { "Japanese", "ja" },
            { "Korean", "ko" },
            { "Spanish", "es" },
            { "Portuguese", "pt" },
            { "Brazilian", "pt-BR" },
            { "Hebrew", "he" },
            { "Polish", "pl" },
            { "Toki Pona", "tok" },
            { "Indonesian", "id" },
            { "Chinese Simplified", "zh-CN" },
            { "Chinese Traditional", "zh-HK" },
            { "Russian", "ru" },
            { "Swedish", "sv" },
            { "Dutch", "nl" },
            { "Ukrainian", "uk" },
            { "Danish", "da" },
            { "Norwegian", "no" },
            { "Thai", "th" },
            { "Finnish", "fi" },
            { "Hungarian", "hu" },
            { "Czech", "cs" },
            { "Turkish", "tr" },
            { "Arabic", "ar" },
            { "Romanian", "ro" },
            { "Vietnamese", "vi" }
        };
        
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
        public float playerStartHeight = ClientSimTrackingProviderBase.AVATAR_HEIGHT; // Default avatar height is 1.9 units tall
        
        public string currentLanguage = "en";
        public string[] availableDisplayLanguages = Array.Empty<string>();
        public string[] availableLanguages = Array.Empty<string>();
        
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

        public string GetLanguage(int languageIndex)
        {
            if (!LanguagesInitialized())
            {
                if (!TryInitLanguages()) 
                    return "en";
            }

            if (languageIndex > availableLanguages.Length)
                return "en";
            
            return availableLanguages[languageIndex];
        }

        public string[] GetAvailableDisplayLanguages()
        {
            if (!LanguagesInitialized())
            {
                if (!TryInitLanguages())
                    return new[] { "English" };
            }
            
            return availableDisplayLanguages;
        }
        
        public string[] GetAvailableLanguages()
        {
            if (!LanguagesInitialized())
            {
                if (!TryInitLanguages())
                    return new[] { "en" };
            }
            
            return availableLanguages;
        }

        private bool TryInitLanguages()
        {
            if (ConfigManager.RemoteConfig.IsInitialized())
            {
                availableLanguages = ConfigManager.RemoteConfig.GetList(AVAILABLE_LANGUAGES_CODES_KEY).ToArray();
                availableDisplayLanguages = availableLanguages.Where(code => LanguageByCode.ContainsKey(code)).Select(code => LanguageByCode[code]).ToArray();
                return true;
            }

            ConfigManager.RemoteConfig.Init();
            return false;
        }

        private bool LanguagesInitialized()
        {
            // if the remote config isn't available, use cached languages if available
            if (availableLanguages.Length == 0 && availableDisplayLanguages.Length > 0)
            {
                availableLanguages = availableDisplayLanguages.Where(lang => CodeByLanguage.ContainsKey(lang)).Select(lang => CodeByLanguage[lang]).ToArray();
            }
            
            return availableLanguages.Length > 0 && availableDisplayLanguages.Length > 0;
        }

#endif

        public void SetInitialPlayerHeight(float height)
        {
            playerStartHeight = height;
        }
    }
}