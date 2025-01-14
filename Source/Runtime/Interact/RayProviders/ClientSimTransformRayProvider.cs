using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimTransformRayProvider : IClientSimRayProvider
    {
        private readonly Transform _rayTransform;

        public ClientSimTransformRayProvider(Transform rayTransform)
        {
            _rayTransform = rayTransform;
        }
        
        public Ray GetRay()
        {
            return new Ray(_rayTransform.position, _rayTransform.forward);
        }
    }
}