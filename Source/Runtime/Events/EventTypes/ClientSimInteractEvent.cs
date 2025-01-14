using System.Collections.Generic;
using UnityEngine;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimInteractEvent : IClientSimEvent
    {
        public HandType handType;
        public GameObject interactObject;
        public float interactDistance;
        public List<IClientSimInteractable> interacts;
    }
}