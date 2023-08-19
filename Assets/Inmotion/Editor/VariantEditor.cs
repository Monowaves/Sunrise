using UnityEditor;
using UnityEngine;
using InMotion;
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

        int drawFields = current.Directions switch
        {
            MotionDirections.Simple => 1,
            MotionDirections.Platformer => 2,
            MotionDirections.FourDirectional => 4,
            MotionDirections.EightDirectional => 8,
            _ => 0
        };

        for (int frame = 0; frame < current.FramesContainer.Count; frame++)
        {
            EditorGUILayout.LabelField("Frame " + (frame + 1));
            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.grey;
            for (int i = 0; i < drawFields; i++)
            {
                string directionName = DirectionUtility.DefineDirection((MotionDirections)_directions.enumValueIndex, i).Item1;

                float width = inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields);
                EditorGUILayout.LabelField(directionName, new GUILayoutOption[]{ GUILayout.Width(width) });
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < drawFields; i++)
            {
                DrawField(drawFields, frame, i);
            }
            EditorGUILayout.EndHorizontal();
            IncreaseHeight(300 / drawFields);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < drawFields; i++)
            {
                GetSpriteAt(frame, i) = (Sprite)EditorGUILayout.ObjectField
                (
                    obj: GetSpriteAt(frame, i), 
                    objType: typeof(Sprite), 
                    allowSceneObjects: true
                );
            }
            EditorGUILayout.EndHorizontal();
            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Insert"))
            {
                current.FramesContainer.Insert(frame + 1, new DirectionalSprite());
            }

            if (current.FramesContainer.Count > 1 && GUILayout.Button("Remove"))
            {
                current.FramesContainer.RemoveAt(frame);
            }
            EditorGUILayout.EndHorizontal();

            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.Space(5);
            IncreaseHeight(5);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawField(int drawFields, int frame, int sprite)
    {
        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        float fieldHeight = 300 / drawFields;
        float fieldWidth = inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields);

        GUI.color = Color.clear;
        GetSpriteAt(frame, sprite) = (Sprite)EditorGUILayout.ObjectField
        (
            obj: GetSpriteAt(frame, sprite), 
            objType: typeof(Sprite), 
            allowSceneObjects: true, 
            options: new GUILayoutOption[] 
            { 
                GUILayout.Width(fieldWidth), 
                GUILayout.Height(fieldHeight),
            }
        );
        GUI.color = Color.white;

        Rect lastRect = GUILayoutUtility.GetLastRect();

        Sprite spriteInfo = GetSpriteAt(frame, sprite);
        if (spriteInfo)
        {
            Vector2 spriteRatio = new(spriteInfo.rect.width / spriteInfo.rect.height, 1);

            Texture2D texture = spriteInfo.texture;

            float boundsWidth = spriteInfo.rect.width / texture.width;
            float boundsHeight = spriteInfo.rect.height / texture.height;
            float spriteX = spriteInfo.rect.x / texture.width;
            float spriteY = spriteInfo.rect.y / texture.height;
            float spriteWidth = fieldHeight * spriteRatio.x;
            float spriteHeight = fieldHeight;

            GUI.DrawTextureWithTexCoords(new Rect(lastRect.x + (fieldWidth / 2 - spriteWidth / 2), lastRect.y, spriteWidth, spriteHeight), texture, 
            new Rect(spriteX, spriteY, boundsWidth, boundsHeight));
        }
    }

    private ref Sprite GetSpriteAt(int frame, int field)
    {
        return ref current.FramesContainer[frame].Sprites[field];
    }

    private void IncreaseHeight(float amount)
    {
        _height += amount + 2;
    }
}
