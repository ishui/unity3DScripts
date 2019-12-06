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

[System.Serializable]
public struct FlatBounds
{
    private Rect _core;
    private bool _initialised;

    public FlatBounds(float x, float y, float width, float height)
    {
        _core = new Rect(x, y, width, height);
        _initialised = true;
    }

    public FlatBounds(Vector2 position, Vector2 size)
    {
        _core = new Rect(position.x, position.y, size.x, size.y);
        _initialised = true;
    }

    public FlatBounds(FlatBounds copy)
    {
        _core = new Rect(copy.x, copy.y, copy.width, copy.height);
        _initialised = copy.initialised;
    }

    public FlatBounds(Vector2[] points)
    {
        _core = new Rect(points[0].x, points[0].y, 0, 0);
        _initialised = true;
        int pointSize = points.Length;
        for(int p = 0; p < pointSize; p++)
            Encapsulate(points[p]);
    }

    public FlatBounds(Rect copy)
    {
        _core = new Rect(copy.x, copy.y, copy.width, copy.height);
        _initialised = copy.size.x != 0 && copy.size.y != 0;
    }

    public FlatBounds(Bounds bounds)
    {
        _core = new Rect(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z, bounds.size.x, bounds.size.z);
        _initialised = bounds.extents.magnitude > Mathf.Epsilon;
    }

    public float x { get {return _core.x;} }
    public float y { get {return _core.y;} }
    public float width { get {return _core.width;} }
    public float height { get { return _core.height; } }
    public Vector2 center { get { return _core.center; } }
    public Rect rect { get { return _core; } }
    public float xMin { get { return _core.xMin; } }
    public float xMax { get { return _core.xMax; } }
    public float yMin { get { return _core.yMin; } }
    public float yMax { get { return _core.yMax; } }
    public Vector2 min { get { return _core.min; } }
    public Vector2 max { get { return _core.max; } }
    public Vector2 size { get { return _core.size; } }

    public bool Contains(float x, float y)
    {
        return Contains(new Vector2(x, y));
    }

    public bool Contains(Vector2 point)
    {
//        if(_core.xMin < 0 || _core.yMin < 0)
//        {
//            Vector2 offset = new Vector2(-_core.xMin, -_core.yMin);
//            _core.center += offset;
//            point += offset;
//            bool test = _core.Contains(point, true);
//            _core.center += -offset;
//            return test;
//        }
        return _core.Contains(point, true);
    }

    public void Encapsulate(float x, float y)
    {
        Encapsulate(new Vector2(x,y));
    }

    public void Encapsulate(Vector2 point)
    {
        if(_core.Contains(point))
            return;

        if(!_initialised)
        {
            _core.position = point;
            _initialised = true;
            return;
        }

        if (_core.xMin > point.x)
            _core.xMin = point.x;
        else if (_core.xMax < point.x)
            _core.xMax = point.x;

        if (_core.yMin > point.y)
            _core.yMin = point.y;
        else if (_core.yMax < point.y)
            _core.yMax = point.y;
    }

    public void Encapsulate(Vector2[] points)
    {
        int pointSize = points.Length;
        for (int p = 0; p < pointSize; p++)
            Encapsulate(points[p]);
    }

    public void Encapsulate(List<Vector2> points)
    {
        int pointSize = points.Count;
        for (int p = 0; p < pointSize; p++)
            Encapsulate(points[p]);
    }

    public void Encapsulate(Rect rectangle)
    {
        Encapsulate(rectangle.min);
        Encapsulate(rectangle.max);
    }

    public void Encapsulate(FlatBounds bounds)
    {
        Encapsulate(bounds.min);
        Encapsulate(bounds.max);
    }

    public bool Overlaps(FlatBounds other, bool allowInverse = false)
    {
        return _core.Overlaps(other.rect, allowInverse);
    }

    public void Expand(float margin)
    {
        _core.min = new Vector2(_core.xMin - margin, _core.yMin - margin);
        _core.size = new Vector2(_core.width + margin * 2f, _core.height + margin * 2f);
    }

    public bool initialised { get {return _initialised;} }

    public void DrawDebug(Color col)
    {
        Debug.DrawLine(new Vector3(_core.x, 0, _core.y), new Vector3(_core.x, 0, _core.y + _core.height), col);
        Debug.DrawLine(new Vector3(_core.x, 0, _core.y), new Vector3(_core.x + _core.width, 0, _core.y), col);
        Debug.DrawLine(new Vector3(_core.x + _core.width, 0, _core.y), new Vector3(_core.x + _core.width, 0, _core.y + _core.height), col);
        Debug.DrawLine(new Vector3(_core.x, 0, _core.y + _core.height), new Vector3(_core.x + _core.width, 0, _core.y + _core.height), col);
    }

    public float Area()
    {
        return _core.width * _core.height;
    }

    public void Clear()
    {
        _core.size = Vector2.zero;
        _core.position = Vector2.zero;
        _initialised = false;
    }
    
    public override string ToString()
    {
        return _core.ToString();
    }
}
