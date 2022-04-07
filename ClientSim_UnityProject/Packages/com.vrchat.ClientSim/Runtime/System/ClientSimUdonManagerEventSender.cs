using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimUdonManagerEventSender : IClientSimUdonEventSender
    {
        private readonly UdonManager _udonManager;

        public ClientSimUdonManagerEventSender(UdonManager udonManager)
        {
            _udonManager = udonManager;
        }
        
        public void RunEvent(string eventName, params (string, object)[] programVariables)
        {
            _udonManager.RunEvent(eventName, programVariables);
        }
    }
}