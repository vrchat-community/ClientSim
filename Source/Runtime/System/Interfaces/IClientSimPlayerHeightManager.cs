using System;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerHeightManager : IDisposable
    {
        bool GetManualAvatarScalingAllowed();
        float GetAvatarEyeHeightMinimumAsMeters();
        float GetAvatarEyeHeightMaximumAsMeters();
        float GetAvatarEyeHeightAsMeters();
        float GetAvatarEyeHeightAsMetersClamped();
        void SetManualAvatarScalingAllowed(bool value);
        void SetAvatarEyeHeightMinimumByMeters(float value);
        void SetAvatarEyeHeightMaximumByMeters(float value);
        void SetAvatarEyeHeightByMeters(float newHeight, bool isManual = false);
        void SetAvatarEyeHeightByMultiplier(float multiplier);
    }
}