using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimStackedCamera : ScriptableObject
    {
        [SerializeField] private string cameraName = "Generic Stacked Camera";
        [SerializeField] private LayerMask renderLayer;
        [SerializeField] private bool useOcclusionCulling = true;
        public string CameraName => cameraName;
        public LayerMask RenderLayer => renderLayer;
        public bool UseOcclusionCulling => useOcclusionCulling;
    }
}