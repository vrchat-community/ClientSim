using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRC.SDK3.ClientSim.Tests.WorldTests
{
    public class ClientSimWorldTestBase : IPrebuildSetup, IPostBuildCleanup
    {
        // Items are static to ensure they survive test start clearing variables.
        protected static ClientSimTestRuntimeHelper Helper { get; private set; }
        protected static IClientSimEventDispatcher EventDispatcher  { get; private set; }
        protected static ClientSimSettings Settings { get; private set; }

        private static int _testCount = 0;

        protected virtual ClientSimSettings GetTestSettings()
        {
            return new ClientSimSettings
            {
                enableClientSim = true,
                spawnPlayer = true,
                localPlayerIsMaster = true,
                isInstanceOwner = true,
                initializationDelay = 0,
                displayLogs = true,
                deleteEditorOnly = true
            };
        }
        
        protected virtual void SetupScene() { }
        protected virtual void TearDownScene() { }

        // Setup is called as part of IPrebuildSetup. This method is called before playmode is entered for the test and
        // allows for notifying ClientSim that it is in the test environment. All test classes running will have this
        // method called before playmode is entered. Since this style test requires loading a scene, only one can run at
        // a time. This setup checks if multiple have started and fails all if detected. 
        public void Setup()
        {
            ++_testCount;

            // Verify only one instance of this type of test is running at a time.
            // Due to how ClientSim starts using InitializeOnLoad, this is the only method that can occur in tests
            // before ClientSim will initialize. Both this method and ClientSim will only try to initialize once for all
            // test runs. This means that there can only be one version of tests running at a time.
            if (ClientSimRuntimeLoader.IsInUnityTest() || _testCount != 1)
            {
                // Expect the error to prevent failing the test and breaking teardown.
                LogAssert.Expect(LogType.Error, "Only one instance of ClientSimWorldTest can run at a time!");
                Debug.LogError("Only one instance of ClientSimWorldTest can run at a time!");
                return;
            }

            // Begin test and disable default ClientSim behavior
            Helper = new ClientSimTestRuntimeHelper();
            EventDispatcher = Helper.EventDispatcher;

            // TODO guard against failures here as this may prevent resetting domain reload settings.
            SetupScene();
            
            Assert.IsFalse(ClientSimMain.HasInstance());
            Assert.IsFalse(Helper.HasReadyEventSent());
            
            Settings = GetTestSettings();
            ClientSimRuntimeLoader.BeginUnityTesting(Settings, Helper.EventDispatcher);
            
            // Set domain at the end to guarantee that nothing can fail in this method at this point.
            if (_testCount == 1)
            {
                // Force Disable Domain Reloading to ensure ClientSim starts with the provided settings and not the user's saved settings.
                // After this method finishes, Unity will enter playmode and reload the domain, clearing all variable data.
                ClientSimTestDomainReloadSetter.SetDisableDomainReloadingSetting();
            }
        }
        
        public void Cleanup()
        {
            --_testCount;
            if (_testCount == 0)
            {
                ClientSimTestDomainReloadSetter.ResetDisableDomainReloadingSetting();
            }

            Helper?.Dispose();
            EventDispatcher = null;
            Settings = null;
            
            if (ClientSimMain.HasInstance())
            {
                ClientSimMain.RemoveInstance();
            }
            
            ClientSimRuntimeLoader.EndUnityTesting();
            
            TearDownScene();
        }

        // Force fail all tests if multiple running to prevent any other errors from occurring.
        [SetUp]
        public void VerifyOnlyOneTest()
        {
            Assert.IsTrue(_testCount == 1, $"Only one instance of ClientSimWorldTest can run at a time! Count: {_testCount}");
        }
        
        protected void LoadSceneFromPath(string scenePath)
        {
            ClientSimTestSceneLoader.LoadSceneFromPath(scenePath, false);
        }

        protected IEnumerator WaitForClientSimStartup()
        {
            Assert.IsTrue(ClientSimRuntimeLoader.IsInUnityTest());

            yield return ClientSimTestUtils.WaitUntil(ClientSimMain.HasInstance, "ClientSim never started.");
            
            Assert.IsTrue(ClientSimMain.HasInstance());

            // Wait for ClientSim ready event to fire.
            yield return ClientSimTestUtils.WaitUntil(Helper.HasReadyEventSent, "ClientSim never sent ready event.");
            
            Assert.IsTrue(Helper.HasReadyEventSent());
        }
    }
}