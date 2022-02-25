
using System;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This system is responsible for displaying the Reticle in the center of the screen
    /// and for displaying the mouse UI pointer if there is a UI Shape under the mouse.
    /// </summary>
    /// <remarks>
    /// Listens to Events:
    /// - ClientSimMouseReleasedEvent
    /// - ClientSimRaycastHitResultsEvent
    /// </remarks>
    [AddComponentMenu("")]
    public class ClientSimReticle : ClientSimBehaviour, IDisposable
    {
        [SerializeField]
        private Texture2D reticle;
        [SerializeField]
        private Texture2D uiShapeHoverIcon;
        
        private IClientSimEventDispatcher _eventDispatcher;
        private ClientSimSettings _settings;
        private IClientSimMousePositionProvider _mousePositionProvider;

        private bool _mouseReleased = false;
        private int _lastUiShapeHoveredFrame = -1;

        public void Initialize(
            IClientSimEventDispatcher eventDispatcher, 
            ClientSimSettings settings,
            IClientSimMousePositionProvider mousePositionProvider)
        {
            _settings = settings;
            _eventDispatcher = eventDispatcher;
            _mousePositionProvider = mousePositionProvider;

            _eventDispatcher.Subscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            _eventDispatcher.Subscribe<ClientSimRaycastHitResultsEvent>(OnRaycastHit);
        }

        private void OnDestroy()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            _eventDispatcher?.Unsubscribe<ClientSimMouseReleasedEvent>(MouseReleasedEvent);
            _eventDispatcher?.Unsubscribe<ClientSimRaycastHitResultsEvent>(OnRaycastHit);
        }
        
        #region ClientSim Events

        private void MouseReleasedEvent(ClientSimMouseReleasedEvent mouseReleasedEvent)
        {
            _mouseReleased = mouseReleasedEvent.isReleased;
        }

        private void OnRaycastHit(ClientSimRaycastHitResultsEvent hitEvent)
        {
            if (hitEvent.raycastResults?.uiShape != null)
            {
                _lastUiShapeHoveredFrame = Time.frameCount;
            }
        }
        
        #endregion

        private bool ShouldShowReticle()
        {
            return _settings.showDesktopReticle && !_mouseReleased;
        }

        private bool ShouldShowUiShapeHoverIcon()
        {
            return _lastUiShapeHoveredFrame == Time.frameCount;
        }
        
        private void OnGUI()
        {
            if (ShouldShowReticle())
            {
                Vector2 center = ClientSimBaseInput.GetScreenCenter();
                Vector2 size = new Vector2(reticle.width, reticle.height);
                Rect position = new Rect(center - size * 0.5f, size);
                GUI.DrawTexture(position, reticle);
            }

            if (ShouldShowUiShapeHoverIcon())
            {
                Vector2 mousePos = _mousePositionProvider.GetMousePosition();
                // GUI draws with inverted y
                mousePos.y = Screen.height - mousePos.y;
                
                Vector2 size = new Vector2(uiShapeHoverIcon.width, uiShapeHoverIcon.height);
                Rect position = new Rect(mousePos - new Vector2(8, 8), size);
                GUI.DrawTexture(position, uiShapeHoverIcon);
            }
        }
    }
}