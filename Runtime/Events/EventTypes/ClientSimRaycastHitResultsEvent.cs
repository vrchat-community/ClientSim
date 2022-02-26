using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimRaycastHitResultsEvent : IClientSimEvent
    {
        public HandType handType;
        public ClientSimRaycastResults raycastResults;
    }
}