#if UNITY_EDITOR

using UnityEditor;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// If 'settings.stopOnScriptChanges' is enabled, this AssetPostprocessor will detect if
    /// script changes occured while in play mode and stop play mode if so. It will override the
    /// Unity Editor setting 'Preferences > General > Script Changes While Playing'
    /// </summary>
    class ClientSimAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths)
        {
            if (EditorApplication.isPlaying && ClientSimSettings.Instance.enableClientSim && ClientSimSettings.Instance.stopOnScriptChanges)
            {
                foreach (string str in imported)
                {
                    if (str.EndsWith(".cs"))
                    {
                        EditorApplication.isPlaying = false;
                        return;
                    }
                }

                foreach (string str in deleted)
                {
                    if (str.EndsWith(".cs"))
                    {
                        EditorApplication.isPlaying = false;
                        return;
                    }
                }
            }
        }
    }

}

#endif