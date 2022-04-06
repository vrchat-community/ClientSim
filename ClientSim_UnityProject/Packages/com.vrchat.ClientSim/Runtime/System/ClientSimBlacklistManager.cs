
using UnityEngine;
using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for adding objects to the Udon blacklist.
    /// </summary>
    /// <remarks>Really just a wrapper for calling the blacklist method on UdonManager.</remarks>
    public class ClientSimBlacklistManager : IClientSimBlacklistManager
    {
        public void AddObjectAndChildrenToBlackList(GameObject obj)
        {
            AddObjectAndChildrenToBlackList(obj, UdonManager.Instance);
        }
        
        private void AddObjectAndChildrenToBlackList(GameObject obj, UdonManager udonManager)
        {
            udonManager.Blacklist(obj);

            Transform xform = obj.transform;
            for (int child = 0; child < xform.childCount; ++child)
            {
                AddObjectAndChildrenToBlackList(xform.GetChild(child).gameObject, udonManager);
            }
        }
    }
}