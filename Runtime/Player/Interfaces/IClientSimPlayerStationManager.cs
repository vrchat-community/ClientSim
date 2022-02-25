namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerStationManager
    {
        bool InStation();
        IClientSimStation GetCurrentStation();
        bool IsLockedInStation();
        bool CanPlayerMove(float moveValue);
        void EnterStation(IClientSimStation station);
        void ExitStation(IClientSimStation station, bool forcedExit = false);
    }
}