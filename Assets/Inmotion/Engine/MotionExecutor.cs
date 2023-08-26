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
        private Motion _playThis;
        private bool _terminated;
        private Queue<NodeScriptableObject> _nodesQueue = new();
        private Queue<Motion> _motionsQueue = new();

        private bool HasMotion => _playThis;

        //CALLBACKS
        public Action OnMotionEnd;
        public Action OnMotionStart;
        public Action OnMotionFrame;

        
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
                    if (Conditioner.StringToCondition(branchNode.Condition, MotionTree.RegisteredParameters.ToArray()))
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
                }
                else MotionFrame++;
            }
        }

        public void SetParameter(string key, object value)
        {
            if (MotionTree.Parameters[key] != value.ToString()) 
                MotionTree.Parameters[key] = value.ToString();
        }

        public void SetMotion(Motion target)
        {
            if (_playThis == target || !target) return;

            _playThis = target;

            MotionFrame = 0;

            MotionFramerate = target.UseCustomFramerate ? target.Framerate : Framerate;
        }
    }
}
