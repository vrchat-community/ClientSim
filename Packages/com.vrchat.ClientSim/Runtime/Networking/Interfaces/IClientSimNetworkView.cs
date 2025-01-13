using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Interfaces
{
    public interface IClientSimNetworkView
    {
        DataList Encode(GameObject gameObject = null);

        void Decode(DataList data);
    }
}