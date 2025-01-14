using UnityEditor;

namespace VRC.SDK3.ClientSim.Editor
{
    [CustomEditor(typeof(ClientSimObjectSyncHelper))]
    public class ClientSimObjectSyncHelperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ClientSimSyncableEditorHelper.DisplaySyncOptions(target as ClientSimObjectSyncHelper);
        }
    }
}