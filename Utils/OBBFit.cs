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
using System.Collections.Generic;
using JaspLib;
using UnityEngine;

namespace BuildR2
{
    /// <summary>
    /// Rotating Calipers! Init!
    /// </summary>
    public class OBBFit {

        private FlatBounds _bounds = new FlatBounds();
        private List<OBBox> _obbList = new List<OBBox>();
        private List<float> _areaList = new List<float>();

        public OBBox Create(Vector2[] points)
        {
            if (JMath.IsConvex(points))
                return CreateConvex(points);
            //else
            return CreateConcave(points);
        }

        public OBBox Create(Vector2[] points, bool isConvex)
        {
            if (isConvex)
                return CreateConvex(points);
            //else
            return CreateConcave(points);
        }

        public List<OBBox> CreateSorted(Vector2[] points)
        {
            if (JMath.IsConvex(points))
                return CreateConvexSortedList(points);
            //else
            return CreateConcaveSorted(points);
        }

        public List<OBBox> CreateSorted(Vector2[] points, bool isConvex)
        {
            if (isConvex)
                return CreateConvexSortedList(points);
            //else
            return CreateConcaveSorted(points);
        }

        public OBBox CreateConcave(Vector2[] points)
        {
            int[] convexPoints = JMath.ConvexHull(points);

            int convexSize = convexPoints.Length;
            Vector2[] convexHull = new Vector2[convexSize];
            for (int c = 0; c < convexSize; c++)
                convexHull[c] = points[convexPoints[c]];
            return CreateConvex(convexHull);
        }

        public List<OBBox> CreateConcaveSorted(Vector2[] points)
        {
            int[] convexPoints = JMath.ConvexHull(points);

            int convexSize = convexPoints.Length;
            Vector2[] convexHull = new Vector2[convexSize];
            for (int c = 0; c < convexSize; c++)
                convexHull[c] = points[convexPoints[c]];
            return CreateConvexSortedList(convexHull);
        }

        public OBBox CreateConvex(Vector2[] points)
        {
            if (points.Length == 0) Debug.LogError("No points sent!");
            int pointCount = points.Length;
            OBBox defaultBox = GetBox();
            OBBox output = defaultBox;
            float minArea = Mathf.Infinity;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = points[p];
                Vector2 p1 = points[(p + 1) % pointCount];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Mathf.Epsilon) continue;//ignore duplicate points
                float angle = JMath.SignAngle(dir);

                _bounds.Clear();
                for (int o = 0; o < pointCount; o++)//encapsulate rotated points
                    _bounds.Encapsulate(JMath.Rotate(points[o], angle));

                Vector2 center = JMath.Rotate(_bounds.center, -angle);
                OBBox candidate = GetBox(center, dir, _bounds.height, JMath.Rotate(dir, 90), _bounds.width);
                if(_bounds.Area() < minArea)
                {
                    if(output != defaultBox)
                        PutBox(output);
                    output = candidate;
                }
                else
                {
                    PutBox(candidate);
                }
            }

