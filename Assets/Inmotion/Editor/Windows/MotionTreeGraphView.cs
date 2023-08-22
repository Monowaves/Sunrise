using System;
using System.Collections.Generic;
using InMotion.Enumerations;
using InMotion.EditorOnly.GraphElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using InMotion.EditorOnly.Data.Error;
using System.Linq;

namespace InMotion.EditorOnly.Windows
{
    public class MotionTreeGraphView : GraphView
    {
        public RootNode RootNode;
        public string CurrentAssetPath;

        private MotionTreeSearchWindow _searchWindow;
        public MotionTreeEditorWindow MotionTreeWindow;

        private SerializableDictionary<string, NodeErrorData> _ungroupedNodes;
        private SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>> _groupedNodes;
        private SerializableDictionary<string, GroupErrorData> _groups;

        private int _repeatedNamesAmount;

        public int RepeatedNamesAmount
        {
            get
            {
                return _repeatedNamesAmount;
            }
            set
            {
                _repeatedNamesAmount = value;

                if (_repeatedNamesAmount > 0)
                {
                    MotionTreeWindow.SwitchSaving(false);
                }
                else
                {
                    MotionTreeWindow.SwitchSaving(true);
                }
            }
        }

        public MotionTreeGraphView(MotionTreeEditorWindow window, string current)
        {
            MotionTreeWindow = window;
            CurrentAssetPath = current;

            _ungroupedNodes = new SerializableDictionary<string, NodeErrorData>();
            _groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>>();
            _groups = new SerializableDictionary<string, GroupErrorData>();

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddStyles();

            OnElementsDeleted(); //mmm
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
        }

