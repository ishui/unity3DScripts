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
    public class OffsetPoly
    {
        private OffsetPolyCore _core;
        private bool _debug;
        private float _direction = 0;
        private int _maxIterations = 26;

        public bool complete { get { return _core.complete; } }
        public bool error { get { return _core.error; } }

        public OffsetPoly(Vector2[] poly, float distance = 0, bool debug = false)
        {
            _debug = debug;
            _direction = distance > 0 ? _direction = 1 : _direction = -1;
            _core = new OffsetPolyCore();
            _core.LiveDebug(debug);
            _core.maxOffset = distance;
            _core.calculateInteractions = false;
            _core.Init(poly);
            _core.OnFlipEvent += OnFlip;
            _core.OnSplitEvent += OnSplit;
            _core.OnMergedEvent += OnMerged;
            _core.OnCompleteEvent += OnComplete;
            _core.OnErrorEvent += OnError;
        }

        public Vector2[] Shape()
        {
            if(_core.shape == null) return new Vector2[0];
            return _core.shape.GetLiveShape();
        }

        private void OnError(string errormessage)
        {
            if (_debug) Debug.Log(errormessage);
        }

        private void OnComplete()
        {

        }

        private void OnFlip(FlipEvent fEvent)
        {
            CollapseEdge(fEvent.edge, fEvent.point, fEvent.height);
        }

        private void OnSplit(SplitEvent sEvent)
        {
            SplitEdge(sEvent);
        }

        private void OnMerged(MergedEvent mEvent)
        {
            MergeEvent(mEvent);
        }

        public void Execute()
        {
            int it = 0;
            while (!complete && !error && it < _maxIterations)
            {
                _core.OffsetPoly(_direction);
                it++;
            }
        }

        public void ExecuteStep()
        {
            _core.OffsetPoly(_direction);

            if (_core.error)
                Debug.LogWarning(_core.errorMessage);
        }

        public void Cleanup()
        {

        }


        public void CollapseEdge(Edge edge, Vector2 toPoint, float height)
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(edge.nodeA);
            nodes.Add(edge.nodeB);
            CollapseNodes(nodes, toPoint, height);
        }

        public void MergeEvent(MergedEvent evnt)
        {
            CollapseNodes(evnt.mergeNodes, evnt.point, evnt.height);
        }

        public void Draw()
        {
            OffsetShapeLog.enabled = true;
            OffsetShapeLog.DrawShapeNodes(_core.shape);
        }

        private void CollapseNodes(List<Node> nodes, Vector2 toPoint, float height)
        {
            Shape _shape = _core.shape;

            int nodeCount = nodes.Count;
            float minHeight = nodes[0].height;
            float maxHeight = nodes[0].height;
            for (int n = 1; n < nodeCount; n++)
            {
                minHeight = Mathf.Min(minHeight, nodes[n].height);
                maxHeight = Mathf.Min(maxHeight, nodes[n].height);
            }

            Node survivor = nodes[0];
            survivor.position = toPoint;
            survivor.height = maxHeight;

            int liveEdgeCount = _shape.liveEdges.Count;
            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = _shape.liveEdges[e];
                if (!nodes.Contains(edge.nodeA) || !nodes.Contains(edge.nodeB)) continue;
                edge.UpdateValues();
                if (nodes.Contains(edge.nodeA) && nodes.Contains(edge.nodeB))
                {
                    OffsetShapeLog.AddLine(edge.ToString(), "has collapsed", edge.length, _core.pointAccuracy);
                    OffsetShapeLog.AddLine("Remove edge ", edge.ToString());
                    _shape.liveEdges.Remove(edge);//remove collapsed edge - from length reaching zero
                    liveEdgeCount--;
                    e--;
                    continue;
                    //                    if (!nodes.Contains(edge.nodeA))
                    //                        nodes.Add(edge.nodeA);
                    //                    if (!nodes.Contains(edge.nodeB))
                    //                        nodes.Add(edge.nodeB);
                }
                //else affected edge
                if (nodes.Contains(edge.nodeA))
                    edge.ReplaceNode(edge.nodeA, survivor);
                if (nodes.Contains(edge.nodeB))
                    edge.ReplaceNode(edge.nodeB, survivor);
            }



