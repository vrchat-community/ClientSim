using System.Reflection;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimUdonHelper : 
        ClientSimPositionSyncedHelperBase, 
        IClientSimInteractable, 
        IClientSimPickupHandler, 
        IClientSimStationHandler, 
        IClientSimSyncableHandler
    {
        private IClientSimUdonManager _udonManager;
        private UdonBehaviour _udonBehaviour;
        
        private static readonly FieldInfo _isReady = 
            typeof(UdonBehaviour).GetField("_isReady", (BindingFlags.Instance | BindingFlags.NonPublic));

        public void Initialize(
            UdonBehaviour udonBehaviour, 
            IClientSimUdonManager udonManager, 
            IClientSimSyncedObjectManager syncedObjectManager, 
            bool isReady)
        {
            _udonBehaviour = udonBehaviour;
#pragma warning disable 618
            SyncPosition = _udonBehaviour.SynchronizePosition;
#pragma warning restore 618

            SetIsReady(isReady);

            _udonManager = udonManager;
            _udonManager.AddUdonBehaviour(_udonBehaviour);

            // Ensure that SyncPosition is set before calling this.
            base.Initialize(syncedObjectManager);
        }
        
        private void Start()
        {
            // Catch Helper not initialized.
            if (_udonBehaviour == null)
            {
                this.LogWarning($"Destroying uninitialized Helper. Object: {Tools.GetGameObjectPath(gameObject)}");
                DestroyImmediate(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Nullable needed for uninitialized case.
            _udonManager?.RemoveUdonBehaviour(_udonBehaviour);
        }
        
        private void SetIsReady(bool isReady)
        {
            _isReady.SetValue(_udonBehaviour, isReady);
        }

        public void OnReady()
        {
            SetIsReady(true);
        }
        
        public UdonBehaviour GetUdonBehaviour()
        {
            return _udonBehaviour;
        }

        #region IClientSimSyncableHandler

        public void OnOwnershipTransferred(int ownerID)
        {
            _udonBehaviour.RunEvent("_onOwnershipTransferred", ("Player", VRCPlayerApi.GetPlayerById(ownerID)));
        }

        #endregion

        #region IClientSimInteractable

        public float GetProximity()
        {
            return _udonBehaviour.proximity;
        }

        public bool CanInteract()
        {
            return _udonBehaviour.IsInteractive;
        }

        public string GetInteractText()
        {
            return _udonBehaviour.interactText;
        }

        public Vector3 GetInteractTextPlacement()
        {
            // VRChatBug: Tooltips always ignore the tooltipPlacement transform and instead place the tooltip at the top
            // of the first collider on the object.
            
            //check if this object has already been destroyed, we can't just do a null check because that still throws a destroyed object error in unity
            if (!Utilities.IsValid(this))
            {
                return Vector3.zero;
            }


            return ClientSimTooltip.GetToolTipPosition(gameObject);
        }

        public void Interact()
        {
            _udonBehaviour.Interact();
        }

        #endregion

        #region IClientSimPickupable

        public void OnPickup()
        {
            _udonBehaviour.OnPickup();
        }

        public void OnDrop()
        {
            _udonBehaviour.OnDrop();
        }

        public void OnPickupUseDown()
        {
            _udonBehaviour.OnPickupUseDown();
        }

        public void OnPickupUseUp()
        {
            _udonBehaviour.OnPickupUseUp();
        }

        #endregion

        #region IClientSimStationHandler

        public void OnStationEnter(VRCStation station)
        {
            VRC.SDK3.Components.VRCStation sdk3Station = station as VRC.SDK3.Components.VRCStation;
            _udonBehaviour.RunEvent(sdk3Station.OnLocalPlayerEnterStation, ("Player", Networking.LocalPlayer));
        }

        public void OnStationExit(VRCStation station)
        {
            VRC.SDK3.Components.VRCStation sdk3Station = station as VRC.SDK3.Components.VRCStation;
            _udonBehaviour.RunEvent(sdk3Station.OnLocalPlayerExitStation, ("Player", Networking.LocalPlayer));
        }

        #endregion
    }
}