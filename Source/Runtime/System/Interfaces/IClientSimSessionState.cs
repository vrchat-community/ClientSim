namespace VRC.SDK3.ClientSim
{
    public interface IClientSimSessionState
    {
        bool GetBool(string key);
        void SetBool(string key, bool value);
    }
}