using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                Object.FindObjectOfType<PipelineSaver>() == null && 
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
            
            // Create ClientSim Instance
            ClientSimMain.CreateInstance(settings, eventDispatcher);
        }

        private static void DestroyEditorOnly(ClientSimSettings settings)
        {
            if (!settings.deleteEditorOnly)
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