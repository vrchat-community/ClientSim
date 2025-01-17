using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimOnPickupEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public HandType handType;
        public IClientSimPickupable pickup;
    }
    
    public class ClientSimOnPickupDropEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public HandType handType;
        public IClientSimPickupable pickup;
    }
    
    public class ClientSimOnPickupUseDownEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public HandType handType;
        public IClientSimPickupable pickup;
    }
    
    public class ClientSimOnPickupUseUpEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public HandType handType;
        public IClientSimPickupable pickup;
    }
}