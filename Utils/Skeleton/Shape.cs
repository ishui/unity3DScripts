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



using UnityEngine;
using System.Collections.Generic;

namespace BuildR2.ShapeOffset
{
    public class Shape
    {
        public Vector2[] baseShape = new Vector2[0];
        private List<Node> nodes = new List<Node>();
        public List<Edge> edges = new List<Edge>();
        public List<Edge> baseEdges = new List<Edge>();
        public List<Node> liveNodes = new List<Node>();
        public List<Edge> liveEdges = new List<Edge>();//edges that sweep into the polygon
        public Dictionary<Node, Edge> formingEdges = new Dictionary<Node, Edge>();//a linked by node of the generated edges
        public float shrinkLength;

        private List<Node> terminatedNodes = new List<Node>();//used for mansard
        private int _liveNodeIndex = 1;
        private int _staticNodeIndex = 101;

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

        public void InsertLiveNode(int index, Node newNode)
        {
            OffsetShapeLog.AddLine("New live node " + _liveNodeIndex);
            liveNodes.Insert(index, newNode);
            newNode.id = _liveNodeIndex;
            _liveNodeIndex++;
        }

        public Node AddStaticNode(Node newNode)
        {
            int staticNodeCount = nodes.Count;
            for (int s = 0; s < staticNodeCount; s++)
            {
                if ((nodes[s].position - newNode.position).sqrMagnitude < (Mathf.Epsilon * Mathf.Epsilon))
                    return nodes[s];
            }
            OffsetShapeLog.AddLine("New static node " + _staticNodeIndex);
            nodes.Add(newNode);
            newNode.id = _staticNodeIndex;
            _staticNodeIndex++;
            return newNode;
        }

        public void RetireNode(Node liveNode)
        {
            liveNodes.Remove(liveNode);
            AddLiveNode(liveNode);
            if (liveNode.earlyTemination) terminatedNodes.Add(liveNode);
        }

        public void TerminateNode(Node liveNode)
        {
            liveNode.earlyTemination = true;
            terminatedNodes.Add(liveNode);
            edges.Add(formingEdges[liveNode]);
        }

        public void TerminateAllNodes()
        {
            for (int i = 0; i < liveNodeCount; i++)
                TerminateNode(liveNodes[i]);
            edges.AddRange(liveEdges);
        }

        public Shape(Vector2[] points)
        {
            Clear();
            baseShape = (Vector2[])points.Clone();
        }

        public Vector2[] GetLiveShape()
        {
            Vector2[] output = new Vector2[liveNodeCount];
            for (int l = 0; l < liveNodeCount; l++)
                output[l] = liveNodes[l].position;
            return output;
        }

        public float HeighestPoint()
        {
            float output = 0;
            int edgeCount = edges.Count;
            for(int e = 0; e < edgeCount; e++)
            {
                if (edges[e].nodeA.height > output) output = edges[e].nodeA.height;
                if (edges[e].nodeB.height > output) output = edges[e].nodeB.height;
            }
            //                for (int i = 0; i < liveNodeCount; i++)
            //                if (liveNodes[i].height > output) output = liveNodes[i].height;
            //            for (int i = 0; i < staticNodeCount; i++)
            //                if (nodes[i].height > output) output = nodes[i].height;
            //            for (int i = 0; i < terminatedNodeCount; i++)
            //                if (terminatedNodes[i].height > output) output = terminatedNodes[i].height;
            return output;
        }

        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            liveNodes.Clear();
            liveEdges.Clear();
            formingEdges.Clear();
            //            _liveNodeIndex = 1;
            //            _staticNodeIndex = 101;
            //            mesh.Clear();
        }
    }
}