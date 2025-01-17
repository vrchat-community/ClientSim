using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimExtensions
    {
        #region VRCPlayerApi 

        public static ClientSimPlayerController GetPlayerController(this VRCPlayerApi player)
        {
            return player.GetClientSimPlayer().GetPlayerController();
        }

        public static ClientSimPlayer GetClientSimPlayer(this VRCPlayerApi player)
        {
            // Check if the object exists and has not been destroyed due to playmode end.
            if (player.gameObject != null)
            {
                return player.gameObject.GetComponent<ClientSimPlayer>();
            }
            return null;
        }

        #endregion

        #region ClientSimBehaviour
        
        internal static void PreventComponentFromSaving(this MonoBehaviour behaviour)
        {
            // Ensure that no components are ever saved in user project files.
            behaviour.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        #endregion
        
        #region Logging

        internal static void Log(string area, string message)
        {
            if (ClientSimSettings.Instance.displayLogs)
            {
                Debug.Log("[" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "][" + area + "] " + message);
            }
        }

        internal static void Log(this object obj, string message)
        {
            Log(obj.GetType().Name, message);
        }
        
        internal static void LogWarning(string area, string message)
        {
            Debug.LogWarning("[" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "][" + area + "] " + message);
        }

        internal static void LogWarning(this object obj, string message)
        {
            LogWarning(obj.GetType().Name, message);
        }

        internal static void LogError(string area, string message)
        {
            Debug.LogError("[" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "][" + area + "] " + message);
        }

        internal static void LogError(this object obj, string message)
        {
            LogError(obj.GetType().Name, message);
        }

        #endregion
    }
}