//            OffsetShapeLog.AddLine("Collapse Nodes:");
//            foreach (Node node in nodes)
//                OffsetShapeLog.Add(node.id + " ");
//            OffsetShapeLog.AddLine("to point: ", toPoint);
//            //            Node newStaticNode = new Node(toPoint, height);//new static node to mark point of collapse
//            //            _shape.AddStaticNode(newStaticNode);//add static node to node array
//            //            OffsetShapeLog.AddLine("new static node added ", newStaticNode.id);
//
//            //            Node newLiveNode = new Node(toPoint, height);//new live node to continue shape forming
//
//            for (int n = 0; n < nodeCount; n++)
//            {
//                nodes[n].position = toPoint;
//                nodes[n].height = maxHeight;
//            }
//
//            Edge collapsedEdge = null;
//            for (int e = 0; e < liveEdgeCount; e++)
//            {
//                Edge edge = _shape.liveEdges[e];
//                if (!nodes.Contains(edge.nodeA) || !nodes.Contains(edge.nodeB)) continue;
//                edge.UpdateValues();
//                if (edge.length < _core.pointAccuracy)//when the edge reaches 0 length it has flipped and should be collapsed
//                {
//                    OffsetShapeLog.AddLine(edge.ToString(), "has collapsed", edge.length, _core.pointAccuracy);
//                    //                    _shape.mesh.CollapseEdge(edge, newLiveNode, newStaticNode);
//                    OffsetShapeLog.AddLine("Remove edge ", edge.ToString());
//                    collapsedEdge = edge;
//                    _shape.liveEdges.Remove(edge);//remove collapsed edge - from length reaching zero
//                    liveEdgeCount--;
//                    e--;
//                    if (!nodes.Contains(edge.nodeA))
//                        nodes.Add(edge.nodeA);
//                    if (!nodes.Contains(edge.nodeB))
//                        nodes.Add(edge.nodeB);
//                }
//            }

//            OffsetShapeLog.AddLine("find live node edges");
//            for (int e = 0; e < liveEdgeCount; e++)
//            {
//                Edge edge = _shape.liveEdges[e];
//                if (!edge.Contains(nodes)) continue;
//                if (nodes.Contains(edge.nodeA) && nodes.Contains(edge.nodeB))
//                {
//                    OffsetShapeLog.AddLine("Remove collapsed edge ", edge.ToString());
//                    _shape.liveEdges.Remove(edge);//remove collapsed edge - likely from parallel
//                    liveEdgeCount--;
//                    e--;
//                    continue;
//                }
//                if (nodes.Contains(edge.nodeA) || newLiveNode == edge.nodeA)
//                {
//                    OffsetShapeLog.AddLine("replace node a");
//                    edge.ReplaceNode(edge.nodeA, newLiveNode);//replace old live node reference to new one
//                    liveEdges.Add(edge);
//                    continue;
//                }
//                if (nodes.Contains(edge.nodeB) || newLiveNode == edge.nodeB)
//                {
//                    OffsetShapeLog.AddLine("replace node b");
//                    edge.ReplaceNode(edge.nodeB, newLiveNode);//replace old live node reference to new one
//                    liveEdges.Add(edge);
//                }
//            }

            for (int n = 0; n < nodeCount; n++)
            {
                Node node = nodes[n];
                if(node == survivor)continue;
                //                Utils.RetireFormingEdge(_shape, node, newStaticNode);
                _shape.liveNodes.Remove(node);
            }

            Utils.CheckParrallel(_shape);

