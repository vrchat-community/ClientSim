using System.Collections.Generic;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This class is responsible for saying if an object can be interacted with given the distance.
    /// </summary>
    public class ClientSimInteractManager : IClientSimInteractManager
    {
        public const float INTERACT_SCALE = 1.25f;
        
        private readonly IClientSimTrackingProvider _trackingScaleProvider;
        private readonly IClientSimPlayerPickupData _pickupData;

        public ClientSimInteractManager(
            IClientSimTrackingProvider trackingScaleProvider, 
            IClientSimPlayerPickupData pickupData)
        {
            _trackingScaleProvider = trackingScaleProvider;
            _pickupData = pickupData;
        }
        
        private float CalculateInteractDistanceFormula()
        {
            return _trackingScaleProvider.GetTrackingScale() * INTERACT_SCALE;
        }

        public IClientSimInteractable GetFirstInteractable(GameObject obj, float distance)
        {
            foreach (var interactable in obj.GetComponents<IClientSimInteractable>())
            {
                if (CanInteract(interactable, distance))
                {
                    return interactable;
                }
            }

            return null;
        }

        public bool CanInteract(IClientSimInteractable interactable, float distance)
        {
            if (interactable is IClientSimPickupable && !_pickupData.GetPickupsEnabled())
            {
                return false;
            }
            
            float proximityCalculation = CalculateInteractDistanceFormula() + interactable.GetProximity();
            return interactable.CanInteract() && distance <= proximityCalculation;
        }
        
        public List<IClientSimInteractable> Interact(GameObject obj, float distance)
        {
            List<IClientSimInteractable> interacts = new List<IClientSimInteractable>();
            foreach (var interactable in obj.GetComponents<IClientSimInteractable>())
            {
                if (CanInteract(interactable, distance))
                {
                    interactable.Interact();
                    interacts.Add(interactable);
                }
            }

            return interacts;
        }
    }
}