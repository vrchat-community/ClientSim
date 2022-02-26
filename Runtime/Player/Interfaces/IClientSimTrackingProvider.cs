using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimTrackingProvider : IClientSimPlayerCameraProvider
    {
        VRCPlayerApi.TrackingData GetTrackingData(VRCPlayerApi.TrackingDataType trackingDataType);
        Transform GetTrackingTransform(VRCPlayerApi.TrackingDataType trackingDataType);
        float GetTrackingScale();
        void SetTrackingScale(float scale);
        ClientSimPlayerStanceEnum GetPlayerStance();
        Transform GetHandRaycastTransform(HandType handType);
        bool IsVR();
        bool SupportsPickupAutoHold();
    }
}