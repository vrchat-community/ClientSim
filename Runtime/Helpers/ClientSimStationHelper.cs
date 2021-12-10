
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimStationHelper : ClientSimBehaviour
    {
        private VRCStation station_;
        private VRCPlayerApi usingPlayer_;

        public Transform EnterLocation => station_.stationEnterPlayerLocation;

        public Transform ExitLocation => station_.stationExitPlayerLocation;

        public bool IsMobile =>
            station_.PlayerMobility == VRCStation.Mobility.Mobile &&
            !station_.seated;

        public bool IsSeated => station_.seated;

        public static void InitializeStations(VRCStation station)
        {
            ClientSimStationHelper prevHelper = station.gameObject.GetComponent<ClientSimStationHelper>();
            if (prevHelper != null)
            {
                DestroyImmediate(prevHelper);
                station.LogWarning("Destroying old station helper on object: " + VRC.Tools.GetGameObjectPath(station.gameObject));
            }

            ClientSimStationHelper helper = station.gameObject.AddComponent<ClientSimStationHelper>();
            if (!station.seated && station.PlayerMobility != VRCStation.Mobility.Mobile)
            {
                helper.LogWarning("Station has seated unchecked but is not mobile! This will immobilize the player, causing them to not be able to move on exit. Use VRCPlayerApi.Immobilize(false) to allow them to move again. " + VRC.Tools.GetGameObjectPath(station.gameObject));
            }
        }

        public static void UseStation(VRCStation station, VRCPlayerApi player)
        {
            ClientSimStationHelper helper = station.GetComponent<ClientSimStationHelper>();
            if (helper != null)
            {
                helper.UseStation(player);
            }
        }

        public static void ExitStation(VRCStation station, VRCPlayerApi player)
        {
            ClientSimStationHelper helper = station.GetComponent<ClientSimStationHelper>();
            if (helper != null)
            {
                helper.ExitStation(player);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            station_ = GetComponent<VRCStation>();

            CheckForMissingComponents();

            if (station_.stationEnterPlayerLocation == null)
            {
                station_.stationEnterPlayerLocation = transform;
            }
            if (station_.stationExitPlayerLocation == null)
            {
                station_.stationExitPlayerLocation = transform;
            }
        }

        private void CheckForMissingComponents()
        {
            Collider stationCollider = GetComponent<Collider>();
            if (stationCollider == null)
            {
                gameObject.AddComponent<BoxCollider>().isTrigger = true;
            }

#if UNITY_EDITOR
            UdonBehaviour udon = GetComponent<UdonBehaviour>();
            if (udon == null)
            {
                udon = gameObject.AddComponent<UdonBehaviour>();
                udon.interactText = "Sit";
                
                // TODO properly load udon chair program asset.
                AbstractUdonProgramSource program = UnityEditor.AssetDatabase.LoadAssetAtPath<AbstractUdonProgramSource>("Assets/VRChat Examples/Prefabs/VRCChair/StationGraph.asset");
                if (program != null)
                {
                    udon.AssignProgramAndVariables(program.SerializedProgramAsset, new UdonVariableTable());
                }
            }
#endif
        }

        public void UseStation(VRCPlayerApi player)
        {
            if (usingPlayer_ != null || !player.isLocal)
            {
                return;
            }

            usingPlayer_ = player;
            
            this.Log("Entering Station " + name);

            // Immobilize the player while sitting in the station. 
            // VRChatBug: Note that "mobile" stations require that seated be set to false and have mobility set to mobile.
            if (!IsMobile)
            {
                player.Immobilize(true);
            }
            
            var playerController = player.GetPlayerController();
            if (playerController != null)
            {
                playerController.EnterStation(this);
            }

            gameObject.OnStationEnter(station_);
        }

        public void ExitStation()
        {
            ExitStation(usingPlayer_);
        }
        
        private void ExitStation(VRCPlayerApi player)
        {
            if (usingPlayer_ != player)
            {
                return;
            }
            usingPlayer_ = null;

            this.Log("Exiting Station " + name);
            
            // If the station is set to seated, unset immobilize, allowing the player to move again.
            // VRChatBug: Note that players are set immobile based on if the station is mobile and seated, but setting
            // the player mobilized again is only if the station is not seated.
            if (station_.seated)
            {
                player.Immobilize(false);
            }
            
            gameObject.OnStationExit(station_);
            
            var playerController = player.GetPlayerController();
            if (playerController != null)
            {
                playerController.ExitStation(this);
            }
        }

        // Returns if should move
        public bool CanPlayerMoveWhileSeated(float speed)
        {
            if (Mathf.Abs(speed) >= 0.1f && !station_.disableStationExit)
            {
                ExitStation();
                return true;
            }

            if (IsMobile)
            {
                return true;
            }

            return false;
        }
    }
}