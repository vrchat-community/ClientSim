using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Editor
{
    /// <summary>
    /// Class used to verify current project settings and to apply required ClientSim Settings.
    /// Currently only has the ability to set Audio Spatializer, Input Axes, and Input Type project settings.
    /// </summary>
    public static class ClientSimProjectSettingsSetup
    {
        public static bool IsUsingCorrectSettings()
        {
            return
                IsUsingCorrectInputAxesSettings()
                && IsUsingCorrectInputTypeSettings()
                && IsUsingCorrectAudioSettings()
                && (UpdateLayers.AreLayersSetup() && UpdateLayers.IsCollisionLayerMatrixSetup());
        }
        
        #region InputAxes

        private const string INPUT_AXES_NAME = "ClientSimInputManager";
        private const string INPUT_AXES_RAW_NAME = ".ClientSimInputManagerRaw.asset";

        private static readonly string _projectInputManagerPath = Path.Combine("ProjectSettings", "InputManager.asset");
        private static readonly string _inputAxesRawPath = Path.Combine("Editor", "Resources", INPUT_AXES_RAW_NAME);
        
        private static ClientSimInputAxesSettings _inputAxes;

        public static bool IsUsingCorrectInputAxesSettings()
        {
            ClientSimInputAxesSettings clientSimAxes = ReadClientSimInputAxes();
            ClientSimInputAxesSettings projectAxes = ReadProjectInputAxes();

            // TODO verify axes on an individual level rather than verifying the entire axis list. Users may want other
            // axes for personal reasons but this prevents that.  
            
            return clientSimAxes.Equals(projectAxes);
        }

        public static void ApplyClientSimInputAxes()
        {
            string packagePath = ClientSimResourceLoader.GetResolvePath();
            string clientSimAxesPath = Path.Combine(packagePath, _inputAxesRawPath);
            
            // User may have installed ClientSim in a non package way. The file is expected to be in the same directory
            // as the input settings file. Use that to determine the proper location.
            if (string.IsNullOrEmpty(packagePath))
            {
                ReadClientSimInputAxes();
                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_inputAxes));
                clientSimAxesPath = Path.Combine(directory, INPUT_AXES_RAW_NAME); 
            }
            
            string projectAxesPath = Path.Combine(Application.dataPath, "..", _projectInputManagerPath);

            File.Copy(clientSimAxesPath, projectAxesPath, true);
            AssetDatabase.Refresh();
        }

        private static ClientSimInputAxesSettings ReadClientSimInputAxes()
        {
            if (_inputAxes == null)
            {
                _inputAxes = Resources.Load<ClientSimInputAxesSettings>(INPUT_AXES_NAME);
            }
            
            return _inputAxes;
        }

        private static ClientSimInputAxesSettings ReadProjectInputAxes()
        {
            Object inputManager = AssetDatabase.LoadAssetAtPath<Object>(_projectInputManagerPath);
            ClientSimInputAxesSettings inputObject = ScriptableObject.CreateInstance<ClientSimInputAxesSettings>();
            inputObject.LoadFromSerializedObject(new SerializedObject(inputManager));
            return inputObject;
        }
        
        #endregion

        #region Input Manager

        // Verify input is set to use both old and new input types.
        public static bool IsUsingCorrectInputTypeSettings()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset"));
            SerializedProperty activeInputHandler = serializedObject.FindProperty("activeInputHandler");

            return activeInputHandler.intValue != 1;
        }

        public static void SetInputTypeSettings()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset"));
            SerializedProperty activeInputHandler = serializedObject.FindProperty("activeInputHandler");

            activeInputHandler.intValue = 1;
            
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Audio Spatializer

        public static bool IsUsingCorrectAudioSettings()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/AudioManager.asset"));
            SerializedProperty spatializerPluginProp = serializedObject.FindProperty("m_SpatializerPlugin");
            SerializedProperty ambisonicDecoderPluginProp = serializedObject.FindProperty("m_AmbisonicDecoderPlugin");

            return 
                spatializerPluginProp.stringValue == "OculusSpatializer" 
                && ambisonicDecoderPluginProp.stringValue == "OculusSpatializer";
        }
        
        public static void SetAudioSettings()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/AudioManager.asset"));
            SerializedProperty spatializerPluginProp = serializedObject.FindProperty("m_SpatializerPlugin");
            SerializedProperty ambisonicDecoderPluginProp = serializedObject.FindProperty("m_AmbisonicDecoderPlugin");

            spatializerPluginProp.stringValue = "OculusSpatializer";
            ambisonicDecoderPluginProp.stringValue = "OculusSpatializer";

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}