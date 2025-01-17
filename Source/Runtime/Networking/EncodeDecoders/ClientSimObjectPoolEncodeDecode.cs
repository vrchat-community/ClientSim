using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Components;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.EncodeDecoders
{
    public class ClientSimObjectPoolEncodeDecode : IClientSimEncodeDecoder
    {
        public DataDictionary Encode(MonoBehaviour component)
        {
            VRCObjectPool objectPool = (VRCObjectPool) component;
            
            DataDictionary data = new DataDictionary();
            
            data["Length"] = objectPool.Pool.Length;
            DataList values = new DataList();

            for (int i = 0; i < objectPool.Pool.Length; i++)
            {   
                values.Add( objectPool.Pool[i].activeSelf);
            }
            
            data["Values"] = values;
            
            return data;
        }
         
        public void PreEncode(MonoBehaviour component)
        {
        }
        
        public void PostEncode(MonoBehaviour component, DataDictionary data)
        {
        }
       

        public void Decode(MonoBehaviour component, DataDictionary data)
        {
            VRCObjectPool objectPool = (VRCObjectPool) component;
            
            if (data.TryGetValue("Length", out DataToken lengthToken))
            {
                int length = (int)lengthToken.Double;
                
                if (data.TryGetValue("Values", out DataToken valuesToken))
                {
                    DataList values = valuesToken.DataList;
                    for (int i = 0; i < length; i++)
                    {
                        objectPool.Pool[i].SetActive(values[i].Boolean);
                    }
                }
            }
        }
        
        public bool IsManualSynced(MonoBehaviour component)
        {
            return false;
        }

        public bool IsDirty(MonoBehaviour component, DataDictionary data)
        {
            VRCObjectPool objectPool = (VRCObjectPool) component;
            
            if (data.TryGetValue("Length", out DataToken lengthToken))
            {
                int length = (int)lengthToken.Double;
                
                if (data.TryGetValue("Values", out DataToken valuesToken))
                {
                    DataList values = valuesToken.DataList;
                    for (int i = 0; i < length; i++)
                    {
                        if (objectPool.Pool[i].activeSelf != values[i].Boolean)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}