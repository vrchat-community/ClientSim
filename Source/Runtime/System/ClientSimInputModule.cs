using UnityEngine;
using UnityEngine.EventSystems;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System responsible for filtering out UI elements that cannot be interacted with.
    /// </summary>
    [AddComponentMenu("")]
    public class ClientSimInputModule : StandaloneInputModule
    {
        private IClientSimInteractiveLayerProvider _interactiveLayerProvider;
        private ClientSimBaseInput _baseInput;
        
        protected override void Awake()
        {
            base.Awake();
            this.PreventComponentFromSaving();
        }

        private void DisableOtherEventSystems()
        {
            // Go through and Disable all other event systems in the scene.
            EventSystem thisEventSystem = GetComponent<EventSystem>();
            EventSystem[] systems = FindObjectsOfType<EventSystem>();
            foreach (EventSystem system in systems)
            {
                if (system != thisEventSystem)
                {
                    system.enabled = false;
                }
            }
        }

        public void Initialize(IClientSimInteractiveLayerProvider interactiveLayerProvider)
        {
            _interactiveLayerProvider = interactiveLayerProvider;
        }

        protected override void Start()
        {
            DisableOtherEventSystems();
            
            // TODO check settings and disable self if player is not spawned to allow normal ui raycasting.
            m_InputOverride = _baseInput = GetComponent<ClientSimBaseInput>();
            
            base.Start();
        }

        
        // Force processing of mouse events even if the cursors is locked.
        public override void Process()
        {
            CursorLockMode currentLockState = Cursor.lockState;

            Cursor.lockState = CursorLockMode.None;
            
            base.Process();

            Cursor.lockState = currentLockState;
        }

        // Prevent clicking on menus that are not currently interactable.
        protected override MouseState GetMousePointerEventData(int id)
        {
            var pointerEventData = base.GetMousePointerEventData(id);
            var leftEventData = pointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var pointerRaycast = leftEventData.buttonData.pointerCurrentRaycast;
            
            // Check if this raycast result is valid. If not, reset the data.
            if (!ShouldUseRaycastResult(pointerRaycast))
            {
                leftEventData.buttonData.pointerCurrentRaycast = new RaycastResult();
            }

            return pointerEventData;
        }
        
        private bool ShouldUseRaycastResult(RaycastResult result)
        {
            GameObject hitObj = result.gameObject;
            if (hitObj == null)
            {
                return false;
            }
            
            // If raycaster did not hit a UI shape, this means some other collider is in front.
            if (!_baseInput.HitUIShape())
            {
                return false;
            }
            
            // If there is no UI shape on this object or in its parents, ignore it.
            VRC_UiShape shape = hitObj.GetComponentInParent<VRC_UiShape>();
            if (shape == null)
            {
                return false;
            }

            GameObject shapeObj = shape.gameObject;
            // Ignore UI elements not on a currently Interactive layer
            if (((1 << shapeObj.layer) & _interactiveLayerProvider.GetInteractiveLayers()) == 0)
            {
                return false;
            }

            Vector3 position = result.worldPosition;
            // If ray is within any collider on the UIShape, it is valid.
            foreach (var shapeCollider in shapeObj.GetComponents<Collider>())
            {
                // If the closest point is itself, then it is in the collider.
                if (Vector3.Distance(shapeCollider.ClosestPoint(position), position) < 0.01f)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
