#region copyright
// BuildR 2.0
// Available on the Unity Asset Store https://www.assetstore.unity3d.com/#!/publisher/412
// Copyright (c) 2017 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
#endregion



using System.Collections.Generic;
using UnityEngine;

namespace BuildR2.ShapeOffset
{
    public class SkeletonData
    {
        public Vector2[] baseShape = new Vector2[0];
        private List<Node> nodes = new List<Node>();
        public List<Edge> edges = new List<Edge>();
        public List<Node> liveNodes = new List<Node>();
        public List<Edge> liveEdges = new List<Edge>();//edges that sweep into the polygon
        public Dictionary<Node, Edge> formingEdges = new Dictionary<Node, Edge>();//a linked by node of the generated edges
        public float shrinkLength;
        public float maxOffset = 0;
        private List<Node> terminatedNodes = new List<Node>();//used for mansard
        public float unitHeight = 0;
        private int _liveNodeIndex = 1;
        private int _staticNodeIndex = 101;
        public SkeletonMesh mesh = new SkeletonMesh();

        public Node LiveNode(int index)
        {
            return liveNodes[index];
        }

        public Node StaticNode(int index)
        {
            return nodes[index];
        }

        public Node TerminatedNode(int index)
        {
            return terminatedNodes[index];
        }

        public int liveNodeCount { get { return liveNodes.Count; } }
        public int staticNodeCount { get { return nodes.Count; } }
        public int terminatedNodeCount { get { return terminatedNodes.Count; } }

        public int LiveIndex(Node node)
        {
            return liveNodes.IndexOf(node);
        }
        public int StaticIndex(Node node)
        {
            return nodes.IndexOf(node);
        }

        public bool isStaticNode(Node node)
        {
            return nodes.Contains(node);
        }

        public void AddLiveNode(Node newNode)
        {
            OffsetShapeLog.AddLine("New live node " + _liveNodeIndex);
            liveNodes.Add(newNode);
            newNode.id = _liveNodeIndex;
            _liveNodeIndex++;
        }

        public void AddStaticNode(Node newNode)
        {
            nodes.Add(newNode);
            newNode.id = _staticNodeIndex;
            _staticNodeIndex++;
        }

        public void RetireNode(Node liveNode)
        {
            liveNodes.Remove(liveNode);
            AddLiveNode(liveNode);
            if (liveNode.earlyTemination) terminatedNodes.Add(liveNode);
        }

        public SkeletonData(Vector2[] points)
        {
            Clear();
            baseShape = (Vector2[])points.Clone();
        }

        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            liveNodes.Clear();
            liveEdges.Clear();
            formingEdges.Clear();
            _liveNodeIndex = 1;
            _staticNodeIndex = 101;
            mesh.Clear();
        }

        public void DebugDraw()
        {
            mesh.DrawDebug();
        }
    }
}