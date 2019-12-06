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

namespace BuildR2.ShapeOffset
{
    public class Node
    {
        private Vector2 _position;
        private Vector2 _previousPosition;
        private Vector2 _movement;
        private Vector2 _direction;
        private Edge _attachEdge;
        private float _angle;
        private float _distance;
        private float _totalDistance;
        public int id = 0;
        private float _height;
        private float _previousHeight;
        public bool startNode;
//        public Vector2 startTangent;
        public bool earlyTemination;

        public Vector2 position
        {
            get { return _position; }
            set
            {
                _previousPosition = _position;
                _movement = value - _position;
                _position = value;
            }
        }

        public Vector2 previousPosition
        {
            get { return _previousPosition; }
        }

        public Vector2 movement
        {
            get { return _movement; }
        }

        public float distance
        {
            get { return _distance; }
        }

        public float totalDistance
        {
            get { return _totalDistance; }
        }

        public Vector2 direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public float angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public Edge attachEdge
        {
            set { _attachEdge = value; }
            get { return _attachEdge; }
        }

        public float height {
            get
            {
                return _height;
            }
            set
            {
                _previousHeight = _height;
                _height = value;
            }
        }

        public Node(Vector2 pos, float height)
        {
            _position = pos;
            _previousPosition = pos;
            _height = height;
            _previousHeight = height;
        }

        public void MoveForward(float amount, float height)
        {
            if (_direction == Vector2.zero)
            {
                Debug.LogError("Direction not calculated " + id);
                return;
            }
            _distance = amount;
            _totalDistance += amount;
            position += _direction * _distance;
            this.height += height;
        }

        public void MoveBack(float percent)
        {
            _position = Vector2.Lerp(_previousPosition, _position, Mathf.Clamp01(percent));
            _height = Mathf.Lerp(_previousHeight, _height, Mathf.Clamp01(percent)); //-(1.0f - percent);
        }

        public void DebugDrawDirection(Color col)
        {
            OffsetShapeLog.DrawDirection(Utils.ToV3(position), Utils.ToV3(direction), "Node" + id, col);
        }

        public void DebugDrawMovement(Color col)
        {
            OffsetShapeLog.DrawLine(previousPosition, position, col);
            OffsetShapeLog.DrawLabel(position, "Node " + id);
        }

        public override string ToString()
        {
            return string.Format("node {0}", id);
        }
    }
}