#if VRC_ENABLE_PLAYER_PERSISTENCE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Persistence;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using Random = UnityEngine.Random;

namespace VRC.SDK3.ClientSim.Editor
{
    public class ClientSimPlayerDataWindow : EditorWindow
    {
        // max number of characters to display PlayerData values with (value strings longer than this are truncated)
        private const int MAX_LENGTH_SINGLE_LINE = 50;
        private const int MAX_LENGTH_MULTI_LINE = 10000;

        // number of PlayerData elements to show per page 
        private const int PAGE_SIZE = 12;

        // key used to store selected sort mode in player prefs
        private const string SORT_MODE_KEY = "PlayerDataSortMode";
        
        private bool hasLocalPlayerData;
        private bool isLocalPlayerSelected = true;
        private string localPlayerName;
        private int page;
        private int numPages;
        
        private Dictionary<string, ClientSimPlayerDataPair> localPlayerData;
        private IClientSimEventDispatcher eventDispatcher;
        private VisualElement labelContainer;
        private VisualElement noDataInfoContainer;
        private VisualElement pagingContainer;
        private Label pageLabel;
        private readonly List<VisualElement> elementsToDisableInPlayMode = new();
        private DropdownField playerDropdown;
        private DropdownField sortDropdown;
        private Button addRemotePlayerTestDataButton;
        private Button nextPageButton;
        private Button prevPageButton;
        private Button clearDataButton;
        
