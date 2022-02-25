using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim.Tests
{
    public class ClientSimTestRuntimeHelper : IDisposable
    {
        public ClientSimEventDispatcher EventDispatcher { get; private set; }
        public ClientSimTestInput TestInput  { get; private set; }

        private bool _initialized = false;
        
        private bool _isReadySent = false;

        private ClientSimMain _clientSim;
        
        private VRCPlayerApi _localPlayer;
        private ClientSimPlayer _localPlayerPlayer;
        private ClientSimPlayerController _localPlayerController;
        private IClientSimPlayerStationManager _stationManager;
        private ClientSimTrackingProviderBase _playerTracking;
        
        private VRCPlayerApi _lastJoinPlayer;
        private VRCPlayerApi _lastLeftPlayer;
        
        private ClientSimRaycastResults _raycastResultsRight;
        private ClientSimRaycastResults _raycastResultsLeft;
        
        private ClientSimInteractEvent _lastInteractResultsRight;
        private ClientSimInteractEvent _lastInteractResultsLeft;
        
        private VRC_Pickup _lastPickupPickedUpRight;
        private VRC_Pickup _lastPickupPickedUpLeft;
        private VRC_Pickup _lastPickupDroppedRight;
        private VRC_Pickup _lastPickupDroppedLeft;
        private VRC_Pickup _lastPickupUseUpRight;
        private VRC_Pickup _lastPickupUseUpLeft;
        private VRC_Pickup _lastPickupUseDownRight;
        private VRC_Pickup _lastPickupUseDownLeft;

        private VRCStation _lastEnteredStation;
        private VRCStation _lastExitedStation;


        public ClientSimTestRuntimeHelper()
        {
            EventDispatcher = new ClientSimEventDispatcher();
            EventDispatcher.Subscribe<ClientSimReadyEvent>(OnReady);
            EventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            EventDispatcher.Subscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
            EventDispatcher.Subscribe<ClientSimRaycastHitResultsEvent>(OnRaycastResults);
            EventDispatcher.Subscribe<ClientSimInteractEvent>(OnInteract);
            EventDispatcher.Subscribe<ClientSimOnPickupEvent>(OnPickup);
            EventDispatcher.Subscribe<ClientSimOnPickupDropEvent>(OnDrop);
            EventDispatcher.Subscribe<ClientSimOnPickupUseDownEvent>(OnPickupUseDown);
            EventDispatcher.Subscribe<ClientSimOnPickupUseUpEvent>(OnPickupUseUp);
            EventDispatcher.Subscribe<ClientSimOnPlayerEnteredStationEvent>(PlayerEnteredStation);
            EventDispatcher.Subscribe<ClientSimOnPlayerExitedStationEvent>(PlayerExitedStation);

            TestInput = new ClientSimTestInput();
            
            _initialized = true;
        }

        ~ClientSimTestRuntimeHelper()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_initialized)
            {
                return;
            }
            _initialized = false;
            
            _clientSim = null;
            _lastJoinPlayer = null;
            _lastLeftPlayer = null;

            EventDispatcher.Unsubscribe<ClientSimReadyEvent>(OnReady);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
            EventDispatcher.Unsubscribe<ClientSimRaycastHitResultsEvent>(OnRaycastResults);
            EventDispatcher.Unsubscribe<ClientSimInteractEvent>(OnInteract);
            EventDispatcher.Unsubscribe<ClientSimOnPickupEvent>(OnPickup);
            EventDispatcher.Unsubscribe<ClientSimOnPickupDropEvent>(OnDrop);
            EventDispatcher.Unsubscribe<ClientSimOnPickupUseDownEvent>(OnPickupUseDown);
            EventDispatcher.Unsubscribe<ClientSimOnPickupUseUpEvent>(OnPickupUseUp);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerEnteredStationEvent>(PlayerEnteredStation);
            EventDispatcher.Unsubscribe<ClientSimOnPlayerExitedStationEvent>(PlayerExitedStation);

            EventDispatcher.Dispose();
            EventDispatcher = null;
            
            TestInput.Dispose();
            TestInput = null;
        }

        #region ClientSim Events

        private void OnReady(ClientSimReadyEvent readyEvent)
        {
            _isReadySent = true;
            
            _clientSim = ClientSimMain.GetInstance();
            
            // ClientSim has started. Get references to local player
            _localPlayer = Networking.LocalPlayer;
            _localPlayerPlayer = _localPlayer.GetClientSimPlayer();
            _localPlayerController = _localPlayerPlayer.GetPlayerController();
            _stationManager = _localPlayerPlayer.GetStationHandler();
            _playerTracking = (ClientSimTrackingProviderBase)_localPlayerPlayer.GetTrackingProvider();
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent joinEvent)
        {
            _lastJoinPlayer = joinEvent.player;
        }

        private void OnPlayerLeft(ClientSimOnPlayerLeftEvent leftEvent)
        {
            _lastLeftPlayer = leftEvent.player;
        }

        private void OnRaycastResults(ClientSimRaycastHitResultsEvent raycastHitResultsEvent)
        {
            if (raycastHitResultsEvent.handType == HandType.RIGHT)
            {
                _raycastResultsRight = raycastHitResultsEvent.raycastResults;
            }
            else
            {
                _raycastResultsLeft = raycastHitResultsEvent.raycastResults;
            }
        }
        
        private void OnInteract(ClientSimInteractEvent interactEvent)
        {
            if (interactEvent.handType == HandType.RIGHT)
            {
                _lastInteractResultsRight = interactEvent;                
            }
            else
            {
                _lastInteractResultsLeft = interactEvent;
            }
        }
        
        private void OnPickup(ClientSimOnPickupEvent pickupEvent)
        {
            Assert.IsTrue(pickupEvent.player.isLocal);
            if (pickupEvent.handType == HandType.RIGHT)
            {
                _lastPickupPickedUpRight = pickupEvent.pickup.GetPickup();
            }
            else
            {
                _lastPickupPickedUpLeft = pickupEvent.pickup.GetPickup();
            }
        }

        private void OnDrop(ClientSimOnPickupDropEvent pickupEvent)
        {
            Assert.IsTrue(pickupEvent.player.isLocal);
            if (pickupEvent.handType == HandType.RIGHT)
            {
                _lastPickupDroppedRight = pickupEvent.pickup.GetPickup();
            }
            else
            {
                _lastPickupDroppedLeft = pickupEvent.pickup.GetPickup();
            }
        }

        private void OnPickupUseDown(ClientSimOnPickupUseDownEvent pickupEvent)
        {
            Assert.IsTrue(pickupEvent.player.isLocal);
            if (pickupEvent.handType == HandType.RIGHT)
            {
                _lastPickupUseDownRight = pickupEvent.pickup.GetPickup();
            }
            else
            {
                _lastPickupUseDownLeft = pickupEvent.pickup.GetPickup();
            }
        }

        private void OnPickupUseUp(ClientSimOnPickupUseUpEvent pickupEvent)
        {
            Assert.IsTrue(pickupEvent.player.isLocal);
            if (pickupEvent.handType == HandType.RIGHT)
            {
                _lastPickupUseUpRight = pickupEvent.pickup.GetPickup();
            }
            else
            {
                _lastPickupUseUpLeft = pickupEvent.pickup.GetPickup();
            }
        }
        
        private void PlayerEnteredStation(ClientSimOnPlayerEnteredStationEvent stationEvent)
        {
            Assert.IsTrue(stationEvent.player.isLocal);
            _lastEnteredStation = stationEvent.station.GetStation();
        }

        private void PlayerExitedStation(ClientSimOnPlayerExitedStationEvent stationEvent)
        {
            Assert.IsTrue(stationEvent.player.isLocal);
            _lastExitedStation = stationEvent.station.GetStation();
        }
        
        #endregion

        public ClientSimProxyObjects GetProxyObjects()
        {
            return _clientSim.GetProxyObjects();
        }

        public ClientSimMenu GetMenu()
        {
            return _clientSim.GetMenu();
        }
        
        #region Event result getters 
        
        public bool HasReadyEventSent()
        {
            return _isReadySent;
        }

        public bool IsLocalPlayerInStation()
        {
            return _stationManager.InStation();
        }

        public VRCPlayerApi GetLastJoinPlayer()
        {
            return _lastJoinPlayer;
        }

        public VRCPlayerApi GetLastLeftPlayer()
        {
            return _lastLeftPlayer;
        }

        public ClientSimRaycastResults GetLastRaycastResults(HandType handType)
        {
            if (handType == HandType.RIGHT)
            {
                return _raycastResultsRight;
            }

            return _raycastResultsLeft;
        }
        
        public ClientSimInteractEvent GetLastInteractResults(HandType handType, bool clearResults)
        {
            ClientSimInteractEvent results;
            if (handType == HandType.RIGHT)
            {
                results = _lastInteractResultsRight;
                if (clearResults)
                {
                    _lastInteractResultsRight = null;
                }
            }
            else
            {
                results = _lastInteractResultsLeft;
                if (clearResults)
                {
                    _lastInteractResultsLeft = null;
                }
            }

            return results;
        }
        
        public VRC_Pickup GetLastPickupPickedUp(HandType handType, bool clearResults)
        {
            VRC_Pickup results;
            if (handType == HandType.RIGHT)
            {
                results = _lastPickupPickedUpRight;
                if (clearResults)
                {
                    _lastPickupPickedUpRight = null;
                }
            }
            else
            {
                results = _lastPickupPickedUpLeft;
                if (clearResults)
                {
                    _lastPickupPickedUpLeft = null;
                }
            }

            return results;
        }
        
        public VRC_Pickup GetLastPickupDropped(HandType handType, bool clearResults)
        {
            VRC_Pickup results;
            if (handType == HandType.RIGHT)
            {
                results = _lastPickupDroppedRight;
                if (clearResults)
                {
                    _lastPickupDroppedRight = null;
                }
            }
            else
            {
                results = _lastPickupDroppedLeft;
                if (clearResults)
                {
                    _lastPickupDroppedLeft = null;
                }
            }

            return results;
        }
        
        public VRC_Pickup GetLastPickupUseDown(HandType handType, bool clearResults)
        {
            VRC_Pickup results;
            if (handType == HandType.RIGHT)
            {
                results = _lastPickupUseDownRight;
                if (clearResults)
                {
                    _lastPickupUseDownRight = null;
                }
            }
            else
            {
                results = _lastPickupUseDownLeft;
                if (clearResults)
                {
                    _lastPickupUseDownLeft = null;
                }
            }

            return results;
        }
        
        public VRC_Pickup GetLastPickupUseUp(HandType handType, bool clearResults)
        {
            VRC_Pickup results;
            if (handType == HandType.RIGHT)
            {
                results = _lastPickupUseUpRight;
                if (clearResults)
                {
                    _lastPickupUseUpRight = null;
                }
            }
            else
            {
                results = _lastPickupUseUpLeft;
                if (clearResults)
                {
                    _lastPickupUseUpLeft = null;
                }
            }

            return results;
        }
        
        public VRCStation GetLastEnteredStation(bool clearResults)
        {
            VRCStation results = _lastEnteredStation;
            if (clearResults)
            {
                _lastEnteredStation = null;
            }
            
            return results;
        }
        
        public VRCStation GetLastExitedStation(bool clearResults = true)
        {
            VRCStation results = _lastExitedStation;
            if (clearResults)
            {
                _lastExitedStation = null;
            }

            return results;
        }
        
        #endregion

        #region Helper Actions

        public void CloseMenu()
        {
            _clientSim.GetMenu().CloseMenu();
        }

        public void OpenMenu()
        {
            _clientSim.GetMenu().OpenMenu();
        }

        public void SetMenuActive(bool active)
        {
            _clientSim.GetMenu().gameObject.SetActive(active);
        }

        public void OpenAndDisableMenu()
        {
            OpenMenu();
            SetMenuActive(false);
            
            // Move the mouse to the center of the screen.
            TestInput.SetInputLook(ClientSimBaseInput.GetScreenCenter());
        }
        
        public VRCPlayerApi SpawnRemotePlayer(string name = null)
        {
            _lastJoinPlayer = null;
            int playerCountBefore = VRCPlayerApi.AllPlayers.Count;
            
            // Spawn the player
            ClientSimMain.SpawnRemotePlayer(name);
            
            Assert.IsTrue(playerCountBefore + 1 == VRCPlayerApi.AllPlayers.Count, "Player count did not increase after spawning remote player.");
            Assert.IsNotNull(_lastJoinPlayer, "Did not spawn remote player.");
            Assert.IsFalse(_lastJoinPlayer.isLocal, "Remote player is local.");
            Assert.IsTrue(_lastJoinPlayer.IsValid(), "Remote player is not valid.");
            
            if (!string.IsNullOrEmpty(name))
            {
                Assert.IsTrue(_lastJoinPlayer.displayName.Equals(name), $"Remote player's name is not as expected. Expected: {name}, Actual: {_lastJoinPlayer.displayName}");
            }
            
            return _lastJoinPlayer;
        }

        public void RemoveRemotePlayer(VRCPlayerApi player)
        {
            Assert.IsFalse(player.isLocal, "Cannot remove local player!");
            Assert.IsNotNull(player);
            Assert.IsTrue(player.IsValid(), "Trying to remove an invalid player");
            
            _lastLeftPlayer = null;
            int playerCountBefore = VRCPlayerApi.AllPlayers.Count;
            
            // Actually remove the player
            ClientSimMain.RemovePlayer(player);
            
            Assert.IsTrue(playerCountBefore - 1 == VRCPlayerApi.AllPlayers.Count, "Player count did not decrease after remove remote player.");
            Assert.IsTrue(_lastLeftPlayer == player, "Leaving player was not the expected player.");
            Assert.IsFalse(player.IsValid(), "Removed player is still valid.");
        }

        public void RespawnPlayer()
        {
            EventDispatcher.SendEvent(new ClientSimMenuRespawnClickedEvent());
        }
        
        public void SetPlayerHeight(float playerHeight)
        {
            EventDispatcher.SendEvent(new ClientSimOnPlayerHeightUpdateEvent{ playerHeight = playerHeight});
        }
        
        public void SetTrackingScale(float trackingScale)
        {
            SetPlayerHeight(ClientSimTrackingProviderBase.CalculatePlayerHeightFromTrackingScale(trackingScale));
        }
        
        public float GetTrackingScale()
        {
            return _playerTracking.GetTrackingScale();
        }
        
        public void MoveObjectInFrontOfPlayer(Transform obj, float distance = 1f)
        {
            var trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Ray ray = new Ray(trackingData.position, trackingData.rotation * Vector3.forward);
            obj.transform.position = ray.GetPoint(distance);
        }

        public void LookAtObject(Transform obj)
        {
            LookAtPoint(obj.position);
        }

        public void LookAtPoint(Vector3 point)
        {
            _localPlayerController.LookTowardsPoint(point);
            _playerTracking.LookTowardsPoint(point);
        }

        public void PutMouseOverObject(Transform obj)
        {
            PutMouseOverWorldPoint(obj.position);
        }
        
        public void PutMouseOverWorldPoint(Vector3 point)
        {
            Camera playerCamera = _playerTracking.GetCamera();
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(point);
            TestInput.SetInputLook(screenPoint);
        }
        
        public IEnumerator WaitUntilObjectHovered(GameObject obj, HandType handType)
        {
            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    if (handType == HandType.RIGHT)
                    {
                        return _raycastResultsRight != null && _raycastResultsRight.hitObject == obj;
                    }
                    return _raycastResultsLeft != null && _raycastResultsLeft.hitObject == obj;
                },
                "Object was never hovered to interact.", 
                0.3f);
        }

        public IEnumerator WalkToPoint(
            Transform destination, 
            string failMessage,
            float maxWaitDuration = 2f,
            float distanceTolerance = 0.3f)
        {
            Vector3 destinationPoint = destination.position;
            destinationPoint.y = _playerTracking.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position.y;
            
            // Look towards the destination point and move in that direction.
            LookAtPoint(destinationPoint);
            destinationPoint.y = 0;
            
            TestInput.SetInputMoveForward(true);
            
            yield return null;
            
            yield return ClientSimTestUtils.WaitUntil(
                () =>
                {
                    Vector3 playerPos = _localPlayer.GetPosition();
                    playerPos.y = 0;
                    return Vector3.Distance(destinationPoint, playerPos) < distanceTolerance;
                },
                failMessage,
                maxWaitDuration);

            TestInput.SetInputMoveForward(false);
            
            yield return null;
        }

        public IEnumerator WalkThroughPoints(
            Transform[] points, 
            string failMessage,
            float maxWaitDuration,
            float distanceTolerance = 0.3f)
        {
            float timeRemaining = maxWaitDuration;

            for (int curPoint = 0; curPoint < points.Length; ++curPoint)
            {
                float startTime = Time.time;
                yield return WalkToPoint(
                    points[curPoint],
                    $"Point [{curPoint}]: {failMessage}",
                    timeRemaining,
                    distanceTolerance);
                timeRemaining -= Time.time - startTime;
            }
        }

        #endregion
    }
}