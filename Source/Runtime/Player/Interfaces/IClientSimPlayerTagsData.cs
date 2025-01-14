namespace VRC.SDK3.ClientSim
{
    public interface IClientSimPlayerTagsData
    {
        void ClearPlayerTags();
        void SetPlayerTag(string tagName, string tagValue);
        string GetPlayerTag(string tagName);
        bool HasPlayerTag(string tagName, string tagValue);
    }
}