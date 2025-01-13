using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.SDKBase.Network;
using VRC.Utility;
using Object = UnityEngine.Object;

namespace VRC.SDK3.ClientSim
{
    public static class ClientSimNetworkingUtilities
    {
        public const int MinID = 10;
        public const int MaxID = 100_000;
        public const int MinimumViewID = 10;
        
        public const int PlayerDataObjectID = 4;
        public static int FirstPlayerPersistenceID => System.Math.Max(MinimumViewID, 100);
        public static int MaxPlayerPersistenceID => Mathf.CeilToInt(MaxID * 0.1f);
        
        private static VRCPlayerObject[] _playerObjectList;
        
        public enum OwnershipOption
        {
            /// <summary>
            ///     Ownership is fixed. Instantiated objects stick with their creator, scene objects always belong to the Master Client.
            /// </summary>
            Fixed,
            /// <summary>
            ///     Ownership can be taken away from the current owner who can't object.
            /// </summary>
            Takeover,
            /// <summary>
            ///     Ownership can be requested with PhotonView.RequestOwnership but the current owner has to agree to give up ownership.
            /// </summary>
            /// <remarks>The current owner has to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.</remarks>
            Request
        }
        
        public static VRCPlayerObject[] GetPlayerObjectList()
        {
            if (_playerObjectList == null)
            {
                _playerObjectList = Object.FindObjectsOfType<VRCPlayerObject>(true);
            }
            return _playerObjectList;
        }

        #region NetworkIdGeneration

        public static int FlattenPlayerViewId(int viewID)
            => viewID % MaxID;
        
        public static void ConfigureNetworkOnScene(VRC_SceneDescriptor sceneDescriptor)
        {

#if VRC_ENABLE_PLAYER_PERSISTENCE
            VRCPlayerObject[] playerPersistence = GetPlayerObjectList();
            bool IsPlayerPersistence(GameObject obj)
                => playerPersistence != null && playerPersistence.Any(pp => obj.transform.IsChildOf(pp.transform) || obj.transform == pp.transform);
#endif
            
            var (netIDs, _) = ConfigureNetworkIdsForClientSim(sceneDescriptor, out List<NetworkIDAssignment.SetErrorLocation> errors, NetworkIDAssignment.SetError.IncompatibleTypes);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Debug.LogError(error.ToString());
                Debug.LogErrorFormat("Found {0} errors while configuring network IDs!", errors.Count);
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #endif
                return;
            }

