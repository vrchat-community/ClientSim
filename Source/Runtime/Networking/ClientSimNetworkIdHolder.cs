using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.SDK3.ClientSim.EncodeDecoders;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRC.SDK3.ClientSim
{
    public class ClientSimNetworkIdHolder : ClientSimBehaviour, IClientSimNetworkSerializer
    {
        internal ClientSimNetworkingView _networkId;
        internal DataList _data = new DataList();
        private bool IsManual = false;
        
        private bool _isInitialized;
        
        internal  List<MonoBehaviour> _components;
        internal static Dictionary<string,IClientSimEncodeDecoder> encodeDecoders = new Dictionary<string,IClientSimEncodeDecoder>
        {
            { typeof(VRCObjectSync).FullName, new ClientSimObjectSyncEncodeDecoder() },
            { typeof(UdonBehaviour).FullName, new ClientSimUdonEncodeDecode() },
            { typeof(VRCObjectPool).FullName, new ClientSimObjectPoolEncodeDecode()}
        };
        
        public void SetNetworkView(ClientSimNetworkingView networkId)
        {
            _networkId = networkId;
        }

        public ClientSimNetworkingView GetNetworkView()
        {
            return _networkId;
        }

        public int GetPlayerId()
        {
            return _networkId.GetPlayerId();
        }

        public void SetNetworkComponents()
        {
            _components = new List<MonoBehaviour>();
            MonoBehaviour[] allComponents = GetComponents<MonoBehaviour>();
            IsManual = true;
            for (int i =0; i < allComponents.Length;i++)
            {
                if (encodeDecoders.ContainsKey(allComponents[i].GetType().FullName))    
                {
                    if (allComponents[i].GetType() == typeof(UdonBehaviour))
                    {
                        if (((UdonBehaviour)allComponents[i]).SyncMethod == Networking.SyncType.None)
                            continue;
                    }

                    IsManual = IsManual && encodeDecoders[allComponents[i].GetType().FullName].IsManualSynced(allComponents[i]);
                    _components.Add(allComponents[i]);
                }
            }
        }
        
        public bool IsDirty(GameObject gameObject = null)
        {
            if(this == null)
                return false;
            
            if(gameObject != null)
                if(gameObject != this.gameObject)
                    return false;
            
            if(IsManual && gameObject == null)
                return false;
            
            if(_data.Count < _components.Count)
                return true;
            
            for(int i = 0; i < _components.Count;i++)
            {
                string componentType = _components[i].GetType().FullName;
                if (encodeDecoders.TryGetValue(componentType, out var encodeDecoder))
                {
                    if(encodeDecoder.IsDirty(_components[i], _data[i].DataDictionary))
                        return true;
                }
            }

            return false;
        }

        public DataList Encode(GameObject gameObject = null)
        {
            if(gameObject != null)
                if (gameObject != this.gameObject)
                    return _data;

            if (IsManual && gameObject == null)
                return _data;

            _data ??= new DataList();
            for(int i = 0; i < _components.Count;i++)
            {
                string componentType = _components[i].GetType().FullName;
                if (encodeDecoders.TryGetValue(componentType, out var encodeDecoder))
                {
                    if(_data.Count > i)
                        _data[i] = encodeDecoder.Encode(_components[i]);
                    else
                    {
                        _data.Add(encodeDecoder.Encode(_components[i]));
                    }
                }
            }
            #if VRC_ENABLE_PLAYER_PERSISTENCE
            ClientSimMain.GetInstance().GetEventDispatcher().SendEvent(new ClientSimOnPlayerObjectUpdatedEvent{Data = this});
            #endif
            return _data;
        }
        
        public void PostEncode(GameObject gameObject = null)
        {
            if(this == null)
                return;
            
            if(gameObject != null)
                if(gameObject != this.gameObject)
                    return;
            
            if(IsManual && gameObject == null)
                return;
            
            for (int i = 0; i < _components.Count; i++)
            {
                string componentType = _components[i].GetType().FullName;
                if (encodeDecoders.TryGetValue(componentType, out var encodeDecoder))
                {
                    encodeDecoder.PostEncode(_components[i], _data[i].DataDictionary);
                }
            }
        }
        
        public void PreEncode(GameObject gameObject = null)
        {
            if(this == null)
                return;
            
            if(gameObject != null)
                if(gameObject != this.gameObject)
                    return;
            
            if(IsManual && gameObject == null)
                return;
            
            for (int i = 0; i < _components.Count; i++)
            {
                string componentType = _components[i].GetType().FullName;
                if (encodeDecoders.TryGetValue(componentType, out var encodeDecoder))
                {
                    encodeDecoder.PreEncode(_components[i]);
                }
            }
        }

        public void Decode(DataList data)
        {
            _data = data;
            for(int i = 0; i < _components.Count;i++)
            {
                string componentType = _components[i].GetType().FullName;
                if (encodeDecoders.TryGetValue(componentType, out var encodeDecoder))
                {
                    if(data.Count > i && !data[i].IsNull)
                        encodeDecoder.Decode(_components[i], data[i].DataDictionary);
                }
            }
            
#if VRC_ENABLE_PLAYER_PERSISTENCE
            ClientSimMain.GetInstance().GetEventDispatcher().SendEvent(new ClientSimOnPlayerObjectUpdatedEvent{Data = this});
#endif
        }
        
        public int GetNetworkComponentCount()
        {
            return _components.Count;
        }
        
        public DataList GetData()
        {
            return _data;
        }
        
        public List<MonoBehaviour> GetNetworkComponents()
        {
            return _components;
        }
    }
    
}