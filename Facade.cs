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
using System.IO;
using UnityEngine;

namespace BuildR2
{
    public class Facade : ScriptableObject, ICustomIconAsset
    {
        [SerializeField]
        private List<FacadeRow> _basePattern = new List<FacadeRow>();
        [SerializeField]
        private bool _hasGroundFloorPattern = false;
        [SerializeField]
        private List<WallSection> _groundFloorPattern = new List<WallSection>();
        [SerializeField]
        private List<WallSection> _usedWallSections = new List<WallSection>();

        [SerializeField]
        private int _baseWidth = 1;
        [SerializeField]
        private int _baseHeight = 1;

        public enum PatternAnchors
        {
            Left,
            Middle,
            Right
        }

        [SerializeField]
        private PatternAnchors _patternAnchors = PatternAnchors.Left;

        public enum StretchModes
        {
            Wrap,
            Clamp,
            Fit
        }
        [SerializeField]
        private StretchModes _stretchMode = StretchModes.Wrap;

        [SerializeField]
        private bool _tiled = true;

        [SerializeField]
        private bool _stringCourse = false;
        [SerializeField]
        private float _stringCourseHeight = 0.12f;
        [SerializeField]
        private float _stringCourseDepth = 0.08f;
        [SerializeField]
        private float _stringCoursePosition = 0.0f;
        [SerializeField]
        private Surface _stringCourseSurface;

        public enum RandomisationModes
        {
            Fixed,
            Random,
            RandomRows,
            RandomColumns
        }

        [SerializeField]
        private RandomisationModes _randomisationMode = RandomisationModes.Random;


        //        [SerializeField]
        //        private Color32[] _previewTextureArray = null;
        [SerializeField]
        private Texture2D _previewTexture = null;
        [SerializeField]
        private string _customIconPath = "";

        public string customIconPath
        {
            get { return _customIconPath; }
            set { _customIconPath = value; }
        }

        #region API

        public int baseWidth
        {
            get { return _baseWidth; }
            set
            {
                _baseWidth = value;
                CheckPatternSize();
                MarkModified();
            }
        }

        public int baseHeight
        {
            get { return _baseHeight; }
            set
            {
                _baseHeight = value;
                CheckPatternSize();
                MarkModified();
            }
        }

        public PatternAnchors patternAnchors
        {
            get { return _patternAnchors; }
            set
            {
                if (_patternAnchors != value)
                {
                    _patternAnchors = value;
                    MarkModified();
                }
            }
        }

        public StretchModes stretchMode
        {
            get { return _stretchMode; }
            set
            {
                if (_stretchMode != value)
                {
                    _stretchMode = value;
                    MarkModified();
                }
            }
        }

        public bool tiled
        {
            get { return _tiled; }
            set
            {
                if (_tiled != value)
                {
                    _tiled = value;
                    MarkModified();
                }
            }
        }

        public bool hasGroundFloorPattern
        {
            get { return _hasGroundFloorPattern; }
            set
            {
                if (_hasGroundFloorPattern != value)
                {
                    _hasGroundFloorPattern = value;
                    MarkModified();
                }
            }
        }

        #region Main Pattern

        public WallSection GetWallSection(int x, int y)
        {
            HUtils.log();


            if (_hasGroundFloorPattern && y == 0)
                return GetGroundWallSection(x);
            return GetBaseWallSection(x, y);
        }

        public WallSection GetWallSection(int x, int y, int facadeWidth, int facadeHeight)
        {
            HUtils.log();

            if (_hasGroundFloorPattern && y == 0)
                return GetGroundWallSection(x, facadeWidth);
            return GetBaseWallSection(x, y, facadeWidth, facadeHeight);
        }

        public bool stringCourse
        {
            get { return _stringCourse; }
            set
            {
                if (_stringCourse != value)
                {
                    _stringCourse = value;
                    MarkModified();
                }
            }
        }

        public float stringCourseHeight
        {
            get { return _stringCourseHeight; }
            set
            {
                if (_stringCourseHeight != value)
                {
                    _stringCourseHeight = value;
                    MarkModified();
                }
            }
        }

        public float stringCourseDepth
        {
            get { return _stringCourseDepth; }
            set
            {
                if (_stringCourseDepth != value)
                {
                    _stringCourseDepth = value;
                    MarkModified();
                }
            }
        }

