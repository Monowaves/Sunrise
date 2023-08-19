using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InMotion.Utilities;

namespace InMotion.Tools.RuntimeScripts
{
    public class MotionExecutor : MonoBehaviour
    {
        [field: Header("Base Settings")]
        [field: SerializeField] public SpriteRenderer Target { get; private set;}
        [field: SerializeField] public MotionTree TargetMotionTree { get; private set; }
        [field: SerializeField, Min(1)] public int GlobalFramerate { get; private set; } = 10;

        [field: Header("Info")]
        [field: SerializeField, ReadOnly] public int GlobalFrameCounter { get; private set; }
        [field: SerializeField, ReadOnly] public int MotionFramerate { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Direction { get; private set; }
        [field: SerializeField, ReadOnly] public int VariantIndex { get; private set; }

        private float _updateFrametime;

        private bool _saveDataExisting;

        private bool _proccesing;
        private NodeScriptableObject _currentNode;
        private Motion _playThis;

        private bool _breaked;

        public Action OnAnimationEnd;
        public Action OnAnimationStart;
        public Action OnAnimationFrame;
        
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

            if (_breaked) { return; }

            _updateFrametime -= Time.deltaTime;

            if (_updateFrametime <= 0)
            {
                GlobalFrameCounter++;
                _updateFrametime = 1 / Convert.ToSingle(MotionFramerate);
                
                OnFrameUpdate();
            }
        }

        public void SetMotion(Motion target)
        {
            _playThis = target;

            MotionFramerate = target.UseCustomFramerate ? target.Framerate : GlobalFramerate;
        }

        public void SetDirection(Vector2Int direction) => Direction = direction;
        public void SetVariant(int idx) => VariantIndex = idx;

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

        private void ProccesStop() => _proccesing = false;

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
            else if (_currentNode is BranchNodeScriptableObject typedNode)
            {
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
            OnAnimationFrame?.Invoke();

            if (_playThis != null)
            {
                List<DirectionalSprite> framesContainer = _playThis.Variants[VariantIndex].FramesContainer;
                int dirIdx = DirectionUtility.DefineDirectionIndex(Direction);

                Target.sprite = framesContainer[GlobalFrameCounter % framesContainer.Count].Sprites[dirIdx];

                if (Target.sprite == framesContainer.Last().Sprites[dirIdx])
                {
                    OnAnimationEnd?.Invoke();

                    ProccesStart();
                }

                if (Target.sprite == framesContainer.First().Sprites[dirIdx])
                {
                    OnAnimationStart?.Invoke();

                    ProccesStart();
                }
            }
        }
    }
}

public class ReadOnlyAttribute : PropertyAttribute {}
