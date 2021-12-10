using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimExtensions
    {
        #region IClientSimInteractable

        private const float INTERACT_SCALE = 1.25f;

        public static float CalculateInteractDistanceFormula()
        {
            float camSize = ClientSimPlayerController.instance.GetCameraScale();
            return camSize * INTERACT_SCALE;
        }

        public static IClientSimInteractable GetFirstInteractable(this GameObject obj, float distance)
        {
            foreach (var interactable in obj.GetComponents<IClientSimInteractable>())
            {
                if (interactable.CanInteract(distance))
                {
                    return interactable;
                }
            }

            return null;
        }

        public static bool CanInteract(this IClientSimInteractable interactable, float distance)
        {
            float proximityCalculation = CalculateInteractDistanceFormula() + interactable.GetProximity();
            return interactable.CanInteract() && distance <= proximityCalculation;
        }

        public static void Interact(this GameObject obj, float distance)
        {
            foreach (var interactable in obj.GetComponents<IClientSimInteractable>())
            {
                if (interactable.CanInteract(distance))
                {
                    interactable.Interact();
                }
            }
        }

        #endregion

        #region IClientSimPickupable

        public static void OnPickup(this GameObject obj)
        {
            foreach (var pickupable in obj.GetComponents<IClientSimPickupable>())
            {
                pickupable.OnPickup();
            }
        }

        public static void OnDrop(this GameObject obj)
        {
            foreach (var pickupable in obj.GetComponents<IClientSimPickupable>())
            {
                pickupable.OnDrop();
            }
        }

        public static void OnPickupUseDown(this GameObject obj)
        {
            foreach (var pickupable in obj.GetComponents<IClientSimPickupable>())
            {
                pickupable.OnPickupUseDown();
            }
        }

        public static void OnPickupUseUp(this GameObject obj)
        {
            foreach (var pickupable in obj.GetComponents<IClientSimPickupable>())
            {
                pickupable.OnPickupUseUp();
            }
        }

        #endregion

        #region IClientSimStationHandler

        public static void OnStationEnter(this GameObject obj, VRCStation station)
        {
            foreach (var stationHandler in obj.GetComponents<IClientSimStationHandler>())
            {
                stationHandler.OnStationEnter(station);
            }
        }

        public static void OnStationExit(this GameObject obj, VRCStation station)
        {
            foreach (var stationHandler in obj.GetComponents<IClientSimStationHandler>())
            {
                stationHandler.OnStationExit(station);
            }
        }

        #endregion

        #region IClientSimSyncable & IClientSimSyncableHandler

        public static void SetOwner(this GameObject obj, VRCPlayerApi player)
        {
            if (Networking.GetOwner(obj) == player)
            {
                return;
            }

            IClientSimSyncable[] syncs = obj.GetComponents<IClientSimSyncable>();
            foreach (IClientSimSyncable sync in syncs)
            {
                sync.SetOwner(player.playerId);
            }

            IClientSimSyncableHandler[] syncHandlers = obj.GetComponents<IClientSimSyncableHandler>();
            foreach (IClientSimSyncableHandler syncHandler in syncHandlers)
            {
                syncHandler.OnOwnershipTransferred(player.playerId);
            }
        }

        #endregion

        #region VRCPlayerApi 

        public static ClientSimPlayerController GetPlayerController(this VRCPlayerApi player)
        {
            return player.gameObject.GetComponent<ClientSimPlayerController>();
        }

        public static ClientSimPlayer GetClientSimPlayer(this VRCPlayerApi player)
        {
            return player.gameObject.GetComponent<ClientSimPlayer>();
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

        private static void Log(string area, string message)
        {
            Debug.Log("[" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "][" + area + "] " + message);
        }

        internal static void Log(this object obj, string message)
        {
            if (ClientSimSettings.Instance.displayLogs)
            {
                Log(obj.GetType().Name, message);
            }
        }
        
        private static void LogWarning(string area, string message)
        {
            Debug.LogWarning("[" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "][" + area + "] " + message);
        }

        internal static void LogWarning(this object obj, string message)
        {
            LogWarning(obj.GetType().Name, message);
        }

        private static void LogError(string area, string message)
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