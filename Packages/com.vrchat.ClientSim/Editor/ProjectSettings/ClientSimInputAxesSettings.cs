using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRC.SDK3.ClientSim
{
    public class ClientSimInputAxesSettings : ScriptableObject, IEquatable<ClientSimInputAxesSettings>
    {
        // Name copying Unity's Input settings
        public List<ClientSimInputAxis> m_Axes = new List<ClientSimInputAxis>();

        public void LoadFromSerializedObject(SerializedObject serializedObject)
        {
            m_Axes.Clear();
            SerializedProperty axesProp = serializedObject.FindProperty(nameof(m_Axes));
            for (int curAxis = 0; curAxis < axesProp.arraySize; ++curAxis)
            {
                SerializedProperty inputAxisProperty = axesProp.GetArrayElementAtIndex(curAxis);
                m_Axes.Add(new ClientSimInputAxis(inputAxisProperty));
            }
        }
        
        public bool Equals(ClientSimInputAxesSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (m_Axes.Count != other.m_Axes.Count)
            {
                return false;
            }

            for (int cur = 0; cur < m_Axes.Count; ++cur)
            {
                if (!m_Axes[cur].Equals(other.m_Axes[cur]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientSimInputAxesSettings) obj);
        }

        public override int GetHashCode()
        {
            return (m_Axes != null ? m_Axes.GetHashCode() : 0);
        }
        
        [Serializable]
        public class ClientSimInputAxis : IEquatable<ClientSimInputAxis>
        {
            [FormerlySerializedAs("name")] 
            public string m_Name;
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

            public ClientSimInputAxis() { }

            public ClientSimInputAxis(SerializedProperty inputAxisProperty)
            {
                SerializedProperty nameProp = inputAxisProperty.FindPropertyRelative(nameof(m_Name));
                SerializedProperty descriptiveNameProp = inputAxisProperty.FindPropertyRelative(nameof(descriptiveName));
                SerializedProperty descriptiveNegativeNameProp = inputAxisProperty.FindPropertyRelative(nameof(descriptiveNegativeName));
                SerializedProperty negativeButtonProp = inputAxisProperty.FindPropertyRelative(nameof(negativeButton));
                SerializedProperty positiveButtonProp = inputAxisProperty.FindPropertyRelative(nameof(positiveButton));
                SerializedProperty altNegativeButtonProp = inputAxisProperty.FindPropertyRelative(nameof(altNegativeButton));
                SerializedProperty altPositiveButtonProp = inputAxisProperty.FindPropertyRelative(nameof(altPositiveButton));
                SerializedProperty gravityProp = inputAxisProperty.FindPropertyRelative(nameof(gravity));
                SerializedProperty deadProp = inputAxisProperty.FindPropertyRelative(nameof(dead));
                SerializedProperty sensitivityProp = inputAxisProperty.FindPropertyRelative(nameof(sensitivity));
                SerializedProperty snapProp = inputAxisProperty.FindPropertyRelative(nameof(snap));
                SerializedProperty invertProp = inputAxisProperty.FindPropertyRelative(nameof(invert));
                SerializedProperty typeProp = inputAxisProperty.FindPropertyRelative(nameof(type));
                SerializedProperty axisProp = inputAxisProperty.FindPropertyRelative(nameof(axis));
                SerializedProperty joyNumProp = inputAxisProperty.FindPropertyRelative(nameof(joyNum));

                m_Name = nameProp.stringValue;
                descriptiveName = descriptiveNameProp.stringValue;
                descriptiveNegativeName = descriptiveNegativeNameProp.stringValue;
                negativeButton = negativeButtonProp.stringValue;
                positiveButton = positiveButtonProp.stringValue;
                altNegativeButton = altNegativeButtonProp.stringValue;
                altPositiveButton = altPositiveButtonProp.stringValue;
                gravity = gravityProp.floatValue;
                dead = deadProp.floatValue;
                sensitivity = sensitivityProp.floatValue;
                snap = snapProp.boolValue;
                invert = invertProp.boolValue;
                type = typeProp.intValue;
                axis = axisProp.intValue;
                joyNum = joyNumProp.intValue;
            }

            public bool Equals(ClientSimInputAxis other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return m_Name == other.m_Name 
                       && descriptiveName == other.descriptiveName 
                       && descriptiveNegativeName == other.descriptiveNegativeName 
                       && negativeButton == other.negativeButton 
                       && positiveButton == other.positiveButton 
                       && altNegativeButton == other.altNegativeButton 
                       && altPositiveButton == other.altPositiveButton 
                       && Mathf.Approximately(gravity, other.gravity) 
                       && Mathf.Approximately(dead, other.dead) 
                       && Mathf.Approximately(sensitivity, other.sensitivity) 
                       && snap == other.snap 
                       && invert == other.invert 
                       && type == other.type 
                       && axis == other.axis 
                       && joyNum == other.joyNum;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ClientSimInputAxis) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (m_Name != null ? m_Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (descriptiveName != null ? descriptiveName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (descriptiveNegativeName != null ? descriptiveNegativeName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (negativeButton != null ? negativeButton.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (positiveButton != null ? positiveButton.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (altNegativeButton != null ? altNegativeButton.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (altPositiveButton != null ? altPositiveButton.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ gravity.GetHashCode();
                    hashCode = (hashCode * 397) ^ dead.GetHashCode();
                    hashCode = (hashCode * 397) ^ sensitivity.GetHashCode();
                    hashCode = (hashCode * 397) ^ snap.GetHashCode();
                    hashCode = (hashCode * 397) ^ invert.GetHashCode();
                    hashCode = (hashCode * 397) ^ type;
                    hashCode = (hashCode * 397) ^ axis;
                    hashCode = (hashCode * 397) ^ joyNum;
                    return hashCode;
                }
            }
        }
    }
}
