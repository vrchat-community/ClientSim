using System.Collections.Generic;

namespace VRC.SDK3.ClientSim.Interfaces
{
    public interface IClientSimNetworkId
    {
        public void SetNetworkId(int networkId);
        public int GetNetworkId();
        
        public void OwnershipStyle(ClientSimNetworkingUtilities.OwnershipOption option);
    }
}