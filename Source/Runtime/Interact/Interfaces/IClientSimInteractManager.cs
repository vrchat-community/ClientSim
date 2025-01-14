using System.Collections.Generic;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimInteractManager
    {
        IClientSimInteractable GetFirstInteractable(GameObject obj, float distance);
        bool CanInteract(IClientSimInteractable interactable, float distance);
        List<IClientSimInteractable> Interact(GameObject obj, float distance);
    }
}