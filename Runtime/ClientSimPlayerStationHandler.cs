using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    // TODO move more station helper code in here to reduce the locations to search for implementations. 
    // High execution order but still lower than Udon's PostLateUpdate
    [DefaultExecutionOrder(30000)]
    [AddComponentMenu("")]
    public class ClientSimPlayerStationHandler : ClientSimBehaviour
    {
        private ClientSimPlayerController playerController_;
        private ClientSimStationHelper currentStation_;

        protected override void Awake()
        {
            base.Awake();
            playerController_ = GetComponent<ClientSimPlayerController>();
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
            return currentStation_ != null;
        }

        public bool IsLockedInStation()
        {
            return InStation() && !currentStation_.IsMobile;
        }

        public bool CanPlayerMove(float moveValue)
        {
            return !InStation() || currentStation_.CanPlayerMoveWhileSeated(moveValue);
        }

        public void SetUsingStation(ClientSimStationHelper station)
        {
            currentStation_ = station;
        }
        
        public void ExitStation()
        {
            if (!InStation())
            {
                return;
            }
            currentStation_.ExitStation();
        }
        
        private void UpdateStationPosition()
        {
            if (!InStation() || currentStation_.IsMobile)
            {
                return;
            }

            playerController_.SitPosition(currentStation_.EnterLocation);
        }
    }
}