        public Surface stringCourseSurface
        {
            get { return _stringCourseSurface; }
            set
            {
                if (_stringCourseSurface != value)
                {
                    _stringCourseSurface = value;
                    MarkModified();
                }
            }
        }

        public float stringCoursePosition
        {
            get { return _stringCoursePosition; }
            set
            {
                if (_stringCoursePosition != value)
                {
                    _stringCoursePosition = Mathf.Clamp01(value);
                    MarkModified();
                }
            }
        }

        public RandomisationModes randomisationMode
        {
            get { return _randomisationMode; }
            set
            {
                if (_randomisationMode != value)
                {
                    _randomisationMode = value;
                    MarkModified();
                }
            }
        }

        #endregion

        #region Base Pattern
        /// <summary>
        /// Base pattern is a repeating pattern
        /// values are automatically handled outside width/height values of array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public WallSection GetBaseWallSection(int x, int y)
        {
            HUtils.log();

            if (_basePattern.Count == 0)
                _basePattern.Add(new FacadeRow());
            if (_basePattern[0].Count == 0)
                _basePattern[0].Add(null);

            x = Mathf.Clamp(x, 0, _baseWidth - 1);
            y = Mathf.Clamp(y, 0, _baseHeight - 1);
            return _basePattern[x][y];
        }

        public WallSection GetBaseWallSection(int x, int y, int facadeWidth, int facadeHeight)
        {
            HUtils.log();

            if (_hasGroundFloorPattern && y > 0)
                y += -1;
            int anchoredX = x;
            switch (_patternAnchors)
            {
                case PatternAnchors.Left:
                    //no need to modify XY
                    break;
                case PatternAnchors.Right:
                    int diff = baseWidth - facadeWidth;
                    while (diff < 0 && baseWidth > 0) diff += baseWidth;
                    anchoredX = (x + diff) % baseWidth;
                    //	                Debug.Log("fAnc "+anchoredX+" "+ diff+" "+ baseWidth+" "+ facadeWidth);
                    break;
                case PatternAnchors.Middle:
                    int facadeMiddle = Mathf.CeilToInt(facadeWidth / 2f);
                    int patternMiddle = Mathf.CeilToInt(baseWidth / 2f);
                    int midDiff = patternMiddle - facadeMiddle;
                    anchoredX += midDiff;
                    break;
            }

            switch (stretchMode)
            {
                case StretchModes.Wrap:
                    while (anchoredX < 0)
                        anchoredX += Mathf.Max(1, baseWidth);
                    x = anchoredX % _baseWidth;
                    y = y % _baseHeight;
                    break;

                case StretchModes.Clamp:
                    x = Mathf.Clamp(anchoredX, 0, _baseWidth - 1);
                    y = Mathf.Clamp(y, 0, _baseHeight - 1);
                    break;

                case StretchModes.Fit:
                    x = Mathf.RoundToInt(_baseWidth * (x / (float)facadeWidth));
                    y = Mathf.RoundToInt(_baseHeight * (y / (float)facadeHeight));
                    break;
            }

            return GetBaseWallSection(x, y);
        }

        public void SetBaseWallSection(int x, int y, WallSection newWallSection)
        {
            HUtils.log();

            x = x % _baseWidth;
            y = y % _baseHeight;
            _basePattern[x][y] = newWallSection;
            MarkModified();
        }

        public void SetBaseWallSections(WallSection[] newWallSections)
        {
            HUtils.log();

            int total = _baseWidth * _baseHeight;
            int aLength = newWallSections.Length;
            if (aLength != total)
                Debug.Log("SetBaseWallSections incompatible array sent");

            for (int x = 0; x < _baseWidth; x++)
            {
                for (int y = 0; y < _baseHeight; y++)
                {
                    int index = x + y * _baseWidth;
                    _basePattern[x][y] = newWallSections[index];
                }
            }
            MarkModified();
        }

        public void SetBaseWallSections(WallSection[,] newWallSections)
        {
            HUtils.log();

            if (newWallSections.GetLength(0) != _baseWidth || newWallSections.GetLength(1) != _baseHeight)
                Debug.Log("SetBaseWallSections incompatible array sent");

            for (int x = 0; x < _baseWidth; x++)
            {
                for (int y = 0; y < _baseHeight; y++)
                {
                    _basePattern[x][y] = newWallSections[x, y];
                }
            }
            MarkModified();
        }
        #endregion