            foreach (NetworkIDPair netID in netIDs)
            {
                GameObject obj = netID.gameObject;
                if (obj == null)
                    continue;

#if VRC_ENABLE_PLAYER_PERSISTENCE
                // This is initialized later, as players join
                if (IsPlayerPersistence(netID.gameObject))
                    continue;
#endif
                    
                try
                {
                    IClientSimNetworkId view = obj.GetComponent<IClientSimNetworkId>();
                    if(view == null)
                        view = obj.AddComponent<ClientSimNetworkingView>() as IClientSimNetworkId;
                    view.SetNetworkId(netID.ID);
                    view.OwnershipStyle(OwnershipOption.Takeover);
                }
                catch (Exception e)
                {
                    #if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
                    #endif
                    Debug.LogErrorFormat("Failed to configure network IDs on {0}", obj.name);
                    Debug.LogException(e);
                }
            }
        }
        
#if UNITY_EDITOR
        public static void DoSceneSetup()
        {
            //Find scene descriptor
            VRC_SceneDescriptor descriptor = VRC_SceneDescriptor.Instance;
            
            if(descriptor == null)
            {
                VRC.Core.Logger.LogError("Unable to find scene descriptor in scene");
                EditorApplication.isPlaying = false;
                return;
            }

            var (_, newIDs) = NetworkIDAssignment.ConfigureNetworkIDs(
                descriptor,
                out List<NetworkIDAssignment.SetErrorLocation> errors,
                NetworkIDAssignment.SetError.InvalidObject);
            
            if (errors.Count > 0)
            {
                // Try to fix the errors by removing duplicates
                List<NetworkIDPair> newIDsList = errors.FindAll(error => error.error == NetworkIDAssignment.SetError.IncompatibleTypes)
                    .Select(error => new NetworkIDPair{ gameObject = error.location.gameObject, ID = error.location.ID, SerializedTypeNames = NetworkIDAssignment.GetSerializedTypes(error.location.gameObject) })
                    .ToList();

                descriptor.NetworkIDCollection = descriptor.NetworkIDCollection
                    .Where(pair =>
                        newIDsList.FindIndex(newID => newID.ID == pair.ID && pair.gameObject == newID.gameObject) == -1)
                    .ToList();
            
                descriptor.NetworkIDCollection.AddRange(newIDsList);
                
                (_, newIDs) = NetworkIDAssignment.ConfigureNetworkIDs(
                    descriptor,
                    out errors,
                    NetworkIDAssignment.SetError.InvalidObject);
                
                if (errors.Count > 0)
                {
                    VRC.Core.Logger.LogError($"Failed to assign network IDs, {errors.Count} errors encountered!\nTry using the Network ID Utility to resolve them.");
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            if (newIDs.Count() > 0)
            {
                descriptor.gameObject.MarkDirty();
                PrefabUtility.RecordPrefabInstancePropertyModifications(descriptor);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
#endif
        
        private static (IEnumerable<NetworkIDPair> allIDs, IEnumerable<NetworkIDPair> newIDs) ConfigureNetworkIdsForClientSim(INetworkIDContainer container, out List<NetworkIDAssignment.SetErrorLocation> errors, params NetworkIDAssignment.SetError[] errorsToIgnore)
        {
            errors = new List<NetworkIDAssignment.SetErrorLocation>();
            if(container.NetworkIDCollection == null)
                container.NetworkIDCollection = new List<NetworkIDPair>();
            
            List<INetworkID> netIDs = new List<INetworkID>();
            List<NetworkIDPair> newIDs = new List<NetworkIDPair>();
            Dictionary<GameObject, NetworkIDPair> idMap = new Dictionary<GameObject, NetworkIDPair>();
            int id = MinID;
            Scene scene = SceneManager.GetActiveScene();
            
            foreach (var pair in container.NetworkIDCollection)
            {
                if (!pair.gameObject)
                {
                    errors.Add(new NetworkIDAssignment.SetErrorLocation { error = NetworkIDAssignment.SetError.InvalidObject, location = pair });
                }
                else if (pair.ID < MinID || pair.ID >= MaxID)
                {
                    errors.Add(new NetworkIDAssignment.SetErrorLocation { error = NetworkIDAssignment.SetError.InvalidId, location = pair });
                }
                else
                {
                    idMap[pair.gameObject] = pair;
                    
                    if (pair.ID >= id)
                        id = pair.ID + 1;
                }
            }
            
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
            
            foreach (var obj in allObjects)
            {
                netIDs.AddRange(obj.GetComponents<INetworkID>());
            }
            
            netIDs = netIDs
                .Where(nid => (nid is Component cnid) && cnid.gameObject && scene == cnid.gameObject.scene)
                .Select(nid => (nid as Component).gameObject)
                .Distinct()
                .OrderBy(go => go.transform.Path(), StringComparer.InvariantCulture)
                .Select(go => go.GetComponent<INetworkID>())
                .ToList();

            id = FindNextFreeID(id);
            foreach (INetworkID netID in netIDs)
                if (TrySetID(netID, ref id, ref errors, out NetworkIDPair newIDPair) && newIDPair != null)
                    newIDs.Add(newIDPair);

            errors.RemoveAll(err => errorsToIgnore.Contains(err.error));
            
            container.NetworkIDCollection = idMap.Values.ToList();

            return (netIDs.Select(nid => idMap[(nid as Component).gameObject]).Distinct(), newIDs);
            
            bool TrySetID(INetworkID NetworkID, ref int refId, ref List<NetworkIDAssignment.SetErrorLocation> errors, out NetworkIDPair newIDPair)
            {
                GameObject source = (NetworkID as Component).gameObject;
                List<string> typeNames = GetSerializedTypes(NetworkID);
                int curId = refId;
                newIDPair = null;
            
                if (curId < MinID || curId >= MaxID)
                {
                    errors.Add(new NetworkIDAssignment.SetErrorLocation { error = NetworkIDAssignment.SetError.InvalidId, location = new NetworkIDPair { gameObject = source, ID = curId } });
                    return false;
                }

                if (idMap.Values.Any(pair => pair.ID == curId && pair.gameObject != source))
                {
                    errors.Add(new NetworkIDAssignment.SetErrorLocation 
                    { 
                        error = NetworkIDAssignment.SetError.IdInUse, 
                        location = new NetworkIDPair { gameObject = source, ID = curId },
                        relatedLocation = idMap.Values.First(pair => pair.ID == curId && pair.gameObject != source)
                    });
                    
                    return false;
                }

                if (idMap.TryGetValue(source, out NetworkIDPair matchInfo))
                {
                    if (!DoTypesMatch(matchInfo, typeNames))
                        errors.Add(new NetworkIDAssignment.SetErrorLocation { error = NetworkIDAssignment.SetError.IncompatibleTypes, location = matchInfo });
                }
                else
                {
                    newIDPair = new SDKBase.Network.NetworkIDPair
                    {
                        gameObject = source,
                        ID = curId,
                        SerializedTypeNames = typeNames
                    };
                    
                    idMap.Add(source, newIDPair);
                    
                    refId = FindNextFreeID(curId);
                }
                
                return true;
            }
            
            bool DoTypesMatch(NetworkIDPair idInfo, List<string> typeNames)
                => idInfo.SerializedTypeNames.Count == 0 // Won't exist for old scenes
                   || idInfo.SerializedTypeNames.SequenceEqual(typeNames);
            
            int FindNextFreeID(int curId)
            {
                void findNextFree()
                {
                    while (idMap.Values.Any(pair => pair.ID == curId) && curId < MaxID)
                        curId++;
                }

                findNextFree();

                // Loop over once, in case there are gaps
                if (curId >= MaxID)
                {
                    curId = MinID;
                    findNextFree();
                }

                if (curId >= MaxID)
                    throw new System.ApplicationException("Ran out of Network IDs!");

                return curId;
            }
        }
        
        public static List<string> GetSerializedTypes(INetworkID NetworkID)
            => GetSerializedTypes((NetworkID as Component).gameObject);
        
        public static List<string> GetSerializedTypes(GameObject NetworkID)
            => NetworkID
                .GetComponents<VRCNetworkBehaviour>()
                .Select(nb => nb.GetType().FullName)
                .ToList();
        
        #endregion

        #region Conversion To/From JToken

        public static DataToken GetJTokenFromVector3(this Vector3 vector3)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "V3";
            jObject["x"] = vector3.x;
            jObject["y"] = vector3.y;
            jObject["z"] = vector3.z;
            return jObject;
        }
        
        public static DataToken GetJTokenFromQuaternion(this Quaternion quaternion)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "Q";
            jObject["x"] = quaternion.x;
            jObject["y"] = quaternion.y;
            jObject["z"] = quaternion.z;
            jObject["w"] = quaternion.w;
            return jObject;
        }
        
        public static DataToken GetJTokenFromVector2(this Vector2 vector2)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "V2";
            jObject["x"] = vector2.x;
            jObject["y"] = vector2.y;
            return jObject;
        }
        
        public static DataToken GetJTokenFromColor(this Color color)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "C";
            jObject["r"] = color.r;
            jObject["g"] = color.g;
            jObject["b"] = color.b;
            jObject["a"] = color.a;
            return jObject;
        }
        
