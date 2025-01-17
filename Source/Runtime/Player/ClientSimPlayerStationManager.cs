
using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This system is responsible for the player entering and exiting stations as well as repositioning the player to the station at the end of frame.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimOnPlayerEnteredStationEvent
    /// - ClientSimOnPlayerExitedStationEvent
    /// Listens to Events:
    /// - ClientSimOnPlayerTeleportedEvent
    /// </remarks>
    [AddComponentMenu("")]
    // High execution order to ensure the player is positioned properly at the end of the frame.
    [DefaultExecutionOrder(30000)]
    public class ClientSimPlayerStationManager : ClientSimBehaviour, IClientSimPlayerStationManager, IDisposable
    {
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimPlayerApiProvider _playerApiProvider;
        private ClientSimPlayerController _playerController;
        private IClientSimStation _currentStation;

        protected override void Awake()
        {
            base.Awake();
            _playerController = GetComponent<ClientSimPlayerController>();
        }

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimPlayerApiProvider playerApiProvider)
        {
            _eventDispatcher = eventDispatcher;
            _playerApiProvider = playerApiProvider;

            _eventDispatcher.Subscribe<ClientSimOnPlayerTeleportedEvent>(OnPlayerTeleported);

            // Only enable this object while sitting in a station to prevent unneeded update checks.
            enabled = false;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerTeleportedEvent>(OnPlayerTeleported);
        }

        private void Update()
        {
            UpdateStationPosition();
        }
        
        private void LateUpdate()
        {
            // VRChatBug: VRChat seems to not handle the rotation in late update causing player's rotation to jitter
            // while in a station that is updated in late update. This is not recreated here.
            UpdateStationPosition();
        }
        
        private void FixedUpdate()
        {
            UpdateStationPosition();
        }

        public bool InStation()
        {
            return _currentStation != null;
        }

        public IClientSimStation GetCurrentStation()
        {
            return _currentStation;
        }

        public bool IsLockedInStation()
        {
            return InStation() && _currentStation.IsLockedInStation();
        }

        public bool CanPlayerMove(float moveValue)
        {
            return !InStation() || CanPlayerMoveWhileSeated(moveValue);
        }

        public void EnterStation(IClientSimStation station)
        {
            GameObject stationObj = station.GetStationGameObject();
            if (_currentStation != null)
            {
                // VRChatBug: VRCStation.CanUseStationFromStation does not care about actually trying to enter stations. 
                // This will only block the interact on the object. Calling enter station while sitting in a station
                // with this property set to false will still allow you to exit the current station and enter the new one.
                ExitStation(_currentStation, true);
            }
            
            VRCPlayerApi player = _playerApiProvider.Player;
            
            this.Log($"Entering Station {Tools.GetGameObjectPath(stationObj)}");

            // Immobilize the player while sitting in the station. 
            // VRChatBug: Note that "mobile" stations require that seated be set to false and have mobility set to mobile.
            if (!station.IsMobile())
            {
                player.Immobilize(true);
            }

            station.EnterStation(player);

            _playerController.EnterStation(station);
            
            // Set the station after notifying the player controller to prevent exit on Teleport.
            _currentStation = station;
            enabled = true;
            
            // Notify all station handlers after notifying to ensure that player location is updated first.
            var vrcStation = station.GetStation();
            foreach (var stationHandler in stationObj.GetComponents<IClientSimStationHandler>())
            {
                stationHandler.OnStationEnter(vrcStation);
            }
            
            _eventDispatcher.SendEvent(new ClientSimOnPlayerEnteredStationEvent {player = player, station = station});
        }

        public void ExitStation(IClientSimStation station, bool forcedExit = false)
        {
            // Prevent Exception on exit playmode when this object is destroyed.
            if (this == null)
            {
                return;
            }
            
            GameObject stationObj = station.GetStationGameObject();
            if (_currentStation != station)
            {
                this.LogError($"Cannot exit station that the player is not in.  {Tools.GetGameObjectPath(stationObj)}");
                return;
            }

            _currentStation = null;
            enabled = false;
            
            this.Log($"Exiting Station {Tools.GetGameObjectPath(stationObj)}");

            VRCPlayerApi player = _playerApiProvider.Player;
            
            player.Immobilize(false);
            
            station.ExitStation(player);
            
            // Notify all station handlers first as player exit is handled after this event.
            var vrcStation = station.GetStation();
            foreach (var stationHandler in stationObj.GetComponents<IClientSimStationHandler>())
            {
                stationHandler.OnStationExit(vrcStation);
            }
            
            _playerController.ExitStation(station, forcedExit);
            
            _eventDispatcher.SendEvent(new ClientSimOnPlayerExitedStationEvent {player = player, station = station});
        }
        
        private void UpdateStationPosition()
        {
            if (!InStation() || _currentStation.IsMobile())
            {
                return;
            }

            _playerController.SitPosition(_currentStation.EnterLocation());
        }
        
        // Returns if the player should move, and exit station if the player is in a non mobile station with exit enabled.
        private bool CanPlayerMoveWhileSeated(float speed)
        {
            if (Mathf.Abs(speed) >= 0.1f && !_currentStation.DisableStationExit())
            {
                ExitStation(_currentStation);
                return true;
            }

            if (_currentStation.IsMobile())
            {
                return true;
            }

            return false;
        }

        #region ClientSim Events

        // Note that respawn is considered a teleport and will automatically be handled by this event.
        private void OnPlayerTeleported(ClientSimOnPlayerTeleportedEvent teleportEvent)
        {
            if (InStation())
            {
                ExitStation(_currentStation, true);
            }
        }

        #endregion
    }
}