        private static string LocalPlayerDataRoot
        {
            get
            {
                string root = Path.GetDirectoryName(Application.dataPath);
                string path = Path.Combine(root, ClientSimPlayerDataStorage.PlayerDataFolder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        private static string LocalPlayerDataPath => LocalPlayerDataRoot + $"/PlayerData_1_{SceneManager.GetActiveScene().name}.json";
        private static string LocalDataPathPattern => $"PlayerData_*_{SceneManager.GetActiveScene().name}.json";
        
        [MenuItem("VRChat SDK/ClientSim PlayerData", false, 1500)]
        public static void Init()
        {
            var window = GetWindow<ClientSimPlayerDataWindow>(false, "ClientSim PlayerData");
            window.minSize = new Vector2(400, 400);
            window.Show();
        }
        
        private static void SetData(ClientSimPlayerDataPair data, VRCPlayerApi player)
        {
            var playerData = player.GetClientSimPlayer().PlayerDataObject;
            switch (data.Value.Type)
            {
                case ClientSimPlayerDataType.Vector2:
                    playerData.SetVector2(data.Key, data.Value.AsVector2());
                    break;
                case ClientSimPlayerDataType.Vector3:
                    playerData.SetVector3(data.Key, data.Value.AsVector3());
                    break;
                case ClientSimPlayerDataType.Vector4:
                    playerData.SetVector4(data.Key, data.Value.AsVector4());
                    break;
                case ClientSimPlayerDataType.Quaternion:
                    playerData.SetQuaternion(data.Key, data.Value.AsQuaternion());
                    break;
                case ClientSimPlayerDataType.Color:
                    playerData.SetColor(data.Key, data.Value.AsColor()); 
                    break;
                case ClientSimPlayerDataType.Color32:
                    playerData.SetColor32(data.Key, data.Value.AsColor32());
                    break;
                case ClientSimPlayerDataType.WrappedString:
                    playerData.SetString(data.Key, data.Value.AsWrappedString());
                    break;
                case ClientSimPlayerDataType.WrappedShort:
                    playerData.SetShort(data.Key, data.Value.AsWrappedShort());
                    break;
                case ClientSimPlayerDataType.WrappedInt:
                    playerData.SetInt(data.Key, data.Value.AsWrappedInt());
                    break;
                case ClientSimPlayerDataType.WrappedFloat:
                    playerData.SetFloat(data.Key, data.Value.AsWrappedFloat());
                    break;
                case ClientSimPlayerDataType.WrappedBool:
                    playerData.SetBool(data.Key, data.Value.AsWrappedBool());
                    break;
                case ClientSimPlayerDataType.WrappedByte:
                    playerData.SetSByte(data.Key, data.Value.AsWrappedSByte());
                    break;
                case ClientSimPlayerDataType.WrappedUByte:
                    playerData.SetByte(data.Key, data.Value.AsWrappedUByte());
                    break;
                case ClientSimPlayerDataType.WrappedBytes:
                    playerData.SetBytes(data.Key, data.Value.AsWrappedBytes());
                    break;
                case ClientSimPlayerDataType.WrappedUShort:
                    playerData.SetUShort(data.Key, data.Value.AsWrappedUShort());
                    break;
                case ClientSimPlayerDataType.WrappedUInt:
                    playerData.SetUInt(data.Key, data.Value.AsWrappedUInt());
                    break;
                case ClientSimPlayerDataType.WrappedULong:
                    playerData.SetULong(data.Key, data.Value.AsWrappedULong());
                    break;
                case ClientSimPlayerDataType.WrappedDouble:
                    playerData.SetDouble(data.Key, data.Value.AsWrappedDouble());
                    break;
                case ClientSimPlayerDataType.WrappedLong:
                    playerData.SetLong(data.Key, data.Value.AsWrappedLong());
                    break;
            }
        }

        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>(nameof(ClientSimPlayerDataWindow));
            root.Add(visualTree.CloneTree());

            Button openDataFolderButton = root.Q<Button>("OpenDataFolderButton");
            Button refreshDataButton = root.Q<Button>("RefreshDataButton");
            clearDataButton = root.Q<Button>("ClearDataButton");
            nextPageButton = root.Q<Button>("NextPageButton");
            prevPageButton = root.Q<Button>("PreviousPageButton");
            addRemotePlayerTestDataButton = root.Q<Button>("RandomizeButton");
            playerDropdown = root.Q<DropdownField>("PlayerDropdown");
            sortDropdown = root.Q<DropdownField>("SortDropdown");
            noDataInfoContainer = root.Q<VisualElement>("NoDataInfo");
            pagingContainer = root.Q<VisualElement>("Paging");
            pageLabel = root.Q<Label>("PageLabel");
            ScrollView playerDataList = root.Q<ScrollView>("PlayerDataList");

            labelContainer = new VisualElement();
            playerDataList.Add(labelContainer);

            openDataFolderButton.clicked += OpenLocalDataDirectory;
            refreshDataButton.clicked += RefreshPlayerData;
            clearDataButton.clicked += ClearPlayerData;
            nextPageButton.clicked += NextPage;
            prevPageButton.clicked += PreviousPage;
            addRemotePlayerTestDataButton.clicked += AddRemotePlayerTestData;
            
            // dropdown always includes local player so that users can view PlayerData outside of play mode
            CheckLocalPlayerName();
            playerDropdown.choices = new List<string> { localPlayerName };
            playerDropdown.index = 0;
            playerDropdown.RegisterValueChangedCallback(OnPlayerSelected);
            
            // load sort mode from player prefs - it will be applied in LoadPlayerData
            sortDropdown.SetValueWithoutNotify(PlayerPrefs.GetString(SORT_MODE_KEY, "Alphabetically"));
            sortDropdown.RegisterValueChangedCallback(OnSortSelected);
            
            LoadPlayerData();
            UpdatePage(false);
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (EditorApplication.isPlaying)
            {
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            }
            
            // reload data on scene switch
            EditorSceneManager.sceneOpened += (_, _) => LoadPlayerData();
        }

        private void AddRemotePlayerTestData()
        {
            object RandomizePlayerDataValue(ClientSimPlayerDataTypeUnion dataPoint)
            {
                return dataPoint.Type switch
                {
                    ClientSimPlayerDataType.Vector2 => new Vector2(Random.Range(0,100), Random.Range(0,100)),
                    ClientSimPlayerDataType.Vector3 => new Vector3(Random.Range(0,100), Random.Range(0,100), Random.Range(0,100)),
                    ClientSimPlayerDataType.Vector4 => new Vector4(Random.Range(0,100), Random.Range(0,100), Random.Range(0,100), Random.Range(0,100)),
                    ClientSimPlayerDataType.Quaternion => new Quaternion(Random.Range(0,100), Random.Range(0,100), Random.Range(0,100), Random.Range(0,100)),
                    ClientSimPlayerDataType.Color => new Color(Random.Range(0,1f), Random.Range(0,1f), Random.Range(0,1f), Random.Range(0,1f)),
                    ClientSimPlayerDataType.Color32 => (Color32)new Color(Random.Range(0,1f), Random.Range(0,1f), Random.Range(0,1f), Random.Range(0,1f)),
                    ClientSimPlayerDataType.WrappedString => MakeRandomString(),
                    ClientSimPlayerDataType.WrappedShort => (short)Random.Range(-10, 10),
                    ClientSimPlayerDataType.WrappedInt => Random.Range(-100, 100),
                    ClientSimPlayerDataType.WrappedFloat => Random.Range(0, 1f),
                    ClientSimPlayerDataType.WrappedBool => Random.Range(0, 2) != 0,
                    ClientSimPlayerDataType.WrappedByte => (sbyte)Random.Range(sbyte.MinValue, sbyte.MaxValue),
                    ClientSimPlayerDataType.WrappedUByte => (byte)Random.Range(byte.MinValue, byte.MaxValue),
                    ClientSimPlayerDataType.WrappedBytes => MakeRandomByteArray(),
                    ClientSimPlayerDataType.WrappedUShort => (ushort)Random.Range(0, 10),
                    ClientSimPlayerDataType.WrappedUInt => (uint)Random.Range(0, 100),
                    ClientSimPlayerDataType.WrappedULong => (ulong)Random.Range(0, 1000),
                    ClientSimPlayerDataType.WrappedDouble => (double)Random.Range(0, 0.1f),
                    ClientSimPlayerDataType.WrappedLong => (long)Random.Range(-1000, 1000),
                    _ => default
                };
            }

            string MakeRandomString()
            {
                const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string result = "";
                for (int i = 0; i < 10; i++)
                {
                    result += characters[Random.Range(0, characters.Length)];
                }
                return result;
            }

            byte[] MakeRandomByteArray()
            {
                byte[] randomBytes = new byte[2];
                for (int i = 0; i < randomBytes.Length; i++)
                {
                    randomBytes[i] = (byte)Random.Range(byte.MinValue, byte.MaxValue + 1);
                }
                return randomBytes;
            }

            string playerName = playerDropdown.value;
            VRCPlayerApi player = VRCPlayerApi.AllPlayers.Find(p => p.displayName.Equals(playerName));
            if (player.isLocal) return;
            
            foreach (var kvp in localPlayerData)
            {
                var randomizedValue = RandomizePlayerDataValue(kvp.Value.Value);
                var typeUnion = new ClientSimPlayerDataTypeUnion
                {
                    Type = kvp.Value.Value.Type,
                    Value = randomizedValue
                };
                var dataPair = new ClientSimPlayerDataPair
                {
                    Key = kvp.Key,
                    Value = typeUnion
                };
                SetData(dataPair, player);
            }
        }
        
        private void OnPlayerSelected(ChangeEvent<string> playerSelectedEvent)
        {
            string selectedPlayerName = playerSelectedEvent.newValue;
            bool isLocalPlayer = string.Equals(selectedPlayerName, localPlayerName);
            addRemotePlayerTestDataButton.style.display = isLocalPlayer || !hasLocalPlayerData 
                ? DisplayStyle.None 
                : DisplayStyle.Flex;

            isLocalPlayerSelected = isLocalPlayer;
            if (isLocalPlayer)
            {
                LoadPlayerData(Networking.LocalPlayer);
                return;
            }

            foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
            {
                if (player.displayName.Equals(selectedPlayerName))
                {
                    LoadPlayerData(player);
                    break;
                }
            }
        }

        private void OnSortSelected(ChangeEvent<string> evt)
        {
            PlayerPrefs.SetString(SORT_MODE_KEY, evt.newValue);
            LoadPlayerDataForSelectedPlayer();   
        }
        
        private void LoadPlayerDataForSelectedPlayer()
        {
            if (!EditorApplication.isPlaying)
            {
                LoadPlayerData();
                return;
            }
            
            string selectedPlayerName = playerDropdown.value;
            foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
            {
                if (player.displayName.Equals(selectedPlayerName))
                {
                    LoadPlayerData(player);
                    break;
                }
            }
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void LoadPlayerData(VRCPlayerApi player = null, bool broadcastPlayerDataUpdated = false)
        {
            // outside of play mode, we don't have a player game object
            string path = player == null ? LocalPlayerDataPath : ClientSimPlayerDataStorage.PlayerDataFilePath(player);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}");
                CheckDataEmpty();
            }
            else
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<Dictionary<string, ClientSimPlayerDataPair>>(json);
                UpdatePlayerDataList(data, player);

                // when adding test data for a remote player, let the rest of the system know
                if (EditorApplication.isPlaying && broadcastPlayerDataUpdated)
                {
                    var infos = data
                        .Select(kvp => new PlayerData.Info(kvp.Key, PlayerData.State.Added))
                        .ToArray();
                    
                    player.GetClientSimPlayer().PlayerDataObject.QueuePlayerDataUpdate(infos);
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                eventDispatcher = ClientSimMain.GetInstance().GetEventDispatcher();
                eventDispatcher.Subscribe<ClientSimOnPlayerRestoredEvent>(OnPlayerRestored);
                eventDispatcher.Subscribe<ClientSimOnPlayerDataUpdatedEvent>(OnPlayerDataUpdated);
                eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
                eventDispatcher.Subscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);

                CheckLocalPlayerName();
                playerDropdown.choices[0] = localPlayerName;
                playerDropdown.value = localPlayerName;
                
                elementsToDisableInPlayMode.ForEach(e => e.SetEnabled(false));
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                playerDropdown.choices.RemoveRange(1, playerDropdown.choices.Count - 1);
                playerDropdown.index = 0;

                eventDispatcher?.Unsubscribe<ClientSimOnPlayerRestoredEvent>(OnPlayerRestored);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerDataUpdatedEvent>(OnPlayerDataUpdated);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
                eventDispatcher = null;
                
                elementsToDisableInPlayMode.ForEach(e => e.SetEnabled(true));
            }
        }

