using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Class used to hold information related to the last raycast. This is created in ClientSimRaycaster.
    /// </summary>
    public class ClientSimRaycastResults
    {
        public readonly Ray ray;
        public readonly GameObject hitObject;
        public readonly Vector3 hitPoint;
        public readonly Vector3 hitNormal;
        public readonly float distance;
        public readonly VRC_UiShape uiShape;
        public readonly IClientSimInteractable interactable;

        public ClientSimRaycastResults(Ray ray, float distance)
        {
            this.ray = ray;
            hitPoint = ray.GetPoint(distance);
            this.distance = distance;
            
            hitNormal = Vector3.zero;
            
            hitObject = null;
            uiShape = null;
            interactable = null;
        }
        
        public ClientSimRaycastResults(Ray ray, RaycastHit hit)
        {
            this.ray = ray;
            hitObject = hit.collider.gameObject;
            hitPoint = hit.point;
            distance = hit.distance;
            hitNormal = hit.normal;
            
            uiShape = null;
            interactable = null;
        }
        
        // TODO create results from SphereOverlap
        
        public ClientSimRaycastResults(Ray ray, RaycastHit hit, VRC_UiShape shape) : this(ray, hit)
        {
            uiShape = shape;
        }
        
        public ClientSimRaycastResults(Ray ray, RaycastHit hit, IClientSimInteractable interactable) : this(ray, hit)
        {
            this.interactable = interactable;
        }

        public IClientSimPickupable GetPickupable()
        {
            if (interactable != null)
            {
                return hitObject.GetComponent<IClientSimPickupable>();
            }

            return null;
        }
    }
}