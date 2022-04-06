using System.IO;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Tests
{
    public static class ClientSimTestPrefabSpawner
    {
        private static readonly string _prefabResourcePath = Path.Combine("ClientSim", "Prefabs");
        private static readonly string _testPrefabResourcePath = Path.Combine("ClientSimTests", "Prefabs");

        public static GameObject GetRuntimePrefab(string prefabName)
        {
            return Resources.Load<GameObject>(Path.Combine(_prefabResourcePath, prefabName));
        }
        
        public static GameObject GetTestPrefab(string prefabName)
        {
            return Resources.Load<GameObject>(Path.Combine(_testPrefabResourcePath, prefabName));
        }
    }
}