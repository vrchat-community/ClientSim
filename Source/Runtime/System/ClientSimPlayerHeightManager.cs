using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimPlayerHeightManager : IClientSimPlayerHeightManager
    {
        // used when scale is adjusted via Udon
        private const float SYSTEM_EYE_HEIGHT_MIN = 0.1f;
        private const float SYSTEM_EYE_HEIGHT_MAX = 100.0f;

        // used when the user adjusts scale manually
        private float userEyeHeightMin = 0.2f;
        private float userEyeHeightMax = 5f;

        // Default avatar height is 1.9 units tall
        private float playerHeight = ClientSimSettings.Instance.playerStartHeight;
        private float playerSpawnHeight = ClientSimSettings.Instance.playerStartHeight;
        private bool manualScalingAllowed = true;
        private IClientSimEventDispatcher eventDispatcher;
        private IClientSimUdonEventSender udonEventSender;

        public ClientSimPlayerHeightManager(IClientSimEventDispatcher eventDispatcher, IClientSimUdonEventSender udonEventSender)
        {
            this.eventDispatcher = eventDispatcher;
            this.udonEventSender = udonEventSender;
            
            eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
        }
        
        ~ClientSimPlayerHeightManager()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            eventDispatcher?.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);

            eventDispatcher = null;
            udonEventSender = null;
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent joinEvent)
        {
            udonEventSender.RunEvent("_onAvatarChanged", ("player", joinEvent.player));
            udonEventSender.RunEvent("_onAvatarEyeHeightChanged",
                ("player", joinEvent.player),
                ("prevEyeHeightAsMeters", 0f));
        }
        
        public bool GetManualAvatarScalingAllowed() => manualScalingAllowed;
        public void SetManualAvatarScalingAllowed(bool value)
        {
            manualScalingAllowed = value;
            eventDispatcher?.SendEvent(new ClientSimOnToggleManualScalingEvent { manualScalingAllowed = value });
        }

        public float GetAvatarEyeHeightMinimumAsMeters() => userEyeHeightMin;
        public void SetAvatarEyeHeightMinimumByMeters(float value)
        {
            // min can't go above max, and can't pass system thresholds
            userEyeHeightMin = Mathf.Clamp(
                Mathf.Min(value, userEyeHeightMax),
                SYSTEM_EYE_HEIGHT_MIN,
                SYSTEM_EYE_HEIGHT_MAX);
            if (playerHeight < userEyeHeightMin)
            {
                SetAvatarEyeHeightByMeters(userEyeHeightMin);
            }
        }
        public float GetAvatarEyeHeightMaximumAsMeters() => userEyeHeightMax;

        public void SetAvatarEyeHeightMaximumByMeters(float value)
        {
            // max can't go below min, and can't pass system thresholds
            userEyeHeightMax = Mathf.Clamp(
                Mathf.Max(value, userEyeHeightMin), 
                SYSTEM_EYE_HEIGHT_MIN,
                SYSTEM_EYE_HEIGHT_MAX);
            if (playerHeight > userEyeHeightMax)
            {
                SetAvatarEyeHeightByMeters(userEyeHeightMax);
            }
        }
        public float GetAvatarEyeHeightAsMeters() => playerHeight;
        public float GetAvatarEyeHeightAsMetersClamped() => Mathf.Clamp(playerHeight, userEyeHeightMin, userEyeHeightMax);
        public void SetAvatarEyeHeightByMultiplier(float multiplier) => SetAvatarEyeHeightByMeters(playerSpawnHeight * multiplier);
        public void SetAvatarEyeHeightByMeters(float newHeight, bool isManual = false)
        {
            float previousHeight = playerHeight;
            playerHeight = isManual 
                ? Mathf.Clamp(newHeight, userEyeHeightMin, userEyeHeightMax) 
                : Mathf.Clamp(newHeight, SYSTEM_EYE_HEIGHT_MIN, SYSTEM_EYE_HEIGHT_MAX);
            
            eventDispatcher?.SendEvent(new ClientSimOnPlayerHeightUpdateEvent
            {
                playerHeight = playerHeight,
                exceedsManualScalingMinimum = !isManual && playerHeight < userEyeHeightMin,
                exceedsManualScalingMaximum = !isManual && playerHeight > userEyeHeightMax
            });
            udonEventSender.RunEvent("_onAvatarEyeHeightChanged",
                ("player", Networking.LocalPlayer),
                ("prevEyeHeightAsMeters", previousHeight));
        }
    }
}