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
    [RequireComponent(typeof(BuildRPart))]
    public class VisualPart : MonoBehaviour, IVisualPart
    {
        private const string DYNAMIC_MESH_NAME = "BuildR Mesh";
        private const string DYNAMIC_COLLIDER_NAME = "BuildR Collider";

        private BuildRMesh _dynamicMesh;
        private BuildRCollider _colliderMesh;
        [SerializeField]
        private Mesh _mesh;

        [SerializeField]
        private MeshFilter _filter;
        [SerializeField]
        private MeshRenderer _renderer;

        [SerializeField]
        private VisualPart _sibling;

        [SerializeField]
        private ColliderPart _colliderPart;

        public Mesh mesh { get { return _mesh; } }

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
                _sibling.DestroyVisual();

            if (_colliderPart == null)
                _colliderPart = ColliderPart.Create(transform.parent);
            _colliderPart.Clear();
        }

        public void GenerateFromDynamicMesh(BuildRMesh overflow = null)
        {
            Debug.Log("VisualPart.cs GenerateFromDynamicMesh ："+overflow);
            HUtils.log();

            if (_dynamicMesh == null)
                _dynamicMesh = new BuildRMesh(DYNAMIC_MESH_NAME);
            if (_mesh == null)
                _mesh = new Mesh();
            if (_filter == null)
            {
#if UNITY_EDITOR
                _filter = UnityEditor.Undo.AddComponent<MeshFilter>(gameObject);
#else
                _filter = gameObject.AddComponent<MeshFilter>();
#endif
            }
            if (_renderer == null)
            {
#if UNITY_EDITOR
                _renderer = UnityEditor.Undo.AddComponent<MeshRenderer>(gameObject);
#else
                _renderer = gameObject.AddComponent<MeshRenderer>();
#endif
            }
            if (overflow != null)
                _dynamicMesh = overflow;
            _dynamicMesh.Build(_mesh);
            _filter.sharedMesh = _mesh;
            _renderer.sharedMaterials = _dynamicMesh.materials.ToArray();


            if (_dynamicMesh.hasOverflowed)
            {
                if (_sibling == null)
                    _sibling = Create(transform.parent, name);
                _sibling.GenerateFromDynamicMesh(_dynamicMesh.overflow);
            }
            else
            {
                if (_sibling != null)
                    _sibling.DestroyVisual();
                _sibling = null;
            }

            if (_colliderPart == null)
                _colliderPart = ColliderPart.Create(transform.parent);

            if (_colliderMesh == null)
                _colliderMesh = new BuildRCollider(DYNAMIC_COLLIDER_NAME);
            _colliderPart.GenerateFromColliderMesh(_colliderMesh);
        }

        public void DestroyVisual()
        {
            if (_sibling != null) _sibling.DestroyVisual();
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
        }

        public void UpdateMeshFilter() { _filter.sharedMesh = _mesh; }

        public Material material
        {
            set
            {
                if (_renderer == null)
                {
#if UNITY_EDITOR
                    _renderer = UnityEditor.Undo.AddComponent<MeshRenderer>(gameObject);
#else
                _renderer = gameObject.AddComponent<MeshRenderer>();
#endif
                }
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
                {
#if UNITY_EDITOR
                    _renderer = UnityEditor.Undo.AddComponent<MeshRenderer>(gameObject);
#else
                _renderer = gameObject.AddComponent<MeshRenderer>();
#endif
                }

                //                setMaterials = value;
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
            HUtils.log();
            
        }

        public static VisualPart Create(Transform parent, string name = "visual part")
        {
            HUtils.log();

#if UNITY_EDITOR
            GameObject go = new GameObject(name);
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created Floorplan GameObject");
            UnityEditor.Undo.SetTransformParent(go.transform, parent, "Parent New Floorplan GameObject");
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            BuildRPart part = go.GetComponent<BuildRPart>();
            if (part == null)
                part = UnityEditor.Undo.AddComponent<BuildRPart>(go);

            part.parent = parent.GetComponent<BuildRPart>();

            VisualPart output = UnityEditor.Undo.AddComponent<VisualPart>(go);

            return output;
#else
            GameObject go = new GameObject(name);
            go.transform.parent = parent;

            BuildRPart part = go.GetComponent<BuildRPart>();
            if (part == null)
                part = go.AddComponent<BuildRPart>();

            part.parent = parent.GetComponent<BuildRPart>();

            VisualPart output = go.AddComponent<VisualPart>();

            return output;
#endif
        }
    }
}