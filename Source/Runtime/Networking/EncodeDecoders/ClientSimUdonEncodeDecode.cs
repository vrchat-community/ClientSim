using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace VRC.SDK3.ClientSim.EncodeDecoders
{
    public class ClientSimUdonEncodeDecode : IClientSimEncodeDecoder
    {
        public void PreEncode(MonoBehaviour component)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            udonBehaviour.OnPreSerialization();
        }
        
        public DataDictionary Encode(MonoBehaviour component)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            DataDictionary data = new DataDictionary();

            IEnumerable<IUdonSyncMetadata> SyncMetadatas = udonBehaviour.SyncMetadataTable.GetAllSyncMetadata();

            foreach (IUdonSyncMetadata syncMetadata in SyncMetadatas)
            {
                object obj = udonBehaviour.GetProgramVariable(syncMetadata.Name);
                data.Add(syncMetadata.Name, GetJTokenFromObject(obj));
            }
            
            return data;
        }
        
        public void PostEncode(MonoBehaviour component, DataDictionary data)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            udonBehaviour.OnPostSerialization(new SerializationResult(true,data.Count*4));
        }
        
        public bool IsManualSynced(MonoBehaviour component)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            return udonBehaviour.SyncMethod == Networking.SyncType.Manual;
        }

        public bool IsDirty(MonoBehaviour component, DataDictionary data)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            
            IUdonSyncMetadataTable syncMetadataTable = udonBehaviour.SyncMetadataTable;
            if(syncMetadataTable == null)
                return false;
            
            IEnumerable<IUdonSyncMetadata> SyncMetadatas = syncMetadataTable.GetAllSyncMetadata();

            foreach (IUdonSyncMetadata syncMetadata in SyncMetadatas)
            {
                if (data.ContainsKey(syncMetadata.Name))
                {
                    if (!IsDataSame(data[syncMetadata.Name],GetJTokenFromObject(udonBehaviour.GetProgramVariable(syncMetadata.Name))))
                        return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private static DataToken GetJTokenFromObject(object obj)
        {
            if(obj == null)
                return new DataToken();
            
            if (obj.GetType().IsArray)
            {
                DataList jArray = new DataList();
                foreach (object arrayObj in (Array)obj)
                {
                    jArray.Add(GetJTokenFromObject(arrayObj));
                }
                return jArray;
            }
            
            return obj switch
            {
                bool bo => new DataToken(bo),
                char c => new DataToken(c),
                byte b => new DataToken(b),
                sbyte sb => new DataToken(sb),
                short s => new DataToken(s),
                ushort us => new DataToken(us),
                int i => new DataToken(i),
                uint u => new DataToken(u),
                long l => new DataToken(l),
                ulong ul => new DataToken(ul),
                float f => new DataToken(f),
                double d => new DataToken(d),
                Vector2 vector2 => vector2.GetJTokenFromVector2(),
                Vector3 vector3 => vector3.GetJTokenFromVector3(),
                Vector4 vector4 => vector4.GetJTokenFromVector4(),
                Quaternion quaternion => quaternion.GetJTokenFromQuaternion(),
                string str => new DataToken(str),
                VRCUrl url => new DataToken(url.Get()),
                Color color => color.GetJTokenFromColor(),
                Color32 color32 => color32.GetJTokenFromColor32(),
                _ => new DataToken()
            };
        }
        
        public object Get(Type t, DataToken dataToken)
        {
            if (t.IsArray)
            {
                Array array = Array.CreateInstance(t.GetElementType(), dataToken.DataList.Count);
                for (int i = 0; i < dataToken.DataList.Count; i++)
                {
                    array.SetValue(Get(t.GetElementType(), dataToken.DataList[i]), i);
                }
                return array;
            }
            
            return t switch
            {
                Type boolType when boolType == typeof(bool) => dataToken.Boolean,
                Type charType when charType == typeof(char) => dataToken.String[0],
                Type byteType when byteType == typeof(byte) => (byte)dataToken.Double,
                Type sbyteType when sbyteType == typeof(sbyte) => (sbyte)dataToken.Double,
                Type shortType when shortType == typeof(short) => (short)dataToken.Double,
                Type ushortType when ushortType == typeof(ushort) => (ushort)dataToken.Double,
                Type intType when intType == typeof(int) => (int)dataToken.Double,
                Type uintType when uintType == typeof(uint) => (uint)dataToken.Double,
                Type longType when longType == typeof(long) => (long)dataToken.Double,
                Type ulongType when ulongType == typeof(ulong) => (ulong)dataToken.Double,
                Type floatType when floatType == typeof(float) => (float)dataToken.Double,
                Type doubleType when doubleType == typeof(double) => dataToken.Double,
                Type vector2Type when vector2Type == typeof(Vector2) => dataToken.GetVector2(),
                Type vector3Type when vector3Type == typeof(Vector3) => dataToken.GetVector3(),
                Type vector4Type when vector4Type == typeof(Vector4) => dataToken.GetVector4(),
                Type quaternionType when quaternionType == typeof(Quaternion) => dataToken.GetQuaternion(),
                Type stringType when stringType == typeof(string) => dataToken.String,
                Type urlType when urlType == typeof(VRCUrl) => new VRCUrl(dataToken.String),
                Type colorType when colorType == typeof(Color) => dataToken.GetColor(),
                Type color32Type when color32Type == typeof(Color32) => dataToken.GetColor32(),
                _ => null
            };
            
        }

        public void Decode(MonoBehaviour component, DataDictionary data)
        {
            UdonBehaviour udonBehaviour = component as UdonBehaviour;
            
            IEnumerable<IUdonSyncMetadata> SyncMetadatas = udonBehaviour.SyncMetadataTable.GetAllSyncMetadata();

            foreach (IUdonSyncMetadata syncMetadata in SyncMetadatas)
            {
                if (data.ContainsKey(syncMetadata.Name))
                {
                    Type type = udonBehaviour.GetProgramVariableType(syncMetadata.Name);
                    udonBehaviour.SetProgramVariable(syncMetadata.Name,
                        Get(type,data[syncMetadata.Name]));
                }
            }

            udonBehaviour.OnDeserialization(new DeserializationResult(0, 0, true));
            
        }

        private bool IsDataSame(DataToken a, DataToken b)
        {
            if(a.TokenType != b.TokenType)
                return false;

            switch (a.TokenType)
            {
                case TokenType.DataList when a.DataList.Count != b.DataList.Count:
                    return false;
                case TokenType.DataList:
                {
                    for (int i = 0; i < a.DataList.Count; i++)
                    {
                        if (!IsDataSame(a.DataList[i], b.DataList[i]))
                            return false;
                    }

                    break;
                }
                case TokenType.DataDictionary when a.DataDictionary.Count != b.DataDictionary.Count:
                    return false;
                case TokenType.DataDictionary:
                {
                    foreach (KeyValuePair<DataToken, DataToken> keyValuePair in a.DataDictionary)
                    {
                        if (!b.DataDictionary.ContainsKey(keyValuePair.Key))
                            return false;

                        if (!IsDataSame(keyValuePair.Value, b.DataDictionary[keyValuePair.Key]))
                            return false;
                    }

                    break;
                }
                default:
                    return a.Equals(b);
            }
            
            return true;
        }
    }
}