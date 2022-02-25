using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimStation
    {
        Transform EnterLocation();
        Transform ExitLocation();
        bool IsMobile();
        bool IsSeated();
        bool DisableStationExit();
        bool CanUseStationFromStation();
        bool IsLockedInStation();
        VRCStation GetStation();
        GameObject GetStationGameObject();
        Transform GetStationTransform();

        void EnterStation(VRCPlayerApi player);
        void ExitStation(VRCPlayerApi player);

        bool IsOccupied();
        VRCPlayerApi GetCurrentSittingPlayer();
    }
}