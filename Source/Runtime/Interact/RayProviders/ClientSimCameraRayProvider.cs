using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimCameraRayProvider : IClientSimRayProvider
    {
        private readonly Camera _playerCamera;
        private readonly IClientSimMousePositionProvider _mousePositionProvider;

        public ClientSimCameraRayProvider(
            IClientSimPlayerCameraProvider cameraProvider,
            IClientSimMousePositionProvider mousePositionProvider)
        {
            _playerCamera = cameraProvider.GetCamera();
            _mousePositionProvider = mousePositionProvider;
        }
        
        public Ray GetRay()
        {
            return _playerCamera.ScreenPointToRay(_mousePositionProvider.GetMousePosition());
        }
    }
}