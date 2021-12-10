using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimSDKManager
    {
        void OnNetworkReady();
        void OnPlayerJoined(VRCPlayerApi player);
        void OnPlayerLeft(VRCPlayerApi player);
        void OnPlayerRespawn(VRCPlayerApi player);
    }
}