        #region Ground Pattern
        public WallSection GetGroundWallSection(int x)
        {
            if (!_hasGroundFloorPattern) return null;
            if (_groundFloorPattern.Count == 0)
                _groundFloorPattern.Add(null);
            CheckPatternSize();

            if (_groundFloorPattern.Count == 0)
                return null;


            x = x % _baseWidth;
            return _groundFloorPattern[x];
        }

        public WallSection GetGroundWallSection(int x, int facadeWidth)
        {
            HUtils.log();

            switch (_patternAnchors)
            {
                case PatternAnchors.Left:
                    //no need to modify XY
                    break;
                case PatternAnchors.Right:
                    int diff = baseWidth - facadeWidth;
                    x = (x + diff) % baseWidth;
                    break;
                case PatternAnchors.Middle:
                    int facadeMiddle = Mathf.CeilToInt(facadeWidth / 2f);
                    int patternMiddle = Mathf.CeilToInt(baseWidth / 2f);
                    int midDiff = patternMiddle - facadeMiddle;
                    x = (x + midDiff) % baseWidth;
                    break;
            }
            while (x < 0)
                x += Mathf.Max(1, baseWidth);
            return GetGroundWallSection(x);
        }

        public void SetGroundWallSection(int x, WallSection newWallSection)
        {
            HUtils.log();

            x = x % _baseWidth;
            _groundFloorPattern[x] = newWallSection;
            MarkModified();
        }
        #endregion

        //        public Color32[] previewTextureArray
        //        {
        //            get
        //            {
        ////                Debug.Log("get preview texture: "+name);
        //                if (_previewTextureArray == null || _previewTextureArray.Length == 0)
        //                    UpdatePreviewTexture();
        //                return _previewTextureArray;
        //            }
        //        }

        public Texture2D previewTexture
        {
            get
            {
                if (_previewTexture == null)
                    LoadPreviewTexture();
                return _previewTexture;
            }
        }

        private void UpdatePreviewTexture(IconUtil.GUIDIconData iconData = null)
        {
            HUtils.log();
#if UNITY_EDITOR
            if (GetBaseWallSection(0, 0) != null)
            {
                Vector2Int size = new Vector2Int(64, 64);
                Color32[] previewTextureArray = new Color32[size.x * size.y];
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                if (iconData == null)
                    iconData = IconUtil.GenerateGUIDIconData(guid);
                previewTextureArray = SimpleTextureGenerator.GenerateFacadePreview(this, size, new Vector2Int(4, 4));
                Texture2D iconTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
                iconTexture.SetPixels32(0, 0, 64, 64, previewTextureArray);
                iconTexture.Apply(false, false);
                IconUtil.SaveIcon(iconData.tempTexturePath, iconTexture);
                customIconPath = iconData.tempTexturePath;
                //                UnityEditor.AssetDatabase.Refresh();
            }
#endif
        }

        private void LoadPreviewTexture()
        {
#if UNITY_EDITOR

            if (string.IsNullOrEmpty(customIconPath))
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                IconUtil.GUIDIconData iconData = IconUtil.GenerateGUIDIconData(guid);
                customIconPath = iconData.tempTexturePath;
            }
            _previewTexture = IconUtil.GetIcon(customIconPath);
            if (_previewTexture == null)
            {
                UpdatePreviewTexture();
                _previewTexture = IconUtil.GetIcon(customIconPath);
            }
#endif
        }

        public void MarkModified()
        {
            HUtils.log();
            //Debug.Log("Facade.cs MarkModified()");
            UpdatePreviewTexture();
            LoadPreviewTexture();
            UpdateUsedWallsectionList();
            SaveData();
        }

        public List<WallSection> usedWallSections
        {
            get
            {
                HUtils.log();

                if (_usedWallSections == null)
                    _usedWallSections = new List<WallSection>();
                if (_usedWallSections.Count == 0)
                    UpdateUsedWallsectionList();
                return _usedWallSections;
            }
        }

        #endregion

        #region Privates

