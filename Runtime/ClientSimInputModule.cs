using UnityEngine;
using UnityEngine.EventSystems;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(100)] // Delay update to allow getting data from the interact manager.
    public class ClientSimInputModule : StandaloneInputModule
    {
        private const int UILayer = 5;
        private const int UIMenuLayer = 12;
        private const int MirrorReflectionLayer = 18;

        private int interactiveLayersDefault_;
        private int interactiveLayersUI_;
        
        private CursorLockMode currentLockState_ = CursorLockMode.None;
        private ClientSimBaseInput baseInput_;
        private bool hitUIShape_;
        
        public static void DisableOtherInputModules()
        {
            EventSystem[] systems = FindObjectsOfType<EventSystem>();
            foreach (EventSystem system in systems)
            {
                system.enabled = false;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.PreventComponentFromSaving();

            // Only the UI and UIMenu layers are interactable when the UI is open.
            interactiveLayersUI_ = (1 << UILayer) | (1 << UIMenuLayer);
            // When the menu is not open, all layers but UI, UIMenu, and MirrorReflection layers are interactable.
            interactiveLayersDefault_ = ~(1 << MirrorReflectionLayer) & ~interactiveLayersUI_;
        }

        protected override void Start()
        {
            m_InputOverride = baseInput_ = GetComponent<ClientSimBaseInput>();
            eventSystem.sendNavigationEvents = false;
            
            base.Start();
        }
        
        public override void Process()
        {
            currentLockState_ = Cursor.lockState;

            Cursor.lockState = CursorLockMode.None;

            base.Process();

            Cursor.lockState = currentLockState_;
        }

        // Prevent clicking on menus on the ui layer if your menu is not open
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

            // Always clear this for the next frame.
            hitUIShape_ = false;

            return pointerEventData;
        }
        
        private bool ShouldUseRaycastResult(RaycastResult result)
        {
            GameObject hitObj = result.gameObject;
            if (hitObj == null)
            {
                return false;
            }
            
            // TODO fix the ClientSim menu so that it is not rendered as an overlay, but physical menu that renders on
            // top of everything and still reacts to input.
            if (hitObj.scene.path.Equals("DontDestroyOnLoad"))
            {
                return true;
            }
            
            // If raycaster did not hit a UI shape, this means some other collider is in front.
            if (!hitUIShape_)
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
            if (((1 << shapeObj.layer) & GetInteractiveLayers()) == 0)
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

        public void InteractRayHitUIShape()
        {
            hitUIShape_ = true;
        }

        public LayerMask GetInteractiveLayers()
        {
            return baseInput_.isMenuOpen ? interactiveLayersUI_ : interactiveLayersDefault_;
        }
    }

    class ClientSimBaseInput : BaseInput
    {
        // Start menu open to allow mouse movement before the client sim has been initialized.
        public bool isMenuOpen = true;
        private Vector2 lastMousePos_;
        private Vector2 mouseDelta_;

        public static Vector2 GetScreenCenter()
        {
            return new Vector2(Screen.width, Screen.height) * 0.5f;
        }

        public void SetMenuOpen(bool isOpen)
        {
            isMenuOpen = isOpen;
            InternalLockUpdate();
        }

        public override Vector2 mousePosition
        {
            get
            {
                if (isMenuOpen)
                {
                    return base.mousePosition;
                }
                return GetScreenCenter() - mouseDelta_;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.PreventComponentFromSaving();
        }

        private void Update()
        {
            Vector2 curPos = base.mousePosition;
            mouseDelta_ = curPos - lastMousePos_;
            lastMousePos_ = curPos;
            
            // Update mouse lock every frame to ensure it is always locked when needed.
            InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            // TODO check if tab is held to know if the mouse should be unlocked. Ensure that interacts also work properly.
            
            // If the menu is open, do not lock the mouse and show the cursor.
            if (isMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // If the menu is not open, hide the cursor and lock the cursor to the center of the screen.
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
