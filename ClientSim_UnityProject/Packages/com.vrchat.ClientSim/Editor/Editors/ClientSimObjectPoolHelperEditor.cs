using UnityEditor;

namespace VRC.SDK3.ClientSim.Editor
{
    [CustomEditor(typeof(ClientSimObjectPoolHelper))]
    public class ClientSimObjectPoolHelperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ClientSimSyncableEditorHelper.DisplaySyncOptions(target as ClientSimObjectPoolHelper);
        }
    }
}
