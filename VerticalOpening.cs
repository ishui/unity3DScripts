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
    public class VerticalOpening
    {
        public const float WALL_THICKNESS = 0.15f;

        public enum Usages
        {
            Space,
            Stairwell,
            Elevator
        }
        //todo complex shapes


        [SerializeField]
        private string _name = "Opening";
        [SerializeField]
        private Usages _usage = Usages.Space;
        [SerializeField]
        private Vector2Int _position;
        [SerializeField]
        private Vector2Int _size;
        [SerializeField]
        private float _rotation = 0;
        [SerializeField]
        private int _baseFloor = 0;
		[SerializeField]
		private int _floors = 1;
		[SerializeField]
		private float _stairWidth = 1;

		[SerializeField]
        private Surface _surfaceA;
        [SerializeField]
        private Surface _surfaceB;
        [SerializeField]
        private Surface _surfaceC;
        [SerializeField]
        private Surface _surfaceD;

        //stair type: straight, half landed, double, spiral
        //stairs or flat
        //enclosed, barriered, open
        //wall height
        //wall depth
        //step width

        private bool _isModified;

        public string name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    MarkModified();
                }
            }
        }

        public Usages usage
        {
            get { return _usage; }
            set
            {
                if (_usage != value)
                {
                    _usage = value;
                    MarkModified();
                }
            }
        }

        public Vector2Int position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    MarkModified();
                }
            }
        }

        public Vector2Int size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    MarkModified();
                }
            }
        }

        public float rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    MarkModified();
                }
            }
        }

        public int baseFloor
        {
            get { return _baseFloor; }
            set
            {
                if (value > -1 && _baseFloor != value)
                {
                    _baseFloor = value;
                    MarkModified();
                }
            }
        }

        public int floors
        {
            get { return _floors; }
            set
            {
                if (value > 0 && _floors != value)
                {
                    _floors = value;
                    MarkModified();
                }
            }
        }

        public float stairWidth
		{
            get { return _stairWidth; }
            set
            {
                if (value > 0 && _stairWidth != value)
                {
					_stairWidth = value;
                    MarkModified();
                }
            }
        }

        public Surface surfaceA
        {
            get { return _surfaceA; }
            set
            {
                if (_surfaceA != value)
                {
                    _surfaceA = value;
                    MarkModified();
                }
            }
        }

        public Surface surfaceB
        {
            get { return _surfaceB; }
            set
            {
                if (_surfaceB != value)
                {
                    _surfaceB = value;
                    MarkModified();
                }
            }
        }

        public Surface surfaceC
        {
            get { return _surfaceC; }
            set
            {
                if (_surfaceC != value)
                {
                    _surfaceC = value;
                    MarkModified();
                }
            }
        }

        public Surface surfaceD
        {
            get { return _surfaceD; }
            set
            {
                if (_surfaceD != value)
                {
                    _surfaceD = value;
                    MarkModified();
                }
            }
        }

        public bool hasBlankSurfaces
        {
            get
            {
                if(_surfaceA == null) return true;
                if(_surfaceB == null) return true;
                if(_surfaceC == null) return true;
                if(_surfaceD == null) return true;
                return false;
            }
        }

        public Vector2[] Points()
        {
            Vector2[] output = new Vector2[4];
            Vector2 v2pos = _position.vector2;
            output[0] = v2pos + new Vector2(-_size.vx, -_size.vy) * 0.5f;
            output[1] = v2pos + new Vector2(_size.vx, -_size.vy) * 0.5f;
            output[2] = v2pos + new Vector2(_size.vx, _size.vy) * 0.5f;
            output[3] = v2pos + new Vector2(-_size.vx, _size.vy) * 0.5f;
            return output;
        }


        public Vector2[] PointsRotated()
        {
            Vector2[] output = new Vector2[4];
            Vector2 v2pos = _position.vector2;
            output[0] = v2pos + Rotate(new Vector2(-_size.vx, -_size.vy) * 0.5f, -_rotation);
            output[1] = v2pos + Rotate(new Vector2(_size.vx, -_size.vy) * 0.5f, -_rotation);
            output[2] = v2pos + Rotate(new Vector2(_size.vx, _size.vy) * 0.5f, -_rotation);
            output[3] = v2pos + Rotate(new Vector2(-_size.vx, _size.vy) * 0.5f, -_rotation);
            return output;
        }

        public bool FloorIsIncluded(int floor)
        {
            return floor >= _baseFloor && floor <= _baseFloor + _floors;
        }

        public bool isModified
        {
            get
            {
                if (_isModified) return true;
                return false;
            }
        }

        public void MarkModified()
        {
            _isModified = true;
        }

        public static Vector2 Rotate(Vector2 input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            input.x = (cos * tx) - (sin * ty);
            input.y = (sin * tx) + (cos * ty);
            return input;
        }
    }
}