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
    public interface IEvent
    {
        float percent { get; set; }
        Vector2 point { get; set; }
        float height { get; set; }
        List<Node> affectedNodes { get; }
        bool ContainsNode(Node node);
        bool ContainsNodes(List<Node> nodes);
        void ReplaceNode(Node node, Node withNode);

        void DrawDebug();
        void Log();
        string ToString();
    }

    public abstract class BaseEvent : IEvent
    {
        private float _percent = 1;
        public float realPercent = 1;
        public float percent
        {
            get { return _percent; }
            set { _percent = value; }
        }

        private Vector2 _point;
        public Vector2 point
        {
            get { return _point; }
            set { _point = value; }
        }

        private float _height;

        public float height
        {
            get { return _height; }
            set { _height = value; }
        }

        private List<Node> _affectedNodes = new List<Node>();

        public List<Node> affectedNodes
        {
            get { return _affectedNodes; }
        }

        public bool ContainsNode(Node node)
        {
            return _affectedNodes.Contains(node);
        }

        public bool ContainsNodes(List<Node> nodes)
        {
            foreach (Node othernode in nodes)
                if (_affectedNodes.Contains(othernode)) return true;
            return false;
        }

        public void ReplaceNode(Node node, Node withNode)
        {
            if (_affectedNodes.Contains(node))
            {
                int index = _affectedNodes.IndexOf(node);
                _affectedNodes.Insert(index, withNode);
                _affectedNodes.Remove(node);
            }
        }

        public Node newStaticNode;

        //override
        public virtual void DrawDebug()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Log()
        {
            //            foreach(Node node in _affectedNodes)
            //                OffsetShapeLog.Add(node.id);
        }
    }

    public class FlipEvent : BaseEvent
    {
        private Edge _edge;
        public Node newLiveNode;

        public Edge edge
        {
            get { return _edge; }
            set
            {
                _edge = value;
                affectedNodes.Add(_edge.nodeA);
                affectedNodes.Add(_edge.nodeB);
            }
        }

        public override void DrawDebug()
        {
//            GizmoLabel.LabelLineTo("Flip Event", edge.positionA + Vector2.up * 10, edge.positionA, Color.white, 4);
        }

        public override void Log()
        {
            //            OffsetShapeLog.AddLine("Flip event");
            //            OffsetShapeLog.DrawLabel(Utils.ToV3(point), "Flip event");
            //            OffsetShapeLog.DrawLine(edge.nodeA.position, edge.nodeA.previousPosition, Color.red);
            //            OffsetShapeLog.DrawLine(edge.nodeB.position, edge.nodeB.previousPosition, Color.red);
        }

        public override string ToString()
        {
            return string.Format("Flip event edge {0} is collapsing at percentage {1}", _edge, percent);
        }
    }

    public class SplitEvent : BaseEvent
    {
        private Edge _edge;

        public Edge edge
        {
            get { return _edge; }
            set
            {
                _edge = value;
                affectedNodes.Add(_edge.nodeA);
                affectedNodes.Add(_edge.nodeB);
            }
        }

        private Node _node;

        public Node node
        {
            get { return _node; }
            set
            {
                _node = value;
                affectedNodes.Add(_node);
            }
        }

        public Vector2 nodeMovementStart;
        public Vector2 nodeMovementEnd;

        private Edge _nodeEdgeA;
        public Edge nodeEdgeA
        {
            get { return _nodeEdgeA; }
            set
            {
                _nodeEdgeA = value;
                affectedNodes.Add(_nodeEdgeA.nodeA);
                affectedNodes.Add(_nodeEdgeA.nodeB);
            }
        }

        private Edge _nodeEdgeB;
        public Edge nodeEdgeB
        {
            get { return _nodeEdgeB; }
            set
            {
                _nodeEdgeB = value;
                affectedNodes.Add(_nodeEdgeB.nodeA);
                affectedNodes.Add(_nodeEdgeB.nodeB);
            }
        }

        public Node newLiveNodeA;
        public Node newLiveNodeB;
        public Edge newLiveEdgeA;
        public Edge newLiveEdgeB;

        public override void DrawDebug()
        {
            //            OffsetShapeLog.DrawLabel(point, "Split Event");
            //            OffsetShapeLog.DrawEdge(edge, Color.red);
            //            OffsetShapeLog.LabelNode(node);
            //            OffsetShapeLog.DrawLine(node.position, node.previousPosition, Color.yellow);
        }

        public override void Log()
        {
            //            OffsetShapeLog.AddLine("Split event " + edge.ToString());
            //            OffsetShapeLog.DrawLabel(point, "Split Event");
            //            base.Log();
        }

        public override string ToString()
        {
            return string.Format("Split event node {0} is splitting edge {1} at percentage {2}", _node, _edge, percent);
        }
    }

    public class MergedEvent : BaseEvent
    {
        public List<Node> mergeNodes = new List<Node>();
        //        public List<Edge> mergeEdges = new List<Edge>();
        public List<SplitEvent> splitEvents = new List<SplitEvent>();
        public int eventCount;

        public void Merge(IEvent e)
        {
            OffsetShapeLog.AddLine("Merge Event type " + e.GetType());
            if (e.GetType() == typeof(FlipEvent)) Merge((FlipEvent)e);
            if (e.GetType() == typeof(SplitEvent)) Merge((SplitEvent)e);
            if (e.GetType() == typeof(MergedEvent)) Merge((MergedEvent)e);
            affectedNodes.AddRange(e.affectedNodes);
            if (eventCount > 0)
            {
                height = Mathf.Min(height, e.height);
            }
            else
            {
                height = e.height;
            }
            percent = Mathf.Min(percent, e.percent);
            eventCount++;
        }

        private void Merge(FlipEvent e)
        {
            point = e.point;
            if (!mergeNodes.Contains(e.edge.nodeA))
                mergeNodes.Add(e.edge.nodeA);
            if (!mergeNodes.Contains(e.edge.nodeB))
                mergeNodes.Add(e.edge.nodeB);
            percent = Mathf.Min(percent, e.percent);
        }

        private void Merge(SplitEvent e)
        {
//            bool proximity = Vector2.Distance(e.point, point) < EventLog._pointAccuracy;
            OffsetShapeLog.AddLine("merging split event", e);

//            if (proximity)
//            {
//                splitEvents.Add(e);
//                int splitCount = splitEvents.Count;
//                for(int s = 0; s < splitCount; s++)
//                {
//                    if(Vector2.Distance(splitEvents[s].point, point) < EventLog._pointAccuracy)
//                    {
//                        if (!mergeNodes.Contains(splitEvents[s].node))
//                            mergeNodes.Add(splitEvents[s].node);
////                        if (!mergeNodes.Contains(splitEvents[s].edge.nodeA))
////                            mergeNodes.Add(splitEvents[s].edge.nodeA);
////                        if (!mergeNodes.Contains(splitEvents[s].edge.nodeB))
////                            mergeNodes.Add(splitEvents[s].edge.nodeB);
//                    }
//                }
//                point = e.point;
//                percent = Mathf.Min(percent, e.percent);
//            }
//            else
//            {
                int splitCount = splitEvents.Count;
                bool addSplit = true;
                for (int s = 0; s < splitCount; s++)
                {
                    SplitEvent os = splitEvents[s];
                    OffsetShapeLog.AddLine("checking against split event node", os.node);
                    if (os.node == e.node)
                    {
                        addSplit = false;
                        OffsetShapeLog.AddLine("new merged split conflicts with internal splits - disolving into collapsing node");
                        splitEvents.RemoveAt(s);

                        if (!mergeNodes.Contains(e.node))
                            mergeNodes.Add(e.node);
                        if (!mergeNodes.Contains(os.node))
                            mergeNodes.Add(os.node);

                        if (e.edge.nodeA == os.edge.nodeA || e.edge.nodeA == os.edge.nodeB && !mergeNodes.Contains(e.edge.nodeA))
                            mergeNodes.Add(e.edge.nodeA);

                        if (e.edge.nodeB == os.edge.nodeA || e.edge.nodeB == os.edge.nodeB && !mergeNodes.Contains(e.edge.nodeB))
                            mergeNodes.Add(e.edge.nodeB);

                        point = e.point;
                        percent = Mathf.Min(percent, e.percent);
                        s--;
                        splitCount--;
                    }
                }

                if (!addSplit) return;
                point = e.point;
                splitEvents.Add(e);
                percent = Mathf.Min(percent, e.percent);
//            }
        }

        private void Merge(MergedEvent e)
        {
            point = e.point;
            int nodeCount = e.mergeNodes.Count;
            for (int n = 0; n < nodeCount; n++)
                if (!mergeNodes.Contains(e.mergeNodes[n])) mergeNodes.Add(e.mergeNodes[n]);

            int splitCount = e.splitEvents.Count;
            for (int s = 0; s < splitCount; s++)
                if (!splitEvents.Contains(e.splitEvents[s])) splitEvents.Add(e.splitEvents[s]);

            //            int edgeCount = e.mergeEdges.Count;
            //            for (int s = 0; s < edgeCount; s++)
            //                if (!mergeEdges.Contains(e.mergeEdges[s])) mergeEdges.Add(e.mergeEdges[s]);
            percent = Mathf.Min(percent, e.percent);
        }

        public override void DrawDebug()
        {
            //            GizmoLabel.Label("Merge Event", JMath.ToV3(point), 0.1f);
        }

        public override void Log()
        {
            OffsetShapeLog.AddLine("Merge event ");
            base.Log();
        }

        public override string ToString()
        {
            string output = string.Format("Merge event containing {0} nodes, {1} split events occuring at {2}% \n", mergeNodes.Count, splitEvents.Count, percent);

            output += "Merging nodes: ";
            foreach (Node node in mergeNodes)
                output += node.id + ", ";
            output += "\n";

            foreach (SplitEvent splitEvent in splitEvents)
                output += splitEvent.ToString() + "\n";
            return output;
        }
    }

    public class EventLog
    {
        private List<IEvent> _events = new List<IEvent>();
        private List<IEvent> _allEvents = new List<IEvent>();
        private List<IEvent> _discardedEvents = new List<IEvent>();
        private List<Vector2> _allPoints = new List<Vector2>();
        private float _percent = 1;//default to one for comparison

        public static float percentAccuracy = 0.02f;
        public static float _pointAccuracy = 0.2f;
        public static float _sqrPointAccuracy = 0;

        public IEvent this[int index]
        {
            get { return _events[index]; }
        }

        public int count { get { return _events.Count; } }

        public float percent { get { return _percent; } }

        public static float pointAccuracy
        {
            get { return _pointAccuracy; }
            set
            {
                _pointAccuracy = value;
                _sqrPointAccuracy = _pointAccuracy * _pointAccuracy;
            }
        }

        public EventLog()
        {
            _sqrPointAccuracy = _pointAccuracy * _pointAccuracy;
            //            Debug.Log(sqrPointAccuracy);
        }

        public FlipEvent CreateFlipEvent(Edge edge, Vector2 atPoint)
        {
            Node nodeA = edge.nodeA;
            Node nodeB = edge.nodeB;
            OffsetShapeLog.AddLine("Flip event detected");
            //            Debug.Log("Flip event detected");
            FlipEvent flipEvent = new FlipEvent();
            flipEvent.edge = edge;
            OffsetShapeLog.AddLine(edge.ToString());
            Vector2 point = atPoint;//intersectionInfo.Point0;
            float pointADistance = Vector2.Distance(point, nodeA.previousPosition);
            float pointBDistance = Vector2.Distance(point, nodeB.previousPosition);
            //            float percentA = pointADistance / nodeA.distance;
            //            float percentB = pointBDistance / nodeB.distance;
            float percentA = pointADistance / Mathf.Abs(nodeA.distance);
            float percentB = pointBDistance / Mathf.Abs(nodeB.distance);
            float height = (nodeA.height + nodeB.height) * 0.5f;
            flipEvent.percent = Mathf.Min(percentA, percentB);
            flipEvent.point = point;
            flipEvent.height = height;
            //            AddEvent(flipEvent);
            return flipEvent;
        }

        public SplitEvent CreateSplitEvent(Shape data, Node node, Edge edge, Vector2 point, float eventPercent, Vector2 calculationPoint)
        {

            OffsetShapeLog.AddLine("Split event detected");
            SplitEvent splitEvent = new SplitEvent();
            splitEvent.node = node;
            splitEvent.edge = edge;
            OffsetShapeLog.AddLine(eventPercent.ToString("P"));
            OffsetShapeLog.AddLine("node " + node.id);
            OffsetShapeLog.AddLine("splits edge " + edge);

            Vector2 a = node.previousPosition;
            Vector2 b = node.position;
//            Vector2 x = point;//intersectionInfo.Point0;

            //            float movementMag = nodeMovement.magnitude;
            //            float intersectionMag = (x - a).magnitude;
            //            float eventPercent = intersectionMag / movementMag;
            Vector2 actualIntersectionPoint = Vector2.Lerp(a, b, eventPercent);//translate the point to the real movement point
            splitEvent.point = actualIntersectionPoint;
            splitEvent.percent = eventPercent;
            splitEvent.height = node.height;
            splitEvent.nodeMovementStart = node.previousPosition;//movementSegment.P0;
            splitEvent.nodeMovementEnd = node.position;//calculationPoint;//movementSegment.P1;

            Edge[] edges = Utils.GetABEdge(data, node);
            if (edges[0] == null || edges[1] == null)
            {
                return null;
            }
            splitEvent.nodeEdgeA = edges[0];
            splitEvent.nodeEdgeB = edges[1];

            if (splitEvent.ContainsNode(splitEvent.nodeEdgeA.GetOtherNode(node)))
                OffsetShapeLog.AddLine("Split event collapses shape");
            if (splitEvent.ContainsNode(splitEvent.nodeEdgeB.GetOtherNode(node)))
                OffsetShapeLog.AddLine("Split event collapses shape");

            //            AddEvent(splitEvent);
            return splitEvent;
        }

        public MergedEvent CreateMergeEvent(Node[] nodes, Vector2 atPoint, float eventPercent, Vector2 calculationPoint)
        {
            OffsetShapeLog.AddLine("Merge event detected");
            OffsetShapeLog.Add(nodes);
            MergedEvent mergeEvent = new MergedEvent();
            mergeEvent.mergeNodes.AddRange(nodes);
            mergeEvent.point = atPoint;
            float height = 0;
            //            float eventPercent = 1;
            foreach (Node node in nodes)
            {
                height += node.height;

                //                float pointDistance = Vector2.Distance(atPoint, node.previousPosition);
                //                float pointPercent = pointDistance / node.distance;
                //                eventPercent = Mathf.Min(eventPercent, pointPercent);
            }
            mergeEvent.height = height / nodes.Length;
            mergeEvent.percent = eventPercent;
            //            AddEvent(mergeEvent);
            return mergeEvent;
        }

        public void AddEvent(IEvent newEvent)
        {
            OffsetShapeLog.AddLine("AddEvent", newEvent);
            if (newEvent == null) return;
//            OffsetShapeLog.AddLine(newEvent.percent);
//            OffsetShapeLog.AddLine(newEvent.percent > 1.0f - Mathf.Epsilon);
//            if (newEvent.percent > 0.99999f) return; //percent accuracy...
            OffsetShapeLog.AddLine("AddEvent");
            OffsetShapeLog.AddLine("current number of events: " + _events.Count);
            bool lowerPercent = _percent > newEvent.percent;
            bool closePercent = Mathf.Abs(_percent - newEvent.percent) < percentAccuracy;
            OffsetShapeLog.AddLine(lowerPercent + " " + closePercent + " " + _percent + " " + newEvent.percent);

            if (!lowerPercent && !closePercent)//if this event is later than the currently logged events, ignore
            {
                _discardedEvents.Add(newEvent);
                _allEvents.Add(newEvent);
                _allPoints.Add(newEvent.point);
                return;
            }
            if (lowerPercent && !closePercent)//if this event is sooner then clear the list and log these earlier events
            {
                _discardedEvents.AddRange(_events);
                _events.Clear();
            }

            Vector2 point = newEvent.point;
            IEvent pEvent = null;
            if (closePercent)
                pEvent = CheckPoints(newEvent);
            if (pEvent != null)
            {
                OffsetShapeLog.AddLine("Merge existing event");
                pEvent.Log();
                OffsetShapeLog.AddLine("Merge existing event");
                OffsetShapeLog.AddLine(pEvent);
                OffsetShapeLog.AddLine("With new event");
                OffsetShapeLog.AddLine(newEvent);
                OffsetShapeLog.AddLine("Merged Outcome");
                IEvent mergedEvent = MergeEvent(pEvent, newEvent);
                OffsetShapeLog.AddLine(mergedEvent);
                OffsetShapeLog.AddLine("percent set ", _percent, mergedEvent.percent);
                _percent = mergedEvent.percent;
                _allPoints.Add(point);
                _allEvents.Add(mergedEvent);

                int discardedListCount = _discardedEvents.Count;
                for (int a = 0; a < discardedListCount; a++)
                {
                    IEvent e = _discardedEvents[a];
                    if (mergedEvent.ContainsNodes(e.affectedNodes) && !_events.Contains(e))
                    {
                        float sqrDist = Vector2.SqrMagnitude(e.point - mergedEvent.point);
                        if(sqrDist < _sqrPointAccuracy)
                        {
                            _events.Add(e);
                            mergedEvent.affectedNodes.AddRange(e.affectedNodes);
                            a = 0;//restart to pick up other straglers
                        }
                    }
                }
            }
            else
            {
                OffsetShapeLog.AddLine("Add new event");
                newEvent.Log();
                _events.Add(newEvent);
                _allPoints.Add(point);
                _allEvents.Add(newEvent);
                OffsetShapeLog.AddLine("percent set ", _percent, newEvent.percent);
                _percent = Mathf.Min(_percent, newEvent.percent);
            }
        }

        public void Clear()
        {
            _events.Clear();
            _allPoints.Clear();
            _allEvents.Clear();
            _discardedEvents.Clear();
            _percent = 1;
        }

        private IEvent CheckPoints(IEvent evnt)
        {
            Vector2 point = evnt.point;
            int pointCount = _allPoints.Count;
            if (pointCount == 0)
                return null;

            //find if this event has nodes that are affected by another event and elect such an event for merging
            int eventCount = _events.Count;
            for (int e = 0; e < eventCount; e++)
            {
                IEvent aevnt = _events[e];
                if (aevnt == evnt) continue;
                if (aevnt.ContainsNodes(evnt.affectedNodes))
                {
                    float percentDiff = Mathf.Abs(evnt.percent - aevnt.percent);
                    if(percentDiff > percentAccuracy)
                        continue;
                    float sqrMag = Vector2.SqrMagnitude(evnt.point - aevnt.point);
                    OffsetShapeLog.AddLine("SQR MAG ", sqrMag, _sqrPointAccuracy);
                    if (sqrMag < _sqrPointAccuracy)
                        return aevnt;
                }
            }

            //find any events that are close to this one physically.
            //this could be any logged event
            //as the subject event exists in the main _event list
            float lowestMag = Mathf.Infinity;
            int lowestMagIndex = -1;
            for (int p = 0; p < pointCount; p++)
            {
                float sqrMag = Vector2.SqrMagnitude(point - _allPoints[p]);
                if (sqrMag < lowestMag)
                {
                    lowestMag = sqrMag;
                    lowestMagIndex = p;
                }
            }
            if(lowestMagIndex != -1 && lowestMag < _sqrPointAccuracy)
            {
                float percentDiff = Mathf.Abs(evnt.percent - _allEvents[lowestMagIndex].percent);
                if (percentDiff <= percentAccuracy)
                    return _allEvents[lowestMagIndex];
            }

            return null;
        }

        /// <summary>
        /// Merge the two specified events
        /// </summary>
        /// <param name="eventA"></param>
        /// <param name="eventB"></param>
        /// <returns></returns>
        private MergedEvent MergeEvent(IEvent eventA, IEvent eventB)
        {
            OffsetShapeLog.AddLine(_events.Count);
            eventA.Log();
            eventB.Log();
            if (eventA.GetType() == typeof(MergedEvent))
            {
                if (eventA == eventB) return (MergedEvent)eventA;
                MergedEvent mergeEvent = (MergedEvent)eventA;
                mergeEvent.Merge(eventB);
                _events.Remove(eventB);
                if (!_events.Contains(eventA))
                    _events.Add(eventA);
                OffsetShapeLog.AddLine("Event B merged into A");
                return mergeEvent;
            }
            if (eventB.GetType() == typeof(MergedEvent))
            {
                if (eventA == eventB) return (MergedEvent)eventB;
                MergedEvent mergeEvent = (MergedEvent)eventB;
                mergeEvent.Merge(eventA);
                _events.Remove(eventA);
                if (!_events.Contains(eventB))
                    _events.Add(eventB);
                OffsetShapeLog.AddLine("Event A merged into B");
                return mergeEvent;
            }

            OffsetShapeLog.AddLine(_events.Count);
            //create the merge event and merge into it the two events
            MergedEvent mergedEvent = new MergedEvent();
            OffsetShapeLog.Add("merge percent ", mergedEvent.percent);
            mergedEvent.Merge(eventA);
            OffsetShapeLog.Add("merge percent ", mergedEvent.percent);
            mergedEvent.Merge(eventB);
            OffsetShapeLog.Add("merge percent ", mergedEvent.percent);
            eventA.Log();
            eventB.Log();
            mergedEvent.Log();
            _events.Remove(eventA);
            _events.Remove(eventB);
            if (_allEvents.Contains(eventA))
                _allPoints.RemoveAt(_allEvents.IndexOf(eventA));
            _allEvents.Remove(eventA);
            if (_allEvents.Contains(eventB))
                _allPoints.RemoveAt(_allEvents.IndexOf(eventB));
            _allEvents.Remove(eventB);

            OffsetShapeLog.AddLine("Current Events");
            foreach (IEvent evt in _events)
                evt.Log();

            OffsetShapeLog.AddLine(_events.Count);
            if (!_events.Contains(mergedEvent))
                _events.Add(mergedEvent);
            OffsetShapeLog.AddLine(_events.Count);
            return mergedEvent;
        }

        public void ReplaceNode(SkeletonData data, Node[] nodes, Node withNode)
        {
            foreach (Node node in nodes)
            {
                foreach (IEvent evnt in _allEvents)
                    evnt.ReplaceNode(node, withNode);
            }
        }
    }
}
