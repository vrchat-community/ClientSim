
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    // This class is used for when an object has no usable mesh renderers to highlight. This will copy the collider on
    // the interact instead of any mesh on or under the interact.
    // An object will not have usable colliders if:
    // - There are no mesh renderers on or as a child of the object.
    // - The mesh renderers do not have a mesh
    // - The mesh renderers are part of a combined or static batched mesh.
    [AddComponentMenu("")]
    public class ClientSimHighlightProxy : ClientSimBehaviour
    {
        public Renderer Renderer { get; private set; }
        
        private Mesh _cubeMesh;
        private Mesh _capsuleMesh;
        private Mesh _sphereMesh;
        
        private MeshFilter _filter;
        private Transform _proxyObject;
        private Collider _proxyCollider;
        
        protected override void Awake()
        {
            base.Awake();

            Renderer = GetComponent<Renderer>();
            _filter = GetComponent<MeshFilter>();
        }

        public void Initialize(Mesh cubeMesh, Mesh capsuleMesh, Mesh sphereMesh)
        {
            _cubeMesh = cubeMesh;
            _capsuleMesh = capsuleMesh;
            _sphereMesh = sphereMesh;
        }

        private void OnWillRenderObject()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector3 position = _proxyObject.position;
            Quaternion rotation = _proxyObject.rotation;
            Vector3 scale = _proxyObject.lossyScale;
            
            if (_proxyCollider is BoxCollider boxCollider)
            {
                position += rotation * Vector3.Scale(boxCollider.center, scale);
                scale = Vector3.Scale(scale, boxCollider.size);
                _filter.sharedMesh = _cubeMesh;
            }
            else if (_proxyCollider is SphereCollider sphereCollider)
            {
                position += rotation * Vector3.Scale(sphereCollider.center, scale);
                
                // VRChatBug: Unity will always keep spheres round, but VRChat ignores this for the proxy rendering.
                
                // Sphere colliders are always round. Get the max component as the sphere size. 
                float sphereScale = sphereCollider.radius * 2 * Mathf.Max(scale.x, scale.y, scale.z);
                scale = sphereScale * Vector3.one;
                _filter.sharedMesh = _sphereMesh;
            }
            else if (_proxyCollider is CapsuleCollider capsuleCollider)
            {
                position += rotation * Vector3.Scale(capsuleCollider.center, scale);
                float height = capsuleCollider.height * 0.5f;
                float radius = capsuleCollider.radius * 2;
                float sphereScale = radius;
                
                switch (capsuleCollider.direction)
                {
                    case 0: // X
                        rotation *= Quaternion.Euler(0, 0, 90);
                        sphereScale = Mathf.Max(scale.y, scale.z) * radius;
                        height *= scale.x;
                        break;
                    case 1: // Y
                        // No need to rotate since mesh is already along y axis.
                        sphereScale = Mathf.Max(scale.x, scale.z) * radius;
                        height *= scale.y;
                        break;
                    case 2: // Z
                        rotation *= Quaternion.Euler(90, 0, 0);
                        sphereScale = Mathf.Max(scale.y, scale.x) * radius;
                        height *= scale.z;
                        break;
                }

                height = Mathf.Max(sphereScale * 0.5f, height);
                scale = new Vector3(sphereScale, height, sphereScale);
                _filter.sharedMesh = _capsuleMesh;
            }
            else if (_proxyCollider is MeshCollider meshCollider)
            {
                _filter.sharedMesh = meshCollider.sharedMesh;
            }

            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        public void EnableProxy(Transform obj, Collider proxyCollider)
        {
            gameObject.SetActive(true);
            _filter.sharedMesh = null;
            _proxyObject = obj;
            _proxyCollider = proxyCollider;

            UpdatePosition();
        }

        public void DisableProxy()
        {
            gameObject.SetActive(false);
            _filter.sharedMesh = null;
            _proxyObject = null;
            _proxyCollider = null;
        }
    }
}