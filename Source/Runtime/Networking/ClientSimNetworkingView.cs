using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimNetworkingView : ClientSimBehaviour, IClientSimNetworkId, IClientSimNetworkView
    {
        private int _networkId;
        private int _playerId;
        private ClientSimNetworkingUtilities.OwnershipOption _ownershipOption;
        private List<IClientSimNetworkSerializer> _networkedObjects = new List<IClientSimNetworkSerializer>();
        private bool _persist = true;
        private DataList _data;
        
        public void SetNetworkId(int networkId)
        {
            _networkId = networkId;
        }
        
        public void SetPersist(bool persist)
        {
            _persist = persist;
        }
        
        public bool GetPersist()
        {
            return _persist;
        }

        public int GetNetworkId()
        {
            return _networkId;
        }
        
        public void SetPlayerId(int playerId)
        {
            _playerId = playerId;
        }
        
        public int GetPlayerId()
        {
            return _playerId;
        }

        public void OwnershipStyle(ClientSimNetworkingUtilities.OwnershipOption option)
        {
            _ownershipOption = option;
        }

        public void AddNetworkedObject(IClientSimNetworkSerializer obj)
        {
            if(!_networkedObjects.Contains(obj))
                _networkedObjects.Add(obj);
        }

        public DataList Encode(GameObject gameObject = null)
        {
            _data ??= new DataList();
            
            if(!_persist) return _data;
            
            for (int i = 0; i < _networkedObjects.Count; i++)
            {
                IClientSimNetworkSerializer obj = _networkedObjects[i];
                obj.PreEncode(gameObject);
                if(obj.IsDirty(gameObject))
                {
                    DataList objData = obj.Encode(gameObject);
                    if(objData != null)
                        if(_data.Count > i)
                            _data[i] = objData;
                        else
                            _data.Add(objData);
                }
                obj.PostEncode(gameObject);
            }
            
            return _data;
        }

        public void Decode(DataList data)
        {
            _data = data;
            // If the data is not the same size as the networked objects, we can't decode it. This is possible if PlayerObjects are added or removed.
            if(data.Count != _networkedObjects.Count)
            {
                return;
            }
            
            for (int i =0; i < _networkedObjects.Count;i++)
            {
                _networkedObjects[i].Decode(data[i].DataList);
            }
        }
    }
}