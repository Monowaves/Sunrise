using InMotion.Engine;
using UnityEditor;
using UnityEngine;

namespace InMotion.EditorOnly.Drawers
{
    [CustomPropertyDrawer(typeof(Callbacks))]
    public class CallbacksPropertyDrawer : PropertyDrawer
    {
        const string KeysFieldName = "m_keys";
    	const string ValuesFieldName = "m_values";
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
    
            var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
    		var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);
    
            EditorGUI.EndProperty();
        }
    }
}
