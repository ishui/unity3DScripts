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
    public class Edge
    {
        //cache these for calculations
        private Vector2 _positionA;
        private Vector2 _positionB;
        private Node _nodeA;
        private Node _nodeB;
        private float _angle;
        private Vector2 _direction = Vector2.zero;
        private Vector2 _uphill;
//        private Segment2 _segment;
        private Vector2 _center;
        private float _length;

        public Edge(Node a, Node b)
        {
            _nodeA = a;
            _nodeB = b;
//            _segment = new Segment2();
            UpdateValues();
        }

        public Vector2 positionA { get { return _positionA; } }
        public Vector2 positionB { get { return _positionB; } }
        public Node nodeA { get { return _nodeA; } }
        public Node nodeB { get { return _nodeB; } }
        public float angle { get { return _angle; } }
        public Vector2 direction { get { return _direction; } }
        public Vector2 uphill { get { return _uphill; } }
        public Vector2 center { get { return _center; } }
        public float length { get { return _length; } }

        public float sqrMagnitude { get { return (_positionB - _positionA).sqrMagnitude; } }

//        public Segment2 segment { get {return _segment;} }

        public bool startEdge { get {return _nodeA.startNode && _nodeB.startNode;} }

//        public Segment2 GetBaseSegment
//        {
//            get { return new Segment2(_nodeA.previousPosition, _nodeB.previousPosition);}
//        }

        //Co-linear
        public bool SameDirectedLine(Edge nextL)
        {
            return Mathf.Abs(nextL.angle - angle) < 0.01;
        }

        public void UpdateValues()
        {
            _positionA = _nodeA.position;
            _positionB = _nodeB.position;
//            _segment.SetEndpoints(_positionA, _positionB);
            _direction = (_positionB - _positionA).normalized;
            float angleSign = Mathf.Sign(Vector2.Dot(Vector2.right, _direction));
            _angle = Vector2.Angle(Vector2.up, _direction) * angleSign;
            _uphill = Utils.Rotate(_direction, -90);

            _center = Vector2.Lerp(_positionA, _positionB, 0.5f);

            _length = Vector2.Distance(_positionA, _positionB);
        }

        public Edge Clone()
        {
            return new Edge(_nodeA, nodeB);
        }

        public bool Contains(Node node)
        {
            if (node == _nodeA) return true;
            if (node == _nodeB) return true;
            return false;
        }

        public bool Contains(Node[] nodes)
        {
            foreach (Node node in nodes)
                if (Contains(node)) return true;
            return false;
        }

        public bool Contains(List<Node> nodes)
        {
            foreach (Node node in nodes)
                if (Contains(node)) return true;
            return false;
        }

        public Node GetOtherNode(Node node)
        {
            if (node == _nodeA) return _nodeB;
            if (node == _nodeB) return _nodeA;
            return null;
        }

        public Node GetEdgeNode(List<Node> nodes)
        {
            if (nodes.Contains(_nodeA)) return _nodeA;
            if (nodes.Contains(_nodeB)) return _nodeB;
            return null;
        }

        public Node GetOtherEdgeNode(List<Node> nodes)
        {
            if (nodes.Contains(_nodeA)) return _nodeB;
            if (nodes.Contains(_nodeB)) return _nodeA;
            return null;
        }

        public Node RelatedNode(Edge other)
        {
            if (other.Contains(_nodeA)) return nodeA;
            if (other.Contains(_nodeB)) return nodeB;
            return null;
        }

        public void ReplaceNode(Node oldNode, Node newNode)
        {
            OffsetShapeLog.AddLine("ReplaceNode: ", oldNode.id, newNode.id);
            if (oldNode == newNode) return;
            if(_nodeA == oldNode) _nodeA = newNode;
            if(_nodeB == oldNode) _nodeB = newNode;
            UpdateValues();
        }

        public float Percent(Vector2 point)
        {
            float distA = Vector2.Distance(_positionA, point);
            if(distA == 0) return 0;
            float distB = Vector2.Distance(_positionA, _positionB);
            if(distB == 0) return 1;
            return distA / distB;
        }

        public void DebugDraw(Color col)
        {
//            Debug.DrawLine(JMath.ToV3(positionA), JMath.ToV3(positionB), col);
        }

        public void DebugDrawLineNodes(Color col)
        {
//            Debug.DrawLine(JMath.ToV3(positionA), JMath.ToV3(positionB), col);
//            Vector3 p0 = JMath.ToV3(nodeA.position);
//            Vector3 p1 = JMath.ToV3(nodeB.position);
//            Vector3 v0 = p0 + Vector3.up * nodeA.height * 20;
//            Vector3 v1 = p1 + Vector3.up * nodeB.height * 20;
//            GizmoLabel.Label("Node" + nodeA.id, v0);
//            GizmoLabel.Label("Node" + nodeB.id, v1);
        }

        public void DebugDrawPrevious(Color col)
        {
//            Debug.DrawLine(JMath.ToV3(_nodeA.previousPosition), JMath.ToV3(_nodeB.previousPosition), col);
        }

        public void DebugDrawHeight(Color col)
        {
//            Vector3 heightA = new Vector3(_nodeA.position.x, _nodeA.height, _nodeA.position.y);
//            Vector3 heightB = new Vector3(_nodeB.position.x, _nodeB.height, _nodeB.position.y);
//            Debug.DrawLine(heightA, heightB, col);
        }

        public void DebugDrawFormingEdge(Color col)
        {
//            Debug.DrawLine(JMath.ToV3(positionA), JMath.ToV3(positionB), col);
//            Vector3 heightA = new Vector3(_nodeA.position.x, _nodeA.height, _nodeA.position.y);
//            Vector3 heightB = new Vector3(_nodeB.position.x, _nodeB.height, _nodeB.position.y);
//            Debug.DrawLine(heightA, heightB, col);
//            Vector3 centre = Vector3.Lerp(heightA, heightB, 0.5f);
//            GizmoLabel.Label("Forming Edge" + nodeB.id, centre);
        }

        public override string ToString()
        {
            return string.Format("edge {0} {1}", nodeA.id, nodeB.id);
        }
    }
}