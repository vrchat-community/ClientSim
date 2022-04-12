using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimBlacklistManager
    {
        void AddObjectAndChildrenToBlackList(GameObject obj);
    }
}