            return output;
        }

        public List<OBBox> CreateConvexSortedList(Vector2[] points)
        {
            List<OBBox> output = new List<OBBox>();
            _obbList.Clear();
            _areaList.Clear();
            if (points.Length == 0) Debug.LogError("No points sent!");
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = points[p];
                Vector2 p1 = points[(p + 1) % pointCount];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Mathf.Epsilon) continue;//ignore invalid sides
                float angle = JMath.SignAngle(dir);

                _bounds.Clear();
                for (int o = 0; o < pointCount; o++)//encapsulate rotated points
                    _bounds.Encapsulate(JMath.Rotate(points[o], angle));

                Vector2 center = JMath.Rotate(_bounds.center, -angle);
                OBBox box = GetBox(center, dir, _bounds.height, JMath.Rotate(dir, 90), _bounds.width);
                _obbList.Add(box);
                _areaList.Add(_bounds.Area());
            }

            int obbCount = _obbList.Count;
            OBBox defaultBox = GetBox();
            for(int i = 0; i < obbCount; i++)
            {
                float smallestArea = float.PositiveInfinity;
                OBBox candidate = defaultBox;
                int index = -1;
                for(int j = 0; j < _obbList.Count; j++)
                {
                    if(smallestArea > _areaList[j])
                    {
                        candidate = _obbList[j];
                        smallestArea = _areaList[j];
                        index = j;
                    }
                }

                if(index != -1)
                {
                    output.Add(candidate);
                    _obbList.RemoveAt(index);
                    _areaList.RemoveAt(index);
                }
            }

            PutBox(defaultBox);
            while(_obbList.Count > 0) {
                PutBox(_obbList[0]);
                _obbList.RemoveAt(0);
            }

            return output;
        }

        public static OBBox CreateAlongSide(Vector2[] points, int index)
        {
            if (points.Length == 0) Debug.LogError("No points sent!");
            int pointCount = points.Length;
            OBBox output = GetBox();

            Vector2 p0 = points[index];
            Vector2 p1 = points[index < pointCount - 1 ? index + 1 : 0];
            Vector2 dir = (p1 - p0).normalized;
            float angle = JMath.SignAngle(dir);

            FlatBounds bounds = new FlatBounds();
            for (int o = 0; o < pointCount; o++)//encapsulate rotated points
                bounds.Encapsulate(JMath.Rotate(points[o], angle));

            Vector2 center = JMath.Rotate(bounds.center, -angle);
            output.SetValues(center, dir, bounds.height, JMath.Rotate(dir, 90), bounds.width);

            return output;
        }

        //STATICS
        //POOL
        private static List<OBBox> _obPool = new List<OBBox>();

        public static void Release() {
            _obPool.Clear();
        }

        public static OBBox GetBox() {
            OBBox output;
            if (_obPool.Count > 0) {
                output = _obPool[0];
                _obPool.RemoveAt(0);
            }
            else {
                output = new OBBox();
            }
            return output;
        }

        public static OBBox GetBox(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB) {
            OBBox output;
            if (_obPool.Count > 0) {
                output = _obPool[0];
                output.SetValues(newCenter, dirA, sizeA, dirB, sizeB);
                _obPool.RemoveAt(0);
            }
            else {
                output = new OBBox(newCenter, dirA, sizeA, dirB, sizeB);
            }
            return output;
        }

        public static void PutBox(OBBox box) {
            box.Clear();
            _obPool.Add(box);
        }
    }

    [Serializable]
    public struct OBBox {
        private Vector2 _center;
        private Vector2 _longDir;
        private Vector2 _shortDir;
        private float _area;
        private float _aspect;

        //    public OBBox() { }

        public OBBox(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB) : this() {
            SetValues(newCenter, dirA, sizeA, dirB, sizeB);
        }

        public void SetValues(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB) {
            _center = newCenter;
            if (sizeB < sizeA) {
                _longDir = dirA;
                longSize = sizeA;
                _shortDir = dirB;
                shortSize = sizeB;
            }
            else {
                _longDir = dirB;
                longSize = sizeB;
                _shortDir = dirA;
                shortSize = sizeA;
            }

            _area = longSize * shortSize;
            _aspect = shortSize / longSize;
        }

        public void Clear() {
            _center = Vector2.zero;
            _longDir = Vector2.zero;
            _shortDir = Vector2.zero;
            _area = 0;
            _aspect = 0;
        }

        public Vector2 center { get { return _center; } }
        public Vector2 longDir { get { return _longDir; } }
        public Vector2 shortDir { get { return _shortDir; } }
        public float longSize { get; private set; }
        public float shortSize { get; private set; }
        public float area { get { return _area; } }
        public float aspect { get { return _aspect; } }

        public void DebugDraw() {
            float hLen = longSize * 0.5f;
            float hWid = shortSize * 0.5f;
            Vector2 tl = _center + _longDir * hLen - _shortDir * hWid;
            Vector2 tr = _center + _longDir * hLen + _shortDir * hWid;
            Vector2 bl = _center - _longDir * hLen - _shortDir * hWid;
            Vector2 br = _center - _longDir * hLen + _shortDir * hWid;

            Vector3 tlv3 = new Vector3(tl.x, 0, tl.y);
            Vector3 trv3 = new Vector3(tr.x, 0, tr.y);
            Vector3 blv3 = new Vector3(bl.x, 0, bl.y);
            Vector3 brv3 = new Vector3(br.x, 0, br.y);

            Debug.DrawLine(tlv3, trv3, Color.blue);
            Debug.DrawLine(trv3, brv3, Color.blue);
            Debug.DrawLine(brv3, blv3, Color.blue);
            Debug.DrawLine(blv3, tlv3, Color.blue);

            Vector3 vCent = JMath.ToV3(_center);
            Debug.DrawLine(vCent, tlv3, Color.red);
            Debug.DrawLine(vCent, trv3, Color.red);
            Debug.DrawLine(vCent, blv3, Color.red);
            Debug.DrawLine(vCent, brv3, Color.red);
        }

        public void DebugDrawPlotCut() {
            DebugDrawPlotCut(0.5f);
        }

        public void DebugDrawPlotCut(float variation) {
            float hLen = longSize * 0.5f;
            float hWid = shortSize * 0.5f;
            Vector2 tl = _center + _longDir * hLen - _shortDir * hWid;
            Vector2 tr = _center + _longDir * hLen + _shortDir * hWid;
            Vector2 bl = _center - _longDir * hLen - _shortDir * hWid;
            Vector2 br = _center - _longDir * hLen + _shortDir * hWid;

            Vector3 tlv3 = new Vector3(tl.x, 0, tl.y);
            Vector3 trv3 = new Vector3(tr.x, 0, tr.y);
            Vector3 blv3 = new Vector3(bl.x, 0, bl.y);
            Vector3 brv3 = new Vector3(br.x, 0, br.y);

            Debug.DrawLine(tlv3, trv3, Color.blue);
            Debug.DrawLine(trv3, brv3, Color.blue);
            Debug.DrawLine(brv3, blv3, Color.blue);
            Debug.DrawLine(blv3, tlv3, Color.blue);


            Vector2 cenExt = _longDir * longSize * 0.5f;
            Vector2 cutCenter = Vector2.Lerp(_center - cenExt, _center + cenExt, variation);
            Vector2 intExt = _shortDir * shortSize;
            Vector2 intP0 = cutCenter - intExt;
            Vector2 intP1 = cutCenter + intExt;
            Debug.DrawLine(JMath.ToV3(intP0), JMath.ToV3(intP1), Color.red, 20);
        }

        public void DebugMark() {
            Vector3 cen = JMath.ToV3(_center);
            Debug.DrawLine(cen + Vector3.left * 10, cen + Vector3.right * 10, Color.red, 20);
            Debug.DrawLine(cen + Vector3.forward * 10, cen + Vector3.back * 10, Color.blue, 20);
        }

        public static bool operator ==(OBBox a, OBBox b)
        {
            if(a._area != b._area) return false;
            if(a._aspect != b._aspect) return false;
            if(a._center != b._center) return false;
            if(a._longDir != b._center) return false;
            if(a._shortDir != b._shortDir) return false;
            return true;
        }

        public static bool operator !=(OBBox a, OBBox b) {
            if (a._area == b._area) return false;
            if (a._aspect == b._aspect) return false;
            if (a._center == b._center) return false;
            if (a._longDir == b._center) return false;
            if (a._shortDir == b._shortDir) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if(obj == null) return false;
            return obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return (int)(_area * _aspect * _longDir.x * _longDir.y * _shortDir.x * _shortDir.y);
        }
    }
}

