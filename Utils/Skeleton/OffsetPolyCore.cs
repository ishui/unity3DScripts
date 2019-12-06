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
using System.Text;
using JaspLib;

namespace BuildR2.ShapeOffset
{
    public class OffsetPolyCore
    {
        private static float BOUNDS_MARGIN = 10f;
        private static float SHINK_MULTIPLIER = 0.333f;

        public float maxOffset = 0;
        public float pointAccuracy = 2f;
        public float percentAccuracy = 0.02f;
        public float maxHeight = 0;//default 0, no max
        public float maxDepth = 0;//default 0, no max

        private Shape _shape;
        private bool _init;
        private bool _complete;
        private bool _error;
        public bool calculateInteractions = true;
        private Rect _bounds;

        //        private Dictionary<int, List<int>> _lastSplits = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> currentSplits = new Dictionary<int, List<int>>();

        public Shape shape { get { return _shape; } }
        public bool complete { get { return _complete; } }
        public bool error { get { return _error; } }

        public delegate void OnCompleteEventHandler();
        public delegate void OnErrorEventHandler(string errorMessage);
        public delegate void OnFlipEventHandler(FlipEvent fEvent);
        public delegate void OnSplitEventHandler(SplitEvent sEvent);
        public delegate void OnMergedEventHandler(MergedEvent mEvent);

        public event OnCompleteEventHandler OnCompleteEvent;
        public event OnErrorEventHandler OnErrorEvent;
        public event OnFlipEventHandler OnFlipEvent;
        public event OnSplitEventHandler OnSplitEvent;
        public event OnMergedEventHandler OnMergedEvent;

        public void DebugExecution(bool val)
        {
            OffsetShapeLog.enabled = val;
        }

        public void LiveDebug(bool val)
        {
            if (val) DebugExecution(true);
            OffsetShapeLog.liveLog = val;
        }

        public void Init(Vector2[] points, bool[] gables = null)
        {
            OffsetShapeLog.AddLine("Init");

            if (ShapeIntersection(points))
            {
                _error = true;
                OffsetShapeLog.AddLine("Shape intersection");
                if (OnErrorEvent != null) OnErrorEvent("Provided shape interescts with itself.");
                return;
            }

            _shape = new Shape(points);
            int pointCount = points.Length;

            if (pointCount == 0)
            {
                Debug.LogError("no points sent!");
                return;
            }

            //bounds used to ensure no crazy values created
            _bounds = new Rect(points[0].x, points[0].y, 0, 0);

            //Initialise the data points
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 point = points[p];
                if (point.x < _bounds.xMin) _bounds.xMin = point.x;
                if (point.x > _bounds.xMax) _bounds.xMax = point.x;
                if (point.y < _bounds.yMin) _bounds.yMin = point.y;
                if (point.y > _bounds.yMax) _bounds.yMax = point.y;

                Node outer = new Node(points[p], 0);//create outer node points that outline the shape
                outer.startNode = true;
                _shape.AddStaticNode(outer);

                Node live = new Node(points[p], 0);//create live inner nodes that will sweep inwards
                _shape.AddLiveNode(live);
                Utils.NewFormingEdge(_shape, outer, live);
            }

//            float boundsMargin = BOUNDS_MARGIN;
            _bounds.xMin -= BOUNDS_MARGIN;
            _bounds.xMax += BOUNDS_MARGIN;
            _bounds.yMin -= BOUNDS_MARGIN;
            _bounds.yMax += BOUNDS_MARGIN;

