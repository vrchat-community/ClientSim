
using BestHTTP.JSON;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Interfaces
{
    public interface IClientSimEncodeDecoder
    {
         DataDictionary Encode(MonoBehaviour component);
         
         void Decode(MonoBehaviour component, DataDictionary data);
         
         public bool IsManualSynced(MonoBehaviour component);
         
         bool IsDirty(MonoBehaviour component, DataDictionary data);
         
         void PostEncode(MonoBehaviour component, DataDictionary data);
         
         void PreEncode(MonoBehaviour component);
    }
}