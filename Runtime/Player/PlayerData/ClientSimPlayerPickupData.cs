using System;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [Serializable]
    public class ClientSimPlayerPickupData : IClientSimPlayerPickupData
    {
        public bool pickupsEnabled = true;
        private VRC_Pickup _leftHandPickup;
        private VRC_Pickup _rightHandPickup;

        public void SetPickupsEnabled(bool enabled)
        {
            pickupsEnabled = enabled;
        }

        public bool GetPickupsEnabled()
        {
            return pickupsEnabled;
        }

        public VRC_Pickup GetPickupInHand(VRC_Pickup.PickupHand hand)
        {
            switch (hand)
            {
                case VRC_Pickup.PickupHand.Left:
                    return _leftHandPickup;
                case VRC_Pickup.PickupHand.Right:
                    return _rightHandPickup;
                default:
                    return null;
            }
        }

        public void SetPickupInHand(VRC_Pickup.PickupHand hand, VRC_Pickup pickup)
        {
            switch (hand)
            {
                case VRC_Pickup.PickupHand.Left:
                    _leftHandPickup = pickup;
                    break;
                case VRC_Pickup.PickupHand.Right:
                    _rightHandPickup = pickup;
                    break;
            }
        }
    }
}