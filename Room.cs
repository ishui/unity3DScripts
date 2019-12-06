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
using System.Collections.Generic;

namespace BuildR2
{
    [Serializable]
    public class Room
    {
        [SerializeField]
        private string name = "Room";
        [SerializeField]
        private List<RoomPoint> _points = new List<RoomPoint>();
        [SerializeField]
        private List<RoomPortal> _portals = new List<RoomPortal>();
        private bool _isModified = true;
        private FlatBounds _bounds = new FlatBounds(Vector3.zero, Vector3.zero);

        [SerializeField]
        private RoomStyle _roomStyle;

		#region Constructors
		public Room(List<RoomPoint> points)
		{
			_points.Clear();
			_points.AddRange(points);
			MarkModified();
		}

		public Room(Vector3[] points, Vector3 offset)
	    {
		    _points.Clear();
		    bool clockwise = BuildrPolyClockwise.Check(points);
		    if (!clockwise) Array.Reverse(points);
		    int pointCount = points.Length;
		    for (int p = 0; p < pointCount; p++)
			    _points.Add(new RoomPoint(points[p] + offset));
		    MarkModified();
		}

	    public Room(List<Vector3> points, Vector3 offset)
	    {
		    _points.Clear();
		    bool clockwise = BuildrPolyClockwise.Check(points);
		    if (!clockwise) points.Reverse();
		    int pointCount = points.Count;
		    for (int p = 0; p < pointCount; p++)
			    _points.Add(new RoomPoint(points[p] + offset));
		    MarkModified();
	    }

		public Room(List<Vector2> points, Vector3 offset)
		{
			Vector2 of = new Vector2(offset.x, offset.z);
			_points.Clear();
            bool clockwise = BuildrPolyClockwise.Check(points);
            if (!clockwise) points.Reverse();
            int pointCount = points.Count;
            for (int p = 0; p < pointCount; p++)
                _points.Add(new RoomPoint(points[p] + of));
            MarkModified();
        }

        public Room(List<Vector2Int> points, Vector3 offset)
		{
			Vector2Int of = new Vector2Int(offset.x, offset.z);
			_points.Clear();
            int pointCount = points.Count;
            bool clockwise = BuildrPolyClockwise.Check(points);
            if (!clockwise) points.Reverse();
            for (int p = 0; p < pointCount; p++)
                _points.Add(new RoomPoint(points[p] + of));
            MarkModified();
        }

        public Room(FlatBounds bounds, Vector3 offset)
        {
            _points.Clear();
            Vector2Int of = new Vector2Int(offset.x, offset.z);
            Vector2Int p0 = new Vector2Int(bounds.xMin, bounds.yMin) + of;
            Vector2Int p1 = new Vector2Int(bounds.xMax, bounds.yMin) + of;
            Vector2Int p2 = new Vector2Int(bounds.xMax, bounds.yMax) + of;
            Vector2Int p3 = new Vector2Int(bounds.xMin, bounds.yMax) + of;

            _points.Add(new RoomPoint(p0));
            _points.Add(new RoomPoint(p1));
            _points.Add(new RoomPoint(p2));
            _points.Add(new RoomPoint(p3));

            MarkModified();
        }
        #endregion

