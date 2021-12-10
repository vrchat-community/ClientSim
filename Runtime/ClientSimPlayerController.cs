using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using System.Reflection;

// Noodle bowl defines its own button class, which would break ClientSim.
// Ensuring button is properly defined with this using statement.
using UIButton = UnityEngine.UI.Button; 


#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimPlayerController : ClientSimBehaviour
    {
        public static ClientSimPlayerController instance;

        private enum Stance {
            STANDING,
            CROUCHING,
            PRONE,
            SITTING
        }

        public const float DEFAULT_RUN_SPEED_ = 4;
        public const float DEFAULT_WALK_SPEED_ = 2;

        private const float CROUCH_SPEED_MULTIPLYER_ = 0.35f;
        private const float PRONE_SPEED_MULTIPLYER_ = 0.15f;

        private const float STANDING_HEIGHT_ = 1.6f;
        private const float CROUCHING_HEIGHT_ = 1.0f;
        private const float PRONE_HEIGHT_ = 0.5f;
        private const float SITTING_HEIGHT_ = 0.88f;

        private const float STICK_TO_GROUND_FORCE_ = 2f;
        private const float RATE_OF_AIR_ACCELERATION_ = 5f;

        // TODO, make based on avatar armspan/settings
        private const float AVATAR_SCALE_ = 1.13f;

        private readonly KeyCode MenuKey = KeyCode.Escape;

        
        private GameObject playspace_;
        private GameObject playerCamera_;
        private GameObject rightArmPosition_;
        private GameObject leftArmPosition_;
        private Rigidbody rightArmRigidbody_;
        private Rigidbody leftArmRigidbody_;
        private GameObject menu_;
        private Transform cameraProxyObject_;
        private Rigidbody rigidbody_;
        private VRC_SceneDescriptor descriptor_;
        private CharacterController characterController_;
        private Camera camera_;
        private MouseLook mouseLook_;
        private ClientSimPlayerStationHandler stationHandler_;
        private ClientSimPickupHelper currentPickup_;
        private ClientSimBaseInput baseInput_;
        private ClientSimInteractManager interactManager_;
        private ClientSimPlayer player_;

        private Stance stance_;
        private bool isDead_;
        private bool isImmobile_;
        private bool isWalking_;

        private float walkSpeed_ = DEFAULT_WALK_SPEED_;
        private float strafeSpeed_ = DEFAULT_WALK_SPEED_;
        private float runSpeed_ = DEFAULT_RUN_SPEED_;
        private float jumpSpeed_;
        private bool jump_;
        private float gravityStrength_ = 1f;
        
        private Vector2 prevInput_;
        // Check if the directionality changed to apply "stutter stepping" for legacy locomotion.
        private bool directionChanged_;

        private bool velSet;
        private Vector3 playerRetainedVelocity_;

        private CollisionFlags collisionFlags_;
        private bool peviouslyGrounded_;
        private bool legacyLocomotion_;

        private Texture2D reticleTexture_;

        // Used for determining pickup throw
        private Vector3 prevousHandPosition_;
        private Vector3 prevousHandRotation_;
        
        private bool pickupsEnabled_ = true;
        
        // The last frame the menu was opened or closed to prevent input events from firing at the same time.
        private int lastMenuUpdateFrame_;

        public static Camera GetPlayerCamera()
        {
            if (instance != null)
            {
                return instance.GetCamera();
            }
            return Camera.main;
        }

        protected override void Awake()
        {
            base.Awake();

            if (instance != null)
            {
                this.LogError("Player controller instance already exists!");
                DestroyImmediate(this);
                return;
            }

            instance = this;
            descriptor_ = FindObjectOfType<VRC_SceneDescriptor>();
            gameObject.layer = LayerMask.NameToLayer("PlayerLocal");
            gameObject.tag = "Player";

            // Ensure that the player is in the correct location at the end of frame when in a station.
            stationHandler_ = gameObject.AddComponent<ClientSimPlayerStationHandler>();

            rigidbody_ = gameObject.AddComponent<Rigidbody>();
            rigidbody_.isKinematic = true;

            characterController_ = gameObject.AddComponent<CharacterController>();
            characterController_.slopeLimit = 50;
            characterController_.stepOffset = .5f;
            characterController_.skinWidth = 0.005f;
            characterController_.minMoveDistance = 0;
            characterController_.center = new Vector3(0, 0.8f, 0);
            characterController_.radius = 0.2f;
            characterController_.height = 1.6f;


            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.localScale = new Vector3(0.4f, 1, 0.4f);
            capsule.transform.SetParent(transform, false);
            capsule.transform.localPosition = new Vector3(0, 1, 0);
            capsule.layer = LayerMask.NameToLayer("MirrorReflection");
            DestroyImmediate(capsule.GetComponent<Collider>());


            playerCamera_ = new GameObject("Player Camera");
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(playerCamera_.transform, false);
            camera_ = cameraHolder.AddComponent<Camera>();
            camera_.cullingMask &= ~(1 << 18); // remove mirror reflection

            // TODO, make based on avatar armspan/settings
            cameraHolder.transform.localScale = Vector3.one * AVATAR_SCALE_;

            foreach (var listener in FindObjectsOfType<AudioListener>())
            {
                DestroyImmediate(listener);
            }
            
            playerCamera_.AddComponent<AudioListener>();
            playerCamera_.transform.SetParent(transform, false);
            playerCamera_.transform.localPosition = new Vector3(0, STANDING_HEIGHT_, .1f);
            playerCamera_.transform.localRotation = Quaternion.identity;

            playspace_ = new GameObject("Playspace Center");
            playspace_.transform.SetParent(transform, false);

            rightArmPosition_ = new GameObject("Right Arm Position");
            rightArmPosition_.transform.SetParent(playerCamera_.transform, false);
            rightArmPosition_.transform.localPosition = new Vector3(0.15f, -0.13f, 0.4f);
            rightArmPosition_.transform.localRotation = Quaternion.Euler(-35, 0, -90);
            rightArmRigidbody_ = rightArmPosition_.AddComponent<Rigidbody>();
            rightArmRigidbody_.isKinematic = true;
            
            leftArmPosition_ = new GameObject("Left Arm Position");
            leftArmPosition_.transform.SetParent(playerCamera_.transform, false);
            leftArmPosition_.transform.localPosition = new Vector3(-0.15f, -0.13f, 0.4f);
            leftArmPosition_.transform.localRotation = Quaternion.Euler(-35, 0, -90);
            leftArmRigidbody_ = leftArmPosition_.AddComponent<Rigidbody>();
            leftArmRigidbody_.isKinematic = true;
            
            mouseLook_ = new MouseLook();
            mouseLook_.Init(transform, playerCamera_.transform);

            SetStance(Stance.STANDING);

            baseInput_ = transform.parent.gameObject.GetComponent<ClientSimBaseInput>();
            CreateMenu();

            // TODO refactor this out of the player controller
            GameObject interactManager = new GameObject("InteractManager");
            interactManager.transform.SetParent(transform.parent, false);
            interactManager_ = interactManager.AddComponent<ClientSimInteractManager>();
            interactManager_.Initialize(this, playerCamera_.transform);

            reticleTexture_ = Resources.Load<Texture2D>("Images/Reticle");
            
            cameraProxyObject_ = new GameObject("CameraDamageProxy").transform;
            cameraProxyObject_.SetParent(ClientSimMain.GetProxyObjectTransform(), false);
            UpdateCameraProxyPosition();
        }

        private void Start()
        {
            Camera refCamera = null;
            if (descriptor_.ReferenceCamera != null)
            {
                refCamera = descriptor_.ReferenceCamera.GetComponent<Camera>();
            }

            if (refCamera != null)
            {
                CopyCameraValues(refCamera, camera_);
            }
            else
            {
                this.LogWarning("Reference camera is not set in the SceneDescriptor!");
            }

            // Go through all ui shapes to update canvas cameras?
            foreach (var ui in FindObjectsOfType<VRC_UiShape>())
            {
                Canvas canvas = ui.GetComponent<Canvas>();
                if (canvas == null) continue;
                canvas.worldCamera = camera_;
            }
        }

        public static void CopyCameraValues(Camera refCamera, Camera camera)
        {
            if (refCamera == null)
            {
                return;
            }
            
            camera.farClipPlane = refCamera.farClipPlane;
            camera.nearClipPlane = Mathf.Clamp(refCamera.nearClipPlane, 0.01f, 0.05f);
            camera.clearFlags = refCamera.clearFlags;
            camera.backgroundColor = refCamera.backgroundColor;
            camera.allowHDR = refCamera.allowHDR;

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessLayer refPostProcessLayer = refCamera.GetComponent<PostProcessLayer>();
            if (refPostProcessLayer != null)
            {
#if UNITY_ANDROID
                instance.LogWarning("Post processing is not supported on Android");
#else
                // VRChatBug: VRChat ignores if the PostProcessingLayer is disabled on the reference camera and always
                // enables it for the player's camera.
                PostProcessLayer postProcessLayer = camera.gameObject.AddComponent<PostProcessLayer>();
                postProcessLayer.volumeLayer = refPostProcessLayer.volumeLayer;

                postProcessLayer.volumeTrigger = refPostProcessLayer.volumeTrigger != refPostProcessLayer.transform
                    ? refPostProcessLayer.volumeTrigger
                    : camera.transform;

                // Use reflection to copy over resources : https://github.com/Unity-Technologies/PostProcessing/issues/467
                FieldInfo resourcesInfo = typeof(PostProcessLayer).GetField("m_Resources", BindingFlags.NonPublic | BindingFlags.Instance);
                PostProcessResources postProcessResources = resourcesInfo.GetValue(refPostProcessLayer) as PostProcessResources;
                postProcessLayer.Init(postProcessResources);
#endif
            }
#endif
            refCamera.gameObject.SetActive(false);
            
        }

        // TODO Refactor this out so that menu is a prefab spawned in and not created in code.
        private void CreateMenu()
        {
            const int menuLayer = 12;
            Font font = Font.CreateDynamicFontFromOSFont("Ariel", 20);

            // Create Menu
            menu_ = new GameObject("Menu");
            menu_.layer = menuLayer;
            menu_.transform.parent = transform.parent;
            Canvas canvas = menu_.AddComponent<Canvas>();
            // TODO make canvas world space and updated to in front of the player when menu is opened.
            // Menu will need to render over everything and accept input even when an object is in front.
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = camera_;
            canvas.planeDistance = camera_.nearClipPlane + 0.1f;
            canvas.sortingOrder = 1000;
            menu_.AddComponent<GraphicRaycaster>();

            GameObject respawnButton = new GameObject("RespawnButton");
            respawnButton.layer = menuLayer;
            respawnButton.transform.SetParent(menu_.transform, false);
            respawnButton.transform.localPosition = new Vector3(60, 0, 0);
            respawnButton.AddComponent<Image>();
            UIButton button = respawnButton.AddComponent<UIButton>();
            button.onClick.AddListener(RespawnFromMenu);

            GameObject respawnText = new GameObject("RespawnText");
            respawnText.layer = menuLayer;
            respawnText.transform.SetParent(respawnButton.transform, false);
            Text text = respawnText.AddComponent<Text>();
            respawnText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            text.text = "Respawn";
            text.fontSize = 20;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.font = font;


            GameObject exitMenuButton = new GameObject("ExitMenuButton");
            exitMenuButton.layer = menuLayer;
            exitMenuButton.transform.SetParent(menu_.transform, false);
            exitMenuButton.transform.localPosition = new Vector3(-60, 0, 0);
            exitMenuButton.AddComponent<Image>();
            button = exitMenuButton.AddComponent<UIButton>();
            button.onClick.AddListener(CloseMenu);

            GameObject exitMenuText = new GameObject("ExitMenuText");
            exitMenuText.layer = menuLayer;
            exitMenuText.transform.SetParent(exitMenuButton.transform, false);
            text = exitMenuText.AddComponent<Text>();
            exitMenuText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            text.text = "Close\nMenu";
            text.fontSize = 20;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.font = font;
            
#if UNITY_EDITOR           
            GameObject settingsButton = new GameObject("SettingsButton");
            settingsButton.layer = menuLayer;
            settingsButton.transform.SetParent(menu_.transform, false);
            settingsButton.transform.localPosition = new Vector3(-60, -100, 0);
            settingsButton.AddComponent<Image>();
            button = settingsButton.AddComponent<UIButton>();
            
            // TODO handle this better
            Type settingsWindow = Type.GetType("VRC.SDK3.ClientSim.Editor.ClientSimSettingsWindow, VRC.ClientSim.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            button.onClick.AddListener(() => UnityEditor.EditorWindow.GetWindow(settingsWindow, false, "ClientSim Settings"));
            
            RectTransform rect = settingsButton.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 50);

            GameObject settingsText = new GameObject("SettingsText");
            settingsText.layer = menuLayer;
            settingsText.transform.SetParent(settingsButton.transform, false);
            text = settingsText.AddComponent<Text>();
            settingsText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            text.text = "Settings";
            text.fontSize = 20;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.font = font;
#endif
            

            ToggleMenu(true);
        }


        public float GetJump()
        {
            return jumpSpeed_;
        }

        public void SetJump(float jump)
        {
            jumpSpeed_ = jump;
        }

        public float GetRunSpeed()
        {
            return runSpeed_;
        }

        public void SetRunSpeed(float runSpeed)
        {
            runSpeed_ = runSpeed;
        }

        public float GetWalkSpeed()
        {
            return walkSpeed_;
        }

        public void SetWalkSpeed(float walkSpeed)
        {
            walkSpeed_ = walkSpeed;
        }

        public float GetStrafeSpeed()
        {
            return strafeSpeed_;
        }

        public void SetStrafeSpeed(float strafeSpeed)
        {
            strafeSpeed_ = strafeSpeed;
        }
        
        public float GetGravityStrength()
        {
            return gravityStrength_;
        }

        public void SetGravityStrength(float gravity)
        {
            gravityStrength_ = gravity;
        }

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
            // TODO fix value
            return characterController_.velocity;
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            playerRetainedVelocity_ = velocity;
            velSet = true;
            jump_ = false;
        }

        public bool IsGrounded()
        {
            return characterController_.isGrounded;
        }

        public void UseLegacyLocomotion()
        {
            legacyLocomotion_ = true;
        }

        public VRCPlayerApi.TrackingData GetTrackingData(VRCPlayerApi.TrackingDataType trackingDataType)
        {
            VRCPlayerApi.TrackingData data = new VRCPlayerApi.TrackingData();

            if (trackingDataType == VRCPlayerApi.TrackingDataType.Head)
            {
                data.position = playerCamera_.transform.position;
                data.rotation = playerCamera_.transform.rotation;
            }
            else if (trackingDataType == VRCPlayerApi.TrackingDataType.LeftHand)
            {
                data.position = leftArmPosition_.transform.position;
                data.rotation = leftArmPosition_.transform.rotation;
            }
            else if (trackingDataType == VRCPlayerApi.TrackingDataType.RightHand)
            {
                data.position = rightArmPosition_.transform.position;
                data.rotation = rightArmPosition_.transform.rotation;
            }
            else if (trackingDataType == VRCPlayerApi.TrackingDataType.Origin)
            {
                data.position = playspace_.transform.position;
                data.rotation = playspace_.transform.rotation;
            }

            return data;
        }

        public void SetPlayer(ClientSimPlayer player)
        {
            player_ = player;
        }

        private void RespawnFromMenu()
        {
            CloseMenu();
            Respawn();
        }

        public void Respawn()
        {
            Teleport(ClientSimMain.GetNextSpawnPoint(), false);
            ClientSimMain.PlayerRespawned(player_.player);
        }

        public void Teleport(Transform point, bool fromPlaySpace)
        {
            Teleport(point.position, Quaternion.Euler(0, point.rotation.eulerAngles.y, 0), fromPlaySpace);
        }

        public void Teleport(Vector3 position, Quaternion floorRotation, bool fromPlaySpace)
        {
            // Udon auto exits players from stations when they are teleported. 
            stationHandler_.ExitStation();
            
            floorRotation = Quaternion.Euler(0, floorRotation.eulerAngles.y, 0);
            if (fromPlaySpace)
            {
                floorRotation = Quaternion.Inverse(playspace_.transform.localRotation) * floorRotation;
                position = position + floorRotation * -playspace_.transform.localPosition;
            }

            this.Log("Moving player to " + position.ToString("F3") + " and rotation " + floorRotation.eulerAngles.ToString("F3"));

            transform.rotation = floorRotation;
            transform.position = position;
            mouseLook_.SetRotation(floorRotation);
            UpdateCameraProxyPosition();
            Physics.SyncTransforms();
        }

        #region Stations

        public void EnterStation(ClientSimStationHelper station)
        {
            stationHandler_.ExitStation();

            if (!station.IsMobile)
            {
                characterController_.enabled = false;
                Teleport(station.EnterLocation, false);
                mouseLook_.SetBaseRotation(station.EnterLocation);
                mouseLook_.SetRotation(Quaternion.identity);
            }
            // VRChatBug: Note that in the else case, the player is teleported to a location that is twice the distance
            // to the station, but since this appears to be a bug, it will not be implemented.
            
            stationHandler_.SetUsingStation(station);
            if (station.IsSeated)
            {
                SetStance(Stance.SITTING);
            }
        }

        public void ExitStation(ClientSimStationHelper station)
        {
            stationHandler_.SetUsingStation(null);
            characterController_.enabled = true;

            Teleport(station.ExitLocation, false);
            
            if (station.IsSeated)
            {
                SetStance(Stance.STANDING);
            }
            
            mouseLook_.SetBaseRotation(null);
            jump_ = false;
        }
        
        public void SitPosition(Transform seat)
        {
            transform.position = seat.position;
            mouseLook_.LookRotation(transform, playerCamera_.transform, stationHandler_.IsLockedInStation());
            UpdateCameraProxyPosition();
        }

        public ClientSimPlayerStationHandler GetStationHandler()
        {
            return stationHandler_;
        }

        #endregion
        
        #region Pickups

        public void PickupObject(ClientSimPickupHelper pickup)
        {
            if (currentPickup_ != null)
            {
                currentPickup_.Drop();
            }
            currentPickup_ = pickup;
            
            pickup.UpdatePosition(rightArmPosition_.transform, true);
            FixedJoint fixedJoint = rightArmPosition_.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = pickup.GetRigidbody();
        }

        public void DropObject(ClientSimPickupHelper pickup)
        {
            if (currentPickup_ == pickup)
            {
                currentPickup_ = null;
                FixedJoint fixedJoint = rightArmPosition_.GetComponent<FixedJoint>();
                if (fixedJoint)
                {
                    Destroy(fixedJoint);
                }

                Rigidbody rigidbody = pickup.GetRigidbody();
                rigidbody.velocity = (rightArmPosition_.transform.position - prevousHandPosition_) * (0.5f / Time.deltaTime);
                rigidbody.angularVelocity = (rightArmPosition_.transform.rotation.eulerAngles - prevousHandRotation_);
            }
        }
        
        public void EnablePickups(bool pickupsEnabled)
        {
            pickupsEnabled_ = pickupsEnabled;
        }

        public bool GetPickupsEnabled()
        {
            return pickupsEnabled_;
        }

        public bool IsHoldingPickup(VRC_Pickup.PickupHand hand)
        {
            if (hand == VRC_Pickup.PickupHand.Right)
            {
                return currentPickup_ != null;
            }
            return false;
        }
        
        public VRC_Pickup GetHeldPickup(VRC_Pickup.PickupHand hand)
        {
            if (hand == VRC_Pickup.PickupHand.Right && currentPickup_ != null)
            {
                return currentPickup_.GetPickup();
            }
            return null;
        }

        #endregion
        
        public Camera GetCamera()
        {
            return camera_;
        }

        public float GetCameraScale()
        {
            return camera_.transform.lossyScale.x;
        }

        public Transform GetCameraProxyTransform()
        {
            return cameraProxyObject_;
        }

        public Transform GetArmTransform()
        {
            return rightArmPosition_.transform;
        }

        public void PlayerDied()
        {
            if (currentPickup_ != null)
            {
                currentPickup_.Drop();
            }
            stationHandler_.ExitStation();
            
            isDead_ = true;
        }

        public void PlayerRevived()
        {
            isDead_ = false;
        }

        public void Immobilize(bool immobilize)
        {
            isImmobile_ = immobilize;
        }

        private void Update()
        {
            GetInput();
            
            // TODO move to a pickup handler
            if (currentPickup_ != null)
            {
                currentPickup_.UpdatePosition(rightArmPosition_.transform);
            }
            
            UpdateCameraProxyPosition();
            
            prevousHandPosition_ = rightArmPosition_.transform.position;
            prevousHandRotation_ = rightArmPosition_.transform.rotation.eulerAngles;
        }
   
        private void FixedUpdate()
        {
            Physics.SyncTransforms();
            Vector2 speed = GetSpeed();
            Vector2 input = prevInput_;

            if (!stationHandler_.CanPlayerMove(input.magnitude))
            {
                return;
            }

            if (menu_.activeInHierarchy || isDead_)
            {
                input = Vector2.zero;
                jump_ = false;
            }

            // Immobile does not affect Jump
            if (isImmobile_)
            {
                input = Vector2.zero;
            }

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * input.y * speed.x + transform.right * input.x * speed.y;
            desiredMove.y = 0;

            float gravityContribution = (gravityStrength_ * Physics.gravity * Time.fixedDeltaTime).y;

            if (!velSet)
            {
                if (characterController_.isGrounded)
                {
                    playerRetainedVelocity_ = Vector3.zero;
                    playerRetainedVelocity_.y = -STICK_TO_GROUND_FORCE_;
                    if (jump_)
                    {
                        if (!legacyLocomotion_)
                        {
                            playerRetainedVelocity_ = desiredMove;
                        }
                        playerRetainedVelocity_.y = jumpSpeed_;
                        desiredMove = Vector3.zero;
                        jump_ = false;
                    }
                }
                else
                {
                    // Slowly add velocity from movement inputs
                    if (!legacyLocomotion_)
                    {
                        Vector3 localVelocity = transform.InverseTransformVector(characterController_.velocity);
                        localVelocity.x = Mathf.Clamp(localVelocity.x, -speed.y, speed.y);
                        localVelocity.z = Mathf.Clamp(localVelocity.z, -speed.x, speed.x);

                        Vector3 maxAc = new Vector3(speed.y - localVelocity.x, 0, speed.x - localVelocity.z);
                        Vector3 minAc = new Vector3(-speed.y - localVelocity.x, 0, -speed.x - localVelocity.z);

                        Vector3 inputAcceleration = new Vector3(input.x * speed.y, 0, input.y * speed.x) * Time.fixedDeltaTime * RATE_OF_AIR_ACCELERATION_;
                        inputAcceleration.x = Mathf.Clamp(inputAcceleration.x, minAc.x, maxAc.x);
                        inputAcceleration.z = Mathf.Clamp(inputAcceleration.z, minAc.z, maxAc.z);

                        inputAcceleration = transform.TransformVector(inputAcceleration);
                        playerRetainedVelocity_ += inputAcceleration;
                        desiredMove = Vector3.zero;
                    }
                    // Legacy stutter stepping
                    else if (directionChanged_)
                    {
                        playerRetainedVelocity_ = Vector3.zero;
                    }
                    playerRetainedVelocity_.y += gravityContribution;
                }
            }
            else // Dumb behavior that hopefully needs to be removed
            {
                characterController_.Move(new Vector3(desiredMove.x * 0.05f, desiredMove.y * 0.05f + gravityContribution, desiredMove.z * 0.05f) * Time.fixedDeltaTime);
                desiredMove = Vector3.zero;
            }

            desiredMove += playerRetainedVelocity_;

            peviouslyGrounded_ = characterController_.isGrounded;
            collisionFlags_ = characterController_.Move(desiredMove * Time.fixedDeltaTime);

            velSet = false;

            UpdateCameraProxyPosition();
        }

        #region Input

        private void GetInput()
        {
            // Only allow these input actions while the menu is closed
            if (!menu_.activeInHierarchy)
            {
                GetMovementInput();
                RotateView();
                CheckForJump();
                UpdateStance();
            }
            
            // VRChatBug: Pickup use and drop are not affected by the menu being open.
            if (currentPickup_ != null)
            {
                currentPickup_.UpdateUse();
            }
            
            UpdateMenu();
            
            HandleUdonInput();
        }
        
        private void GetMovementInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 input = new Vector2(horizontal, vertical);
            
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }
            
            directionChanged_ = (input.sqrMagnitude < 1e-3 ^ prevInput_.sqrMagnitude < 1e-3);
            prevInput_ = input;
        }
        
        private void RotateView()
        {
            mouseLook_.GetRotationInput();
            mouseLook_.LookRotation(transform, playerCamera_.transform, stationHandler_.IsLockedInStation());
        }

        private void CheckForJump()
        {
            if (!jump_ && characterController_.isGrounded && jumpSpeed_ > 0)
            {
                jump_ = Input.GetButtonDown("Jump");
            }
        }

        // TODO Refactor all input out of the player controller and simply have it listen to these events
        private readonly (KeyCode, string, HandType)[] keyToEvent_ =
        {
            (KeyCode.Space, UdonManager.UDON_INPUT_JUMP, HandType.LEFT),
            (KeyCode.Mouse0, UdonManager.UDON_INPUT_USE, HandType.RIGHT),
            (KeyCode.Mouse0, UdonManager.UDON_INPUT_GRAB, HandType.RIGHT),
            (KeyCode.Mouse1, UdonManager.UDON_INPUT_DROP, HandType.RIGHT),
        };

        private Vector2 prevMouseInput_ = Vector2.zero;
        private Vector2 prevMoveInput_ = Vector2.zero;
        private void HandleUdonInput()
        {
            // If the menu is open or the menu was updated on this frame, skip sending input events. 
            if (menu_.activeInHierarchy || lastMenuUpdateFrame_ == Time.frameCount)
            {
                return;
            }
            
            foreach (var (keyCode, eventName, handType) in keyToEvent_)
            {
                HandleInputForKey(keyCode, eventName, handType);
            }

            Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            // Clamp value from -1 to 1
            mouseInput = Vector2.Max(Vector2.Min(mouseInput, Vector2.one), -Vector2.one);

            if (baseInput_.isMenuOpen)
            {
                mouseInput = Vector2.zero;
            }

            if (Mathf.Abs(mouseInput.x - prevMouseInput_.x) > 0.001f)
            {
                var args = new UdonInputEventArgs(mouseInput.x, HandType.RIGHT);
                UdonManager.Instance.RunInputAction(UdonManager.UDON_LOOK_HORIZONTAL, args);
            }
            if (Mathf.Abs(mouseInput.y - prevMouseInput_.y) > 0.001f)
            {
                var args = new UdonInputEventArgs(mouseInput.y, HandType.RIGHT);
                UdonManager.Instance.RunInputAction(UdonManager.UDON_LOOK_VERTICAL, args);
            }
            prevMouseInput_ = mouseInput;
            
            // TODO refactor all this out into its own input manager that the player controller listens to.
            // Handle sending movement input to Udon.
            if (Mathf.Abs(prevMoveInput_.x - prevInput_.x) > 1e-3)
            {
                var args = new UdonInputEventArgs(prevInput_.x, HandType.RIGHT);
                UdonManager.Instance.RunInputAction(UdonManager.UDON_MOVE_HORIZONTAL, args);
            }
            if (Mathf.Abs(prevMoveInput_.y - prevInput_.y) > 1e-3)
            {
                var args = new UdonInputEventArgs(prevInput_.y, HandType.RIGHT);
                UdonManager.Instance.RunInputAction(UdonManager.UDON_MOVE_VERTICAL, args);
            }

            prevMoveInput_ = prevInput_;
        }
        
        private void HandleInputForKey(KeyCode key, string eventName, HandType handType)
        {
            if (Input.GetKeyDown(key))
            {
                var args = new UdonInputEventArgs(true, handType);
                UdonManager.Instance.RunInputAction(eventName, args);
            }
            if (Input.GetKeyUp(key))
            {
                var args = new UdonInputEventArgs(false, handType);
                UdonManager.Instance.RunInputAction(eventName, args);
            }
        }
        
        #endregion

        private void UpdateCameraProxyPosition()
        {
            cameraProxyObject_.position = playerCamera_.transform.position;
            cameraProxyObject_.rotation = playerCamera_.transform.rotation;
            cameraProxyObject_.localScale = playerCamera_.transform.lossyScale;
        }

        private void UpdateMenu()
        {
            // If the user presses the escape key, open the menu.
            // Note that you cannot close the menu this way as Unity uses the escape key as a special key to unfocus the
            // game window. It is not possible, without hacky code, to capture the mouse again.
            if (Input.GetKeyDown(MenuKey) && !menu_.activeInHierarchy)
            {
                ToggleMenu(true);
            }
        }

        private void CloseMenu()
        {
            ToggleMenu(false);
        }

        private void ToggleMenu(bool isOpen)
        {
            menu_.SetActive(isOpen);
            baseInput_.SetMenuOpen(isOpen);
            lastMenuUpdateFrame_ = Time.frameCount;
        }

        private void UpdateStance()
        {
            // TODO move to input events
            if (Input.GetKeyDown(ClientSimSettings.Instance.crouchKey))
            {
                if (stance_ == Stance.CROUCHING)
                {
                    SetStance(Stance.STANDING);
                }
                else
                {
                    SetStance(Stance.CROUCHING);
                }
            }
            if (Input.GetKeyDown(ClientSimSettings.Instance.proneKey))
            {
                if (stance_ == Stance.PRONE)
                {
                    SetStance(Stance.STANDING);
                }
                else
                {
                    SetStance(Stance.PRONE);
                }
            }
        }

        private void SetStance(Stance stance)
        {
            if (stationHandler_.IsLockedInStation() && stance != Stance.SITTING)
            {
                return;
            }
            
            stance_ = stance;
            Vector3 cameraPosition = playerCamera_.transform.localPosition;

            switch (stance_)
            {
                case Stance.PRONE:
                    cameraPosition.y = PRONE_HEIGHT_;
                    break;
                case Stance.CROUCHING:
                    cameraPosition.y = CROUCHING_HEIGHT_;
                    break;
                case Stance.SITTING:
                    cameraPosition.y = SITTING_HEIGHT_;
                    break;
                case Stance.STANDING:
                    cameraPosition.y = STANDING_HEIGHT_;
                    break;
            }
            
            playerCamera_.transform.localPosition = cameraPosition;
        }

        private Vector2 GetSpeed()
        {
            isWalking_ = !Input.GetKey(ClientSimSettings.Instance.runKey);
            Vector2 speed = new Vector2(isWalking_? walkSpeed_ : runSpeed_, strafeSpeed_);
            
            if (stance_ == Stance.CROUCHING)
            {
                speed *= CROUCH_SPEED_MULTIPLYER_;
            }
            else if (stance_ == Stance.PRONE)
            {
                speed *= PRONE_SPEED_MULTIPLYER_;
            }

            return speed;
        }
        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // TODO fake implementing OnPlayerColliderEnter/Stay/Exit for Udon with this method.

            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (collisionFlags_ == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(characterController_.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        private void OnGUI()
        {
            // TODO if VR also return
            if (!ClientSimSettings.Instance.showDesktopReticle)
            {
                return;
            }
            Vector2 center = ClientSimBaseInput.GetScreenCenter();
            Vector2 size = new Vector2(reticleTexture_.width, reticleTexture_.height);
            Rect position = new Rect(center - size * 0.5f, size);
            GUI.DrawTexture(position, reticleTexture_);
        }
    }

    // Modified from standard assets
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public float MinimumXAngle = -80f;
        public float MaximumXAngle = 70f;
        public float MinimumYAngle = -90F;
        public float MaximumYAngle = 90F;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private Transform m_BaseRotation;

        public void Init(Transform character, Transform camera)
        {
            SetBaseRotation(null);
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        public void SetRotation(Quaternion newRotation)
        {
            m_CharacterTargetRot = newRotation;
        }

        public void SetBaseRotation(Transform baseRotation)
        {
            m_BaseRotation = baseRotation;
        }

        public void GetRotationInput()
        {
            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        }
        
        public void LookRotation(Transform character, Transform camera, bool inStation)
        {
            m_CameraTargetRot = ClampRotationAroundAxis(m_CameraTargetRot, 0, MinimumXAngle, MaximumXAngle);

            if (inStation)
            {
                m_CharacterTargetRot = ClampRotationAroundAxis(m_CharacterTargetRot, 1, MinimumYAngle, MaximumYAngle);
                character.localRotation = m_BaseRotation.rotation * m_CharacterTargetRot;
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
            }
            
            camera.localRotation = m_CameraTargetRot;
        }

        Quaternion ClampRotationAroundAxis(Quaternion q, int index, float minAngle, float maxAngle)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q[index]);

            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            q[index] = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);

            return q;
        }
    }
}
