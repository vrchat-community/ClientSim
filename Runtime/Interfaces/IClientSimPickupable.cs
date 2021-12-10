namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPickupable
    {
        void OnPickup();
        void OnDrop();
        void OnPickupUseDown();
        void OnPickupUseUp();
    }
}