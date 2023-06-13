using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimStackedCamera : ScriptableObject
    {
        public string CameraName = "Generic Stacked Camera";
        public LayerMask RenderLayer;
        public bool UseOcclusionCulling = true;
    }
}