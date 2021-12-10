using System.Reflection;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimUdonHelper : ClientSimSyncedObjectHelper, IClientSimInteractable, IClientSimPickupable, IClientSimStationHandler, IClientSimSyncableHandler
    {
        private static FieldInfo isReady_ = 
            typeof(UdonBehaviour).GetField("_isReady", (BindingFlags.Instance | BindingFlags.NonPublic));

        private UdonBehaviour udonBehaviour_;

        public static void OnInit(UdonBehaviour behaviour, IUdonProgram program)
        {
            ClientSimUdonHelper helper = behaviour.gameObject.AddComponent<ClientSimUdonHelper>();
            helper.SetUdonBehaviour(behaviour);

            isReady_.SetValue(behaviour, ClientSimMain.IsNetworkReady());
        }

        public void OnNetworkReady()
        {
            isReady_.SetValue(udonBehaviour_, true);
        }

        private void Start()
        {
            if (udonBehaviour_ == null)
            {
                DestroyImmediate(this);
            }
        }

        private void SetUdonBehaviour(UdonBehaviour udonBehaviour)
        {
            if (udonBehaviour == null)
            {
                this.LogError("UdonBehaviour is null. Destroying helper.");
                DestroyImmediate(this);
                return;
            }
            udonBehaviour_ = udonBehaviour;
#pragma warning disable 618
            SyncPosition = udonBehaviour_.SynchronizePosition;
#pragma warning restore 618

            ClientSimUdonManager.AddUdonBehaviour(udonBehaviour_);
        }

        public UdonBehaviour GetUdonBehaviour()
        {
            return udonBehaviour_;
        }

        private void OnDestroy()
        {
            ClientSimUdonManager.RemoveUdonBehaviour(udonBehaviour_);
        }

        #region IClientSimSyncableHandler

        public void OnOwnershipTransferred(int ownerID)
        {
            udonBehaviour_.RunEvent("_onOwnershipTransferred", ("Player", VRCPlayerApi.GetPlayerById(ownerID)));
        }

        #endregion

        #region IClientSimInteractable

        public float GetProximity()
        {
            return udonBehaviour_.proximity;
        }

        public bool CanInteract()
        {
            return udonBehaviour_.IsInteractive;
        }

        public string GetInteractText()
        {
            return udonBehaviour_.interactText;
        }

        public void Interact()
        {
            udonBehaviour_.Interact();
        }

        #endregion

        #region IClientSimPickupable

        public void OnPickup()
        {
            udonBehaviour_.OnPickup();
        }

        public void OnDrop()
        {
            udonBehaviour_.OnDrop();
        }

        public void OnPickupUseDown()
        {
            udonBehaviour_.OnPickupUseDown();
        }

        public void OnPickupUseUp()
        {
            udonBehaviour_.OnPickupUseUp();
        }

        #endregion

        #region IClientSimStationHandler

        public void OnStationEnter(VRCStation station)
        {
            VRC.SDK3.Components.VRCStation sdk3Station = station as VRC.SDK3.Components.VRCStation;
            udonBehaviour_.RunEvent(sdk3Station.OnLocalPlayerEnterStation, ("Player", Networking.LocalPlayer));
        }

        public void OnStationExit(VRCStation station)
        {
            VRC.SDK3.Components.VRCStation sdk3Station = station as VRC.SDK3.Components.VRCStation;
            udonBehaviour_.RunEvent(sdk3Station.OnLocalPlayerExitStation, ("Player", Networking.LocalPlayer));
        }

        #endregion
    }
}