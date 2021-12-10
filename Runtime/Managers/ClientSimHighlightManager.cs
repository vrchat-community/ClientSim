using System;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimHighlightManager : ClientSimBehaviour
    {
        private static Material highlightMaterial_;

        private Renderer interactHighlight_;
        private Collider target_;
        private Transform cameraTransform_;
        
        private GameObject toolTip_;
        private Transform toolTipTransform_;
        private TextMesh toolTipText_;

        private static Material GetHighlightMaterial()
        {
            if (highlightMaterial_ == null)
            {
                highlightMaterial_ = new Material(Shader.Find("UI/Default"));
                highlightMaterial_.SetColor("_Color", new Color32(0, 255, 255, 50));
            }
            return highlightMaterial_;
        }

        public static ClientSimHighlightManager CreateInteractHelper(Transform parent, Transform cameraTransform)
        {
            GameObject interact = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interact.name = "Highlight";
            DestroyImmediate(interact.GetComponent<BoxCollider>());
            ClientSimHighlightManager highlight = interact.AddComponent<ClientSimHighlightManager>();
            highlight.interactHighlight_ = interact.GetComponent<Renderer>();
            highlight.interactHighlight_.sharedMaterial = GetHighlightMaterial();
            highlight.transform.parent = parent;
            highlight.cameraTransform_ = cameraTransform;
            
            // Tool tip text
            highlight.toolTip_ = new GameObject("ToolTip");
            highlight.toolTipTransform_ = highlight.toolTip_.transform;
            highlight.toolTipTransform_.parent = parent;
            GameObject child = new GameObject("ToolTipText");
            child.transform.parent = highlight.toolTip_.transform;
            child.transform.localRotation = Quaternion.Euler(0, 180, 0);
            child.transform.localPosition = new Vector3(0, .05f, 0);

            highlight.toolTipText_ = child.AddComponent<TextMesh>();
            highlight.toolTipText_.anchor = TextAnchor.LowerCenter;
            highlight.toolTipText_.characterSize = 0.01f;
            highlight.toolTipText_.fontSize = 100;
            highlight.toolTipText_.text = "";
            
            return highlight;
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeInHierarchy != active)
            {
                gameObject.SetActive(active);
                toolTip_.SetActive(active);
            }
        }

        public void HighlightCollider(Collider collider, string toolTip)
        {
            target_ = collider;
            toolTipText_.text = toolTip;
            UpdateInteractLocation(target_);
        }

        private void UpdateInteractLocation(Collider collider)
        {
            GetColliderSize(collider, out Vector3 size, out Vector3 offset);
            Transform colliderTransform = collider.transform;
            Quaternion rotation = colliderTransform.rotation;
            Vector3 position = colliderTransform.position + rotation * offset;
            
            transform.rotation = rotation;
            transform.position = position;
            transform.localScale = size * 1.01f;
            
            Vector3 up = cameraTransform_.up;
            toolTipTransform_.position = position;
            toolTipTransform_.up = up;
            toolTipTransform_.LookAt(cameraTransform_, up);
        }

        private void GetColliderSize(Collider collider, out Vector3 highlightSize, out Vector3 highlightOffset)
        {
            Vector3 scale = collider.transform.lossyScale;
            highlightSize = Vector3.one;
            Vector3 center = Vector3.zero;
            
            if (collider is BoxCollider boxCollider)
            {
                Vector3 size = boxCollider.size;
                center = boxCollider.center;
                highlightSize = new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
            }
            if (collider is SphereCollider sphereCollider)
            {
                float max = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
                highlightSize = max * sphereCollider.radius * 2 * Vector3.one;
                center = sphereCollider.center;
            }
            if (collider is CapsuleCollider capsuleCollider)
            {
                int direction = capsuleCollider.direction;
                float max = Mathf.Max(scale[(direction + 1) % 3], scale[(direction + 2) % 3]);
                Vector3 size = capsuleCollider.radius * 2 * max * Vector3.one;
                size[direction] = Mathf.Max(capsuleCollider.height * scale[direction], size[direction]);
                highlightSize = size;
                center = capsuleCollider.center;
            }
            if (collider is MeshCollider meshCollider)
            {
                Vector3 size = meshCollider.sharedMesh.bounds.size;
                highlightSize = new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
            }

            highlightOffset = new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
        }

        private void OnWillRenderObject()
        {
            if (interactHighlight_.enabled && target_ != null)
            {
                UpdateInteractLocation(target_);
            }
        }
    }
}