        public void InsertRoot(Vector2 position, Node connectTo = null)
        {
            RootNode = new RootNode();

            RootNode.Initialize(this, position);
            RootNode.Draw();

            if (connectTo != null)
            {
                Edge edge = ((Port)RootNode.outputContainer.Children().First()).ConnectTo((Port)connectTo.inputContainer.Children().First());
                AddElement(edge);
            }

            RootNode.RefreshPorts();

            AddElement(RootNode);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<MotionTreeSearchWindow>();

                _searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node/Motion Node", InMotionType.Motion));
            this.AddManipulator(CreateNodeContextualMenu("Add Node/Branch Node", InMotionType.Branch));
            this.AddManipulator(CreateNodeContextualMenu("Add Node/Return Node", InMotionType.Return));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction($"Add Group", actionEvent => CreateGroup("Sample Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateNodeContextualMenu(string nodePath, InMotionType nodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(nodePath, actionEvent => AddElement(CreateNode("Sample Node", nodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        public MotionTreeGroup CreateGroup(string title, Vector2 position)
        {
            MotionTreeGroup group = new MotionTreeGroup(title, position);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selected in selection)
            {
                if (!(selected is MotionTreeNode))
                {
                    continue;
                }

                MotionTreeNode node = (MotionTreeNode)selected;

                group.AddElement(node);
            }

            return group;
        }

        public MotionTreeNode CreateNode(string nodeName, InMotionType type, Vector2 position, bool draw = true)
        {
            MotionTreeNode node = new MotionTreeNode();

            switch (type)
            {
                case InMotionType.Motion:
                    node = new MotionNode();
                    break;
                
                case InMotionType.Branch:
                    node = new BranchNode();
                    break;
                
                case InMotionType.Return:
                    node = new ReturnNode();
                    break;
            }

            node.Initialize(nodeName, this, position);
            if (draw)
            {
                node.Draw();
            }

            AddUngroupedNode(node);

            return node;
        }

        public void AddGroup(MotionTreeGroup group)
        {
            string groupName = group.title.ToLower();

            if (!_groups.ContainsKey(groupName))
            {
                GroupErrorData groupErrorData = new GroupErrorData();

                groupErrorData.Groups.Add(group);  

                _groups.Add(groupName, groupErrorData);

                return;
            }

            List<MotionTreeGroup> mottGroups = _groups[groupName].Groups;

            mottGroups.Add(group);

            Color errorColor = _groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (mottGroups.Count == 2)
            {
                mottGroups[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroup(MotionTreeGroup group)
        {
            string groupName = group.OldTitle.ToLower();

            List<MotionTreeGroup> groupsList = _groups[groupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                _groups.Remove(groupName);
            }
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                MotionTreeGroup mottGroup = (MotionTreeGroup)group;

                mottGroup.title = newTitle.RemoveSpecialCharacters();

                RemoveGroup(mottGroup);

                mottGroup.OldTitle = mottGroup.title;

                AddGroup(mottGroup);
            };
        }

        public void AddUngroupedNode(MotionTreeNode node)
        {
            string nodeName = node.NodeName.ToLower();

            if (!_ungroupedNodes.ContainsKey(nodeName))
            {
                NodeErrorData nodeErrorData = new NodeErrorData();

                nodeErrorData.Nodes.Add(node);  

                _ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<MotionTreeNode> mottNodes = _ungroupedNodes[nodeName].Nodes;

            mottNodes.Add(node);

            Color errorColor = _ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (mottNodes.Count == 2)
            {
                ++RepeatedNamesAmount;

                mottNodes[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(MotionTreeNode node)
        {
            string nodeName = node.NodeName.ToLower();

            List<MotionTreeNode> mottNodes = _ungroupedNodes[nodeName].Nodes;

            mottNodes.Remove(node);

            node.ResetStyle();

            if (mottNodes.Count == 1)
            {
                RepeatedNamesAmount--;
                
                mottNodes[0].ResetStyle();

                return;
            }

            if (mottNodes.Count == 0)
            {
                _ungroupedNodes.Remove(nodeName);
            }
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, asker) =>
            {
                var count = selection.Count;

                for (var i = count -1; i >= 0; i--)
                {
                    ISelectable element = selection[i];

                    if (element is MotionTreeNode node)
                    {
                        if (node.Group != null)
                        {
                            node.Group.RemoveElement(node);
                        }

                        node.DisconnectAllPorts();
                        
                        RemoveUngroupedNode(node);
                        RemoveElement(node);
                    }
                    else if (element is ReturnNode node1)
                    {
                        node1.DisconnectAllPorts();
                        RemoveElement(node1);
                    }
                    else if (element is MotionTreeGroup group)
                    {
                        List<MotionTreeNode> groupNodes = new List<MotionTreeNode>();

                        foreach (GraphElement item in group.containedElements)
                        {
                            if (!(item is MotionTreeNode))
                            {
                                continue;
                            }

                            groupNodes.Add((MotionTreeNode)item);
                        }

                        group.RemoveElements(groupNodes);

                        RemoveGroup(group);
                        RemoveElement(group);
                    }
                    else if (element is Edge)
                    {
                        List<Edge> toDel = new List<Edge>() { (Edge)element };

                        DeleteElements(toDel);
                    }
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is MotionTreeNode))
                    {
                        continue;
                    }

                    MotionTreeNode node = (MotionTreeNode)element;

                    MotionTreeGroup tarGroup = (MotionTreeGroup)group;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, tarGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is MotionTreeNode))
                    {
                        continue;
                    }

                    MotionTreeNode node = (MotionTreeNode)element;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }

        public void RemoveGroupedNode(MotionTreeNode node, Group group)
        {
            string nodeName = node.NodeName;

            node.Group = null;

            List<MotionTreeNode> groupedNodesList = _groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                RepeatedNamesAmount--;

                groupedNodesList[0].ResetStyle();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                _groupedNodes[group].Remove(nodeName);

                if (_groupedNodes[group].Count == 0)
                {
                    _groupedNodes.Remove(group);
                }
            }
        }

        public void AddGroupedNode(MotionTreeNode node, MotionTreeGroup group)
        {
            string nodeName = node.NodeName;

            node.Group = group;

            if (!_groupedNodes.ContainsKey(group))
            {
                _groupedNodes.Add(group, new SerializableDictionary<string, NodeErrorData>());
            }

            SerializableDictionary<string, NodeErrorData> currGroupedNodes = _groupedNodes[group];

            if (!currGroupedNodes.ContainsKey(nodeName))
            {
                NodeErrorData nodeErrorData = new NodeErrorData();

                nodeErrorData.Nodes.Add(node);

                currGroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<MotionTreeNode> groupedNodesList = currGroupedNodes[nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = currGroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                RepeatedNamesAmount++;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        private void AddGridBackground()
        {
            GridBackground gridBG = new GridBackground();

            gridBG.StretchToParentSize();

            Insert(0, gridBG);
        }

        private void AddStyles()
        {
            StyleSheet mainStyleSheet = (StyleSheet)EditorGUIUtility.Load("MotionTree/MotionTreeGraphViewStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load("MotionTree/MotionTreeNodeStyles.uss");

            styleSheets.Add(mainStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port => 
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMouse = mousePosition;

            if (isSearchWindow)
            {
                worldMouse -= MotionTreeWindow.position.position;
            }

            Vector2 localMouse = contentViewContainer.WorldToLocal(worldMouse);

            return localMouse;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement =>
                RemoveElement(graphElement)
            );

            _groups.Clear();
            _ungroupedNodes.Clear();
            _groupedNodes.Clear();

            RepeatedNamesAmount = 0;
        }

        internal void RemoveGroupedNode(MotionTreeNode motionTreeNode, MotionTreeGroup group)
        {
            throw new NotImplementedException();
        }
    }
}
