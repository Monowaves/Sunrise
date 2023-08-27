using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InMotion.Utilities;
using UnityEngine.Events;
using InMotion.SO;
using UnityEngine.SceneManagement;

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
        private Motion _playThis;
        private bool _terminated;
        private Queue<NodeScriptableObject> _nodesQueue = new();
        private Queue<Motion> _motionsQueue = new();

        private bool HasMotion => _playThis;

        //CALLBACKS
        public Action OnMotionEnd;
        public Action OnMotionStart;
        public Action OnMotionFrame;
        
        private Dictionary<string, object> _invoked;
        private Dictionary<string, string> _clonedParameters;
        
        private void OnValidate() 
        {
            if (TryGetComponent(out SpriteRenderer target)) Target = target;
        }

        private void Awake() 
        {
            ResetExecutor();
        }

        private void ResetExecutor()
        {
            _clonedParameters = MotionTree.Parameters.ToDictionary(entry => entry.Key, entry => entry.Value);

            _invoked = new();
            _nodesQueue = new();
            _motionsQueue = new();
        }

        private void Start() 
        {
            if (!MotionTree) throw new Exception("Target motion tree is null!");

            if (MotionTree.SavedData)
            {
                _saveDataExisting = true;
            }
        }

        private void Update() 
        {
            if (!_saveDataExisting) throw new Exception("There is nothing to execute in " + MotionTree.name);

            if (_terminated) return;

            ManageQueue();
            SetMotion(_motionsQueue.Dequeue());

            _updateFrametime -= Time.deltaTime;

            if (_updateFrametime <= 0)
            {
                _updateFrametime = 1 / Convert.ToSingle(MotionFramerate);
                
                OnMotionFrame?.Invoke();
                OnFrameUpdate();
            }
        }

        private void ManageQueue()
        {
            if (_terminated) return;

            if (_nodesQueue.Count == 0) _nodesQueue.Enqueue(MotionTree.SavedData.RootNext);

            while (_nodesQueue.Count > 0)
            {   
                NodeScriptableObject current = _nodesQueue.Dequeue();

                if (current == null)
                {
                    _terminated = true;
                    return;
                }

                if (current is MotionNodeScriptableObject motionNode)
                {
                    _motionsQueue.Enqueue(motionNode.TargetMotion);
                }
                else if (current is BranchNodeScriptableObject branchNode)
                {
                    if (Conditioner.StringToCondition(branchNode.Condition, MotionTree.ZipParameters(_clonedParameters)))
                        _nodesQueue.Enqueue(branchNode.True);
                    else
                        _nodesQueue.Enqueue(branchNode.False);
                }
            }
        }

        private void OnFrameUpdate()
        {
            if (HasMotion)
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
                    OnMotionStart?.Invoke();

                if (Target.sprite == framesContainer.Last().Sprites[dirIdx])
                {
                    OnMotionEnd?.Invoke();

                    if (_playThis.Looping) MotionFrame = 0;

                    List<string> keys = new(_invoked.Keys);
                    foreach(string key in keys)
                    {
                        SetParameter(key, _invoked[key]);
                        _invoked.Remove(key);
                    }
                }
                else MotionFrame++;
            }
        }

        public void SetParameter(string key, object value)
        {
            if (!_clonedParameters.ContainsKey(key)) throw new Exception($"Parameter {key} does not exist in {MotionTree.name}! Make sure you have added it to registered parameters, or have not misspeled it's name");
            if (_clonedParameters[key] != value.ToString()) 
                _clonedParameters[key] = value.ToString();
        }

        public void InvokeParameter(string key, object first, object after)
        {
            if (_invoked.ContainsKey(key)) return;

            SetParameter(key, first);
            _invoked.Add(key, after);
        }

        public void SetMotion(Motion target)
        {
            if (_playThis == target || !target) return;

            _playThis = target;

            MotionFrame = 0;
            MotionFramerate = target.UseCustomFramerate ? target.Framerate : Framerate;

            VariantIndex = 0;
        }

        public void SetVariant(int index) => VariantIndex = index;
        public void SetDirection(Vector2Int direction) => Direction = direction;
    }
}
