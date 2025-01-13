using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System that handles spawning new players for ClientSim.
    /// New players will then be initialized through the PlayerManager.
    /// </summary>
    [AddComponentMenu("")]
    public class ClientSimPlayerSpawner : ClientSimBehaviour
    {
        [SerializeField]
        private GameObject localPlayerPrefab;
        [SerializeField]
        private GameObject remotePlayerPrefab;

        private IClientSimSceneManager _sceneManager;
        private IClientSimPlayerManager _playerManager;
        private IClientSimBlacklistManager _blacklistManager;
        private IClientSimEventDispatcher _eventDispatcher;
        private Transform _parent;

        public void Initialize(
            IClientSimSceneManager sceneManager,
            IClientSimPlayerManager playerManager,
            IClientSimBlacklistManager blacklistManager,
            IClientSimEventDispatcher eventDispatcher,
            Transform parent)
        {
            _sceneManager = sceneManager;
            _playerManager = playerManager;
            _blacklistManager = blacklistManager;
            _eventDispatcher = eventDispatcher;
            _parent = parent;
        }

        public ClientSimPlayer SpawnPlayer(string playerName, bool isLocal)
        {
            if (!_sceneManager.HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot spawn player if there is no world descriptor!");
            }
            
            GameObject playerPrefab = isLocal ? localPlayerPrefab : remotePlayerPrefab;
            if (playerPrefab == null)
            {
                throw new ClientSimException("Failed to spawn player! Player prefab was not found.");
            }

            Transform spawn = _sceneManager.GetSpawnPoint(!isLocal);
            Quaternion rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);

            GameObject playerInstance = Instantiate(playerPrefab, spawn.position, rotation, _parent);

            if (_parent == null)
            {
                DontDestroyOnLoad(playerInstance);
            }
            _blacklistManager.AddObjectAndChildrenToBlackList(playerInstance);
            
            ClientSimPlayer player = playerInstance.GetComponent<ClientSimPlayer>();
            if (player == null)
            {
                throw new ClientSimException("Failed to spawn player! ClientSimPlayer script was not found.");
            }
            
            // PlayerManager will automatically handle sending player join event
            _playerManager.CreateNewPlayer(isLocal, player, playerName);

            
            if (isLocal)
            {
                // Disable player controller until ClientSim is initialized, which is when the player should be able to gain control.
                playerInstance.SetActive(false);
            }

            player.SetEventDispatcher(_eventDispatcher);
            
            return player;
        }
        
        
    }
}