using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon;

namespace VRC.SDK3.ClientSim.Tests.IntegrationTests
{
    [TestFixture]
    public class ClientSimHelperTests : ClientSimTestBase
    {
        [UnityTest]
        public IEnumerator TestUIShape()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
            };

            yield return StartClientSim(settings);
            
            GameObject canvasPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestCanvas");

            GameObject canvasObj = Object.Instantiate(canvasPrefab);
            
            // Give time to initialize
            yield return null;

            Canvas canvas = canvasObj.GetComponent<Canvas>();
            Assert.IsNotNull(canvas);
            Assert.IsNotNull(canvas.worldCamera);
            Assert.IsTrue(canvas.worldCamera == VRC_UiShape.GetEventCamera());

            Object.Destroy(canvasObj);
        }
        
        [UnityTest]
        public IEnumerator TestObjectSync()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
            };

            yield return StartClientSim(settings);
            
            GameObject syncedObjectPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestObjectSync");
            
            GameObject syncedObj = Object.Instantiate(syncedObjectPrefab);
            GameObject teleportPoint = new GameObject("Teleport");
            Transform teleport = teleportPoint.transform;
            teleport.SetPositionAndRotation(Vector3.one * 5, Quaternion.Euler(12, 34, 56));

            Vector3 originalPos = syncedObj.transform.position;
            Quaternion originalRotation = syncedObj.transform.rotation;
            
            // Give time to initialize
            yield return null;
            
            VRCObjectSync sync = syncedObj.GetComponent<VRCObjectSync>();
            Assert.IsNotNull(sync);
            Assert.IsNotNull(syncedObj.GetComponent<ClientSimObjectSyncHelper>());
            
            // TODO test these when implemented.
            sync.FlagDiscontinuity();
            sync.SetGravity(true);
            sync.SetKinematic(true);
            
            
            // Verify object is not at this location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(teleport.position, syncedObj.transform.position), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(teleport.rotation, syncedObj.transform.rotation), 0));
            
            sync.TeleportTo(teleportPoint.transform);
            
            // Verify object is now at this location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(teleport.position, syncedObj.transform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(teleport.rotation, syncedObj.transform.rotation), 0));
            
            
            // Verify object is not at original location.
            Assert.IsFalse(Mathf.Approximately(Vector3.Distance(originalPos, syncedObj.transform.position), 0));
            Assert.IsFalse(Mathf.Approximately(Quaternion.Angle(originalRotation, syncedObj.transform.rotation), 0));
            
            sync.Respawn();
            
            // Verify object is now at original location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(originalPos, syncedObj.transform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(originalRotation, syncedObj.transform.rotation), 0));

            yield return null;
            
            // Test going below respawn height
            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            Assert.IsNotNull(descriptor);

            // Force respawn setting
            descriptor.ObjectBehaviourAtRespawnHeight = VRC_SceneDescriptor.RespawnHeightBehaviour.Respawn;

            // Set object below respawn height.
            Vector3 belowRespawn = new Vector3(0, descriptor.RespawnHeightY - 10, 0);
            syncedObj.transform.position = belowRespawn;
            
            // Wait a frame to allow Object manager to respawn the object.
            yield return null;

            // Verify object is now at original location.
            Assert.IsTrue(Mathf.Approximately(Vector3.Distance(originalPos, syncedObj.transform.position), 0));
            Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(originalRotation, syncedObj.transform.rotation), 0));
            
            
            // Force respawn setting
            descriptor.ObjectBehaviourAtRespawnHeight = VRC_SceneDescriptor.RespawnHeightBehaviour.Destroy;

            // Set object below respawn height.
            syncedObj.transform.position = belowRespawn;
            
            // Wait a frame to allow Object manager to Destroy the object.
            yield return null;
            
            Assert.IsTrue(syncedObj == null);
            
            Object.Destroy(teleportPoint);
        }
        
        [UnityTest]
        public IEnumerator TestAudioSpatializer()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
            };

            yield return StartClientSim(settings);
            
            GameObject audioPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestAudioSpatializer");
            
            GameObject audioObj = Object.Instantiate(audioPrefab);
            
            // Give time to initialize
            yield return null;

            VRCSpatialAudioSource spatial = audioObj.GetComponent<VRCSpatialAudioSource>();
            Assert.IsNotNull(spatial);
            Assert.IsNotNull(audioObj.GetComponent<ClientSimSpatialAudioHelper>());

            Object.Destroy(audioObj);
        }
        
        [UnityTest]
        public IEnumerator TestAVProVideoPlayer()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
            };

            yield return StartClientSim(settings);

            GameObject avProPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestAVProPlayer");
            
            GameObject avProObj = Object.Instantiate(avProPrefab);
            
            // Give time to initialize
            yield return null;

            VRCAVProVideoPlayer avProPlayer = avProObj.GetComponent<VRCAVProVideoPlayer>();
            
            // These values throw exceptions without ClientSim.
            Assert.IsFalse(avProPlayer.IsReady);
            Assert.IsFalse(avProPlayer.IsPlaying);
            
            // TODO other tests
            
            Object.Destroy(avProObj);
        }
        
        
        [UnityTest]
        public IEnumerator TestObjectPool()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi remotePlayer = Helper.SpawnRemotePlayer();

            GameObject objectPoolPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestObjectPool");
            
            GameObject objectPoolObj = Object.Instantiate(objectPoolPrefab);
            
            // Give time to initialize
            yield return null;

            VRCObjectPool objectPool = objectPoolObj.GetComponent<VRCObjectPool>();
            Assert.IsNotNull(objectPool);
            Assert.IsNotNull(objectPoolObj.GetComponent<ClientSimObjectPoolHelper>());
            Assert.IsTrue(localPlayer.IsOwner(objectPoolObj));
            Assert.IsTrue(objectPool.Pool.Length == 3);
            
            
            GameObject poolObj1 = objectPool.TryToSpawn();
            Assert.IsNotNull(poolObj1);
            
            
            // Test using object pool when not the owner.
            Networking.SetOwner(remotePlayer, objectPoolObj);
            
            LogAssert.Expect(LogType.Error, $"Non-owner attempted to spawn object from {objectPoolObj.name}");
            Assert.IsNull(objectPool.TryToSpawn());
            
            LogAssert.Expect(LogType.Error, $"Non-owner attempted to return {poolObj1.name} to {objectPoolObj.name}");
            objectPool.Return(poolObj1);
            
            
            // Empty the pool
            Networking.SetOwner(localPlayer, objectPoolObj);
            
            GameObject poolObj2 = objectPool.TryToSpawn();
            Assert.IsNotNull(poolObj2);
            GameObject poolObj3 = objectPool.TryToSpawn();
            Assert.IsNotNull(poolObj3);
            
            // Pool should be empty
            Assert.IsNull(objectPool.TryToSpawn());
            
            objectPool.Return(poolObj3);
            Assert.IsTrue(objectPool.TryToSpawn() == poolObj3);
            
            // Return all objects
            objectPool.Return(poolObj1);
            objectPool.Return(poolObj2);
            objectPool.Return(poolObj3);
            
            // Test owner leaving with new owner being the local player.
            Networking.SetOwner(remotePlayer, objectPoolObj);
            Helper.RemoveRemotePlayer(remotePlayer);
            Assert.IsTrue(localPlayer.IsOwner(objectPoolObj));
            
            Object.Destroy(objectPoolObj);
        }
        
        [UnityTest]
        public IEnumerator TestUdon()
        {
            yield return LoadBasicScene();

            ClientSimSettings settings = new ClientSimSettings
            {
                enableClientSim = true,
                initializationDelay = 0,
                spawnPlayer = true,
            };

            yield return StartClientSim(settings);

            // See ClientSimWorldTestExample sample for proper udon program testing. 
            
            GameObject udonEmptyPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestUdon");
            GameObject udonSyncedPrefab = ClientSimTestPrefabSpawner.GetTestPrefab("ClientSimTestUdonSyncedPos");
            
            GameObject udonEmptyObj = Object.Instantiate(udonEmptyPrefab);
            GameObject udonSyncedObj = Object.Instantiate(udonSyncedPrefab);
            
            // Give time to initialize
            yield return null;

            Assert.IsNotNull(udonEmptyObj.GetComponent<UdonBehaviour>());
            Assert.IsNotNull(udonSyncedObj.GetComponent<UdonBehaviour>());
                
            // Udon can't be initialized if there is no udon program, which isn't supported in Packages. :(
            Assert.IsNull(udonEmptyObj.GetComponent<ClientSimUdonHelper>());
            Assert.IsNull(udonSyncedObj.GetComponent<ClientSimUdonHelper>());
            
            // TODO other tests
            // TODO since ClientSim component is never added, it's not possible to test ownership over the object. 
            
            Object.Destroy(udonEmptyObj);
            Object.Destroy(udonSyncedObj);
        }
    }
}