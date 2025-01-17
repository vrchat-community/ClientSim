#if VRC_ENABLE_PLAYER_PERSISTENCE
using Newtonsoft.Json;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Persistence
{
    [JsonConverter(typeof(ClientSimPlayerDataConverter))] 
    public class ClientSimPlayerDataTypeUnion
    {
        public object Value { get; set; }
        public ClientSimPlayerDataType Type { get; set; }
        public ClientSimPlayerDataTypeUnion() {
            this.Type = ClientSimPlayerDataType.None;
            this.Value = null;
        }

        public Vector2 AsVector2() => (Vector2)Value;
        public Vector3 AsVector3() => (Vector3)Value;
        public Vector4 AsVector4() => (Vector4)Value;
        public Quaternion AsQuaternion() => (Quaternion)Value;
        public Color AsColor() => (Color)Value;
        public Color32 AsColor32() => (Color32)Value;
        public string AsWrappedString() { return (string)Value; }
        public short AsWrappedShort() { return (short)Value; }
        public ushort AsWrappedUShort() { return (ushort)Value; }
        public int AsWrappedInt() { return (int)Value; }
        public uint AsWrappedUInt() { return (uint)Value; }
        public long AsWrappedLong() { return (long)Value; }
        public ulong AsWrappedULong() { return ( ulong)Value; }
        public float AsWrappedFloat() { return ( float)Value; }
        public double AsWrappedDouble() { return (double)Value; }
        public bool AsWrappedBool() { return (bool)Value; }
        public byte AsWrappedByte() { return (byte)Value ; }
        public sbyte AsWrappedSByte() { return (sbyte)Value; }
        public byte[] AsWrappedBytes() { return (byte[])Value; }
        public byte AsWrappedUByte() { return (byte)Value; }
    }
}
#endif