        public static DataToken GetJTokenFromColor32(this Color32 color32)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "C32";
            jObject["r"] = color32.r;
            jObject["g"] = color32.g;
            jObject["b"] = color32.b;
            jObject["a"] = color32.a;
            return jObject;
        }
        
        public static DataToken GetJTokenFromVector4(this Vector4 vector4)
        {
            DataDictionary jObject = new DataDictionary();
            jObject["T"] = "V4";
            jObject["x"] = vector4.x;
            jObject["y"] = vector4.y;
            jObject["z"] = vector4.z;
            jObject["w"] = vector4.w;
            return jObject;
        }
        
        public static Vector3 GetVector3(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Vector3((float)dict["x"].Double, (float)dict["y"].Double, (float)dict["z"].Double);
            }
            return Vector3.zero;
        }
        
        public static Quaternion GetQuaternion(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Quaternion((float)dict["x"].Double, (float)dict["y"].Double, (float)dict["z"].Double, (float)dict["w"].Double);
            }
            return Quaternion.identity;
        }
        
        public static Vector2 GetVector2(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Vector2((float)dict["x"].Double, (float)dict["y"].Double);
            }
            return Vector2.zero;
        }
        
        public static Color GetColor(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Color((float)dict["r"].Double, (float)dict["g"].Double, (float)dict["b"].Double, (float)dict["a"].Double);
            }
            return Color.black;
        }
        
        public static Color32 GetColor32(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Color32((byte)dict["r"].Double, (byte)dict["g"].Double, (byte)dict["b"].Double, (byte)dict["a"].Double);
            }
            return new Color32(0, 0, 0, 0);
        }
        
        public static Vector4 GetVector4(this DataToken jToken)
        {
            DataDictionary dict = jToken.DataDictionary;
            if (dict != null)
            {
                return new Vector4((float)dict["x"].Double, (float)dict["y"].Double, (float)dict["z"].Double, (float)dict["w"].Double);
            }
            return Vector4.zero;
        }
        
        
        #endregion
    }
}