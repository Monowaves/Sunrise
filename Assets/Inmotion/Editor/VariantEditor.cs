using UnityEditor;
using UnityEngine;
using InMotion;
using InMotion.Utilities;
using System.IO;
using System.Collections.Generic;
using System;

[System.Serializable]
[CustomEditor(typeof(Variant))]
public class VariantEditor : Editor
{
    Variant current;

    private SerializedProperty _directions;
    private SerializedProperty _frames;
    private float _height;

    private void OnEnable() 
    {
        current = (Variant)target;

        _directions = serializedObject.FindProperty("Directions");
        _frames = serializedObject.FindProperty("FramesContainer");
    }

    public override void OnInspectorGUI() 
    {
        serializedObject.Update();

        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        _directions.enumValueIndex = (int)(MotionDirections)EditorGUILayout.EnumPopup("Motion Directions", (MotionDirections)_directions.enumValueIndex);

        EditorGUILayout.Space(10);

        EditorGUI.DrawRect(new Rect(0, 57.5f, inspectorWidth, _height), new Color32(45, 45, 45, 255));
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

        if (current.FramesContainer.Count == 0)
        {
            if (GUILayout.Button("Load Sheet")) 
            {
                for (int fieldIdx = 0; fieldIdx < drawFields; fieldIdx++)
                {
                    string direction = DirectionUtility.DefineDirection(current.Directions, fieldIdx).Item1;
                    string texturePath = EditorUtility.OpenFilePanel($"Select sprite sheet for {direction} direction", "", "png,jpg,jpeg");
    
                    if (!string.IsNullOrEmpty(texturePath))
                    {
                        string relativePath = texturePath[Application.dataPath.Length..];
                        string assetPath = "Assets" + relativePath;
    
                        UnityEngine.Object[] loadedContent = AssetDatabase.LoadAllAssetsAtPath(assetPath);
    
                        List<Sprite> sprites = new();
                        foreach (var content in loadedContent)
                        {
                            if (AssetDatabase.IsSubAsset(content)) sprites.Add((Sprite)content);
                        }

                        sprites.Sort((first, second) => 
                        {
                            int firstNumber = Convert.ToInt32(first.name[^1]);
                            int secondNumber = Convert.ToInt32(second.name[^1]);

                            if (firstNumber > secondNumber) return 1;
                            else if (firstNumber < secondNumber) return -1;
                            else return 0;
                        });

                        for (int spriteIdx = 0; spriteIdx < sprites.Count; spriteIdx++)
                        {
                            if (current.FramesContainer.Count <= spriteIdx)
                            {
                                DirectionalSprite directionalSprite = new();
                                directionalSprite.Sprites[fieldIdx] = sprites[spriteIdx];
                                current.FramesContainer.Add(directionalSprite);
                            }
                            else
                            {
                                current.FramesContainer[spriteIdx].Sprites[fieldIdx] = sprites[spriteIdx];
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("New Frame"))
            {
                current.FramesContainer.Add(new DirectionalSprite());
            }

            IncreaseHeight(EditorGUIUtility.singleLineHeight + 3);
        }

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
            IncreaseHeight(225 / drawFields);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < drawFields; i++)
            {
                SetSpriteAt(frame, i, (Sprite)EditorGUILayout.ObjectField
                (
                    obj: GetSpriteAt(frame, i), 
                    objType: typeof(Sprite), 
                    allowSceneObjects: true
                ));
            }
            EditorGUILayout.EndHorizontal();
            IncreaseHeight(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Insert Frame"))
            {
                current.FramesContainer.Insert(frame + 1, new DirectionalSprite());
            }

            if (GUILayout.Button("Remove Frame"))
            {
                current.FramesContainer.RemoveAt(frame);
            }
            EditorGUILayout.EndHorizontal();
            IncreaseHeight(EditorGUIUtility.singleLineHeight);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawField(int drawFields, int frame, int sprite)
    {
        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        float fieldHeight = 225 / drawFields;
        float fieldWidth = inspectorWidth / drawFields - ((35 + drawFields * 2) / drawFields);

        GUI.color = Color.clear;
        SetSpriteAt(frame, sprite, (Sprite)EditorGUILayout.ObjectField
        (
            obj: GetSpriteAt(frame, sprite), 
            objType: typeof(Sprite), 
            allowSceneObjects: true, 
            options: new GUILayoutOption[] 
            { 
                GUILayout.Width(fieldWidth), 
                GUILayout.Height(fieldHeight),
            }
        ));
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

    private Sprite GetSpriteAt(int frame, int field)
    {
        if (_frames.arraySize <= frame) return null;

        SerializedProperty directionalSprite = _frames.GetArrayElementAtIndex(frame);

        if (directionalSprite == null) return null;
        
        SerializedProperty sprites = directionalSprite.FindPropertyRelative("Sprites");

        return sprites.GetArrayElementAtIndex(field).objectReferenceValue as Sprite;
    }

    private void SetSpriteAt(int frame, int field, Sprite value)
    {
        if (_frames.arraySize <= frame) return;

        SerializedProperty directionalSprite = _frames.GetArrayElementAtIndex(frame);

        if (directionalSprite == null) return;

        SerializedProperty sprites = directionalSprite.FindPropertyRelative("Sprites");

        sprites.GetArrayElementAtIndex(field).objectReferenceValue = value;
    }

    private void IncreaseHeight(float amount)
    {
        _height += amount + 2;
    }
}