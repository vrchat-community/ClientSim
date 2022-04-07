using UnityEngine;
using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Wrapper class for UdonInput
    /// </summary>
    [AddComponentMenu("")]
    [DefaultExecutionOrder(1)] // Ensure that input events happen after UdonBehaviour.Update
    public class ClientSimUdonInputBehaviour : ClientSimBehaviour
    {
        private ClientSimUdonInput _udonInput;

        public void Initialize(IClientSimEventDispatcher eventDispatcher, IClientSimInput input)
        {
            _udonInput = new ClientSimUdonInput(
                eventDispatcher, 
                input,
                new ClientSimUdonManagerInputEventSender(UdonManager.Instance));
        }

        private void OnDestroy()
        {
            _udonInput?.Dispose();
        }

        public void Update()
        {
            _udonInput?.ProcessInputEvents();
        }
    }
}