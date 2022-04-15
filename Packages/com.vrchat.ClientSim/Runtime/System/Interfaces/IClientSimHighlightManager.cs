using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimHighlightManager
    {
        void EnableObjectHighlight(GameObject obj);
        void DisableObjectHighlight(GameObject obj);
    }
}