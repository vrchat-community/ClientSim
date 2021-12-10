using UnityEngine;
using VRC.SDK3.Components;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimObjectSyncHelper : ClientSimSyncedObjectHelper
    {
        private VRCObjectSync sync_;

        private Rigidbody rigidbody_;

        public static void InitializeObjectSync(VRCObjectSync sync)
        {
            var helper = sync.GetComponent<ClientSimObjectSyncHelper>();
            if (helper)
            {
                DestroyImmediate(helper);
            }
            
            sync.gameObject.AddComponent<ClientSimObjectSyncHelper>();
        }

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

            rigidbody_ = GetComponent<Rigidbody>();
            sync_ = GetComponent<VRCObjectSync>();
        }

        private void SetIsKinematic(bool value)
        {
            if (rigidbody_)
            {
                rigidbody_.isKinematic = value;
            }
        }
        
        private void SetUseGravity(bool value)
        {
            if (rigidbody_)
            {
                rigidbody_.useGravity = value;
            }
        }
        
        private bool GetIsKinematic()
        {
            return rigidbody_ && rigidbody_.isKinematic;
        }
        
        private bool GetUseGravity()
        {
            return rigidbody_ && rigidbody_.useGravity;
        }
    }
}