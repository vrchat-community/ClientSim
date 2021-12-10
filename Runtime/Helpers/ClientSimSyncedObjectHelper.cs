
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public abstract class ClientSimSyncedObjectHelper : ClientSimBehaviour, IClientSimSyncable, IClientSimRespawnable
    {
        private int ownerID_ = 1;

        private Vector3 originalPosition_;
        private Quaternion originalRotation_;

        private Rigidbody rigidbody_;

        public bool SyncPosition { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            originalPosition_ = transform.position;
            originalRotation_ = transform.rotation;
            rigidbody_ = GetComponent<Rigidbody>();
            
            ClientSimMain.AddSyncedObject(this);
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            this.Log("Teleporting Object " + VRC.Tools.GetGameObjectPath(gameObject) + " to " + position + " and rotation " + rotation);
            FlagDiscontinuity();
            transform.SetPositionAndRotation(position, rotation);
        }

        public void FlagDiscontinuity()
        {
            // TODO As of right now, ClientSim doesn't handle any actual sync.
        }

        private void OnDestroy()
        {
            ClientSimMain.RemoveSyncedObject(this);
        }

        #region IClientSimSyncable

        public int GetOwner()
        {
            return ownerID_;
        }

        public void SetOwner(int ownerID)
        {
            ownerID_ = ownerID;
        }

        #endregion

        #region IClientSimRespawnable

        public void Respawn()
        {
            this.Log("Respawning Object " + VRC.Tools.GetGameObjectPath(gameObject));
            TeleportTo(originalPosition_, originalRotation_);
            
            if (rigidbody_ != null)
            {
                rigidbody_.velocity = Vector3.zero;
            }
        }

        #endregion
    }
}