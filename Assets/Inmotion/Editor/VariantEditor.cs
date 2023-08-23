using UnityEditor;
using UnityEngine;
using InMotion.Engine;
using InMotion.Utilities;
using System.Collections.Generic;
using System;
using System.IO;

namespace InMotion.EditorOnly.Drawers
{
    [Serializable]
    [CustomEditor(typeof(Variant))]
    public class VariantEditor : Editor
    {
        Variant current;
    
        private SerializedProperty _directions;
        private SerializedProperty _frames;
        private float _height;
        private Texture2D _frameBackground;

        private bool _isLoadingSheet;
    
        private void OnEnable() 
        {
            current = (Variant)target;
    
            _directions = serializedObject.FindProperty("Directions");
            _frames = serializedObject.FindProperty("FramesContainer");
    
            _frameBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/InMotion/Images/FrameBackground.png");
        }
    
        public override void OnInspectorGUI() 
        {
            if (_isLoadingSheet) return;

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
    
            EditorGUILayout.Space(12);
            IncreaseHeight(12);
    
            if (GUILayout.Button("Load Sheet")) 
            {
                LoadSheet(drawFields);
            }
            IncreaseHeight((EditorGUIUtility.singleLineHeight + 3) / 2);
    
            if (current.FramesContainer.Count > 0)
            {
                if (GUILayout.Button("Clear All")) 
                {
                    ClearAll();
                }
    
                IncreaseHeight((EditorGUIUtility.singleLineHeight + 3) / 2);
            }
    
            if (current.FramesContainer.Count == 0)
            {
                if (GUILayout.Button("New Frame"))
                {
                    current.FramesContainer.Add(new Frame());
                }
    
                IncreaseHeight((EditorGUIUtility.singleLineHeight + 3) / 2);
            }
    
            EditorGUILayout.Space(6);
            IncreaseHeight(6);
    
            for (int frame = 0; frame < current.FramesContainer.Count; frame++)
            {
                EditorGUILayout.LabelField("Frame " + (frame + 1));
                IncreaseHeight(EditorGUIUtility.singleLineHeight);
    
                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.grey;
                for (int i = 0; i < drawFields; i++)
                {
                    string directionName = DirectionUtility.DefineDirection((MotionDirections)_directions.enumValueIndex, i).Item1;
                    
                    EditorGUILayout.LabelField(directionName, new GUILayoutOption[]{ GUILayout.MinWidth(50f) });
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
                IncreaseHeight(225 / drawFields + 10);
    
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
                EditorGUILayout.LabelField("Callback:", new GUILayoutOption[] { GUILayout.MaxWidth(100) });
                SetCallbackAt(frame, EditorGUILayout.TextField(GetCallbackAt(frame)));
                EditorGUILayout.EndHorizontal();
                IncreaseHeight(EditorGUIUtility.singleLineHeight);
    
                EditorGUILayout.Space(5);
                IncreaseHeight(5);
    
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Insert Frame"))
                {
                    current.FramesContainer.Insert(frame + 1, new Frame());
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
    
        private void LoadSheet(int drawFields)
        {
            _isLoadingSheet = true;

            List<Frame> output = new();
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
                        if (output.Count <= spriteIdx)
                        {
                            Frame directionalSprite = new();
                            directionalSprite.Sprites[fieldIdx] = sprites[spriteIdx];
                            output.Add(directionalSprite);
                        }
                        else
                        {
                            output[spriteIdx].Sprites[fieldIdx] = sprites[spriteIdx];
                        }
                    }
                }
                else return;
            }

            SetFramesContainer(output);

            _isLoadingSheet = false;
        }
    
        private void ClearAll() => _frames.ClearArray();
    
        private void DrawField(int drawFields, int frame, int sprite)
        {
            float fieldHeight = 225 / drawFields;
    
            GUI.color = Color.clear;
            SetSpriteAt(frame, sprite, (Sprite)EditorGUILayout.ObjectField
            (
                obj: GetSpriteAt(frame, sprite), 
                objType: typeof(Sprite), 
                allowSceneObjects: true, 
                options: new GUILayoutOption[] 
                { 
                    GUILayout.Height(fieldHeight),
                }
            ));
            GUI.color = Color.white;
    
            Rect lastRect = GUILayoutUtility.GetLastRect();
    
            EditorGUI.DrawTextureTransparent(lastRect, _frameBackground);
    
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
    
                GUI.DrawTextureWithTexCoords(new Rect(lastRect.center.x - spriteWidth / 2, lastRect.y, spriteWidth, spriteHeight), texture, 
                new Rect(spriteX, spriteY, boundsWidth, boundsHeight));
            }
        }
    
