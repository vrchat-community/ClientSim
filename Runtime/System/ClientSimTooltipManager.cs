using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Display a tooltip above an interactable.
    /// </summary>
    [AddComponentMenu("")]
    [DefaultExecutionOrder(30001)] // Ensure that all tooltip positions are updated at the end of frame.
    public class ClientSimTooltipManager : ClientSimBehaviour, IClientSimTooltipManager
    {
        [SerializeField]
        private GameObject tooltipPrefab;
        
        private IClientSimTrackingProvider _trackingProvider;
        private ClientSimSettings _settings;

        private readonly Queue<ClientSimTooltip> _unusedTooltips = new Queue<ClientSimTooltip>();
        private readonly List<ClientSimTooltip> _displayedTooltips = new List<ClientSimTooltip>();
        
        public void Initialize(ClientSimSettings settings, IClientSimTrackingProvider trackingProvider)
        {
            _trackingProvider = trackingProvider;
            _settings = settings;
        }
        
        // TODO expand this to allow passing in any object to display tooltips
        // (Allow for pickup use text without changing the interactable since the other hand still needs the original text)
        public void DisplayTooltip(IClientSimInteractable interact)
        {
            if (!_settings.showTooltips)
            {
                return;
            }
            
            ClientSimTooltip tooltip = GetUnusedTooltip();
            tooltip.EnableTooltip(interact);
            _displayedTooltips.Add(tooltip);
        }

        public void DisableTooltip(IClientSimInteractable interact)
        {
            // Loop through list backwards to ensure removing doesn't change the order. 
            for (int cur = _displayedTooltips.Count - 1; cur >= 0; --cur)
            {
                ClientSimTooltip tooltip = _displayedTooltips[cur];
                if (tooltip.Interactable == interact)
                {
                    _displayedTooltips.RemoveAt(cur);
                    DisableTooltip(tooltip);
                }
            }
        }

        private ClientSimTooltip GetUnusedTooltip()
        {
            if (_unusedTooltips.Count == 0)
            {
                GameObject tooltipObj = Instantiate(tooltipPrefab, transform);
                return tooltipObj.GetComponent<ClientSimTooltip>();
            }

            return _unusedTooltips.Dequeue();
        }

        private void DisableTooltip(ClientSimTooltip tooltip)
        {
            tooltip.DisableTooltip();
            _unusedTooltips.Enqueue(tooltip);
        }

        private void LateUpdate()
        {
            if (_trackingProvider == null)
            {
                return;
            }
            Transform playerHead = _trackingProvider.GetTrackingTransform(VRCPlayerApi.TrackingDataType.Head);
            Vector3 playerPos = playerHead.position;
            Vector3 playerUp = playerHead.up;
            // Use playspace up to prevent billboard effect in vr.
            if (_trackingProvider.IsVR())
            {
                playerUp = _trackingProvider.GetTrackingTransform(VRCPlayerApi.TrackingDataType.Origin).up;
            }
            
            for (int cur = _displayedTooltips.Count - 1; cur >= 0; --cur)
            {
                ClientSimTooltip tooltip = _displayedTooltips[cur];
                
                // Remove tooltips with destroyed interactables.
                if (tooltip.Interactable == null)
                {
                    _displayedTooltips.RemoveAt(cur);
                    DisableTooltip(tooltip);
                    continue;
                }
                
                tooltip.UpdateTooltip(playerPos, playerUp);
            }
        }
    }
}