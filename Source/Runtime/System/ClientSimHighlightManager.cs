
using System.Collections.Generic;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for highlighting objects.
    /// </summary>
    [AddComponentMenu("")]
    public class ClientSimHighlightManager : ClientSimBehaviour, IClientSimHighlightManager
    {
        [SerializeField]
        private Mesh cubeMesh;
        [SerializeField]
        private Mesh capsuleMesh;
        [SerializeField]
        private Mesh sphereMesh;
        
        [SerializeField]
        private GameObject proxyHighlightPrefab;
        
        // TODO wrap highlightFX to make it easier to test/replace.
        private HighlightsFX _highlightsFX;
        
        private readonly Dictionary<GameObject, ClientSimHighlightProxy> _objectRenderProxies =
            new Dictionary<GameObject, ClientSimHighlightProxy>();

        private readonly Queue<ClientSimHighlightProxy> _proxyMeshQueue = new Queue<ClientSimHighlightProxy>();
        
        public void Initialize(Camera playerCamera)
        {
            _highlightsFX = playerCamera.gameObject.AddComponent<HighlightsFX>();
        }

        public void EnableObjectHighlight(GameObject obj)
        {
            List<Renderer> renderers = GatherRenderers(obj, false);
            if (renderers.Count == 0)
            {
                Renderer rend = GetProxyHighlight(obj);
                if (rend != null)
                {
                    renderers.Add(rend);
                }
            }
            
            foreach (var rend in renderers)
            {
                EnableObjectHighlight(rend, true);
            }
        }

        public void DisableObjectHighlight(GameObject obj)
        {
            if (_objectRenderProxies.TryGetValue(obj, out ClientSimHighlightProxy proxy))
            {
                _objectRenderProxies.Remove(obj);
                EnableObjectHighlight(proxy.Renderer, false);

                _proxyMeshQueue.Enqueue(proxy);
                proxy.DisableProxy();
            }
            
            List<Renderer> renderers = GatherRenderers(obj, true);
            foreach (var rend in renderers)
            {
                EnableObjectHighlight(rend, false);
            }
        }

        public void EnableObjectHighlight(Renderer rend, bool isEnabled)
        {
            _highlightsFX.EnableOutline(rend, isEnabled);
        }

        private List<Renderer> GatherRenderers(GameObject obj, bool findDisabled)
        {
            if(obj == null)
            {
                return new List<Renderer>();
            }
            List<Renderer> results = new List<Renderer>();
            foreach (var rend in obj.GetComponentsInChildren<Renderer>(findDisabled))
            {
                if (!rend.enabled || rend.isPartOfStaticBatch)
                {
                    continue;
                }
                MeshFilter filter = rend.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }
                
                results.Add(rend);
            }

            return results;
        }

        private Renderer GetProxyHighlight(GameObject obj)
        {
            ClientSimHighlightProxy proxy = GetUnusedProxy();
            
            Collider objCollider = obj.GetComponent<Collider>();
            _objectRenderProxies.Add(obj, proxy);
            proxy.EnableProxy(obj.transform, objCollider);
            
            return proxy.Renderer;
        }

        private ClientSimHighlightProxy GetUnusedProxy()
        {
            if (_proxyMeshQueue.Count == 0)
            {
                GameObject tooltipObj = Instantiate(proxyHighlightPrefab, transform);
                ClientSimHighlightProxy tooltip = tooltipObj.GetComponent<ClientSimHighlightProxy>();
                tooltip.Initialize(cubeMesh, capsuleMesh, sphereMesh);
                return tooltip;
            }

            return _proxyMeshQueue.Dequeue();
        }
    }
}
