using UnityEditor;
using UnityEngine;

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
            // On exiting playmode, remove the Editor method hooks.
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClientSimMenu.openSettingsHook -= ClientSimSettingsWindow.Init;
                ClientSimMenu.checkValidSettingsHook -= ClientSimProjectSettingsSetup.IsUsingCorrectSettings;
                
                EditorApplication.playModeStateChanged -= ModeStateChanged;
            }
        }

        // When entering playmode, set Editor method hooks.
        // Using this runtime initialized method due to timing issues with with Domain Reloading on and off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnPlaymodeStart()
        {
            ClientSimMenu.openSettingsHook += ClientSimSettingsWindow.Init;
            ClientSimMenu.checkValidSettingsHook += ClientSimProjectSettingsSetup.IsUsingCorrectSettings;
            
            EditorApplication.playModeStateChanged += ModeStateChanged;
        }
    }
}