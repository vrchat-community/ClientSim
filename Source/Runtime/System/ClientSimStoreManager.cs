using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Udon.Common.Interfaces;
using VRC.Economy;
using VRC.Udon;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VRC.Economy.Editor;
#endif

namespace VRC.SDK3.ClientSim
{
    public class ClientSimStoreManager : IDisposable
    {
        private IClientSimEventDispatcher _eventDispatcher;
        private static IClientSimUdonEventSender _udonEventSender;
        private static IClientSimPlayerManager _playerManager;
        
        public static List<IProduct> WorldProducts;
        public static Dictionary<string, IProduct> OwnedProducts;
        
        public ClientSimStoreManager(IClientSimEventDispatcher eventDispatcher, IClientSimUdonEventSender udonEventSender, IClientSimPlayerManager playerManager)
        {
            _eventDispatcher = eventDispatcher;
            _udonEventSender = udonEventSender;
            _playerManager = playerManager;
            
            Subscribe();

            WorldProducts = new List<IProduct>();
            OwnedProducts = new Dictionary<string, IProduct>();

#if UNITY_EDITOR
            // Fetch UdonProducts scriptable objects to add to WorldProducts list
            UdonProduct[] products = Resources.FindObjectsOfTypeAll<UdonProduct>();
            if (products.Length != 0)
            {
                Dictionary<string, UdonProduct> productsDict = new Dictionary<string, UdonProduct>();
                foreach (UdonProduct prod in products)
                {
                    productsDict.Add(AssetDatabase.GetAssetPath(prod), prod);
                }
                
                var dependencies = AssetDatabase.GetDependencies(SceneManager.GetActiveScene().path);
                foreach (string s in dependencies)
                {
                    productsDict.TryGetValue(s, out UdonProduct prod);
                    if (prod != null) WorldProducts.Add(prod);
                }
            }
#endif
        }

        ~ClientSimStoreManager()
        {
            Dispose();
        }
        
        private void Subscribe()
        {
            _eventDispatcher.Subscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
        }

        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<ClientSimOnPlayerJoinedEvent>(OnPlayerJoined);
            WorldProducts.Clear();
            OwnedProducts.Clear();
        }
        
        private void OnPlayerJoined(ClientSimOnPlayerJoinedEvent joinEvent)
        {
            if (joinEvent.player.isLocal)
            {
                if (WorldProducts.Count == 0) return;
                List<UdonBehaviour> udonBehaviours = UdonManager.Instance.GetUdonBehavioursInScene();
                foreach (UdonProduct p in WorldProducts)
                {
                    if (p.Purchased)
                    {
                        UdonProductStub stub = new UdonProductStub(p, _playerManager.LocalPlayer());
                        OwnedProducts.Add(p.ID, stub);
                        foreach (UdonBehaviour ub in udonBehaviours)
                        {
                            ub.RunEvent("_onPurchaseConfirmed", ("result", stub), ("player", _playerManager.LocalPlayer()), ("purchasedNow", false));
                        }
                    }
                }
                foreach (UdonBehaviour ub in UdonManager.Instance.GetUdonBehavioursInScene())
                {
                    ub.RunEvent("_onPurchasesLoaded", ("result", OwnedProducts.Values.ToArray()), ("player", _playerManager.LocalPlayer()));
                }
            }
        }

#if UNITY_EDITOR
        // This section hooks into the UdonProductEditor.OnDrawProductGUI event
        // to modify the custom ScriptableObject editor if the user is running ClientSim
        [InitializeOnLoadMethod]
        public static void ModifyUdonProductGUI()
        {
            UdonProductEditor.OnDrawProductGUI += OnDrawProductGUI;
            UdonProductElement.OnCustomGUI += OnDrawProductElementGUI;
        }
        public static event EventHandler<UdonProduct> RefreshProductElementButton;
        private const string stringExpireProduct = "Expire product";
        private const string stringPurchaseProduct = "Purchase product";
        private const string stringPurchaseTooltip = "This product (if used in any UdonBehaviours in the scene) will act as if it has already been bought prior to launching playmode, firing the OnPurchaseConfirmed & OnPurchasesLoaded events accordingly.";
        private const string stringNotUsedInScene = "This product is not used in this scene and therefore cannot be bought or expired.";
        
        private static bool IsProductUsedInScene(UdonProduct product)
        {
            return product != null && WorldProducts != null && WorldProducts.Contains(product);
        }

