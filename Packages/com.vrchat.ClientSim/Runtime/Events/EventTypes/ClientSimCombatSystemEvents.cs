using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimPlayerDeathStatusChangedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public bool isDead;
    }
}