using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.TestTools;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim.Tests.ComponentTests
{
    public class ClientSimSceneDescriptorTests : ClientSimTestBase
    {
        [UnityTest]
        public IEnumerator TestNoDescriptor()
        {
            yield return LoadEmptyScene();

            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNull(descriptor);

            ClientSimSceneManager sceneManager = new ClientSimSceneManager();
            Assert.IsFalse(sceneManager.HasSceneDescriptor());

            Assert.Throws<ClientSimException>(() => sceneManager.GetRespawnHeight());
            Assert.Throws<ClientSimException>(() => sceneManager.ShouldObjectsDestroyAtRespawnHeight());
            Assert.Throws<ClientSimException>(() => sceneManager.SetupCamera(null));
            Assert.Throws<ClientSimException>(() => sceneManager.GetSpawnPoint());
        }
        
        [UnityTest]
        public IEnumerator TestDescriptorSettings()
        {
            yield return LoadBasicScene();

            // Verify there is a descriptor.
            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNotNull(descriptor);

            ClientSimSceneManager sceneManager = new ClientSimSceneManager();
            Assert.IsTrue(sceneManager.HasSceneDescriptor());

            // Verify respawn height setting.
            float respawnHeight = -123;
            descriptor.RespawnHeightY = respawnHeight;
            Assert.IsTrue(Mathf.Approximately(sceneManager.GetRespawnHeight(), respawnHeight));
            respawnHeight = -44;
            descriptor.RespawnHeightY = respawnHeight;
            Assert.IsTrue(Mathf.Approximately(sceneManager.GetRespawnHeight(), respawnHeight));

            // Verify respawn height behaviour.
            descriptor.ObjectBehaviourAtRespawnHeight = VRC_SceneDescriptor.RespawnHeightBehaviour.Destroy;
            Assert.IsTrue(sceneManager.ShouldObjectsDestroyAtRespawnHeight());
            descriptor.ObjectBehaviourAtRespawnHeight = VRC_SceneDescriptor.RespawnHeightBehaviour.Respawn;
            Assert.IsFalse(sceneManager.ShouldObjectsDestroyAtRespawnHeight());
            
            // Test Invalid spawns
            descriptor.spawns = new Transform[0];
            Assert.Throws<ClientSimException>(() => sceneManager.GetSpawnPoint());

            descriptor.spawns = new Transform[1];
            Assert.Throws<ClientSimException>(() => sceneManager.GetSpawnPoint());
            
            // Create list of spawn points to verify transforms
            int size = 5;
            Transform[] spawns = new Transform[size];
            for (int i = 0; i < size; ++i)
            {
                spawns[i] = new GameObject().transform;
            }
            
            descriptor.spawns = spawns;

            // Test spawn order first is always first.
            descriptor.spawnOrder = VRC_SceneDescriptor.SpawnOrder.First;
            for (int i = 0; i < size; ++i)
            {
                Assert.IsTrue(sceneManager.GetSpawnPoint(false) == spawns[0]);
            }
            
            // Verify sequential order is actually in order.
            descriptor.spawnOrder = VRC_SceneDescriptor.SpawnOrder.Sequential;
            for (int i = 0; i < size * 2; ++i)
            {
                Assert.IsTrue(sceneManager.GetSpawnPoint(false) == spawns[i % size]);
            }
            
            // Verify random order is not null.
            descriptor.spawnOrder = VRC_SceneDescriptor.SpawnOrder.Random;
            for (int i = 0; i < size; ++i)
            {
                Assert.IsNotNull(sceneManager.GetSpawnPoint(false));
            }

            // Verify remote spawns are always initial spawn point.
            for (int i = 0; i < size; ++i)
            {
                Assert.IsTrue(sceneManager.GetSpawnPoint(true) == spawns[0]);
            }

            // Cleanup
            for (int i = 0; i < size; ++i)
            {
                Object.Destroy(spawns[i].gameObject);
            }
        }

        [UnityTest]
        public IEnumerator TestCopyCamera()
        {
            yield return LoadBasicScene();
            
            // Spawn a camera with post processing set. 
            GameObject cameraPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestCameraWithPPS");
            GameObject cameraObj = Object.Instantiate(cameraPrefab);
            VRC_SceneDescriptor.Instance.ReferenceCamera = cameraObj;
            
            // Android tests
            TestCameraSettings(
                true, 
                Random.Range(0.01f, 0.05f), 
                1 + Random.value * 1000,
                CameraClearFlags.Color,
                Random.ColorHSV(), 
                false);
            
            // Test same on PC
            TestCameraSettings(
                false, 
                Random.Range(0.01f, 0.05f), 
                1 + Random.value * 1000,
                CameraClearFlags.Color,
                Random.ColorHSV(), 
                false);
            
            TestCameraSettings(
                false, 
                0, 
                500,
                CameraClearFlags.Nothing,
                Color.black, 
                false);
            
            TestCameraSettings(
                false, 
                1, 
                333,
                CameraClearFlags.Depth,
                Color.cyan, 
                false);
            
            Object.Destroy(cameraObj);
        }

        private void TestCameraSettings(
            bool isAndroid,
            float nearClipPlane, 
            float farClipPlane, 
            CameraClearFlags clearFlags,
            Color backgroundColor, 
            bool allowHDR)
        {
            GameObject obj = new GameObject("Camera");
            Camera camera = obj.AddComponent<Camera>();

            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNotNull(descriptor);
            Assert.IsNotNull(descriptor.ReferenceCamera);
            
            Camera refCam = descriptor.ReferenceCamera.GetComponent<Camera>();
            Assert.IsNotNull(refCam);
            PostProcessLayer postProcessLayer = refCam.GetComponent<PostProcessLayer>();
            Assert.IsNotNull(postProcessLayer);

            Assert.IsFalse(Mathf.Approximately(camera.nearClipPlane, nearClipPlane));
            Assert.IsFalse(Mathf.Approximately(camera.farClipPlane, farClipPlane));
            Assert.IsFalse(camera.clearFlags == clearFlags);
            Assert.IsFalse(camera.backgroundColor == backgroundColor);
            Assert.IsFalse(camera.allowHDR == allowHDR);
            
            postProcessLayer = camera.GetComponent<PostProcessLayer>();
            Assert.IsNull(postProcessLayer);
            
            refCam.nearClipPlane = nearClipPlane;
            refCam.farClipPlane = farClipPlane;
            refCam.clearFlags = clearFlags;
            refCam.backgroundColor = backgroundColor;
            refCam.allowHDR = allowHDR;

            if (isAndroid)
            {
                LogAssert.Expect(LogType.Warning, "Post processing is not supported on Android");
            }
            
            ClientSimSceneManager sceneManager = ClientSimSceneManager.CreateTestInstance(isAndroid);
            sceneManager.SetupCamera(camera);
            
            Assert.IsTrue(0.01f <= camera.nearClipPlane && camera.nearClipPlane <= 0.05f);
            Assert.IsTrue(Mathf.Approximately(camera.farClipPlane, farClipPlane));
            Assert.IsTrue(camera.clearFlags == clearFlags);
            Assert.IsTrue(camera.backgroundColor == backgroundColor);
            Assert.IsTrue(camera.allowHDR == allowHDR);

            postProcessLayer = camera.GetComponent<PostProcessLayer>();

            if (isAndroid)
            {
                Assert.IsNull(postProcessLayer);
            }
            else
            {
                Assert.IsNotNull(postProcessLayer);
            }
            
            Object.Destroy(obj);
        }
    }
}