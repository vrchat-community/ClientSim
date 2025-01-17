using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim.Editor.VisualElements.Fields;
using VRC.SDK3.Data;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.EncodeDecodeEditors
{
    public class ClientSimObjectPoolEncodeDecodeEditor : IClientSimEncodeDecoderEditor
    {

        public VisualElement GenerateFields(MonoBehaviour component, DataDictionary data)
        {
            VisualElement container = new VisualElement();
            DataList values = (DataList) data["Values"];
            for(int i = 0; i < (int)data["Length"].Double; i++)
            {
                container.Add(FieldFactory.GenerateField("Object " + i , values[i].Boolean));
            }

            return container;
        }

        public void UpdateFields(MonoBehaviour component, VisualElement dataElement, DataDictionary data)
        {
            DataList values = (DataList) data["Values"];
            for(int i = 0; i < (int)data["Length"].Double; i++)
            {
                FieldFactory.UpdateField(dataElement[i], values[i].Boolean);
            }
        }
    }
}