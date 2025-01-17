using UnityEngine;
using VRC.SDKBase;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Wrapper around ClientSimInputActionBased that supplies the input actions.
    /// Classes were split to allow for testing without monobehaviours.
    /// </summary>
    [AddComponentMenu("")]
    public class ClientSimInputManager : ClientSimBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
        private ClientSimInputActionBased _input;
        
        protected override void Awake()
        {
            base.Awake();
            _playerInput = GetComponent<PlayerInput>();
        }
        
        private void OnDestroy()
        {
            _input?.Dispose();
        }
#endif
        
        public void Initialize(ClientSimSettings settings)
        {
#if ENABLE_INPUT_SYSTEM
            _input = new ClientSimInputActionBased(_playerInput.actions, settings);
#endif
        }

        public IClientSimInput GetInput()
        {
#if ENABLE_INPUT_SYSTEM
            return _input;
#else
            return null;
#endif
        }

        public VRCInputMethod GetLastUsedInputMethod()
        {
#if ENABLE_INPUT_SYSTEM
            return _input.LastInputMethod;
#else
            return VRCInputMethod.Generic;
#endif
        }
    }
}