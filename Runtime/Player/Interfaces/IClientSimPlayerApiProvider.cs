using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerApiProvider
    {
        VRCPlayerApi Player { get; }
    }
}