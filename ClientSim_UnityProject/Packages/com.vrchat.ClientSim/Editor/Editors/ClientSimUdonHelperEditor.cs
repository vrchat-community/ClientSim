using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;

namespace VRC.SDK3.ClientSim.Editor
{
    [CustomEditor(typeof(ClientSimUdonHelper))]
    public class ClientSimUdonHelperEditor : UnityEditor.Editor
    {
        private static readonly MethodInfo _drawPropertyMethod;
        
        private bool _expandVariableEditor = false;
        private bool _expandEventSelector = false;

        static ClientSimUdonHelperEditor()
        {
            _drawPropertyMethod = typeof(UdonProgramAsset).GetMethod("DrawPublicVariableField", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ClientSimUdonHelper udonHelper = target as ClientSimUdonHelper;

            ClientSimSyncableEditorHelper.DisplaySyncOptions(udonHelper);

            UdonBehaviour udonBehaviour = udonHelper.GetUdonBehaviour();

            ShowVariableEditor(udonBehaviour);
            
            ShowExportedEvents(udonBehaviour);
        }

        private void ShowVariableEditor(UdonBehaviour udonBehaviour)
        {
            _expandVariableEditor = EditorGUILayout.Foldout(_expandVariableEditor, "Edit Public Variables", true);

            if (!_expandVariableEditor)
            {
                return;
            }

            var program = udonBehaviour.programSource;

            if (!(program is UdonProgramAsset programAsset))
            {
                return;
            }
            
            var publicVariables = udonBehaviour.publicVariables;

            foreach (var varName in publicVariables.VariableSymbols)
            {
                publicVariables.TryGetVariableType(varName, out Type varType);
                object value = udonBehaviour.GetProgramVariable(varName);
                object[] parameters = {varName, value, varType, false, true};
                var res = _drawPropertyMethod.Invoke(programAsset, parameters);
                
                if ((bool)parameters[3])
                {
                    udonBehaviour.SetProgramVariable(varName, res);
                }
            }
        }
        
        private void ShowExportedEvents(UdonBehaviour udonBehaviour)
        {
            _expandEventSelector = EditorGUILayout.Foldout(_expandEventSelector, "Run Custom Event", true);

            if (!_expandEventSelector)
            {
                return;
            }

            foreach (string eventName in udonBehaviour.GetPrograms())
            {
                if (GUILayout.Button(eventName))
                {
                    udonBehaviour.SendCustomEvent(eventName);
                }
            }
        }
    }
}