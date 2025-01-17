using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerPickupData
    {
        void SetPickupsEnabled(bool enabled);
        bool GetPickupsEnabled();
        VRC_Pickup GetPickupInHand(VRC_Pickup.PickupHand hand);
        void SetPickupInHand(VRC_Pickup.PickupHand hand, VRC_Pickup pickup);
    }
}