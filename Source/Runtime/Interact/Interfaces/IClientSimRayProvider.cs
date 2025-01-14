using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Allows the Raycasting system to act the same for VR and Desktop.
    /// </summary>
    /// <remarks>
    /// Only need to provide a different initial ray and the rest is handled automatically. There are currently two different ray providers:
    /// ClientSimCameraRayProvider used for Desktop
    /// ClientSimTransformRayProvider used for VR
    /// </remarks>
    public interface IClientSimRayProvider
    {
        Ray GetRay();
    }
}