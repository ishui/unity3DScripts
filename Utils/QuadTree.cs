using System.Collections.Generic;
using UnityEngine;

/*
Quadtree by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace BuildR2
{

//Any object that you insert into the tree must implement this interface
    public interface IQuadTreeObject
    {
        Vector2 position {get;}
        Vector3 positionV3 {get;}
    }

    public class QuadTree<T> where T : IQuadTreeObject
    {
        private readonly int _maxObjectCount;
        private readonly List<T> _objects;
        private readonly List<T> _subObjects;
        private readonly List<QuadTree<T>> _subQuads;
        private Rect _bounds;
        private float _area = 0;
        private readonly QuadTree<T>[] _cells;
        private readonly QuadTree<T> _parent = null;

        public QuadTree(int maxSize, Rect bounds, QuadTree<T> parent = null)
        {
            _parent = parent;
            _bounds = bounds;
            _area = _bounds.size.x * _bounds.size.y;
            _maxObjectCount = maxSize;
            _cells = new QuadTree<T>[4];
            _objects = new List<T>(maxSize);
            _subObjects = new List<T>();
            _subQuads = new List<QuadTree<T>>();

            if(_parent != null)
                _parent.NewQuad(this);
        }

        public void NewQuad(QuadTree<T> quad)
        {
            _subQuads.Add(quad);
            if(_parent != null)
                _parent.NewQuad(quad);
        }

        public Rect bounds {get {return _bounds;}}

        public float area {get {return _area;}}

        public List<T> objects {get {return _objects;}}

        public List<QuadTree<T>> subQuads {get {return _subQuads;}}

        public bool isLeaf {get {return _cells[0] == null;}}

        public bool isEmpty {get {return _objects.Count == 0;}}

        public int count
        {
            get
            {
                int output = 1;
                if(_cells[0] != null)
                {
                    output += _cells[0].count;
                    output += _cells[1].count;
                    output += _cells[2].count;
                    output += _cells[3].count;
                }

                return output;
            }
        }

        public void Insert(T objectToInsert)
        {
            _subObjects.Add(objectToInsert);
            if(_cells[0] != null)
            {
                int iCell = GetCellToInsertObject(objectToInsert.position);
                if(iCell > -1)
                    _cells[iCell].Insert(objectToInsert);
                return;
            }

            _objects.Add(objectToInsert);
            //Objects exceed the maximum count
            if(_objects.Count > _maxObjectCount)
            {
                //Split the quad into 4 sections
                if(_cells[0] == null)
                {
                    float subWidth = (_bounds.width / 2f);
                    float subHeight = (_bounds.height / 2f);
                    float x = _bounds.x;
                    float y = _bounds.y;
                    _cells[0] = new QuadTree<T>(_maxObjectCount, new Rect(x, y, subWidth, subHeight), this);
                    _cells[1] = new QuadTree<T>(_maxObjectCount, new Rect(x + subWidth, y, subWidth, subHeight), this);
                    _cells[2] = new QuadTree<T>(_maxObjectCount, new Rect(x, y + subHeight, subWidth, subHeight), this);
                    _cells[3] = new QuadTree<T>(_maxObjectCount, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), this);
                }

                //Reallocate this quads objects into its children
//            int i = _storedObjects.Count - 1;
//            Debug.Log("Drop values into child quads");
//            Debug.Log(_storedObjects.Count);
                while(_objects.Count > 0)
                {
                    T storedObj = _objects[0];
                    int iCell = GetCellToInsertObject(storedObj.position);
                    if(iCell > -1)
//                {
//                    Debug.Log(iCell + " " + storedObj.position + " " + _cells[iCell]._bounds);
                        _cells[iCell].Insert(storedObj);
//                }
                    _objects.RemoveAt(0);
//                i--;
                }
            }
        }

        public void Remove(T objectToRemove)
        {
            if(ContainsLocation(objectToRemove.position))
            {
                _subObjects.Remove(objectToRemove);
                _objects.Remove(objectToRemove);
                if(_cells[0] != null)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        _cells[i].Remove(objectToRemove);
                    }
                }
            }
        }

        //TODO make this function I need.
        //We need to specify two objects and it return a quad node that contains the leaves of both these
        //ideally it can do this with multiple object but two for now
        //that would be one function
        //we then want a function that dumps all the objects within that quad node...

        public QuadTree<T> GetContainer(T[] points)
        {
            int size = points.Length;
            Vector2[] positions = new Vector2[size];
            for(int p = 0; p < size; p++)
                positions[p] = points[p].position;
            return GetContainer(positions);
        }

        public List<T> GetObjectsInProximity(Vector2 position, float radius)
        {
            if(ContainsLocation(position))
            {
                if(_cells[0] == null)
                    return _subObjects;

                if(_cells[0]._bounds.width < radius)
                    return _subObjects;//leaves too small

                for(int i = 0; i < 4; i++)
                {
                    if(_cells[i].ContainsLocation(position))
                        return _cells[i].GetObjectsInProximity(position, radius);
                }
            }

            return _objects;
        }

        public QuadTree<T> GetContainer(Vector2[] points)
        {
            if(ContainsLocations(points))
            {
                if(_cells[0] == null) return this;//no children, this is the lowest container

                for(int i = 0; i < 4; i++)
                {
                    var cell = _cells[i].GetContainer(points);
                    if(cell != null)
                        return cell;//child claims to contain the points
                }

                return this;//no children contain all points
            }

            return null;
        }

        public List<T> RetreiveNeighbourObjects(Vector2 pointA, Vector2 pointB)
        {
            if(_bounds.Contains(pointA) || _bounds.Contains(pointB))
            {
                if(_cells[0] == null) return _objects;

                List<T> output = new List<T>();
                for(int i = 0; i < 4; i++)
                {
                    var cellObjects = _cells[i].RetreiveNeighbourObjects(pointA, pointB);
                    if(cellObjects != null)
                        output.AddRange(cellObjects);
                }

                return output;
            }

            return null;
        }

        public List<T> RetrieveObjectsInArea(Rect area)
        {
            if(RectOverlap(_bounds, area))
            {
                List<T> returnedObjects = new List<T>();
                foreach(T t in _objects)
                {
                    if(_bounds.Contains(t.position))
                        returnedObjects.Add(t);
                }

                if(_cells[0] == null) return returnedObjects;
                for(int i = 0; i < 4; i++)
                {
                    var cellObjects = _cells[i].RetrieveObjectsInArea(area);
                    if(cellObjects != null)
                        returnedObjects.AddRange(cellObjects);
                }

                return returnedObjects;
            }

            return null;
        }

        public List<T> RetrieveObjectsInAreaQuick(Rect area)
        {
            if(RectOverlap(_bounds, area) && _cells[0] != null)
            {
                int cells = 0;
                if(_cells[0].RectOverlap(_bounds, area)) cells++;
                if(_cells[1].RectOverlap(_bounds, area)) cells++;
                if(_cells[2].RectOverlap(_bounds, area)) cells++;
                if(_cells[3].RectOverlap(_bounds, area)) cells++;

                if(cells > 1)
                    return _subObjects;

                if(_cells[0].RectOverlap(_bounds, area)) return _cells[0].RetrieveObjectsInAreaQuick(area);
                if(_cells[1].RectOverlap(_bounds, area)) return _cells[1].RetrieveObjectsInAreaQuick(area);
                if(_cells[2].RectOverlap(_bounds, area)) return _cells[2].RetrieveObjectsInAreaQuick(area);
                if(_cells[3].RectOverlap(_bounds, area)) return _cells[3].RetrieveObjectsInAreaQuick(area);
            }

            return _subObjects;
        }


        public List<T> subObjects
        {
            get
            {
//            List<T> output = new List<T>();
//            output.AddRange(_storedObjects);
//            if(_cells[0] == null) return output;
//            for (int i = 0; i < 4; i++) output.AddRange(_cells[i].subObjects);
                return _subObjects;
            }
        }

        // Clear quadtree
        public void Clear()
        {
            _objects.Clear();
            _subObjects.Clear();

            for(int i = 0; i < _cells.Length; i++)
            {
                if(_cells[i] != null)
                {
                    _cells[i].Clear();
                    _cells[i] = null;
                }
            }
        }

        public QuadTree<T> GetObject(Vector2 location)
        {
            if(_cells[0] != null)
            {
                int iCell = GetCellToInsertObject(location);
                if(iCell > -1) return _cells[iCell];
            }

            return this;
        }

        public bool ContainsLocation(Vector2 location)
        {
            return _bounds.Contains(location, true);
        }

        public bool ContainsLocations(Vector2[] locations)
        {
            int locationCount = locations.Length;
            for(int l = 0; l < locationCount; l++)
            {
                Vector2 location = locations[l];
                if(!_bounds.Contains(location, true)) return false;
            }

            return true;
        }

        private int GetCellToInsertObject(Vector2 location)
        {
            for(int i = 0; i < 4; i++)
                if(_cells[i].ContainsLocation(location))
                    return i;
            return -1;
        }

        bool ValueInRange(float value, float min, float max)
        {
            return (value >= min) && (value <= max);
        }

        bool RectOverlap(Rect a, Rect b)
        {
            bool xOverlap = ValueInRange(a.x, b.x, b.x + b.width) || ValueInRange(b.x, a.x, a.x + a.width);

            bool yOverlap = ValueInRange(a.y, b.y, b.y + b.height) || ValueInRange(b.y, a.y, a.y + a.height);

            return xOverlap && yOverlap;
        }

        public void DrawDebugQuads(Color col, float time = 0, bool subBranches = true)
        {
            if(time == 0) time = Time.deltaTime;
            Debug.DrawLine(new Vector3(_bounds.x, 0, _bounds.y), new Vector3(_bounds.x, 0, _bounds.y + _bounds.height), col);
            Debug.DrawLine(new Vector3(_bounds.x, 0, _bounds.y), new Vector3(_bounds.x + _bounds.width, 0, _bounds.y), col);
            Debug.DrawLine(new Vector3(_bounds.x + _bounds.width, 0, _bounds.y), new Vector3(_bounds.x + _bounds.width, 0, _bounds.y + _bounds.height), col);
            Debug.DrawLine(new Vector3(_bounds.x, 0, _bounds.y + _bounds.height), new Vector3(_bounds.x + _bounds.width, 0, _bounds.y + _bounds.height), col);
            if(subBranches && _cells[0] != null)
            {
                foreach(QuadTree<T> t in _cells)
                    if(t != null)
                        t.DrawDebugQuads(col);
            }
        }

        public void DrawDebugQuadsCross(Color col, bool subBranches = false)
        {
            Debug.DrawLine(new Vector3(_bounds.xMin, 0, _bounds.yMin), new Vector3(_bounds.xMax, 0, _bounds.yMax), col);
            Debug.DrawLine(new Vector3(_bounds.xMax, 0, _bounds.yMin), new Vector3(_bounds.xMin, 0, _bounds.yMax), col);
            if(subBranches && _cells[0] != null)
            {
                foreach(QuadTree<T> t in _cells)
                    if(t != null)
                        t.DrawDebugQuadsCross(col);
            }
        }

        public void DrawDebugItems(Color col, float time = 0, bool subBranches = true)
        {
            if(time == 0) time = Time.deltaTime;
            foreach(T obj in _subObjects)
                Debug.DrawLine(obj.positionV3, obj.positionV3 + Vector3.up * 50, new Color(1, 1, 0, 0.3f), time);

            if(subBranches && _cells[0] != null)
            {
                foreach(QuadTree<T> t in _cells)
                    if(t != null)
                        t.DrawDebugItems(col);
            }
        }
    }
}