        private Sprite GetSpriteAt(int frame, int field)
        {
            if (_frames.arraySize <= frame) return null;
    
            SerializedProperty frameProperty = _frames.GetArrayElementAtIndex(frame);
    
            if (frameProperty == null) return null;
            
            SerializedProperty sprites = frameProperty.FindPropertyRelative("Sprites");

            if (sprites.arraySize <= field) return null;
    
            return sprites.GetArrayElementAtIndex(field).objectReferenceValue as Sprite;
        }
    
        private void SetSpriteAt(int frame, int field, Sprite value)
        {
            if (_frames.arraySize <= frame) return;
    
            SerializedProperty frameProperty = _frames.GetArrayElementAtIndex(frame);
    
            if (frameProperty == null) return;
    
            SerializedProperty sprites = frameProperty.FindPropertyRelative("Sprites");

            if (sprites.arraySize <= field) return;
    
            sprites.GetArrayElementAtIndex(field).objectReferenceValue = value;
        }

        private void SetFramesContainer(List<Frame> newValue)
        {
            _frames.ClearArray();

            for (int i = 0; i < newValue.Count; i++)
            {
                _frames.InsertArrayElementAtIndex(i);

                SerializedProperty fields = _frames.GetArrayElementAtIndex(i).FindPropertyRelative("Sprites");

                for (int field = 0; field < newValue[i].Sprites.Length; field++)
                {
                    fields.InsertArrayElementAtIndex(field);
                    fields.GetArrayElementAtIndex(field).objectReferenceValue = newValue[i].Sprites[field];
                }
            }
        }
    
        private string GetCallbackAt(int frame)
        {
            if (_frames.arraySize <= frame) return null;
    
            SerializedProperty frameProperty = _frames.GetArrayElementAtIndex(frame);
    
            if (frameProperty == null) return null;
            
            SerializedProperty callback = frameProperty.FindPropertyRelative("Callback");
    
            return callback.stringValue;
        }
    
        private void SetCallbackAt(int frame, string value)
        {
            if (_frames.arraySize <= frame) return;
    
            SerializedProperty frameProperty = _frames.GetArrayElementAtIndex(frame);
    
            if (frameProperty == null) return;
    
            SerializedProperty callback = frameProperty.FindPropertyRelative("Callback");
    
            callback.stringValue = value;
        }
    
        private void IncreaseHeight(float amount)
        {
            _height += amount + 2;
        }

        [MenuItem("Assets/Create/InMotion/Motion Based on Selection", false, 0)]
        public static void CreateYourScriptableObject()
        {
            if (Selection.activeObject is Variant)
            {
                Engine.Motion newObject = CreateInstance<Engine.Motion>();

                string origin = AssetDatabase.GetAssetPath(Selection.activeObject);
                string directory = Path.GetDirectoryName(origin);
                string variantName = Path.GetFileName(origin);
                string motionName = variantName.Replace("Variant", "Motion");
                string fullPath = $"{directory}/{motionName}";

                Variant current = Selection.activeObject as Variant;
                newObject.Variants.Add(current);

                if (!string.IsNullOrEmpty(fullPath))
                {
                    AssetDatabase.CreateAsset(newObject, fullPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Selection.activeObject = newObject;
                }
            }
            else
            {
                Debug.LogWarning("Selected object isn't a Variant");
            }
        }
    }
}
