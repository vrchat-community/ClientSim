using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This system is responsible for handling pickups.
    /// </summary>
    /// <remarks>
    /// Sends Events:
    /// - ClientSimOnPickupEvent
    /// - ClientSimOnPickupDropEvent
    /// - ClientSimOnPickupUseDownEvent
    /// - ClientSimOnPickupUseUpEvent
    /// Listens to Input Events:
    /// - Grab
    /// - Use
    /// - Drop
    /// </remarks>
    [AddComponentMenu("")]
    public class ClientSimPlayerHand : ClientSimBehaviour, IDisposable
    {
        // The duration after picking up an object to start sending pickup UseDown and UseUp events.
        private const float INITIAL_PICKUP_DURATION = 0.5f;
        // The distance at which a pickup will force snap to the hand.
        private const float MAX_PICKUP_DISTANCE = 0.25f;
        // How many units will the object rotate during manipulation.
        private const float DESKTOP_ROTATION_MULTIPLIER = 2f;
        // How many units will an object be moved forward or backwards during manipulation.
        private const float DESKTOP_MANIPULATE_MULTIPLIER = 0.01f;
        // How far can the object move away from the hand during manipulation.
        private const float DESKTOP_MANIPULATION_MAX_DISTANCE = 0.64f;
        
        private static readonly Quaternion _gripOffsetRotation = Quaternion.Euler(0, 35, 0);
        private static readonly Quaternion _gunOffsetRotation = Quaternion.Euler(0, 305, 0);
        private static readonly Quaternion _desktopManipulationRotation = Quaternion.Euler(180, 35, 90);
        
        [SerializeField]
        private HandType handType;
        [SerializeField]
        private Transform handTransform;
        [SerializeField] 
        private ClientSimPlayerHand otherHand;

        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimInput _input;
        private IClientSimTrackingProvider _trackingProvider;
        private IClientSimPlayerApiProvider _player;
        private IClientSimPlayerPickupData _pickupData;
        private VRC_Pickup.PickupHand _pickupHandType;

        // The object this hand hovering to know if it should pickup an object.
        private IClientSimPickupable _hoverPickupable;
        // The object currently held by this hand.
        private IClientSimPickupable _heldPickupable;
        private Rigidbody _heldPickupRigidbody;
        private Transform _heldPickupTransform;
        private GameObject _heldPickupGameObject;
        private FixedJoint _heldPickupJoint;
        
        // Used for determining pickup throw
        private Vector3 _previousHandPosition;
        private Vector3 _previousHandRotation;
        
        // Check if the use input is down (true) or up (false)
        private bool _useInputHeldDown;
        // Has this pickup fired the UseDown event, to know if we need to fire the UseUp event.
        private bool _isUseDown;

        private bool _initialGrab;
        private float _grabActionStartTime;
        private float _dropActionStartTime;

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimInput input, 
            IClientSimTrackingProvider trackingProvider,
            IClientSimPlayerApiProvider player, 
            IClientSimPlayerPickupData pickupData)
        {
            _eventDispatcher = eventDispatcher;
            _input = input;
            _trackingProvider = trackingProvider;
            _player = player;
            _pickupData = pickupData;

            // Too many hand enums...
            _pickupHandType = (handType == HandType.LEFT ? VRC_Pickup.PickupHand.Left : VRC_Pickup.PickupHand.Right);
            
            enabled = false; // Only enabled while holding something to reduce Update checks.

            // Subscribe to input events
            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeGrab(GrabInput);
            _input?.SubscribeUse(UseInput);
            _input?.SubscribeDrop(DropInput);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            // Unsubscribe
            _input?.UnsubscribeGrab(GrabInput);
            _input?.UnsubscribeUse(UseInput);
            _input?.UnsubscribeDrop(DropInput);
        }

        private void Update()
        {
            if (!IsHolding())
            {
                return;
            }
            
            if (_initialGrab && _useInputHeldDown)
            {
                HandleUseInput();
            }

            UpdateManipulation();
            UpdatePosition();

            _previousHandPosition = handTransform.position;
            _previousHandRotation = handTransform.rotation.eulerAngles;
        }


        #region ClientSim Input

        private void GrabInput(bool value, HandType hand)
        {
            if (hand != handType)
            {
                return;
            }
            
            if (value)
            {
                // Try to grab hover object
                if (!IsHolding() && _hoverPickupable != null)
                {
                    Pickup(_hoverPickupable);
                }
            }
            else
            {
                // If releasing grab input and holding a pickup that is not auto hold, drop the pickup.
                if (IsHolding() && !ShouldAutoHoldPickupable(_heldPickupable))
                {
                    ForceDrop(_heldPickupable);
                }
            }
        }
        
        private void UseInput(bool value, HandType hand)
        {
            if (hand != handType)
            {
                return;
            }

            // Save input state to know when we can fire the first UseDown event after being picked up. See Update.
            _useInputHeldDown = value;

            if (!IsHolding())
            {
                return;
            }
            
            HandleUseInput();
        }
        
        private void DropInput(bool value, HandType hand)
        {
            if (hand != handType)
            {
                return;
            }

            // Do not try to drop anything if not holding a pickup.
            if (!IsHolding())
            {
                return;
            }
            
            if (value)
            {
                // Button was just pressed. Start a timer to know how much "throw" charge should be added.
                _dropActionStartTime = Time.time;
            }
            else
            {
                Drop(_heldPickupable, Time.time - _dropActionStartTime);
            }
        }

        #endregion

        public void SetHoverPickupable(IClientSimPickupable pickupable)
        {
            _hoverPickupable = pickupable;
        }
        
        public bool IsHolding()
        {
            return _heldPickupable != null;
        }

        private bool ShouldAutoHoldPickupable(IClientSimPickupable pickupable)
        {
            // Some VR controllers do not support auto hold.
            return pickupable.AutoHold() && _trackingProvider.SupportsPickupAutoHold();
        }

        private void Pickup(IClientSimPickupable pickupable)
        {
            if (IsHolding())
            {
                LogErrorMessage("Cannot pickup a pickup while holding another.");
                return;
            }

            if (pickupable.IsHeld())
            {
                // Allow yourself to grab a pickup from your other hand. 
                if (otherHand != null && otherHand._heldPickupable == pickupable)
                {
                    otherHand.ForceDrop(pickupable);
                }
                else
                {
                    LogErrorMessage("Cannot pickup a pickup someone else is holding.");
                    return;
                }
            }

            handTransform.localPosition = Vector3.zero;
            handTransform.localRotation = Quaternion.identity;

            _heldPickupable = pickupable;
            _heldPickupTransform = pickupable.GetTransform();
            _heldPickupGameObject = pickupable.GetGameObject();
            _heldPickupRigidbody = pickupable.GetRigidbody();
            
            LogMessage($"Picking up object {_heldPickupGameObject.name}");
            
            VRC_Pickup pickup = pickupable.GetPickup();
            _pickupData.SetPickupInHand(_pickupHandType, pickup);
            pickupable.Pickup(_player.Player, _pickupHandType, ForceDrop);
            
            // Set the grab time to know if the player has held long enough to send Use events
            _grabActionStartTime = Time.time;
            _initialGrab = true;
            // Set self enabled to allow for pickup manipulation
            enabled = true;
            

            VRC_Pickup.PickupOrientation pickupOrientation = pickupable.GetOrientation();
            Transform pickupExactGrip = pickupable.GetGripLocation();
            Transform pickupExactGun = pickupable.GetGunLocation();
            
            // Calculate offset
            Transform pickupHoldPoint = null;
            Quaternion offsetRotation = Quaternion.identity;
            if (pickupOrientation == VRC_Pickup.PickupOrientation.Grip && pickupExactGrip != null)
            {
                pickupHoldPoint = pickupExactGrip;
                offsetRotation = _gripOffsetRotation;
            }
            else if (pickupOrientation == VRC_Pickup.PickupOrientation.Gun && pickupExactGun != null)
            {
                pickupHoldPoint = pickupExactGun;
                offsetRotation = _gunOffsetRotation;
            }
        
            
            Vector3 positionOffset;
            Quaternion rotationOffset;

            // Grab as if no pickup point
            if (pickupHoldPoint == null)
            {
                rotationOffset = Quaternion.Inverse(handTransform.rotation) * _heldPickupTransform.rotation;
                positionOffset = handTransform.InverseTransformDirection(_heldPickupTransform.position - handTransform.position);

                if (positionOffset.magnitude > MAX_PICKUP_DISTANCE && pickupOrientation == VRC_Pickup.PickupOrientation.Any)
                {
                    positionOffset = positionOffset.normalized * MAX_PICKUP_DISTANCE;
                }
            }
            else
            {
                rotationOffset = offsetRotation * Quaternion.Inverse(Quaternion.Inverse(_heldPickupTransform.rotation) * pickupHoldPoint.rotation);
                positionOffset = rotationOffset * _heldPickupTransform.InverseTransformDirection(_heldPickupTransform.position - pickupHoldPoint.position);
            }
            
            
            Vector3 position = handTransform.position + handTransform.TransformDirection(positionOffset);
            Quaternion rotation = handTransform.rotation * rotationOffset;

            // Move hand and pickup to the same location
            handTransform.position = _heldPickupTransform.position = position;
            handTransform.rotation = _heldPickupTransform.rotation = rotation;
            
            // Link with hand rigidbody
            _heldPickupJoint = handTransform.gameObject.AddComponent<FixedJoint>();
            _heldPickupJoint.connectedBody = _heldPickupRigidbody;
            
            // Set the owner of this object to the player picking it up.
            Networking.SetOwner(_player.Player, _heldPickupGameObject);
            
            _eventDispatcher.SendEvent(new ClientSimOnPickupEvent
            {
                player = _player.Player,
                handType = handType,
                pickup = pickupable,
            });
            
            // Notify pickup handlers of object pickup.
            foreach (var pickupHandler in _heldPickupGameObject.GetComponents<IClientSimPickupHandler>())
            {
                pickupHandler.OnPickup();
            }
        }

        public void ForceDrop()
        {
            if (_heldPickupable != null)
            {
                ForceDrop(_heldPickupable);
            }
        }
        
        private void ForceDrop(IClientSimPickupable pickupable)
        {
            Drop(pickupable, 0);
        }
        
        private void Drop(IClientSimPickupable pickupable, float throwHoldDuration)
        {
            if (_heldPickupable != pickupable || !pickupable.IsHeld() || pickupable.GetHoldingPlayer() != _player.Player)
            {
                LogErrorMessage("Cannot drop a pickup that you aren't holding.");
                return;
            }

            // Ensure that UseUp is called before the drop event finishes. 
            OnPickupUseUp();
            
            // Check to return early and ensure no errors if OnPickupUseUp calls Drop.
            if (_heldPickupable != pickupable || !pickupable.IsHeld() || pickupable.GetHoldingPlayer() != _player.Player)
            {
                return;
            }
            
            LogMessage($"Dropping object {_heldPickupGameObject.name}");

            // Unlink from arm rigidbody
            if (_heldPickupJoint != null)
            {
                Destroy(_heldPickupJoint);
            }
            
            // When exiting playmode while holding an object, Drop will be called and Time.deltaTime will be 0.
            // This check prevents setting the velocity to NaN due to divide by zero.
            if (Time.deltaTime > 0)
            {
                _heldPickupRigidbody.velocity = (handTransform.position - _previousHandPosition) * (0.5f / Time.deltaTime);
                _heldPickupRigidbody.angularVelocity = (handTransform.rotation.eulerAngles - _previousHandRotation);
            }
            
            
            // Calculate throw velocity
            // TODO Verify how VR handles throwing pickups
            if (!_heldPickupRigidbody.isKinematic)
            {
                float holdDuration = Mathf.Clamp(throwHoldDuration, 0, 3);
                if (holdDuration > 0.2f)
                {
                    float power = holdDuration * 500 * pickupable.GetThrowVelocityBoostScale();
                    Vector3 throwForce = power * transform.TransformDirection(_gripOffsetRotation * Vector3.forward);
                    _heldPickupRigidbody.AddForce(throwForce);
                    LogMessage($"Adding throw force: {throwForce}");
                }
            }

            pickupable.Drop(_player.Player);
            _pickupData.SetPickupInHand(_pickupHandType, null);
            
            _eventDispatcher.SendEvent(new ClientSimOnPickupDropEvent
            {
                player = _player.Player,
                handType = handType,
                pickup = pickupable,
            });
            
            // Notify pickup handlers that the object has been dropped.
            foreach (var pickupHandler in _heldPickupGameObject.GetComponents<IClientSimPickupHandler>())
            {
                pickupHandler.OnDrop();
            }
            
            _heldPickupable = null;
            _heldPickupTransform = null;
            _heldPickupGameObject = null;
            _heldPickupRigidbody = null;

            // Prevent throwing an exception when exiting playmode due to this object being destroyed.
            if (this != null)
            {
                enabled = false;
            }
            
            handTransform.localPosition = Vector3.zero;
            handTransform.localRotation = Quaternion.identity;
        }

        private bool HeldLongEnoughForUseEvents()
        {
            float grabDuration = Time.time - _grabActionStartTime;
            return grabDuration >= INITIAL_PICKUP_DURATION;
        }
        
        private void HandleUseInput()
        {
            // Grab time has not been long enough to send use events
            if (!HeldLongEnoughForUseEvents())
            {
                return;
            }

            // Only auto hold pickups can be used.
            if (!_heldPickupable.AutoHold())
            {
                return;
            }

            if (_useInputHeldDown)
            {
                OnPickupUseDown();
            }
            else
            {
                OnPickupUseUp();
            }
        }
        
        private void OnPickupUseDown()
        {
            LogMessage($"Pickup Use Down {_heldPickupGameObject.name}");
            _initialGrab = false;
            _isUseDown = true;
            
            _eventDispatcher.SendEvent(new ClientSimOnPickupUseDownEvent
            {
                player = _player.Player,
                handType = handType,
                pickup = _heldPickupable,
            });
            
            // Notify pickup handlers that the object has Use Down.
            foreach (var pickupHandler in _heldPickupGameObject.GetComponents<IClientSimPickupHandler>())
            {
                pickupHandler.OnPickupUseDown();
            }
        }

        private void OnPickupUseUp()
        {
            // Prevent calling UseUp if UseDown was never called.
            if (!_isUseDown)
            {
                return;
            }
            
            LogMessage($"Pickup Use Up {_heldPickupGameObject.name}");
            _isUseDown = false;
            
            _eventDispatcher.SendEvent(new ClientSimOnPickupUseUpEvent
            {
                player = _player.Player,
                handType = handType,
                pickup = _heldPickupable,
            });
            
            // Notify pickup handlers that the object has Use Up.
            foreach (var pickupHandler in _heldPickupGameObject.GetComponents<IClientSimPickupHandler>())
            {
                pickupHandler.OnPickupUseUp();
            }
        }
        
        // Apply desktop hand rotations
        private void UpdateManipulation()
        {
            if (handType != HandType.RIGHT || !_heldPickupable.AllowManipulation())
            {
                return;
            }
            
            // Get the input for rotating the pickup.
            Vector3 angles = new Vector3(
                _input.GetPickupRotateUpDown(),
                _input.GetPickupRotateLeftRight(),
                _input.GetPickupRotateCwCcw());

            // Only apply rotation if some input has been detected. 
            if (angles.sqrMagnitude > 0)
            {
                // Rotate the input angles to match rotation based on desktop view.
                angles = transform.rotation * _desktopManipulationRotation * angles;
                
                // Apply rotation to hand.
                handTransform.Rotate(angles, DESKTOP_ROTATION_MULTIPLIER,Space.World);
            }

            // Move pickup forward and back.
            float manipulateForwardBack = _input.GetPickupManipulateDistance();
            if (!Mathf.Approximately(manipulateForwardBack, 0))
            {
                Vector3 forward = _gripOffsetRotation * Vector3.forward;
                Vector3 offset = forward * Mathf.Sign(manipulateForwardBack) * DESKTOP_MANIPULATE_MULTIPLIER;
                Vector3 handLocal = handTransform.localPosition + offset;
                handLocal = Vector3.ClampMagnitude(handLocal, DESKTOP_MANIPULATION_MAX_DISTANCE);
                handTransform.localPosition = handLocal;
            }
        }
        
        public void UpdatePosition(bool force = false)
        {
            if ((_heldPickupRigidbody != null && _heldPickupRigidbody.isKinematic) || force)
            {
                _heldPickupTransform.SetPositionAndRotation(handTransform.position, handTransform.rotation);
            }
        }

        private void LogMessage(string message)
        {
            this.Log($"[{handType}] {message}");
        }
        
        private void LogErrorMessage(string message)
        {
            this.LogError($"[{handType}] {message}");
        }
    }
}