        private static void OnDrawProductElementGUI(object sender, VisualElement ve)
        {
            UdonProductElement element = (UdonProductElement)sender;
            UdonProduct product = element.UdonProduct;
            ve.style.display = DisplayStyle.Flex;
            ve.SetEnabled(true);
            
            EditorApplication.playModeStateChanged += change =>
            {
                RefreshProductElementButton?.Invoke(sender, product);
            };

            Button button = new Button();
            button.text = product.Purchased ? stringExpireProduct : stringPurchaseProduct;
            button.tooltip = product.Purchased ? stringPurchaseTooltip : string.Empty;
            button.clicked += () =>
            {
                product.Purchased = !product.Purchased;
                if (EditorApplication.isPlaying && IsProductUsedInScene(product))
                {
                    if (product.Purchased) PurchaseConfirmed(product, true);
                    else PurchaseExpired(product);
                }
                RefreshProductElementButton?.Invoke(sender, product);
            };
            ve.Add(button);
            
            RefreshProductElementButton += (o, args) =>
            {
                if (args != product) return;
                button.text = product.Purchased ? stringExpireProduct : stringPurchaseProduct;
                if(product.Purchased) UdonProductsManagerWindow.ShowInfoBox(UdonProductsManagerWindow.InfoBoxStatus.Info, stringPurchaseTooltip);
                button.tooltip = product.Purchased ? stringPurchaseTooltip : string.Empty;
                ve.SetEnabled(!EditorApplication.isPlaying || IsProductUsedInScene(product));
            };
        }

        private static void OnDrawProductGUI(object sender, UdonProduct e)
        {
            bool isUsedInScene = e != null && WorldProducts != null && EditorApplication.isPlaying && WorldProducts.Contains(e);
            
            GUILayout.Label("ClientSim functions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(EditorApplication.isPlaying && !isUsedInScene))
            {
                if (GUILayout.Button(e.Purchased ? stringExpireProduct : stringPurchaseProduct))
                {
                    e.Purchased = !e.Purchased;
                    if (EditorApplication.isPlaying && isUsedInScene)
                    {
                        if (e.Purchased) PurchaseConfirmed(e, true);
                        else PurchaseExpired(e);
                    }
                }

                if (!EditorApplication.isPlaying && e.Purchased) 
                    EditorGUILayout.HelpBox(stringPurchaseTooltip, MessageType.Info);
                if (EditorApplication.isPlaying && !isUsedInScene) EditorGUILayout.HelpBox(stringNotUsedInScene, MessageType.Warning);
            }
        }
#endif

        public static void PurchaseConfirmed(IProduct product, bool purchasedNow)
        {
            UdonProductStub stub = new UdonProductStub(product, _playerManager.LocalPlayer());
            OwnedProducts.Add(product.ID, stub);
            foreach (UdonBehaviour ub in UdonManager.Instance.GetUdonBehavioursInScene())
            {
                ub.RunEvent("_onPurchaseConfirmed", ("result", stub), ("player", _playerManager.LocalPlayer()), ("purchasedNow", purchasedNow));
            }
        }

        public static void PurchaseExpired(IProduct product)
        {
            IProduct ownedProduct = OwnedProducts[product.ID];
            OwnedProducts.Remove(product.ID);
            _udonEventSender.RunEvent("_onPurchaseExpired", ("result", ownedProduct), ("player", _playerManager.LocalPlayer()));
        }

        public static void SendProductEvent(IUdonEventReceiver behaviour, IProduct product)
        {
            if (OwnedProducts.ContainsKey(product.ID))
            {
                behaviour.RunEvent("_onProductEvent", ("result", OwnedProducts[product.ID]), ("player", OwnedProducts[product.ID].Buyer));
            }
        }

        public static void ListPurchases(IUdonEventReceiver behaviour, VRC.SDKBase.VRCPlayerApi player)
        {
            behaviour.RunEvent("_onListPurchases", ("result", OwnedProducts.Values.ToArray()), ("player", _playerManager.LocalPlayer()));
        }

        public static void ListAvailableProducts(IUdonEventReceiver behaviour)
        {
            behaviour.RunEvent("_onListAvailableProducts", ("result", WorldProducts.ToArray()));
        }

        public static bool DoesPlayerOwnProduct(VRC.SDKBase.VRCPlayerApi player, IProduct product)
        {
            return OwnedProducts.ContainsKey(product.ID);
        }
        
        public static bool DoesAnyPlayerOwnProduct(IProduct product)
        {
            return OwnedProducts.ContainsKey(product.ID);
        }
        
        public static VRC.SDKBase.VRCPlayerApi[] GetPlayersWhoOwnProduct(IProduct product)
        {
            VRC.SDKBase.VRCPlayerApi[] playerApis;
            if (OwnedProducts.ContainsKey(product.ID))
            {
                playerApis = new VRC.SDKBase.VRCPlayerApi[1] { _playerManager.LocalPlayer() };
            }
            else
            {
                playerApis = Array.Empty<VRC.SDKBase.VRCPlayerApi>();
            }
            return playerApis;
        }

        public static void ListProductOwners(IUdonEventReceiver behaviour, IProduct product)
        {
            behaviour.RunEvent("_onListProductOwners", ("result", product), ("owners", new string[] {"VRCat", "Fred", "VRRat"}));
        }
    }
}