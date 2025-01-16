using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimStationHandler
    {
        void OnStationEnter(VRCStation station);
        void OnStationExit(VRCStation station);
    }
}
