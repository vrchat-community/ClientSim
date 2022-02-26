using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace VRC.SDK3.ClientSim.Tests
{
    public abstract class ClientSimTestBase : IPrebuildSetup, IPostBuildCleanup
    {
        protected ClientSimTestRuntimeHelper Helper { get; private set; }
        protected IClientSimEventDispatcher EventDispatcher { get; private set; }
        protected ClientSimTestInput TestInput  { get; private set; }

        private bool _sceneWasLoaded = false;
        private Scene _loadedScene;
        
        public virtual void Setup()
        {
            // Force Disable Domain Reloading to ensure ClientSim starts with the provided settings and not the user's saved settings.
            // After this method finishes, Unity will enter playmode and reload the domain, clearing all variable data.
            ClientSimTestDomainReloadSetter.SetDisableDomainReloadingSetting();
            
            // Begin test and disable default ClientSim behavior
            ClientSimRuntimeLoader.BeginUnityTesting(new ClientSimSettings { enableClientSim = false });
        }
        
        public virtual void Cleanup()
        {
            ClientSimTestDomainReloadSetter.ResetDisableDomainReloadingSetting();
            
            ClientSimRuntimeLoader.EndUnityTesting();
        }

        [SetUp]
        public virtual void TestSetUp()
        {
            _sceneWasLoaded = false;

            Helper = new ClientSimTestRuntimeHelper();

            EventDispatcher = Helper.EventDispatcher;
            TestInput = Helper.TestInput;
        }
        
        [UnityTearDown]
        public virtual IEnumerator TearDown()
        {
            ClientSimMain.RemoveInstance();

            if (_sceneWasLoaded)
            {
                yield return ClientSimTestSceneLoader.UnloadPlayModeScene(_loadedScene);
                _sceneWasLoaded = false;
            }

            Helper.Dispose();
            EventDispatcher = null;
            TestInput = null; 

            // Extra frame delay for cleaning.
            yield return null;
        }

        protected IEnumerator StartClientSim(ClientSimSettings settings)
        {
            Assert.IsFalse(ClientSimMain.HasInstance());
            Assert.IsFalse(Helper.HasReadyEventSent());

            ClientSimRuntimeLoader.StartClientSim(settings, EventDispatcher);

            yield return ClientSimTestUtils.WaitUntil(ClientSimMain.HasInstance, "ClientSim never started.");
            
            Assert.IsTrue(ClientSimMain.HasInstance());

            // Wait for ClientSim ready event to fire.
            yield return ClientSimTestUtils.WaitUntil(Helper.HasReadyEventSent, "ClientSim never sent ready event.");
            
            Assert.IsTrue(Helper.HasReadyEventSent());
        }

        #region Scene loading
        
        private IEnumerator LoadScene(Scene scene)
        {
            _sceneWasLoaded = true;
            _loadedScene = scene;
            
            // Loading a scene requires one frame for it to finish loading.
            yield return null;
        }
        
        // Empty scene only contains basic unity elements but no VRC components. This is useful for testing if
        // ClientSim will fail to start.
        protected IEnumerator LoadEmptyScene()
        {
            yield return LoadScene(ClientSimTestSceneLoader.LoadEmptyScene());
        }
        
        // Basic scene contains a cube floor and a scene descriptor, but no other VRC components. Just enough to start 
        // ClientSim and spawn a player.
        protected IEnumerator LoadBasicScene()
        {
            yield return LoadScene(ClientSimTestSceneLoader.LoadBasicScene());
        }

        #endregion
    }
}