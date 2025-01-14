using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerCameraProvider
    {
        Camera GetCamera();
        Camera GetCameraForObject(GameObject obj);
    }
}