            float smallestLength = Mathf.Infinity;
            //create edges from previous points
            //calculate node directions
            for (int p = 0; p < pointCount; p++)
            {
                int indexA = p;
                int indexX = (p + 1) % pointCount;//next index
                int indexY = (p - 1 + pointCount) % pointCount;//previous

                Node a = _shape.StaticNode(indexA);
                Node x = _shape.StaticNode(indexX);
                Node y = _shape.StaticNode(indexY);
                Node la = _shape.liveNodes[indexA];
                Node lb = _shape.liveNodes[indexX];
                Edge outerEdge = new Edge(a, x);//outer edge - will not move
                _shape.baseEdges.Add(outerEdge);
                _shape.edges.Add(outerEdge);
                if (outerEdge.sqrMagnitude < smallestLength) smallestLength = outerEdge.sqrMagnitude;

//                if (gables != null)
//                {
//                    bool leftIsGabled = gables[indexY];
//                    bool rightIsGabled = gables[indexA];
//
//                    if (!leftIsGabled && !rightIsGabled)
//                    {
//                        Utils.CalculateNodeDirAng(la, x, y);//assign the direction of the node for shrinkage
//                        Edge liveEdge = new Edge(la, lb);//create the live edge that will be sweeping inwards
//                        _shape.liveEdges.Add(liveEdge);
//                    }
//                    else if (leftIsGabled && !rightIsGabled)
//                    {
//                        Utils.CalculateNodeDirAng(la, x, y);//assign the direction of the node for shrinkage
//                        Node l = _shape.liveNodes[indexX];
//                        Edge liveEdge = new Edge(la, lb);//create the live edge that will be sweeping inwards
//                        _shape.liveEdges.Add(liveEdge);
//                    }
//                }
//                else
//                {
                    Utils.CalculateNodeDirAng(la, x, y);//assign the direction of the node for shrinkage
                    Edge liveEdge = new Edge(la, lb);//create the live edge that will be sweeping inwards
                    _shape.liveEdges.Add(liveEdge);
//                }

                //                a.startTangent = ((a.position - y.position).normalized + (x.position - a.position).normalized).normalized;


            }
            _shape.shrinkLength = Mathf.Sqrt(smallestLength);
            OffsetShapeLog.AddLine("shrink length " + _shape.shrinkLength);

            _complete = false;
            _error = false;
            _init = true;
        }

        public void Execute(int maxIterations = 1000)
        {
            int it = 0;
            while (!complete && !error && it < maxIterations)
            {
                //skeleton.ShinkPoly();
                it++;
            }
        }

