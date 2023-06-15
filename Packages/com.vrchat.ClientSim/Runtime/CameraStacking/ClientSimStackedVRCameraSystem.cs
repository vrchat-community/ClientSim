using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using VRC.Core;
using VRC.SDK3.ClientSim;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimStackedVRCameraSystem : MonoBehaviour
    {
        public ClientSimStackedCamera[] CameraStack;
        [HideInInspector] public bool IsCameraStackingEnabled = false;
        
        private Camera MainSceneCamera;
        private bool IsInitialized = false;
        private bool IsReady = false;
        private List<Camera> Cameras;
        
        private ClientSimMenu clientSimMenu;

        public void Initialize(Camera playerCamera, ClientSimMenu menu)
        {
            MainSceneCamera = playerCamera;
            clientSimMenu = menu;
        }

        public void Ready()
        {
            IsReady = true;
        }
        
        void Update()
        {
            if(!IsReady) return;
            if (!IsInitialized) { InitializeStackedSystem(); }
            else
            {
                if (!Cameras[0].enabled)
                {
                    Debug.LogError("Stacked Cameras are not enabled.");
                }
            }
        }
     
        void OnDisable()
        {
            #if !UNITY_ANDROID
            if (MainSceneCamera != null)
            {
                if (IsCameraStackingEnabled)
                    DestroyCameraStack();
            }
            #endif
        }

        public void InitializeStackedSystem()
        {
            #if !UNITY_ANDROID
            Cameras = new List<Camera>();
            if (MainSceneCamera != null)
            {
                CreateCameraStack();
                IsInitialized = true;
            }
            #else
            gameObject.SetActive(false);
            #endif
        }

        private void CreateCameraStack()
        {
            for (int i = 0; i < CameraStack.Length; i++)
            {
                AddCamera(i);
            }
            IsCameraStackingEnabled = true;
        }

        private void DestroyCameraStack()
        {
            for (int i = 0; i < Cameras.Count; i++)
            {
                DestroyCamera(i);
            }
            IsCameraStackingEnabled = false;
        }

        private void AddCamera(int index)
        {
            GameObject cameraObj = Instantiate(new GameObject(), MainSceneCamera.transform);
            Camera cam = cameraObj.AddComponent<Camera>();
            XRDevice.DisableAutoXRCameraTracking(cam, true);

            cam.CopyFrom(MainSceneCamera); // Start by copying all the settings from the main camera
            #if VRC_VR_STEAM // We only want this on SteamVR.
            cameraObj.AddComponent<SteamVRCantedProjectionCullingFix>();
            #endif
            Cameras.Add(cam);

            cameraObj.tag = "Untagged";
            cameraObj.name = $"StackedCamera : {CameraStack[index].CameraName}";
            cam.clearFlags = CameraClearFlags.Depth;
            cam.depth = 100 - index;
            cam.cullingMask = CameraStack[index].RenderLayer;
            cam.useOcclusionCulling = CameraStack[index].UseOcclusionCulling;

            //Remove this cameras layers from the base camera
            MainSceneCamera.cullingMask = MainSceneCamera.cullingMask ^ CameraStack[index].RenderLayer;

            // Set the ClientSim UI canvas to use this camera
            clientSimMenu.SetCanvasCamera(cam);
        }

        private void DestroyCamera(int index)
        {
            Camera cam = Cameras[index];
            Cameras.RemoveAt(index);

            //Restore Layers from this camera to the main camera
            MainSceneCamera.cullingMask = MainSceneCamera.cullingMask | CameraStack[index].RenderLayer;
            Destroy(cam.gameObject);
        }
    }
}