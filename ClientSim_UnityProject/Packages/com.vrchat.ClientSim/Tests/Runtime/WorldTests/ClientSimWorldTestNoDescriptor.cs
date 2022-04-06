using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRC.SDK3.ClientSim.Tests.WorldTests
{
    public class ClientSimWorldTestNoDescriptor : ClientSimWorldTestBase
    {
        protected override ClientSimSettings GetTestSettings()
        {
            return new ClientSimSettings
            {
                enableClientSim = false,
            };
        }

        protected override void SetupScene()
        {
            // Load an empty scene with no VRC components.
            LoadSceneFromPath(ClientSimTestSceneLoader.GetEmptyScenePath());

            Assert.IsFalse(ClientSimMain.HasInstance());
        }
        
        // This test is for checking if the ClientSim will prevent starting if there is no VRC_SceneDescriptor in the scene.
        [UnityTest]
        public IEnumerator TestNoWorldDescriptor()
        {
            // Wait one frame for ClientSimMain to attempt to start.
            // Note that it is not easy to check if ClientSim logs the "fail to start" message due to it happening
            // between setup and this test.
            yield return null;
            
            Assert.IsFalse(ClientSimMain.HasInstance());
            Assert.IsFalse(Helper.HasReadyEventSent(), "ClientSim ready event sent when ClientSim shouldn't start.");
        }
    }
}

