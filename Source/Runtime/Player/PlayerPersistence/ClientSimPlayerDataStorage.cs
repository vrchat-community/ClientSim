using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using VRC.SDKBase;

#if VRC_ENABLE_PLAYER_PERSISTENCE
using UnityEngine.SceneManagement;
using VRC.SDK3.Persistence;
using VRC.Udon;
#endif

namespace VRC.SDK3.ClientSim.Persistence
{
    [AddComponentMenu("")] // hides component in Add Component menu
    public class ClientSimPlayerDataStorage : ClientSimBehaviour
    {
#if VRC_ENABLE_PLAYER_PERSISTENCE
        public static string PlayerDataFolder => Path.Combine("ClientSimStorage", "PlayerData");
        internal static string PlayerDataFilePath(VRCPlayerApi player) 
        {
            string root = Path.GetDirectoryName(Application.dataPath);
            string path = Path.Combine(root, PlayerDataFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + "/PlayerData_" + $"{player.playerId}" + $"_{SceneManager.GetActiveScene().name}" + ".json";
        }

        private VRCPlayerApi _player;
        private IClientSimUdonEventSender _udonEventSender;
        private IClientSimEventDispatcher _eventDispatcher;
        
        private readonly Dictionary<string, ClientSimPlayerDataPair> leData = new();
        private readonly Dictionary<string, PlayerData.Info> localInfoChanges = new();
        private readonly List<PlayerData.Info> queuedPlayerDataUpdates = new();

        private bool doDecode;
        private bool isDoneDecoding;
        private bool hasPostedPlayerDataDecoded;
        private bool hasPostedPlayerRestored;

        public IEnumerable<string> GetKeys()
        {
            return leData.Keys;
        }
        
        public bool HasKey(string key) => leData.ContainsKey(key);

        public Type GetType(string key)
        {
            if (!leData.TryGetValue(key, out ClientSimPlayerDataPair value))
                return null;

            switch (value.Value.Type)
            {
                default:
                case ClientSimPlayerDataType.None:
                    return null;
                case ClientSimPlayerDataType.Vector2:
                    return typeof(Vector2);
                case ClientSimPlayerDataType.Vector3:
                    return typeof(Vector3);
                case ClientSimPlayerDataType.Vector4:
                    return typeof(Vector4);
                case ClientSimPlayerDataType.Quaternion:
                    return typeof(Quaternion);
                case ClientSimPlayerDataType.Color:
                    return typeof(Color);
                case ClientSimPlayerDataType.Color32:
                    return typeof(Color32);
                case ClientSimPlayerDataType.WrappedString:
                    return typeof(string);
                case ClientSimPlayerDataType.WrappedShort:
                    return typeof(short);
                case ClientSimPlayerDataType.WrappedUShort:
                    return typeof(ushort);
                case ClientSimPlayerDataType.WrappedInt:
                    return typeof(int);
                case ClientSimPlayerDataType.WrappedUInt:
                    return typeof(uint);
                case ClientSimPlayerDataType.WrappedLong:
                    return typeof(long);
                case ClientSimPlayerDataType.WrappedULong:
                    return typeof(ulong);
                case ClientSimPlayerDataType.WrappedFloat:
                    return typeof(float);
                case ClientSimPlayerDataType.WrappedDouble:
                    return typeof(double);
                case ClientSimPlayerDataType.WrappedBool:
                    return typeof(bool);
                case ClientSimPlayerDataType.WrappedByte:
                    return typeof(sbyte);
                case ClientSimPlayerDataType.WrappedUByte:
                    return typeof(byte);
                case ClientSimPlayerDataType.WrappedBytes:
                    return typeof(byte[]);
            }
        }
        
#region Setters

        private PlayerData.State _SetWrapper(string key, Func<ClientSimPlayerDataPair, bool> set, bool isRestore, bool flushChanges, DateTime lastUpdated)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key was invalid");

            if (!isRestore && leData.TryGetValue(key, out ClientSimPlayerDataPair value))
            {
                if (set(value))
                {
                    value.LastUpdated = DateTime.Now;
                    localInfoChanges[key] = new PlayerData.Info(key, PlayerData.State.Changed);
                    if (flushChanges)
                    {
                        FlushLocalInfoChanges();
                    }
                    return PlayerData.State.Changed;
                }
                return PlayerData.State.Unchanged;
            }

            value = new ClientSimPlayerDataPair { Key = key, LastUpdated = isRestore ? lastUpdated : DateTime.Now };
            leData[key] = value;
            set(value);

            var state = isRestore ? PlayerData.State.Restored : PlayerData.State.Added;
            localInfoChanges[key] = new PlayerData.Info(key, state);
            if (flushChanges)
            {
                FlushLocalInfoChanges();
            }
            return state;
        }
        
