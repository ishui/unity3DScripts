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
using UnityEngine;

namespace BuildR2
{
    public class Portal : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Serialisation

        public List<SerializableDivision> serializedNodes = new List<SerializableDivision>();

        public void OnBeforeSerialize()
        {
            //ignore calls - serialisation handled internally when data has changed
        }

        public void OnAfterDeserialize()
        {
            if (serializedNodes.Count > 0)
                _root = ReadNodeFromSerializedNodes(0);
            else
                _root = new Division();
        }

        public void Serialise()
        {
            serializedNodes.Clear();
            AddNodeToSerializedNodes(root);
        }

        void AddNodeToSerializedNodes(Division n)
        {
            var serializedNode = new SerializableDivision()
            {
                name = n.name,
                frame = n.frame,
                recess = n.recess,
                size = n.size,
                type = n.type,
                divisionType = n.divisionType,
                surface = n.surface,
                identicalChildren = n.identicalChildren,
                identicalChildCount = n.identicalChildCount,
                expanded = n.expanded,
                childCount = n.children.Count,
                indexOfFirstChild = serializedNodes.Count + 1
            };

            serializedNodes.Add(serializedNode);
            foreach (var child in n.children)
                AddNodeToSerializedNodes(child);
        }

        Division ReadNodeFromSerializedNodes(int index)
        {
            var serializedNode = serializedNodes[index];
            var children = new List<Division>();
            for (int i = 0; i != serializedNode.childCount; i++)
                children.Add(ReadNodeFromSerializedNodes(serializedNode.indexOfFirstChild + i));

            return new Division()
            {
                name = serializedNode.name,
                frame = serializedNode.frame,
                recess = serializedNode.recess,
                size = serializedNode.size,
                type = serializedNode.type,
                divisionType = serializedNode.divisionType,
                surface = serializedNode.surface,
                expanded = serializedNode.expanded,
                identicalChildren = serializedNode.identicalChildren,
                identicalChildCount = serializedNode.identicalChildCount,
                children = children
            };
        }
        #endregion

        public enum ShapeTypes
        {
            Square,
            Circle,
            Semicircle,
            Arched
        }

        public enum DivisionTypes
        {
            Horizontal,
            Vertical
        }

        public enum Types
        {
            Window,
            Door
        }

        public Types type = Types.Window;

        [SerializeField]
        private Surface _defaultFrameTexture;

        [SerializeField]
        private Surface _defaultPanelTexture;

        private bool _modified = false;

        public Surface defaultFrameTexture
        {
            get { return _defaultFrameTexture; }
            set
            {
                if (value != _defaultFrameTexture)
                {
                    _defaultFrameTexture = value;
                    MarkModified();
                }
            }
        }

        public Surface defaultPanelTexture
        {
            get { return _defaultPanelTexture; }
            set
            {
                if (value != _defaultPanelTexture)
                {
                    _defaultPanelTexture = value;
                    MarkModified();
                }
            }
        }

        Division _root = new Division();

        public Division root
        {
            get { return _root; }
        }

        public void MarkModified()
        {
            _modified = true;
        }

        public bool CheckModification()
        {
            List<Division> treeNodes = new List<Division>();
            treeNodes.Add(root);
            while (treeNodes.Count > 0)
            {
                if (!_modified && treeNodes[0].modified) _modified = true;
                treeNodes[0].MarkUnmodified();
                treeNodes.AddRange(treeNodes[0].children);
                treeNodes.RemoveAt(0);
            }
            bool output = _modified;
            _modified = false;
            if (output)
            {
                GenereateData();
                SaveData();
            }
            return output;
        }

