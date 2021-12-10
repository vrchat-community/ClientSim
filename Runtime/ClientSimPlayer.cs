using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    [AddComponentMenu("")]
    [SelectionBase]
    public class ClientSimPlayer : ClientSimBehaviour
    {
        public VRCPlayerApi player;
        private Dictionary<string, string> tags = new Dictionary<string, string>();

        public void SetPlayer(VRCPlayerApi player)
        {
            this.player = player;

            // TODO handle this better
            ClientSimPlayerController playerController = GetComponent<ClientSimPlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayer(this);
            }
        }

        public void ClearTags()
        {
            tags.Clear();
        }

        public void SetTag(string tagName, string tagValue)
        {
            tags[tagName] = tagValue;
        }

        public string GetTag(string tagName)
        {
            if (tags.TryGetValue(tagName, out string tagValue))
            {
                return tagValue;
            }
            return "";
        }

        public bool HasTag(string tagName, string tagValue)
        {
            return GetTag(tagName) == tagValue;
        }
    }
}