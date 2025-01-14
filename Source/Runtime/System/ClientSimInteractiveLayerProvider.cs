using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// System that provides what layers the player can interact with at a given time.
    /// Interactive layers will change depending on if the Menu is open or not.
    /// </summary>
    /// <remarks>
    /// Listens to Events:
    /// - ClientSimMenuStateChangedEvent
    /// </remarks>
    public class ClientSimInteractiveLayerProvider : IClientSimInteractiveLayerProvider, IDisposable
    {
        private const int UI_LAYER = 5;
        private const int PLAYER_LOCAL_LAYER = 10;
        private const int UI_MENU_LAYER = 12;
        private const int MIRROR_REFLECTION_LAYER = 18;
        private const int INTERNAL_UI_LAYER = 19;
        private const int FIRST_USER_LAYER = 22;

        private readonly int _interactiveLayersDefault;
        private readonly int _interactiveLayersUI;

        private readonly IClientSimEventDispatcher _eventDispatcher;
        
        private bool _menuIsOpen;

        public ClientSimInteractiveLayerProvider(IClientSimEventDispatcher eventDispatcher)
        {
            // Only the UI and UIMenu layers are interactable when the UI is open.
            _interactiveLayersUI = (1 << UI_LAYER) | (1 << UI_MENU_LAYER) | (1 << INTERNAL_UI_LAYER);
            // When the menu is not open, all layers but UI, UIMenu, PlayerLocal, and MirrorReflection layers are interactable.
            _interactiveLayersDefault = ~(1 << UI_LAYER) & ~(1 << UI_MENU_LAYER) & ~(1 << PLAYER_LOCAL_LAYER) & ~(1 << MIRROR_REFLECTION_LAYER);
            // If Interaction Passthrough is set, these User Layers will also be ignored.
            _interactiveLayersDefault &= ~(VRC_SceneDescriptor.Instance.interactThruLayers << FIRST_USER_LAYER);
            
            _eventDispatcher = eventDispatcher;
            _eventDispatcher.Subscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
        }
        
        ~ClientSimInteractiveLayerProvider()
        {
            Dispose();
        }

        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<ClientSimMenuStateChangedEvent>(SetMenuOpen);
        }

        public LayerMask GetInteractiveLayers()
        {
            return _menuIsOpen ? _interactiveLayersUI : _interactiveLayersDefault;
        }
        
        private void SetMenuOpen(ClientSimMenuStateChangedEvent stateChangedEvent)
        {
            _menuIsOpen = stateChangedEvent.isMenuOpen;
        }
    }
}