        public void Remove(Division division)
        {
            if (division == root) return;//cannot delete the root man!!!
            List<Division> treeNodes = new List<Division>();
            treeNodes.Add(root);
            while (treeNodes.Count > 0)
            {
                //                if (!_modified && treeNodes[0].modified) _modified = true;
                //                treeNodes[0].MarkUnmodified();
                int childcount = treeNodes[0].children.Count;
                for (int c = 0; c < childcount; c++)
                {
                    if (treeNodes[0].children[c] == division)
                    {
                        treeNodes[0].children.RemoveAt(c);
                        MarkModified();
                        CheckModification();
                        return;
                    }
                }
                treeNodes.AddRange(treeNodes[0].children);
                treeNodes.RemoveAt(0);
            }
        }

//        [SerializeField]
//        private byte[] _textureBytes = null;
//        private Texture2D _previewTexture;
//        private Surface[] _usedSurfaces;
        private Mesh _previewMesh;
//        private bool _hasBlankSurfaces = false;


//        public Texture2D previewTexture
//        {
//            get { return _previewTexture; }
//        }

//        public Surface[] usedSurfaces
//        {
//            get { return _usedSurfaces; }
//        }

        public Mesh previewMesh
        {
            get
            {
                if (_previewMesh == null)
                    UpdatePreviewMesh();
                return _previewMesh;
            }
        }

        private void GenereateData()
        {
            UpdatePreviewTexture();
            LoadPreviewTexture();
            UpdatePreviewMesh();
        }

