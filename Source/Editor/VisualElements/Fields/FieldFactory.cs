using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.Fields
{
    public class FieldFactory
    {
        private static Dictionary<Type, (Func<string, VisualElement>, Action<VisualElement,object>)> _types =
            new Dictionary<Type, (Func<string, VisualElement>, Action<VisualElement,object>)>()
            {
                { typeof(byte), ((name) => new ByteField(name), 
                    (element, o) => ((ByteField)element).SetValueWithoutNotify((byte)o)) },
                { typeof(sbyte), ((name) => new SbyteField(name), 
                    (element, o) => ((SbyteField)element).SetValueWithoutNotify((sbyte)o)) },
                { typeof(double), ((name) => new DoubleField(name), 
                    (element, o) => ((DoubleField)element).SetValueWithoutNotify((double)o)) },
                { typeof(float), ((name) => new FloatField(name), 
                    (element, o) => ((FloatField)element).SetValueWithoutNotify((float)o)) },
                { typeof(int), ((name) => new IntegerField(name), 
                    (element, o) => ((IntegerField)element).SetValueWithoutNotify((int)o)) },
                { typeof(uint), ((name) => new UIntField(name), 
                    (element, o) => ((UIntField)element).SetValueWithoutNotify((uint)o)) },
                { typeof(long), ((name => new LongField(name), 
                    (element, o) => ((LongField)element).SetValueWithoutNotify((long)o))) },
                { typeof(ulong), ((name) => new ULongField(name), 
                    (element, o) => ((ULongField)element).SetValueWithoutNotify((ulong)o)) },
                { typeof(short), ((name) => new ShortField(name), 
                    (element, o) => ((ShortField)element).SetValueWithoutNotify((short)o)) },
                { typeof(ushort), ((name) => new UShortField(name), 
                    (element, o) => ((UShortField)element).SetValueWithoutNotify((ushort)o)) },
                { typeof(string), ((name) => new TextField(name), 
                    (element, o) => ((TextField)element).SetValueWithoutNotify((string)o)) },
                { typeof(char), ((name) => new TextField(name) { maxLength = 1 }, 
                    (element, o) => ((TextField)element).SetValueWithoutNotify(o.ToString())) },
                { typeof(bool), ((name) => new Toggle(name), 
                    (element, o) => ((Toggle)element).SetValueWithoutNotify((bool)o)) },
                { typeof(Vector2), ((name) => new Vector2Field(name),
                    (element, o) => ((Vector2Field)element).SetValueWithoutNotify((Vector2)o))},
                { typeof(Vector3), ((name) => new Vector3Field(name),
                    (element, o) => ((Vector3Field)element).SetValueWithoutNotify((Vector3)o))},
                { typeof(Vector4), ((name) => new Vector4Field(name),
                    (element, o) => ((Vector4Field)element).SetValueWithoutNotify((Vector4)o))},
                { typeof(Quaternion), ((name) => new Vector3Field(name),
                    (element, o) => ((Vector3Field)element).SetValueWithoutNotify(((Quaternion)o).eulerAngles))},
                { typeof(Color), ((name) => new ColorField(name),
                    (element, o) => ((ColorField)element).SetValueWithoutNotify((Color)o))},
                { typeof(Color32), ((name) => new ColorField(name),
                    (element, o) => ((ColorField)element).SetValueWithoutNotify((Color32)o))},
            };
        
        private static Dictionary<Type,Type> _FieldType = new Dictionary<Type, Type>()
        {
            { typeof(byte), typeof(ByteField) },
            { typeof(sbyte), typeof(SbyteField) },
            { typeof(double), typeof(DoubleField) },
            { typeof(float), typeof(FloatField) },
            { typeof(int), typeof(IntegerField) },
            { typeof(uint), typeof(UIntField) },
            { typeof(long), typeof(LongField) },
            { typeof(ulong), typeof(ULongField) },
            { typeof(short), typeof(ShortField) },
            { typeof(ushort), typeof(UShortField) },
            { typeof(string), typeof(TextField) },
            { typeof(char), typeof(TextField) },
            { typeof(bool), typeof(Toggle) },
            { typeof(Vector2), typeof(Vector2Field) },
            { typeof(Vector3), typeof(Vector3Field) },
            { typeof(Vector4), typeof(Vector4Field) },
            { typeof(Quaternion), typeof(Vector3Field) },
            { typeof(Color), typeof(ColorField) },
            { typeof(Color32), typeof(ColorField) },
        };
        
        public static VisualElement GenerateField<T>(string fieldName, T data)
        {
            Type type = data.GetType();

            if (type.IsArray)
            {
                Foldout Array = new Foldout();
                Array.text = fieldName;
                
                for (int i = 0; i < ((Array)(object)data).Length; i++)
                {
                    VisualElement field = GenerateField(i.ToString(), ((Array)(object)data).GetValue(i));
                    field.name = i.ToString();
                    Array.Add(field);
                }
                
                return Array;
            }
            
            if (_types.ContainsKey(type))
            {
                VisualElement field = _types[type].Item1(fieldName);
                field.name = fieldName;
                return field;
            }
            else
            {
                Label label = new Label(fieldName +" ("+type.Name+ "): " + data.ToString());
                label.name = fieldName;
                return label;
            }
        }
        
        public static bool UpdateField<T>(VisualElement field, T data)
        {
            Type type = data.GetType();

            bool visible = true;
            if (type.IsArray)
            {
                visible = false;
                int length = Math.Min(((Array)(object)data).Length,field.childCount);
                
                for (int i = 0; i < length; i++)
                {
                    visible |= UpdateField(field[i], ((Array)(object)data).GetValue(i));
                }
                
                if(length < field.childCount)
                {
                    for (int i = length; i < field.childCount; i++)
                    {
                        field[i].style.display = DisplayStyle.None;
                    }
                }
                else if(length > field.childCount)
                {
                    for (int i = field.childCount; i < length; i++)
                    {
                        VisualElement newField = GenerateField(i.ToString(), ((Array)(object)data).GetValue(i));
                        newField.name = i.ToString();
                        field.Add(newField);
                    }
                }
                
                return visible;
            }
            
            if (!_types.ContainsKey(type))
            {
                ((Label)field).text = field.name + "("+type.Name+ "): " + data.ToString();
                return visible;
            }
            
            // If the field is not the correct type, replace it with the correct type
            // Can happen because json deserialization deserializes int, float, double, etc as double
            if(_FieldType.ContainsKey(type))
            {
                if (field.GetType() != _FieldType[type])
                {
                    VisualElement newField = GenerateField(field.name, data);
                    field.parent.Add(newField);
                    newField.PlaceInFront(field);
                    field.RemoveFromHierarchy();
                    return true;
                }
            }
            else
            {
                Label label = new Label(field.name +" ("+type.Name+ "): " + data.ToString());
                label.name = field.name;
                field.parent.Add(label);
                field.RemoveFromHierarchy();
                return true;
            }
            
            _types[type].Item2(field, data);
            return visible;
        }
    }
}