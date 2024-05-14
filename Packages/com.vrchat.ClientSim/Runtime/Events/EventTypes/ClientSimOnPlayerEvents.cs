using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimOnPlayerJoinedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerLeftEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerRespawnEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerTeleportedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerMovedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerEnteredStationEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public IClientSimStation station;
    }
    
    public class ClientSimOnPlayerExitedStationEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public IClientSimStation station;
    }

    public class ClientSimOnPlayerHeightUpdateEvent : IClientSimEvent
    {
        public float playerHeight;
        public bool exceedsManualScalingMaximum;
        public bool exceedsManualScalingMinimum;
    }
    
    public class ClientSimOnTrackingScaleUpdateEvent : IClientSimEvent
    {
        public float trackingScale;
    }

    public class ClientSimOnNewMasterEvent : IClientSimEvent
    {
        public VRCPlayerApi oldMasterPlayer;
        public VRCPlayerApi newMasterPlayer;
    }
    
    public class ClientSimOnToggleManualScalingEvent : IClientSimEvent
    {
        public bool manualScalingAllowed;
    }
}