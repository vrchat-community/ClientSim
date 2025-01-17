using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPickupable : IClientSimInteractable
    {
        bool IsHeld();
        VRCPlayerApi GetHoldingPlayer();
        bool AutoHold();
        GameObject GetGameObject();
        Transform GetTransform();
        Rigidbody GetRigidbody();
        VRC_Pickup GetPickup();
        VRC_Pickup.PickupOrientation GetOrientation();
        Transform GetGunLocation();
        Transform GetGripLocation();
        float GetThrowVelocityBoostScale();
        bool AllowManipulation();
        void Pickup(VRCPlayerApi player, VRC_Pickup.PickupHand heldHand, Action<IClientSimPickupable> forceDrop);
        void Drop(VRCPlayerApi player);
    }
}