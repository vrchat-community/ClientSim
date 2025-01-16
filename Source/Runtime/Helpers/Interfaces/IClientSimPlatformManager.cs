using VRC.SDK3.Platform;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlatformManager
    {
        void OnScreenUpdate(ScreenUpdateData data);
    }
}