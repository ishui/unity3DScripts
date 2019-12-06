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

namespace BuildR2
{
    public class VisualPartRuntime : MonoBehaviour, IVisualPart
    {
        private const string DYNAMIC_MESH_NAME = "BuildR Mesh";
        private const string DYNAMIC_COLLIDER_NAME = "BuildR Collider";

        private BuildRMesh _dynamicMesh;
        private BuildRCollider _colliderMesh;
        private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private VisualPartRuntime _sibling;
        private ColliderPartRuntime _colliderPart;

        public Mesh mesh { get { return _mesh; } }

        public void Init()
        {
            HUtils.log();

            if (_mesh == null)
                _mesh = new Mesh();
            if (_filter == null)
                _filter = gameObject.AddComponent<MeshFilter>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<MeshRenderer>();
            if (_dynamicMesh == null)
                _dynamicMesh = new BuildRMesh(DYNAMIC_MESH_NAME);
            if (_colliderMesh == null)
                _colliderMesh = new BuildRCollider(DYNAMIC_COLLIDER_NAME);
        }

        public void Activate()
        {
            HUtils.log();

            if (_renderer == null)
                _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.enabled = true;
            if (_sibling != null)
                _sibling.Activate();
        }

        public void Deactivate()
        {
            HUtils.log();

            _filter.sharedMesh = null;
            _renderer.sharedMaterials = new Material[0];
            _renderer.enabled = false;

            _mesh.Clear(false);
            _mesh = null;

            if (_sibling != null)
            {
                _sibling.Deactivate();
                VisualPartRuntimePool.Instance.Push(_sibling);
                _sibling = null;
            }
        }

        public BuildRMesh dynamicMesh
        {
            get
            {
                if (_dynamicMesh == null)
                    _dynamicMesh = new BuildRMesh(DYNAMIC_MESH_NAME);
                return _dynamicMesh;
            }
        }

        public BuildRCollider colliderMesh
        {
            
            get
            {
                if (_colliderMesh == null)
                    _colliderMesh = new BuildRCollider(DYNAMIC_COLLIDER_NAME);
                return _colliderMesh;
            }
        }

        public IColliderPart colliderPart
        {
            get { return _colliderPart; }
        }

        public void Clear()
        {
            if (_mesh == null)
                _mesh = new Mesh();
            _mesh.Clear();
            if (_sibling != null)
            {
                _sibling.Deactivate();
                VisualPartRuntimePool.Instance.Push(_sibling);
            }
            _colliderMesh.Clear();

            if (_colliderPart == null)
                _colliderPart = ColliderPartRuntime.GetPoolItem();
            _colliderPart.Clear();
        }

        public void GenerateFromDynamicMesh(BuildRMesh overflow = null)
        {
            HUtils.log();

            if (_dynamicMesh.vertexCount == 0)
                return;
            if (_mesh == null)
                _mesh = new Mesh();
            if (_filter == null)
                _filter = gameObject.AddComponent<MeshFilter>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<MeshRenderer>();
            if (overflow != null)
                _dynamicMesh = overflow;
            _dynamicMesh.Build(_mesh);
            _filter.sharedMesh = _mesh;
            _renderer.sharedMaterials = _dynamicMesh.materials.ToArray();


            if (_dynamicMesh.hasOverflowed)
            {
                if (_sibling == null)
                    _sibling = GetPoolItem();
                _sibling.GenerateFromDynamicMesh(_dynamicMesh.overflow);
            }
            else
            {
                if (_sibling != null)
                {
                    _sibling.Deactivate();
                    VisualPartRuntimePool.Instance.Push(_sibling);
                }
                _sibling = null;
            }

            if (_colliderPart == null)
            {
                _colliderPart = ColliderPartRuntime.GetPoolItem();
                _colliderPart.GenerateFromColliderMesh(_colliderMesh);
            }
        }

        public void UpdateMeshFilter() { _filter.sharedMesh = _mesh; }

        public Material material
        {
            set
            {
                if (_renderer == null)
                    _renderer = gameObject.AddComponent<MeshRenderer>();
                _renderer.sharedMaterial = value;
            }
        }

        public Material[] materials
        {
            get
            {
                if (_renderer == null)
                    return new Material[0];
                else
                    return _renderer.sharedMaterials;
            }
            set
            {
                if (_renderer == null)
                    _renderer = gameObject.AddComponent<MeshRenderer>();
                if (_sibling != null)
                    _sibling.materials = value;
                _renderer.sharedMaterials = _dynamicMesh.materials.ToArray();


            }
        }

        public void Move(Vector3 by)
        {
            transform.position += by;
            if (_sibling != null)
                _sibling.Move(by);
        }

        public void Place(Vector3 position)
        {
            transform.position = position;
            if (_sibling != null)
                _sibling.Move(position);
        }

        public static VisualPartRuntime Create(Transform parent = null, string name = "visual part")
        {
            HUtils.log();

            GameObject go = new GameObject(name);
            if (parent != null)
                go.transform.parent = parent;
            VisualPartRuntime output = go.AddComponent<VisualPartRuntime>();
            output.Init();
            output.Deactivate();
            return output;
        }

        public static VisualPartRuntime GetPoolItem()
        {
            HUtils.log();

            return VisualPartRuntimePool.Instance.Pull();
        }
    }
}