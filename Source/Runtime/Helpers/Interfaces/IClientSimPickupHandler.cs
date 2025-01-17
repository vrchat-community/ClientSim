namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPickupHandler
    {
        void OnPickup();
        void OnDrop();
        void OnPickupUseDown();
        void OnPickupUseUp();
    }
}