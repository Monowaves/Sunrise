using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace InMotion.EditorOnly.Utilities
{
    public static class InMotionElementUtility
    {
        public static TextField InsertTextField(string value = "", EventCallback<ChangeEvent<string>> onValueChanged = null, string styleClass = "mott-node__textfield")
        {
            TextField inserting = new TextField()
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

        public static ObjectField InsertObjectField(Type objectType, string fieldName = "Sample Field", UnityEngine.Object value = null, EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null, string styleClass = "mott-node__objectfield")
        {
            ObjectField inserting = new ObjectField()
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

        public static Button InsertButton(string text = "button", Action onClick = null, string styleClass = "mott-node__button")
        {
            Button inserting = new Button(onClick)
            {
                text = text
            };

            inserting.AddToClassList(styleClass);

            return inserting;
        }

        public static Port InsertPort(this Node node, Direction dir, string portName = "port", Orientation ort = Orientation.Horizontal, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port inserting = node.InstantiatePort(ort, dir, capacity, typeof(bool));

            inserting.portName = portName;

            return inserting;
        }
    }
}
