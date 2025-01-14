
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Represents an object that syncs its position
    /// </summary>
    public interface IClientSimPositionSyncable : IClientSimSyncable, IClientSimRespawnHandler
    {
        bool SyncPosition { get; }
        void SetIsKinematic(bool value);
        void SetUseGravity(bool value);
        bool GetIsKinematic();
        bool GetUseGravity();
        void UpdatePositionSync();
        Transform GetTransform();
    }
}