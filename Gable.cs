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
    public class Gable : ScriptableObject
    {
        public List<GablePart> _parts = new List<GablePart>();
        //        [SerializeField]
        //        private float _thickness = 0.3f;
        //        [SerializeField]
        //        private float _additionalHeight = 0.2f;
        [SerializeField]
        private Surface _surface;
        [SerializeField]
        private int _segments = 12;

        private Mesh _previewMesh;
        private BuildRSettings _settings;

        public GablePart this[int index]
        {
            get { return _parts[index]; }
            set
            {
                _parts[index] = value;
                if (_parts[index].isModified)
                    MarkModified();
            }
        }

        public int count { get { return _parts.Count; } }

        public GablePart AddNewPart()
        {
            GablePart output = new GablePart(GablePart.Types.Diagonal);
            output.size = Vector2.one;
            _parts.Add(output);
            MarkModified();
            return output;
        }

        public GablePart InsertNewPart(int index)
        {
            GablePart output = new GablePart(GablePart.Types.Diagonal);
            output.size = Vector2.one;
            _parts.Insert(index, output);
            MarkModified();
            return output;
        }

        public void RemovePart(GablePart part)
        {
            _parts.Remove(part);
            MarkModified();
        }

        public void RemovePartAt(int index)
        {
            _parts.RemoveAt(index);
            MarkModified();
        }
        
        public Surface surface
        {
            get { return _surface; }
            set
            {
                if (_surface != value)
                {
                    _surface = value;
                    MarkModified();
                }
            }
        }

        public int segments
        {
            get { return _segments; }
            set
            {
                if (_segments != value)
                {
                    _segments = value;
                    MarkModified();
                }
            }
        }


        public Mesh previewMesh
        {
            get
            {
                if (_previewMesh == null)
                    UpdatePreviewMesh();
                return _previewMesh;
            }
        }

        private void UpdatePreviewMesh()
        {
            string meshName = string.Format("{0}_Preview_Mesh", name);
            BuildRMesh pBMesh = new BuildRMesh(meshName);
            if (_previewMesh == null)
                _previewMesh = new Mesh();
            _previewMesh.name = meshName;
            if (_settings == null) _settings = BuildRSettings.GetSettings();
            Vector3 left = new Vector3(-_settings.previewGableWidth * 0.5f, -_settings.previewGableHeight * 0.5f, 0);
            Vector3 right = new Vector3(_settings.previewGableWidth * 0.5f, -_settings.previewGableHeight * 0.5f, 0);
            GableGenerator.Generate(ref pBMesh, this, left, right, _settings.previewGableHeight, _settings.previewGableThickness, new Vector2());
            pBMesh.Build(_previewMesh);
        }

        public void MarkModified()
        {
#if UNITY_EDITOR
            //tell everyone the good news...
            Building[] buildings = FindObjectsOfType<Building>();
            foreach (Building building in buildings)
            {
                UnityEditor.Undo.RecordObject(building, "gable modification");
                building.MarkModified();
            }

            UpdatePreviewMesh();
#endif

            SaveData();
        }
        
        private void SaveData()
        {
#if UNITY_EDITOR
            int partCount = _parts.Count;
            for (int i = 0; i < partCount; i++)
                _parts[i].MarkUnmodified();

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        #region statics
        public static Gable CreateGable()
        {
            Gable output = CreateInstance<Gable>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(output, AssetCreator.GeneratePath("newGable.asset", "Gables"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return output;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Gable Design", false, 60)]
        private static Gable MenuCreateNewGable()
        {
            Gable output = CreateGable();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Gable Design", false, 60)]
        private static Gable MenuCreateNewGableB()
        {
            Gable output = CreateGable();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }

    [Serializable]
    public class GablePart
    {
        public enum Types
        {
            Horizonal,
            Vertical,
            Diagonal,
            Concave,
            Convex
        }

        public Types _type;
        public Vector2 _size;
        public bool _modified;

        public GablePart(Types type = Types.Diagonal)
        {
            _type = type;
            _size = Vector2.one;
            _modified = false;
        }

        public Types type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    _modified = true;
                }
            }
        }

        public Vector2 size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    _modified = true;
                }
            }
        }

        public Vector2 GetSize()
        {
            Vector2 output = _size;
            if (_type == Types.Horizonal) output.y = 0;
            if (_type == Types.Vertical) output.x = 0;
            return output;
        }

        public bool isModified
        {
            get { return _modified; }
        }

        public void MarkModified()
        {
            _modified = true;
        }

        public void MarkUnmodified()
        {
            _modified = false;
        }
    }
}