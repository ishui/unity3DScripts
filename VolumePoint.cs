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

using System;
using UnityEngine;

namespace BuildR2
{
    [Serializable]
    public class VolumePoint
    {
        public enum CurveStyles
        {
            WallSection,
            Vertex,
            Distance
        }

        [SerializeField]
        private Vector2Int _position;
        [SerializeField]
        private Vector2Int _controlA;
        [SerializeField]
        private Vector2Int _controlB;
        [SerializeField]
        private CurveStyles _curveStyle = CurveStyles.WallSection;
        private bool _modified;
        private bool _moved;
        [SerializeField]
        private bool _illegal;
        [SerializeField]
        private bool _render;
        [SerializeField]
        private Facade _facade;

        //GABLES
        [SerializeField]
        private bool _isGabled;
        [SerializeField]
        private Gable _gableStyle;
        [SerializeField]
        private bool _simpleGable;
        [SerializeField]
        private float _gableHeight;//additional
        [SerializeField]
        private float _gableThickness = 0.3f;

        private Vector2Int _lastPosition;

        public Vector2Int position
        {
            get { return _position; }
            set
            {
                Debug.Log("VolumePoint.cs position value.x=" + value.x + " value.y=" + value.y);
                if (_position != value)
                {
                    _lastPosition = _position;
                    _position = value;
                    _modified = true;
                    _moved = true;
                }
            }
        }

        public Vector2Int lastPosition
        {
            get { return _lastPosition; }
        }

        public Vector2Int controlA
        {
            get { return _controlA; }
            set
            {
                Debug.Log("VolumePoint.cs controlA value.x=" + value.x + " value.y=" + value.y);

                if (_controlA != value)
                {
                    _controlA = value;
                    _modified = true;
                }
            }
        }

        public Vector2Int controlB
        {
            get { return _controlB; }
            set
            {
                Debug.Log("VolumePoint.cs controlB value.x=" + value.x + " value.y=" + value.y);

                if (_controlB != value)
                {
                    _controlB = value;
                    _modified = true;
                }
            }
        }

        public CurveStyles curveStyle
        {
            get { return _curveStyle; }
            set
            {
                if (_curveStyle != value)
                {
                    _curveStyle = value;
                    _modified = true;
                }
            }
        }

        public bool modified
        {
            get { return _modified; }
        }

        public bool moved
        {
            get { return _moved; }
        }

        public void MarkUnmodified()
        {
            _modified = false;
            _moved = false;
        }

        public void ClearMovement()
        {
            _moved = false;
        }

        public void MoveBack()
        {
            _position = _lastPosition;
            _modified = false;
            _moved = false;
        }

        public bool illegal
        {
            get { return _illegal; }
            set { _illegal = value; }
        }

        public bool render
        {
            get { return _render; }
            set { _render = value; }
        }

        public Facade facade
        {
            get {return _facade;}
            set
            {
                if (_facade != value)
                {
                    _facade = value;
                    _modified = true;
                    HUtils.log();
                    Debug.Log("VolumePoint.cs facade facade.baseHeight=" + facade.baseHeight + " facade.baseWidth=" + facade.baseWidth);
                }
            }
        }

        public VolumePoint(Vector2Int pos)
        {
            Debug.Log("VolumePoint.cs VolumePoint pos.x=" + pos.x + " pos.y=" + pos.y);
            
            _position = pos;
            _controlA = Vector2Int.zero;
            _controlB = Vector2Int.zero;
            _modified = false;
            _illegal = false;
            _render = true;
            _facade = null;
        }

        public VolumePoint Clone()
        {
            VolumePoint output = new VolumePoint(_position);
            output._controlA = _controlA;
            output._controlB = _controlB;
            output._curveStyle = _curveStyle;
            output._render = _render;
            output._facade = _facade;
            return output;
        }

        public bool IsWallStraight()
        {
            return _controlA == Vector2Int.zero && _controlB == Vector2Int.zero;
        }

        public bool isGabled
        {
            get {return _isGabled;}
            set
            {
                if(_isGabled != value)
                {
                    _isGabled = value;
                    _modified = true;
                }
            }
        }

        public Gable gableStyle
        {
            get { return _gableStyle; }
            set
            {
                if (_gableStyle != value)
                {
                    _gableStyle = value;
                    _modified = true;
                }
            }
        }

        public bool simpleGable
        {
            get { return _simpleGable; }
            set
            {
                if (_simpleGable != value)
                {
                    _simpleGable = value;
                    _modified = true;
                }
            }
        }

        public float gableHeight
        {
            get { return _gableHeight; }
            set
            {
                if (_gableHeight != value)
                {
                    _gableHeight = value;
                    _modified = true;
                }
            }
        }

        public float gableThickness
        {
            get { return _gableThickness; }
            set
            {
                if (_gableThickness != value)
                {
                    _gableThickness = value;
                    _modified = true;
                }
            }
        }

        public bool Equals(VolumePoint p)
        {
            if(p == null) return false;
            if(_position != p._position) return false;
            if(_controlA != p._controlA) return false;
            if(_controlB != p._controlB) return false;
            if(_facade != p._facade) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return _position.GetHashCode();
        }

        public bool Equals(UnityEngine.Object a)
        {
            return Equals(a);// (Dot(this, a) > 0.999f);
        }

        public override bool Equals(object a)
        {
            return Equals(a);// (Dot(this, a) > 0.999f);
        }

        public static bool operator ==(VolumePoint a, VolumePoint b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            return a.Equals(b);
        }

        public static bool operator !=(VolumePoint a, VolumePoint b)
        {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);
            return !a.Equals(b);
        }
    }
}