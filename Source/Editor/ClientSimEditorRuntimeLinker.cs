using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Editor
{
    /// <summary>
    /// This class is used to link any editor specific method to a runtime event hook.
    /// Currently only used for the Pause Menu to be able to open the Settings Window
    /// and to check for proper project settings.
    /// </summary>
    public static class ClientSimEditorRuntimeLinker
    {
        private static void ModeStateChanged(PlayModeStateChange state)
        {
            
            if(state == PlayModeStateChange.EnteredPlayMode)
            {
                ClientSimMenu.openSettingsHook += ClientSimSettingsWindow.Init;
                ClientSimMenu.checkValidSettingsHook += ClientSimProjectSettingsSetup.IsUsingCorrectSettings;
            }
            
            // On exiting playmode, remove the Editor method hooks.
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClientSimMenu.openSettingsHook -= ClientSimSettingsWindow.Init;
                ClientSimMenu.checkValidSettingsHook -= ClientSimProjectSettingsSetup.IsUsingCorrectSettings;
            }

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                InitializeScene();
            }
        }

        // When entering playmode, set Editor method hooks.
        // Using this runtime initialized method due to timing issues with with Domain Reloading on and off.
        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            EditorApplication.playModeStateChanged += ModeStateChanged;
        }
        
        
        private static void InitializeScene()
        {
            ClientSimNetworkingUtilities.DoSceneSetup();
        }
    }
}