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
    public class RoomStyle : ScriptableObject
    {
        [SerializeField]
        private Surface _floorSurface;
        [SerializeField]
        private Surface _wallSurface;
        [SerializeField]
        private Surface _ceilingSurface;

        private Surface[] _usedSurfaces;
        private Mesh _previewMesh;
        private bool _hasBlankSurfaces = false;

        public Surface floorSurface
        {
            get { return _floorSurface; }
            set
            {
                if (_floorSurface != value)
                {
                    _floorSurface = value;
                    MarkModified();
                }
            }
        }

        public Surface wallSurface
        {
            get { return _wallSurface; }
            set
            {
                if (_wallSurface != value)
                {
                    _wallSurface = value;
                    MarkModified();
                }
            }
        }

        public Surface ceilingSurface
        {
            get { return _ceilingSurface; }
            set
            {
                if (_ceilingSurface != value)
                {
                    _ceilingSurface = value;
                    MarkModified();
                }
            }
        }

        public void MarkModified()
        {
            GenereateData();
            SaveData();
        }

        public Surface[] usedSurfaces
        {
            get { return _usedSurfaces; }
        }

        public bool hasBlankSurfaces
        {
            get
            {
                return _hasBlankSurfaces;
            }
        }

        private void OnEnable()
        {
            GenereateData();
        }

        private void GenereateData()
        {
            UpdateUsedSurfaces();
        }

        private void SaveData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            //tell everyone the good news...
            Building[] buildings = FindObjectsOfType<Building>();
            foreach (Building building in buildings)
            {
                UnityEditor.Undo.RecordObject(building, "room styl modification");
                building.MarkModified();
            }
#endif
        }

        public void UpdateUsedSurfaces()
        {
            _hasBlankSurfaces = false;
            List<Surface> surfaces = new List<Surface>();
            
            if (_floorSurface != null)
                surfaces.Add(_floorSurface);
            else
                _hasBlankSurfaces = true;

            if (_wallSurface != null)
                surfaces.Add(_wallSurface);
            else
                _hasBlankSurfaces = true;

            if (_ceilingSurface != null)
                surfaces.Add(_ceilingSurface);
            else
                _hasBlankSurfaces = true;

            _usedSurfaces = surfaces.ToArray();
        }

        #region statics
        public static RoomStyle CreateRoomStyle()
        {
            RoomStyle roomStyle = CreateInstance<RoomStyle>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(roomStyle, AssetCreator.GeneratePath("newRoomStyle.asset", "RoomStyles"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return roomStyle;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Room Style", false, ToolsMenuLevels.CREATE_ROOM_STYLE)]
        private static RoomStyle MenuCreateNewRoomStyle()
        {
            RoomStyle output = CreateRoomStyle();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Room Style", false, ToolsMenuLevels.CREATE_ROOM_STYLE)]
        private static RoomStyle MenuCreateNewRoomStyleB()
        {
            RoomStyle output = CreateRoomStyle();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        private static bool isRoomStyle()
        {
            if (UnityEditor.Selection.activeObject == null) return false;
            return UnityEditor.Selection.activeObject.GetType() == typeof(RoomStyle);
        }
#endif
        #endregion
    }
}