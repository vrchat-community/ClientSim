namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Wrapper for sending events to all udon programs.
    /// Helps in tests without directly referencing UdonManager.
    /// </summary>
    public interface IClientSimUdonEventSender
    {
        void RunEvent(string eventName, params (string, object)[] programVariables);
    }
}