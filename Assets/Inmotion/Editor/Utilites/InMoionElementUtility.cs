using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace InMotion.EditorOnly.Utilities
{
    public static class InMotionElementUtility
    {
        public static TextField InsertTextField(string value = "",
                                                EventCallback<ChangeEvent<string>> onValueChanged = null,
                                                string styleClass = "mott-node__textfield")
        {
            TextField inserting = new()
            {
                value = value
            };

            if (onValueChanged != null)
            {
                inserting.RegisterValueChangedCallback(onValueChanged);
            }

            inserting.AddToClassList(styleClass);

            return inserting;
        }

        public static ObjectField InsertObjectField(Type objectType,
                                                    UnityEngine.Object value = null,
                                                    EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null,
                                                    string styleClass = "mott-node__objectfield")
        {
            ObjectField inserting = new()
            {
                objectType = objectType,
                value = value
            };

            if (onValueChanged != null)
            {
                inserting.RegisterValueChangedCallback(onValueChanged);
            }

            inserting.AddToClassList(styleClass);

            return inserting;
        }

        public static Button InsertButton(string text = "button",
                                          Action onClick = null,
                                          string styleClass = "mott-node__button")
        {
            Button inserting = new(onClick)
            {
                text = text
            };

            inserting.AddToClassList(styleClass);

            return inserting;
        }

        public static Port InsertPort(this Node node,
                                      Direction dir,
                                      string portName = "port",
                                      Orientation ort = Orientation.Horizontal,
                                      Port.Capacity capacity = Port.Capacity.Single)
        {
            Port inserting = node.InstantiatePort(ort, dir, capacity, typeof(bool));

            inserting.portName = portName;

            return inserting;
        }
    }
}
