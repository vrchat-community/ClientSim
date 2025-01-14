using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements;

namespace VRC.SDK3.ClientSim.Editor
{
    #if VRC_ENABLE_PLAYER_PERSISTENCE
    [UnityEditor.CustomEditor(typeof(ClientSimNetworkIdHolder))]
    public class ClientSimNetworkIdHolderEditor : UnityEditor.Editor
    {
        private VisualElement _List;
        private IClientSimEventDispatcher _eventDispatcher;

        public void OnEnable()
        {
            _eventDispatcher = ClientSimMain.GetInstance().GetEventDispatcher();
            _eventDispatcher.Subscribe<ClientSimOnPlayerObjectUpdatedEvent>(OnPlayerObjectUpdated);
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            ClientSimNetworkIdHolder networkIdHolder = (ClientSimNetworkIdHolder) target;
            
            if (networkIdHolder._components != null)
            {
                root.Add(new Label("Network Id: " + networkIdHolder._networkId.GetNetworkId()));
                root.Add(new Label("Network Components"));
                _List = new VisualElement();
                for (int i = 0; i < networkIdHolder._components.Count; i++)
                {
                    if (ClientSimNetworkHolderInstanceElement.encodeDecoders.TryGetValue(networkIdHolder._components[i].GetType().FullName, out var encodeDecoder))
                    {
                        MonoBehaviour component = networkIdHolder._components[i];
                        if(networkIdHolder._data.Count > i)
                            _List.Add(encodeDecoder.GenerateFields(component,networkIdHolder._data[i].DataDictionary));
                    }
                }
                root.Add(_List);
            }
            
            return root;
        }
        
        private void OnPlayerObjectUpdated(ClientSimOnPlayerObjectUpdatedEvent e)
        {
            ClientSimNetworkIdHolder networkIdHolder = (ClientSimNetworkIdHolder) target;
            if ((ClientSimNetworkIdHolder)e.Data != networkIdHolder) return;
            
            if (networkIdHolder._components != null)
            {
                for (int i = 0; i < networkIdHolder._components.Count; i++)
                {
                    if (ClientSimNetworkHolderInstanceElement.encodeDecoders.TryGetValue(networkIdHolder._components[i].GetType().FullName, out var encodeDecoder))
                    {
                        MonoBehaviour component = networkIdHolder._components[i];
                        if(networkIdHolder._data.Count > i)
                            encodeDecoder.UpdateFields(component,_List[i],networkIdHolder._data[i].DataDictionary);
                    }
                }
            }
        }
        
        public void OnDisable()
        {
            _eventDispatcher?.Unsubscribe<ClientSimOnPlayerObjectUpdatedEvent>(OnPlayerObjectUpdated);
        }
    }
#endif
}