using System;
using System.Collections.Generic;
using InMotion.Enumerations;
using InMotion.SO;
using UnityEngine;

namespace InMotion.Data.Save
{
    public class MotionTreeSaveData : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<MotionTreeGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<MotionNodeSaveData> MotionNodes { get; set; }
        [field: SerializeField] public List<BranchNodeSaveData> BranchNodes { get; set; }
        [field: SerializeField] public List<MotionTreeNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<MotionTreeNodeSaveData> AllNodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public InMotionDictionary<string, List<string>> OldGroupedNodeNames { get; set; }
        [field: SerializeField] public Vector2 RootPosition { get; set; }
        [field: SerializeField] public string RootNextID { get; set; }
        [field: SerializeField] public NodeScriptableObject RootNext { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<MotionTreeGroupSaveData>();

            Nodes = new List<MotionTreeNodeSaveData>();
            MotionNodes = new List<MotionNodeSaveData>();
            BranchNodes = new List<BranchNodeSaveData>();
            AllNodes = new List<MotionTreeNodeSaveData>();
        }
    }

    [Serializable]
    public class MotionTreeNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public InMotionType Type { get; set; } 
        [field: SerializeField] public Vector2 Position { get; set; }
    }

    [Serializable]
    public class MotionNodeSaveData : MotionTreeNodeSaveData
    {
        [field: SerializeField] public Engine.Motion Motion { get; set; } 
        [field: SerializeField] public bool Transition { get; set; } 
        [field: SerializeField] public string NextNodeID { get; set; } 
    }

    [Serializable]
    public class BranchNodeSaveData : MotionTreeNodeSaveData
    {
        [field: SerializeField] public string Condition { get; set; } 
        [field: SerializeField] public string TrueNodeID { get; set; } 
        [field: SerializeField] public string FalseNodeID { get; set; } 
    }

    [Serializable]
    public class MotionTreeGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}
