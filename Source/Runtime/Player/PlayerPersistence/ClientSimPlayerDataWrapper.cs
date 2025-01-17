#if VRC_ENABLE_PLAYER_PERSISTENCE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Persistence
{
    public static class ClientSimPlayerDataWrapper
    {
        public static void ConfigureSDK()
        {
            System.Func<VRCPlayerApi, IEnumerable<string>> _getKeys = GetKeys;

            FieldInfo getKeysInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getKeys", BindingFlags.Static | BindingFlags.NonPublic);
            getKeysInfo?.SetValue(null, _getKeys);
            
            System.Func<VRCPlayerApi, string, bool> _hasKey = HasKey;

            FieldInfo hasKeyInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_hasKey", BindingFlags.Static | BindingFlags.NonPublic);
            hasKeyInfo?.SetValue(null, _hasKey);
            
            System.Func<VRCPlayerApi, string, Type> _getType = GetType;

            FieldInfo getTypeInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getType", BindingFlags.Static | BindingFlags.NonPublic);
            getTypeInfo?.SetValue(null, _getType);

            System.Action<string, bool> _setBool = SetBool;
            System.Func<VRCPlayerApi, string, bool> _getBool = GetBool;

            FieldInfo setBoolInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setBool", BindingFlags.Static | BindingFlags.NonPublic);
            setBoolInfo?.SetValue(null, _setBool);
            
            FieldInfo getBoolInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getBool", BindingFlags.Static | BindingFlags.NonPublic);
            getBoolInfo?.SetValue(null, _getBool);
            
            System.Action<string, byte> _setByte = SetByte;
            System.Func<VRCPlayerApi, string, byte> _getByte = GetByte;

            FieldInfo setByteInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setByte", BindingFlags.Static | BindingFlags.NonPublic);
            setByteInfo?.SetValue(null, _setByte);
            
            FieldInfo getByteInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getByte", BindingFlags.Static | BindingFlags.NonPublic);
            getByteInfo?.SetValue(null, _getByte);
            
            System.Action<string, sbyte> _setUByte = SetSByte;
            System.Func<VRCPlayerApi, string, sbyte> _getUByte = GetSByte;
            
            FieldInfo setUByteInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setSByte", BindingFlags.Static | BindingFlags.NonPublic);
            setUByteInfo?.SetValue(null, _setUByte);
            
            FieldInfo getUByteInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getSByte", BindingFlags.Static | BindingFlags.NonPublic);
            getUByteInfo?.SetValue(null, _getUByte);
            
            System.Action<string, byte[]> _setBytes = SetBytes;
            System.Func<VRCPlayerApi, string, byte[]> _getBytes = GetBytes;

            FieldInfo setBytesInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setBytes", BindingFlags.Static | BindingFlags.NonPublic);
            setBytesInfo?.SetValue(null, _setBytes);
            
            FieldInfo getBytesInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getBytes", BindingFlags.Static | BindingFlags.NonPublic);
            getBytesInfo?.SetValue(null, _getBytes);
            
            System.Action<string, string> _setString = SetString;
            System.Func<VRCPlayerApi, string, string> _getString = GetString;

            FieldInfo setStringInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setString", BindingFlags.Static | BindingFlags.NonPublic);
            setStringInfo?.SetValue(null, _setString);
            
            FieldInfo getStringInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getString", BindingFlags.Static | BindingFlags.NonPublic);
            getStringInfo?.SetValue(null, _getString);
            
            System.Action<string, short> _setShort = SetShort;
            System.Func<VRCPlayerApi, string, short> _getShort = GetShort;

            FieldInfo setShortInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setShort", BindingFlags.Static | BindingFlags.NonPublic);
            setShortInfo?.SetValue(null, _setShort);
            
            FieldInfo getShortInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getShort", BindingFlags.Static | BindingFlags.NonPublic);
            getShortInfo?.SetValue(null, _getShort);
            
            System.Action<string, ushort> _setUShort = SetUShort;
            System.Func<VRCPlayerApi, string, ushort> _getUShort = GetUShort;

            FieldInfo setUShortInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setUShort", BindingFlags.Static | BindingFlags.NonPublic);
            setUShortInfo?.SetValue(null, _setUShort);
            
            FieldInfo getUShortInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getUShort", BindingFlags.Static | BindingFlags.NonPublic);
            getUShortInfo?.SetValue(null, _getUShort);
            
            System.Action<string, int> _setInt = SetInt;
            System.Func<VRCPlayerApi, string, int> _getInt = GetInt;

            FieldInfo setIntInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setInt", BindingFlags.Static | BindingFlags.NonPublic);
            setIntInfo?.SetValue(null, _setInt);
            
            FieldInfo getIntInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getInt", BindingFlags.Static | BindingFlags.NonPublic);
            getIntInfo?.SetValue(null, _getInt);
            
            System.Action<string, uint> _setUInt = SetUInt;
            System.Func<VRCPlayerApi, string, uint> _getUInt = GetUInt;

            FieldInfo setUIntInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setUInt", BindingFlags.Static | BindingFlags.NonPublic);
            setUIntInfo?.SetValue(null, _setUInt);
            
            FieldInfo getUIntInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getUInt", BindingFlags.Static | BindingFlags.NonPublic);
            getUIntInfo?.SetValue(null, _getUInt);
            
            System.Action<string, long> _setLong = SetLong;
            System.Func<VRCPlayerApi, string, long> _getLong = GetLong;
            
            FieldInfo setLongInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setLong", BindingFlags.Static | BindingFlags.NonPublic);
            setLongInfo?.SetValue(null, _setLong);
            
            FieldInfo getLongInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getLong", BindingFlags.Static | BindingFlags.NonPublic);
            getLongInfo?.SetValue(null, _getLong);
            
            System.Action<string, ulong> _setULong = SetULong;
            System.Func<VRCPlayerApi, string, ulong> _getULong = GetULong;
            
            FieldInfo setULongInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setULong", BindingFlags.Static | BindingFlags.NonPublic);
            setULongInfo?.SetValue(null, _setULong);
            
            FieldInfo getULongInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getULong", BindingFlags.Static | BindingFlags.NonPublic);
            getULongInfo?.SetValue(null, _getULong);
            
            System.Action<string, double> _setDouble = SetDouble;
            System.Func<VRCPlayerApi, string, double> _getDouble = GetDouble;
            
            FieldInfo setDoubleInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setDouble", BindingFlags.Static | BindingFlags.NonPublic);
            setDoubleInfo?.SetValue(null, _setDouble);
            
            FieldInfo getDoubleInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getDouble", BindingFlags.Static | BindingFlags.NonPublic);
            getDoubleInfo?.SetValue(null, _getDouble);
            
            System.Action<string, float> _setFloat = SetFloat;
            System.Func<VRCPlayerApi, string, float> _getFloat = GetFloat;

            FieldInfo setFloatInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setFloat", BindingFlags.Static | BindingFlags.NonPublic);
            setFloatInfo?.SetValue(null, _setFloat);
            
            FieldInfo getFloatInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getFloat", BindingFlags.Static | BindingFlags.NonPublic);
            getFloatInfo?.SetValue(null, _getFloat);
            
            System.Action<string, Vector2> _setVector2 = SetVector2;
            System.Func<VRCPlayerApi, string, Vector2> _getVector2 = GetVector2;

            FieldInfo setVector2Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setVector2", BindingFlags.Static | BindingFlags.NonPublic);
            setVector2Info?.SetValue(null, _setVector2);
            
            FieldInfo getVector2Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getVector2", BindingFlags.Static | BindingFlags.NonPublic);
            getVector2Info?.SetValue(null, _getVector2);
            
            System.Action<string, Vector3> _setVector3 = SetVector3;
            System.Func<VRCPlayerApi, string, Vector3> _getVector3 = GetVector3;

            FieldInfo setVector3Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setVector3", BindingFlags.Static | BindingFlags.NonPublic);
            setVector3Info?.SetValue(null, _setVector3);
            
            FieldInfo getVector3Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getVector3", BindingFlags.Static | BindingFlags.NonPublic);
            getVector3Info?.SetValue(null, _getVector3);
            
            System.Action<string, Vector4> _setVector4 = SetVector4;
            System.Func<VRCPlayerApi, string, Vector4> _getVector4 = GetVector4;

            FieldInfo setVector4Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setVector4", BindingFlags.Static | BindingFlags.NonPublic);
            setVector4Info?.SetValue(null, _setVector4);
            
            FieldInfo getVector4Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getVector4", BindingFlags.Static | BindingFlags.NonPublic);
            getVector4Info?.SetValue(null, _getVector4);
            
            System.Action<string, Quaternion> _setQuaternion = SetQuaternion;
            System.Func<VRCPlayerApi, string, Quaternion> _getQuaternion = GetQuaternion;

            FieldInfo setQuaternionInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setQuaternion", BindingFlags.Static | BindingFlags.NonPublic);
            setQuaternionInfo?.SetValue(null, _setQuaternion);
            
            FieldInfo getQuaternionInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getQuaternion", BindingFlags.Static | BindingFlags.NonPublic);
            getQuaternionInfo?.SetValue(null, _getQuaternion);
            
            System.Action<string, UnityEngine.Color> _setColor = SetColor;
            System.Func<VRCPlayerApi, string, UnityEngine.Color> _getColor = GetColor;

            FieldInfo setColorInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setColor", BindingFlags.Static | BindingFlags.NonPublic);
            setColorInfo?.SetValue(null, _setColor);
            
            FieldInfo getColorInfo = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getColor", BindingFlags.Static | BindingFlags.NonPublic);
            getColorInfo?.SetValue(null, _getColor);
            
            System.Action<string, UnityEngine.Color32> _setColor32 = SetColor32;
            System.Func<VRCPlayerApi, string, UnityEngine.Color32> _getColor32 = GetColor32;

            FieldInfo setColor32Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_setColor32", BindingFlags.Static | BindingFlags.NonPublic);
            setColor32Info?.SetValue(null, _setColor32);
            
            FieldInfo getColor32Info = typeof(VRC.SDK3.Persistence.PlayerData).GetField("_getColor32", BindingFlags.Static | BindingFlags.NonPublic);
            getColor32Info?.SetValue(null, _getColor32);
        }

        private static bool FindStorage(VRCPlayerApi playerApi, out ClientSimPlayerDataStorage storage)
        {
            ClientSimPlayer player = playerApi.GetClientSimPlayer();
            if (!player)
            {
                UnityEngine.Debug.LogError("Could not locate player with id " + playerApi.playerId + " for PlayerData storage.");
                storage = null;
                return false;
            }

            storage = player.PlayerDataObject;
            if (!storage)
            {
                UnityEngine.Debug.LogError("Could not locate PlayerData storage for player " + playerApi.playerId);
                return false;
            }

            return true;
        }

        public static IEnumerable<string> GetKeys(VRCPlayerApi playerApi)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return Enumerable.Empty<string>();
            return storage.GetKeys();
        }

        public static bool HasKey(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return false;
            return storage.HasKey(key);
        }

        public static Type GetType(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return null;
            return storage.GetType(key);
        }

        public static void SetBool(string key, bool data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetBool(key, data);
        }

        public static bool GetBool(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetBool(key);
        }

        public static void SetByte(string key, byte data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetByte(key, data);
        }

        public static byte GetByte(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetByte(key);
        }
        
        public static void SetSByte(string key, sbyte data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetSByte(key, data);
        }
        
        public static sbyte GetSByte(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetSByte(key);
        }
        
        public static void SetBytes(string key, byte[] data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetBytes(key, data);
        }

        public static byte[] GetBytes(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetBytes(key);
        }

        public static void SetString(string key, string data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetString(key, data);
        }

        public static string GetString(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetString(key);
        }

        public static void SetShort(string key, short data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetShort(key, data);
        }

        public static void SetUShort(string key, ushort data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetUShort(key, data);
        }

        public static short GetShort(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetShort(key);
        }

        public static ushort GetUShort(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetUShort(key);
        }

        public static void SetInt(string key, int data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetInt(key, data);
        }

        public static void SetUInt(string key, uint data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetUInt(key, data);
        }

        public static int GetInt(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetInt(key);
        }

        public static uint GetUInt(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetUInt(key);
        }
        
        public static void SetLong(string key, long data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetLong(key, data);
        }
        
        public static long GetLong(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetLong(key);
        }
        
        public static void SetULong(string key, ulong data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetULong(key, data);
        }

        public static ulong GetULong(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetULong(key);
        }
        
        public static void SetFloat(string key, float data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetFloat(key, data);
        }

        public static float GetFloat(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetFloat(key);
        }
        
        public static void SetDouble(string key, double data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetDouble(key, data);
        }
        
        public static double GetDouble(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetDouble(key);
        }

        public static void SetVector2(string key, Vector2 data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetVector2(key, data);
        }

        public static Vector2 GetVector2(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetVector2(key);
        }

        public static void SetVector3(string key, Vector3 data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetVector3(key, data);
        }

        public static Vector3 GetVector3(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetVector3(key);
        }

        public static void SetVector4(string key, Vector4 data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetVector4(key, data);
        }

        public static Vector4 GetVector4(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetVector4(key);
        }

        public static void SetQuaternion(string key, Quaternion data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetQuaternion(key, data);
        }

        public static Quaternion GetQuaternion(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetQuaternion(key);
        }

        public static void SetColor(string key, UnityEngine.Color data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetColor(key, data);
        }

        public static UnityEngine.Color GetColor(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetColor(key);
        }

        public static void SetColor32(string key, UnityEngine.Color32 data)
        {
            if (!FindStorage(Networking.LocalPlayer, out ClientSimPlayerDataStorage storage)) return;
            storage.SetColor32(key, data);
        }

        public static UnityEngine.Color32 GetColor32(VRCPlayerApi playerApi, string key)
        {
            if (!FindStorage(playerApi, out ClientSimPlayerDataStorage storage)) return default;
            return storage.GetColor32(key);
        }
    }
}
#endif