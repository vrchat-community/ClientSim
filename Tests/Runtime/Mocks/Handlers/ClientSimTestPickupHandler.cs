using System;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Tests
{
    [AddComponentMenu("")]
    public class ClientSimTestPickupHandler : ClientSimBehaviour, IClientSimPickupHandler
    {
        public Action onPickupAction;
        public Action onDropAction;
        public Action onPickupUseDownAction;
        public Action onPickupUseUpAction;

        public void OnPickup()
        {
            onPickupAction?.Invoke();
        }

        public void OnDrop()
        {
            onDropAction?.Invoke();
        }

        public void OnPickupUseDown()
        {
            onPickupUseDownAction?.Invoke();
        }

        public void OnPickupUseUp()
        {
            onPickupUseUpAction?.Invoke();
        }
    }
}