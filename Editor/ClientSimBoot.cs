
using UnityEditor;

namespace VRC.SDK3.ClientSim.Editor
{
    public class ClientSimBoot : AssetPostprocessor
    {
        // TODO Do not auto populate items on load, but ask user first.
        // Audio settings also requires ensuring the Oculus Desktop package has been installed as well.
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            ClientSimSettingsWindow.TryInitOnLoad();
            SetAudioSettings(); 
        }

        // Used to ensure that everything has been imported before trying to load the inputmap.
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            ClientSimInputAxesSetup.SetupInputMap();
        }

        private static void SetAudioSettings()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
            SerializedProperty spatializerPluginProp = serializedObject.FindProperty("m_SpatializerPlugin");
            SerializedProperty ambisonicDecoderPluginProp = serializedObject.FindProperty("m_AmbisonicDecoderPlugin");

            spatializerPluginProp.stringValue = "OculusSpatializer";
            ambisonicDecoderPluginProp.stringValue = "OculusSpatializer";

            serializedObject.ApplyModifiedProperties();
        }
    }
}