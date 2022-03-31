
using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    // Sends Events:
    // - ClientSimOnPlayerTeleportedEvent
    // - ClientSimOnPlayerRespawnEvent  
    // - ClientSimOnPlayerMovedEvent
    // Listens to Events:
    // - ClientSimMenuStateChangedEvent 
    // - ClientSimMenuRespawnClickedEvent
    // - ClientSimMouseReleasedEvent
    // - ClientSimPlayerDeathStatusChangedEvent
    // - ClientSimOnTrackingScaleUpdateEvent
    // Listens to Input Events:
    // - Jump
    // - Run
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-3000)] // Update before player raycasting
    public class ClientSimPlayerController : ClientSimBehaviour, IDisposable
    {
        private const float CROUCH_SPEED_MULTIPLIER = 0.35f;
        private const float PRONE_SPEED_MULTIPLIER = 0.15f;

        private const float STICK_TO_GROUND_FORCE = 2f;
        private const float RATE_OF_AIR_ACCELERATION = 5f;

        private IClientSimPlayerStationManager _stationManager;
        private IClientSimPlayerLocomotionData _playerLocomotionData;
        private IClientSimPlayerApiProvider _playerApi;
        private IClientSimSceneManager _sceneManager;
        private IClientSimEventDispatcher _eventDispatcher;
        private IClientSimInput _input;
        private IClientSimTrackingProvider _trackingProvider;
        
        private CharacterController _characterController;
        private Transform _cameraProxyObject;

        private bool _isDead;
        private bool _isWalking = true; // Player defaults to walking
        private bool _jump;
        
        private Vector2 _prevInput;
        // Check if the directionality changed to apply "stutter stepping" for legacy locomotion.
        private bool _directionChanged;

        private bool _velSet;
        // TODO fix handling of SetVelocity as retaining the velocity causes strange bugs due to ignoring collisions
        // that normally would stop the player.
        private Vector3 _playerRetainedVelocity;

        private bool _menuIsOpen;
        private bool _mouseReleased;

        protected override void Awake()
        {
            base.Awake();

            _characterController = GetComponent<CharacterController>();
        }

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimInput input,
            IClientSimPlayerApiProvider playerApiProvider,
            IClientSimPlayerLocomotionData locomotionData,
            IClientSimSceneManager sceneManager,
            IClientSimProxyObjectProvider proxyProvider,
            IClientSimTrackingProvider trackingProvider,
            IClientSimPlayerStationManager stationManager)
        {
            _eventDispatcher = eventDispatcher;
            _input = input;
            _playerApi = playerApiProvider;
            _playerLocomotionData = locomotionData;
            _sceneManager = sceneManager;
            _trackingProvider = trackingProvider;
            _stationManager = stationManager;

            _cameraProxyObject = proxyProvider.CameraProxy().transform;
            
            Subscribe();
        }
        
        private void Start()
        {
            NotifyPlayerMoved();
        }

        private void OnDestroy()
        {
            Dispose();
        }
        
        private void Subscribe()
        {
            // Input will be null with incorrect Unity input project settings.
            _input?.SubscribeJump(JumpInput);
            _input?.SubscribeRun(RunInput);

            _eventDispatcher.Subscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            _eventDispatcher.Subscribe<ClientSimMenuRespawnClickedEvent>(MenuRespawnEvent);
            _eventDispatcher.Subscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            _eventDispatcher.Subscribe<ClientSimPlayerDeathStatusChangedEvent>(CombatStatusEvent);
            _eventDispatcher.Subscribe<ClientSimOnTrackingScaleUpdateEvent>(OnTrackingScaleUpdated);
        }
        
        public void Dispose()
        {
            _input?.UnsubscribeJump(JumpInput);
            _input?.UnsubscribeRun(RunInput);

            _eventDispatcher.Unsubscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
            _eventDispatcher.Unsubscribe<ClientSimMenuRespawnClickedEvent>(MenuRespawnEvent);
            _eventDispatcher.Unsubscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            _eventDispatcher.Unsubscribe<ClientSimPlayerDeathStatusChangedEvent>(CombatStatusEvent);
            _eventDispatcher.Unsubscribe<ClientSimOnTrackingScaleUpdateEvent>(OnTrackingScaleUpdated);
        }

        #region Input Events

        private void JumpInput(bool value, HandType hand)
        {
            // Only handle on down, and not on release.
            if (!value)
            {
                return;
            }

            if (!_jump && _characterController.isGrounded && _playerLocomotionData.GetJump() > 0)
            {
                _jump = true;
            }
        }

        private void RunInput(bool value)
        {
            _isWalking = !value;
        }

        #endregion

        #region ClientSim Events

        private void SetMenuOpen(ClientSimMenuStateChangedEvent stateChangedEvent)
        {
            _menuIsOpen = stateChangedEvent.isMenuOpen;
        }
        
        private void MenuRespawnEvent(ClientSimMenuRespawnClickedEvent stateChangedEvent)
        {
            Respawn();
        }
        
        private void MouseReleasedEvent(ClientSimMouseReleasedEvent mouseReleasedEvent)
        {
            _mouseReleased = mouseReleasedEvent.isReleased;
        }
        
        private void CombatStatusEvent(ClientSimPlayerDeathStatusChangedEvent combatStatusEvent)
        {
            _isDead = combatStatusEvent.isDead;
        }

        private void OnTrackingScaleUpdated(ClientSimOnTrackingScaleUpdateEvent scaleUpdatedEvent)
        {
            NotifyPlayerMoved();
        }

        #endregion
        
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        public Vector3 GetVelocity()
        {
            return _characterController.velocity;
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            _playerRetainedVelocity = velocity;
            _velSet = true;
            _jump = false;
        }

        public bool IsGrounded()
        {
            return _characterController.isGrounded;
        }

        public void Respawn()
        {
            Teleport(_sceneManager.GetSpawnPoint(false), false);
            _eventDispatcher.SendEvent(new ClientSimOnPlayerRespawnEvent { player = _playerApi.Player });
        }

        public void Respawn(int index)
        {
            
            Transform spawnPoint = _sceneManager.GetSpawnPoint(index);
            if (spawnPoint == null)
            {
                this.LogError($"Spawn {index} not found. Spawning at spawn 0");
                spawnPoint = _sceneManager.GetSpawnPoint(0);
            }
            Teleport(spawnPoint, false);
            _eventDispatcher.SendEvent(new ClientSimOnPlayerRespawnEvent { player =  _playerApi.Player });
        }

        public void Teleport(Transform point, bool fromPlaySpace)
        {
            Teleport(point.position, Quaternion.Euler(0, point.rotation.eulerAngles.y, 0), fromPlaySpace);
        }

        public void Teleport(Vector3 position, Quaternion floorRotation, bool fromPlaySpace)
        {
            floorRotation = Quaternion.Euler(0, floorRotation.eulerAngles.y, 0);
            if (fromPlaySpace)
            {
                var playspaceData = _trackingProvider.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
                floorRotation = Quaternion.Inverse(playspaceData.rotation) * floorRotation;
                position = position + floorRotation * -playspaceData.position;
            }

            this.Log($"Moving player to {position.ToString("F3")} and rotation {floorRotation.eulerAngles.ToString("F3")}");

            transform.rotation = floorRotation;
            transform.position = position;

            NotifyPlayerMoved();
            _eventDispatcher.SendEvent(new ClientSimOnPlayerTeleportedEvent { player = _playerApi.Player });
            
            Physics.SyncTransforms();
        }

        #region Stations

        public void EnterStation(IClientSimStation station)
        {
            if (!station.IsMobile())
            {
                _characterController.enabled = false;
                Teleport(station.EnterLocation(), false);
            }
            // VRChatBug: Note that in the else case, the player is teleported to a location that is twice the distance
            // to the station, but since this appears to be a bug, it will not be implemented.
        }

        public void ExitStation(IClientSimStation station, bool skipTeleport)
        {
            _characterController.enabled = true;

            if (!skipTeleport)
            {
                Teleport(station.ExitLocation(), false);
            }
            
            _jump = false;
        }
        
        public void SitPosition(Transform seat)
        {
            transform.SetPositionAndRotation(seat.position, seat.rotation);
            NotifyPlayerMoved();
        }

        #endregion

        private void Update()
        {
            // Handle below respawn height.
            if (transform.position.y < _sceneManager.GetRespawnHeight())
            {
                Respawn();
            }
            
            GetInput();

            NotifyPlayerMoved();
        }
   
        private void FixedUpdate()
        {
            Physics.SyncTransforms();
            Vector2 speed = GetSpeed();
            Vector2 input = _prevInput;

            if (!_stationManager.CanPlayerMove(input.magnitude))
            {
                return;
            }

            if (_menuIsOpen || _isDead)
            {
                input = Vector2.zero;
                _jump = false;
            }

            // Immobile does not affect Jump
            if (_playerLocomotionData.GetImmobilized())
            {
                input = Vector2.zero;
            }

            // Always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = input.y * speed.x * transform.forward + input.x * speed.y * transform.right;
            desiredMove.y = 0;

            float gravityContribution = _playerLocomotionData.GetGravityStrength() * Time.fixedDeltaTime * Physics.gravity.y;

            if (!_velSet)
            {
                if (_characterController.isGrounded)
                {
                    _playerRetainedVelocity = Vector3.zero;
                    _playerRetainedVelocity.y = -STICK_TO_GROUND_FORCE;
                    if (_jump)
                    {
                        if (!_playerLocomotionData.GetUseLegacyLocomotion())
                        {
                            _playerRetainedVelocity = desiredMove;
                        }
                        _playerRetainedVelocity.y = _playerLocomotionData.GetJump();
                        desiredMove = Vector3.zero;
                        _jump = false;
                    }
                }
                else
                {
                    // Slowly add velocity from movement inputs
                    if (!_playerLocomotionData.GetUseLegacyLocomotion())
                    {
                        Vector3 localVelocity = transform.InverseTransformVector(_characterController.velocity);
                        localVelocity.x = Mathf.Clamp(localVelocity.x, -speed.y, speed.y);
                        localVelocity.z = Mathf.Clamp(localVelocity.z, -speed.x, speed.x);

                        Vector3 maxAc = new Vector3(speed.y - localVelocity.x, 0, speed.x - localVelocity.z);
                        Vector3 minAc = new Vector3(-speed.y - localVelocity.x, 0, -speed.x - localVelocity.z);

                        Vector3 inputAcceleration = Time.fixedDeltaTime * RATE_OF_AIR_ACCELERATION * new Vector3(input.x * speed.y, 0, input.y * speed.x);
                        inputAcceleration.x = Mathf.Clamp(inputAcceleration.x, minAc.x, maxAc.x);
                        inputAcceleration.z = Mathf.Clamp(inputAcceleration.z, minAc.z, maxAc.z);

                        inputAcceleration = transform.TransformVector(inputAcceleration);
                        _playerRetainedVelocity += inputAcceleration;
                        desiredMove = Vector3.zero;
                    }
                    // Legacy stutter stepping
                    else if (_directionChanged)
                    {
                        _playerRetainedVelocity = Vector3.zero;
                    }
                    _playerRetainedVelocity.y += gravityContribution;
                }
            }
            else // Dumb behavior that hopefully needs to be removed
            {
                _characterController.Move(new Vector3(desiredMove.x * 0.05f, desiredMove.y * 0.05f + gravityContribution, desiredMove.z * 0.05f) * Time.fixedDeltaTime);
                desiredMove = Vector3.zero;
            }

            desiredMove += _playerRetainedVelocity;

            _characterController.Move(desiredMove * Time.fixedDeltaTime);

            _velSet = false;

            NotifyPlayerMoved();
        }

        #region Input

        private void GetInput()
        {
            // Only allow these input actions while the menu is closed
            if (!_menuIsOpen)
            {
                GetMovementInput();
                RotateView();
            }
        }
        
        private void GetMovementInput()
        {
            Vector2 input = _input.GetMovementAxes();
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }
            
            _directionChanged = (input.sqrMagnitude < 1e-3 ^ _prevInput.sqrMagnitude < 1e-3);
            _prevInput = input;
        }
        
        // TODO Move rotation of the player controller to be done in the tracking provider and have the player controller
        // copy the head rotation. This would allow more generic handling of mouse released, VR snap turning, and locked in station.
        private void RotateView()
        {
            // Allow player controller to look left and right when not in a locked station and for desktop users
            // when the mouse is not released..
            if (!_mouseReleased && !_stationManager.IsLockedInStation())
            {
                float yRot = _input.GetLookHorizontal();
                transform.rotation *= Quaternion.Euler(0f, yRot, 0f);
            }
        }
        
        // Used in tests to help look a specific direction
        public void LookTowardsPoint(Vector3 point)
        {
            if (_stationManager.IsLockedInStation())
            {
                return;
            }

            point.y = transform.position.y;
            transform.LookAt(point);
        }

        #endregion

        // Notify all systems that are dependent on the player's position that the player has moved to a new location.
        private void NotifyPlayerMoved()
        {
            _eventDispatcher.SendEvent(new ClientSimOnPlayerMovedEvent { player = _playerApi.Player });

            if (_cameraProxyObject == null)
            {
                return;
            }
            var cameraTrackingData = _trackingProvider.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            _cameraProxyObject.SetPositionAndRotation(cameraTrackingData.position, cameraTrackingData.rotation);
            _cameraProxyObject.localScale = _trackingProvider.GetTrackingScale() * Vector3.one; 
        }

        private Vector2 GetSpeed()
        {
            // TODO check current bindings to see if non keyboard and only use runspeed.
            Vector2 speed = new Vector2(
                _isWalking? _playerLocomotionData.GetWalkSpeed() : _playerLocomotionData.GetRunSpeed(),
                _playerLocomotionData.GetStrafeSpeed());

            switch (_trackingProvider.GetPlayerStance())
            {
                case ClientSimPlayerStanceEnum.CROUCHING:
                    speed *= CROUCH_SPEED_MULTIPLIER;
                    break;
                case ClientSimPlayerStanceEnum.PRONE:
                    speed *= PRONE_SPEED_MULTIPLIER;
                    break;
            }

            return speed;
        }
    }
}
