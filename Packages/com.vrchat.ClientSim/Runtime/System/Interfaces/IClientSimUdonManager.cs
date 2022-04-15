using VRC.Udon;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimUdonManager
    {
        void AddUdonBehaviour(UdonBehaviour udon);
        void RemoveUdonBehaviour(UdonBehaviour udon);
    }
}