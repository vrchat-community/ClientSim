using UnityEngine;
using VRC.Udon;

namespace ClientSimTest
{
    // This class is only to make getting references to objects in the scene easier. 
    // This can also be done without a dedicated MonoBehaviour and search the scene based on object names.
    [AddComponentMenu("")]
    public class ClientSimIssue3RespawnTestObjectReferences : MonoBehaviour
    {
        public UdonBehaviour respawnCube;
        public UdonBehaviour respawnWithIndexCube;
        public Transform spawn1;
        public Transform spawn2;
    }
}