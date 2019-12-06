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
    [RequireComponent(typeof(BuildRPart))]
    public class ColliderPart : MonoBehaviour, IColliderPart
	{
        [SerializeField]
        private Mesh _mesh;

        [SerializeField]
        private ColliderPart _sibling;

        [SerializeField]
        private MeshCollider _meshCollider;

        [SerializeField]
        private GameObject[] _boxColliders = new GameObject[0];

        public Mesh mesh
        {
            get {return _mesh;}
        }

		public void Deactivate()
		{
			Clear();
		}

		public void Clear()
        {
            if (_mesh == null)
                _mesh = new Mesh();
            _mesh.Clear();
            if (_sibling != null)
                _sibling.DestroyVisual();
            if (_meshCollider != null)
                _meshCollider.sharedMesh = null;
            int boxCount = _boxColliders.Length;
            for (int b = 0; b < boxCount; b++)
            {
#if UNITY_EDITOR
                DestroyImmediate(_boxColliders[b]);
#else
            Destroy(_boxColliders[b]);
#endif
            }
            _boxColliders = new GameObject[0];
        }

        public void GenerateFromColliderMesh(BuildRCollider mesh)
        {
            //todo try to use old box colliders on regeneration
            int boxCount = _boxColliders.Length;
            for (int b = 0; b < boxCount; b++)
            {
#if UNITY_EDITOR
                DestroyImmediate(_boxColliders[b]);
#else
            Destroy(_boxColliders[b]);
#endif
            }

            //            if (mesh.mesh.vertexCount > 0)
            //            {
            _boxColliders = new GameObject[0];
            GenerateFromDynamicMesh(mesh.mesh);
            //            }
            //            else
            //            {
            List<BuildRCollider.BBox> boxes = mesh.boxList;
            int newBoxCount = boxes.Count;
            _boxColliders = new GameObject[newBoxCount];
            for (int b = 0; b < newBoxCount; b++)
            {
                GameObject newBoxCollider = new GameObject("box collider");
                newBoxCollider.transform.parent = transform;
                newBoxCollider.transform.localPosition = boxes[b].position;
                newBoxCollider.transform.localRotation = boxes[b].rotation;
#if UNITY_EDITOR
                UnityEditor.Undo.RegisterCreatedObjectUndo(newBoxCollider, "Created Box Collider");
                BoxCollider bColl = UnityEditor.Undo.AddComponent<BoxCollider>(newBoxCollider);
#else
                BoxCollider bColl = newBoxCollider.AddComponent<BoxCollider>();
#endif
                bColl.size = boxes[b].size;
                _boxColliders[b] = newBoxCollider;
            }
            //            }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void GenerateFromDynamicMesh(BuildRMesh mesh)
        {
            if (_mesh == null)
                _mesh = new Mesh();
            if (_meshCollider == null)
            {
#if UNITY_EDITOR
                _meshCollider = UnityEditor.Undo.AddComponent<MeshCollider>(gameObject);
#else
                _meshCollider = gameObject.AddComponent<MeshCollider>();
#endif
            }
            //            Debug.Log("GenerateFromDynamicMesh "+ _mesh.vertexCount+" "+_mesh.triangles.Length);
            mesh.Build(_mesh);
            _meshCollider.sharedMesh = _mesh;

            if (mesh.hasOverflowed)
            {
                if (_sibling == null)
                    _sibling = Create(transform.parent, name);
                _sibling.GenerateFromDynamicMesh(mesh.overflow);
            }
            else
            {
                if (_sibling != null)
                    _sibling.DestroyVisual();
                _sibling = null;
            }
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

        public static ColliderPart Create(Transform parent, string name = "Collider")
        {

#if UNITY_EDITOR
            GameObject go = new GameObject(name);
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Created Collider Part GameObject");
            UnityEditor.Undo.SetTransformParent(go.transform, parent, "Parent New Collider Part GameObject");

            BuildRPart part = go.GetComponent<BuildRPart>();
            if (part == null)
                part = UnityEditor.Undo.AddComponent<BuildRPart>(go);

            part.parent = parent.GetComponent<BuildRPart>();

            ColliderPart output = UnityEditor.Undo.AddComponent<ColliderPart>(go);

            return output;
#else
            GameObject go = new GameObject(name);
            go.transform.parent = parent;

            BuildRPart part = go.GetComponent<BuildRPart>();
            if (part == null)
                part = go.AddComponent<BuildRPart>();

            part.parent = parent.GetComponent<BuildRPart>();

            ColliderPart output = go.AddComponent<ColliderPart>();

            return output;
#endif
        }
    }
}
