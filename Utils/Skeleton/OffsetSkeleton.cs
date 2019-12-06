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
    public class OffsetSkeleton
    {

        private OffsetPolyCore _core;
        private bool _debug;
        public float direction = 0;
        private int _maxIterations = 150;

        public float pointAccuracy = 5f;
        public float percentAccuracy = 0.1f;

//        private Dictionary<int, List<int>> _lastSplits = new Dictionary<int, List<int>>();
//        private Dictionary<int, List<int>> _currentSplits = new Dictionary<int, List<int>>();
        private Dictionary<Node, Node> _substitutions = new Dictionary<Node, Node>();
        private Dictionary<Edge, Node> _collapsedEdges = new Dictionary<Edge, Node>();
        private Dictionary<Edge, SplitEvent> _splitEdges = new Dictionary<Edge, SplitEvent>();

        private List<Node> nodeTris = new List<Node>();

        public bool complete { get { return _core.complete; } }
        public bool error { get { return _core.error; } }

        public Shape shape { get { return _core.shape; } }

        public OffsetSkeleton(Vector2[] poly, bool[] gables = null, float distance = 0, bool debug = false)
        {
            nodeTris.Clear();
            _debug = debug;
            if (direction == 0) direction = -1;
            _core = new OffsetPolyCore();
            _core.LiveDebug(debug);
            _core.maxOffset = distance;
            _core.percentAccuracy = percentAccuracy;
            _core.calculateInteractions = true;
            _core.Init(poly, gables);
            _core.OnFlipEvent += OnFlip;
            _core.OnSplitEvent += OnSplit;
            _core.OnMergedEvent += OnMerged;
            _core.OnCompleteEvent += OnComplete;
            _core.OnErrorEvent += OnError;
        }

        public Vector2[] Shape()
        {
            if (_core.shape == null) return new Vector2[0];
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
            fEvent.DrawDebug();

            List<Node> nodes = new List<Node>();
            nodes.Add(fEvent.edge.nodeA);
            nodes.Add(fEvent.edge.nodeB);
            CollapseNodes(nodes, fEvent.point, fEvent.height, fEvent);
            //CollapseEdge(fEvent.edge, fEvent.point, fEvent.height);
        }

        private void OnSplit(SplitEvent sEvent)
        {
            sEvent.DrawDebug();
            SplitEvent(sEvent);
            Utils.CheckParrallel(_core.shape);
        }

        private void OnMerged(MergedEvent mEvent)
        {
            mEvent.DrawDebug();
            MergeEvent(mEvent);
            Utils.CheckParrallel(_core.shape);
        }

        private void ClearPreviousOffsetData()
        {
            _substitutions.Clear();
            _collapsedEdges.Clear();
            _splitEdges.Clear();
        }

        public void Execute()
        {
            int it = 0;
            while (!complete && !error && it < _maxIterations)
            {
                ClearPreviousOffsetData();
                _core.OffsetPoly(direction);
                it++;
            }
        }

        public void ExecuteStep()
        {
            ClearPreviousOffsetData();
            _core.OffsetPoly(direction);

            if (_core.error)
                Debug.LogWarning(_core.errorMessage);
        }

        public void Cleanup()
        {

        }


        //        public void CollapseEdge(Edge edge, Vector2 toPoint, float height)
        //        {
        //            List<Node> nodes = new List<Node>();
        //            nodes.Add(edge.nodeA);
        //            nodes.Add(edge.nodeB);
        //            CollapseNodes(nodes, toPoint, height);
        //        }

        public void MergeEvent(MergedEvent mEvent)
        {
            OffsetShapeLog.AddLine("Complex merge event gynastics ahead");
            CollapseNodes(mEvent.mergeNodes, mEvent.point, mEvent.height, mEvent);

            int splitCount = mEvent.splitEvents.Count;

            OffsetShapeLog.AddLine("Do the splits - in percent order");
            float currentPercent = -1;
            for (int s = 0; s < splitCount; s++)
            {
                float minimumPercent = 1;
                SplitEvent candidate = null;
                for (int so = 0; so < splitCount; so++)
                {
                    SplitEvent sEvent = mEvent.splitEvents[so];
                    //                    OffsetShapeLog.AddLine(sEvent.ToString());
                    //                    OffsetShapeLog.AddLine(sEvent.percent, currentPercent);
                    if (sEvent.percent > currentPercent)
                    {
                        //                        OffsetShapeLog.AddLine(sEvent.percent, minimumPercent);
                        if (sEvent.percent < minimumPercent)
                        {
                            minimumPercent = sEvent.percent;
                            candidate = sEvent;
                        }
                    }
                }
                if (candidate != null)
                {
                    SplitEvent(candidate);
                    currentPercent = candidate.percent;
                }
                //                mEvent.splitEvents[s].point = mEvent.point;
            }
        }

        public void SplitEvent(SplitEvent sEvent)
        {
            if (_substitutions.ContainsKey(sEvent.node))
            {
                OffsetShapeLog.AddLine(sEvent.node + " is replaced with " + _substitutions[sEvent.node]);
                sEvent.node = _substitutions[sEvent.node];//replce any modified node values
            }

            if (sEvent.edge.Contains(sEvent.node))//edge splitting itself
                return;

            if (_collapsedEdges.ContainsKey(sEvent.edge))
            {
                CollapseNodes(new List<Node> { sEvent.node, _collapsedEdges[sEvent.edge] }, sEvent.point, sEvent.height, sEvent);
                OffsetShapeLog.AddLine("Split event falls on edge collapse.");
                return;//if the edge we were splitting already collaped - merge it
            }

            if (_splitEdges.ContainsKey(sEvent.edge))
            {
                SplitEvent previousSplit = _splitEdges[sEvent.edge];

                float sqrMagA = Vector2.SqrMagnitude(sEvent.edge.nodeA.position - sEvent.point);
                float sqrMagB = Vector2.SqrMagnitude(previousSplit.edge.nodeA.position - previousSplit.point);

                if (sqrMagA < sqrMagB)
                {
                    OffsetShapeLog.AddLine(sEvent.edge.ToString() + " is replaced with " + previousSplit.newLiveEdgeA.ToString());
                    sEvent.edge = previousSplit.newLiveEdgeA;
                }
                else
                {
                    OffsetShapeLog.AddLine(sEvent.edge.ToString() + " is replaced with " + previousSplit.newLiveEdgeB.ToString());
                    sEvent.edge = previousSplit.newLiveEdgeB;
                }
            }

            SplitEdge(sEvent);
        }

        public void Draw()
        {
            OffsetShapeLog.enabled = true;
            OffsetShapeLog.DrawShapeNodes(_core.shape);

            int triCount = nodeTris.Count;
            for (int t = 0; t < triCount; t += 3)
            {
                Node a = nodeTris[t];
                Node b = nodeTris[t + 1];
                Node c = nodeTris[t + 2];

                Debug.DrawLine(Utils.ToV3(a.position), Utils.ToV3(b.position), Color.yellow);
                Debug.DrawLine(Utils.ToV3(b.position), Utils.ToV3(c.position), Color.yellow);
                Debug.DrawLine(Utils.ToV3(c.position), Utils.ToV3(a.position), Color.yellow);
            }
        }

        private void CollapseNodes(List<Node> nodes, Vector2 toPoint, float height, BaseEvent evt)
        {
            if (nodes.Count < 2)
                return;
            OffsetShapeLog.AddLine("Collapse Nodes:");
            foreach (Node node in nodes)
                OffsetShapeLog.Add(node.id + " ");
            OffsetShapeLog.AddLine("to point: ", toPoint);
            Node newStaticNode = (evt.newStaticNode == null) ? new Node(toPoint, height) : evt.newStaticNode;//new static node to mark point of collapse
            evt.newStaticNode = newStaticNode;
            _core.shape.AddStaticNode(newStaticNode);//add static node to node array

            int liveEdgeCount = _core.shape.liveEdges.Count;
            List<Edge> liveEdges = new List<Edge>();
            //            Node newLiveNode = new Node(toPoint, nodes[0].height);//new live node to continue shape forming

            int nodeCount = nodes.Count;
            for (int n = 0; n < nodeCount; n++)
                nodes[n].position = toPoint;

            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = _core.shape.liveEdges[e];
                if (!nodes.Contains(edge.nodeA) || !nodes.Contains(edge.nodeB)) continue;
                edge.UpdateValues();
                if (edge.length < pointAccuracy)//when the edge reaches 0 length it has flipped and should be collapsed
                {
                    OffsetShapeLog.AddLine(edge.ToString(), "has collapsed", edge.length, pointAccuracy);
                    OffsetShapeLog.AddLine("Remove edge ", edge.ToString());
                    _core.shape.liveEdges.Remove(edge);//remove collapsed edge - from length reaching zero
                    _collapsedEdges.Add(edge, newStaticNode);
                    liveEdgeCount--;
                    e--;
                    if (!nodes.Contains(edge.nodeA))
                        nodes.Add(edge.nodeA);
                    if (!nodes.Contains(edge.nodeB))
                        nodes.Add(edge.nodeB);
                }
            }

            OffsetShapeLog.AddLine("find live node edges");
            Dictionary<Node, int> nodeOccurances = new Dictionary<Node, int>();//check parallel collapses
            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = _core.shape.liveEdges[e];
                if (!edge.Contains(nodes)) continue;//unaffected edge

                if (nodes.Contains(edge.nodeA) && nodes.Contains(edge.nodeB))//edge is completely affected by the merge and has collapsed
                {
                    OffsetShapeLog.AddLine("Remove collapsed edge ", edge.ToString());
                    _core.shape.liveEdges.Remove(edge);//remove collapsed edge
                    _collapsedEdges.Add(edge, newStaticNode);
                    liveEdgeCount--;
                    e--;
                    continue;
                }
                if (nodes.Contains(edge.nodeA))// || newLiveNode == edge.nodeA)
                {
                    //                    edge.ReplaceNode(edge.nodeA, newLiveNode);//replace old live node reference to new one //TODO
                    liveEdges.Add(edge);
                    OffsetShapeLog.AddLine("Live edges: ", edge);
                    if (nodeOccurances.ContainsKey(edge.nodeB))
                        nodeOccurances[edge.nodeB]++;
                    else
                        nodeOccurances.Add(edge.nodeB, 1);
                    continue;
                }
                if (nodes.Contains(edge.nodeB))// || newLiveNode == edge.nodeB)
                {
                    //                    edge.ReplaceNode(edge.nodeB, newLiveNode);//replace old live node reference to new one //TODO
                    liveEdges.Add(edge);
                    OffsetShapeLog.AddLine("Live edges: ", edge);
                    if (nodeOccurances.ContainsKey(edge.nodeA))
                        nodeOccurances[edge.nodeA]++;
                    else
                        nodeOccurances.Add(edge.nodeA, 1);
                }
            }

            int affectedLiveEdges = liveEdges.Count;
            foreach (KeyValuePair<Node, int> kv in nodeOccurances)
            {
                OffsetShapeLog.AddLine("node occured: ", kv.Key.id, kv.Value);
                if (kv.Value > 1)
                {
                    Node pinchedNode = kv.Key;
                    OffsetShapeLog.AddLine("Pinched node: ", pinchedNode.id);
                    pinchedNode.position = toPoint;
                    for (int a = 0; a < affectedLiveEdges; a++)
                    {
                        shape.formingEdges[kv.Key].ReplaceNode(kv.Key, newStaticNode);
                        if (liveEdges[a].Contains(kv.Key))//any live edges that contains the node should be culled - it has collapsed
                        {
                            Edge edge = liveEdges[a];
                            OffsetShapeLog.AddLine("Collapsed Edge: ", edge);
                            liveEdges.Remove(edge);
                            _core.shape.liveEdges.Remove(edge);//remove collapsed edge
                            affectedLiveEdges--;
                            a--;
                        }
                    }
                    Utils.RetireFormingEdge(shape, kv.Key, newStaticNode);
                    _core.shape.liveNodes.Remove(kv.Key);
                }
            }

            //            for (int n = 0; n < nodeCount; n++)
            //            {
            //                Node node = nodes[n];
            //                Utils.RetireFormingEdge(_core.shape, node, newStaticNode);
            //                _core.shape.liveNodes.Remove(node);
            ////                _substitutions.Add(node, newLiveNode); TODO EEK!
            //            }

            OffsetShapeLog.AddLine("Live edges: ", liveEdges.Count);
            if (affectedLiveEdges > 0)//deal with left live edges after the collapse - calculate the angle the new node needs to move into
            {
                float[] angles = new float[affectedLiveEdges];
                int smallestAngleIndex = 0;//keep this for when we need to loop the angle comparison
                float smallestAngle = Mathf.Infinity;
                for (int a = 0; a < affectedLiveEdges; a++)
                {
                    Node from = liveEdges[a].GetEdgeNode(nodes);
                    Node to = liveEdges[a].GetOtherNode(from);
                    Vector2 dir = (to.position - from.position).normalized;
                    float angle = Utils.SignAngle(dir);
                    angles[a] = angle;
                    OffsetShapeLog.AddLine(liveEdges[a], angle);
                    if (angle < smallestAngle)
                    {
                        smallestAngle = angle;
                        smallestAngleIndex = a;
                    }
                }

                Edge startEdge = null;
                for (int a = 0; a < affectedLiveEdges; a++)
                {
                    if (nodes.Contains(liveEdges[a].nodeA))
                    {
                        startEdge = liveEdges[a];
                        break;
                    }
                }
                if (startEdge != null)
                {
                    Edge[] orderedEdges = new Edge[affectedLiveEdges];
                    orderedEdges[0] = startEdge;
                    Edge currentEdge = startEdge;
                    float currentAngle = angles[liveEdges.IndexOf(currentEdge)];
                    int orderIndex = 1;

                    OffsetShapeLog.AddLine("order edges by angle");
                    OffsetShapeLog.AddLine(0, startEdge);
                    while (orderIndex < affectedLiveEdges)
                    {
                        Edge candidate = null;
                        float candidateAngle = Mathf.Infinity;
                        for (int a = 0; a < affectedLiveEdges; a++)
                        {
                            Edge nextEdge = liveEdges[a];
                            if (currentEdge == nextEdge) continue;
                            float nextAngle = angles[liveEdges.IndexOf(nextEdge)];
                            if (nextAngle > currentAngle)
                            {
                                if (nextAngle < candidateAngle)
                                {
                                    candidateAngle = nextAngle;
                                    candidate = nextEdge;
                                }
                            }
                        }

                        if (candidate == null)
                            candidate = liveEdges[smallestAngleIndex];

                        if (candidate != null)
                        {
                            OffsetShapeLog.AddLine(orderIndex, candidate);
                            orderedEdges[orderIndex] = candidate;
                            orderIndex++;
                            currentEdge = candidate;
                            currentAngle = angles[liveEdges.IndexOf(currentEdge)];
                        }
                    }

                    OffsetShapeLog.AddLine("affected Live Edge count" + affectedLiveEdges);
                    List<Node> newLiveNodes = new List<Node>();
                    if (affectedLiveEdges % 2 != 0)
                    {
                        //                        Debug.LogError("affected Live Edge count uneven: "+ affectedLiveEdges);
                        //                        Debug.LogError("");
                        return;
                    }
                    for (int o = 0; o < affectedLiveEdges; o += 2)
                    {
                        Edge splitEdgeA = orderedEdges[o];
                        Edge splitEdgeB = orderedEdges[o + 1];
                        OffsetShapeLog.AddLine("split Edge A", splitEdgeA);
                        OffsetShapeLog.AddLine("split Edge B", splitEdgeB);

                        Node newLiveNode = new Node(toPoint, nodes[0].height);//new live node to continue shape forming
                        _core.shape.AddLiveNode(newLiveNode);//new live node from collapse
                        newLiveNodes.Add(newLiveNode);

                        if (nodes.Contains(splitEdgeA.nodeA))
                            splitEdgeA.ReplaceNode(splitEdgeA.nodeA, newLiveNode);//replace old live node reference to new one
                        else
                            splitEdgeA.ReplaceNode(splitEdgeA.nodeB, newLiveNode);//replace old live node reference to new one

                        if (nodes.Contains(splitEdgeB.nodeA))
                            splitEdgeB.ReplaceNode(splitEdgeB.nodeA, newLiveNode);//replace old live node reference to new one
                        else
                            splitEdgeB.ReplaceNode(splitEdgeB.nodeB, newLiveNode);//replace old live node reference to new one

                        Node x = splitEdgeA.GetOtherNode(newLiveNode);
                        Node y = splitEdgeB.GetOtherNode(newLiveNode);
                        Utils.CalculateNodeDirAng(newLiveNode, x, y);//recalculate node angle
                        Utils.NewFormingEdge(_core.shape, newStaticNode, newLiveNode);//add new forming edge

                    }

                    int newLiveNodeCount = newLiveNodes.Count;
                    for (int l = 0; l < newLiveNodeCount; l++)
                    {
                        Node lNode = newLiveNodes[l];
                        if (!_core.currentSplits.ContainsKey(lNode.id))
                            _core.currentSplits.Add(lNode.id, new List<int>());

                        for (int lb = 0; lb < newLiveNodeCount; lb++)
                        {
                            if (l == lb) continue;
                            Node lbNode = newLiveNodes[lb];
                            _core.currentSplits[lNode.id].Add(lbNode.id);
                        }
                    }
                }






                //                Edge edgeA = null, edgeB = null;
                //                liveEdgeCount = _core.shape.liveEdges.Count;
                //                for (int e = 0; e < liveEdgeCount; e++)//find the two edges left from the collapse
                //                {
                //                    Edge edge = _core.shape.liveEdges[e];
                //                    if (!_core.shape.liveEdges.Contains(edge)) continue;//not a live edge
                //                    if (edge.nodeA == newLiveNode) edgeA = edge;
                //                    if (edge.nodeB == newLiveNode) edgeB = edge;
                //                }
                //
                //                if (edgeA != null && edgeB != null)//if there is a live edge
                //                {
                //                    Node x = edgeA.GetOtherNode(newLiveNode);
                //                    Node y = edgeB.GetOtherNode(newLiveNode);
                //                    Utils.CalculateNodeDirAng(newLiveNode, x, y);//recalculate node angle
                //                    Utils.NewFormingEdge(_core.shape, newStaticNode, newLiveNode);//add new forming edge
                //                }
                //                else
                //                {
                //                    OffsetShapeLog.AddLine("New live node has not been calculted ", newLiveNode.id);
                //                }
            }


            for (int n = 0; n < nodeCount; n++)
            {
                Node node = nodes[n];
                Utils.RetireFormingEdge(_core.shape, node, newStaticNode);
                _core.shape.liveNodes.Remove(node);
                //                _substitutions.Add(node, newLiveNode); TODO EEK!
            }

            Utils.CheckParrallel(_core.shape);
        }


        public void SplitEdge(SplitEvent e)
        {
            OffsetShapeLog.AddLine("Split event");
            OffsetShapeLog.AddLine("by node ", e.node.id);
            OffsetShapeLog.AddLine(e.edge.ToString());
            //nodes
            float realHeight = e.node.height;

            Node nodeStatic = (e.newStaticNode == null) ? new Node(e.point, realHeight) : e.newStaticNode;
            e.newStaticNode = nodeStatic;

            Node nodeOldA = e.edge.nodeA;
            Node nodeOldB = e.edge.nodeB;

            Edge[] edges = Utils.GetABEdge(shape, e.node);
            if (edges[0] == null || edges[1] == null)
            {
                //TODO work out what to really do here.
                return;
            }
            Edge byEdgeA = edges[0];
            Edge byEdgeB = edges[1];
            Node byNodeA = byEdgeA.GetOtherNode(e.node);
            Node byNodeB = byEdgeB.GetOtherNode(e.node);

            OffsetShapeLog.AddLine("by node a", byNodeA.id);
            OffsetShapeLog.AddLine("by node b", byNodeB.id);

            if (byNodeA == null || byNodeB == null)
                return;
            int insertionIndex = _core.shape.LiveIndex(e.node);
            _core.shape.AddStaticNode(nodeStatic);
            _core.shape.liveNodes.Remove(e.node);
            //discard the old edge
            OffsetShapeLog.AddLine("Discard old edge ", e.edge.ToString());
            _core.shape.liveEdges.Remove(e.edge);//
            if (!_splitEdges.ContainsKey(e.edge))
                _splitEdges.Add(e.edge, e);
            Utils.RetireFormingEdge(_core.shape, e.node, nodeStatic);

            Node newLiveNodeA = null;
            Node newLiveNodeB = null;
            //node a
            if(!e.edge.Contains(byNodeA))
            {
                newLiveNodeA = new Node(e.point, realHeight);
                e.newLiveNodeA = newLiveNodeA;
                //calculate new node directions
                Utils.CalculateNodeDirAng(newLiveNodeA, byNodeA, nodeOldA);
                _core.shape.InsertLiveNode(insertionIndex, newLiveNodeA);
                byEdgeA.ReplaceNode(e.node, newLiveNodeA);
                //create the two new edges from the split
                Edge newEdgeA = new Edge(nodeOldA, newLiveNodeA);
                _core.shape.liveEdges.Add(newEdgeA);
                e.newLiveEdgeA = newEdgeA;
                Edge formingEdgeA = Utils.NewFormingEdge(_core.shape, nodeStatic, newLiveNodeA);

                OffsetShapeLog.AddLine("new live node a");
                OffsetShapeLog.AddLine(newLiveNodeA.id);
                OffsetShapeLog.AddLine("new edges - old edge - forming edge a");
                OffsetShapeLog.AddLine(newEdgeA.ToString());
                OffsetShapeLog.AddLine(byEdgeA.ToString());
                OffsetShapeLog.AddLine(formingEdgeA.ToString());
            }
            else
            {
                Utils.RetireFormingEdge(_core.shape, byNodeA, nodeStatic);
                _core.shape.liveNodes.Remove(byNodeA);
            }

            //node b
            if (!e.edge.Contains(byNodeB))
            {
                newLiveNodeB = new Node(e.point, realHeight);
                e.newLiveNodeB = newLiveNodeB;
                Utils.CalculateNodeDirAng(newLiveNodeB, nodeOldB, byNodeB);
                _core.shape.InsertLiveNode(insertionIndex, newLiveNodeB);
                byEdgeB.ReplaceNode(e.node, newLiveNodeB);
                Edge newEdgeB = new Edge(newLiveNodeB, nodeOldB);
                _core.shape.liveEdges.Add(newEdgeB);
                e.newLiveEdgeB = newEdgeB;
                Edge formingEdgeB = Utils.NewFormingEdge(_core.shape, nodeStatic, newLiveNodeB);

                OffsetShapeLog.AddLine("new live node b");
                OffsetShapeLog.AddLine(newLiveNodeB.id);
                OffsetShapeLog.AddLine("new edges - old edge - forming edge b");
                OffsetShapeLog.AddLine(byEdgeB.ToString());
                OffsetShapeLog.AddLine(newEdgeB.ToString());
                OffsetShapeLog.AddLine(formingEdgeB.ToString());
            }
            else
            {
                Utils.RetireFormingEdge(_core.shape, byNodeB, nodeStatic);
                _core.shape.liveNodes.Remove(byNodeB);
            }

            if (newLiveNodeA != null && newLiveNodeB != null)
            {
                if (!_core.currentSplits.ContainsKey(newLiveNodeA.id))
                    _core.currentSplits.Add(newLiveNodeA.id, new List<int>());
                _core.currentSplits[newLiveNodeA.id].Add(newLiveNodeB.id);
                if (!_core.currentSplits.ContainsKey(newLiveNodeB.id))
                    _core.currentSplits.Add(newLiveNodeB.id, new List<int>());
                _core.currentSplits[newLiveNodeB.id].Add(newLiveNodeA.id);
            }
        }
    }
}
