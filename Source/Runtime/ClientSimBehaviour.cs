using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public abstract class ClientSimBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            this.PreventComponentFromSaving();
        }
    }
}