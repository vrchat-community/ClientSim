using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimInputAxesSettings : ScriptableObject
    { 
        public List<InputAxis> inputAxes = new List<InputAxis>();

        [Serializable]
        public class InputAxis
        {
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;
            public float gravity;
            public float dead;
            public float sensitivity;
            public bool snap;
            public bool invert;
            public int type;
            public int axis;
            public int joyNum;
        }
    }
}