        public void OffsetPoly(float direction)
        {
            if (!_init)
                return;

            float amount = _shape.shrinkLength * SHINK_MULTIPLIER * Mathf.Sign(direction);
            int liveEdgeCount = _shape.liveEdges.Count;
            int liveNodeCount = _shape.liveNodes.Count;

            if (liveNodeCount == 0 || liveEdgeCount == 0)//nothing more to calculate
            {
                OffsetShapeLog.AddLine("Skeleton Complete");
                _complete = true;
                if (OnCompleteEvent != null) OnCompleteEvent();
                return;
            }

            bool earlyTermination = false;
            float directionSign = Mathf.Sign(direction);
            float maxOffsetSign = Mathf.Sign(maxOffset);
            float useMaxOffset = (directionSign == maxOffsetSign) ? maxOffset : -maxOffset;
            for (int l = 0; l < liveNodeCount; l++)
            {
                Node node = _shape.liveNodes[l];
                //                if(l==0)Debug.Log(node.height+" "+amount+" "+useMaxOffset);
                if (useMaxOffset > 0)
                {
                    if (node.height + amount >= useMaxOffset)//terminate nodes that have reached a defined maximum
                    {
                        amount = useMaxOffset - node.height;
                        earlyTermination = true;
                    }
                }
                else if (useMaxOffset < 0)
                {
                    if (node.height + amount <= useMaxOffset)//terminate nodes that have reached a defined maximum
                    {
                        amount = useMaxOffset - node.height;
                        earlyTermination = true;
                    }
                }
            }

            float maxMovement = 0;
            liveNodeCount = _shape.liveNodes.Count;
            float[] angleNodeMovements = new float[liveNodeCount];
            for (int l = 0; l < _shape.liveNodes.Count; l++)
            {
//                Debug.Log(l);
                Node node = _shape.liveNodes[l];//TODO out of range error
                angleNodeMovements[l] = amount / Mathf.Sin(node.angle * 0.5f * Mathf.Deg2Rad);
                if (Mathf.Abs(angleNodeMovements[l]) > Mathf.Abs(maxMovement) && Mathf.Abs(angleNodeMovements[l]) > 0)
                {
                    maxMovement = angleNodeMovements[l];

                    if (node.angle > 350)
                    {
                        Edge[] edges = Utils.GetABEdge(shape, node);
                        if (edges[0] == null || edges[1] == null)
                            continue;
                        float shortestLength = (edges[0].length + edges[1].length) * 0.5f;
                        //                        Debug.Log(Mathf.Abs(shortestLength / amount));
                        maxMovement /= Mathf.Abs(shortestLength / amount);
                    }

                    //                    Debug.Log(node.id+" "+node.angle);
                }
            }

            float angleScale = amount / maxMovement;
            if (angleScale == Mathf.Infinity) angleScale = 1;
            OffsetShapeLog.AddLine(angleScale);

            for (int l = 0; l < _shape.liveNodes.Count; l++)
            {
                Node node = _shape.liveNodes[l];
                if (node.direction.magnitude < Mathf.Epsilon)//directionless node
                {
                    _shape.liveNodes.Remove(node);
                    liveNodeCount--;
                    l--;
                    continue;
                }
                node.MoveForward(angleNodeMovements[l] * angleScale, amount * angleScale);
            }

            Vector2[] edgeMovements = new Vector2[liveEdgeCount];
            if (calculateInteractions)
            {
                //Event log will collect all events and sort them for us for use once we're happy everything has been processed
                EventLog eventLog = new EventLog();
                EventLog.pointAccuracy = pointAccuracy;
                EventLog.percentAccuracy = percentAccuracy;

                //flip events
                for (int e = 0; e < liveEdgeCount; e++)//TODO check laters
                {
                    Edge edge = _shape.liveEdges[e];
                    edge.DebugDraw(Color.cyan);
                    edge.UpdateValues();//update the edge values to reflect the new node positions
                    Node nodeA = edge.nodeA;
                    Node nodeB = edge.nodeB;
                    edgeMovements[e] = (nodeA.movement + nodeB.movement) * 0.5f;
                    Vector2 intersectionPoint;
                    if (Utils.Intersects(edge.nodeA.previousPosition, edge.nodeA.position, edge.nodeB.previousPosition, edge.nodeB.position, out intersectionPoint))
                        eventLog.AddEvent(eventLog.CreateFlipEvent(edge, intersectionPoint));
                }

                //

                //split events
                for (int n = 0; n < _shape.liveNodeCount; n++)
                {
                    Node node = _shape.liveNodes[n];
                    int nodeID = node.id;

                    //find connecting nodes of splitting node
                    Node nodeA = null, nodeB = null;
                    for (int e = 0; e < liveEdgeCount; e++)
                    {
                        Edge edge = _shape.liveEdges[e];
                        if (edge.Contains(node))
                        {
                            if (edge.nodeA == node) nodeB = edge.nodeB;
                            if (edge.nodeB == node) nodeA = edge.nodeA;
                        }
                    }


                    for (int e = 0; e < liveEdgeCount; e++)
                    {
                        Edge edge = _shape.liveEdges[e];
                        if (edge.Contains(node)) continue;//nodes can't split their own edges - carry on!
                        if (nodeA != null && edge.Contains(nodeA)) continue;//nodes can't split adjacent edges - carry on!
                        if (nodeB != null && edge.Contains(nodeB)) continue;//nodes can't split adjacent edges - carry on!

                        if (currentSplits.ContainsKey(nodeID))//previous splits should never intersect - ingore
                        {
                            if (currentSplits[nodeID].Contains(edge.nodeA.id))
                                continue;
                            if (currentSplits[nodeID].Contains(edge.nodeB.id))
                                continue;
                        }

                        //                        if(!isPartOfShape(node, edge)) continue;

                        Vector2 edgeMovement = edgeMovements[e];
                        Vector2 nodeMovement = node.direction * node.distance - edgeMovement;//simulate collision by moving the point by the vectors of both the point and the edge,
                        //note: collisions are simpler if only one body is moving so we're going to add the edge vector onto the point vector, making the edge remain stationary
                        Vector2 calculationPoint = node.previousPosition + nodeMovement;//calculate the point vector by adding the edge one to it
                        Vector2 edgePosA = edge.nodeA.previousPosition;
                        Vector2 edgePosB = edge.nodeB.previousPosition;
//                        float intersectionalDot = Vector2.Dot(edgeMovement.normalized, node.direction.normalized);

                        //                        OffsetShapeLog.DrawLine(node.previousPosition, calculationPoint,new Color(1,0,0,0.4f));
                        //                        OffsetShapeLog.DrawLine(edgePosA, edgePosB, new Color(1, 0, 1, 0.4f));

                        Vector2 intersectionPoint;
                        float percent = 0;
                        bool intersects = false;
                        //                        if (intersectionalDot < -10.75f)
                        //                        {
                        //                            Debug.DrawLine(Utils.ToV3(node.previousPosition), Utils.ToV3(calculationPoint), Color.red);
                        //                            intersects = Utils.Intersects(node.previousPosition, calculationPoint, edgePosA, edgePosB, out intersectionPoint);
                        //                            if (intersects)
                        //                            {
                        //                                Debug.DrawLine(Utils.ToV3(calculationPoint), Utils.ToV3(calculationPoint) + Vector3.up * 5, Color.magenta);
                        //                            }
                        //                            Vector2 a = node.previousPosition;
                        //                            Vector2 b = node.position;
                        //                            float movementMag = nodeMovement.magnitude;
                        //                            float intersectionMag = (intersectionPoint - a).magnitude;
                        //                            percent = intersectionMag / movementMag;
                        //                            intersectionPoint = Vector2.Lerp(a, b, percent);//translate the point to the real movement point
                        //                        }
                        //                        else
                        //                        {
                        bool dbi = false;//node.id == 14;
                        intersects = Utils.SweepIntersects2(edge.nodeA.previousPosition, edge.nodeB.previousPosition, edge.nodeA.position, edge.nodeB.position, node.previousPosition, node.position, out intersectionPoint, out percent, 0.1f, dbi);
                        calculationPoint = node.position;
                        //                        }
                        //                        if(Utils.Intersects(node.previousPosition, calculationPoint, edgePosA, edgePosB, out intersectionPoint))
                        //                        if (Utils.SweepIntersects(edge.nodeA.previousPosition , edge.nodeB.previousPosition, edge.nodeA.position, edge.nodeB.position, node.previousPosition, node.position, out intersectionPoint, out percent))
                        if (intersects)
                        {
                            for (int be = 0; be < shape.baseEdges.Count; be++)
                            {
                                Edge baseEdge = shape.edges[be];
                                if (Utils.FastLineIntersection(node.previousPosition, node.position, baseEdge.positionA, baseEdge.positionB))
                                    intersects = false;
                                if (Utils.FastLineIntersection(edge.nodeA.position, edge.nodeB.position, baseEdge.positionA, baseEdge.positionB))
                                    intersects = false;
                            }
                        }
                        if (intersects)
                        {
                            OffsetShapeLog.AddLine("Split event detected");
                            SplitEvent splitEvent = new SplitEvent();
                            splitEvent.node = node;
                            splitEvent.edge = edge;
                            OffsetShapeLog.AddLine("node " + node.id);
                            OffsetShapeLog.AddLine("splits edge " + edge.ToString());

                            OffsetShapeLog.DrawLine(node.previousPosition, calculationPoint, Color.red);
                            OffsetShapeLog.DrawLine(edgePosA, edgePosB, Color.magenta);

                            //                            Vector2 a = node.previousPosition;
                            //                            Vector2 b = node.position;
                            //                            Vector2 x = intersectionPoint;//intersectionInfo.Point0;

                            //                            float movementMag = nodeMovement.magnitude;
                            //                            float intersectionMag = (x - a).magnitude;
                            //                            float percent = intersectionMag / movementMag;
                            OffsetShapeLog.AddLine("at percent " + percent);
                            //                            Vector2 actualIntersectionPoint = Vector2.Lerp(a, b, percent);//translate the point to the real movement point

                            float newLengthA = (intersectionPoint - edge.positionA).magnitude;
                            float newLengthB = (intersectionPoint - edge.positionB).magnitude;
                            OffsetShapeLog.AddLine("line a length ", newLengthA);
                            OffsetShapeLog.AddLine("line b length ", newLengthB);

                            SplitEvent sEvent = eventLog.CreateSplitEvent(_shape, node, edge, intersectionPoint, percent, calculationPoint);
                            if (sEvent == null)
                                continue;
                            if (newLengthA > pointAccuracy && newLengthB > pointAccuracy)
                                eventLog.AddEvent(sEvent);//can split - split point not close to either edge nodes
                            else
                            {
                                Node[] nodes = null;
                                if (newLengthA < pointAccuracy && newLengthB < pointAccuracy)
                                    nodes = new[] { node, edge.nodeA, edge.nodeB };//point will split the edge into two edges that can't exist - collapse all nodes
                                else if (newLengthA < pointAccuracy)
                                    nodes = new[] { node, edge.nodeA };//split point close to node a - collapse split node into edge.nodea
                                else if (newLengthB < pointAccuracy)
                                    nodes = new[] { node, edge.nodeB };//split point close to node b - collapse split node into edge.nodeb
                                if (nodes != null)
                                {
                                    MergedEvent mEvent = eventLog.CreateMergeEvent(nodes, intersectionPoint, percent, calculationPoint);
                                    mEvent.Merge(sEvent);
                                    eventLog.AddEvent(mEvent);
                                }
                            }
                        }
                    }

                }

                currentSplits.Clear();
                int eventCount = eventLog.count;
                OffsetShapeLog.AddLine("event count: ", eventCount);
                if (eventCount > 0)
                {
                    float percent = eventLog.percent;
                    earlyTermination = false;
                    foreach (Node node in _shape.liveNodes)
                        node.MoveBack(percent);//move all nodes back to the position of the event
                    foreach (Edge edge in _shape.liveEdges)
                        edge.UpdateValues();//update all edges to reflect this
                    foreach (Node node in _shape.liveNodes)
                        if (_shape.formingEdges.ContainsKey(node)) _shape.formingEdges[node].UpdateValues();
                    for (int e = 0; e < eventCount; e++)
                    {
                        IEvent sevent = eventLog[e];
                        sevent.DrawDebug();
                        OffsetShapeLog.AddLine(string.Format("Event {0} of type {4} at {1} percent and {2},{3}", e, sevent.percent, sevent.point.x, sevent.point.y, sevent.GetType()));
                        switch (sevent.GetType().ToString())
                        {
                            case "BuildR2.ShapeOffset.FlipEvent":
                                FlipEvent fEvent = (FlipEvent)sevent;
                                OffsetShapeLog.AddLine(fEvent.ToString());
                                if (OnFlipEvent != null) OnFlipEvent(fEvent);
                                //                            CollapseEdge(fEvent.edge, fEvent.point, fEvent.height);
                                break;

                            case "BuildR2.ShapeOffset.SplitEvent":
                                SplitEvent sEvent = (SplitEvent)sevent;
                                OffsetShapeLog.AddLine(sevent.ToString());
                                if (OnSplitEvent != null) OnSplitEvent(sEvent);
                                //                            SplitEdge(sEvent);
                                break;

                            case "BuildR2.ShapeOffset.MergedEvent":
                                MergedEvent mEvent = (MergedEvent)sevent;
                                OffsetShapeLog.AddLine(mEvent.ToString());
                                if (OnMergedEvent != null) OnMergedEvent(mEvent);
                                //                            MergeEvent(mEvent);
                                break;
                        }
                    }
                }
                else
                {
                    if (!earlyTermination)
                    {
                        float percent = 1.0f - percentAccuracy;
                        earlyTermination = false;
                        foreach (Node node in _shape.liveNodes)
                            node.MoveBack(percent);//move all nodes back to the position of the event
                        foreach (Edge edge in _shape.liveEdges)
                            edge.UpdateValues();//update all edges to reflect this
                    }
                    foreach (Node node in _shape.liveNodes)
                        if (_shape.formingEdges.ContainsKey(node)) _shape.formingEdges[node].UpdateValues();
                }
            }

            if (earlyTermination)
            {
                _complete = true;
                _shape.TerminateAllNodes();
                if (OnCompleteEvent != null) OnCompleteEvent();
                return;
            }
            else
            {
                //recalculate node directions
                foreach (Node node in _shape.liveNodes)
                    Utils.CalculateNodeDirAng(_shape, node);
            }
        }