//            OffsetShapeLog.AddLine("Live edges: ", liveEdges.Count);
//            if (liveEdges.Count > 0)//deal with left live edges after the collapse
//            {
//                _shape.AddLiveNode(newLiveNode);//new live node from collapse
//                Edge edgeA = null, edgeB = null;
//                liveEdgeCount = _shape.liveEdges.Count;
//                for (int e = 0; e < liveEdgeCount; e++)//find the two edges left from the collapse
//                {
//                    Edge edge = _shape.liveEdges[e];
//                    if (!_shape.liveEdges.Contains(edge)) continue;
//                    if (edge.nodeA == newLiveNode) edgeA = edge;
//                    if (edge.nodeB == newLiveNode) edgeB = edge;
//                }
//
//                if (edgeA != null && edgeB != null)//if there is a live edge
//                {
//                    Node x = edgeA.GetOtherNode(newLiveNode);
//                    Node y = edgeB.GetOtherNode(newLiveNode);
//                    Utils.CalculateNodeDirAng(newLiveNode, x, y);//recalculate node angle
//                    Utils.NewFormingEdge(_shape, newStaticNode, newLiveNode);//add new forming edge
//                }
//                else
//                {
//                    OffsetShapeLog.AddLine("New live node has not been calculated ", newLiveNode.id);
//                }
//            }

            //            foreach (Node node in nodes)
            //                _data.mesh.ReplaceNode(node, newStaticNode);
        }


        public void SplitEdge(SplitEvent e)
        {
            e.node.position = e.point;
            e.node.earlyTemination = true;
            return;


//            OffsetShapeLog.AddLine("Split event");
//            OffsetShapeLog.AddLine("by node ", e.node.id);
//            OffsetShapeLog.AddLine(e.edge.ToString());
//            //nodes
//            Node nodeStatic = new Node(e.point, e.height);
//            Node newLiveNodeA = new Node(e.point, e.height);
//            Node newLiveNodeB = new Node(e.point, e.height);
//
//            e.newLiveNodeA = newLiveNodeA;
//            e.newLiveNodeB = newLiveNodeB;
//            e.newStaticNode = nodeStatic;
//
//
//            Node nodeOldA = e.edge.nodeA;
//            Node nodeOldB = e.edge.nodeB;
//
//            Edge byEdgeA = e.nodeEdgeA;
//            Edge byEdgeB = e.nodeEdgeB;
//            if (byEdgeA == null || byEdgeB == null)
//            {
//                //TODO work out what to really do here.
//                return;
//            }
//            Node byNodeA = byEdgeA.GetOtherNode(e.node);
//            Node byNodeB = byEdgeB.GetOtherNode(e.node);
//
//            OffsetShapeLog.AddLine("by node a", byNodeA.id);
//            OffsetShapeLog.AddLine("by node b", byNodeB.id);
//
//            if (byNodeA == null || byNodeB == null)
//                return;
//
//            //calculate new node directions
//            Utils.CalculateNodeDirAng(newLiveNodeA, byNodeA, nodeOldA);
//            Utils.CalculateNodeDirAng(newLiveNodeB, nodeOldB, byNodeB);
//
//            _core.shape.AddLiveNode(newLiveNodeA);
//            _core.shape.AddLiveNode(newLiveNodeB);
//            _core.shape.AddStaticNode(nodeStatic);
//            _core.shape.liveNodes.Remove(e.node);
//
//            //discard the old edge
//            OffsetShapeLog.AddLine("Discard old edge ", e.edge.ToString());
//            _core.shape.liveEdges.Remove(e.edge);//
//            byEdgeA.ReplaceNode(e.node, newLiveNodeA);
//            byEdgeB.ReplaceNode(e.node, newLiveNodeB);
//            //create the two new edges from the split
//            Edge newEdgeA = new Edge(nodeOldA, newLiveNodeA);
//            _core.shape.liveEdges.Add(newEdgeA);
//            e.newLiveEdgeA = newEdgeA;
//            Edge newEdgeB = new Edge(newLiveNodeB, nodeOldB);
//            _core.shape.liveEdges.Add(newEdgeB);
//            e.newLiveEdgeB = newEdgeB;

            //forming edges
            //            Utils.RetireFormingEdge(_core.shape, e.node, nodeStatic);
            //            Edge formingEdgeA = Utils.NewFormingEdge(_core.shape, nodeStatic, newLiveNodeA);
            //            Edge formingEdgeB = Utils.NewFormingEdge(_core.shape, nodeStatic, newLiveNodeB);

            //            int aIndex = data.liveNodes.IndexOf(nodeLiveA);
            //            int bIndex = data.liveNodes.IndexOf(nodeLiveB);

//            if (!_core.currentSplits.ContainsKey(newLiveNodeA.id))
//                _core.currentSplits.Add(newLiveNodeA.id, new List<int>());
//            _core.currentSplits[newLiveNodeA.id].Add(newLiveNodeB.id);
//            if (!_core.currentSplits.ContainsKey(newLiveNodeB.id))
//                _core.currentSplits.Add(newLiveNodeB.id, new List<int>());
//            _core.currentSplits[newLiveNodeB.id].Add(newLiveNodeA.id);

            //            _shape.mesh.SplitEdge(e);

//            OffsetShapeLog.AddLine("new live nodes");
//            OffsetShapeLog.AddLine(newLiveNodeA.id);
//            OffsetShapeLog.AddLine(newLiveNodeB.id);
//            OffsetShapeLog.AddLine("new edges - old edge - forming edge a");
//            OffsetShapeLog.AddLine(newEdgeA.ToString());
//            OffsetShapeLog.AddLine(byEdgeA.ToString());
//            //            OffsetShapeLog.AddLine(formingEdgeA.ToString());
//
//            OffsetShapeLog.AddLine("new edges - old edge - forming edge b");
//            OffsetShapeLog.AddLine(byEdgeB.ToString());
//            OffsetShapeLog.AddLine(newEdgeB.ToString());
//            //            OffsetShapeLog.AddLine(formingEdgeB.ToString());
//
//            Utils.CheckParrallel(_core.shape);

        }
    }
}