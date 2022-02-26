using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Performs the physics raycast to find objects that can be interacted with.
    /// </summary>
    public class ClientSimRaycaster
    {
        private const int MAX_INTERACT_RESULTS = 100;
        private const float MAX_INTERACT_DISTANCE = 100;
        
        private static readonly RaycastHit[] _raycastHitBuffer = new RaycastHit[MAX_INTERACT_RESULTS];
        private static readonly RaycastHitComparer _raycastHitComparerInstance = new RaycastHitComparer();
        

        private readonly IClientSimRayProvider _rayProvider;
        private readonly IClientSimInteractiveLayerProvider _interactiveLayerProvider;
        private readonly IClientSimInteractManager _interactManager;
        
        // TODO pass in parameter if should perform overlap sphere at ray origin (eg Desktop should not but VR should)
        public ClientSimRaycaster(
            IClientSimRayProvider rayProvider, 
            IClientSimInteractiveLayerProvider interactiveLayerProvider,
            IClientSimInteractManager interactManager)
        {
            _rayProvider = rayProvider;
            _interactiveLayerProvider = interactiveLayerProvider;
            _interactManager = interactManager;
        }

        public ClientSimRaycastResults CheckForInteracts()
        {
            Ray ray = _rayProvider.GetRay();
            int layers = _interactiveLayerProvider.GetInteractiveLayers();

            // TODO use Physics.OverlapSphere to check for objects close to the ray origin.
            // TODO make sure to scale based on tracking scale.

            // Note: This method has a chance of failing to find objects that move but do not have rigidbodies.
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                _raycastHitBuffer,
                MAX_INTERACT_DISTANCE,
                layers);
            
            Array.Sort(_raycastHitBuffer, 0, hitCount, _raycastHitComparerInstance);

            // Go through all colliders in order of distance and stop after find something
            // interactable, or a physical collider blocking everything else. 
            for (int curHit = 0; curHit < hitCount; ++curHit)
            {
                // VRChatBug: This implementation ignores the bug where trigger colliders extend a player's proximity.
                // https://feedback.vrchat.com/sdk-bug-reports/p/incorrect-proximity-calculation-when-aiming-through-trigger-colliders
                
                RaycastHit hit = _raycastHitBuffer[curHit];
                GameObject hitObject = hit.collider.gameObject;

                // UIShapes are higher priority than interacts/pickups.
                VRC_UiShape shape = hitObject.GetComponent<VRC_UiShape>();
                if (shape != null)
                {
                    return new ClientSimRaycastResults(ray, hit, shape);
                }

                if (ShouldIgnoreObject(hitObject))
                {
                    continue;
                }
                
                IClientSimInteractable interactable = _interactManager.GetFirstInteractable(hitObject, hit.distance);
                if (interactable != null)
                {
                    return new ClientSimRaycastResults(ray, hit, interactable);
                }

                // Object found but can't interact with it. 
                if (!hit.collider.isTrigger)
                {
                    return new ClientSimRaycastResults(ray, hit);
                }
            }

            return new ClientSimRaycastResults(ray, MAX_INTERACT_DISTANCE);
        }
        
        private bool ShouldIgnoreObject(GameObject hitObject)
        {
            if (hitObject == null)
            {
                return true;
            }
            
            // Do not allow raycasting other players. ClientSim has no reason to select players, so ignore their colliders.
            if (VRCPlayerApi.GetPlayerByGameObject(hitObject) != null)
            {
                return true;
            }
            
            // Do not allow raycasting occupied stations
            // VRChatBug: Raycasting to your own station appears to not work properly in Udon.
            // Remove the last check on if the player is local to remove this. 
            // Since only local players can enter stations in ClientSim, this code will not be needed unless a method is introduced for remote players to enter stations.
            IClientSimStation stationHandler = hitObject.GetComponent<IClientSimStation>();
            if (stationHandler != null && stationHandler.IsOccupied() && !stationHandler.GetCurrentSittingPlayer().isLocal)
            {
                return true;
            }
            
            return false;
        }

        private class RaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}