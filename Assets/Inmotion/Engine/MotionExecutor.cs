using System;
using InMotion.SO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InMotion.Utilities;

namespace InMotion.Tools.RuntimeScripts
{
    public class MotionExecutor : MonoBehaviour
    {
        [Header("Base Settings")]
        public SpriteRenderer Target;
        public MotionTree TargetMotionTree;
        [Min(1)] public int GlobalFramerate = 10;

        [Header("Info")]
        [ShowOnly] [SerializeField] private int _globalFrameCounter;
        [ReadOnly] public Vector2Int Direction;
        [ReadOnly] public int VariantIndex;

        private float _updateFrametime;

        private bool _saveDataExisting;

        private bool _proccesing;
        private NodeScriptableObject _currentNode;
        private Motion _playThis;

        private bool _breaked;

        private int _localVariantIndex;

        public Action OnAnimationEnd;
        public Action OnAnimationStart;
        public Action OnAnimationFrame;
        
        private void OnValidate() 
        {
            TryGetComponent<SpriteRenderer>(out Target);
        }

        private void Start() 
        {
            if (TargetMotionTree.SavedData != null)
            {
                _saveDataExisting = true;

                ProccesStart();
            }
        }

        private void Update() 
        {
            if (!_saveDataExisting)
            {
                Debug.LogWarning("There is nothing to execute in " + TargetMotionTree.name);
                return;
            }

            if (_breaked) { return; }

            _updateFrametime -= Time.deltaTime;

            if (_updateFrametime <= 0)
            {
                _globalFrameCounter++;
                _updateFrametime = (1 / Convert.ToSingle(GlobalFramerate));
                
                OnFrameUpdate();
            }
        }

        public void SetMotion(InMotion.Motion target)
        {
            _playThis = target;
            _localVariantIndex = Mathf.Clamp(VariantIndex, 0, target.Variants.Count() - 1);
        }

        public void ReturnProcces()
        {
            ProccesStop();
            _currentNode = null;
            ProccesStart();
        }

        public void BreakAll()
        {
            ProccesStop();
            _breaked = true;
        }

        private void ProccesStart()
        {
            _proccesing = true;
            ProccesUpdate();
        }

        private void ProccesStop()
        {
            _proccesing = false;
        }

        private void ProccesUpdate()
        {
            if (_currentNode == null)
            {
                if (TargetMotionTree.SavedData.RootNext == null)
                {
                    BreakAll();
                    return;
                }

                _currentNode = TargetMotionTree.SavedData.RootNext;
            }
            else if (_currentNode is MotionNodeScriptableObject)
            {
                MotionNodeScriptableObject typedNode = (MotionNodeScriptableObject)_currentNode;

                if (typedNode.Next == null)
                {
                    BreakAll();
                    return;
                }

                SetMotion(typedNode.TargetMotion);

                _currentNode = typedNode.Next;

                ProccesStop();
                return;
            }
            else if (_currentNode is BranchNodeScriptableObject)
            {
                BranchNodeScriptableObject typedNode = (BranchNodeScriptableObject)_currentNode;

                if (Conditioner.StringToCondition(typedNode.Condition, TargetMotionTree.RegisteredParameters.ToArray()))
                {
                    if (typedNode.True == null)
                    {
                        BreakAll();
                        return;
                    }

                    _currentNode = typedNode.True;
                }
                else
                {
                    if (typedNode.False == null)
                    {
                        BreakAll();
                        return;
                    }

                    _currentNode = typedNode.False;
                }
            }
            else
            {
                _currentNode = null;
            }
            
            if (_proccesing)
            {
                ProccesUpdate();
            }
        }

        private void OnFrameUpdate()
        {
            InvokeAction(OnAnimationFrame);

            if (_playThis != null)
            {
                List<DirectionalSprite> framesContainer = _playThis.Variants[_localVariantIndex].FramesContainer;
                int dirIdx = DirectionUtility.DefineDirectionIndex(Direction);

                Target.sprite = framesContainer[_globalFrameCounter % _playThis.Variants[0].FramesContainer.Count].sprites[dirIdx];

                if (Target.sprite == framesContainer.Last().sprites[dirIdx])
                {
                    InvokeAction(OnAnimationEnd);

                    ProccesStart();
                }

                if (Target.sprite == framesContainer.First().sprites[dirIdx])
                {
                    InvokeAction(OnAnimationStart);

                    ProccesStart();
                }
            }
        }

        private void InvokeAction(Action target)
        {
            if (target != null)
            {
                target.Invoke();
            }
        }
    }
}

public class ShowOnlyAttribute : PropertyAttribute {}
public class ReadOnlyAttribute : PropertyAttribute {}

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string valueStr;

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                valueStr = prop.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                valueStr = prop.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                valueStr = prop.floatValue.ToString("0.00000");
                break;
            case SerializedPropertyType.String:
                valueStr = prop.stringValue;
                break;
            default:
                valueStr = "(not supported)";
                break;
        }

        EditorGUI.LabelField(position,label.text, valueStr);
    }
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
