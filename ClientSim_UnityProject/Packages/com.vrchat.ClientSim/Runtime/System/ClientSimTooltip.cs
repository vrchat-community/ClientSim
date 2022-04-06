using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    public class ClientSimTooltip : ClientSimBehaviour
    {
        [SerializeField]
        private TextMesh tooltipText;

        public IClientSimInteractable Interactable { get; private set; }

        // Both Udon Interacts and Pickups display tooltips at the same location.
        // Moved here as a helper method to not repeat code. 
        // Tooltip is displayed at the top bound of the object's first collider. 
        public static Vector3 GetToolTipPosition(GameObject obj)
        {
            // VRChatBug: Tooltips always ignore the tooltipPlacement transform and instead place the tooltip at the top
            // of the first collider on the object.
            Collider interactCollider = obj.GetComponent<Collider>();

            if (interactCollider == null)
            {
                return obj.transform.position;
            }
            
            // Note due to colliders not updating their bounds property until after a physics update, to prevent extra
            // calls to Physics.SyncTransforms, the following code approximates the updated center and top extends
            // position based on the type of collider. 
            
            Transform transform = obj.transform;
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.lossyScale;
            
            Vector3 colliderCenter = Vector3.zero;
            if (interactCollider is BoxCollider boxCollider)
            {
                colliderCenter = boxCollider.center;
            }
            else if (interactCollider is SphereCollider sphereCollider)
            {
                colliderCenter = sphereCollider.center;
            }
            else if (interactCollider is CapsuleCollider capsuleCollider)
            {
                colliderCenter = capsuleCollider.center;
            }
            else if (interactCollider is MeshCollider meshCollider)
            {
                colliderCenter = meshCollider.sharedMesh.bounds.center;
            }

            position += rotation * Vector3.Scale(colliderCenter, scale);
            return position + new Vector3(0, interactCollider.bounds.extents.y, 0);
        }
        
        public void UpdateTooltip(Vector3 playerPos, Vector3 up)
        {
            Vector3 position = Interactable.GetInteractTextPlacement();
            
            // Rotate to look towards the player while keeping the proper up direction.
            // VRChatBug: Build 1160 has this broken again so that rotating the player through stations does not properly rotate tooltips.
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(playerPos - position, up), up);
            transform.SetPositionAndRotation(position, rotation);
        }
        
        public void EnableTooltip(IClientSimInteractable interactable)
        {
            gameObject.SetActive(true);
            Interactable = interactable;
            tooltipText.text = Interactable.GetInteractText();
        }

        public void DisableTooltip()
        {
            gameObject.SetActive(false);
            Interactable = null;
            tooltipText.text = "";
            
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}