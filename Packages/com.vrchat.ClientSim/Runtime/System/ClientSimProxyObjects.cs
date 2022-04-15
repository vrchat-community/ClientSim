using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Manager class for holding any proxy object. Currently only the camera proxy provided for the combat system.
    /// </summary>
    [AddComponentMenu("")]
    public class ClientSimProxyObjects : ClientSimBehaviour, IClientSimProxyObjectProvider
    {
        [SerializeField] 
        private Transform cameraProxy;

        public Transform CameraProxy() => cameraProxy;

        public void DestroyProxy()
        {
            Destroy(gameObject);
        }
    }
}