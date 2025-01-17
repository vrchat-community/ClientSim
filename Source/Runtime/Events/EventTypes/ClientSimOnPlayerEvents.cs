using System.Collections.Generic;
using VRC.SDKBase;

#if VRC_ENABLE_PLAYER_PERSISTENCE
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.ClientSim.Persistence;
#endif

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
    
#if VRC_ENABLE_PLAYER_PERSISTENCE
    public class ClientSimOnPlayerDataDecodedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerObjectsDecodedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerRestoredEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerDataUpdatedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
        public Dictionary<string, ClientSimPlayerDataPair> playerData;
    }
    
    public class ClientSimOnPlayerDataClearedEvent : IClientSimEvent
    {
        public VRCPlayerApi player;
    }
    
    public class ClientSimOnPlayerObjectUpdatedEvent : IClientSimEvent
    {
        public IClientSimNetworkSerializer Data;
    }
    
    public class ClientSimOnPlayerObjectUpdateEndedEvent : IClientSimEvent
    {
    }
#endif

    public class ClientSimOnToggleManualScalingEvent : IClientSimEvent
    {
        public bool manualScalingAllowed;
    }

    public class ClientSimOnVRCPlusMassGift : IClientSimEvent
    {
        public VRCPlayerApi gifter;
        public int numGifts;
    }
}