        public PlayerData.State SetBool(string key, bool value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedBool)
                {
                    if (pair.Value.AsWrappedBool() == value)
                        return false; 
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedBool,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetByte(string key, byte value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedUByte)
                {
                    if (pair.Value.AsWrappedUByte() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedUByte,
                        Value = value
                    };
                
                return true;
            }
        }
        public PlayerData.State SetSByte(string key, sbyte value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedByte)
                {
                    if (pair.Value.AsWrappedSByte() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedByte,
                        Value = value
                    };
                
                return true;
            }
        }
        public PlayerData.State SetBytes(string key, byte[] value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);

            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedBytes)
                {
                    if (pair.Value.AsWrappedBytes().SequenceEqual(value))
                        return false;
                    
                    pair.Value.Value = value.ToArray();
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedBytes,
                        Value = value.ToArray()
                    };
                
                return true;
            }
        }
        public PlayerData.State SetString(string key, string value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedString)
                {
                    if (pair.Value.AsWrappedString() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedString,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetShort(string key, short value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedShort)
                {
                    if (pair.Value.AsWrappedShort() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedShort,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetUShort(string key, ushort value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedUShort)
                {
                    if (pair.Value.AsWrappedUShort() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedUShort,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetInt(string key, int value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedInt)
                {
                    if (pair.Value.AsWrappedInt() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedInt,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetUInt(string key, uint value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);

            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedUInt)
                {
                    if (pair.Value.AsWrappedUInt() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedUInt,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetLong(string key, long value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedLong)
                {
                    if (pair.Value.AsWrappedLong() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedLong,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetULong(string key, ulong value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedULong)
                {
                    if (pair.Value.AsWrappedULong() == value)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedULong,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetFloat(string key, float value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedFloat)
                {
                    if (Math.Abs(pair.Value.AsWrappedFloat() - value) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedFloat,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetDouble(string key, double value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);

            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.WrappedDouble)
                {
                    if (Math.Abs(pair.Value.AsWrappedDouble() - value) < 0.000001)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.WrappedDouble,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetVector2(string key, Vector2 value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Vector2)
                {
                    if (Math.Abs(pair.Value.AsVector2().x - value.x) < 0.000001f
                        && Math.Abs(pair.Value.AsVector2().y - value.y) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Vector2,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetVector3(string key, Vector3 value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Vector3)
                {
                    if (Math.Abs(pair.Value.AsVector3().x - value.x) < 0.000001f
                        && Math.Abs(pair.Value.AsVector3().y - value.y) < 0.000001f
                        && Math.Abs(pair.Value.AsVector3().z - value.z) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Vector3,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetVector4(string key, Vector4 value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Vector4)
                {
                    if (Math.Abs(pair.Value.AsVector4().x - value.x) < 0.000001f
                        && Math.Abs(pair.Value.AsVector4().y - value.y) < 0.000001f
                        && Math.Abs(pair.Value.AsVector4().z - value.z) < 0.000001f
                        && Math.Abs(pair.Value.AsVector4().w - value.w) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Vector4,
                        Value = value
                    };
                return true;
            }
        }
        public PlayerData.State SetQuaternion(string key, Quaternion value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Quaternion)
                {
                    if (Math.Abs(pair.Value.AsQuaternion().x - value.x) < 0.000001f
                        && Math.Abs(pair.Value.AsQuaternion().y - value.y) < 0.000001f
                        && Math.Abs(pair.Value.AsQuaternion().z - value.z) < 0.000001f
                        && Math.Abs(pair.Value.AsQuaternion().w - value.w) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Quaternion,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetColor(string key, Color value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Color)
                {
                    var color = pair.Value.AsColor();
                    if (Math.Abs(color.r - value.r) < 0.01f
                        && Math.Abs(color.g - value.g) < 0.01f
                        && Math.Abs(color.b - value.b) < 0.01f
                        && Math.Abs(color.a - value.a) < 0.01f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Color,
                        Value = value
                    };

                return true;
            }
        }
        public PlayerData.State SetColor32(string key, Color32 value, DateTime lastUpdated = new DateTime(), bool isRestore = false, bool flushChanges = true)
        {
            return _SetWrapper(key, Set, isRestore, flushChanges, lastUpdated);
            
            bool Set(ClientSimPlayerDataPair pair)
            {
                if (pair.Value?.Type == ClientSimPlayerDataType.Color32)
                {
                    var color32 = pair.Value.AsColor32();
                    if (Math.Abs(color32.r - value.r) < 0.000001f
                        && Math.Abs(color32.g - value.g) < 0.000001f
                        && Math.Abs(color32.b - value.b) < 0.000001f
                        && Math.Abs(color32.a - value.a) < 0.000001f)
                        return false;
                    pair.Value.Value = value;
                }
                else
                    pair.Value = new ClientSimPlayerDataTypeUnion()
                    {
                        Type = ClientSimPlayerDataType.Color32,
                        Value = value
                    };

                return true;
            }
        }
#endregion

#region Getters

        private ClientSimPlayerDataPair _GetChecked(string key, ClientSimPlayerDataType expectedType)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key was invalid");
            if (!leData.TryGetValue(key, out ClientSimPlayerDataPair data) || data.Value == null)
            {
                return null;
            }

            if (data.Value.Type != expectedType)
            {
                this.LogError($"Data at {key} was not a {expectedType}, it was a {data.Value.Type}");
                return null;
            }
            return data;
        }

        public bool GetBool(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedBool)?.Value.AsWrappedBool() ?? default;

        public byte GetByte(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedUByte)?.Value.AsWrappedUByte() ?? default;
        
        public sbyte GetSByte(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedByte)?.Value.AsWrappedSByte() ?? default;

        public byte[] GetBytes(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedBytes)?.Value.AsWrappedBytes().ToArray();

        public string GetString(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedString)?.Value.AsWrappedString();

        public short GetShort(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedShort)?.Value.AsWrappedShort() ?? default;

        public ushort GetUShort(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedUShort)?.Value.AsWrappedUShort() ?? default;

        public int GetInt(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedInt)?.Value.AsWrappedInt() ?? default;

        public uint GetUInt(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedUInt)?.Value.AsWrappedUInt() ?? default;

        public long GetLong(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedLong)?.Value.AsWrappedLong() ?? default;

        public ulong GetULong(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedULong)?.Value.AsWrappedULong() ?? default;

        public float GetFloat(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedFloat)?.Value.AsWrappedFloat() ?? default;

        public double GetDouble(string key)
            => _GetChecked(key, ClientSimPlayerDataType.WrappedDouble)?.Value.AsWrappedDouble() ?? default;

        public Vector2 GetVector2(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Vector2);
            if (data != null)
                return new Vector2(data.Value.AsVector2().x, data.Value.AsVector2().y);
            return default;
        }

        public Vector3 GetVector3(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Vector3);
            if (data != null)
                return new Vector3(data.Value.AsVector3().x, data.Value.AsVector3().y, data.Value.AsVector3().z);
            return default;
        }

        public Vector4 GetVector4(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Vector4);
            if (data != null)
                return new Vector4(data.Value.AsVector4().x, data.Value.AsVector4().y, data.Value.AsVector4().z, data.Value.AsVector4().w);
            return default;
        }

        public Quaternion GetQuaternion(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Quaternion);
            if (data != null)
                return new Quaternion(data.Value.AsQuaternion().x, data.Value.AsQuaternion().y, data.Value.AsQuaternion().z, data.Value.AsQuaternion().w);
            return default;
        }

        public Color GetColor(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Color);
            if (data != null)
                return new Color(data.Value.AsColor().r, data.Value.AsColor().g, data.Value.AsColor().b, data.Value.AsColor().a);
            return default;
        }

        public Color32 GetColor32(string key)
        {
            ClientSimPlayerDataPair data = _GetChecked(key, ClientSimPlayerDataType.Color32);
            if (data != null)
                return new Color32(data.Value.AsColor32().r, data.Value.AsColor32().g, data.Value.AsColor32().b, data.Value.AsColor32().a);
            return default;
        }

#endregion

#region DataPropagation

        // can't decode json here because this is called before scene manager is ready
        public void Init(VRCPlayerApi player, IClientSimUdonEventSender udonEventSender, IClientSimEventDispatcher eventDispatcher)
        {
            _player = player;
            _udonEventSender = udonEventSender;
            _eventDispatcher = eventDispatcher;

            _eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Subscribe<ClientSimOnPlayerDataClearedEvent>(OnPlayerDataCleared);
            _eventDispatcher.Subscribe<ClientSimOnPlayerRestoredEvent>(OnPlayerRestored);
        }

        private void OnDestroy()
        {
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerDataClearedEvent>(OnPlayerDataCleared);
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerRestoredEvent>(OnPlayerRestored);
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent payload)
        {
            // remote player test data is decoded via ClientSimPlayerDataWindow.OnPlayerJoined
            if (payload.player.playerId == _player.playerId)
                doDecode = true;
        }

        private void OnPlayerDataCleared(ClientSimOnPlayerDataClearedEvent payload)
        {
            leData.Clear();
        }
        
        private void OnPlayerRestored(ClientSimOnPlayerRestoredEvent payload)
        {
            if (payload.player.isLocal)
                hasPostedPlayerRestored = true;
        }

        private void Encode()
        {
            // PlayerData updates before OnPlayerRestored are ignored 
            if (!hasPostedPlayerRestored)
                return;
            
            string json = JsonConvert.SerializeObject(leData, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            File.WriteAllText(PlayerDataFilePath(_player), json);
        }
        
        internal void Decode(bool isRestore)
        {
            string path = PlayerDataFilePath(_player);
            
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}");
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, ClientSimPlayerDataPair>>(json);
                    foreach (KeyValuePair<string, ClientSimPlayerDataPair> kvp in data)
                    {
                        Set(kvp.Value);
                    }

                    FlushLocalInfoChanges(); // bulk flush changes
                }
                catch (Exception e)
                {
                    this.LogError($"Error initializing PlayerData: {e.Message}");
                }
            }

            isDoneDecoding = true;
            
            PlayerData.State Set(ClientSimPlayerDataPair pair)
            {
                try
                {
                    switch (pair.Value.Type)
                    {
                        case ClientSimPlayerDataType.Color: return SetColor(pair.Key, pair.Value.AsColor(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.Color32: return SetColor32(pair.Key, pair.Value.AsColor32(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.Quaternion: return SetQuaternion(pair.Key, pair.Value.AsQuaternion(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.Vector2: return SetVector2(pair.Key, pair.Value.AsVector2(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.Vector3: return SetVector3(pair.Key, pair.Value.AsVector3(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.Vector4: return SetVector4(pair.Key, pair.Value.AsVector4(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedBool: return SetBool(pair.Key, pair.Value.AsWrappedBool(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedByte: return SetSByte(pair.Key, pair.Value.AsWrappedSByte(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedUByte: return SetByte(pair.Key, pair.Value.AsWrappedByte(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedBytes: return SetBytes(pair.Key, pair.Value.AsWrappedBytes(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedFloat: return SetFloat(pair.Key, pair.Value.AsWrappedFloat(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedDouble: return SetDouble(pair.Key, pair.Value.AsWrappedDouble(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedLong: return SetLong(pair.Key, pair.Value.AsWrappedLong(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedULong: return SetULong(pair.Key, pair.Value.AsWrappedULong(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedInt: return SetInt(pair.Key, pair.Value.AsWrappedInt(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedUInt: return SetUInt(pair.Key, pair.Value.AsWrappedUInt(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedShort: return SetShort(pair.Key, pair.Value.AsWrappedShort(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedUShort: return SetUShort(pair.Key, pair.Value.AsWrappedUShort(), pair.LastUpdated, isRestore, false);
                        case ClientSimPlayerDataType.WrappedString: return SetString(pair.Key, pair.Value.AsWrappedString(), pair.LastUpdated, isRestore, false);
                        
                        default:
                        case ClientSimPlayerDataType.None: return PlayerData.State.Unchanged;
                    }
                }
                catch (Exception e)
                {
                    this.LogError("Error reading PlayerData: " +
                                  $"key={pair.Key}, " +
                                  $"type={pair.Value.Type} ({pair.Value.Value.GetType()}), " +
                                  $"value={pair.Value.Value}, " +
                                  $"error={e.Message}");
                    return default;
                }
            }
        }
        
        private void FlushLocalInfoChanges()
        {
            var infos = leData
                .Select(kvp =>
                    localInfoChanges.TryGetValue(kvp.Key, out var value)
                        ? value
                        : new PlayerData.Info(kvp.Key, PlayerData.State.Unchanged))
                .ToArray();

            QueuePlayerDataUpdate(infos);
            
            localInfoChanges.Clear();
        }
        
        internal void QueuePlayerDataUpdate(PlayerData.Info[] infos)
        {
            if (infos.Length == 0) 
                return;

            queuedPlayerDataUpdates.AddRange(infos);
        }

        private void RaisePlayerDataUpdated(PlayerData.Info[] infos)
        {
            _udonEventSender.RunEvent(UdonManager.UDON_EVENT_ONPLAYERDATAUPDATED, ("player", _player), ("infos", infos));
            _eventDispatcher.SendEvent(new ClientSimOnPlayerDataUpdatedEvent
            {
                player = _player,
                playerData = leData
            });
        }

        private void LateUpdate()
        {
            if (queuedPlayerDataUpdates.Count > 0)
            {
                Encode();
                RaisePlayerDataUpdated(queuedPlayerDataUpdates.ToArray());
                queuedPlayerDataUpdates.Clear();
            }

            if (doDecode)
            {
                doDecode = false;
                Decode(true);
            }
            else if (isDoneDecoding && !hasPostedPlayerDataDecoded)
            {
                hasPostedPlayerDataDecoded = true;
                _eventDispatcher.SendEvent(new ClientSimOnPlayerDataDecodedEvent { player = _player });
            }
        }
        
#endregion

#endif
    }
}
