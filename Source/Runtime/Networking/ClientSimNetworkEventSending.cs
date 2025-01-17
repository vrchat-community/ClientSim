using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using VRC.SDK3.ClientSim.Persistence;
using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimNetworkEventSending
    {
#if VRC_ENABLE_PLAYER_PERSISTENCE
        private static ClientSimNetworkEventSending _instance;
        
        public static ClientSimNetworkEventSending Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClientSimNetworkEventSending();
                }

                return _instance;
            }
        }
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private Dictionary<ClientSimPlayerObjectStorage,List<UdonBehaviour>> _RequestedObjects = new Dictionary<ClientSimPlayerObjectStorage,List<UdonBehaviour>>();
        
        private const int TIME_BETWEEN_EVENTS = 50;
        
        private ClientSimNetworkEventSending()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SendNetworkEvents().Forget();
        }
        
        ~ClientSimNetworkEventSending()
        {
            _cancellationTokenSource.Cancel();
        }
        
        public void QueueRequest(UdonBehaviour udonBehaviour, ClientSimPlayerObjectStorage playerObjectStorage)
        {
            if(!_RequestedObjects.ContainsKey(playerObjectStorage)){
                List<UdonBehaviour> udonBehaviours = new List<UdonBehaviour>();
                udonBehaviours.Add(udonBehaviour);
                _RequestedObjects[playerObjectStorage] = udonBehaviours;
            }
            else
            {
                if(!_RequestedObjects[playerObjectStorage].Contains(udonBehaviour))
                    _RequestedObjects[playerObjectStorage].Add(udonBehaviour);
            }
        }

        private async UniTask SendNetworkEvents()
        {
            UniTask.SwitchToMainThread();
            
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await UniTask.Delay(TIME_BETWEEN_EVENTS, cancellationToken: _cancellationTokenSource.Token);

                List<ClientSimPlayerObjectStorage> keys = _RequestedObjects.Keys.ToList();
                
                for (int i = keys.Count-1; i >= 0; i--)
                {
                    ClientSimPlayerObjectStorage storage = keys[i];
                    List<UdonBehaviour> udonBehaviours = _RequestedObjects[storage];
                    for (int j = udonBehaviours.Count-1; j >= 0; j--)
                    {
                        storage.Encode(udonBehaviours[j].gameObject);
                    }
                }
                
                _RequestedObjects.Clear();
            }
        }
#endif
    }
}