using System;
using System.Collections.Generic;

namespace VRC.SDK3.ClientSim
{
    [Serializable]
    public class ClientSimPlayerTagsData : IClientSimPlayerTagsData
    {
        private readonly Dictionary<string, string> _tags = new Dictionary<string, string>();
        
        public void ClearPlayerTags()
        {
            _tags.Clear();
        }

        public void SetPlayerTag(string tagName, string tagValue)
        {
            _tags[tagName] = tagValue;
        }

        public string GetPlayerTag(string tagName)
        {
            if (_tags.TryGetValue(tagName, out string tagValue))
            {
                return tagValue;
            }
            return "";
        }

        public bool HasPlayerTag(string tagName, string tagValue)
        {
            return GetPlayerTag(tagName) == tagValue;
        }
    }
}