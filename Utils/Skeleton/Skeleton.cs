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



//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//
//namespace BuildR2.ShapeOffset
//{
//    public class Skeleton
//    {
//        private static float SHINK_MULTIPLIER = 0.333f;
//        private static float BOUNDS_MARGIN = 10f;
//
//        public float pointAccuracy = 2f;
//        public float percentAccuracy = 0.02f;
//        public float maxHeight = 0;//default 0, no max
//        public float maxDepth = 0;//default 0, no max
//
//        private SkeletonData _data;
//        private Dictionary<int, List<int>> _lastSplits = new Dictionary<int, List<int>>();
//        private Dictionary<int, List<int>> currentSplits = new Dictionary<int, List<int>>();
//        private Rect _bounds;
//        private bool _complete;
//        private bool _error;
//
//        public SkeletonData data {get {return _data;}}
//
//        public bool complete { get {return _complete;} }
//        public bool error { get {return _error; } }
//
//        public delegate void OnCompleteEventHandler();
//        public delegate void OnErrorEventHandler(string errorMessage);
//
//        public event OnCompleteEventHandler OnCompleteEvent;
//        public event OnErrorEventHandler OnErrorEvent;
//        
////        public void SetShape(Vector2[] points, float maxOffset = 0)
//        public void SetShape(Vector2[] points)
//        {
//            if(!TestBaseShape(points))
//            {
//                _error = true;
//                if (OnErrorEvent != null) OnErrorEvent("Provided shape interescts with itself.");
//                return;
//            }
//
//            _data = new SkeletonData(points);
//            _data.maxOffset = 0;
//            int pointCount = points.Length;
//
//            _bounds = new Rect(points[0].x, points[0].y, 0, 0);
//
//            //Initialise the data points
//            for (int p = 0; p < pointCount; p++)
//            {
//                Vector2 point = points[p];
//                if (point.x < _bounds.xMin) _bounds.xMin = point.x;
//                if (point.x > _bounds.xMax) _bounds.xMax = point.x;
//                if (point.y < _bounds.yMin) _bounds.yMin = point.y;
//                if (point.y > _bounds.yMax) _bounds.yMax = point.y;
//
//                Node outer = new Node(points[p], 0);//create outer node points that outline the shape
//                outer.startNode = true;
//                _data.AddStaticNode(outer);
//
//                Node live = new Node(points[p], 0);//create live inner nodes that will sweep inwards
//                _data.AddLiveNode(live);
//                Utils.NewFormingEdge(_data, outer, live);
//            }
//
//            _bounds.xMin -= BOUNDS_MARGIN;
//            _bounds.xMax += BOUNDS_MARGIN;
//            _bounds.yMin -= BOUNDS_MARGIN;
//            _bounds.yMax += BOUNDS_MARGIN;
//
//            float smallestLength = Mathf.Infinity;
//            //create edges from previous points
//            //calculate node directions
//            for (int p = 0; p < pointCount; p++)
//            {
//                int indexA = p;
//                int indexX = (p + 1) % pointCount;//next index
//                int indexY = (p - 1 + pointCount) % pointCount;//previous
//
//                Node a = _data.StaticNode(indexA);
//                Node x = _data.StaticNode(indexX);
//                Node y = _data.StaticNode(indexY);
//                Node la = _data.liveNodes[indexA];
//                Node lb = _data.liveNodes[indexX];
//                Edge newEdge = new Edge(a, x);//outer edge - will no move
//                _data.edges.Add(newEdge);
//                if (newEdge.sqrMagnitude < smallestLength) smallestLength = newEdge.sqrMagnitude;
//
//                Utils.CalculateNodeDirAng(la, x, y);//assign the direction of the node for shrinkage
//
//                a.startTangent = ((a.position-y.position).normalized + (x.position - a.position).normalized).normalized;
//
//
//                Edge liveEdge = new Edge(la, lb);//create the live edge that will be sweeping inwards
//                _data.liveEdges.Add(liveEdge);
//            }
//            _data.shrinkLength = Mathf.Sqrt(smallestLength);
//
//            _data.mesh.Build(_data);
//            OffsetShapeLog.Clear();
//
//            _complete = false;
//            _error = false;
//        }
//
//        public void Clear()
//        {
//            if(_data!=null)
//                _data.Clear();
//            _lastSplits.Clear();
//            currentSplits.Clear();
//            _complete = false;
//            _error = false;
//        }
//
//        public void OffsetPoly(float direction)
//        {
//            OffsetShapeLog.AddLine("Shrink Poly");
//            _data.unitHeight += 1.0f;
//            float amount = _data.shrinkLength * SHINK_MULTIPLIER * Mathf.Sign(direction);
//            float maxOffset = _data.maxOffset;
//            int liveEdgeCount = _data.liveEdges.Count;
//            int liveNodeCount = _data.liveNodes.Count;
//
//            for(int l = 0; l < liveNodeCount; l++)
//            {
//                Node node = _data.liveNodes[l];
//                if(node.earlyTemination)
//                {
////                    _data.RetireNode(node);
//                    CollapseNodes(new List<Node> { node }, node.position, node.height);
//                    liveNodeCount--;
//                    l--;
//                }
//            }
//
//            if (liveNodeCount == 0 || liveEdgeCount == 0)//nothing more to calculate
//            {
//                OffsetShapeLog.AddLine("Skeleton Complete");
//                _data.mesh.Clean(_data);
//                NormaliseHeights();
//                SkeletonUV uvGen = new SkeletonUV();
//                uvGen.Build(_data);
//                _complete = true;
//                if (OnCompleteEvent != null) OnCompleteEvent();
//                return;
//            }
//            if(!CheckBounds())//if a node breaches the bounds of the origin shape, the generation has failed somehow.
//            {
//                _error = true;
//                if (OnErrorEvent != null) OnErrorEvent(errorMessage);
//                return;
//            }
//
//            Vector2[] edgeMovements = new Vector2[liveEdgeCount];
//            float maxMovement = 0;
//            for(int l = 0; l < liveNodeCount; l++)
//            {
//                Node node = _data.liveNodes[l];
//                if(node.earlyTemination) continue;
//                float nodeMovement = amount / Mathf.Sin(node.angle * 0.5f * Mathf.Deg2Rad);
//                if(maxOffset > 0 && node.totalDistance + nodeMovement >= maxOffset)
//                {
//                    nodeMovement = maxOffset - node.totalDistance;
//                    node.earlyTemination = true;
//                }
//                if (nodeMovement > maxMovement)
//                    maxMovement = nodeMovement;
//            }
//            float movementScale = amount / maxMovement;
//            
//            for (int l = 0; l < liveNodeCount; l++)
//            {
//                Node node = _data.liveNodes[l];
//                if(node.direction.magnitude == 0)
//                {
//                    _data.liveNodes.Remove(node);
//                    liveNodeCount--;
//                    l--;
//                    continue;
//                }
//                float nodeAmount = (amount * movementScale) / Mathf.Sin(node.angle * 0.5f * Mathf.Deg2Rad);
//                OffsetShapeLog.LabelNode(node);
//                node.MoveForward(nodeAmount, movementScale);
//                node.DebugDrawMovement(Color.blue);
//            }
//
//            //Event log will collect all events and sort them for us for use once we're happy everything has been processed
//            EventLog eventLog = new EventLog();
//            eventLog.pointAccuracy = pointAccuracy;
//            eventLog.percentAccuracy = percentAccuracy;
//
//            //flip events
//            for(int e = 0; e < liveEdgeCount; e++)//TODO check laters
//            {
//                Edge edge = _data.liveEdges[e];
//                edge.UpdateValues();//update the edge values to reflect the new node positions
////                edge.DebugDraw(new Color(1, 0, 1, 0.2f));
//                Node nodeA = edge.nodeA;
//                Node nodeB = edge.nodeB;
//                edgeMovements[e] = (nodeA.movement + nodeB.movement) * 0.5f;
////                Segment2 movementA = nodeA.movementSeg;
////                Segment2 movementB = nodeB.movementSeg;
////                Segment2Segment2Intr intersectionInfo;
////                if(Intersection.FindSegment2Segment2(ref movementA, ref movementB, out intersectionInfo))
//                Vector2 intersectionPoint;
//                if(Utils.Intersects(edge.nodeA.previousPosition, edge.nodeA.position, edge.nodeB.previousPosition, edge.nodeB.position, out intersectionPoint))
//                {
//                    eventLog.AddEvent(eventLog.CreateFlipEvent(edge, intersectionPoint));
////                    OffsetShapeLog.AddLine("Flip event detected");
////                    FlipEvent flipEvent = new FlipEvent();
////                    flipEvent.edge = edge;
////                    OffsetShapeLog.AddLine(edge.ToString());
////                    Vector2 point = intersectionPoint;//intersectionInfo.Point0;
////                    float pointADistance = Vector2.Distance(point, nodeA.previousPosition);
////                    float pointBDistance = Vector2.Distance(point, nodeB.previousPosition);
////                    float percentA = pointADistance / nodeA.distance;
////                    float percentB = pointBDistance / nodeB.distance;
////                    float percent = Mathf.Min(percentA, percentB);
//////                    float height = Mathf.Min(nodeA.height, nodeB.height);
////                    float height = (nodeA.height + nodeB.height) * 0.5f;
////                    flipEvent.percent = percent;
////                    flipEvent.point = point;
////                    flipEvent.height = height;
////                    eventLog.AddEvent(flipEvent);
//                }
//            }
//
////            _lastSplits = new Dictionary<int, List<int>>(currentSplits);
////            currentSplits.Clear();
//            
//            OffsetShapeLog.AddLine("previous splits ", currentSplits.Count);
//
//            //split events
//            for(int n = 0; n < _data.liveNodeCount; n++)
//            {
//                Node node = _data.liveNodes[n];
//                int nodeID = node.id;
//
//                Node nodeA = null, nodeB = null;
//                for(int e = 0; e < liveEdgeCount; e++)
//                {
//                    Edge edge = _data.liveEdges[e];
//                    if(edge.Contains(node))
//                    {
//                        if(edge.nodeA == node) nodeB = edge.nodeB;
//                        if(edge.nodeB == node) nodeA = edge.nodeA;
//                    }
//                }
//
//                for(int e = 0; e < liveEdgeCount; e++)
//                {
//                    Edge edge = _data.liveEdges[e];
//                    if (edge.Contains(node)) continue;//nodes can't split their own edges - carry on!
//                    if(nodeA != null && edge.Contains(nodeA)) continue;//nodes can't split adjacent edges - carry on!
//                    if(nodeB != null && edge.Contains(nodeB)) continue;//nodes can't split adjacent edges - carry on!
//                    
//                    if(currentSplits.ContainsKey(nodeID))//previous splits should never intersect - ingore
//                    {
//                        if (currentSplits[nodeID].Contains(edge.nodeA.id))
//                            continue;
//                        if (currentSplits[nodeID].Contains(edge.nodeB.id))
//                            continue;
//                    }
//
//                    if(!isPartOfShape(node, edge)) continue;
//
//                    Vector2 nodeMovement = node.direction * node.distance - edgeMovements[e];//simulate collision by moving the point by the vectors of both the point and the edge,
//                    //note: collisions are simpler if only one body is moving so we're going to add the edge vector onto the point vector, making the edge remain stationary
//                    Vector2 calculationPoint = node.previousPosition + nodeMovement;//calculate the point vector by adding the edge one to it
//                    Vector2 edgePosA = edge.nodeA.previousPosition;
//                    Vector2 edgePosB = edge.nodeB.previousPosition;
//
//                    Vector2 intersectionPoint;
//                    if(Utils.Intersects(node.previousPosition, calculationPoint, edgePosA, edgePosB, out intersectionPoint))
//                    {
//                        OffsetShapeLog.AddLine("Split event detected");
//                        SplitEvent splitEvent = new SplitEvent();
//                        splitEvent.node = node;
//                        splitEvent.edge = edge;
//                        OffsetShapeLog.AddLine(edge.ToString());
//                        OffsetShapeLog.AddLine(node.id);
//
//                        OffsetShapeLog.DrawLine(node.previousPosition, calculationPoint, Color.red);
//                        OffsetShapeLog.DrawLine(edgePosA, edgePosB, Color.magenta);
//
//                        Vector2 a = node.previousPosition;
//                        Vector2 b = node.position;
//                        Vector2 x = intersectionPoint;//intersectionInfo.Point0;
//                                                    
//                        float movementMag = nodeMovement.magnitude;
//                        float intersectionMag = (x - a).magnitude;
//                        float percent = intersectionMag / movementMag;
//                        Vector2 actualIntersectionPoint = Vector2.Lerp(a, b, percent);//translate the point to the real movement point
//
//                        float newLengthA = (x - edge.positionA).magnitude;
//                        float newLengthB = (x - edge.positionB).magnitude;
//                        OffsetShapeLog.AddLine("line a length " , newLengthA);
//                        OffsetShapeLog.AddLine("line b length ", newLengthB);
//
//                        SplitEvent sEvent = eventLog.CreateSplitEvent(_data, node, edge, actualIntersectionPoint, percent, calculationPoint);
//                        if (newLengthA > pointAccuracy && newLengthB > pointAccuracy)
//                            eventLog.AddEvent(sEvent);
//                        else
//                        {
//                            Node[] nodes = null;
//                            if (newLengthA < pointAccuracy && newLengthB < pointAccuracy)
//                                nodes = new []{ node, edge.nodeA, edge.nodeB };
//                            else if(newLengthA < pointAccuracy)
//                                nodes = new[] { node, edge.nodeA };
//                            else if (newLengthB < pointAccuracy)
//                                nodes = new[] { node, edge.nodeB };
//                            if(nodes != null)
//                            {
//                                MergedEvent mEvent = eventLog.CreateMergeEvent(nodes, actualIntersectionPoint, percent, calculationPoint);
//                                mEvent.Merge(sEvent);
//                                eventLog.AddEvent(mEvent);
//                            }
//                        }
//
////                        OffsetShapeLog.AddLine("line a length " , (x - edge.positionA).magnitude);
////                        OffsetShapeLog.AddLine("line b length ", (x - edge.positionB).magnitude);
////
////                        float movementMag = nodeMovement.magnitude;
////                        float intersectionMag = (x - a).magnitude;
////                        float percent = intersectionMag / movementMag;
////                        Vector2 actualIntersectionPoint = Vector2.Lerp(a, b, percent);//translate the point to the real movement point
////                        splitEvent.point = actualIntersectionPoint;
////                        splitEvent.percent = percent;
////                        splitEvent.height = node.height;
////                        splitEvent.nodeMovementStart = node.previousPosition;//movementSegment.P0;
////                        splitEvent.nodeMovementEnd = calculationPoint;//movementSegment.P1;
////
////                        Edge[] edges = Utils.GetABEdge(data, node);
////                        if(edges[0] == null || edges[1] == null) continue;
////                        splitEvent.nodeEdgeA = edges[0];
////                        splitEvent.nodeEdgeB = edges[1];
////
////                        if (splitEvent.ContainsNode(splitEvent.nodeEdgeA.GetOtherNode(node)))
////                            OffsetShapeLog.AddLine("Split event collapses shape");
////                        if (splitEvent.ContainsNode(splitEvent.nodeEdgeB.GetOtherNode(node)))
////                            OffsetShapeLog.AddLine("Split event collapses shape");
////
////
////                        eventLog.AddEvent(splitEvent);
//                    }
//                }
//            }
//
//            currentSplits.Clear();
//            int eventCount = eventLog.count;
//            OffsetShapeLog.AddLine("event count: ",eventCount);
//            if(eventCount > 0)
//            {
//                float percent = eventLog.percent;
//                _data.unitHeight += -(1.0f - percent);
//                foreach(Node node in _data.liveNodes)
//                    node.MoveBack(percent);//move all nodes back to the position of the event
//                foreach(Edge edge in _data.liveEdges)
//                    edge.UpdateValues();//update all edges to reflect this
//                foreach(Node node in _data.liveNodes)
//                    if(_data.formingEdges.ContainsKey(node)) _data.formingEdges[node].UpdateValues();
//                for(int e = 0; e < eventCount; e++)
//                {
//                    IEvent sevent = eventLog[e];
//                    sevent.DrawDebug();
//                    OffsetShapeLog.AddLine(string.Format("Event {0} of type {4} at {1} percent and {2},{3}", e, sevent.percent, sevent.point.x, sevent.point.y, sevent.GetType()));
//                    switch(sevent.GetType().ToString())
//                    {
//                        case "JStraightSkeleton.FlipEvent":
//                            FlipEvent fEvent = (FlipEvent)sevent;
//                            CollapseEdge(fEvent.edge, fEvent.point, fEvent.height);
//                            break;
//
//                        case "JStraightSkeleton.SplitEvent":
//                            SplitEvent sEvent = (SplitEvent)sevent;
//                            SplitEdge(sEvent);
//                            break;
//
//                        case "JStraightSkeleton.MergedEvent":
//                            MergedEvent mEvent = (MergedEvent)sevent;
//                            MergeEvent(mEvent);
//                            break;
//                    }
//                }
//            }
//            else
//            {
//                foreach(Node node in _data.liveNodes)
//                    if(_data.formingEdges.ContainsKey(node)) _data.formingEdges[node].UpdateValues();
//            }
//
//            foreach(Node node in _data.liveNodes)
//                Utils.CalculateNodeDirAng(_data, node);
//
//        }
//
//
//        private void CollapseEdge(Edge edge, Vector2 toPoint, float height)
//        {
//            List<Node> nodes = new List<Node>();
//            nodes.Add(edge.nodeA);
//            nodes.Add(edge.nodeB);
//            CollapseNodes(nodes, toPoint, height);
//        }
//
//        private void CollapseNodes(List<Node> nodes, Vector2 toPoint, float height)
//        {
//            OffsetShapeLog.AddLine("Collapse Nodes:");
//            foreach(Node node in nodes)
//                OffsetShapeLog.Add(node.id+" ");
//            OffsetShapeLog.AddLine("to point: ",toPoint);
//            Node newStaticNode = new Node(toPoint, height);//new static node to mark point of collapse
//            _data.AddStaticNode(newStaticNode);//add static node to node array
//            OffsetShapeLog.AddLine("new static node added ",newStaticNode.id);
//
//            int liveEdgeCount = _data.liveEdges.Count;
//            List<Edge> liveEdges = new List<Edge>();
//            Node newLiveNode = new Node(toPoint, height);//new live node to continue shape forming
//
//            int nodeCount = nodes.Count;
//            float minHeight = nodes[0].height;
//            float maxHeight = nodes[0].height;
//            for(int n = 1; n < nodeCount; n++)
//            {
//                minHeight = Mathf.Min(minHeight, nodes[n].height);
//                maxHeight = Mathf.Min(maxHeight, nodes[n].height);
//            }
//            for(int n = 0; n < nodeCount; n++)
//            {
//                nodes[n].position = toPoint;
//                nodes[n].height = maxHeight;
//            }
//
//            for(int e = 0; e < liveEdgeCount; e++)
//            {
//                Edge edge = _data.liveEdges[e];
//                if(!nodes.Contains(edge.nodeA) || !nodes.Contains(edge.nodeB)) continue;
//                edge.UpdateValues();
//                if (edge.length < pointAccuracy)//when the edge reaches 0 length it has flipped and should be collapsed
//                {
//                    OffsetShapeLog.AddLine(edge.ToString(), "has collapsed", edge.length, pointAccuracy);
//                    _data.mesh.CollapseEdge(edge, newLiveNode, newStaticNode);
//                    OffsetShapeLog.AddLine("Remove edge ",edge.ToString());
//                    _data.liveEdges.Remove(edge);//remove collapsed edge - from length reaching zero
//                    liveEdgeCount--;
//                    e--;
//                    if (!nodes.Contains(edge.nodeA))
//                        nodes.Add(edge.nodeA);
//                    if (!nodes.Contains(edge.nodeB))
//                        nodes.Add(edge.nodeB);
//                }
//            }
//
//            OffsetShapeLog.AddLine("find live node edges");
//            for (int e = 0; e < liveEdgeCount; e++)
//            {
//                Edge edge = _data.liveEdges[e];
//                if (!edge.Contains(nodes)) continue;
////                if(edge.length < pointAccuracy) continue;
//                if(nodes.Contains(edge.nodeA) && nodes.Contains(edge.nodeB))
//                {
//                    OffsetShapeLog.AddLine("Remove collapsed edge ",edge.ToString());
//                    _data.liveEdges.Remove(edge);//remove collapsed edge - likely from parallel
//                    _data.mesh.EdgeComplete(edge);
//                    liveEdgeCount--;
//                    e--;
//                    continue;
//                }
//                if(nodes.Contains(edge.nodeA) || newLiveNode == edge.nodeA)
//                {
//                    OffsetShapeLog.AddLine("replace node a");
//                    edge.ReplaceNode(edge.nodeA, newLiveNode);//replace old live node reference to new one
//                    _data.mesh.ReplaceNode(edge.nodeA, newLiveNode);
//                    liveEdges.Add(edge);
//                    continue;
//                }
//                if(nodes.Contains(edge.nodeB) || newLiveNode == edge.nodeB)
//                {
//                    OffsetShapeLog.AddLine("replace node b");
//                    edge.ReplaceNode(edge.nodeB, newLiveNode);//replace old live node reference to new one
//                    _data.mesh.ReplaceNode(edge.nodeB, newLiveNode);
//                    liveEdges.Add(edge);
//                }
//            }
//
//            for (int n = 0; n < nodeCount; n++)
//            {
//                Node node = nodes[n];
//                Utils.RetireFormingEdge(_data, node, newStaticNode);
//                _data.liveNodes.Remove(node);
//            }
//
//            Utils.CheckParrallel(_data);
//
//            OffsetShapeLog.AddLine("Live edges: ",liveEdges.Count);
//            if(liveEdges.Count > 0)//deal with left live edges after the collapse
//            {
//                _data.AddLiveNode(newLiveNode);//new live node from collapse
//                Edge edgeA = null, edgeB = null;
//                liveEdgeCount = _data.liveEdges.Count;
//                for(int e = 0; e < liveEdgeCount; e++)//find the two edges left from the collapse
//                {
//                    Edge edge = _data.liveEdges[e];
//                    if(!_data.liveEdges.Contains(edge)) continue;
//                    if(edge.nodeA == newLiveNode) edgeA = edge;
//                    if(edge.nodeB == newLiveNode) edgeB = edge;
//                }
//
//                if(edgeA != null && edgeB != null)//if there is a live edge
//                {
//                    Node x = edgeA.GetOtherNode(newLiveNode);
//                    Node y = edgeB.GetOtherNode(newLiveNode);
//                    Utils.CalculateNodeDirAng(newLiveNode, x, y);//recalculate node angle
//                    Utils.NewFormingEdge(_data, newStaticNode, newLiveNode);//add new forming edge
//                }
//                else
//                {
//                    OffsetShapeLog.AddLine("New live node has not been calculted ",newLiveNode.id);
//                }
//            }
//
//            foreach(Node node in nodes)
//                _data.mesh.ReplaceNode(node, newStaticNode);
//        }
//
//        private void SplitEdge(SplitEvent e)
//        {
//            OffsetShapeLog.AddLine("Split event");
//            OffsetShapeLog.AddLine("by node ",e.node.id);
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
//            if(byEdgeA == null || byEdgeB == null)
//            {
//                //TODO work out what to really do here.
//                return;
//            }
//            Node byNodeA = byEdgeA.GetOtherNode(e.node);
//            Node byNodeB = byEdgeB.GetOtherNode(e.node);
//
//            OffsetShapeLog.AddLine("by node a" , byNodeA.id);
//            OffsetShapeLog.AddLine("by node b" , byNodeB.id);
//
//            if(byNodeA == null || byNodeB == null)
//                return;
//
//            //calculate new node directions
//            Utils.CalculateNodeDirAng(newLiveNodeA, byNodeA, nodeOldA);
//            Utils.CalculateNodeDirAng(newLiveNodeB, nodeOldB, byNodeB);
//
//            _data.AddLiveNode(newLiveNodeA);
//            _data.AddLiveNode(newLiveNodeB);
//            _data.AddStaticNode(nodeStatic);
//            _data.liveNodes.Remove(e.node);
//
//            //discard the old edge
//            OffsetShapeLog.AddLine("Discard old edge ",e.edge.ToString());
//            _data.liveEdges.Remove(e.edge);//
//            byEdgeA.ReplaceNode(e.node, newLiveNodeA);
//            byEdgeB.ReplaceNode(e.node, newLiveNodeB);
//            //create the two new edges from the split
//            Edge newEdgeA = new Edge(nodeOldA, newLiveNodeA);
//            _data.liveEdges.Add(newEdgeA);
//            e.newLiveEdgeA = newEdgeA;
//            Edge newEdgeB = new Edge(newLiveNodeB, nodeOldB);
//            _data.liveEdges.Add(newEdgeB);
//            e.newLiveEdgeB = newEdgeB;
//
//            //forming edges
//            Utils.RetireFormingEdge(_data, e.node, nodeStatic);
//            Edge formingEdgeA = Utils.NewFormingEdge(_data, nodeStatic, newLiveNodeA);
//            Edge formingEdgeB = Utils.NewFormingEdge(_data, nodeStatic, newLiveNodeB);
//
////            int aIndex = data.liveNodes.IndexOf(nodeLiveA);
////            int bIndex = data.liveNodes.IndexOf(nodeLiveB);
//
//            if(!currentSplits.ContainsKey(newLiveNodeA.id))
//                currentSplits.Add(newLiveNodeA.id, new List<int>());
//            currentSplits[newLiveNodeA.id].Add(newLiveNodeB.id);
//            if(!currentSplits.ContainsKey(newLiveNodeB.id))
//                currentSplits.Add(newLiveNodeB.id, new List<int>());
//            currentSplits[newLiveNodeB.id].Add(newLiveNodeA.id);
//
//            _data.mesh.SplitEdge(e);
//
//            OffsetShapeLog.AddLine("new live nodes");
//            OffsetShapeLog.AddLine(newLiveNodeA.id);
//            OffsetShapeLog.AddLine(newLiveNodeB.id);
//            OffsetShapeLog.AddLine("new edges - old edge - forming edge a");
//            OffsetShapeLog.AddLine(newEdgeA.ToString());
//            OffsetShapeLog.AddLine(byEdgeA.ToString());
//            OffsetShapeLog.AddLine(formingEdgeA.ToString());
//
//            OffsetShapeLog.AddLine("new edges - old edge - forming edge b");
//            OffsetShapeLog.AddLine(byEdgeB.ToString());
//            OffsetShapeLog.AddLine(newEdgeB.ToString());
//            OffsetShapeLog.AddLine(formingEdgeB.ToString());
//
//            Utils.CheckParrallel(_data);
//
//        }
//
//        private void MergeEvent(MergedEvent evnt)
//        {
//            CollapseNodes(evnt.mergeNodes, evnt.point, evnt.height);
//        }
//
//        private bool CheckBounds()
//        {
//            foreach(Node node in _data.liveNodes)
//            {
////                node.DebugDraw(Color.magenta);
//                Vector2 pos = node.position;
//                if(!_bounds.Contains(pos))
//                {
////                    Debug.LogError("Node boundary error! node id: "+node.id);
//                    return false;
//                }
//            }
//            return true;
//        }
//
//        public string errorMessage
//        {
//            get
//            {
//                StringBuilder sb = new StringBuilder();
//                sb.AppendLine("Straight Skeleton Error");
//
//                if(_data == null)
//                {
//                    sb.AppendLine("No shape has been set in the skeleton.");
//                    return sb.ToString();
//                }
//
//                sb.AppendLine("The algorithm failed to generate a correct straight skeleton.");
//                sb.AppendLine("Please report this to the developer email@jasperstocker.com");
//                sb.AppendLine("Send this entire error message");
//                sb.AppendLine(" ");
//                sb.AppendLine("private Vector2[] errorShape ={");
//                foreach (Vector2 point in _data.baseShape)
//                    sb.AppendLine(string.Format("new Vector2({0}, {1}),", point.x, point.y));
//                sb.AppendLine("};");
//                sb.AppendLine(" ");
//                sb.AppendLine("Error message end");
//                return sb.ToString();
//            }
//        }
//
//        private void NormaliseHeights()
//        {
//            int nodeCount = _data.staticNodeCount;
//            float max = 0;
//            for (int n = 0; n < nodeCount; n++)
//                if (_data.StaticNode(n).height > max) max = _data.StaticNode(n).height;
//            for (int n = 0; n < nodeCount; n++)
//                _data.StaticNode(n).height /= max;
//        }
//
//        private bool TestBaseShape(Vector2[] points)
//        {
//            int pointCount = points.Length;
//            for(int i = 0; i < pointCount; i++)
//            {
//                int ib = (i + 1) % pointCount;
//                for (int j = 0; j < pointCount; j++)
//                {
//                    if(i==j)continue;
//                    if(ib==j)continue;
//
//                    int jb = (j + 1) % pointCount;
//
//                    if (i == jb) continue;
//
//                    if(Utils.Intersects(points[i], points[ib], points[j], points[jb]))
//                        return false;
//
////                    Segment2 si = new Segment2(points[i], points[ib]);
////                    Segment2 sj = new Segment2(points[j], points[jb]);
////
////                    if(Intersection.TestSegment2Segment2(ref si, ref sj))
////                        return false;
//                }
//            }
//            return true;
//        }
//
//        private bool isPartOfShape2(Node subject, Edge edge)
//        {
//            List<Edge> shapeEdges = new List<Edge>();
//            List<Node> shapeNodes = new List<Node>();
//            List<Node> followNodes = new List<Node>();
//            followNodes.Add(edge.nodeA);
//            followNodes.Add(edge.nodeB);
//            shapeEdges.Add(edge);
//            int it = _data.liveEdges.Count* _data.liveEdges.Count;
//            while (followNodes.Count > 0)
//            {
//                Node node = followNodes[0];
//                if(node == subject) return true;
//                followNodes.RemoveAt(0);
//                shapeNodes.Add(node);
//                foreach (Edge liveEdge in _data.liveEdges)
//                {
//                    if(shapeEdges.Contains(liveEdge)) continue;
//
//                    if(liveEdge.Contains(node))
//                    {
//                        followNodes.Add(liveEdge.GetOtherNode(node));
//                        shapeEdges.Add(edge);
//                    }
//                }
//                it--;
//                if(it < 0)
//                    break;
//            }
//
//            return shapeNodes.Contains(subject);
//        }
//
//        private bool isPartOfShape(Node subject, Edge edge)
//        {
//            Node startNode = edge.nodeA;
//            Node thisNode = edge.nodeB;
//            Node lastNode = edge.nodeA;
//
//            if(edge.Contains(subject)) return true;
//
//            int liveEdgeCount = _data.liveEdges.Count;
//            int it = liveEdgeCount * liveEdgeCount;
//            while(thisNode != startNode)
//            {
//                for (int l = 0; l < liveEdgeCount; l++)
//                {
//                    Edge thisEdge = _data.liveEdges[l];
//                    if(thisEdge.Contains(thisNode) && !thisEdge.Contains(lastNode))
//                    {
//                        lastNode = thisNode;
//                        thisNode = thisEdge.GetOtherNode(lastNode);
//                        break;
//                    }
//                }
//                if(thisNode == subject) return true;
//
//                it--;
//                if(it < 0) break;
//            }
//            
//            return false;
//        }
//
//        public void DrawDebug()
//        {
//            foreach (Edge edge in _data.liveEdges)
//            {
//                OffsetShapeLog.DrawLine(Utils.ToV3(edge.nodeA.position), Utils.ToV3(edge.nodeB.position), new Color(0, 0, 1, 0.4f));
//                OffsetShapeLog.DrawLine(Utils.ToV3(edge.nodeA.previousPosition), Utils.ToV3(edge.nodeB.previousPosition), new Color(1, 0, 1, 0.25f));
//            }
//            foreach (Edge edge in _data.edges)
//                OffsetShapeLog.DrawLine(Utils.ToV3(edge.nodeA.position), Utils.ToV3(edge.nodeB.position), new Color(0, 1, 0, 0.5f));
//            foreach (Node node in _data.liveNodes)
//            {
//                node.DebugDrawDirection(Color.red);
//                if (_data.formingEdges.ContainsKey(node))
//                {
//                    Edge edge = _data.formingEdges[node];
//                    OffsetShapeLog.DrawLine(Utils.ToV3(edge.nodeA.position), Utils.ToV3(edge.nodeB.position), new Color(0, 1, 1, 0.4f));
//                }
//            }
//            //            data.mesh.DrawDebug();
//        }
//    }
//}