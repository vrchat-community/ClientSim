using System;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimInput
    {
        float GetMovementHorizontal();
        float GetMovementVertical();
        Vector2 GetMovementAxes();
        float GetLookHorizontal();
        float GetLookVertical();
        Vector2 GetLookAxes();

        float GetPickupRotateUpDown();
        float GetPickupRotateLeftRight();
        float GetPickupRotateCwCcw();
        float GetPickupManipulateDistance();


        void SubscribeJump(Action<bool, HandType> handler);
        void UnsubscribeJump(Action<bool, HandType> handler);
        
        void SubscribeUse(Action<bool, HandType> handler);
        void UnsubscribeUse(Action<bool, HandType> handler);
        
        void SubscribeGrab(Action<bool, HandType> handler);
        void UnsubscribeGrab(Action<bool, HandType> handler);
        
        void SubscribeDrop(Action<bool, HandType> handler);
        void UnsubscribeDrop(Action<bool, HandType> handler);
        
        void SubscribeToggleMenu(Action<bool, HandType> handler);
        void UnsubscribeToggleMenu(Action<bool, HandType> handler);
        

        void SubscribeRun(Action<bool> handler);
        void UnsubscribeRun(Action<bool> handler);
        
        void SubscribeToggleCrouch(Action<bool> handler);
        void UnsubscribeToggleCrouch(Action<bool> handler);
        
        void SubscribeToggleProne(Action<bool> handler);
        void UnsubscribeToggleProne(Action<bool> handler);
        
        void SubscribeReleaseMouse(Action<bool> handler);
        void UnsubscribeReleaseMouse(Action<bool> handler);

        public void SubscribeInputChangedEvent(Action<VRCInputMethod> handler);
        public void UnsubscribeInputChangedEvent(Action<VRCInputMethod> handler);
    }
}