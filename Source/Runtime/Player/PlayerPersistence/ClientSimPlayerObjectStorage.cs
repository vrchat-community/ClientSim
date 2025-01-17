using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace VRC.SDK3.ClientSim.Persistence
{
    [AddComponentMenu("")] // hides component in Add Component menu
    public class ClientSimPlayerObjectStorage : ClientSimBehaviour
    {
#if VRC_ENABLE_PLAYER_PERSISTENCE
        public static string PlayerObjectsFolder => Path.Combine("ClientSimStorage", "PlayerObjects");
        internal static string ActiveSceneName;
        internal static string PlayerDataFilePath(VRCPlayerApi player) 
        {
            string root = Path.GetDirectoryName(Application.dataPath);
            string path = Path.Combine(root, PlayerObjectsFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + "/PlayerObject_" + $"{player.playerId}" + $"_{ActiveSceneName}" + ".json";
        }

        private VRCPlayerApi _player;
        private IClientSimUdonEventSender _udonEventSender;
        private IClientSimEventDispatcher _eventDispatcher;
        
        private Dictionary<int,IClientSimNetworkView> _persistentObjects = new Dictionary<int, IClientSimNetworkView>();
        private DataDictionary _persistentObjectData = new DataDictionary();
            
        private bool _isInitialized = false;
        private bool _HasJoined = false;
        
        private Coroutine _ContinuousUpdate;
        
        private const float _updateInterval = 1 / 4f;
        
        private bool hadUpdate = false;
        
        public void Init(VRCPlayerApi player, IClientSimUdonEventSender udonEventSender, IClientSimEventDispatcher eventDispatcher)
        {
            _player = player;
            _udonEventSender = udonEventSender;
            _eventDispatcher = eventDispatcher;
            
            ActiveSceneName = SceneManager.GetActiveScene().name;
            
            _eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            UdonBehaviour.RequestSerializationHook += RequestSerializationHook;
            _isInitialized = true;
            
            _ContinuousUpdate = StartCoroutine(UpdateContinuous());
        }

        private void OnDestroy()
        {
            if(_eventDispatcher != null)
                _eventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            
            if(_ContinuousUpdate != null)
                StopCoroutine(_ContinuousUpdate);
        }

        public IEnumerator UpdateContinuous()
        {
            while (true)
            {
                if (_HasJoined && _isInitialized)
                {
                    Encode();
                }
                
                yield return new WaitForSeconds(_updateInterval);
            }
        }
        
        public void RequestSerializationHook(UdonBehaviour udonBehaviour)
        {
            ClientSimNetworkEventSending.Instance.QueueRequest(udonBehaviour, this);
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent payload)
        {
            if (payload.player.playerId == _player.playerId)
            {
                _HasJoined = true;
                Decode();
            }
        }
        
        public void Encode(GameObject gameObject = null)
        {
            if (!_isInitialized || !_HasJoined) return;
            
            if(_persistentObjectData == null)
                _persistentObjectData = new DataDictionary();

            foreach (var keyValuePersistentObject in _persistentObjects)
            {
                DataToken key = keyValuePersistentObject.Key.ToString();
                if (!_persistentObjectData.ContainsKey(key))
                    _persistentObjectData.Add(key, new DataList());
                _persistentObjectData[key] = keyValuePersistentObject.Value.Encode(gameObject);
            }
            
            hadUpdate = true;
        }

        private async UniTask SaveToFile(string data)
        {
            await UniTask.SwitchToTaskPool();
            try{
                await File.WriteAllTextAsync(PlayerDataFilePath(_player), data);
            }
            catch (Exception e)
            {
                this.LogError($"Error saving PlayerObjects: {e.Message}");
            }
        }

        private void Decode()
        {
            string path = PlayerDataFilePath(_player);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}");
            }

            string json = File.ReadAllText(path);
            if (!VRCJson.TryDeserializeFromJson(json, out DataToken token))
            {
                this.LogError($"Error initializing PlayerObjects: {token.Error}");
                return;
            }

            _persistentObjectData = token.DataDictionary;
            ClientSimPlayer player = _player.GetClientSimPlayer();
            foreach (GameObject persistantObject in player.PlayerPersistenceObjects)
            {
                IClientSimNetworkId networkId = persistantObject.GetComponent<IClientSimNetworkId>();
                if (networkId == null) continue;
                int id = networkId.GetNetworkId();

                IClientSimNetworkView serializer = persistantObject.GetComponent<IClientSimNetworkView>();

                _persistentObjects.TryAdd(id, serializer);
                if (_persistentObjectData.TryGetValue(id.ToString(), out var data))
                {
                    _persistentObjects[id].Decode(data.DataList);
                }
            }
            
            _eventDispatcher.SendEvent(new ClientSimOnPlayerObjectsDecodedEvent { player = _player });
        }

        public void LateUpdate()
        {
            if (hadUpdate)
            {
                hadUpdate = false;
                _eventDispatcher.SendEvent(new ClientSimOnPlayerObjectUpdateEndedEvent());
                
                VRCJson.TrySerializeToJson(_persistentObjectData, JsonExportType.Beautify, out DataToken json);
                SaveToFile(json.String).Forget();
            }
        }
#endif
    }
}