using VRC.SDK3.Components;

namespace VRC.SDK3.ClientSim
{
    public interface IClientSimSyncedObjectManager
    {
        void AddSyncedObject(IClientSimSyncable sync);
        void RemoveSyncedObject(IClientSimSyncable sync);
        
        void InitializeObjectSync(VRCObjectSync sync);
        void InitializeObjectPool(VRCObjectPool objectPool);
    }
}