
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System to hold all the current initialized Udon in the scene
    /// </summary>
    /// <remarks>
    /// Listens to Events:
    /// - ClientSimOnPlayerJoinedEvent
    /// - ClientSimOnPlayerLeftEvent
    /// - ClientSimOnPlayerRespawnEvent
    /// </remarks>
    public class ClientSimUdonManager : IClientSimUdonManager, IDisposable
    {
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimSyncedObjectManager _syncedObjectManager;
        private IClientSimUdonEventSender _udonEventSender;
        
        private readonly ClientSimObjectCollection<UdonBehaviour> _udonBehaviours = 
            new ClientSimObjectCollection<UdonBehaviour>();

        private bool _isReady = false;

        public ClientSimUdonManager(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimSyncedObjectManager syncedObjectManager,
            IClientSimUdonEventSender udonEventSender)
        {
            _eventDispatcher = eventDispatcher;
            _syncedObjectManager = syncedObjectManager;
            _udonEventSender = udonEventSender;

            Subscribe();
        }

        ~ClientSimUdonManager()
        {
            Dispose();
        }

        private void Subscribe()
        {
            _eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Subscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
            _eventDispatcher.Subscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
            _eventDispatcher.Subscribe<ClientSimScreenUpdateEvent>(OnScreenUpdate);
        }
        
        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
            _eventDispatcher.Unsubscribe<ClientSimScreenUpdateEvent>(OnScreenUpdate);
        }

        public void InitUdon(UdonBehaviour behaviour, IUdonProgram program)
        {
            ClientSimUdonHelper helper = behaviour.gameObject.AddComponent<ClientSimUdonHelper>();
            helper.Initialize(behaviour, this, _syncedObjectManager, _isReady);
        }

        public IEnumerator OnClientSimReady()
        {
            _isReady = true;
            
            _udonBehaviours.ProcessAddedAndRemovedObjects();
            HashSet<GameObject> objs = new HashSet<GameObject>();
            foreach (var udonBehavior in _udonBehaviours.GetObjects())
            {
                if (udonBehavior == null || objs.Contains(udonBehavior.gameObject))
                {
                    continue;
                }
                objs.Add(udonBehavior.gameObject);

                foreach (var helper in udonBehavior.GetComponents<ClientSimUdonHelper>())
                {
                    try
                    {
                        helper.OnReady();
                    }
                    catch (Exception e)
                    {
                        this.LogError($"{e.Message}\n{e.StackTrace}");
                        this.LogWarning($"Failed to set ready for object: {Tools.GetGameObjectPath(helper.gameObject)}");
                    }
                }
            }
            
            // Wait one frame for all active UdonBehaviours to properly run start.
            yield return null;
        }

        #region ClientSimEvent handling

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent joinEvent)
        {
            _udonEventSender.RunEvent("_onPlayerJoined", ("player", joinEvent.player));
        }

        private void OnPlayerLeft(ClientSimOnPlayerLeftEvent leftEvent)
        {
            _udonEventSender.RunEvent("_onPlayerLeft", ("player", leftEvent.player));
        }

        private void OnPlayerRespawn(ClientSimOnPlayerRespawnEvent respawnEvent)
        {
            _udonEventSender.RunEvent("_onPlayerRespawn", ("player", respawnEvent.player));
        }
        
        private void OnScreenUpdate(ClientSimScreenUpdateEvent screenUpdateEvent)
        {
            _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONSCREENUPDATE, ("data", screenUpdateEvent.data));
        }

        #endregion

        #region IClientSimUdonManager

        public void AddUdonBehaviour(UdonBehaviour udonBehaviour)
        {
            _udonBehaviours.AddObject(udonBehaviour);
        }

        public void RemoveUdonBehaviour(UdonBehaviour udonBehaviour)
        {
            _udonBehaviours.RemoveObject(udonBehaviour);
        }

        #endregion
    }
}