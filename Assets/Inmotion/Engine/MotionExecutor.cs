using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InMotion.Utilities;
using UnityEngine.Events;

namespace InMotion.Tools.RuntimeScripts
{
    public class MotionExecutor : MonoBehaviour
    {
        [field: Header("Properties")]
        [field: SerializeField] public SpriteRenderer Target { get; private set;}
        [field: SerializeField] public MotionTree TargetMotionTree { get; private set; }
        [field: SerializeField, Min(1)] public int GlobalFramerate { get; private set; } = 10;

        [field: Header("Callbacks")]
        [field: SerializeField] public List<CallbackExecutor> Callbacks { get; private set; } = new();

        [field: Header("Info")]
        [field: SerializeField, ReadOnly] public int MotionFrame { get; private set; }
        [field: SerializeField, ReadOnly] public int MotionFramerate { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Direction { get; private set; }
        [field: SerializeField, ReadOnly] public int VariantIndex { get; private set; }

        private float _updateFrametime;

        private bool _saveDataExisting;

        private bool _proccesing;
        private NodeScriptableObject _currentNode;
        private Motion _playThis;

        private bool _terminated;

        public Action OnMotionEnd;
        public Action OnMotionStart;
        public Action OnMotionFrame;

        private bool _isFinishedMotion;
        
        private void OnValidate() 
        {
            if (TryGetComponent(out SpriteRenderer target)) Target = target;
        }

        private void Start() 
        {
            if (!TargetMotionTree) throw new Exception("Target motion tree is null!");

            if (TargetMotionTree.SavedData != null)
            {
                _saveDataExisting = true;

                ProccesStart();
            }
        }

        private void Update() 
        {
            if (!_saveDataExisting) throw new Exception("There is nothing to execute in " + TargetMotionTree.name);

            if (_terminated) return;

            _updateFrametime -= Time.deltaTime;

            if (_updateFrametime <= 0)
            {
                _updateFrametime = 1 / Convert.ToSingle(MotionFramerate);
                
                OnFrameUpdate();
            }
        }

        public void SetParameter(string key, object value)
        {
            if (TargetMotionTree.Parameters[key] != value.ToString())
            {
                TargetMotionTree.Parameters[key] = value.ToString();
                Restart();
            }
        }

        public void SetMotion(Motion target)
        {
            if (_playThis == target) return;

            _playThis = target;
            _isFinishedMotion = false;
            MotionFrame = 0;

            MotionFramerate = target.UseCustomFramerate ? target.Framerate : GlobalFramerate;
        }

        public void SetDirection(Vector2Int direction) => Direction = direction;
        public void SetVariant(int idx) => VariantIndex = idx;

        public void Terminate()
        {
            ProccesStop();
            _terminated = true;
        }

        public void Restart()
        {
            if (_terminated) return;
            
            ProccesStop();
            _currentNode = null;
            ProccesStart();
        }

        private void ProccesStart()
        {
            if (_terminated) return;

            _proccesing = true;
            ProccesUpdate();
        }

        private void ProccesStop() => _proccesing = false;

        private void ProccesUpdate()
        {
            if (_currentNode == null)
            {
                if (TargetMotionTree.SavedData.RootNext == null)
                {
                    Terminate();
                    return;
                }

                _currentNode = TargetMotionTree.SavedData.RootNext;
            }
            else if (_currentNode is MotionNodeScriptableObject)
            {
                MotionNodeScriptableObject typedNode = (MotionNodeScriptableObject)_currentNode;

                if (typedNode.Next == null)
                {
                    Terminate();
                    return;
                }

                SetMotion(typedNode.TargetMotion);

                _currentNode = typedNode.Next;

                ProccesStop();
                return;
            }
            else if (_currentNode is BranchNodeScriptableObject typedNode)
            {
                if (Conditioner.StringToCondition(typedNode.Condition, TargetMotionTree.RegisteredParameters.ToArray()))
                {
                    if (typedNode.True == null)
                    {
                        Terminate();
                        return;
                    }

                    _currentNode = typedNode.True;
                }
                else
                {
                    if (typedNode.False == null)
                    {
                        Terminate();
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
            OnMotionFrame?.Invoke();

            if (_isFinishedMotion) return;

            if (_playThis != null)
            {
                List<Frame> framesContainer = _playThis.Variants[VariantIndex].FramesContainer;
                
                if (framesContainer[MotionFrame].Sprites.Length == 0)
                    throw new Exception($"Variant with index {VariantIndex} in motion {_playThis.name} does not contain any frames!");

                int dirIdx = DirectionUtility.DefineDirectionIndex(Direction);

                Target.sprite = framesContainer[MotionFrame].Sprites[dirIdx];

                if (Target.sprite == framesContainer.First().Sprites[dirIdx])
                {
                    OnMotionStart?.Invoke();

                    ProccesStart();
                }

                if (Target.sprite == framesContainer.Last().Sprites[dirIdx])
                {
                    OnMotionEnd?.Invoke();

                    if (!_playThis.Looping) _isFinishedMotion = true;
                    MotionFrame = 0;

                    ProccesStart();
                }
                else
                {
                    MotionFrame++;
                }

                if (!string.IsNullOrEmpty(framesContainer[MotionFrame].Callback))
                {
                    int callbackExecutorIndex = Callbacks.FindIndex(callbackExecutor => callbackExecutor.Callback == framesContainer[MotionFrame].Callback);

                    if (callbackExecutorIndex != -1)
                    {
                        Callbacks[callbackExecutorIndex].Action.Invoke();
                    }
                }
            }
        }
    }
}

public class ReadOnlyAttribute : PropertyAttribute {}

[Serializable]
public class CallbackExecutor
{
    public string Callback;
    public UnityEvent Action = new();
}
