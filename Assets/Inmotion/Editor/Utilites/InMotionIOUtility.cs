using System;
using System.Collections.Generic;
using System.Linq;
using InMotion.Data.Save;
using InMotion.EditorOnly.GraphElements;
using InMotion.EditorOnly.Windows;
using InMotion.Engine;
using InMotion.Enumerations;
using InMotion.SO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InMotion.EditorOnly.Utilities
{
    public static class InMotionIOUtility
    {
        private static MotionTreeGraphView _graphView;
        private static string _graphFileName;
        private static string _containerFolderPath;

        private static List<MotionTreeGroup> _groups;
        private static List<MotionTreeNode> _nodes;

        private static Dictionary<string, GroupScriptableObject> _createdGroups;
        private static Dictionary<string, NodeScriptableObject> _createdNodes;

        private static Dictionary<string, MotionTreeGroup> _loadedGroups;
        private static Dictionary<string, MotionTreeNode> _loadedNodes;

        public static void Initialize(MotionTreeGraphView graphView, string graphName)
        {
            _graphView = graphView;
            _graphFileName = graphName;

            _containerFolderPath = $"Assets/InMotion/SavedDataContainer/Runtime/MotionTrees/{_graphFileName}";

            _groups = new List<MotionTreeGroup>();
            _nodes = new List<MotionTreeNode>();

            _createdGroups = new Dictionary<string, GroupScriptableObject>();
            _createdNodes = new Dictionary<string, NodeScriptableObject>();
            
            _loadedGroups = new Dictionary<string, MotionTreeGroup>();
            _loadedNodes = new Dictionary<string, MotionTreeNode>();
        }

        public static void Save(string motionTreeFilePath)
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            MotionTreeSaveData graphData = CreateAsset<MotionTreeSaveData>("Assets/InMotion/SavedDataContainer/Editor/Graphs", $"{_graphFileName}Graph");
            graphData.Initialize(_graphFileName);

            MotionTree motionTree = AssetDatabase.LoadAssetAtPath<MotionTree>(motionTreeFilePath);
            motionTree.SavedData = graphData;

            EditorUtility.SetDirty(motionTree);

            NodeContainerScriptableObject nodeContainerScriptableObject = CreateAsset<NodeContainerScriptableObject>(_containerFolderPath, _graphFileName);
            nodeContainerScriptableObject.Initialize(_graphFileName);

            SaveGroups(graphData, nodeContainerScriptableObject);
            SaveNodes(graphData, nodeContainerScriptableObject);
            SaveAsset(graphData);
            SaveAsset(nodeContainerScriptableObject);
            SaveConnectionsToSO(graphData);
            SaveRoot(graphData);

            Debug.Log($"Motion Tree {_graphFileName} saved successfully!");
        }

        public static void Load()
        {
            MotionTreeSaveData graphData = LoadAsset<MotionTreeSaveData>("Assets/InMotion/SavedDataContainer/Editor/Graphs", _graphFileName);

            if (graphData == null)
            {
                _graphView.InsertRoot(Vector2.zero);
                return; //kikikaki
            }

            LoadGroups(graphData.Groups);
            LoadMotionNodes(graphData.MotionNodes);
            LoadBranchNodes(graphData.BranchNodes);
            LoadNodes(graphData.Nodes);
            LoadConnections(graphData);
            LoadRoot(graphData);
        }

        public static void DestroySave(MotionTree item)
        {
            RemoveAsset("Assets/InMotion/SavedDataContainer/Editor/Graphs", item.name + "Graph");
            RemoveFolder($"Assets/InMotion/SavedDataContainer/Runtime/MotionTrees/{item.name}");

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
        }

        private static void LoadConnections(MotionTreeSaveData graphData)
        {
            foreach (KeyValuePair<string, MotionTreeNode> loadedNode in _loadedNodes)
            {
                if (loadedNode.Value.NodeType == InMotionType.Motion)
                {
                    MotionNode typedNode = (MotionNode)loadedNode.Value;

                    foreach (MotionNodeSaveData nodeData in graphData.MotionNodes)
                    {
                        if (nodeData.ID == typedNode.ID)
                        {
                            if (nodeData.NextNodeID != null && nodeData.NextNodeID != String.Empty && _loadedNodes.ContainsKey(nodeData.NextNodeID))
                            {
                                Edge edge = typedNode.NextPort.ConnectTo((Port)_loadedNodes[nodeData.NextNodeID].inputContainer.Children().First());
                                _graphView.AddElement(edge);
                            }
                        }
                    }
                }
                else if (loadedNode.Value.NodeType == InMotionType.Branch)
                {
                    BranchNode typedNode = (BranchNode)loadedNode.Value;

                    foreach (BranchNodeSaveData nodeData in graphData.BranchNodes)
                    {
                        if (nodeData.ID == typedNode.ID)
                        {
                            if (nodeData.TrueNodeID != null && nodeData.TrueNodeID != String.Empty && _loadedNodes.ContainsKey(nodeData.TrueNodeID))
                            {
                                Edge edgeT = typedNode.OutputTrue.ConnectTo((Port)_loadedNodes[nodeData.TrueNodeID].inputContainer.Children().First());
                                _graphView.AddElement(edgeT);
                            }

                            if (nodeData.FalseNodeID != null && nodeData.FalseNodeID != String.Empty && _loadedNodes.ContainsKey(nodeData.FalseNodeID))
                            {
                                Edge edgeF = typedNode.OutputFalse.ConnectTo((Port)_loadedNodes[nodeData.FalseNodeID].inputContainer.Children().First());
                                _graphView.AddElement(edgeF);
                            }
                        }
                    }
                }

                loadedNode.Value.RefreshPorts();
            }
        }

        private static void LoadRoot(MotionTreeSaveData graphData)
        {
            if (graphData.RootNextID != string.Empty)
            {
                _graphView.InsertRoot(graphData.RootPosition, _loadedNodes[graphData.RootNextID]);

                return;
            }

            _graphView.InsertRoot(graphData.RootPosition);
        }

        private static void LoadNode(MotionTreeNode node, MotionTreeNodeSaveData nodeData)
        {
            node.Draw();

            _graphView.AddElement(node);
            _loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.GroupID))
            {
                return;
            }

            MotionTreeGroup group = _loadedGroups[nodeData.GroupID];

            node.Group = group;

            group.AddElement(node);
        }

        private static void LoadMotionNodes(List<MotionNodeSaveData> nodes)
        {
            foreach (MotionNodeSaveData nodeData in nodes)
            {
                MotionTreeNode node = _graphView.CreateNode(nodeData.Name, nodeData.Type, nodeData.Position, false);

                node.ID = nodeData.ID;

                MotionNode typedNode = (MotionNode)node;
                typedNode.TargetMotion = nodeData.Motion;
                typedNode.Transition = nodeData.Transition;

                LoadNode(node, nodeData);
            }
        }

        private static void LoadBranchNodes(List<BranchNodeSaveData> nodes)
        {
            foreach (BranchNodeSaveData nodeData in nodes)
            {
                MotionTreeNode node = _graphView.CreateNode(nodeData.Name, nodeData.Type, nodeData.Position, false);

                node.ID = nodeData.ID;

                BranchNode typedNode = (BranchNode)node;
                typedNode.Condition = nodeData.Condition;

                LoadNode(node, nodeData);
            }
        }

        private static void LoadNodes(List<MotionTreeNodeSaveData> nodes)
        {
            foreach (MotionTreeNodeSaveData nodeData in nodes)
            {
                MotionTreeNode node = _graphView.CreateNode(nodeData.Name, nodeData.Type, nodeData.Position, false);

                node.ID = nodeData.ID;

                LoadNode(node, nodeData);
            }
        }

        private static void LoadGroups(List<MotionTreeGroupSaveData> groups)
        {
            foreach (MotionTreeGroupSaveData groupData in groups)
            {
                MotionTreeGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                _loadedGroups.Add(group.ID, group);
            }
        }

        private static void SaveRoot(MotionTreeSaveData graphData)
        {
            graphData.RootPosition = _graphView.RootNode.GetPosition().position;
            
            if (((Port)_graphView.RootNode.outputContainer.Children().First()).connected)
            {
                graphData.RootNextID = ((MotionTreeNode)((Port)_graphView.RootNode.outputContainer.Children().First()).connections.First().input.node).ID;
                graphData.RootNext = _createdNodes[graphData.RootNextID];
            }

            EditorUtility.SetDirty(graphData);
            AssetDatabase.SaveAssets();
        }

        private static void SaveNodes(MotionTreeSaveData graphData, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            InMotionDictionary<string, List<string>> groupedNodesNames = new InMotionDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();
            
            foreach (MotionTreeNode node in _nodes)
            {
                SaveNodeToGraph(node, graphData);
                
                switch (node.NodeType)
                {
                    case InMotionType.Motion:
                        SaveMotionNodeToSO((MotionNode)node, nodeContainerScriptableObject);
                        break;

                    case InMotionType.Branch:
                        SaveBranchNodeToSO((BranchNode)node, nodeContainerScriptableObject);
                        break;

                    case InMotionType.Return:
                        SaveReturnNodeToSO((ReturnNode)node, nodeContainerScriptableObject);
                        break;
                }

                if (node.Group != null)
                {
                    groupedNodesNames.AddItem(node.Group.title, node.NodeName);

                    continue;
                }

                ungroupedNodeNames.Add(node.NodeName);
            }

            UpdateOldGroupedNodes(groupedNodesNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveConnectionsToSO(MotionTreeSaveData graphData)
        {
            foreach (KeyValuePair<string, NodeScriptableObject> savedNode in _createdNodes)
            {
                if (savedNode.Value.NodeType == InMotionType.Motion)
                {
                    MotionNodeScriptableObject typedNode = (MotionNodeScriptableObject)savedNode.Value;

                    foreach (MotionNodeSaveData nodeData in graphData.MotionNodes)
                    {
                        if (nodeData.ID == savedNode.Key)
                        {
                            if (nodeData.NextNodeID != null)
                            {
                                typedNode.Next = _createdNodes[nodeData.NextNodeID];
                                EditorUtility.SetDirty(typedNode);
                            }
                        }
                    }
                }
                else if (savedNode.Value.NodeType == InMotionType.Branch)
                {
                    BranchNodeScriptableObject typedNode = (BranchNodeScriptableObject)savedNode.Value;

                    foreach (BranchNodeSaveData nodeData in graphData.BranchNodes)
                    {
                        if (nodeData.ID == savedNode.Key)
                        {
                            if (nodeData.TrueNodeID != null)
                            {
                                typedNode.True = _createdNodes[nodeData.TrueNodeID];
                            }

                            if (nodeData.FalseNodeID != null)
                            {
                                typedNode.False = _createdNodes[nodeData.FalseNodeID];
                            }

                            EditorUtility.SetDirty(typedNode);
                        }
                    }
                }
            }
        }

        private static void UpdateOldGroupedNodes(InMotionDictionary<string, List<string>> currentGroupedNodesNames, MotionTreeSaveData graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> node in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodesNames.ContainsKey(node.Key))
                    {
                        nodesToRemove = node.Value.Except(currentGroupedNodesNames[node.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{_containerFolderPath}/Groups/{node.Key}/Nodes", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new InMotionDictionary<string, List<string>>(currentGroupedNodesNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, MotionTreeSaveData graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string node in nodesToRemove)
                {
                    RemoveAsset($"{_containerFolderPath}/Global/Nodes", node);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void RemoveAsset(string fullPath, string assetName)
        {
            AssetDatabase.DeleteAsset($"{fullPath}/{assetName}.asset");
        }

        private static void SaveNode(MotionTreeNode node, NodeScriptableObject nodeSO)
        {
            _createdNodes.Add(node.ID, nodeSO);
        }

        private static void SaveReturnNodeToSO(ReturnNode node, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            NodeScriptableObject nodeSO;

            if (node.Group != null)
            {
                nodeSO = CreateAsset<NodeScriptableObject>($"{_containerFolderPath}/Groups/{node.Group.title}/Nodes", node.NodeName);

                nodeContainerScriptableObject.MotionTreeGroups.AddItem(_createdGroups[node.Group.ID], nodeSO);
            }
            else
            {
                nodeSO = CreateAsset<NodeScriptableObject>($"{_containerFolderPath}/Global/Nodes", node.NodeName);

                nodeContainerScriptableObject.UngroupedNodes.Add(nodeSO);
            }

            nodeSO.Initialize(
                node.NodeName,
                node.NodeType
            );
            SaveAsset(nodeSO);
            SaveNode(node, nodeSO);
        }

        private static void SaveBranchNodeToSO(BranchNode node, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            BranchNodeScriptableObject nodeSO;

            if (node.Group != null)
            {
                nodeSO = CreateAsset<BranchNodeScriptableObject>($"{_containerFolderPath}/Groups/{node.Group.title}/Nodes", node.NodeName);

                nodeContainerScriptableObject.MotionTreeGroups.AddItem(_createdGroups[node.Group.ID], nodeSO);
            }
            else
            {
                nodeSO = CreateAsset<BranchNodeScriptableObject>($"{_containerFolderPath}/Global/Nodes", node.NodeName);

                nodeContainerScriptableObject.UngroupedNodes.Add(nodeSO);
            }
            
            nodeSO.Initialize(
                node.NodeName,
                node.NodeType
            );
            nodeSO.Condition = node.Condition;

            SaveAsset(nodeSO);
            SaveNode(node, nodeSO);
        }

        private static void SaveMotionNodeToSO(MotionNode node, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            MotionNodeScriptableObject nodeSO;

            if (node.Group != null)
            {
                nodeSO = CreateAsset<MotionNodeScriptableObject>($"{_containerFolderPath}/Groups/{node.Group.title}/Nodes", node.NodeName);

                nodeContainerScriptableObject.MotionTreeGroups.AddItem(_createdGroups[node.Group.ID], nodeSO);
            }
            else
            {
                nodeSO = CreateAsset<MotionNodeScriptableObject>($"{_containerFolderPath}/Global/Nodes", node.NodeName);

                nodeContainerScriptableObject.UngroupedNodes.Add(nodeSO);
            }
            
            nodeSO.Initialize(
                node.NodeName,
                node.NodeType
            );
            nodeSO.TargetMotion = node.TargetMotion;
            nodeSO.Transition = node.Transition;

            SaveAsset(nodeSO);
            SaveNode(node, nodeSO);
        }

        private static void SaveNodeToGraph(MotionTreeNode node, MotionTreeSaveData graphData)
        {
            MotionNodeSaveData defaultNodeData = new MotionNodeSaveData()
            {
                ID = node.ID,
                Name = node.NodeName,
                GroupID = node.Group?.ID,
                Type = node.NodeType,
                Position = node.GetPosition().position
            };

            graphData.AllNodes.Add(defaultNodeData);

            if (node is MotionNode)
            {
                MotionNode typedNode = (MotionNode)node;

                MotionNodeSaveData nodeData = new MotionNodeSaveData()
                {
                    ID = node.ID,
                    Name = node.NodeName,
                    GroupID = node.Group?.ID,
                    Type = node.NodeType,
                    Position = node.GetPosition().position,
                    Motion = typedNode.TargetMotion,
                    Transition = typedNode.Transition
                };

                if (typedNode.NextPort.connections.Count() > 0)
                {
                    MotionTreeNode connectedNode = (MotionTreeNode)typedNode.NextPort.connections.ElementAt(0).input.node;
                    nodeData.NextNodeID = connectedNode.ID;
                }
                
                graphData.MotionNodes.Add(nodeData);
            }
            else if (node is BranchNode)
            {
                BranchNode typedNode = (BranchNode)node;

                BranchNodeSaveData nodeData = new BranchNodeSaveData()
                {
                    ID = node.ID,
                    Name = node.NodeName,
                    GroupID = node.Group?.ID,
                    Type = node.NodeType,
                    Position = node.GetPosition().position,
                    Condition = typedNode.Condition
                };

                if (typedNode.OutputTrue.connections.Count() > 0)
                {
                    MotionTreeNode connectedNode = (MotionTreeNode)typedNode.OutputTrue.connections.ElementAt(0).input.node;
                    nodeData.TrueNodeID = connectedNode.ID;
                }
                if (typedNode.OutputFalse.connections.Count() > 0)
                {
                    MotionTreeNode connectedNode = (MotionTreeNode)typedNode.OutputFalse.connections.ElementAt(0).input.node;
                    nodeData.FalseNodeID = connectedNode.ID;
                }

                graphData.BranchNodes.Add(nodeData);
            }
            else
            {
                MotionTreeNodeSaveData nodeData = new MotionTreeNodeSaveData()
                {
                    ID = node.ID,
                    Name = node.NodeName,
                    GroupID = node.Group?.ID,
                    Type = node.NodeType,
                    Position = node.GetPosition().position,
                };

                graphData.Nodes.Add(nodeData);
            }
        }

        private static void SaveGroups(MotionTreeSaveData graphData, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            List<string> groupNames = new List<string>();

            foreach (MotionTreeGroup group in _groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToSO(group, nodeContainerScriptableObject);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, MotionTreeSaveData graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string group in groupsToRemove)
                {
                    RemoveFolder($"{_containerFolderPath}/Groups/{group}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        private static void SaveGroupToSO(MotionTreeGroup group, NodeContainerScriptableObject nodeContainerScriptableObject)
        {
            string groupName = group.title;

            CreateFolder($"{_containerFolderPath}/Groups", groupName);

            string groupsPath = $"{_containerFolderPath}/Groups/{groupName}";

            CreateFolder(groupsPath, "Nodes");

            GroupScriptableObject motionTreeGroup = CreateAsset<GroupScriptableObject>(groupsPath, groupName);
            motionTreeGroup.Initialize(groupName);

            _createdGroups.Add(group.ID, motionTreeGroup);
            nodeContainerScriptableObject.MotionTreeGroups.Add(motionTreeGroup, new List<NodeScriptableObject>());

            SaveAsset(motionTreeGroup);
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SaveGroupToGraph(MotionTreeGroup group, MotionTreeSaveData graphData)
        {
            MotionTreeGroupSaveData groupData = new MotionTreeGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            string[] seperated = path.Split("/");

            CreateFolder(path.Replace($"/{seperated[seperated.Length - 1]}", String.Empty), seperated[seperated.Length - 1]);

            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(MotionTreeGroup);

            _graphView.graphElements.ForEach(graphElement => 
            {
                if (graphElement is MotionTreeNode node)
                {
                    _nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    MotionTreeGroup group = (MotionTreeGroup)graphElement;
                    _groups.Add(group);

                    return;
                }
            });
        }

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/InMotion/SavedDataContainer", "Editor");
            CreateFolder("Assets/InMotion/SavedDataContainer", "Runtime");

            CreateFolder("Assets/InMotion/SavedDataContainer/Editor", "Graphs");
            CreateFolder("Assets/InMotion/SavedDataContainer/Runtime", "MotionTrees");

            CreateFolder("Assets/InMotion/SavedDataContainer/Runtime/MotionTrees", _graphFileName);
            CreateFolder(_containerFolderPath, "Global");
            CreateFolder(_containerFolderPath, "Groups");
            CreateFolder($"{_containerFolderPath}/Global", "MotionTrees");
        }

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }
    }
}
