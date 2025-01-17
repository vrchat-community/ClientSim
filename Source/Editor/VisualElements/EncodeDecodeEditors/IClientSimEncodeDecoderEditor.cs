using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.EncodeDecodeEditors
{
    public interface IClientSimEncodeDecoderEditor
    {
        public VisualElement GenerateFields(MonoBehaviour component, DataDictionary data);
        public void UpdateFields(MonoBehaviour component, VisualElement dataElement, DataDictionary data);
    }
}