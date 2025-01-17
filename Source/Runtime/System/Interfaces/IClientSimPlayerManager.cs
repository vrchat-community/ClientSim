
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerManager
    {
        VRCPlayerApi CreateNewPlayer(bool local, ClientSimPlayer player, string name = null);
        void RemovePlayer(VRCPlayerApi player);

        int GetMasterID();
        VRCPlayerApi GetMaster();
        VRCPlayerApi LocalPlayer();
        VRCPlayerApi GetPlayerByID(int playerID);
        int GetPlayerID(VRCPlayerApi player);
        bool IsMaster(VRCPlayerApi player);
        bool IsInstanceOwner(VRCPlayerApi player);
        bool IsInstanceOwner();
        bool IsLocalPlayerMaster();
        VRCPlayerApi GetOwner(GameObject obj);
        bool IsOwner(VRCPlayerApi player, GameObject obj);
    }
}