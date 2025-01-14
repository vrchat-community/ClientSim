using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System that keeps track of all synced objects. Handles changing ownership when players leave
    /// and also checks for if position synced objects fall below the respawn height.
    /// </summary>
    /// <remarks>
    /// Listens to Events:
    /// - ClientSimOnPlayerLeftEvent
    /// </remarks>
    [AddComponentMenu("")]
    public class ClientSimSyncedObjectManager : ClientSimBehaviour, IClientSimSyncedObjectManager, IDisposable
    {
        // Collection of all synced objects initialized in the scene. 
        private readonly ClientSimObjectCollection<IClientSimSyncable> _syncedObjects =
            new ClientSimObjectCollection<IClientSimSyncable>();
        
        // Collection of all position synced objects initialized in the scene. 
        private readonly ClientSimObjectCollection<IClientSimPositionSyncable> _positionSyncedObjects =
            new ClientSimObjectCollection<IClientSimPositionSyncable>();
        
        // Collection of all synced playerObjects initialized in the scene.
        private readonly ClientSimObjectCollection<IClientSimSyncable> _playerObjects =
            new ClientSimObjectCollection<IClientSimSyncable>();

        // TODO add generic system for saving synced data and restoring it to simulate late joining into a world.
        // TODO add system to manage udon synced data
        
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimSceneManager _sceneManager;
        private IClientSimPlayerManager _playerManager;

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimSceneManager sceneManager, 
            IClientSimPlayerManager playerManager)
        {
            _eventDispatcher = eventDispatcher;
            _sceneManager = sceneManager;
            _playerManager = playerManager;
            
            _eventDispatcher.Subscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
        }

        public void InitializeObjectSync(VRCObjectSync sync)
        {
            // Only allow one sync helper per object.
            if (sync.TryGetComponent(out ClientSimPositionSyncedHelperBase Synchelper))
            {
                if(Synchelper is ClientSimObjectSyncHelper)
                    ((ClientSimObjectSyncHelper)Synchelper).Initialize(sync,this);
                // else other object sync helper types initialize object sync
            }
            else
            {
                ClientSimObjectSyncHelper helper = sync.gameObject.AddComponent<ClientSimObjectSyncHelper>();
                helper.Initialize(sync,this);
            }
        }
        
        public void InitializeObjectPool(VRCObjectPool objectPool)
        {
            objectPool.gameObject.AddComponent<ClientSimObjectPoolHelper>().Initialize(objectPool, this);
        }

        private void LateUpdate()
        {
            ProcessPositionSyncedObjects();
        }

        private void ProcessPositionSyncedObjects()
        {
            _positionSyncedObjects.ProcessAddedAndRemovedObjects();
            
            // TODO space this out so that there are only x number per frame instead of all every time
            List<GameObject> objsToDestroy = new List<GameObject>();
            foreach (IClientSimPositionSyncable sync in _positionSyncedObjects.GetObjects())
            {
                if (sync == null)
                {
                    _positionSyncedObjects.ShouldVerifyObjects();
                    continue;
                }

                if (!sync.SyncPosition)
                {
                    continue;
                }

                // VRChatBug: The following method will enforce users to use VRCObjectSync's methods for setting
                // useGravity and isKinematic, but the current SDK does not support the hook to allow this.
                // Verify Sync properties, eg check if useGravity and isKinematic is properly set. 
                // sync.UpdatePositionSync();
                
                // Verify if the object is below respawn.
                Transform syncTransform = sync.GetTransform();
                if (syncTransform.position.y < _sceneManager.GetRespawnHeight())
                {
                    if (_sceneManager.ShouldObjectsDestroyAtRespawnHeight())
                    {
                        objsToDestroy.Add(syncTransform.gameObject);
                    }
                    else
                    {
                        sync.Respawn();
                    }
                }
            }

            foreach (var obj in objsToDestroy)
            {
                Destroy(obj);
            }
        }

        #region ClientSim Events
        
        // Handle updating object ownership for all objects the leaving player previously owned.
        private void OnPlayerLeft(ClientSimOnPlayerLeftEvent leftEvent)
        {
            VRCPlayerApi leftPlayer = leftEvent.player;
            int leftPlayerId = leftPlayer.playerId;
            VRCPlayerApi masterPlayer = _playerManager.GetMaster();
            if (masterPlayer == null)
            {
                return;
            }
            
            _syncedObjects.ProcessAddedAndRemovedObjects();
            foreach (IClientSimSyncable sync in _syncedObjects.GetObjects())
            {
                if (sync == null)
                {
                    continue;
                }

                if (sync is Component syncComp)
                {
                    if(syncComp == null) continue;
                    
                    GameObject syncObj = syncComp.gameObject;
                    if (Networking.GetOwner(syncObj)?.playerId == leftPlayerId)
                    {
                        Networking.SetOwner(masterPlayer, syncObj);
                    }
                }
                else
                {
                    if (sync.GetOwner() == leftPlayerId)
                    {
                        sync.SetOwner(masterPlayer.playerId);
                    }
                }
            }
        }

        #endregion

        #region IClientSimSyncedObjectManager

        public void AddSyncedObject(IClientSimSyncable sync)
        {
            _syncedObjects.AddObject(sync);
            _syncedObjects.ProcessAddedAndRemovedObjects();
            
            if (sync is IClientSimPositionSyncable posSync && posSync.SyncPosition)
            {
                _positionSyncedObjects.AddObject(posSync);
                _positionSyncedObjects.ProcessAddedAndRemovedObjects();
            }
            
            if((sync as MonoBehaviour).GetComponentInParent<VRCPlayerObject>() != null)
            {
                _playerObjects.AddObject(sync);
                _playerObjects.ProcessAddedAndRemovedObjects();
            }
        }

        public void RemoveSyncedObject(IClientSimSyncable sync)
        {
            _syncedObjects.RemoveObject(sync);
            _syncedObjects.ProcessAddedAndRemovedObjects();
            
            if (sync is IClientSimPositionSyncable posSync && posSync.SyncPosition)
            {
                _positionSyncedObjects.RemoveObject(posSync);
                _positionSyncedObjects.ProcessAddedAndRemovedObjects();
            }
            
            if((sync as MonoBehaviour).GetComponentInParent<VRCPlayerObject>() != null)
            {
                _playerObjects.RemoveObject(sync);
                _playerObjects.ProcessAddedAndRemovedObjects();
            }
        }

        #endregion
    }
}