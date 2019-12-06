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

namespace BuildR2
{
    public class BuildRCollider
    {
        public struct BBox
        {
            public Vector3 size;
            public Vector3 position;
            public Quaternion rotation;

            public BBox(Vector3 size, Vector3 position, Quaternion rotation)
            {
                this.size = size;
                this.position = position;
                this.rotation = rotation;
            }

            public BBox(Vector3 size, Vector3 position)
            {
                this.size = size;
                this.position = position;
                rotation = Quaternion.identity;
            }

            public BBox(Vector3 size)
            {
                this.size = size;
                position = Vector3.zero;//-size * 0.5f;
                rotation = Quaternion.identity;
            }

            public BBox(BBox clone)
            {
                size = clone.size;
                position = clone.position;
                rotation = clone.rotation;
            }
        }

        private string _name;
        private BuildRMesh _mesh;
        private List<BBox> _boxList;
        public float thickness = 0.2f;
        private bool _usePrimitives = false;

        public BuildRCollider(string newName)
        {
            _name = newName;
            _mesh = new BuildRMesh(newName);
            _boxList = new List<BBox>();
        }

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public BuildRMesh mesh
        {
            get {return _mesh;}
        }

        public List<BBox> boxList
        {
            get {return _boxList;}
        }

        public void TogglePrimitives(bool value)
        {
            _usePrimitives = value;
        }

        public bool usingPrimitives { get {return _usePrimitives;} }

        public void Build(Mesh mesh)
        {
            if(_mesh.vertexCount == 0)
                return;

            if (mesh == null)
            {
                Debug.LogError("Mesh sent is null - where is this guy?");
                return;
            }

            _mesh.Build(mesh);
        }

        public void Clear()
        {
            _mesh.Clear();
            _boxList.Clear();
        }


        public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if(_usePrimitives)
            {
                Vector3 baseVector = p1 - p0;
                Vector3 upVector = p2 - p0;
                Vector3 size = new Vector3(thickness, upVector.magnitude, baseVector.magnitude);
                Vector3 baseDirection = (baseVector).normalized;
                Vector3 position = p0 + (p3 - p0) * 0.5f + Vector3.Cross(Vector3.down, baseDirection) * thickness * 0.5f;
                AddPlane(size, position, baseDirection);
            }
            else
            {
                _mesh.AddPlaneBasic(p0,p1,p2,p3,0);
            }
        }

        public void AddPlane(Vector3 size, Vector3 position, Quaternion rotation)
        {
            if(_usePrimitives)
                _boxList.Add(new BBox(size, position, rotation));
            else
            {
                size = rotation * size;
                Vector3 p0 = position + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
                Vector3 p1 = position + new Vector3(size.x, -size.y, -size.z) * 0.5f;
                Vector3 p2 = position + new Vector3(-size.x, size.y, -size.z) * 0.5f;
                Vector3 p3 = position + new Vector3(size.x, size.y, -size.z) * 0.5f;
                _mesh.AddPlaneBasic(p0, p1, p2, p3, 0);
            }
        }

        public void AddPlane(Vector3 size, Vector3 position, Vector3 facadeDirection)
        {
            Quaternion rotation = Quaternion.LookRotation(facadeDirection);
            AddPlane(size, position, rotation);
        }

        public void AddPlane(Vector3 size, Vector3 position, float rotation)
        {
            Quaternion quaternion = Quaternion.Euler(0, rotation, 0);
            AddPlane(size, position, quaternion);
        }

        public void AddBBox(BBox box)
        {
            _boxList.Add(box);
        }

        public void AddBBox(BBox box, Vector3 transform, Quaternion rotation)
        {
            BBox newBox = new BBox(box);
            newBox.position = transform + rotation * newBox.position;
            newBox.rotation *= rotation;
            _boxList.Add(newBox);
        }

        public void AddBBox(BBox[] boxes)
        {
            _boxList.AddRange(boxes);
        }

        public void AddBBox(BBox[] boxes, Vector3 transform, Quaternion rotation)
        {
            int boxCount = boxes.Length;
            for(int b = 0; b < boxCount; b++)
                AddBBox(boxes[b], transform, rotation);
        }

        public void AddBBox(List<BBox> boxes)
        {
            _boxList.AddRange(boxes);
        }
    }
}