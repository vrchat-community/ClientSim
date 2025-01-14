using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Interfaces
{
    public interface IClientSimNetworkSerializer
    {
        DataList Encode(GameObject gameObject = null);
        void Decode(DataList data);
        DataList GetData();
        bool IsDirty(GameObject gameObject = null);
        int GetPlayerId();
        void SetNetworkComponents();
        int GetNetworkComponentCount();
        List<MonoBehaviour> GetNetworkComponents();
        void PostEncode(GameObject gameObject = null);
        void PreEncode(GameObject gameObject = null);
    }
}