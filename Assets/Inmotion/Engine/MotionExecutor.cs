using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InMotion.Utilities;
using UnityEngine.Events;
using InMotion.SO;

namespace InMotion.Engine
{
    public class MotionExecutor : MonoBehaviour
    {
        public MotionTree MotionTree;
        
        public SpriteRenderer Target;
        public int Framerate = 10;

        public InMotionDictionary<string, UnityEvent> Callbacks = new();

        public int MotionFrame { get; private set; }
        public int MotionFramerate { get; private set; }
        public Vector2Int Direction { get; private set; }
        public int VariantIndex { get; private set; }

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
            if (!MotionTree) throw new Exception("Target motion tree is null!");

            if (MotionTree.SavedData != null)
            {
                _saveDataExisting = true;

                ProccesStart();
            }
        }

        private void Update() 
        {
            if (!_saveDataExisting) throw new Exception("There is nothing to execute in " + MotionTree.name);

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
            if (MotionTree.Parameters[key] != value.ToString())
            {
                MotionTree.Parameters[key] = value.ToString();
                Restart();
            }
        }

        public void SetMotion(Motion target)
        {
            if (_playThis == target) return;

            _playThis = target;
            _isFinishedMotion = false;
            MotionFrame = 0;

            MotionFramerate = target.UseCustomFramerate ? target.Framerate : Framerate;
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
                if (MotionTree.SavedData.RootNext == null)
                {
                    Terminate();
                    return;
                }

                _currentNode = MotionTree.SavedData.RootNext;
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
                if (Conditioner.StringToCondition(typedNode.Condition, MotionTree.RegisteredParameters.ToArray()))
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
                
                if (framesContainer.Count == 0)
                    throw new Exception($"Variant with index {VariantIndex} in motion {_playThis.name} does not contain any frames!");

                int dirIdx = DirectionUtility.DefineDirectionIndex(Direction);

                Target.sprite = framesContainer[MotionFrame].Sprites[dirIdx];

                string callback = framesContainer[MotionFrame].Callback;
                if (!string.IsNullOrEmpty(callback))
                {
                    if (Callbacks.ContainsKey(callback))
                        Callbacks[callback].Invoke();
                }

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
            }
        }
    }
}

[Serializable]
public class CallbackExecutor
{
    public string Callback;
    public UnityEvent Action = new();
}
