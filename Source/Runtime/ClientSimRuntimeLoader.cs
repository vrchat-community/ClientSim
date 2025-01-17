using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimRuntimeLoader
    {
        private const string EDITOR_ONLY_TAG = "EditorOnly";
        
        // Used in tests to prevent the runtime initialized methods from executing.
        private static bool _isInTestMode = false;
        private static ClientSimSettings _testSettingsOverride;
        private static ClientSimEventDispatcher _testEventDispatcherOverride;

        #region ClientSim Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            StartClientSim(GetSettings(), GetEventDispatcher());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            // Delete all editor only objects before creating ClientSim.
            DestroyEditorOnly(GetSettings());
            
#if VRC_ENABLE_PLAYER_PERSISTENCE
            ClientSimMain.GetInstance().EnablePlayerObjects();
#endif
        }

        #endregion

        #region Test Methods

        [PublicAPI]
        public static void BeginUnityTesting(
            ClientSimSettings testSettingsOverride,
            ClientSimEventDispatcher testEventDispatcherOverride = null)
        {
            _isInTestMode = true;
            _testSettingsOverride = testSettingsOverride;
            _testEventDispatcherOverride = testEventDispatcherOverride;
            
            if (_testSettingsOverride == null)
            {
                _testSettingsOverride = new ClientSimSettings();
            }
        }
        
        [PublicAPI]
        public static void EndUnityTesting()
        {
            _isInTestMode = false;
            _testSettingsOverride = null;
        }
        
        [PublicAPI]
        public static bool IsInUnityTest()
        {
            return _isInTestMode;
        }
        
        #endregion

        private static ClientSimSettings GetSettings()
        {
            return IsInUnityTest() ? _testSettingsOverride : ClientSimSettings.Instance;
        }
        
        private static ClientSimEventDispatcher GetEventDispatcher()
        {
            return IsInUnityTest() ? _testEventDispatcherOverride : null;
        }
        
        private static bool IsClientSimEnabled(ClientSimSettings settings)
        {
            return 
                settings.enableClientSim &&
                Application.isPlaying;
        }

        // Start client sim with the given settings.
        // Optional event dispatcher can be passed in to listen to startup events. Mainly used in tests.
        public static void StartClientSim(
            ClientSimSettings settings,
            IClientSimEventDispatcher eventDispatcher = null)
        {
            if (!IsClientSimEnabled(settings))
            {
                return;
            }
            
            // Delete all editor only objects before creating ClientSim.
            DestroyEditorOnly(settings);

            ClientSimMain.CreateInstance(settings, eventDispatcher);

            // TODO: Below is disabled for now because the rest of the ClientSim initialization code doesn't work if it's called with a delay.
            // Currently, not loading RemoteConfig will not cause any issues, but it may in the future, so this is left in as a reminder.

            // Create ClientSim Instance later
            /*void CreateClientSimInstance() => ClientSimMain.CreateInstance(settings, eventDispatcher);

            // If the Remote Config is not initialized, attempt init before starting ClientSim
            // Start ClientSim after attempt, regardless of success or failure
            if (!ConfigManager.RemoteConfig.IsInitialized())
            {
                API.SetOnlineMode(true);
                ConfigManager.RemoteConfig.Init(CreateClientSimInstance, CreateClientSimInstance);
            }
            // Otherwise, start ClientSim immediately
            else
            {
                CreateClientSimInstance();
            }*/
        }

        private static void DestroyEditorOnly(ClientSimSettings settings)
        {
            if (!settings.enableClientSim || !settings.deleteEditorOnly)
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
                if (obj.CompareTag(EDITOR_ONLY_TAG))
                {
                    obj.Log($"Deleting editor only object: {Tools.GetGameObjectPath(obj)}");
                    Object.DestroyImmediate(obj);
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
    }
}