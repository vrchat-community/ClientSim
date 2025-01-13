#if VRC_ENABLE_PLAYER_PERSISTENCE
namespace VRC.SDK3.ClientSim.Persistence
{
    public enum ClientSimPlayerDataType : byte
    {
        None = 0,
        Vector2 = 1,
        Vector3 = 2,
        Vector4 = 3,
        Quaternion = 4,
        Color = 5,
        Color32 = 6,
        WrappedString = 7,
        WrappedShort = 8,
        WrappedInt = 9,
        WrappedFloat = 10,
        WrappedBool = 11,
        WrappedByte = 12,
        WrappedBytes = 13,
        WrappedUShort = 14,
        WrappedUByte = 15,
        WrappedUInt = 16,
        WrappedULong = 17,
        WrappedDouble = 18,
        WrappedLong = 19,
    }
}
#endif