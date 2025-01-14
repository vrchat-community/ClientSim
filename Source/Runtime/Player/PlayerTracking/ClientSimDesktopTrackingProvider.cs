
using UnityEngine;
using VRC.SDK3.ClientSim.PlayerTracking;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    // Listens to Events:
    // - ClientSimMouseReleasedEvent
    // - ClientSimOnPlayerEnteredStationEvent
    // - ClientSimOnPlayerExitedStationEvent
    // Listens to Input Events:
    // - ToggleCrouch
    // - ToggleProne
    [AddComponentMenu("")]
    public class ClientSimDesktopTrackingProvider : ClientSimTrackingProviderBase
    {
        [SerializeField]
        private Transform playerXRotationBase;
        [SerializeField]
        private Transform playerYRotationBase;
        
        private bool _mouseReleased = false;
        private ClientSimDesktopTrackingRotator _desktopRotator;
        private IClientSimStation _currentStation;

        public override void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            IClientSimInput input, 
            ClientSimSettings settings,
            IClientSimPlayerHeightManager heightManager)
        {
            base.Initialize(eventDispatcher, input, settings, heightManager);

            SetTrackingItemPositions();
            
            _desktopRotator = new ClientSimDesktopTrackingRotator(playerXRotationBase, playerYRotationBase);
        }

        private void SetTrackingItemPositions()
        {
            head.localPosition = new Vector3(0, STANDING_HEIGHT, .1f);
            head.localRotation = Quaternion.identity;

            rightHand.localPosition = new Vector3(0.15f, -0.13f, 0.4f);
            rightHand.localRotation = Quaternion.Euler(-35, 0, -90);
            
            leftHand.localPosition = new Vector3(-0.15f, -0.13f, 0.4f);
            leftHand.localRotation = Quaternion.Euler(-35, 0, -90);
        }

        #region IClientSimInputEventSubscribable

        public override void SubscribeInputEvents()
        {
            base.SubscribeInputEvents();
            
            input.SubscribeToggleCrouch(ToggleCrouchInput);
            input.SubscribeToggleProne(ToggleProneInput);
        }

        public override void UnsubscribeInputEvents()
        {
            base.UnsubscribeInputEvents();
            
            input.UnsubscribeToggleCrouch(ToggleCrouchInput);
            input.UnsubscribeToggleProne(ToggleProneInput);
        }

        #endregion
        
        #region IClientSimInputEventSubscribable

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
            
            eventDispatcher.Subscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            eventDispatcher.Subscribe<ClientSimOnPlayerEnteredStationEvent>(PlayerEnteredStation);
            eventDispatcher.Subscribe<ClientSimOnPlayerExitedStationEvent>(PlayerExitedStation);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            
            eventDispatcher.Unsubscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            eventDispatcher.Unsubscribe<ClientSimOnPlayerEnteredStationEvent>(PlayerEnteredStation);
            eventDispatcher.Unsubscribe<ClientSimOnPlayerExitedStationEvent>(PlayerExitedStation);
        }

        #endregion
        
        #region ClientSim Events

        private void MouseReleasedEvent(ClientSimMouseReleasedEvent mouseReleasedEvent)
        {
            _mouseReleased = mouseReleasedEvent.isReleased;
        }

        private void PlayerEnteredStation(ClientSimOnPlayerEnteredStationEvent stationEvent)
        {
            _currentStation = stationEvent.station;
            _desktopRotator.SetStation(_currentStation);

            if (_currentStation.IsSeated())
            {
                SetStance(ClientSimPlayerStanceEnum.SITTING);
            }
        }
        
        private void PlayerExitedStation(ClientSimOnPlayerExitedStationEvent stationEvent)
        {
            _currentStation = null;
            _desktopRotator.SetStation(null);
            SetStance(ClientSimPlayerStanceEnum.STANDING);
        }

        #endregion

        #region ClientSim Input

        private void ToggleCrouchInput(bool value)
        {
            // Only handle on down, and not on release.
            if (!value)
            {
                return;
            }
            
            if (GetPlayerStance() == ClientSimPlayerStanceEnum.CROUCHING)
            {
                SetStance(ClientSimPlayerStanceEnum.STANDING);
            }
            else
            {
                SetStance(ClientSimPlayerStanceEnum.CROUCHING);
            }
        }
        
        private void ToggleProneInput(bool value)
        {
            // Only handle on down, and not on release.
            if (!value)
            {
                return;
            }
            
            if (GetPlayerStance() == ClientSimPlayerStanceEnum.PRONE)
            {
                SetStance(ClientSimPlayerStanceEnum.STANDING);
            }
            else
            {
                SetStance(ClientSimPlayerStanceEnum.PRONE);
            }
        }

        #endregion

        private void Update()
        {
            // If mouse is released, do not update rotation.
            if (!_mouseReleased)
            {
                _desktopRotator.HandleRotation(input.GetLookHorizontal(), input.GetLookVertical());
            }
        }

        private void SetStance(ClientSimPlayerStanceEnum stance)
        {
            // If in a station, ignore all non sitting stances.
            if (_currentStation != null && _currentStation.IsLockedInStation() && stance != ClientSimPlayerStanceEnum.SITTING)
            {
                return;
            }

            Vector3 cameraPosition = head.localPosition;
            switch (stance)
            {
                case ClientSimPlayerStanceEnum.PRONE:
                    cameraPosition.y = PRONE_HEIGHT;
                    break;
                case ClientSimPlayerStanceEnum.CROUCHING:
                    cameraPosition.y = CROUCHING_HEIGHT;
                    break;
                case ClientSimPlayerStanceEnum.SITTING:
                    cameraPosition.y = SITTING_HEIGHT;
                    break;
                case ClientSimPlayerStanceEnum.STANDING:
                    cameraPosition.y = STANDING_HEIGHT;
                    break;
            }
            
            head.localPosition = cameraPosition;
        }

        public override Transform GetHandRaycastTransform(HandType handType)
        {
            throw new ClientSimException("Desktop tracking does not support arm based raycasting");
        }

        public override bool IsVR()
        {
            return false;
        }

        public override bool SupportsPickupAutoHold()
        {
            return true;
        }

        public override void LookTowardsPoint(Vector3 point)
        {
            _desktopRotator.LookAtPoint(point);
        }
    }
}