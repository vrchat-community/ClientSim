using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements.EncodeDecodeEditors;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.Udon;

namespace VRC.SDK3.ClientSim.Editor.VisualElements
{
    #if VRC_ENABLE_PLAYER_PERSISTENCE
    public class ClientSimNetworkHolderInstanceElement : VisualElement
    {
        private GameObject _gameObject;
        private string _componentName;
        private string _componentType;
        private MonoBehaviour _component;
        
        private Foldout foldout;

        private IClientSimNetworkSerializer networkView;
        
        internal static Dictionary<string,IClientSimEncodeDecoderEditor> encodeDecoders = new Dictionary<string,IClientSimEncodeDecoderEditor>
        {
            { typeof(VRCObjectSync).FullName, new ClientSimObjectSyncEncodeDecodeEditor() },
            { typeof(UdonBehaviour).FullName, new ClientSimUdonEncodeDecodeEditor() },
            { typeof(VRCObjectPool).FullName, new ClientSimObjectPoolEncodeDecodeEditor()}
        };
        
        private VisualElement _dataElements;


        public ClientSimNetworkHolderInstanceElement()
        {
            foldout = new Foldout();
            Button selectButton = new Button(() =>
            {
                Selection.activeObject = _gameObject;
            });
            
            selectButton.text = ">";
            
            foldout.Q<Toggle>().Add(selectButton);
            this.Add(foldout);
        }
        
        private void GetDataFromComponent(int index)
        {
            _component = networkView.GetNetworkComponents()[index];
            
            _gameObject = _component.gameObject;
            _componentType = _component.GetType().FullName;
            
            if (_component is UdonBehaviour)
            {
                _componentName = ((UdonBehaviour) _component).programSource.name;
            }
            else
            {
                _componentName = _component.GetType().Name;
            }
        }
        
        private void GenerateDataElements(int index)
        {
            if (encodeDecoders.TryGetValue(_componentType, out var encodeDecoder))
            {
                _dataElements = encodeDecoder.GenerateFields(_component, networkView.GetData()[index].DataDictionary);
            }
        }

        public void UpdateData(ClientSimPlayerObjectWindow.PlayerObjectData data)
        {
            if (data.NetworkView != networkView)
            {
                foldout.Clear();
                networkView = data.NetworkView;
                GetDataFromComponent(data.index);
                
                this.name = $"{_componentName}({_gameObject.name})";
                foldout.text = $"{_componentName}({_gameObject.name})";
                GenerateDataElements(data.index);
                foldout.Add(_dataElements);
            }
            
            if (encodeDecoders.TryGetValue(_componentType, out var encodeDecoder))
            {
                if (_dataElements == null)
                { 
                    GenerateDataElements(data.index);
                    foldout.Clear();
                    foldout.Add(_dataElements);
                }
                else
                {
                    encodeDecoder.UpdateFields(_component, _dataElements, networkView.GetData()[data.index].DataDictionary);
                }
            }
            
            style.display = networkView.GetData().Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
#endif
}