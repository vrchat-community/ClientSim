using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimPickupHelper : ClientSimBehaviour, IClientSimPickupable
    {
        private Rigidbody _rigidbody;
        private VRC_Pickup _pickup;

        private VRCPlayerApi _heldPlayer;
        private VRC_Pickup.PickupHand _heldHand;
        private Action<IClientSimPickupable> _forceDropHandler;

        public static void InitializePickup(VRC_Pickup pickup)
        {
            ClientSimPickupHelper previousHelper = pickup.gameObject.GetComponent<ClientSimPickupHelper>();
            if (previousHelper != null)
            {
                DestroyImmediate(previousHelper);
                pickup.LogWarning($"Destroying old pickup helper on object: {Tools.GetGameObjectPath(pickup.gameObject)}");
            }

            ClientSimPickupHelper helper = pickup.gameObject.AddComponent<ClientSimPickupHelper>();
            helper.SetPickup(pickup);
        }

        public static void ForceDrop(VRC_Pickup pickup)
        {
            ClientSimPickupHelper helper = pickup.GetComponent<ClientSimPickupHelper>();
            if (helper)
            {
                helper._forceDropHandler?.Invoke(helper);
            }
        }

        public static VRCPlayerApi GetCurrentPlayer(VRC_Pickup pickup)
        {
            ClientSimPickupHelper helper = pickup.GetComponent<ClientSimPickupHelper>();

            if (!helper)
            {
                return null;
            }
            
            return helper.GetHoldingPlayer();
        }

        public static VRC_Pickup.PickupHand GetPickupHand(VRC_Pickup pickup)
        {
            ClientSimPickupHelper helper = pickup.GetComponent<ClientSimPickupHelper>();
            if (helper)
            {
                return helper._heldHand;
            }
            return VRC_Pickup.PickupHand.None;
        }

        public static void PickupDestroy(VRC_Pickup pickup)
        {
            ForceDrop(pickup);
        }
        
        public static void PlayHapticForPickup(VRC_Pickup obj, float duration, float amplitude, float frequency)
        {
            VRCPlayerApi player = obj.currentPlayer;
            VRC_Pickup.PickupHand hand = obj.currentHand;
            if (Utilities.IsValid(player) && hand != VRC_Pickup.PickupHand.None)
            {
                player.PlayHapticEventInHand(obj.currentHand, duration, amplitude, frequency);
            }
        }

        private void SetPickup(VRC_Pickup pickup)
        {
            _pickup = pickup;
            _rigidbody = GetComponent<Rigidbody>();
        }
        

        #region IClientSimInteractible

        public float GetProximity()
        {
            return _pickup.proximity;
        }

        public bool CanInteract()
        {
            return _pickup.pickupable;
        }

        public string GetInteractText()
        {
            if (!string.IsNullOrEmpty(_pickup.InteractionText))
            {
                return _pickup.InteractionText;
            }

            return AutoHold() ? "Equip" : "Hold to Grab";
        }
        
        public Vector3 GetInteractTextPlacement()
        {
            // VRChatBug: Tooltips always ignore the tooltipPlacement transform and instead place the tooltip at the top
            // of the first collider on the object.
            return ClientSimTooltip.GetToolTipPosition(gameObject);
        }

        public void Interact() { }

        #endregion

        #region IClientSimPickupable

        public void Pickup(VRCPlayerApi player, VRC_Pickup.PickupHand heldHand, Action<IClientSimPickupable> forceDropHandler)
        {
            if (IsHeld())
            {
                return;
            }
            _heldPlayer = player;
            _heldHand = heldHand;
            _forceDropHandler = forceDropHandler;
        }

        public void Drop(VRCPlayerApi player)
        {
            if (GetHoldingPlayer() != player)
            {
                return;
            }
            _heldPlayer = null;
            _heldHand = VRC_Pickup.PickupHand.None;
            _forceDropHandler = null;
        }
        
        public bool IsHeld() => Utilities.IsValid(_heldPlayer);

        public VRCPlayerApi GetHoldingPlayer() => IsHeld() ? _heldPlayer : null;

        public bool AutoHold() => _pickup.AutoHold == VRC_Pickup.AutoHoldMode.Yes;

        public GameObject GetGameObject() => gameObject;

        public Transform GetTransform() => transform;

        public Rigidbody GetRigidbody() => _rigidbody;

        public VRC_Pickup GetPickup() => _pickup;

        public VRC_Pickup.PickupOrientation GetOrientation() => _pickup.orientation;

        public Transform GetGunLocation() => _pickup.ExactGun;

        public Transform GetGripLocation() => _pickup.ExactGrip;

        public float GetThrowVelocityBoostScale() => _pickup.ThrowVelocityBoostScale;

        public bool AllowManipulation() => _pickup.allowManipulationWhenEquipped;

        #endregion

        // TODO display use text after picking up a pickup.
        public string PickupText()
        {
            return _pickup.UseText;
        }
    }
}
