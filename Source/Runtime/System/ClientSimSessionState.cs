namespace VRC.SDK3.ClientSim
{
    public class ClientSimSessionState : IClientSimSessionState
    {
        private const string SESSION_KEY_PREFIX = "com.vrchat.clientsim.session";

        private string GetSessionKey(string key)
        {
            return $"{SESSION_KEY_PREFIX}.{key}";
        }
        
        public bool GetBool(string key)
        {
#if UNITY_EDITOR
            return UnityEditor.SessionState.GetBool(GetSessionKey(key), false);
#else
            return false;
#endif
        }

        public void SetBool(string key, bool value)
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.SetBool(GetSessionKey(key), value);
#endif
        }
    }
}