using UnityEngine;
using VRC.SDK3.Components;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimObjectPoolHelper : ClientSimBehaviour, IClientSimSyncable
    {
        private int _ownerID = 1;
        private VRCObjectPool _objectPool;

        private IClientSimSyncedObjectManager _syncedObjectManager;
        
        
        public static void OnSpawn(VRCObjectPool objectPool, int index)
        {
            ClientSimObjectPoolHelper poolHelper = objectPool.GetComponent<ClientSimObjectPoolHelper>();
            if (!poolHelper)
            {
                throw new ClientSimException("Object Pool has not been initialized yet before trying to spawn an object.");
            }
            poolHelper.OnObjectSpawned(index);
        }
        
        public static void OnReturn(VRCObjectPool objectPool, int index)
        {
            ClientSimObjectPoolHelper poolHelper = objectPool.GetComponent<ClientSimObjectPoolHelper>();
            if (!poolHelper)
            {
                throw new ClientSimException("Object Pool has not been initialized yet before trying to return an object.");
            }
            poolHelper.OnObjectReturned(index);
        }
        
        
        public void Initialize(VRCObjectPool objectPool, IClientSimSyncedObjectManager syncedObjectManager)
        {
            _objectPool = objectPool;
            _syncedObjectManager = syncedObjectManager;
            syncedObjectManager.AddSyncedObject(this);
        }
        
        private void Start()
        {
            // Catch Helper not initialized.
            if (_objectPool == null)
            {
                this.LogWarning($"Destroying uninitialized Helper. Object: {Tools.GetGameObjectPath(gameObject)}");
                DestroyImmediate(this);
            }
        }
        
        private void OnDestroy()
        {
            // Nullable needed for uninitialized case.
            _syncedObjectManager?.RemoveSyncedObject(this);
        }

        private void OnObjectSpawned(int index) { }
        
        private void OnObjectReturned(int index) { }
        
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
    }
}