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

namespace BuildR2
{
    public class ColliderPartRuntime : MonoBehaviour, IColliderPart
	{
        private Mesh _mesh;
		private ColliderPartRuntime _sibling;
		private MeshCollider _meshCollider;
        private GameObject[] _boxColliders = new GameObject[0];

        public Mesh mesh
        {
            get {return _mesh;}
		}

	    public void Deactivate()
	    {
		    _meshCollider.sharedMesh = null;
		    for(int i = 0; i < _boxColliders.Length; i++)
				Destroy(_boxColliders[i]);

		    _mesh = null;
		    _mesh.Clear(false);

		    if (_sibling != null)
		    {
			    _sibling.Deactivate();
			    _sibling = null;
		    }

		    ColliderPartRuntimePool.Instance.Push(this);
	    }

		public void Clear()
        {
            if (_mesh == null)
                _mesh = new Mesh();
            _mesh.Clear();
            if (_sibling != null)
	            _sibling.Deactivate();
			if (_meshCollider != null)
                _meshCollider.sharedMesh = null;
            int boxCount = _boxColliders.Length;
            for (int b = 0; b < boxCount; b++)
				Destroy(_boxColliders[b]);
            _boxColliders = new GameObject[0];
        }

        public void GenerateFromColliderMesh(BuildRCollider mesh)
        {
            //todo try to use old box colliders on regeneration
            int boxCount = _boxColliders.Length;
            for (int b = 0; b < boxCount; b++)
				Destroy(_boxColliders[b]);
			
            _boxColliders = new GameObject[0];
            GenerateFromDynamicMesh(mesh.mesh);

            List<BuildRCollider.BBox> boxes = mesh.boxList;
            int newBoxCount = boxes.Count;
            _boxColliders = new GameObject[newBoxCount];
            for (int b = 0; b < newBoxCount; b++)
            {
                GameObject newBoxCollider = new GameObject("box collider");
                newBoxCollider.transform.parent = transform;
                newBoxCollider.transform.localPosition = boxes[b].position;
                newBoxCollider.transform.localRotation = boxes[b].rotation;
                BoxCollider bColl = newBoxCollider.AddComponent<BoxCollider>();
                bColl.size = boxes[b].size;
                _boxColliders[b] = newBoxCollider;
            }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void GenerateFromDynamicMesh(BuildRMesh mesh)
        {
            if (_mesh == null)
                _mesh = new Mesh();
            if (_meshCollider == null)
                _meshCollider = gameObject.AddComponent<MeshCollider>();

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
	                _sibling.Deactivate();
				_sibling = null;
            }
        }
		
	    public static ColliderPartRuntime Create(Transform parent = null, string name = "visual part")
	    {
		    GameObject go = new GameObject(name);
		    if (parent != null)
			    go.transform.parent = parent;
		    ColliderPartRuntime output = go.AddComponent<ColliderPartRuntime>();
		    return output;
	    }

	    public static ColliderPartRuntime GetPoolItem()
	    {
		    return ColliderPartRuntimePool.Instance.Pull();
	    }
	}
}
