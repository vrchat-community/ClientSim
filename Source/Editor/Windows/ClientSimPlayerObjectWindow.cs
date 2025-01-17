using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Editor
{
    #if VRC_ENABLE_PLAYER_PERSISTENCE
    public class ClientSimPlayerObjectWindow : EditorWindow
    {
        
        private Dictionary<int, List<PlayerObjectData>> playerObjects = new Dictionary<int, List<PlayerObjectData>>();
        private List<ClientSimNetworkHolderInstanceElement> playerObjectVisualElements = new List<ClientSimNetworkHolderInstanceElement>();
        private List<VRCPlayerApi> players = new List<VRCPlayerApi>();
        private IClientSimEventDispatcher eventDispatcher;
        
        private DropdownField playerDropdown;
        private DropdownField Sort;
        private Label pageLabel;
        private Button nextPage;
        private Button prevPage;
        private TextField searchField;
        private ScrollView playerDataList;
        private VisualElement HelpBox;
        private Label HelpBoxLabel;
        
        private int CurrentIndex = 0;
        private int CurrentSelectedPlayerID = -1;
        private static int CountPerPage = 10;
        private static string HelpTextEnterPlayMode = "Enter Play Mode to view PlayerObjects";
        
        private enum SortingMode
        {
            Alphabetical,
            LastModified
        }
        
        private SortingMode _sortingMode = SortingMode.Alphabetical;
        
        [MenuItem("VRChat SDK/ClientSim PlayerObjects", false, 1500)]
        public static void Init()
        {
            var window = GetWindow<ClientSimPlayerObjectWindow>(false, "ClientSim PlayerObjects");
            window.minSize = new Vector2(400, 400);
            window.Show();
        }

        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>(nameof(ClientSimPlayerObjectWindow));
            visualTree.CloneTree(root);
            
            playerDropdown = root.Q<DropdownField>("PlayerDropdown");
            playerDropdown.choices = new List<string>();
            playerDropdown.RegisterValueChangedCallback((evt) =>
            {
                CurrentSelectedPlayerID = players[playerDropdown.index].playerId;
                CurrentIndex = 0;
                pageLabel.text = "Page 0";
                UpdateCurrentPage(null);
            });
            
            Sort = root.Q<DropdownField>("Sort");
            Sort.choices = new List<string>()
            {
                "Alphabetical",
                "Last modified"
            };
            Sort.SetValueWithoutNotify("Alphabetical");
            
            Sort.RegisterValueChangedCallback(OnChangeSortingMode);
            _sortingMode = SortingMode.Alphabetical;
            
            searchField = root.Q<TextField>("Search");
            searchField.RegisterValueChangedCallback((evt) =>
            {
                UpdateCurrentPage(null);
            });
            
            playerDataList = root.Q<ScrollView>("PlayerObjectList");
            
            nextPage = root.Q<Button>("Right");
            nextPage.clicked += NextPage;
            nextPage.style.display = DisplayStyle.None;
            
            prevPage = root.Q<Button>("Left");
            prevPage.clicked += PreviousPage;
            prevPage.style.display = DisplayStyle.None;
            
            pageLabel = root.Q<Label>("PagingLabel");
            pageLabel.text = "Page 1";
            
            HelpBox = root.Q<VisualElement>("HelpTextBox");
            HelpBoxLabel = HelpBox.Q<Label>("HelpTextBoxLabel");
            HelpBoxLabel.text = HelpTextEnterPlayMode;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (EditorApplication.isPlaying)
            {
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            }
            
            VRCPlayerApi[] newplayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            newplayers = VRCPlayerApi.GetPlayers(newplayers);

            foreach (VRCPlayerApi player in newplayers)
            {
                ClientSimOnPlayerJoinedEvent playerJoinedEvent = new ClientSimOnPlayerJoinedEvent();
                playerJoinedEvent.player = player;
                OnPlayerJoined(playerJoinedEvent);
            }
            
            for(int i = 0; i < CountPerPage; i++)
            {
                ClientSimNetworkHolderInstanceElement element = new ClientSimNetworkHolderInstanceElement();
                playerObjectVisualElements.Add(element);
                playerDataList.Add(element);
                element.style.display = DisplayStyle.None;
            }
            
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                eventDispatcher = ClientSimMain.GetInstance().GetEventDispatcher();
                eventDispatcher.Subscribe<ClientSimOnPlayerObjectUpdatedEvent>(OnPlayerObjectUpdated);
                eventDispatcher.Subscribe<ClientSimOnPlayerObjectUpdateEndedEvent>(UpdateCurrentPage);
                eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
                eventDispatcher.Subscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
                
                nextPage.style.display = DisplayStyle.Flex;
                HelpBox.style.display = DisplayStyle.None;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                playerDropdown.choices.RemoveRange(1, playerDropdown.choices.Count - 1);
                playerDropdown.index = 0;
                
                playerDataList.Clear();
                CurrentIndex = 0;
                CurrentSelectedPlayerID = -1;
                
                nextPage.style.display = DisplayStyle.None;
                prevPage.style.display = DisplayStyle.None;
                
                HelpBox.style.display = DisplayStyle.Flex;

                eventDispatcher?.Unsubscribe<ClientSimOnPlayerObjectUpdatedEvent>(OnPlayerObjectUpdated);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerObjectUpdateEndedEvent>(UpdateCurrentPage);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
                eventDispatcher?.Unsubscribe<ClientSimOnPlayerLeftEvent>(OnPlayerLeft);
                eventDispatcher = null;
            }
        }

        private void OnChangeSortingMode(ChangeEvent<string> evt)
        {
            if (evt.newValue == "Alphabetical")
            {
                playerObjects[CurrentSelectedPlayerID].Sort( (x, y) => string.Compare(x.Name, y.Name) );
                _sortingMode = SortingMode.Alphabetical;
            }
            else if (evt.newValue == "Last modified")
            {
                _sortingMode = SortingMode.LastModified;
                playerObjects[CurrentSelectedPlayerID].Sort( (x, y) => x.lastModified.CompareTo(y.lastModified) );
            }
        }
        
        private void NextPage()
        {
            CurrentIndex += CountPerPage;
            pageLabel.text = "Page " + (CurrentIndex / CountPerPage + 1);
            if(CurrentIndex+CountPerPage > playerObjects[CurrentSelectedPlayerID].Count)
                nextPage.style.display = DisplayStyle.None;
            else
            {
                nextPage.style.display = DisplayStyle.Flex;
                prevPage.style.display = DisplayStyle.Flex;
            }
        }
        
        private void PreviousPage()
        {
            CurrentIndex -= CountPerPage;
            pageLabel.text = "Page " + (CurrentIndex / CountPerPage + 1);
            
            if(CurrentIndex <= 0)
                prevPage.style.display = DisplayStyle.None;
            else
            {
                prevPage.style.display = DisplayStyle.Flex;
                nextPage.style.display = DisplayStyle.Flex;
            }
        }

        private void OnPlayerObjectUpdated(ClientSimOnPlayerObjectUpdatedEvent payload)
        {
            int playerId = payload.Data.GetPlayerId();
            if(playerObjects.TryGetValue(playerId, out var instanceElements))
            {
                int index = instanceElements.FindIndex((x) => x.NetworkView == payload.Data);
                if (index == -1)
                {
                    AddElementToList(payload);
                }
                
            } else {
                playerObjects[playerId] = new List<PlayerObjectData>();
                AddElementToList(payload);
            }
        }

        private void AddElementToList(ClientSimOnPlayerObjectUpdatedEvent payload)
        {
            int componentCount = payload.Data.GetNetworkComponentCount();
            int playerId = payload.Data.GetPlayerId();
            for (int i = 0; i < componentCount; i++)
            {
                MonoBehaviour component = payload.Data.GetNetworkComponents()[i];
                
                PlayerObjectData element =
                    new PlayerObjectData
                    {
                        NetworkView = payload.Data,
                        lastModified = DateTime.Now,
                        Name = $"{component.GetType().Name} ({component.gameObject.name})",
                    };
                
                playerObjects[playerId].Add(element);
            }
        }
        
        private void UpdateCurrentPage(ClientSimOnPlayerObjectUpdateEndedEvent e)
        {
            if(playerObjects.Count == 0 || CurrentSelectedPlayerID == -1)
                return;
            
            int index = CurrentIndex;
            int count = playerObjects[CurrentSelectedPlayerID].Count;
            int localIndex = 0;
            for (; index < count;)
            {
                PlayerObjectData element = playerObjects[CurrentSelectedPlayerID][index];
                if(searchField.value != "" && !element.Name.Contains(searchField.value))
                {
                    index++;
                    continue;
                }
                
                playerObjectVisualElements[localIndex].UpdateData(element);
                
                localIndex++;
                if(localIndex >= CountPerPage)
                    break;
                
                index++;
            }
            
            for (int i = localIndex; i < playerObjectVisualElements.Count; i++)
            {
                playerObjectVisualElements[i].style.display = DisplayStyle.None;
            }
            
            prevPage.style.display = CurrentIndex - CountPerPage < 0 ? DisplayStyle.None : DisplayStyle.Flex;
            nextPage.style.display = CurrentIndex + CountPerPage >= count ? DisplayStyle.None : DisplayStyle.Flex;
            
            pageLabel.style.display = prevPage.style.display == DisplayStyle.None && nextPage.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent payload)
        {
            if (players.Contains(payload.player)) return;
            
            if(playerObjects.ContainsKey(payload.player.playerId) == false)
                playerObjects.Add(payload.player.playerId, new List<PlayerObjectData>());
            playerDropdown.choices.Add(payload.player.displayName);
            players.Add(payload.player);
            if (CurrentSelectedPlayerID == -1)
            {
                playerDropdown.index = 0;
                CurrentSelectedPlayerID = payload.player.playerId;
            }
        }
        
        private void OnPlayerLeft(ClientSimOnPlayerLeftEvent payload)
        {
            if (!playerObjects.ContainsKey(payload.player.playerId)) return;
            
            playerObjects.Remove(payload.player.playerId);
            players.Remove(payload.player);
            playerDropdown.choices.Remove(playerDropdown.choices.Find(x => x == payload.player.displayName));
            if (CurrentSelectedPlayerID == payload.player.playerId)
            {
                playerDropdown.index = 0;
                CurrentSelectedPlayerID = players[0].playerId;
            }
        }


        public struct PlayerObjectData
        {
            public IClientSimNetworkSerializer NetworkView;
            public DateTime lastModified;
            public string Name;
            public int index;
        }
    }

#endif
}