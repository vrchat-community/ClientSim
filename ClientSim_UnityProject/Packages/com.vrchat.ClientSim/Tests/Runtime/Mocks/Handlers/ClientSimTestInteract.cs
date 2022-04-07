using System;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Tests
{
    // Since Udon cannot be used in tests, this behaviour is here to allow testing the interaction flow without Udon.
    [AddComponentMenu("")]
    public class ClientSimTestInteract : ClientSimBehaviour, IClientSimInteractable
    {
        public float proximity = 2;
        public bool canInteract = true;
        public string interactText = "Use";
        public Action onInteract;

        public float GetProximity()
        {
            return proximity;
        }

        public bool CanInteract()
        {
            return canInteract;
        }

        public string GetInteractText()
        {
            return interactText;
        }

        public Vector3 GetInteractTextPlacement()
        {
            return ClientSimTooltip.GetToolTipPosition(gameObject);
        }

        public void Interact()
        {
            onInteract?.Invoke();
        }
    }
}