        private void OnPlayerRestored(ClientSimOnPlayerRestoredEvent payload)
        {
            LoadPlayerData(payload.player);
        }
        
        private void OnPlayerDataUpdated(ClientSimOnPlayerDataUpdatedEvent payload)
        {
            UpdatePlayerDataList(payload.playerData, payload.player);
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent payload)
        {
            // local player is initialized via ClientSim init flow
            if (payload.player.isLocal) return;
            
            if (!playerDropdown.choices.Contains(payload.player.displayName))
            {
                playerDropdown.choices.Add(payload.player.displayName);
            }
        }

        private void OnPlayerLeft(ClientSimOnPlayerLeftEvent payload)
        {
            if (payload.player.isLocal) return;
            if (playerDropdown.choices.Contains(payload.player.displayName))
            {
                playerDropdown.choices.Remove(payload.player.displayName);
            }

            if (playerDropdown.value.Equals(payload.player.displayName))
            {
                playerDropdown.index = 0;
            }
        }

        private void UpdatePlayerDataList(Dictionary<string, ClientSimPlayerDataPair> playerData, VRCPlayerApi player = null, bool redraw = true)
        {
            numPages = playerData.Count % PAGE_SIZE == 0
                ? playerData.Count / PAGE_SIZE
                : playerData.Count / PAGE_SIZE + 1;
            UpdatePage(false);
            
            if (player != null)
            {
                // show button for adding test data if local player has data and a remote player is selected
                if (player.isLocal)
                {
                    localPlayerData = playerData;
                    if (!hasLocalPlayerData && localPlayerData.Count > 0)
                    {
                        hasLocalPlayerData = true;
                        if (!isLocalPlayerSelected)
                        {
                            addRemotePlayerTestDataButton.style.display = DisplayStyle.Flex;
                        }
                    }
                }

                // only update display if data is updated for currently selected player
                if (!string.Equals(player.displayName, playerDropdown.value)) return;
            }

            if (!redraw || CheckDataEmpty(playerData.Count))
            {
                return;
            }

            var sorted = sortDropdown.value switch
            {
                // sorts by key
                "Alphabetically" => playerData.OrderBy(x => x.Key).ToList(),

                // sorts by what keys were most recently set
                "Last Updated" => playerData
                    .OrderBy(x => x.Value.LastUpdated)
                    .ThenBy(x => x.Key)
                    .Reverse()
                    .ToList(),
                
                // sorts data as shown in the JSON
                _ => playerData.ToList()

            };

            // color fields don't have an isDelayed property, so we disable them in play mode 
            elementsToDisableInPlayMode.Clear();

            int pageIndex = 0;
            int labelIndex = 0;
            foreach (var kvp in sorted)
            {
                // paging
                if (labelIndex >= PAGE_SIZE)
                {
                    labelIndex = 0;
                    pageIndex++;
                }
                labelIndex++;
                if (pageIndex != page) continue;
                
                VisualElement valueField;
                switch (kvp.Value.Value.Type)
                {
                    case ClientSimPlayerDataType.Vector2:
                        Vector2Field vector2Field = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsVector2()
                        };
                        SetIsDelayedForCompositeField(vector2Field);
                        vector2Field.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = vector2Field;
                        break;
                    
                    case ClientSimPlayerDataType.Vector3:
                        Vector3Field vector3Field = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsVector3()
                        };
                        SetIsDelayedForCompositeField(vector3Field);
                        vector3Field.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = vector3Field;
                        break;
                    
                    case ClientSimPlayerDataType.Vector4:
                        Vector4Field vector4Field = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsVector4()
                        };
                        SetIsDelayedForCompositeField(vector4Field);
                        vector4Field.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = vector4Field;
                        break;
                    
