#if VRC_ENABLE_PLAYER_PERSISTENCE
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Persistence
{
    public class ClientSimPlayerDataConverter : JsonConverter<ClientSimPlayerDataTypeUnion>
    {
        public override ClientSimPlayerDataTypeUnion ReadJson(JsonReader reader, Type objectType, ClientSimPlayerDataTypeUnion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            if (jsonObject["type"] == null)
            {
                Debug.LogError("Failed to deserialize PlayerData: No type found");
                return null;
            }
            if (jsonObject["value"] == null)
            {
                Debug.LogError("Failed to deserialize PlayerData: No value found");
                return null;
            }

            try
            {
                ClientSimPlayerDataType type = GetPlayerDataType(jsonObject["type"].ToString());
                object value = GetTypedValue(type, jsonObject["value"], reader, existingValue, serializer);
                return new ClientSimPlayerDataTypeUnion { Type = type, Value = value };
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse PlayerData {objectType} : {e.Message}");
                return default;
            }
        }

        private ClientSimPlayerDataType GetPlayerDataType(string typeName)
        {
            return typeName switch
            {
                "Color" => ClientSimPlayerDataType.Color,
                "Color32" => ClientSimPlayerDataType.Color32,
                "Quaternion" => ClientSimPlayerDataType.Quaternion,
                "Vector2" => ClientSimPlayerDataType.Vector2,
                "Vector3" => ClientSimPlayerDataType.Vector3,
                "Vector4" => ClientSimPlayerDataType.Vector4,
                "Bool" => ClientSimPlayerDataType.WrappedBool,
                "UByte" => ClientSimPlayerDataType.WrappedUByte,
                "Byte" => ClientSimPlayerDataType.WrappedByte,
                "Bytes" => ClientSimPlayerDataType.WrappedBytes,
                "Float" => ClientSimPlayerDataType.WrappedFloat,
                "Double" => ClientSimPlayerDataType.WrappedDouble,
                "Long" => ClientSimPlayerDataType.WrappedLong,
                "ULong" => ClientSimPlayerDataType.WrappedULong,
                "Int" => ClientSimPlayerDataType.WrappedInt,
                "UInt" => ClientSimPlayerDataType.WrappedUInt,
                "Short" => ClientSimPlayerDataType.WrappedShort,
                "UShort" => ClientSimPlayerDataType.WrappedUShort,
                "String" => ClientSimPlayerDataType.WrappedString,
                _ => ClientSimPlayerDataType.Color
            };
        }
        
        public override void WriteJson(JsonWriter writer, ClientSimPlayerDataTypeUnion value, JsonSerializer serializer)
        {
            JsonConverter converter = null;
            if (value.Type == ClientSimPlayerDataType.Color) converter = new ColorConverter();
            else if (value.Type == ClientSimPlayerDataType.Color32) converter = new Color32Converter();
            else if (value.Type == ClientSimPlayerDataType.Quaternion) converter = new QuaternionConverter();
            else if (value.Type == ClientSimPlayerDataType.Vector2) converter = new Vector2Converter();
            else if (value.Type == ClientSimPlayerDataType.Vector3) converter = new Vector3Converter();
            else if (value.Type == ClientSimPlayerDataType.Vector4) converter = new Vector4Converter();
            
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type.ToString().Replace("Wrapped", ""));
            writer.WritePropertyName("value");
            if (converter != null)
            {
                converter.WriteJson(writer, value.Value, serializer);
            }
            else
            {
                writer.WriteValue(value.Value);
            }
            writer.WriteEndObject();
        }

        private static object GetTypedValue(ClientSimPlayerDataType type, object deserializedValue, JsonReader reader, ClientSimPlayerDataTypeUnion existingValue, JsonSerializer serializer)
        {
            switch (type)
            {
                case ClientSimPlayerDataType.Color:
                    ColorUtility.TryParseHtmlString("#" + deserializedValue, out Color loadedColor);
                    return loadedColor;
                
                case ClientSimPlayerDataType.Color32:
                    ColorUtility.TryParseHtmlString("#" + deserializedValue, out Color loadedColor32);
                    return new Color32(
                        (byte)(loadedColor32.r * 255), 
                        (byte)(loadedColor32.g * 255), 
                        (byte)(loadedColor32.b * 255), 
                        (byte)(loadedColor32.a * 255));
                
                case ClientSimPlayerDataType.Quaternion:
                    JObject quaternionObj = (JObject)deserializedValue;
                    return new Quaternion(
                        (float)quaternionObj["x"], 
                        (float)quaternionObj["y"], 
                        (float)quaternionObj["z"], 
                        (float)quaternionObj["w"]);

                case ClientSimPlayerDataType.Vector2:
                    JObject vector2Obj = (JObject)deserializedValue;
                    return new Vector2(
                        (float)vector2Obj["x"], 
                        (float)vector2Obj["y"]);
                
                case ClientSimPlayerDataType.Vector3:
                    JObject vector3Obj = (JObject)deserializedValue;
                    return new Vector3(
                        (float)vector3Obj["x"], 
                        (float)vector3Obj["y"], 
                        (float)vector3Obj["z"]);
                
                case ClientSimPlayerDataType.Vector4:
                    JObject vector4Obj = (JObject)deserializedValue;
                    return new Vector4(
                        (float)vector4Obj["x"], 
                        (float)vector4Obj["y"], 
                        (float)vector4Obj["z"], 
                        (float)vector4Obj["w"]);
                
                case ClientSimPlayerDataType.WrappedBytes:
                    JValue byteArrayObj = (JValue)deserializedValue;
                    return Convert.FromBase64String((string)byteArrayObj.Value ?? string.Empty);

                case ClientSimPlayerDataType.WrappedBool: return (bool)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedUByte: return (byte)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedByte: return (sbyte)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedFloat: return (float)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedDouble: return (double)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedLong: return (long)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedULong: return (ulong)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedInt: return (int)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedUInt: return (uint)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedShort: return (short)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedUShort: return (ushort)(JValue)deserializedValue;
                case ClientSimPlayerDataType.WrappedString: return (string)(JValue)deserializedValue;
                default: return deserializedValue;
            }
        }
    }

    internal class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
                return loadedColor;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse color {objectType} : {ex.Message}");
                return default;
            }
        }

        public override void WriteJson(JsonWriter writer, Color color, JsonSerializer serializer)
        {
            string val = ColorUtility.ToHtmlStringRGBA(color);
            writer.WriteValue(val);
        }
    }
    
    internal class Color32Converter : JsonConverter<Color32>
    {
        public override Color32 ReadJson(JsonReader reader, Type objectType, Color32 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
                return loadedColor;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse color {objectType} : {ex.Message}");
                return default;
            }
        }

        public override void WriteJson(JsonWriter writer, Color32 color, JsonSerializer serializer)
        {
            string val = ColorUtility.ToHtmlStringRGBA(color);
            writer.WriteValue(val);
        }
    }
    
    internal class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try 
            {
                Debug.Log($"Quaternion: type={objectType} ({existingValue.GetType()}), val={existingValue}");

                float x = 0f, y = 0f, z = 0f, w = 0f;
                while (reader.Read())
                {
                    string propertyName = (string)reader.Value;

                    reader.Read();
                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            z = Convert.ToSingle(reader.Value);
                            break;
                        case "w":
                            w = Convert.ToSingle(reader.Value);
                            break;
                    }
                }

                return new Quaternion(x, y, z, w);
            }
        
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse quaternion {objectType} : {ex.Message}");
                return default;
            }
        }

        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }
    }
    
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                float x = 0f, y = 0f;
                while (reader.Read())
                {
                    string propertyName = (string)reader.Value;

                    reader.Read();
                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToSingle(reader.Value);
                            break;
                    }
                }

                return new Vector2(x, y);
            }
        
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse Vector2 {objectType} : {ex.Message}");
                return default;
            }
        }
        
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }
    
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                float x = 0f, y = 0f, z = 0f;
                while (reader.Read())
                {
                    string propertyName = (string)reader.Value;

                    reader.Read();
                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            z = Convert.ToSingle(reader.Value);
                            break;
                    }
                }

                return new Vector3(x, y, z);
            }
        
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse Vector3 {objectType} : {ex.Message}");
                return default;
            }
        }
        
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }
    }
    
    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try 
            {
                float x = 0f, y = 0f, z = 0f, w = 0f;
                while (reader.Read())
                {
                    string propertyName = (string)reader.Value;

                    reader.Read();
                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            z = Convert.ToSingle(reader.Value);
                            break;
                        case "w":
                            w = Convert.ToSingle(reader.Value);
                            break;
                    }
                }

                return new Vector4(x, y, z, w);
            }
        
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse Vector4 {objectType} : {ex.Message}");
                return default;
            }
        }
        
        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }
    }
}
#endif