using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests
{
    [AddComponentMenu("")]
    public class ClientSimTestStationHandler : ClientSimBehaviour, IClientSimStationHandler
    {
        public Action<VRCStation> onEnterStation;
        public Action<VRCStation> onExitStation;

        public void OnStationEnter(VRCStation station)
        {
            onEnterStation?.Invoke(station);
        }

        public void OnStationExit(VRCStation station)
        {
            onExitStation?.Invoke(station);
        }
    }
}