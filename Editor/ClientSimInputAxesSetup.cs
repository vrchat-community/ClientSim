using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace VRC.SDK3.ClientSim.Editor
{
    public static class ClientSimInputAxesSetup
    {
        public static string INPUT_MAP_FILE_NAME = "ClientSimOculusInputMap";
        
        public static void SetupInputMap()
        {
            HashSet<string> inputAxisNames = new HashSet<string>();
            ClientSimInputAxesSettings inputAxes = LoadInputAxesSettings();

            if (inputAxes == null)
            {
                return;
            }
            
            foreach (var inputAxis in inputAxes.inputAxes)
            {
                inputAxisNames.Add(inputAxis.name);
            }

            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProp = serializedObject.FindProperty("m_Axes");

            for (int currentAxis = axesProp.arraySize - 1; currentAxis >= 0; --currentAxis)
            {
                SerializedProperty inputAxisProperty = axesProp.GetArrayElementAtIndex(currentAxis);
                SerializedProperty axisNameProp = inputAxisProperty.FindPropertyRelative("m_Name");
                string axisName = axisNameProp.stringValue;
                
                if (inputAxisNames.Contains(axisName))
                {
                    axesProp.DeleteArrayElementAtIndex(currentAxis);
                }
                
                // Force horizontal and vertical gravity and sensitivity to be large so that it doesn't feel floaty
                // like Unity's default values.
                if (axisName == "Horizontal" || axisName == "Vertical")
                {
                    SerializedProperty gravity = inputAxisProperty.FindPropertyRelative("gravity");
                    SerializedProperty sensitivity = inputAxisProperty.FindPropertyRelative("sensitivity");

                    gravity.floatValue = sensitivity.floatValue = 1000;
                }
            }

            foreach (var inputAxis in inputAxes.inputAxes)
            {
                ++axesProp.arraySize;
                SerializedProperty inputAxisProperty = axesProp.GetArrayElementAtIndex(axesProp.arraySize - 1);

                SerializedProperty axisName = inputAxisProperty.FindPropertyRelative("m_Name");
                SerializedProperty descriptiveName = inputAxisProperty.FindPropertyRelative("descriptiveName");
                SerializedProperty descriptiveNegativeName = inputAxisProperty.FindPropertyRelative("descriptiveNegativeName");
                SerializedProperty negativeButton = inputAxisProperty.FindPropertyRelative("negativeButton");
                SerializedProperty positiveButton = inputAxisProperty.FindPropertyRelative("positiveButton");
                SerializedProperty altNegativeButton = inputAxisProperty.FindPropertyRelative("altNegativeButton");
                SerializedProperty altPositiveButton = inputAxisProperty.FindPropertyRelative("altPositiveButton");
                SerializedProperty gravity = inputAxisProperty.FindPropertyRelative("gravity");
                SerializedProperty dead = inputAxisProperty.FindPropertyRelative("dead");
                SerializedProperty sensitivity = inputAxisProperty.FindPropertyRelative("sensitivity");
                SerializedProperty snap = inputAxisProperty.FindPropertyRelative("snap");
                SerializedProperty invert = inputAxisProperty.FindPropertyRelative("invert");
                SerializedProperty type = inputAxisProperty.FindPropertyRelative("type");
                SerializedProperty axis = inputAxisProperty.FindPropertyRelative("axis");
                SerializedProperty joyNum = inputAxisProperty.FindPropertyRelative("joyNum");

                axisName.stringValue = inputAxis.name;
                descriptiveName.stringValue = inputAxis.descriptiveName;
                descriptiveNegativeName.stringValue = inputAxis.descriptiveNegativeName;
                negativeButton.stringValue = inputAxis.negativeButton;
                positiveButton.stringValue = inputAxis.positiveButton;
                altNegativeButton.stringValue = inputAxis.altNegativeButton;
                altPositiveButton.stringValue = inputAxis.altPositiveButton;
                gravity.floatValue = inputAxis.gravity;
                dead.floatValue = inputAxis.dead;
                sensitivity.floatValue = inputAxis.sensitivity;
                snap.boolValue = inputAxis.snap;
                invert.boolValue = inputAxis.invert;
                type.enumValueIndex = inputAxis.type;
                axis.enumValueIndex = inputAxis.axis;
                joyNum.enumValueIndex = inputAxis.joyNum;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static ClientSimInputAxesSettings LoadInputAxesSettings()
        {
            return Resources.Load<ClientSimInputAxesSettings>(INPUT_MAP_FILE_NAME);
        }
    }
}