                    // show quaternions as euler angles, so they're more intuitive to edit
                    case ClientSimPlayerDataType.Quaternion:
                        Vector3Field quaternionField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsQuaternion().eulerAngles
                        };
                        SetIsDelayedForCompositeField(quaternionField);
                        quaternionField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = quaternionField;
                        break;
                    
                    case ClientSimPlayerDataType.Color:
                        ColorField colorField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsColor()
                        };
                        colorField.RegisterValueChangedCallback(ValueFieldCallback);
                        colorField.SetEnabled(!EditorApplication.isPlaying);
                        elementsToDisableInPlayMode.Add(colorField);
                        valueField = colorField;
                        break;
                    
                    // color32 is shown as a regular color field
                    case ClientSimPlayerDataType.Color32:
                        ColorField color32Field = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsColor32()
                        };
                        color32Field.RegisterValueChangedCallback(ValueFieldCallback);
                        color32Field.SetEnabled(!EditorApplication.isPlaying);
                        elementsToDisableInPlayMode.Add(color32Field);
                        valueField = color32Field;
                        break;
                    
                    // string fields are multiline
                    case ClientSimPlayerDataType.WrappedString:
                        TextField textField = new()
                        {
                            label = kvp.Key, 
                            value = Truncate(true, kvp.Value.Value.AsWrappedString()),
                            isDelayed = true,
                            multiline = true,
                            maxLength = MAX_LENGTH_MULTI_LINE
                        };
                        textField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = textField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedShort:
                        IntegerField shortField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedShort(), 
                            isDelayed = true
                        };
                        shortField.RegisterValueChangedCallback(e =>
                        {
                            if (e.newValue is >= short.MinValue and <= short.MaxValue)
                            {
                                ValueFieldCallback(e);
                            }
                            else
                            {
                                shortField.SetValueWithoutNotify(e.previousValue);
                            }
                        });
                        valueField = shortField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedInt:
                        IntegerField intField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedInt(), 
                            isDelayed = true
                        };
                        intField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = intField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedFloat:
                        FloatField floatField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedFloat(), 
                            isDelayed = true
                        };
                        floatField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = floatField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedBool:
                        Toggle toggle = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedBool()
                        };
                        toggle.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = toggle;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedByte:
                        IntegerField sbyteField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedSByte(), 
                            isDelayed = true
                        };
                        sbyteField.RegisterValueChangedCallback(e =>
                        {
                            if (e.newValue is >= sbyte.MinValue and <= sbyte.MaxValue)
                            {
                                ValueFieldCallback(e);
                            }
                            else
                            {
                                sbyteField.SetValueWithoutNotify(e.previousValue);
                            }
                        });
                        valueField = sbyteField;
                        break;
                    
                    // byte arrays are readonly to avoid confusion about encoding and format
                    // users can still manually set byte array values in the JSON if needed
                    case ClientSimPlayerDataType.WrappedBytes:
                        var byteArray = kvp.Value.Value.AsWrappedBytes();
                        string readableValue = byteArray.Aggregate("[ ", (current, b) => current + $"{(int)b} ") + "]";
                        TextField bytesField = new()
                        {
                            label = kvp.Key, 
                            value = Truncate(false, readableValue), 
                            isDelayed = true,
                            maxLength = MAX_LENGTH_SINGLE_LINE
                        };
                        bytesField.SetEnabled(false);
                        valueField = bytesField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedUShort:
                        UnsignedIntegerField ushortField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedUShort(), 
                            isDelayed = true,
                        };
                        ushortField.RegisterValueChangedCallback(e =>
                        {
                            if (e.newValue <= ushort.MaxValue)
                            {
                                ValueFieldCallback(e);
                            }
                            else
                            {
                                ushortField.SetValueWithoutNotify(e.previousValue);
                            }
                        });
                        valueField = ushortField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedUByte:
                        UnsignedIntegerField byteField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedUByte(), 
                            isDelayed = true,
                        };
                        byteField.RegisterValueChangedCallback(e =>
                        {
                            if (e.newValue <= byte.MaxValue)
                            {
                                ValueFieldCallback(e);
                            }
                            else
                            {
                                byteField.SetValueWithoutNotify(e.previousValue);
                            }
                        });
                        valueField = byteField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedUInt:
                        UnsignedIntegerField uintField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedUInt(), 
                            isDelayed = true,
                        };
                        uintField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = uintField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedULong:
                        UnsignedLongField ulongField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedULong(), 
                            isDelayed = true,
                        };
                        ulongField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = ulongField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedDouble:
                        DoubleField doubleField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedDouble(), 
                            isDelayed = true,
                        };
                        doubleField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = doubleField;
                        break;
                    
                    case ClientSimPlayerDataType.WrappedLong:
                        LongField longField = new()
                        {
                            label = kvp.Key, 
                            value = kvp.Value.Value.AsWrappedLong(), 
                            isDelayed = true,
                        };
                        longField.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = longField;
                        break;
                    
                    default:
                        TextField field = new()
                        {
                            label = kvp.Key, 
                            value = Truncate(false, kvp.Value.Value.Value.ToString()),
                            isDelayed = true,
                            maxLength = MAX_LENGTH_SINGLE_LINE
                        };
                        field.RegisterValueChangedCallback(ValueFieldCallback);
                        valueField = field;
                        break;
                }
                
                labelContainer.Add(valueField);
                continue;

                // truncate a value
                string Truncate(bool isMultiline, string value)
                {
                    const string end = "... (truncated)";
                    int maxLength = isMultiline ? MAX_LENGTH_MULTI_LINE : MAX_LENGTH_SINGLE_LINE;
                    return value.Length > maxLength 
                        ? $"{value.Truncate(maxLength - end.Length)}{end}" 
                        : value;
                }
                
                // in play mode, set value through PlayerData interface
                // in edit mode, update the JSON directly
                void ValueFieldCallback<T>(ChangeEvent<T> evt)
                {
                    ClientSimPlayerDataTypeUnion typeUnion = new ClientSimPlayerDataTypeUnion
                    {
                        Type = kvp.Value.Value.Type,
                        Value = evt.newValue
                    };
                    
                    // quaternions are displayed as Vector3 eulers
                    if (kvp.Value.Value.Type == ClientSimPlayerDataType.Quaternion && evt.newValue is Vector3 eulers)
                    {
                        typeUnion.Value = Quaternion.Euler(eulers);
                    }
                    
                    // color32s are displayed as colors
                    if (kvp.Value.Value.Type == ClientSimPlayerDataType.Color32 && evt.newValue is Color color)
                    {
                        typeUnion.Value = (Color32)color;
                    }

                    // bytes are displayed ints
                    if (kvp.Value.Value.Type == ClientSimPlayerDataType.WrappedUByte)
                    {
                        typeUnion.Value = Convert.ToByte(evt.newValue);
                    }
                    if (kvp.Value.Value.Type == ClientSimPlayerDataType.WrappedByte)
                    {
                        typeUnion.Value = Convert.ToSByte(evt.newValue);
                    }
                    
                    ClientSimPlayerDataPair dataPair = new() { Key = kvp.Key, Value = typeUnion };
                    
                    if (EditorApplication.isPlaying)
                    {
                        SetData(dataPair, player);
                    }
                    
                    else
                    {
                        string json = File.ReadAllText(LocalPlayerDataPath);
                        var data = JsonConvert.DeserializeObject<Dictionary<string, ClientSimPlayerDataPair>>(json);
                        data[kvp.Key] = dataPair;
                        json = JsonConvert.SerializeObject(data, Formatting.Indented);
                        File.WriteAllText(LocalPlayerDataPath, json);
                        
                        // After serializing the new value, reload it into ClientSim PlayerData and refresh the UI.
                        
                        // Note: Color fields lack an isDelayed property and serialize every frame during editing.
                        // Rebuilding the UI while a User edits a color will break the field's update loop.
                        // To avoid this, we update ClientSim PlayerData directly and skip the UI rebuild.
                        // If the user has sorted fields by "Last Updated", the color field will not sort to the top from this.
                        if (kvp.Value.Value.Type is ClientSimPlayerDataType.Color or ClientSimPlayerDataType.Color32)
                        {
                            UpdatePlayerDataList(data, player, false);
                        }
                        else
                        {
                            UpdatePlayerDataList(data, player);
                        }
                    }
                }

                // is there a better way to find all FloatField children and their immediate parent? 
                void SetIsDelayedForCompositeField(VisualElement compositeField)
                {
                    VisualElement container = compositeField.contentContainer.Children().ToList()[1];
                    container.style.flexShrink = 1;
                    foreach (var child in container.Children())
                    {
                        if (child is FloatField f)
                        {
                            f.isDelayed = true;
                        }
                    }
                }
            }
        }

        private void RefreshPlayerData()
        {
            if (!EditorApplication.isPlaying)
            {
                LoadPlayerData();
            }
            else
            {
                foreach (var player in VRCPlayerApi.AllPlayers)
                {
                    player.GetClientSimPlayer().PlayerDataObject.Decode(false);
                }
            }
        }
        
        private static void OpenLocalDataDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", LocalPlayerDataRoot.Replace("/", "\\"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", LocalPlayerDataRoot);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", LocalPlayerDataRoot);
            }
        }
        
        private void ClearPlayerData()
        {
            StringBuilder clearedFiles = new StringBuilder("Cleared PlayerData files:");
            string[] playerDataFiles = Directory.GetFiles(LocalPlayerDataRoot, LocalDataPathPattern);
            foreach (var file in playerDataFiles)
            {
                File.Delete(file);
                clearedFiles.Append($"\n\t{file}");

                // in play mode, let other systems know PlayerData was cleared
                if (EditorApplication.isPlaying)
                {
                    string playerId = file.Split("_")[1].Split(".")[0];
                    int id = int.Parse(playerId);
                    var player = VRCPlayerApi.GetPlayerById(id);
                    {
                        eventDispatcher.SendEvent(new ClientSimOnPlayerDataClearedEvent { player = player });
                    }
                }

                hasLocalPlayerData = false;
                if (!isLocalPlayerSelected)
                {
                    addRemotePlayerTestDataButton.style.display = DisplayStyle.None;
                }
                
                CheckDataEmpty();
            }

            page = 0;
            numPages = 0;
            UpdatePage();

            if (playerDataFiles.Length > 0)
            {
                this.Log(clearedFiles.ToString());
            }
        }

        private void NextPage()
        {
            page++;
            UpdatePage();
        }

        private void PreviousPage()
        {
            page--;
            UpdatePage();   
        }

        private void UpdatePage(bool reload = true)
        { 
            bool isFirstPage = page == 0;
            bool isLastPage = page == numPages - 1;

            prevPageButton.SetEnabled(!isFirstPage);
            nextPageButton.SetEnabled(!isLastPage);

            pageLabel.text = $"{page+1} / {numPages}";
            if (reload)
            {
                LoadPlayerDataForSelectedPlayer();
            }
        }

        private bool CheckDataEmpty(int numPlayerData = 0)
        {
            bool isEmpty = numPlayerData == 0;
            clearDataButton.SetEnabled(!isEmpty);
            noDataInfoContainer.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            sortDropdown.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            pagingContainer.style.display = numPlayerData > PAGE_SIZE ? DisplayStyle.Flex : DisplayStyle.None;
            labelContainer.Clear();
            return isEmpty;
        }

        private void CheckLocalPlayerName()
        {
            localPlayerName = string.IsNullOrEmpty(ClientSimSettings.Instance.customLocalPlayerName)
                ? "[1] Local Player"
                : ClientSimSettings.Instance.customLocalPlayerName;
        }
    }
}
#endif