        private void SaveData()
        {
#if UNITY_EDITOR
            Serialise();
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void UpdatePreviewTexture()
        {
            //TODO
            //            float max = Mathf.Max(_openingWidthAbs, _openingHeightAbs);
            //            Vector2 size = new Vector2(max * 2, max * 2);
            //            if (_wallSurface == null && _ceilingSurface == null && _sillSurface == null && _openingSurface == null)
            //                _previewTexture = new Texture2D(1, 1);
            //            else
            //                WallSectionGenerator.Texture(out _previewTexture, this, size);
            //
            //            _textureBytes = _previewTexture.EncodeToPNG();
        }

        public void LoadPreviewTexture()
        {
            //            _previewTexture.LoadImage(_textureBytes); TODO
        }
        
		public List<Surface> UsedSurfaces()
		{
			List<Surface> usedSurfaces = new List<Surface>();
			List<Division> treeNodes = new List<Division>();
			treeNodes.Add(root);
			bool blankSurfaces = false;
			while (treeNodes.Count > 0)
			{
				Division current = treeNodes[0];
				if (current.surface != null && !usedSurfaces.Contains(current.surface))
					usedSurfaces.Add(current.surface);
				else
					blankSurfaces = true;

				treeNodes.AddRange(treeNodes[0].children);
				treeNodes.RemoveAt(0);
			}

			if (blankSurfaces)
			{
				if (_defaultFrameTexture != null && !usedSurfaces.Contains(_defaultFrameTexture))
					usedSurfaces.Insert(0, _defaultFrameTexture);
				if (_defaultPanelTexture != null && _defaultPanelTexture != _defaultFrameTexture && !usedSurfaces.Contains(_defaultPanelTexture))
					usedSurfaces.Insert(0, _defaultPanelTexture);
			}

			return usedSurfaces;
		}

		public void UpdatePreviewMesh(SubmeshLibrary submeshLibrary = null)
        {
            if (_previewMesh == null)
                _previewMesh = new Mesh();
            _previewMesh.name = string.Format("{0}_Preview_Mesh", name);
            Vector2 size = new Vector2(2, 2);
            PortalGenerator.Generate(this, ref _previewMesh, size, submeshLibrary);
        }

        #region statics
        public static Portal CreateWindow()
        {
            Portal newPortalInstance = CreateInstance<Portal>();
            newPortalInstance.type = Types.Window;
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(newPortalInstance, AssetCreator.GeneratePath("newWindow.asset", "Windows and Doors"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return newPortalInstance;
        }

        public static Portal CreateDoor()
        {
            Portal newPortalInstance = CreateInstance<Portal>();
            newPortalInstance.type = Types.Door;
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(newPortalInstance, AssetCreator.GeneratePath("newDoor.asset", "Windows and Doors"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return newPortalInstance;
        }

#if UNITY_EDITOR
 
        [UnityEditor.MenuItem("Tools/BuildR/Create New Window", false, ToolsMenuLevels.CREATE_WINDOW)]
        private static Portal MenuCreateNewWindow()
        {
            Portal output = CreateWindow();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
        [UnityEditor.MenuItem("Tools/BuildR/Create New Door", false, ToolsMenuLevels.CREATE_DOOR)]
        private static Portal MenuCreateNewDoor()
        {
            Portal output = CreateDoor();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Window", false, ToolsMenuLevels.CREATE_WINDOW)]
        private static Portal MenuCreateNewWindowB()
        {
            Portal output = CreateWindow();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Door", false, ToolsMenuLevels.CREATE_DOOR)]
        private static Portal MenuCreateNewDoorB()
        {
            Portal output = CreateDoor();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }

    public class Division
    {
        public static string NO_NAME = "Unnamed Division";

        public List<Division> children = new List<Division>();

        private string _name = NO_NAME;//frame of division (meters)
        private float _frame = 0.05f;//frame of division (meters)
        private float _recess = 0.01f;//recess of inner (meters)
        private float _size = 1.0f;//ratio of division used against sibling divisions
        private Portal.ShapeTypes _type;
        private Portal.DivisionTypes _divisionType;
        private Surface _surface;
        private bool _identicalChildren;
        private int _identicalChildCount = 1;
        private bool _modified;

        //editor value
        public bool expanded = false;

        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    MarkModified();
                }
            }
        }

        public float frame
        {
            get
            {
                return _frame;
            }
            set
            {
                if (_frame != value)
                {
                    _frame = value;
                    MarkModified();
                }
            }
        }

        public float recess
        {
            get
            {
                return _recess;
            }
            set
            {
                if (_recess != value)
                {
                    _recess = value;
                    MarkModified();
                }
            }
        }

        public float size
        {
            get
            {
                return _size;
            }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    MarkModified();
                }
            }
        }

        public Portal.ShapeTypes type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    MarkModified();
                }
            }
        }

        public Surface surface
        {
            get
            {
                return _surface;
            }
            set
            {
                if (_surface != value)
                {
                    _surface = value;
                    MarkModified();
                }
            }
        }

        public bool identicalChildren
        {
            get
            {
                return _identicalChildren;
            }
            set
            {
                if (_identicalChildren != value)
                {
                    _identicalChildren = value;
                    if (!hasChildren && _identicalChildren)
                        children.Add(new Division());
                    MarkModified();
                }
            }
        }

        public int identicalChildCount
        {
            get { return _identicalChildCount; }
            set
            {
                if (_identicalChildCount != value)
                {
                    _identicalChildCount = Mathf.Max(value, 1);
                    MarkModified();
                }
            }
        }

        public Portal.DivisionTypes divisionType
        {
            get { return _divisionType; }
            set
            {
                if (_divisionType != value)
                {
                    _divisionType = value;
                    MarkModified();
                }
            }
        }

        public bool hasChildren
        {
            get { return children.Count > 0; }
        }

        public List<Division> GetChildren
        {
            get
            {
                if (!_identicalChildren)
                    return children;
                else
                {
                    List<Division> output = new List<Division>();
                    for (int i = 0; i < _identicalChildCount; i++)
                        output.Add(children[0]);
                    return output;
                }
            }
        }

        public void MarkModified()
        {
            _modified = true;
        }

        public void MarkUnmodified()
        {
            _modified = false;
        }

        public bool modified
        {
            get { return _modified; }
        }
    }

    //class that we will use for serialization
    [Serializable]
    public struct SerializableDivision
    {
        public string name;//frame of division (meters)
        public float frame;//frame of division (meters)
        public float recess;//recess of inner (meters)
        public float size;//ratio of division used against sibling divisions
        public Portal.ShapeTypes type;
        public Portal.DivisionTypes divisionType;
        public Surface surface;
        public bool identicalChildren;
        public int identicalChildCount;

        public int childCount;
        public int indexOfFirstChild;

        //editor value
        public bool expanded;
    }
}