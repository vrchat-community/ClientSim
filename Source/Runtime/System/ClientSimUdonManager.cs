
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
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
            _eventDispatcher.Subscribe<ClientSimOnVRCPlusMassGift>(OnVRCPlusMassGift);
        }
        
        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerRespawnEvent>(OnPlayerRespawn);
            _eventDispatcher.Unsubscribe<ClientSimScreenUpdateEvent>(OnScreenUpdate);
            _eventDispatcher.Unsubscribe<ClientSimOnVRCPlusMassGift>(OnVRCPlusMassGift);
        }

        public void InitUdon(UdonBehaviour behaviour, IUdonProgram program)
        {
            ClientSimUdonHelper[] helpers = behaviour.gameObject.GetComponents<ClientSimUdonHelper>();

            foreach (ClientSimUdonHelper helper in helpers)
            {
                if(helper.GetUdonBehaviour() == behaviour)
                {
                    return;
                }
            }
            
            if (helpers.Length == 0)
            {
                ClientSimUdonHelper helper = behaviour.gameObject.AddComponent<ClientSimUdonHelper>();
                helper.Initialize(behaviour, this, _syncedObjectManager, _isReady);
                return;
            }
            
            foreach (ClientSimUdonHelper helper in helpers)
            {
                if(helper.GetUdonBehaviour() == null)
                {
                    helper.Initialize(behaviour, this, _syncedObjectManager, _isReady);
                    return;
                }
            }
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
            _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONINPUTMETHODCHANGED, ("inputMethod", VRCInputMethod.Keyboard));
            _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONLANGUAGECHANGED, ("language", ClientSimSettings.Instance.currentLanguage));
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

        private void OnVRCPlusMassGift(ClientSimOnVRCPlusMassGift giftEvent)
        {
            _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONVRCPLUSMASSGIFT, 
                ("gifter", giftEvent.gifter),
                ("numGifts", giftEvent.numGifts));
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