        public void CollapseEdge(Edge edge, Vector2 toPoint, float height)
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(edge.nodeA);
            nodes.Add(edge.nodeB);
            CollapseNodes(nodes, toPoint, height);
        }

        public void CollapseNodes(List<Node> nodes, Vector2 toPoint, float height)
        {
            OffsetShapeLog.AddLine("Collapse Nodes:");
            foreach (Node node in nodes)
                OffsetShapeLog.Add(node.id + " ");
            OffsetShapeLog.AddLine("to point: ", toPoint);
            Node newStaticNode = new Node(toPoint, height);//new static node to mark point of collapse
            _shape.AddStaticNode(newStaticNode);//add static node to node array
            OffsetShapeLog.AddLine("new static node added ", newStaticNode.id);

            int liveEdgeCount = _shape.liveEdges.Count;
            List<Edge> liveEdges = new List<Edge>();
            Node newLiveNode = new Node(toPoint, height);//new live node to continue shape forming

            int nodeCount = nodes.Count;
            float minHeight = nodes[0].height;
            float maxHeight = nodes[0].height;
            for (int n = 1; n < nodeCount; n++)
            {
                minHeight = Mathf.Min(minHeight, nodes[n].height);
                maxHeight = Mathf.Min(maxHeight, nodes[n].height);
            }
            for (int n = 0; n < nodeCount; n++)
            {
                nodes[n].position = toPoint;
                nodes[n].height = maxHeight;
            }

            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = _shape.liveEdges[e];
                if (!nodes.Contains(edge.nodeA) || !nodes.Contains(edge.nodeB)) continue;
                edge.UpdateValues();
                if (edge.length < pointAccuracy)//when the edge reaches 0 length it has flipped and should be collapsed
                {
                    OffsetShapeLog.AddLine(edge.ToString(), "has collapsed", edge.length, pointAccuracy);
                    //                    _shape.mesh.CollapseEdge(edge, newLiveNode, newStaticNode);
                    OffsetShapeLog.AddLine("Remove edge ", edge.ToString());
                    _shape.liveEdges.Remove(edge);//remove collapsed edge - from length reaching zero
                    liveEdgeCount--;
                    e--;
                    if (!nodes.Contains(edge.nodeA))
                        nodes.Add(edge.nodeA);
                    if (!nodes.Contains(edge.nodeB))
                        nodes.Add(edge.nodeB);
                }
            }

            OffsetShapeLog.AddLine("find live node edges");
            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = _shape.liveEdges[e];
                if (!edge.Contains(nodes)) continue;
                //                if(edge.length < pointAccuracy) continue;
                if (nodes.Contains(edge.nodeA) && nodes.Contains(edge.nodeB))
                {
                    OffsetShapeLog.AddLine("Remove collapsed edge ", edge.ToString());
                    _shape.liveEdges.Remove(edge);//remove collapsed edge - likely from parallel
                                                  //                    _shape.mesh.EdgeComplete(edge);
                    liveEdgeCount--;
                    e--;
                    continue;
                }
                if (nodes.Contains(edge.nodeA) || newLiveNode == edge.nodeA)
                {
                    OffsetShapeLog.AddLine("replace node a");
                    edge.ReplaceNode(edge.nodeA, newLiveNode);//replace old live node reference to new one
                                                              //                    _shape.mesh.ReplaceNode(edge.nodeA, newLiveNode);
                    liveEdges.Add(edge);
                    continue;
                }
                if (nodes.Contains(edge.nodeB) || newLiveNode == edge.nodeB)
                {
                    OffsetShapeLog.AddLine("replace node b");
                    edge.ReplaceNode(edge.nodeB, newLiveNode);//replace old live node reference to new one
                                                              //                    _shape.mesh.ReplaceNode(edge.nodeB, newLiveNode);
                    liveEdges.Add(edge);
                }
            }

            for (int n = 0; n < nodeCount; n++)
            {
                Node node = nodes[n];
                Utils.RetireFormingEdge(_shape, node, newStaticNode);
                _shape.liveNodes.Remove(node);
            }

            Utils.CheckParrallel(_shape);

            OffsetShapeLog.AddLine("Live edges: ", liveEdges.Count);
            if (liveEdges.Count > 0)//deal with left live edges after the collapse
            {
                _shape.AddLiveNode(newLiveNode);//new live node from collapse
                Edge edgeA = null, edgeB = null;
                liveEdgeCount = _shape.liveEdges.Count;
                for (int e = 0; e < liveEdgeCount; e++)//find the two edges left from the collapse
                {
                    Edge edge = _shape.liveEdges[e];
                    if (!_shape.liveEdges.Contains(edge)) continue;
                    if (edge.nodeA == newLiveNode) edgeA = edge;
                    if (edge.nodeB == newLiveNode) edgeB = edge;
                }

                if (edgeA != null && edgeB != null)//if there is a live edge
                {
                    Node x = edgeA.GetOtherNode(newLiveNode);
                    Node y = edgeB.GetOtherNode(newLiveNode);
                    Utils.CalculateNodeDirAng(newLiveNode, x, y);//recalculate node angle
                    Utils.NewFormingEdge(_shape, newStaticNode, newLiveNode);//add new forming edge
                }
                else
                {
                    OffsetShapeLog.AddLine("New live node has not been calculted ", newLiveNode.id);
                }
            }

            //            foreach (Node node in nodes)
            //                _data.mesh.ReplaceNode(node, newStaticNode);
        }

        public void SplitEdge(SplitEvent e)
        {
            OffsetShapeLog.AddLine("Split event");
            OffsetShapeLog.AddLine("by node ", e.node.id);
            OffsetShapeLog.AddLine(e.edge.ToString());
            //nodes
            Node nodeStatic = new Node(e.point, e.height);
            Node newLiveNodeA = new Node(e.point, e.height);
            Node newLiveNodeB = new Node(e.point, e.height);

            e.newLiveNodeA = newLiveNodeA;
            e.newLiveNodeB = newLiveNodeB;
            e.newStaticNode = nodeStatic;


            Node nodeOldA = e.edge.nodeA;
            Node nodeOldB = e.edge.nodeB;

            Edge byEdgeA = e.nodeEdgeA;
            Edge byEdgeB = e.nodeEdgeB;
            if (byEdgeA == null || byEdgeB == null)
            {
                //TODO work out what to really do here.
                return;
            }
            Node byNodeA = byEdgeA.GetOtherNode(e.node);
            Node byNodeB = byEdgeB.GetOtherNode(e.node);

            OffsetShapeLog.AddLine("by node a", byNodeA.id);
            OffsetShapeLog.AddLine("by node b", byNodeB.id);

            if (byNodeA == null || byNodeB == null)
                return;

            //calculate new node directions
            Utils.CalculateNodeDirAng(newLiveNodeA, byNodeA, nodeOldA);
            Utils.CalculateNodeDirAng(newLiveNodeB, nodeOldB, byNodeB);

            _shape.AddLiveNode(newLiveNodeA);
            _shape.AddLiveNode(newLiveNodeB);
            _shape.AddStaticNode(nodeStatic);
            _shape.liveNodes.Remove(e.node);

            //discard the old edge
            OffsetShapeLog.AddLine("Discard old edge ", e.edge.ToString());
            _shape.liveEdges.Remove(e.edge);//
            byEdgeA.ReplaceNode(e.node, newLiveNodeA);
            byEdgeB.ReplaceNode(e.node, newLiveNodeB);
            //create the two new edges from the split
            Edge newEdgeA = new Edge(nodeOldA, newLiveNodeA);
            _shape.liveEdges.Add(newEdgeA);
            e.newLiveEdgeA = newEdgeA;
            Edge newEdgeB = new Edge(newLiveNodeB, nodeOldB);
            _shape.liveEdges.Add(newEdgeB);
            e.newLiveEdgeB = newEdgeB;

            //forming edges
            Utils.RetireFormingEdge(_shape, e.node, nodeStatic);
            Edge formingEdgeA = Utils.NewFormingEdge(_shape, nodeStatic, newLiveNodeA);
            Edge formingEdgeB = Utils.NewFormingEdge(_shape, nodeStatic, newLiveNodeB);

            //            int aIndex = data.liveNodes.IndexOf(nodeLiveA);
            //            int bIndex = data.liveNodes.IndexOf(nodeLiveB);

            newLiveNodeA.MoveForward(0.1f, 1);
            newLiveNodeB.MoveForward(0.1f, 1);
            if (!currentSplits.ContainsKey(newLiveNodeA.id))
                currentSplits.Add(newLiveNodeA.id, new List<int>());
            currentSplits[newLiveNodeA.id].Add(newLiveNodeB.id);
            if (!currentSplits.ContainsKey(newLiveNodeB.id))
                currentSplits.Add(newLiveNodeB.id, new List<int>());
            currentSplits[newLiveNodeB.id].Add(newLiveNodeA.id);

            //            _shape.mesh.SplitEdge(e);

            OffsetShapeLog.AddLine("new live nodes");
            OffsetShapeLog.AddLine(newLiveNodeA.id);
            OffsetShapeLog.AddLine(newLiveNodeB.id);
            OffsetShapeLog.AddLine("new edges - old edge - forming edge a");
            OffsetShapeLog.AddLine(newEdgeA.ToString());
            OffsetShapeLog.AddLine(byEdgeA.ToString());
            OffsetShapeLog.AddLine(formingEdgeA.ToString());

            OffsetShapeLog.AddLine("new edges - old edge - forming edge b");
            OffsetShapeLog.AddLine(byEdgeB.ToString());
            OffsetShapeLog.AddLine(newEdgeB.ToString());
            OffsetShapeLog.AddLine(formingEdgeB.ToString());

            Utils.CheckParrallel(_shape);

        }

        public void MergeEvent(MergedEvent evnt)
        {
            CollapseNodes(evnt.mergeNodes, evnt.point, evnt.height);
        }

        private bool ShapeIntersection(Vector2[] points)
        {
            int pointCount = points.Length;
            for (int i = 0; i < pointCount; i++)
            {
                int ib = (i + 1) % pointCount;
                for (int j = 0; j < pointCount; j++)
                {
                    if (i == j) continue;
                    if (ib == j) continue;

                    int jb = (j + 1) % pointCount;

                    if (i == jb) continue;
                    if (ib == jb) continue;

                    Vector2 intersection;
                    if (Utils.Intersects(points[i], points[ib], points[j], points[jb], out intersection))
                    {
                        OffsetShapeLog.DrawLine(JMath.ToV3(points[i]), JMath.ToV3(points[ib]), Color.magenta);
                        OffsetShapeLog.DrawLine(JMath.ToV3(points[j]), JMath.ToV3(points[jb]), Color.yellow);
                        OffsetShapeLog.DrawLine(JMath.ToV3(intersection), JMath.ToV3(intersection) + Vector3.up * 10, Color.red);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckBounds()
        {
            foreach (Node node in _shape.liveNodes)
            {
                //                node.DebugDraw(Color.magenta);
                Vector2 pos = node.position;
                if (!_bounds.Contains(pos))
                {
                    //                    Debug.LogError("Node boundary error! node id: "+node.id);
                    return false;
                }
            }
            return true;
        }

        private bool isPartOfShape(Node subject, Edge edge)
        {
            Node startNode = edge.nodeA;
            Node thisNode = edge.nodeB;
            Node lastNode = edge.nodeA;

            if (edge.Contains(subject)) return true;

            int liveEdgeCount = _shape.liveEdges.Count;
            int it = liveEdgeCount * liveEdgeCount;
            while (thisNode != startNode)
            {
                for (int l = 0; l < liveEdgeCount; l++)
                {
                    Edge thisEdge = _shape.liveEdges[l];
                    if (thisEdge.Contains(thisNode) && !thisEdge.Contains(lastNode))
                    {
                        lastNode = thisNode;
                        thisNode = thisEdge.GetOtherNode(lastNode);
                        break;
                    }
                }
                if (thisNode == subject) return true;

                it--;
                if (it < 0) break;
            }

            return false;
        }

        public string errorMessage
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Straight Skeleton Error");

                if (_shape == null)
                {
                    sb.AppendLine("No shape has been set in the skeleton.");
                    return sb.ToString();
                }

                sb.AppendLine("The algorithm failed to generate a correct straight skeleton.");
                sb.AppendLine("Please report this to the developer email@jasperstocker.com");
                sb.AppendLine("Send this entire error message");
                sb.AppendLine(" ");
                sb.AppendLine("private Vector2[] errorShape ={");
                foreach (Vector2 point in _shape.baseShape)
                    sb.AppendLine(string.Format("new Vector2({0}, {1}),", point.x, point.y));
                sb.AppendLine("};");
                sb.AppendLine(" ");
                sb.AppendLine("Error message end");
                return sb.ToString();
            }
        }
    }
}