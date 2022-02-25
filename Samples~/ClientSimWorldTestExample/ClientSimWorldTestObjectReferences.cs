using UnityEngine;
using VRC.Udon;

namespace ClientSimTest.Examples
{
    // This class is only to make getting references to objects in the scene easier. 
    // This can also be done without a dedicated MonoBehaviour and search the scene based on object names.
    [AddComponentMenu("")]
    public class ClientSimWorldTestObjectReferences : MonoBehaviour
    {
        public GameObject station;
        public GameObject menu;
        public GameObject pickup;
        public GameObject endCredits;
        public GameObject door1;
        public GameObject door2;
        public UdonBehaviour doorController1;
        public UdonBehaviour doorController2;
        public Transform[] walkLocations;
    }
}