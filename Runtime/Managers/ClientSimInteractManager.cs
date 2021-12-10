using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-100)] // Execute before the player controller to ensure that the player has not yet moved for this frame.
    public class ClientSimInteractManager : ClientSimBehaviour
    {
        private const int MAX_INTERACT_RAYCASTS_ = 100;
        
        private static readonly RaycastHit[] raycastHitBuffer = new RaycastHit[MAX_INTERACT_RAYCASTS_];
        private static readonly RaycastHitComparer raycastHitComparer = new RaycastHitComparer();

        private ClientSimHighlightManager highlight_;
        private Transform raycastTransform_;
        private ClientSimInputModule inputModule_;
        private ClientSimBaseInput baseInput_;
        private ClientSimPlayerController playerController_;
        private Camera playerCamera_;

        public void Initialize(ClientSimPlayerController playerController, Transform raycastTransform)
        {
            raycastTransform_ = raycastTransform;
            playerController_ = playerController;
            playerCamera_ = playerController.GetCamera();
            
            inputModule_ = GetComponentInParent<ClientSimInputModule>();
            baseInput_ = GetComponentInParent<ClientSimBaseInput>();
            
            highlight_ = ClientSimHighlightManager.CreateInteractHelper(transform, playerCamera_.transform);
            highlight_.SetActive(false);
        }

        private void Update()
        {
            CheckForInteracts();
        }

        private void CheckForInteracts()
        {
            // Disable interact check if holding pickup
            if (playerController_.IsHoldingPickup(VRC_Pickup.PickupHand.Right))
            {
                highlight_.SetActive(false);
                return;
            }
            
            bool display = false;
            Ray ray = new Ray(raycastTransform_.position, raycastTransform_.forward);
            
            // Follow the mouse while the menu is open.
            // TODO check if tab is held down
            if (baseInput_.isMenuOpen)
            {
                ray = playerCamera_.ScreenPointToRay(baseInput_.mousePosition);
            }
            
            int hitCount = Physics.RaycastNonAlloc(ray, raycastHitBuffer, float.MaxValue, inputModule_.GetInteractiveLayers());

            Array.Sort(raycastHitBuffer, 0, hitCount, raycastHitComparer);

            // Go through all colliders in order of distance and stop after find something
            // interactable, or a physical collider blocking everything else. 
            for (int curHit = 0; curHit < hitCount; ++curHit)
            {
                // VRChatBug: This implementation ignores the bug where trigger colliders extend a player's proximity.
                // https://feedback.vrchat.com/sdk-bug-reports/p/incorrect-proximity-calculation-when-aiming-through-trigger-colliders
                
                RaycastHit hit = raycastHitBuffer[curHit];
                GameObject hitObject = hit.collider.gameObject;

                // UIShapes are higher priority than interacts/pickups.
                VRC_UiShape shape = hitObject.GetComponent<VRC_UiShape>();
                if (shape != null)
                {
                    // Notify the input module that a UI Shape was found to allow ui interactions.
                    inputModule_.InteractRayHitUIShape();
                    break;
                }
                
                IClientSimInteractable interactable = hitObject.GetFirstInteractable(hit.distance);
                if (interactable != null)
                {
                    highlight_.HighlightCollider(hit.collider, interactable.GetInteractText());
                    display = true;

                    // TODO get input from input manager
                    if (Input.GetMouseButtonDown(0))
                    {
                        hitObject.Interact(hit.distance);
                    }

                    break;
                }

                if (!hit.collider.isTrigger)
                {
                    break;
                }
            }

            highlight_.SetActive(display);
        }

        class RaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}