        public string roomName
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    MarkModified();
                }
            }
        }
        
        public RoomStyle style
        {
            get { return _roomStyle; }
            set
            {
                if (_roomStyle != value)
                {
                    _roomStyle = value;
                    MarkModified();
                }
            }
        }

        #region Point Manipulation
        public RoomPoint this[int index]
        {
            get { return _points[index]; }
            set
            {
                if (_points[index] != value || value.modified)
                {
                    _points[index] = value;
                    MarkModified();
                }
            }
        }

        public int numberOfPoints
        {
            get { return _points.Count; }
        }

        public RoomPoint[] AllRoomPoints()
        {
            return _points.ToArray();
        }

        public Vector2Int center
        {
            get
            {
                return new Vector2Int(_bounds.center);
            }
        }

        public Vector2Int[] AllPoints()
        {
            Vector2Int[] output = new Vector2Int[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                output[i] = this[i].position;
            return output;
        }

        //inform the floorplan that a point has been modified
        //check to see if this point affects this plan
        public void PointModified(List<RoomPoint> data)
        {
            int dataCount = data.Count;
            if (data.Count == 0)
                return;
            int pointCount = numberOfPoints;
            for (int d = 0; d < dataCount; d++)
            {
                RoomPoint point = data[d];
                if (!point.moved) continue;
                for (int p = 0; p < pointCount; p++)
                {
                    if (point.lastPosition == _points[p].position)
                    {
                        _points[p].position = point.position;
                        _points[p].MarkUnmodified();
                    }
                }
            }
            if (isModified)
                MarkModified();
        }

        public void MovePoint(Vector2Int point, Vector2Int delta)
        {
            int roomPointCount = numberOfPoints;
            for (int i = 0; i < roomPointCount; i++)
            {
                if (_points[i].position == point)
                {
                    _points[i].position += delta;
                    MarkModified();
                }
            }
        }

        public void MovePoint(Vector2Int point, Vector3 delta)
        {
            Debug.Log("MovePoint");
            int roomPointCount = numberOfPoints;
            for (int i = 0; i < roomPointCount; i++)
            {
                if (_points[i].position == point)
                    _points[i].position += new Vector2(delta.x, delta.z);
            }
        }

        public void MovePoint(Vector2Int point, Vector2 delta)
        {
            int roomPointCount = numberOfPoints;
            for (int i = 0; i < roomPointCount; i++)
            {
                if (_points[i].position == point)
                    _points[i].position += new Vector2(delta.x, delta.y);
            }
        }

        public List<RoomPoint> GetModifiedPoints()
        {
            List<RoomPoint> output = new List<RoomPoint>();
            for (int i = 0; i < numberOfPoints; i++)
                if (this[i].modified) output.Add(_points[i]);
            return output;
        }

        public List<RoomPoint> GetMovedPoints()
        {
            List<RoomPoint> output = new List<RoomPoint>();
            for (int i = 0; i < numberOfPoints; i++)
                if (this[i].moved) output.Add(_points[i]);
            return output;
        }

        public void AddPoint(Vector2Int newPosition)
        {
            RoomPoint newPoint = new RoomPoint(newPosition);
            _points.Add(newPoint);
            CheckPlan();
            MarkModified();
        }

        public void AddPoint(Vector3 newPoint)
        {
            AddPoint(new Vector2Int(newPoint, true));
        }

        public void AddPoints(Vector2Int[] newPoints)
        {
            if (!BuildrPolyClockwise.Check(newPoints))
                Array.Reverse(newPoints);
            for (int i = 0; i < newPoints.Length; i++) AddPoint(newPoints[i]);
            CheckPlan();
            MarkModified();
        }

        public void AddPoints(Vector3[] newPoints)
        {
            for (int i = 0; i < newPoints.Length; i++) AddPoint(newPoints[i]);
            CheckPlan();
            MarkModified();
        }

        public void RemovePoint(Vector2Int point)
        {
            if (numberOfPoints < 4)
                return;
            int index = -1;
            for (int i = 0; i < numberOfPoints; i++)
                if (this[i].position == point)
                {
                    index = i;
                    break;
                }
            if (index != -1)
            {
                RemovePointAt(index);
                CheckPlan();
                MarkModified();
            }
        }

        public void RemovePointAt(int index)
        {
            if (numberOfPoints < 4)
                return;
            _points.RemoveAt(index);
            CheckPlan();
            MarkModified();
        }

        public void InsertPoint(int index, Vector2Int point)
        {
            _points.Insert(index, new RoomPoint(point));
            MarkModified();
        }

        public int ContainsWall(Vector2Int a, Vector2Int b)
        {
            int pointCount = numberOfPoints;
            for(int w = 0; w < pointCount; w++)
            {
                int wb = (w + 1) % pointCount;
                int wa = (w - 1 + pointCount) % pointCount;
                if (this[w].position == b)
                {
                    if (this[wa].position == a)
                        return wa;
                    if (this[wb].position == a)
                        return w;
                }
                if (this[w].position == a)
                {
                    if (this[wa].position == b)
                        return wa;
                    if (this[wb].position == a)
                        return w;
                }
            }
            return -1;
        }

        #endregion

        #region Portal Manipulation

        public RoomPortal GetPortal(int index)
        {
            return _portals[index];
        }

        public RoomPortal[] GetAllPortals()
        {
            return _portals.ToArray();
        }

        public int numberOfPortals
        {
            get {return _portals.Count;}
        }

        public void AddPortal(RoomPortal newPortal)
        {
            _portals.Add(newPortal);
            CheckPlan();
            MarkModified();
        }

        public void RemovePortal(RoomPortal portal)
        {
            _portals.Remove(portal);
            CheckPlan();
            MarkModified();
        }


        public void RemovePortalAt(int index)
        {
            _portals.RemoveAt(index);
            CheckPlan();
            MarkModified();
        }

        public bool HasPortal(RoomPortal portal)
        {
            return _portals.Contains(portal);
        }

        public Vector3 PortalPosition(RoomPortal portal)
        {
            int wallIndex = portal.wallIndex;
            if (wallIndex == -1)
                return Vector3.zero;
            Vector3 p0 = this[wallIndex % numberOfPoints].position.vector3XZ;
            Vector3 p1 = this[(wallIndex + 1) % numberOfPoints].position.vector3XZ;
            return Vector3.Lerp(p0, p1, portal.lateralPosition);
        }

        #endregion

        private void CheckPlan()
        {
            //TODO
            PlanLegality();
            CalculatePlanBounds();

            //            List<RoomPoint> modifiedPoints = GetModifiedPoints();
            //            for (int p = 0; p < modifiedPoints.Count; p++)
            //                CheckInternalPointMovement(modifiedPoints[p]);
        }

        public bool isModified
        {
            get
            {
                if (_isModified) return true;
                for (int p = 0; p < numberOfPoints; p++)
                    if (this[p].modified) return true;
                for (int p = 0; p < numberOfPortals; p++)
                    if (_portals[p].modified) return true;
                return false;
            }
        }

        public void MarkModified()
        {
            CheckPlan();
            Serialise();
            _isModified = true;
        }

        public void MarkUnmodified()
        {
            _isModified = false;
            for (int p = 0; p < numberOfPoints; p++)
                this[p].MarkUnmodified();
            for (int p = 0; p < numberOfPortals; p++)
                _portals[p].MarkUnmodified();
        }

        private void CalculatePlanBounds()
        {
            _bounds.Clear();
            int pointCount = _points.Count;
            for (int p = 0; p < pointCount; p++)
                _bounds.Encapsulate(_points[p].position.vector2);
        }

        public void CalculatePlanWalls(Volume volume)
        {
            Dictionary<int, List<Vector2Int>> facadeWallAnchors = volume.facadeWallAnchors;
            int pointCount = numberOfPoints;
            
            for (int p = 0; p < pointCount; p++)
            {
                Vector2Int startPoint = this[p].position;
                int pb = (p + 1) % pointCount;
                Vector2Int endPoint = this[pb].position;
                FloorplanUtil.RoomWall newWall = FloorplanUtil.CalculateNewWall(volume, startPoint, endPoint);
                
                int size = newWall.offsetPoints.Length;
                newWall.offsetPointWallSection = new int[size];
                newWall.anchorPointIndicies = new int[size];
                int currentFacade = newWall.facadeIndex;

				for (int s = 0; s < size; s++)
	                newWall.offsetPointWallSection[s] = facadeWallAnchors[currentFacade].IndexOf(newWall.offsetPointsInt[s]);

                this[p].wall= newWall;
            }
        }

        private bool PlanLegality()
        {
            for (int p = 0; p < numberOfPoints; p++)
                this[p].illegal = false;
            for(int p = 0; p < numberOfPoints - 1; p++)
            {
                if(this[p] == this[p+1])
                    _points.RemoveAt(p+1);//remove duplicate points
            }
            bool output = false;
            for (int pA = 0; pA < numberOfPoints; pA++)
            {
                Vector2Int p0 = _points[pA].position;
                Vector2Int p1 = _points[(pA + 1) % numberOfPoints].position;
                for (int pB = 0; pB < numberOfPoints; pB++)
                {
                    if (pA == pB) continue;//skip testing wall on itself

                    Vector2Int p2 = _points[pB].position;
                    Vector2Int p3 = _points[(pB + 1) % numberOfPoints].position;

                    if (p0 == p2 || p0 == p3 || p1 == p2 || p1 == p3) continue;//don't test lines that connect

                    if (BuildrUtils.FastLineIntersection(p0, p1, p2, p3))
                    {
                        _points[pA].illegal = true;
                        _points[pB].illegal = true;
                        output = true;
                    }
                }
            }
            return output;
        }

        public void DebugDraw()
        {
            Vector3 centerV3 = center.vector3XZ;
            for(int p = 0; p < numberOfPoints; p++)
                Debug.DrawLine(centerV3, _points[p].position.vector3XZ);
        }

        #region Serialisation

        [SerializeField]
        private List<Vector2> serializedPoints = new List<Vector2>();
        [SerializeField]
        private RoomPortal[] serializedPortals = new RoomPortal[0];
//        [SerializeField]
//        private RoomStyle serializedRoomStyle = null;

        public void OnBeforeSerialize()
        {
            //ignore calls - serialisation handled internally when data has changed
        }

        public void OnAfterDeserialize()
        {
            int serializedPointCount = serializedPoints.Count;
            if (serializedPointCount > 0)
            {
                _points.Clear();
                for (int p = 0; p < serializedPointCount; p++)
                {
                    _points.Add(new RoomPoint(serializedPoints[p]));
                }
            }
            int serializedPortalCount = serializedPortals.Length;
            if (serializedPortalCount > 0)
            {
                _portals.Clear();
                for (int p = 0; p < serializedPortalCount; p++)
                    _portals.Add(serializedPortals[p]);
            }
            CheckPlan();
        }

        public void Serialise()
        {
            int serializedPointCount = _points.Count;
            serializedPoints.Clear();
            for (int p = 0; p < serializedPointCount; p++)
                serializedPoints.Add(_points[p].position.vector2);

            int serializedPortalCount = _portals.Count;
            serializedPortals = new RoomPortal[serializedPortalCount];
            for(int p = 0; p < serializedPortalCount; p++)
                serializedPortals[p] = _portals[p];
        }
        #endregion
    }

    [Serializable]
    public class RoomPoint
    {
        [SerializeField]
        private Vector2Int _position;
        [SerializeField]
        private FloorplanUtil.RoomWall _wall;

        private Vector2Int _lastPosition;
        private bool _moved;
        private bool _illegal;
        private bool _modified;

        //const
        public RoomPoint(Vector2Int pos)
        {
            _position = pos;
            _modified = false;
            _illegal = false;
        }

        public RoomPoint(Vector2 pos)
        {
            _position = new Vector2Int(pos);
            _modified = false;
            _illegal = false;
        }

        public RoomPoint(Vector3 pos)
        {
            _position = new Vector2Int(pos, true);
            _modified = false;
            _illegal = false;
        }

        public Vector2Int position
        {
            get { return _position; }
            set
            {
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

        public bool illegal
        {
            get { return _illegal; }
            set { _illegal = value; }
        }

        public FloorplanUtil.RoomWall wall
        {
            get { return _wall; }
            set { _wall = value; }
        }

        public RoomPoint Clone()
        {
            RoomPoint output = new RoomPoint(_position);
            return output;
        }

        public bool Equals(RoomPoint p)
        {
            if (_position != p._position) return false;
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

        public static bool operator ==(RoomPoint a, RoomPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RoomPoint a, RoomPoint b)
        {
            return !a.Equals(b);
        }
    }

    [Serializable]
    public class RoomPortal
    {
        [SerializeField]
        private int _wallIndex;
        [SerializeField]
        private float _lateralPosition = 0.5f;
        [SerializeField]
        private float _verticalPosition = 0.0f;
        [SerializeField]
        private float _width = 0.85f;
        [SerializeField]
        private float _height = 2.1f;
        private bool _modified;

        //const
        public RoomPortal(int wallIndex)
        {
            _wallIndex = wallIndex;
            _modified = false;
        }

        public int wallIndex
        {
            get {return _wallIndex;}
            set
            {
                if(_wallIndex != value)
                {
                    _wallIndex = value;
                    _modified = true;
                }
            }
        }

        public float lateralPosition
        {
            get { return _lateralPosition; }
            set
            {
                if (_lateralPosition != value)
                {
                    _lateralPosition = value;
                    _modified = true;
                }
            }
        }

        public float verticalPosition
        {
            get { return _verticalPosition; }
            set
            {
                if (_verticalPosition != value)
                {
                    _verticalPosition = value;
                    _modified = true;
                }
            }
        }

        public float width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    _modified = true;
                }
            }
        }

        public float height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    _modified = true;
                }
            }
        }

        public bool modified
        {
            get { return _modified; }
        }

        public void MarkUnmodified()
        {
            _modified = false;
        }
    }
}