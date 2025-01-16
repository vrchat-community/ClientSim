using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimInteractiveLayerProvider
    {
        LayerMask GetInteractiveLayers();
    }
}