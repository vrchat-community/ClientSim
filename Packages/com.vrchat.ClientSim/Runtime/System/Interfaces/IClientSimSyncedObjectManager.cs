namespace VRC.SDK3.ClientSim
{
    public interface IClientSimSyncedObjectManager
    {
        void AddSyncedObject(IClientSimSyncable sync);
        void RemoveSyncedObject(IClientSimSyncable sync);
    }
}