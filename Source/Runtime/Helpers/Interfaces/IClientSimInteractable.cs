using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimInteractable
    {
        float GetProximity();
        bool CanInteract();
        string GetInteractText();
        Vector3 GetInteractTextPlacement();
        void Interact();
    }
}