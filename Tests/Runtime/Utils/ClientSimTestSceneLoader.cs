using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace VRC.SDK3.ClientSim.Tests
{
    public static class ClientSimTestSceneLoader
    {
        private static readonly string _sceneResourcePath = Path.Combine("Tests", "Runtime", "Resources", "ClientSimTests", "Scenes");
        private const string EMPTY_TEST_SCENE_NAME = "ClientSimTestEmpty.unity";
        private const string BASIC_TEST_SCENE_NAME = "ClientSimTestBasic.unity";

        public static string GetClientSimScenePath(string sceneName)
        {
            return Path.Combine(ClientSimResourceLoader.GetPackagePath(), _sceneResourcePath, sceneName);
        }
        
        private static Scene LoadSceneInPlayMode(string sceneName)
        {
            return LoadSceneInPlayModeFromPath(GetClientSimScenePath(sceneName));
        }

        private static Scene LoadSceneInPlayModeFromPath(string scenePath)
        {
#if UNITY_EDITOR
            return EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Additive));
#else
            return new Scene();
#endif
        }

        private static Scene LoadSceneInEditMode(string sceneName)
        {
            return LoadSceneInEditModeFromPath(GetClientSimScenePath(sceneName));
        }
        
        private static Scene LoadSceneInEditModeFromPath(string scenePath)
        {
#if UNITY_EDITOR
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
#else
            return new Scene();
#endif
        }

        private static Scene LoadScene(string sceneName, bool playMode = true)
        {
            if (playMode)
            {
                return LoadSceneInPlayMode(sceneName);
            }

            return LoadSceneInEditMode(sceneName);
        }

        public static Scene LoadSceneFromPath(string scenePath, bool playMode = true)
        {
            if (playMode)
            {
                return LoadSceneInPlayModeFromPath(scenePath);
            }

            return LoadSceneInEditModeFromPath(scenePath);
        }
        
        // Empty scene only contains basic unity elements but no VRC components. This is useful for testing if
        // ClientSim will fail to start.
        public static Scene LoadEmptyScene(bool playMode = true)
        {
            return LoadScene(EMPTY_TEST_SCENE_NAME, playMode);
        }

        public static string GetEmptyScenePath()
        {
            return GetClientSimScenePath(EMPTY_TEST_SCENE_NAME);
        }
        
        // Basic scene contains a cube floor and a scene descriptor, but no other VRC components. Just enough to start 
        // ClientSim and spawn a player.
        public static Scene LoadBasicScene(bool playMode = true)
        {
            return LoadScene(BASIC_TEST_SCENE_NAME, playMode);
        }
        
        public static string GetBasicScenePath()
        {
            return GetClientSimScenePath(BASIC_TEST_SCENE_NAME);
        }

        public static IEnumerator UnloadPlayModeScene(Scene scene)
        {
            AsyncOperation unloadAction = SceneManager.UnloadSceneAsync(scene);
            yield return new WaitUntil(() => unloadAction.isDone);
        }

        public static void UnloadEditorScene(Scene scene)
        {
#if UNITY_EDITOR
            EditorSceneManager.CloseScene(scene, true);
#endif
        }
    }
}