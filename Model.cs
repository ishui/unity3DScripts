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
    public class Model : ScriptableObject
    {
        public enum Types
        {
            Prefab,
            Mesh
        }

        [SerializeField]
        private Types _type = Types.Prefab;
        [SerializeField]
        private GameObject _subject;
        [SerializeField]
        private Vector3 _anchor = Vector3.zero;
        [SerializeField]
        private Vector3 _normal = Vector3.up;
        [SerializeField]
        private Bounds _modelbounds = new Bounds(Vector3.zero, Vector3.zero);
		[SerializeField]
		private Bounds _userbounds = new Bounds(Vector3.zero, Vector3.zero);
		[SerializeField]
	    private Vector3 _userRotation = Vector3.zero;
		[SerializeField]
        private Mesh[] _meshes = new Mesh[0];
        [SerializeField]
        private MaterialArray[] _materials = new MaterialArray[0];

        [Serializable]
        public struct MaterialArray
        {
            public Material[] materials;
            
            public MaterialArray(Material[] materials = null)
            {
                if(materials != null)
                    this.materials = materials;
                else
                    this.materials = new Material[0];
            }

            public static Material[][] ToArray(MaterialArray[] marray) {
                int count = marray.Length;
                Material[][] output = new Material[count][];
                for(int m = 0; m < count; m++) {
                    MaterialArray array = marray[m];
                    int countB = array.materials.Length;
                    output[m] = new Material[countB];
                    for(int mx = 0; mx < countB; mx++) {
                        output[m][mx] = array.materials[mx];
                    }
                }
                return output;
            }

        }

        public Types type
        {
            get { return _type; }
            set
            {
                if (value != _type)
                {
                    _type = value;
                    MarkModified();
                }
            }
        }

        public GameObject subject
        {
            get { return _subject; }
            set
            {
                if (value != _subject)
                {
                    _subject = value;
                    MarkModified();
                }
            }
        }

        public Vector3 anchor
        {
            get { return _anchor; }
            set
            {
                if (value != _anchor)
                {
                    _anchor = value;
                    MarkModified();
                }
            }
        }

        public Vector3 normal
        {
            get { return _normal; }
            set
            {
                if (value != _normal)
                {
                    _normal = value;
                    MarkModified();
                }
            }
        }

        public Bounds modelBounds
        {
            get { return _modelbounds; }
		}

		public Bounds userBounds
		{
			get { return _userbounds; }
			set
			{
				if (value != _userbounds)
				{
					_userbounds = value;
					MarkModified();
				}
			}
		}

		public Vector3 userRotation
		{
			get { return _userRotation; }
			set
			{
				if (value != _userRotation)
				{
					_userRotation = value;
					MarkModified();
				}
			}
		}

		public Quaternion userRotationQuat
		{
			get { return Quaternion.Euler(_userRotation); }
		}

		public void MarkModified()
        {
            UpdateInternalData();
        }

        public void UpdateInternalData()
        {
            if(_subject == null)
                return;
            _modelbounds.center = Vector3.zero;
            _modelbounds.size = Vector3.zero;
            Vector3 subjectScale = _subject.transform.localScale;
            MeshFilter[] meshFilters = _subject.GetComponentsInChildren<MeshFilter>();
            int meshCount = meshFilters.Length;
//            Debug.Log(meshCount);
            _meshes = new Mesh[meshCount];
            _materials = new MaterialArray[meshCount];
	        Vector3 offset = _subject.transform.position;
            for(int m = 0; m < meshCount; m++)
            {
                _meshes[m] = meshFilters[m].sharedMesh;
                MeshRenderer rend = meshFilters[m].gameObject.GetComponent<MeshRenderer>();
//	            Debug.Log("UpdateInternalData "+rend);
                if(rend != null)
                {
                    _modelbounds.Encapsulate(rend.bounds);
//					Debug.Log("UpdateInternalData " + rend.bounds);
					Material[] mats = rend.sharedMaterials;
//					Debug.Log("UpdateInternalData " + rend.sharedMaterials);
					int rendMatCount = mats.Length;
                    _materials[m] = new MaterialArray(new Material[rendMatCount]);
                    for (int mt = 0; mt < rendMatCount; mt++)
                        _materials[m].materials[mt] = mats[mt];
                }
                else
                {
                    _materials[m] = new MaterialArray();
                }
            }

			_modelbounds.size = Vector3.Scale(_modelbounds.size, subjectScale);
			_modelbounds.center += -offset;

			if (_userbounds.size.magnitude < Mathf.Epsilon)
                _userbounds.Encapsulate(_modelbounds);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public Mesh[] GetMeshes()
        {
            return _meshes;
        }

        public MaterialArray[] GetMaterials()
        {
            return _materials;
        }

        #region statics
        public static Model CreateModel()
        {
            Model newModelInstance = CreateInstance<Model>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(newModelInstance, AssetCreator.GeneratePath("newModel.asset", "Models"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return newModelInstance;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Model", false, ToolsMenuLevels.CREATE_MODEL)]
        private static Model MenuCreateNewWindow()
        {
            Model output = CreateModel();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Model", false, ToolsMenuLevels.CREATE_MODEL)]
        private static Model MenuCreateNewWindowB()
        {
            Model output = CreateModel();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }
}