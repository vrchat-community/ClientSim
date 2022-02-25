using VRC.Udon.Common;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimUdonInputEventSender
    {
        void RunInputAction(string eventName, UdonInputEventArgs args);
    }
}