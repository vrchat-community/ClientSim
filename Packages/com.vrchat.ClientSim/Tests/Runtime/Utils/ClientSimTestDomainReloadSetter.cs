using NUnit.Framework;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace VRC.SDK3.ClientSim.Tests
{
    // When testing ClientSim, in order to disable normal ClientSim behavior of starting on entering play and keep the
    // setting, Domain Reloading needs to be disabled as any variable changes before playmode starts will be cleared.
    // This class provides options to save the current domain/scene reload settings, disable them, and then load back
    // previous settings as they were before. This is handled in ClientSimTestBase and ClientSimWorldTestBase in the
    // pre-build setup and post-build cleanup methods.
    public static class ClientSimTestDomainReloadSetter
    {
        private static bool _playmodeSettingsEnabled = false;
        private static int _playmodeSettings = 0;

        private static int _setCount = 0;
        
        public static void SetDisableDomainReloadingSetting()
        {
            ++_setCount;
            // Multiple items have tried to save the settings. Return early.
            if (_setCount > 1)
            {
                return;
            }
            
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/EditorSettings.asset"));
            SerializedProperty playmodeSettingsEnabledProp = serializedObject.FindProperty("m_EnterPlayModeOptionsEnabled");
            SerializedProperty playmodeSettingsProp = serializedObject.FindProperty("m_EnterPlayModeOptions");

            _playmodeSettingsEnabled = playmodeSettingsEnabledProp.boolValue;
            _playmodeSettings = playmodeSettingsProp.intValue;
            
            playmodeSettingsEnabledProp.boolValue = true;
            playmodeSettingsProp.intValue = 3; // Disable domain and scene reloading.
            
            serializedObject.ApplyModifiedProperties();
#endif
        }

        public static void ResetDisableDomainReloadingSetting()
        { 
            --_setCount;
            // Multiple items have tried to reset the settings. Return early if not the last.
            if (_setCount > 0)
            {
                return;
            }

            if (_setCount < 0)
            {
                Assert.Fail("Reset Disable Domain Reloading Settings called more times than Set.");
            }
            
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/EditorSettings.asset"));
            SerializedProperty playmodeSettingsEnabledProp = serializedObject.FindProperty("m_EnterPlayModeOptionsEnabled");
            SerializedProperty playmodeSettingsProp = serializedObject.FindProperty("m_EnterPlayModeOptions");

            playmodeSettingsEnabledProp.boolValue = _playmodeSettingsEnabled;
            playmodeSettingsProp.intValue = _playmodeSettings;
            
            serializedObject.ApplyModifiedProperties();
#endif
        }
    }
}