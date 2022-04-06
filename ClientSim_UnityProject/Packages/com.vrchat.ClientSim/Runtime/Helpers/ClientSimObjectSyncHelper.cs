using UnityEngine;
using VRC.SDK3.Components;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimObjectSyncHelper : ClientSimPositionSyncedHelperBase
    {
        private VRCObjectSync _sync;

        public static void TeleportTo(VRCObjectSync obj, Vector3 position, Quaternion rotation)
        {
            obj.GetComponent<ClientSimObjectSyncHelper>().TeleportTo(position, rotation);
        }

        public static void RespawnObject(VRCObjectSync sync)
        {
            sync.GetComponent<ClientSimObjectSyncHelper>().Respawn();
        }
        
        public static void SetIsKinematic(VRCObjectSync sync, bool value)
        {
            sync.GetComponent<ClientSimObjectSyncHelper>().SetIsKinematic(value);
        }
        
        public static void SetUseGravity(VRCObjectSync sync, bool value)
        {
            sync.GetComponent<ClientSimObjectSyncHelper>().SetUseGravity(value);
        }
        
        public static bool GetIsKinematic(VRCObjectSync sync)
        {
            return sync.GetComponent<ClientSimObjectSyncHelper>().GetIsKinematic();
        }
        
        public static bool GetUseGravity(VRCObjectSync sync)
        {
            return sync.GetComponent<ClientSimObjectSyncHelper>().GetUseGravity();
        }
        
        public static void FlagDiscontinuityHook(VRCObjectSync sync)
        {
            sync.GetComponent<ClientSimObjectSyncHelper>().FlagDiscontinuity();
        }

        protected override void Awake()
        {
            base.Awake();
            SyncPosition = true;
        }

        public void Initialize(VRCObjectSync sync, IClientSimSyncedObjectManager syncedObjectManager)
        {
            base.Initialize(syncedObjectManager);
            _sync = sync;
        }
        
        private void Start()
        {
            // Catch Helper not initialized.
            if (_sync == null)
            {
                this.LogWarning($"Destroying uninitialized Helper. Object: {Tools.GetGameObjectPath(gameObject)}");
                DestroyImmediate(this);
            }
        }
    }
}