using VRC.Udon;
using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Wrapper for UdonManager. Only needed to prevent direct dependency to UdonManger in the UdonInput class.
    /// </summary>
    public class ClientSimUdonManagerInputEventSender : IClientSimUdonInputEventSender
    {
        private readonly UdonManager _udonManager;

        public ClientSimUdonManagerInputEventSender(UdonManager udonManager)
        {
            _udonManager = udonManager;
        }

        public void RunInputAction(string eventName, UdonInputEventArgs args)
        {
            _udonManager.RunInputAction(eventName, args);
        }
    }
}