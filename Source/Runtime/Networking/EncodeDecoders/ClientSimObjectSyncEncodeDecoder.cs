using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Interfaces;
using VRC.SDK3.Components;
using VRC.SDK3.Data;

#if UNITY_EDITOR
using BestHTTP.JSON;
using UnityEditor;
#endif

namespace VRC.SDK3.ClientSim.EncodeDecoders
{
    public class ClientSimObjectSyncEncodeDecoder : IClientSimEncodeDecoder
    {
        public DataDictionary Encode(MonoBehaviour component)
        {
            VRCObjectSync objectSync = (VRCObjectSync) component;
            Transform transform = objectSync.transform;
            Rigidbody rigidbody = objectSync.GetComponent<Rigidbody>();
            bool hasRigidbody = rigidbody != null;
            DataDictionary data = new DataDictionary();
            
            data["Position"] = transform.position.GetJTokenFromVector3();
            data["Rotation"] = transform.rotation.GetJTokenFromQuaternion();
            data["Velocity"] = hasRigidbody ? rigidbody.velocity.GetJTokenFromVector3() : "not available";
            data["IsKinematic"] = !hasRigidbody || rigidbody.isKinematic;
            data["UseGravity"] = hasRigidbody && rigidbody.useGravity;
            data["Discontinuity"] = "not available";
            data["DiscontinuityCounter"] = "not available";
            data["WasSleeping"] = hasRigidbody && rigidbody.IsSleeping();
            data["Time"] = Time.realtimeSinceStartup;
            data["HeldInHand"] = "not available";
            return data;
        }
        public void PreEncode(MonoBehaviour component)
        {
        }
        
        public void PostEncode(MonoBehaviour component, DataDictionary data)
        {
        }
        
        
        
        public bool IsManualSynced(MonoBehaviour component)
        {
            return false;
        }
        
        public bool IsDirty(MonoBehaviour component, DataDictionary data)
        {
            VRCObjectSync objectSync = (VRCObjectSync) component;
            if(objectSync == null)
            {
                return false;
            }
            Transform transform = objectSync.transform;
            Rigidbody rigidbody = objectSync.GetComponent<Rigidbody>();
            
            if(data.TryGetValue("Position", out DataToken positionToken))
            {
                if (transform.position != positionToken.GetVector3())
                {
                    return true;
                }
            }
            
            if(data.TryGetValue("Rotation", out DataToken rotationToken))
            {
                if (transform.rotation != rotationToken.GetQuaternion())
                {
                    return true;
                }
            }
            
            if(data.TryGetValue("Velocity", out DataToken velocityToken))
            {
                if (rigidbody != null && rigidbody.velocity != velocityToken.GetVector3())
                {
                    return true;
                }
            }
            
            if(data.TryGetValue("IsKinematic", out DataToken isKinematicToken))
            {
                if (rigidbody != null && rigidbody.isKinematic != isKinematicToken.Boolean)
                {
                    return true;
                }
            }
            
            if(data.TryGetValue("UseGravity", out DataToken useGravityToken))
            {
                if (rigidbody != null && rigidbody.useGravity != useGravityToken.Boolean)
                {
                    return true;
                }
            }
            
            if(data.TryGetValue("WasSleeping", out DataToken wasSleepingToken))
            {
                if (rigidbody != null && rigidbody.IsSleeping() != wasSleepingToken.Boolean)
                {
                    return true;
                }
            }
            
            return false;
        }

        public void Decode(MonoBehaviour component, DataDictionary data)
        {
            VRCObjectSync objectSync = (VRCObjectSync) component;
            Transform transform = objectSync.transform;
            Rigidbody rigidbody = objectSync.GetComponent<Rigidbody>();
            
            if (data.TryGetValue("Position", out DataToken positionToken))
            {
                transform.position = positionToken.GetVector3();
            }
            
            if (data.TryGetValue("Rotation", out DataToken rotationToken))
            {
                transform.rotation = rotationToken.GetQuaternion();
            }
            
            if (data.TryGetValue("Velocity", out DataToken velocityToken))
            {
                if (rigidbody != null)
                {
                    rigidbody.velocity = velocityToken.GetVector3();
                }
            }
            
            if (data.TryGetValue("IsKinematic", out DataToken isKinematicToken))
            {
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = isKinematicToken.Boolean;
                }
            }
            
            if (data.TryGetValue("UseGravity", out DataToken useGravityToken))
            {
                if (rigidbody != null)
                {
                    rigidbody.useGravity = useGravityToken.Boolean;
                }
            }
            
            if (data.TryGetValue("WasSleeping", out DataToken wasSleepingToken))
            {
                if (rigidbody != null)
                {
                    if (wasSleepingToken.Boolean)
                    {
                        rigidbody.Sleep();
                    }
                    else
                    {
                        rigidbody.WakeUp();
                    }
                }
            }
        }
    }
}