
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public abstract class ClientSimPositionSyncedHelperBase : 
        ClientSimBehaviour, 
        IClientSimSyncable,
        IClientSimPositionSyncable,
        IClientSimRespawnHandler
    {
        private int _ownerID = 1;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private Rigidbody _rigidbody;
        private bool _useGravity;
        private bool _isKinematic;

        private IClientSimSyncedObjectManager _syncedObjectManager;
        
        public bool SyncPosition { get; protected set; }
        
        protected override void Awake()
        {
            base.Awake();
            
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody != null)
            {
                _isKinematic = _rigidbody.isKinematic;
                _useGravity = _rigidbody.useGravity;
            }
        }

        public virtual void Initialize(IClientSimSyncedObjectManager syncedObjectManager)
        {
            _syncedObjectManager = syncedObjectManager;
            _syncedObjectManager.AddSyncedObject(this);
        }

        protected virtual void OnDestroy()
        {
            // Nullable needed for uninitialized case.
            _syncedObjectManager?.RemoveSyncedObject(this);
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            this.Log($"Teleporting Object {Tools.GetGameObjectPath(gameObject)} to {position} and rotation {rotation.eulerAngles}");
            FlagDiscontinuity();
            transform.SetPositionAndRotation(position, rotation);
        }

        public void FlagDiscontinuity()
        {
            // TODO As of right now, ClientSim doesn't handle any actual sync.
        }

        #region IClientSimSyncable

        public int GetOwner()
        {
            return _ownerID;
        }

        public void SetOwner(int ownerID)
        {
            _ownerID = ownerID;
        }

        #endregion

        #region IClientSimRespawnable

        public void Respawn()
        {
            this.Log($"Respawning Object {Tools.GetGameObjectPath(gameObject)}");
            TeleportTo(_originalPosition, _originalRotation);
            
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        #endregion

        #region IClientSimPositionSyncable
        
        public void SetIsKinematic(bool value)
        {
            _isKinematic = value;
            if (_rigidbody)
            {
                _rigidbody.isKinematic = value;
            }
        }
        
        public void SetUseGravity(bool value)
        {
            _useGravity = value;
            if (_rigidbody)
            {
                _rigidbody.useGravity = value;
            }
        }
        
        public bool GetIsKinematic()
        {
            return _rigidbody && _rigidbody.isKinematic;
        }
        
        public bool GetUseGravity()
        {
            return _rigidbody && _rigidbody.useGravity;
        }

        public void UpdatePositionSync()
        {
            if (_rigidbody != null)
            {
                // TODO if user is not the owner, set useGravity to false, isKinematic to true.
                // This would better simulate ownership, but also make testing awkward. 
                
                if (_rigidbody.isKinematic != _isKinematic)
                {
                    _rigidbody.isKinematic = _isKinematic;
                    this.LogWarning($"Rigidbody.isKinematic was set outside of VRCObjectSync.SetKinematic method! {Tools.GetGameObjectPath(gameObject)}");
                }
                
                if (_rigidbody.useGravity != _useGravity)
                {
                    _rigidbody.useGravity = _useGravity;
                    this.LogWarning($"Rigidbody.useGravity was set outside of VRCObjectSync.SetGravity method! {Tools.GetGameObjectPath(gameObject)}");
                }
                
                // VRChatBug: Modifying a collider's "is Trigger" property is also restricted when VRCObjectSync is on
                // the same object, but this check will not be added.
            }
        }

        public Transform GetTransform() => transform;

        #endregion
    }
}