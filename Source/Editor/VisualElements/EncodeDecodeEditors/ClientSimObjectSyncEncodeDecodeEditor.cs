using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements.Fields;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.EncodeDecodeEditors
{
    public class ClientSimObjectSyncEncodeDecodeEditor : IClientSimEncodeDecoderEditor
    {
        private const string Discontinuitycounter = "DiscontinuityCounter";
        private const string Discontinuity = "Discontinuity";
        private const string Heldinhand = "HeldInHand";
        private const string Time = "Time";
        private const string Wassleeping = "WasSleeping";
        private const string Usegravity = "UseGravity";
        private const string Iskinematic = "IsKinematic";
        private const string Velocity = "Velocity";
        private const string Rotation = "Rotation";
        private const string FieldName = "Position";
        private const string NotAvailable = "not available";

        public VisualElement GenerateFields(MonoBehaviour component, DataDictionary data)
        {
            VisualElement dataElement = new VisualElement();

            if (data.TryGetValue(FieldName, out DataToken positionToken))
            {
                dataElement.Add(FieldFactory.GenerateField(FieldName, positionToken.GetVector3()));
            }
            
            if (data.TryGetValue(Rotation, out DataToken rotationToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Rotation: " , rotationToken.GetQuaternion()));
            }
            
            if (data.TryGetValue(Velocity, out DataToken velocityToken))
            {
                if(velocityToken.TokenType != TokenType.DataDictionary)
                    dataElement.Add(FieldFactory.GenerateField("Velocity: " , NotAvailable));
                else
                    dataElement.Add(FieldFactory.GenerateField("Velocity: " , velocityToken.GetVector3()));
            }
            
            if (data.TryGetValue(Iskinematic, out DataToken isKinematicToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Is Kinematic: " , isKinematicToken.Boolean));
            }
            
            if (data.TryGetValue(Usegravity, out DataToken useGravityToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Use Gravity: " , useGravityToken.Boolean));
            }
            
            if (data.TryGetValue(Wassleeping, out DataToken wasSleepingToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Was Sleeping: " , wasSleepingToken.Boolean));
            }
            
            if (data.TryGetValue(Time, out DataToken timeToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Time: " , (float)timeToken.Double));
            }
            
            if (data.TryGetValue(Heldinhand, out DataToken heldInHandToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Held In Hand: " , heldInHandToken.String));
            }
            
            if (data.TryGetValue(Discontinuity, out DataToken discontinuityToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Discontinuity: " , discontinuityToken.String));
            }
            
            if (data.TryGetValue(Discontinuitycounter, out DataToken discontinuityCounterToken))
            {
                dataElement.Add(FieldFactory.GenerateField("Discontinuity Counter: " , discontinuityCounterToken.String));
            }
            
            return dataElement;
        }

        public void UpdateFields(MonoBehaviour component, VisualElement dataElement, DataDictionary data)
        {
            if (data.TryGetValue(FieldName, out DataToken positionToken))
            {
                FieldFactory.UpdateField(dataElement[0], positionToken.GetVector3());
            }
            
            if (data.TryGetValue(Rotation, out DataToken rotationToken))
            {
                FieldFactory.UpdateField(dataElement[1], rotationToken.GetQuaternion());
            }
            
            if (data.TryGetValue(Velocity, out DataToken velocityToken))
            {
                if(velocityToken.TokenType != TokenType.DataDictionary)
                    FieldFactory.UpdateField(dataElement[2], NotAvailable);
                else
                    FieldFactory.UpdateField(dataElement[2], velocityToken.GetVector3());
            }
            
            if (data.TryGetValue(Iskinematic, out DataToken isKinematicToken))
            {
                FieldFactory.UpdateField(dataElement[3], isKinematicToken.Boolean);
            }
            
            if (data.TryGetValue(Usegravity, out DataToken useGravityToken))
            {
                FieldFactory.UpdateField(dataElement[4], useGravityToken.Boolean);
            }
            
            if (data.TryGetValue(Wassleeping, out DataToken wasSleepingToken))
            {
                FieldFactory.UpdateField(dataElement[5], wasSleepingToken.Boolean);
            }
            
            if (data.TryGetValue(Time, out DataToken timeToken))
            {
                FieldFactory.UpdateField(dataElement[6], (float)timeToken.Double);
            }
            
            if (data.TryGetValue(Heldinhand, out DataToken heldInHandToken))
            {
                FieldFactory.UpdateField(dataElement[7], heldInHandToken.String);
            }
            
            if (data.TryGetValue(Discontinuity, out DataToken discontinuityToken))
            {
                FieldFactory.UpdateField(dataElement[8] , discontinuityToken.String);
            }
            
            if (data.TryGetValue(Discontinuitycounter, out DataToken discontinuityCounterToken))
            {
                FieldFactory.UpdateField(dataElement[9], discontinuityCounterToken.String);
            }
        }
    }
}