        private void CheckPatternSize()
        {
            _baseWidth = Mathf.Max(_baseWidth, 1);
            _baseHeight = Mathf.Max(_baseHeight, 1);

            //decrease pattern arrays
            while (_basePattern.Count > _baseWidth)
                _basePattern.RemoveAt(_basePattern.Count - 1);

            if (_hasGroundFloorPattern)
                while (_groundFloorPattern.Count > _baseWidth)
                    _groundFloorPattern.RemoveAt(_groundFloorPattern.Count - 1);

            //increase pattern arrays

            if (_basePattern.Count == 0)
                _basePattern.Add(new FacadeRow());
            if (_basePattern[0].Count == 0)
                _basePattern[0].Add(null);

            while (_basePattern.Count < _baseWidth)
            {
                _basePattern.Add(_basePattern[_basePattern.Count - 1].Clone());
            }

            if (_hasGroundFloorPattern)
                while (_groundFloorPattern.Count < _baseWidth)
                    _groundFloorPattern.Add(_groundFloorPattern[_groundFloorPattern.Count - 1]);

            //do base pattern heights
            for (int w = 0; w < _basePattern.Count; w++)
            {
                while (_basePattern[w].Count > _baseHeight)
                    _basePattern[w].RemoveAt(_basePattern[w].Count - 1);

                while (_basePattern[w].Count < _baseHeight)
                    _basePattern[w].Add(_basePattern[w][_basePattern[w].Count - 1]);
            }
        }

        private void OnEnable()
        {
            if (_previewTexture == null)
            {
                LoadPreviewTexture();
            }
        }

        private void OnValidate()
        {
            CheckPatternSize();
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
                building.MarkModified();
#endif
        }

        private void UpdateUsedWallsectionList()
        {
            _usedWallSections.Clear();
            int basePatternCount = _basePattern.Count;
            for (int b = 0; b < basePatternCount; b++)
            {
                FacadeRow row = _basePattern[b];
                int rowCount = row.Count;
                for (int r = 0; r < rowCount; r++)
                {
                    WallSection section = row[r];
                    if (!_usedWallSections.Contains(section))
                        _usedWallSections.Add(section);
                }
            }

            int groundPatternCount = _groundFloorPattern.Count;
            for (int b = 0; b < groundPatternCount; b++)
            {
                if (_groundFloorPattern != null && _groundFloorPattern.Count > 0 && !_usedWallSections.Contains(_groundFloorPattern[b]))
                    _usedWallSections.Add(_groundFloorPattern[b]);
            }
            //todo add other pattern lists when implemented
        }

        #endregion

        #region statics
        public static Facade CreateFacade(string name = null, string directory = null)
        {
            HUtils.log();
            Debug.Log("调用了Facade.cs CreateFacade(string name = null, string directory = null) name=" + name + "  directory=" + directory);
            Facade newFacade = CreateInstance<Facade>();
            if (Application.isPlaying) return newFacade;
            if (name != null)
                newFacade.name = name;
#if UNITY_EDITOR
            if (directory == null)
                UnityEditor.AssetDatabase.CreateAsset(newFacade, AssetCreator.GeneratePath("newFacade.asset", "Facades"));
            else
                UnityEditor.AssetDatabase.CreateAsset(newFacade, Path.Combine(directory, "newFacade.asset"));
#endif
            return newFacade;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Facade", false, ToolsMenuLevels.CREATE_FACADE_DESIGN)]
        private static Facade MenuCreateNewFacade()
        {
            Facade output = CreateFacade();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Facade", false, ToolsMenuLevels.CREATE_FACADE_DESIGN)]
        private static Facade MenuCreateNewFacadeB()
        {
            string activeFolder = AssetCreator.ActiveSelectionPath();
            Facade output = CreateFacade(null, activeFolder);
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }

    [System.Serializable]
    public class FacadeRow
    {
        //wrapper class because of Unity Serialisation

        public List<WallSection> content = new List<WallSection>();

        public FacadeRow() { }

        public FacadeRow(List<WallSection> copy)
        {
            content = new List<WallSection>(copy);
        }

        public WallSection this[int index]
        {
            get { return content[index]; }
            set { content[index] = value; }
        }

        public int Count { get { return content.Count; } }

        public void Add(WallSection item) { content.Add(item); }
        public void InsertAt(int index, WallSection item)
        {
            HUtils.log();

            content.Insert(index, item);
        }
        public void Remove(WallSection item) { content.Remove(item); }
        public void RemoveAt(int index)
        {
            HUtils.log();
            Debug.Log("移动了索引第 " + index + " 个的WallSection");
            
            content.RemoveAt(index);
        }

        public FacadeRow Clone()
        {
            return new FacadeRow(content);
        }
    }
}