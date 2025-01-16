
using System.IO;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimStationHelper : ClientSimBehaviour, IClientSimStation
    {
        private VRCStation _station;
        private VRCPlayerApi _usingPlayer;

        public Transform EnterLocation() => _station.stationEnterPlayerLocation;

        public Transform ExitLocation() => _station.stationExitPlayerLocation;

        public bool IsMobile() =>
            _station.PlayerMobility == VRCStation.Mobility.Mobile &&
            !_station.seated;

        public bool IsSeated() => _station.seated;

        public bool DisableStationExit() => _station.disableStationExit;
        
        public bool CanUseStationFromStation() => _station.canUseStationFromStation;
        
        public bool IsLockedInStation() => !IsMobile();
        
        public VRCStation GetStation() => _station;
        
        public GameObject GetStationGameObject() => gameObject;
        
        public Transform GetStationTransform() => transform;

        public bool IsOccupied() => _usingPlayer != null;

        public VRCPlayerApi GetCurrentSittingPlayer() => _usingPlayer;
        
        public static void InitializeStations(VRCStation station)
        {
            ClientSimStationHelper prevHelper = station.gameObject.GetComponent<ClientSimStationHelper>();
            if (prevHelper != null)
            {
                DestroyImmediate(prevHelper);
                station.LogWarning($"Destroying old station helper on object: {Tools.GetGameObjectPath(station.gameObject)}");
            }

            station.gameObject.AddComponent<ClientSimStationHelper>();
        }
        
        public static void UseAttachedStation(VRCPlayerApi player)
        {
            // UseAttachedStation is a method in the VRCPlayerApi class. This method will take the given player and try to put them in the station component on the GameObject running this Udon program. Since the GameObject is not provided in the parameters, it must be retrieved from the UdonManager by checking the current executing UdonBehaviour.
            UdonBehaviour currentUdon = UdonManager.Instance.currentlyExecuting;
            if (currentUdon == null)
            {
                return;
            }
            
            VRCStation station = currentUdon.GetComponent<VRCStation>();
            if (station == null)
            {
                return;
            }
            
            UseStation(station, player);
        }
        
        public static void UseStation(VRCStation station, VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                station.LogWarning($"Trying to force a remote player to enter a station. Force enter a station can only be done for the local player. PlayerId: {player.playerId}, Station: {Tools.GetGameObjectPath(station.gameObject)}");
                return;
            }
            
            ClientSimStationHelper helper = station.GetComponent<ClientSimStationHelper>();
            ClientSimPlayer clientPlayer = player.GetClientSimPlayer();
            if (helper != null && clientPlayer != null)
            {
                clientPlayer.GetStationHandler().EnterStation(helper);
            }
        }

        public static void ExitStation(VRCStation station, VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                station.LogWarning($"Trying to force a remote player to exit a station. Force exit a station can only be done for the local player. PlayerId: {player.playerId}, Station: {Tools.GetGameObjectPath(station.gameObject)}");
                return;
            }

            ClientSimStationHelper helper = station.GetComponent<ClientSimStationHelper>();
            ClientSimPlayer clientPlayer = player.GetClientSimPlayer();
            if (helper != null && clientPlayer != null)
            {
                clientPlayer.GetStationHandler().ExitStation(helper);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _station = GetComponent<VRCStation>();

            CheckForMissingComponents();

            if (_station.stationEnterPlayerLocation == null)
            {
                _station.stationEnterPlayerLocation = transform;
            }
            if (_station.stationExitPlayerLocation == null)
            {
                _station.stationExitPlayerLocation = transform;
            }
            
        }

        private void OnDestroy()
        {
            if (_usingPlayer != null)
            {
                ExitStation(_station, _usingPlayer);
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
                string sitProgramPath = Path.Combine("Assets", "VRChat Examples", "Prefabs", "VRCChair", "StationGraph.asset");
                AbstractUdonProgramSource program = UnityEditor.AssetDatabase.LoadAssetAtPath<AbstractUdonProgramSource>(sitProgramPath);
                if (program != null)
                {
                    udon.AssignProgramAndVariables(program.SerializedProgramAsset, new UdonVariableTable());
                }
            }
#endif
        }

        public void EnterStation(VRCPlayerApi player)
        {
            if (_usingPlayer != null || !player.isLocal)
            {
                return;
            }
            _usingPlayer = player;
        }
        
        public void ExitStation(VRCPlayerApi player)
        {
            if (_usingPlayer != player)
            {
                return;
            }
            _usingPlayer = null;
        }
    }
}