
using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for providing tracking data to the rest of ClientSim.
    /// </summary>
    /// <remarks>
    /// Currently only used for DesktopTracking, but can be extended for VR and even fake VR implementations.
    /// Sends Events:
    /// - ClientSimOnPlayerHeightUpdateEvent
    /// - ClientSimOnTrackingScaleUpdateEvent
    /// Listens to Events:
    /// - ClientSimOnPlayerHeightUpdateEvent
    /// </remarks>
    [DefaultExecutionOrder(-3000)] // Update before player raycasting
    public abstract class ClientSimTrackingProviderBase : ClientSimBehaviour, IClientSimTrackingProvider, IDisposable
    {
        private const float MINIMUM_TRACKING_SCALE = 0.1f;
        private const float MAXIMUM_TRACKING_SCALE = 50f;
        
        // TODO calculate this value based on the avatar instead of hard coding it.
        public const float AVATAR_HEIGHT = 1.9f;
        
        protected const float STANDING_HEIGHT = 1.75f;
        protected const float CROUCHING_HEIGHT = 1.0f;
        protected const float PRONE_HEIGHT = 0.5f;
        protected const float SITTING_HEIGHT = 1.2f;
        
        [SerializeField]
        protected Transform head;
        [SerializeField]
        protected Transform leftHand;
        [SerializeField]
        protected Transform rightHand;
        [SerializeField]
        protected Transform playspace;
        [SerializeField]
        protected Transform playerAudioListener;
        [SerializeField]
        protected Camera playerCamera;
        
        protected IClientSimEventDispatcher eventDispatcher;
        protected IClientSimInput input;
        protected ClientSimSettings settings;
        protected IClientSimPlayerHeightManager heightManager;
        
        private float _trackingScale = 1;
        
        public static float CalculateTrackingScaleFromPlayerHeight(float playerHeight)
        {
            return playerHeight / AVATAR_HEIGHT;
        }
        
        public static float CalculatePlayerHeightFromTrackingScale(float trackingScale)
        {
            return trackingScale * AVATAR_HEIGHT;
        }
        
        public virtual void Initialize(
            IClientSimEventDispatcher eventDispatcher,
            IClientSimInput input,
            ClientSimSettings settings,
            IClientSimPlayerHeightManager heightManager)
        {
            this.eventDispatcher = eventDispatcher;
            this.input = input;
            this.settings = settings;
            this.heightManager = heightManager;

            SubscribeEvents();
            
            // Input will be null with incorrect Unity input project settings.
            if (input != null)
            {
                SubscribeInputEvents();
            }
        }
        
        protected virtual void Start()
        {
            // Send event for this to ensure everything that uses the player height is properly updated. 
            eventDispatcher.SendEvent(new ClientSimOnPlayerHeightUpdateEvent
            {
                playerHeight = heightManager.GetAvatarEyeHeightAsMeters()
            });
            
            
            // Only disable audio listeners and cameras if the player is spawned.
            if (settings.spawnPlayer)
            {
                // Destroy other audio listeners
                foreach (var listener in FindObjectsOfType<AudioListener>())
                {
                    if (listener.transform == playerAudioListener)
                    {
                        continue;
                    }
                
                    DestroyImmediate(listener);
                }

                // Disable all cameras that do not render to a render texture.
                foreach (var worldCamera in FindObjectsOfType<Camera>())
                {
                    if (worldCamera == playerCamera)
                    {
                        continue;
                    }

                    if (worldCamera.targetTexture != null)
                    {
                        continue;
                    }

                    worldCamera.enabled = false;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            UnsubscribeEvents();
            
            // Input will be null with incorrect Unity input project settings.
            if (input != null)
            {
                UnsubscribeInputEvents();
            }
        }

        #region IClientSimTrackingProvider

        public virtual VRCPlayerApi.TrackingData GetTrackingData(VRCPlayerApi.TrackingDataType trackingDataType)
        {
            VRCPlayerApi.TrackingData data = new VRCPlayerApi.TrackingData();

            switch (trackingDataType)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    data.position = head.position;
                    data.rotation = head.rotation;
                    break;
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    data.position = leftHand.position;
                    data.rotation = leftHand.rotation;
                    break;
                case VRCPlayerApi.TrackingDataType.RightHand:
                    data.position = rightHand.position;
                    data.rotation = rightHand.rotation;
                    break;
                case VRCPlayerApi.TrackingDataType.Origin:
                    data.position = playspace.position;
                    data.rotation = playspace.rotation;
                    break;
            }

            return data;
        }
        
        public virtual Transform GetTrackingTransform(VRCPlayerApi.TrackingDataType trackingDataType)
        {
            switch (trackingDataType)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    return head;
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    return leftHand;
                case VRCPlayerApi.TrackingDataType.RightHand:
                    return rightHand;
                case VRCPlayerApi.TrackingDataType.Origin:
                    return playspace;
            }

            return null;
        }

        public float GetTrackingScale()
        {
            return _trackingScale;
        }

        public void SetTrackingScale(float scale)
        {
            scale = Mathf.Clamp(scale, MINIMUM_TRACKING_SCALE, MAXIMUM_TRACKING_SCALE);
            
            _trackingScale = scale;
            playspace.localScale = scale * Vector3.one;
            
            // Audio listener must always be scale 1 to ensure ONSP sounds correctly.
            playerAudioListener.localScale = 1.0f / scale * Vector3.one;

            eventDispatcher.SendEvent(new ClientSimOnTrackingScaleUpdateEvent { trackingScale = scale });
        }

        public ClientSimPlayerStanceEnum GetPlayerStance()
        {
            // Check heights starting from shortest
            float headHeight = head.localPosition.y;
            if (headHeight <= PRONE_HEIGHT)
            {
                return ClientSimPlayerStanceEnum.PRONE;
            }
            if (headHeight <= CROUCHING_HEIGHT)
            {
                return ClientSimPlayerStanceEnum.CROUCHING;
            }

            return ClientSimPlayerStanceEnum.STANDING;
        }

        public abstract Transform GetHandRaycastTransform(HandType handType);
        
        public abstract bool IsVR();
        
        public abstract bool SupportsPickupAutoHold();

        public abstract void LookTowardsPoint(Vector3 point);

        #endregion

        #region IClientSimPlayerCameraProvider
        
        public Camera GetCamera()
        {
            return playerCamera;
        }

        public Camera GetCameraForObject(GameObject obj)
        {
            // TODO: Make this interact with camera stacking
            return playerCamera;
        }
        
        #endregion
        
        #region ClientSim Events

        private void OnPlayerHeightUpdate(ClientSimOnPlayerHeightUpdateEvent heightEvent)
        {
            // Convert player height to tracking scale and set the new scale.
            SetTrackingScale(CalculateTrackingScaleFromPlayerHeight(heightEvent.playerHeight));
        }

        #endregion
        
        public virtual void SubscribeInputEvents() { }

        public virtual void UnsubscribeInputEvents() { }

        public virtual void SubscribeEvents()
        {
            eventDispatcher.Subscribe<ClientSimOnPlayerHeightUpdateEvent>(OnPlayerHeightUpdate);
        }

        public virtual void UnsubscribeEvents()
        {
            eventDispatcher.Unsubscribe<ClientSimOnPlayerHeightUpdateEvent>(OnPlayerHeightUpdate);
        }
    }
}