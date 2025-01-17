using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements.Fields;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI.GraphView;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.EncodeDecodeEditors
{
    public class ClientSimUdonEncodeDecodeEditor : IClientSimEncodeDecoderEditor
    {
        public VisualElement GenerateFields(MonoBehaviour component,DataDictionary data)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            IEnumerable<IUdonSyncMetadata> SyncMetadatas = udonBehaviour.SyncMetadataTable.GetAllSyncMetadata();
            VisualElement dataElement = new VisualElement();
            foreach (IUdonSyncMetadata syncMetadata in SyncMetadatas)
            {
                if (data.ContainsKey(syncMetadata.Name))
                {
                    VisualElement field = FieldFactory.GenerateField(syncMetadata.Name, GetCorrectTypeFromDataToken(data[syncMetadata.Name]));
                    field.name = syncMetadata.Name;
                    dataElement.Add(field);
                }
            }
            return dataElement;
        }

        public void UpdateFields(MonoBehaviour component, VisualElement dataElement, DataDictionary data)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            IEnumerable<IUdonSyncMetadata> SyncMetadatas = udonBehaviour.SyncMetadataTable.GetAllSyncMetadata();
            int index = 0;
            foreach (IUdonSyncMetadata syncMetadata in SyncMetadatas)
            {
                if (data.ContainsKey(syncMetadata.Name))
                {
                    FieldFactory.UpdateField(dataElement[index],  GetCorrectTypeFromDataToken(data[syncMetadata.Name]));
                    
                    index++;
                }
            }
        }
        
        private object GetCorrectTypeFromDataToken(DataToken token)
        {
            switch (token.TokenType)
            {
                case TokenType.Boolean:
                    return token.Boolean;
                case TokenType.SByte:
                    return token.SByte;
                case TokenType.Byte:
                    return token.Byte;
                case TokenType.Short:
                    return token.Short;
                case TokenType.UShort:
                    return token.UShort;
                case TokenType.Int:
                    return token.Int;
                case TokenType.UInt:
                    return token.UInt;
                case TokenType.Long:
                    return token.Long;
                case TokenType.ULong:
                    return token.ULong;
                case TokenType.Float:
                    return token.Float;
                case TokenType.Double:
                    return token.Double;
                case TokenType.String:
                    return token.String;
                case TokenType.Reference:
                    return token.Reference;
                case TokenType.DataList:
                    DataList dataList = token.DataList;
                    object[] list = new object[dataList.Count];

                    for(int i =0; i < dataList.Count; i++)
                    {
                        list[i] = GetCorrectTypeFromDataToken(dataList[i]);
                    }
                    return list;
                case TokenType.DataDictionary:
                    DataDictionary dataDictionary = token.DataDictionary;
                    if(!dataDictionary.ContainsKey("T")) return null;
                    
                    string type = dataDictionary["T"].String;

                    switch (type)
                    {
                        case "V2":
                            return token.GetVector2();
                        case "V3":
                            return token.GetVector3();
                        case "V4":
                            return token.GetVector4();
                        case "Q":
                            return token.GetQuaternion();
                        case "C":
                            return token.GetColor();
                        case "C32":
                            return token.GetColor32();
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }
    }
}