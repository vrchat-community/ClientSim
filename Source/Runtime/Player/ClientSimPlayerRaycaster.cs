
using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This system is responsible for handling finding objects that can be interacted with or picked up.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimRaycastHitResultsEvent
    /// - ClientSimInteractEvent
    /// Listens to Events:
    /// - ClientSimOnPlayerMovedEvent 
    /// - ClientSimPlayerDeathStatusChangedEvent
    /// Listens to Input Events:
    /// - Use
    /// </remarks>
    [AddComponentMenu("")]
    // Unity Event System Updates at -1000. Send raycast events before then to ensure UI interactions happen same frame.
    [DefaultExecutionOrder(-2000)] 
    public class ClientSimPlayerRaycaster : ClientSimBehaviour, IDisposable
    {
        [SerializeField]
        private ClientSimPlayerHand leftHand;
        [SerializeField]
        private ClientSimPlayerHand rightHand;

        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimInput _input;
        private IClientSimPlayerApiProvider _playerApiProvider;
        private IClientSimHighlightManager _highlightManager;
        private IClientSimTooltipManager _tooltipManager;
        private IClientSimInteractiveLayerProvider _interactiveLayerProvider;
        private IClientSimInteractManager _interactManager;
        private IClientSimTrackingProvider _trackingProvider;
        private IClientSimPlayerStationManager _stationManager;

        private ClientSimRaycaster _leftHandRaycaster;
        private ClientSimRaycaster _rightHandRaycaster;

        private ClientSimRaycastResults _hoverLeft;
        private ClientSimRaycastResults _hoverRight;
        
        public void Initialize(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimInput input,
            IClientSimPlayerApiProvider playerApiProvider,
            IClientSimPlayerPickupData pickupData,
            IClientSimHighlightManager highlightManager,
            IClientSimTooltipManager tooltipManager,
            IClientSimInteractiveLayerProvider interactiveLayerProvider,
            IClientSimPlayerCameraProvider cameraProvider,
            IClientSimMousePositionProvider mousePositionProvider,
            IClientSimInteractManager interactManager,
            IClientSimTrackingProvider trackingProvider,
            IClientSimPlayerStationManager stationManager)
        {
            _eventDispatcher = eventDispatcher;
            _input = input;
            _playerApiProvider = playerApiProvider;
            _highlightManager = highlightManager;
            _tooltipManager = tooltipManager;
            _interactiveLayerProvider = interactiveLayerProvider;
            _interactManager = interactManager;
            _trackingProvider = trackingProvider;
            _stationManager = stationManager;
            
            leftHand.Initialize(_eventDispatcher, _input, trackingProvider, _playerApiProvider, pickupData);
            rightHand.Initialize(_eventDispatcher, _input, trackingProvider, _playerApiProvider, pickupData);
            
            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeUse(UseInput);
            _eventDispatcher.Subscribe<ClientSimOnPlayerMovedEvent>(OnPlayerMoved);
            _eventDispatcher.Subscribe<ClientSimPlayerDeathStatusChangedEvent>(CombatStatusEvent);
            
            // Create raycasters
            if (_trackingProvider.IsVR())
            {
                _leftHandRaycaster = new ClientSimRaycaster(
                    new ClientSimTransformRayProvider(_trackingProvider.GetHandRaycastTransform(HandType.LEFT)),
                    _interactiveLayerProvider, 
                    _interactManager);
            
                _rightHandRaycaster = new ClientSimRaycaster(
                    new ClientSimTransformRayProvider(_trackingProvider.GetHandRaycastTransform(HandType.RIGHT)),
                    _interactiveLayerProvider, 
                    _interactManager);
            }
            else
            {
                // Left hand is always null for desktop users.
                _leftHandRaycaster = null;
                
                // Right hand is from the player's camera.
                _rightHandRaycaster = new ClientSimRaycaster(
                    new ClientSimCameraRayProvider(cameraProvider, mousePositionProvider), 
                    _interactiveLayerProvider, 
                    _interactManager);
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _input?.UnsubscribeUse(UseInput);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerMovedEvent>(OnPlayerMoved);
            _eventDispatcher.Unsubscribe<ClientSimPlayerDeathStatusChangedEvent>(CombatStatusEvent);
        }

        private void Update()
        {
            UpdateHandPositions();

            SetHoverLeft(_leftHandRaycaster?.CheckForInteracts());
            SetHoverRight(_rightHandRaycaster.CheckForInteracts());
        }
        
        private void SetHoverLeft(ClientSimRaycastResults raycastResults)
        {
            SetHover(ref _hoverLeft, HandType.LEFT, leftHand, raycastResults);
        }
        
        private void SetHoverRight(ClientSimRaycastResults raycastResults)
        {
            SetHover(ref _hoverRight, HandType.RIGHT, rightHand, raycastResults);
        }
        
        private void SetHover(
            ref ClientSimRaycastResults handHover, 
            HandType handType,
            ClientSimPlayerHand playerHand, 
            ClientSimRaycastResults raycastResults)
        {
            // TODO optimize this to check if previous was the same as new and not disable/re-enable.
            if (handHover != null && handHover.interactable != null)
            {
                _highlightManager.DisableObjectHighlight(handHover.hitObject);
                _tooltipManager.DisableTooltip(handHover.interactable);
            }

            raycastResults = FilterRaycastResults(raycastResults);
            
            // If the player is not holding something, or the hit object is a UIShape, set the hover.
            // This allows players to interact with UI while still holding objects,
            // but holding objects will block pickups and interacts.
            if (!playerHand.IsHolding() || (raycastResults != null && raycastResults.uiShape != null))
            {
                handHover = raycastResults;
            }
            else
            {
                handHover = null;
            }

            // Highlight the object if it has an interactable
            if (handHover != null && handHover.interactable != null)
            {
                _highlightManager.EnableObjectHighlight(handHover.hitObject);
                _tooltipManager.DisplayTooltip(handHover.interactable);
            }

            // If the hovered object has a pickupable that can be interacted with, set that as this hand's hovered pickup.
            IClientSimPickupable pickupable = handHover?.GetPickupable();
            if (pickupable != null && !_interactManager.CanInteract(pickupable, handHover.distance))
            {
                pickupable = null;
            }
            playerHand.SetHoverPickupable(pickupable);
            
            _eventDispatcher.SendEvent(new ClientSimRaycastHitResultsEvent
            {
                handType = handType,
                raycastResults = handHover
            });
        }

        private ClientSimRaycastResults FilterRaycastResults(ClientSimRaycastResults results)
        {
            if (results == null || results.hitObject == null)
            {
                return results;
            }
            
            // If the user is in a station with "CanUseStationFromStation" set to false, disable interacts
            // for any object that contains a station script.
            if (_stationManager.InStation() 
                && !_stationManager.GetCurrentStation().CanUseStationFromStation()
                && results.hitObject.GetComponent<IClientSimStation>() != null)
            {
                return null;
            }

            return results;
        }

        private void TryInteract(HandType handType, ClientSimRaycastResults hover)
        {
            // Nothing to interact with.
            if (hover == null || hover.interactable == null)
            {
                return;
            }
            
            // Interact with the object and get the components that were interacted with.
            var interacts = _interactManager.Interact(hover.hitObject, hover.distance);
                
            // Notify ClientSim of interacted objects.
            _eventDispatcher.SendEvent(new ClientSimInteractEvent
            {
                handType = handType,
                interactObject = hover.hitObject,
                interactDistance = hover.distance,
                interacts = interacts,
            });
        }

        private void UpdateHandPositions()
        {
            // TODO do not update hands if player controller is stuck in a collider.
            // Note that these hands are only for pickups and that raycast logic uses the tracking provider hands. 
            
            var leftHandData = _trackingProvider.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            leftHand.transform.SetPositionAndRotation(leftHandData.position, leftHandData.rotation);
            leftHand.UpdatePosition();
            
            var rightHandData = _trackingProvider.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            rightHand.transform.SetPositionAndRotation(rightHandData.position, rightHandData.rotation);
            rightHand.UpdatePosition();
        }

        #region ClientSim Input

        private void UseInput(bool value, HandType hand)
        {
            if (!value)
            {
                return;
            }

            if (hand == HandType.LEFT)
            {
                TryInteract(HandType.LEFT, _hoverLeft);
            }
            if (hand == HandType.RIGHT)
            {
                TryInteract(HandType.RIGHT, _hoverRight);
            }
        }
        
        #endregion

        #region ClientSim Events

        private void OnPlayerMoved(ClientSimOnPlayerMovedEvent moveEvent)
        {
            UpdateHandPositions();
        }

        private void CombatStatusEvent(ClientSimPlayerDeathStatusChangedEvent combatStatusEvent)
        {
            leftHand.ForceDrop();
            rightHand.ForceDrop();
        }

        #endregion
    
    }
}