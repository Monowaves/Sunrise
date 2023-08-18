using UnityEditor;
using UnityEngine;
using InMotion;
using System.Collections.Generic;
using InMotion.Utilities;

[System.Serializable]
[CustomEditor(typeof(Variant))]
public class VariantEditor : Editor
{
    Variant current;

    private SerializedProperty _directions;
    private float _height;

    private void OnEnable() 
    {
        current = (Variant)target;

        _directions = serializedObject.FindProperty("Directions");
    }

    public override void OnInspectorGUI() 
    {
        serializedObject.Update();

        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        _directions.enumValueIndex = (int)(MotionDirections)EditorGUILayout.EnumPopup("Motion Directions", (MotionDirections)_directions.enumValueIndex);

        EditorGUILayout.Space(10);

        EditorGUI.DrawRect(new Rect(0, 57, inspectorWidth, _height), new Color32(45, 45, 45, 255));
        _height = 0;
        
        EditorGUILayout.LabelField("Frames: " + current.FramesContainer.Count);
        IncreaseHeight(EditorGUIUtility.singleLineHeight);

        EditorGUILayout.Space(5);
        IncreaseHeight(5);

        int drawFields = 0;

        switch (current.Directions)
        {
            case MotionDirections.Simple:
                drawFields = 1;
                break;

            case MotionDirections.Platformer:
                drawFields = 2;
                break;

            case MotionDirections.FourDirectional:
                drawFields = 4;
                break;

            case MotionDirections.EightDirectional:
                drawFields = 8;
                break;
        }

        for (int x = 0; x < current.FramesContainer.Count; x++)
        {
            EditorGUILayout.LabelField("Frame " + (x + 1));
            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            if (current.FramesContainer[x].sprites.Count < drawFields)
            {
                int add = drawFields - current.FramesContainer[x].sprites.Count;

                for (int a = 0; a < add; a++)
                {
                    current.FramesContainer[x].sprites.Add(null);
                }
            }

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < drawFields; i++)
            {
                string directionName = DirectionUtility.DefineDirection((MotionDirections)_directions.enumValueIndex, i).Item1;

                float width = (inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields));
                EditorGUILayout.LabelField(directionName, new GUILayoutOption[]{ GUILayout.Width(width) });
            }

            EditorGUILayout.EndHorizontal();

            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < drawFields; i++)
            {
                Vector2Int direction = DirectionUtility.DefineDirection((MotionDirections)_directions.enumValueIndex, i).Item2;

                float width = (inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields));
                EditorGUILayout.LabelField($"x: {direction.x} y: {direction.y}", new GUILayoutOption[]{ GUILayout.Width(width) });
            }

            EditorGUILayout.EndHorizontal();

            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < drawFields; i++)
            {
                float height = 400 / drawFields;
                float width = inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields);
                
                GUI.color = Color.clear;
                current.FramesContainer[x].sprites[i] = (Sprite)EditorGUILayout.ObjectField(current.FramesContainer[x].sprites[i], typeof(Sprite), true, new GUILayoutOption[]{ GUILayout.Width(width), GUILayout.Height(height)});
                GUI.color = Color.white;

                GUI.Button(GUILayoutUtility.GetLastRect(), "");
                
                if (current.FramesContainer[x].sprites[i] != null)
                {
                    Rect spriteRect = current.FramesContainer[x].sprites[i].rect;
                    Texture2D tex = current.FramesContainer[x].sprites[i].texture;
                    GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetLastRect(), tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width/ tex.width, spriteRect.height / tex.height));
                }

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.LowerRight
                };

                GUI.Button(new Rect(GUILayoutUtility.GetLastRect().x + width - 45, GUILayoutUtility.GetLastRect().y + height - 15, 45, 15), "Select", buttonStyle);
            }

            EditorGUILayout.EndHorizontal();

            IncreaseHeight(400 / drawFields);

            EditorGUILayout.Space(5);
            IncreaseHeight(5);
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create"))
        {
            current.FramesContainer.Add(new DirectionalSprite(new List<Sprite>()));
        }

        if (GUILayout.Button("Delete") && current.FramesContainer.Count > 0)
        {
            current.FramesContainer.RemoveAt(current.FramesContainer.Count - 1);
        }

        EditorGUILayout.EndHorizontal();

        IncreaseHeight(EditorGUIUtility.singleLineHeight);

        serializedObject.ApplyModifiedProperties();
    }

    private void IncreaseHeight(float amount)
    {
        _height += amount + 2;
    }
}
