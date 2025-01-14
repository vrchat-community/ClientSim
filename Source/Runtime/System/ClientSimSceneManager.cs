using UnityEngine;
using VRC.SDKBase;
using System.Reflection;
using UnityEngine.Rendering.PostProcessing;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// Wrapper for the VRC_SceneDescriptor.
    /// </summary>
    public class ClientSimSceneManager : IClientSimSceneManager
    {
        // Using a variable here to allow for testability when not on android platform.
        private bool _isAndroid =
#if UNITY_ANDROID
            true;
#else
            false;
#endif
        
        // Only used for tests to verify android path in copy camera.
        internal static ClientSimSceneManager CreateTestInstance(bool isAndroid)
        {
            return new ClientSimSceneManager { _isAndroid = isAndroid };
        }
        
        
        private int _spawnOrder = 0;
        private VRC_SceneDescriptor _descriptor => VRC_SceneDescriptor.Instance;
        
        
        public bool HasSceneDescriptor()
        {
            return _descriptor != null;
        }

        public void SetupCamera(Camera camera)
        {
            if (!HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot get reference camera when there is no scene descriptor.");
            }
            
            Camera refCamera = null;
            if (_descriptor.ReferenceCamera != null)
            {
                refCamera = _descriptor.ReferenceCamera.GetComponent<Camera>();
            }

            if (refCamera != null)
            {
                CopyCameraValues(refCamera, camera);
            }
            else
            {
                this.LogWarning("Reference camera is not set in the SceneDescriptor!");
                
                // Force camera near settings
                camera.nearClipPlane = Mathf.Clamp(camera.nearClipPlane, 0.01f, 0.05f);
            }
        }

        public float GetRespawnHeight()
        {
            if (!HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot get respawn height when there is no scene descriptor.");
            }
            
            return _descriptor.RespawnHeightY;
        }

        public bool ShouldObjectsDestroyAtRespawnHeight()
        {
            if (!HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot get respawn height setting when there is no scene descriptor.");
            }
            
            return _descriptor.ObjectBehaviourAtRespawnHeight == VRC_SceneDescriptor.RespawnHeightBehaviour.Destroy;
        }

        public void ResetSpawnOrder()
        {
            _spawnOrder = 0;
        }
        
        public Transform GetSpawnPoint(bool remote = false)
        {
            if (!HasSceneDescriptor())
            {
                throw new ClientSimException("Trying to get a Spawn Point but there is no Scene Descriptor. Add a SceneDescriptor or the VRCWorldPrefab to your scene.");
            }
            
            if (_descriptor.spawns.Length == 0 || _descriptor.spawns[0] == null)
            {
                throw new ClientSimException("Trying to get a Spawn Point but the Scene Descriptor doesn't have one. Add a Transform to the 'Spawns' array in the VRC Scene Descriptor component.");
            }

            // Remote players always restart the list, so for now, only first spawn
            if (_descriptor.spawnOrder == VRC_SceneDescriptor.SpawnOrder.First || 
                _descriptor.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Demo || 
                remote)
            {
                return _descriptor.spawns[0];
            }
            if (_descriptor.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Random)
            {
                int spawn = Random.Range(0, _descriptor.spawns.Length);
                return _descriptor.spawns[spawn];
            }
            if (_descriptor.spawnOrder == VRC_SceneDescriptor.SpawnOrder.Sequential)
            {
                Transform spawn = _descriptor.spawns[_spawnOrder];
                _spawnOrder = (_spawnOrder + 1) % _descriptor.spawns.Length;
                return spawn;
            }
            
            // Fallback to first spawn point
            return _descriptor.spawns[0];
        }

        public Transform GetSpawnPoint(int index)
        {
            if (!HasSceneDescriptor())
            {
                throw new ClientSimException("Cannot get spawn point when there is no scene descriptor.");
            }
            
            if (index < 0 || index >= _descriptor.spawns.Length || _descriptor.spawns[index] == null)
            {
                this.LogWarning($"Using spawn point 0 instead of {index}, which is invalid.");
                index = 0;
            }
            
            // Fallback to first spawn point
            return _descriptor.spawns[index];
        }
        
        private void CopyCameraValues(Camera refCamera, Camera camera)
        {
            if (refCamera == null)
            {
                return;
            }
            
            camera.farClipPlane = refCamera.farClipPlane;
            camera.nearClipPlane = Mathf.Clamp(refCamera.nearClipPlane, 0.01f, 0.05f);
            camera.clearFlags = refCamera.clearFlags;
            camera.backgroundColor = refCamera.backgroundColor;
            camera.allowHDR = refCamera.allowHDR;

            PostProcessLayer refPostProcessLayer = refCamera.GetComponent<PostProcessLayer>();
            if (refPostProcessLayer != null)
            {
                if (_isAndroid)
                {
                    Debug.LogWarning("Post processing is not supported on Android");
                }
                else
                {
                    // VRChatBug: VRChat ignores if the PostProcessingLayer is disabled on the reference camera and always
                    // enables it for the player's camera.
                    PostProcessLayer postProcessLayer = camera.gameObject.AddComponent<PostProcessLayer>();
                    postProcessLayer.volumeLayer = refPostProcessLayer.volumeLayer;

                    postProcessLayer.volumeTrigger = refPostProcessLayer.volumeTrigger != refPostProcessLayer.transform
                        ? refPostProcessLayer.volumeTrigger
                        : camera.transform;

                    // Use reflection to copy over resources : https://github.com/Unity-Technologies/PostProcessing/issues/467
                    FieldInfo resourcesInfo = typeof(PostProcessLayer).GetField("m_Resources", BindingFlags.NonPublic | BindingFlags.Instance);
                    PostProcessResources postProcessResources = resourcesInfo.GetValue(refPostProcessLayer) as PostProcessResources;
                    postProcessLayer.Init(postProcessResources);
                }
            }
            refCamera.gameObject.SetActive(false);
        }
    }
}