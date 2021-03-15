using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace XNode {
    /// <summary> Base class for all node graphs </summary>
    [Serializable]
    public abstract class NodeGraph : ScriptableObject {

        /// <summary> All nodes in the graph. <para/>
        /// See: <see cref="AddNode{T}"/> </summary>
        [SerializeField] public List<Node> nodes = new List<Node>();

        [Serializable]
        public struct NodeToVarPos
        {
            [SerializeField]
            public Node node;
            [SerializeField]
            public Vector2 position;
        }
        // Use list instead of dictionnary as unity doesn't display and serialize dictionnaries
        [SerializeField] public List<NodeToVarPos> nodePositions = new List<NodeToVarPos>();

        public Vector2 GetNodePosition(Node node)
        {
            for (int i = 0; i < nodePositions.Count; ++i)
            {
                var val = nodePositions[i];
                if (val.node == node)
                {
                    return val.position;
                }
            }

            return Vector2.zero;
        }

        public void SetNodePosition(Node node, Vector2 position)
        {
            int foundIndex = -1;
            for(int i = 0; i < nodePositions.Count; ++i)
            {
                if (node == nodePositions[i].node)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1)
            {
                nodePositions[foundIndex] = new NodeToVarPos() { node = node, position = position };
            }
            else
            {
                nodePositions.Add(new NodeToVarPos() { node = node, position = position });
            }
        }

        /// <summary> Add a node to the graph by type (convenience method - will call the System.Type version) </summary>
        public T AddNode<T>() where T : Node {
            return AddNode(typeof(T)) as T;
        }

        /// <summary> Add an existing node to the graph </summary>
        public void AddExistingNode(Node node)
        {
            if (!nodes.Contains(node))
            {
                Node.graphHotfix = this;
                nodes.Add(node);
            }
        }

        /// <summary> Add a node to the graph by type </summary>
        public virtual Node AddNode(Type type) {
            Node.graphHotfix = this;
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.graph = this;
            nodes.Add(node);
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual Node CopyNode(Node original) {
            Node.graphHotfix = this;
            Node node = ScriptableObject.Instantiate(original);
            node.graph = this;
            node.ClearConnections();
            nodes.Add(node);
            return node;
        }

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"> The node to remove </param>
        public virtual void RemoveNode(Node node) {
            node.ClearConnections();
            nodes.Remove(node);
            nodePositions.RemoveAll(x => x.node == node);
            if (Application.isPlaying) Destroy(node);
        }

        public bool IsRefNode(Node node)
        {
            return this != node.graph;
        }

        /// <summary> Remove all nodes and connections from the graph </summary>
        public virtual void Clear() {
            if (Application.isPlaying) {
                for (int i = 0; i < nodes.Count; i++) {
                    Destroy(nodes[i]);
                }
            }
            nodes.Clear();
            nodePositions.Clear();
        }

        /// <summary> Create a new deep copy of this graph </summary>
        public virtual XNode.NodeGraph Copy() {
            return null;
        }

        protected virtual void OnDestroy() {
            // Remove all nodes prior to graph destruction
            Clear();
        }

#region Attributes
        /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted. </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class RequireNodeAttribute : Attribute {
            public Type type0;
            public Type type1;
            public Type type2;

            /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
            public RequireNodeAttribute(Type type) {
                this.type0 = type;
                this.type1 = null;
                this.type2 = null;
            }

            /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
            public RequireNodeAttribute(Type type, Type type2) {
                this.type0 = type;
                this.type1 = type2;
                this.type2 = null;
            }

            /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
            public RequireNodeAttribute(Type type, Type type2, Type type3) {
                this.type0 = type;
                this.type1 = type2;
                this.type2 = type3;
            }

            public bool Requires(Type type) {
                if (type == null) return false;
                if (type == type0) return true;
                else if (type == type1) return true;
                else if (type == type2) return true;
                return false;
            }
        }
#endregion
    }
}