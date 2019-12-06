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

using System.IO;
using UnityEngine;

namespace BuildR2
{
    public class WallSection : ScriptableObject, IWallSection, ICustomIconAsset
    {
        public enum DimensionTypes
        {
            Absolute,
            Relative
        }

        public enum VerticalRestrictions
        {
            None,
            GroundOnly,
            UpperOnly
        }

        [SerializeField]
        protected bool _hasOpening = true;

        [SerializeField]
        protected bool _isDoor = false;

        [SerializeField]
        protected VerticalRestrictions _verticalRestriction = VerticalRestrictions.None;

        [SerializeField]
        protected Portal _portal = null;

        [SerializeField]
        protected Model _model = null;

        [SerializeField]
        protected Model _openingModel = null;

        [SerializeField]
        protected Model _balconyModel = null;
        [SerializeField]
        protected float _balconyHeight = 0.5f;
        [SerializeField]
        protected float _balconySideOverhang = 0.07f;

        [SerializeField]
        protected Model _shutterModel = null;

        [SerializeField]
        protected DimensionTypes _dimensionType = DimensionTypes.Relative;

        //TODO minimum width for relative dimentions

        [SerializeField]
        protected float _openingWidthAbs = 1.0f;

        [SerializeField]
        protected float _openingHeightAbs = 1.25f;

        [SerializeField]
        protected float _openingWidthRel = 0.6f;

        [SerializeField]
        protected float _openingHeightRel = 0.8f;

        [SerializeField]
        protected float _openingDepth = 0.25f;

        [SerializeField]
        protected float _openingWidthRatio = 0.5f;//the ratio of space between the left and right walls from the opening

        [SerializeField]
        protected float _openingHeightRatio = 0.95f;//the ratio of space between above and below the opening

        [SerializeField]
        protected float _openingDepthRatio = 0.5f;//the ratio of space between the front and behind the opening

        [SerializeField]
        protected bool _openingFrame = false;

        [SerializeField]
        protected float _openingFrameSize = 0.08f;

        [SerializeField]
        protected float _openingFrameExtrusion = 0.0f;

        [SerializeField]
        protected bool _arched = false;

        [SerializeField]
        protected float _archHeight = 0.35f;

        [SerializeField]
        protected float _archCurve = 0.5f;

        [SerializeField]
        protected int _archSegments = 5;

        [SerializeField]
        protected bool _extrudedSill = false;

        [SerializeField]
        protected Vector3 _extrudedSillDimentions = new Vector3(0.1f, 0.15f, 0.05f);

        [SerializeField]
        protected bool _extrudedLintel = false;

        [SerializeField]
        protected Vector3 _extrudedLintelDimentions = new Vector3(0.1f, 0.15f, 0.05f);

        [SerializeField]
        protected Surface _openingSurface;

        [SerializeField]
        protected Surface _wallSurface;

        [SerializeField]
        protected Surface _sillSurface;

        [SerializeField]
        protected Surface _ceilingSurface;

        [SerializeField]
        protected bool _bayExtruded = false;

        [SerializeField]
        protected float _bayExtrusion = 0.5f;

        [SerializeField]
        protected float _bayBevel = 0.4f;

        [SerializeField]
        protected Texture2D _previewTexture = null;
        [SerializeField]
        private string _customIconPath = "";

        public string customIconPath
        {
            get { return _customIconPath; }
            set { _customIconPath = value; }
        }

        public bool hasOpening
        {
            get
            {
                return _hasOpening;
            }
            set
            {
                if (_hasOpening != value)
                {
                    _hasOpening = value;
                    MarkModified();
                }
            }
        }

        public bool isDoor
        {
            get
            {
                return _isDoor;
            }
            set
            {
                if (_isDoor != value)
                {
                    _isDoor = value;
                    MarkModified();
                }
            }
        }

        public VerticalRestrictions verticalRestriction
        {
            get { return _verticalRestriction; }
            set
            {
                if (_verticalRestriction != value)
                {
                    _verticalRestriction = value;
                    MarkModified();
                }
            }
        }

        public Portal portal
        {
            get
            {
                return _portal;
            }
            set
            {
                if (_portal != value)
                {
                    _portal = value;
                    MarkModified();
                }
            }
        }

        public Model model
        {
            get
            {
                return _model;
            }
            set
            {
                if (_model != value)
                {
                    _model = value;
                    MarkModified();
                }
            }
        }

        public Model openingModel
        {
            get
            {
                return _openingModel;
            }
            set
            {
                if (_openingModel != value)
                {
                    _openingModel = value;
                    MarkModified();
                }
            }
        }

        public Model balconyModel
        {
            get
            {
                return _balconyModel;
            }
            set
            {
                if (_balconyModel != value)
                {
                    _balconyModel = value;
                    MarkModified();
                }
            }
        }

        public float balconyHeight
        {
            get { return _balconyHeight; }
            set
            {
                if (_balconyHeight != value)
                {
                    _balconyHeight = Mathf.Clamp01(value);
                    MarkModified();
                }
            }
        }

        public float balconySideOverhang
        {
            get { return _balconySideOverhang; }
            set
            {
                if (_balconySideOverhang != value)
                {
                    _balconySideOverhang = value;
                    MarkModified();
                }
            }
        }

        public Model shutterModel
        {
            get
            {
                return _shutterModel;
            }
            set
            {
                if (_shutterModel != value)
                {
                    _shutterModel = value;
                    MarkModified();
                }
            }
        }

        public DimensionTypes dimensionType
        {
            get { return _dimensionType; }
            set
            {
                if (_dimensionType != value)
                {
                    _dimensionType = value;
                    MarkModified();
                }
            }
        }

        public float openingWidth
        {
            get
            {
                switch (_dimensionType)
                {
                    case DimensionTypes.Absolute:
                        return _openingWidthAbs;
                    case DimensionTypes.Relative:
                        return _openingWidthRel;
                }
                return _openingWidthAbs;
            }
            set
            {
                switch (_dimensionType)
                {
                    case DimensionTypes.Absolute:
                        if (_openingWidthAbs != value)
                        {
                            _openingWidthAbs = value;
                            MarkModified();
                        }
                        break;
                    case DimensionTypes.Relative:
                        if (_openingWidthRel != value)
                        {
                            _openingWidthRel = Mathf.Clamp01(value);
                            MarkModified();
                        }
                        break;
                }
            }
        }

        public float openingHeight
        {
            get
            {
                switch (_dimensionType)
                {
                    case DimensionTypes.Absolute:
                        return _openingHeightAbs;
                    case DimensionTypes.Relative:
                        return _openingHeightRel;
                }
                return _openingHeightAbs;
            }
            set
            {
                switch (_dimensionType)
                {
                    case DimensionTypes.Absolute:
                        if (_openingHeightAbs != value)
                        {
                            _openingHeightAbs = value;
                            MarkModified();
                        }
                        break;
                    case DimensionTypes.Relative:
                        if (_openingHeightRel != value)
                        {
                            _openingHeightRel = Mathf.Clamp01(value);
                            MarkModified();
                        }
                        break;
                }
            }
        }

        public float openingDepth
        {
            get { return _openingDepth; }
            set
            {
                if (_openingDepth != value)
                {
                    _openingDepth = value;
                    MarkModified();
                }
            }
        }

        public float openingWidthRatio
        {
            get { return _openingWidthRatio; }
            set
            {
                if (_openingWidthRatio != value)
                {
                    _openingWidthRatio = value;
                    MarkModified();
                }
            }
        }

        public float openingHeightRatio
        {
            get { return _openingHeightRatio; }
            set
            {
                if (_openingHeightRatio != value)
                {
                    _openingHeightRatio = value;
                    MarkModified();
                }
            }
        }

        public float openingDepthRatio
        {
            get { return _openingDepthRatio; }
            set
            {
                if (_openingDepthRatio != value)
                {
                    _openingDepthRatio = value;
                    MarkModified();
                }
            }
        }

        public bool openingFrame
        {
            get { return _openingFrame; }
            set
            {
                if (_openingFrame != value)
                {
                    _openingFrame = value;
                    MarkModified();
                }
            }
        }

        public float openingFrameSize
        {
            get { return _openingFrameSize; }
            set
            {
                if (_openingFrameSize != value)
                {
                    _openingFrameSize = value;
                    MarkModified();
                }
            }
        }

        public float openingFrameExtrusion
        {
            get { return _openingFrameExtrusion; }
            set
            {
                if (_openingFrameExtrusion != value)
                {
                    _openingFrameExtrusion = value;
                    MarkModified();
                }
            }
        }

        public bool isArched
        {
            get { return _arched; }
            set
            {
                if (_arched != value)
                {
                    _arched = value;
                    MarkModified();
                }
            }
        }

        public float archHeight
        {
            get { return _archHeight; }
            set
            {
                if (_archHeight != value)
                {
                    _archHeight = value;
                    MarkModified();
                }
            }
        }

        public float archCurve
        {
            get { return _archCurve; }
            set
            {
                if (_archCurve != value)
                {
                    _archCurve = Mathf.Clamp01(value);
                    MarkModified();
                }
            }
        }

        public int archSegments
        {
            get { return _archSegments; }
            set
            {
                if (_archSegments != value)
                {
                    _archSegments = value;
                    MarkModified();
                }
            }
        }

        public bool extrudedSill
        {
            get { return _extrudedSill; }
            set
            {
                if (_extrudedSill != value)
                {
                    _extrudedSill = value;
                    MarkModified();
                }
            }
        }

        public Vector3 extrudedSillDimentions
        {
            get { return _extrudedSillDimentions; }
            set
            {
                if (_extrudedSillDimentions != value)
                {
                    _extrudedSillDimentions = value;
                    _extrudedSillDimentions.x = Mathf.Max(_extrudedSillDimentions.x, 0.0f);
                    _extrudedSillDimentions.z = Mathf.Max(_extrudedSillDimentions.z, 0.1f);
                    MarkModified();
                }
            }
        }

        public bool extrudedLintel
        {
            get { return _extrudedLintel; }
            set
            {
                if (_extrudedLintel != value)
                {
                    _extrudedLintel = value;
                    MarkModified();
                }
            }
        }

        public Vector3 extrudedLintelDimentions
        {
            get { return _extrudedLintelDimentions; }
            set
            {
                if (_extrudedLintelDimentions != value)
                {
                    _extrudedLintelDimentions = value;
                    MarkModified();
                }
            }
        }

        public Surface openingSurface
        {
            get { return _openingSurface; }
            set
            {
                if (_openingSurface != value)
                {
                    _openingSurface = value;
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

        public Surface sillSurface
        {
            get { return _sillSurface; }
            set
            {
                if (_sillSurface != value)
                {
                    _sillSurface = value;
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

        public bool bayExtruded
        {
            get { return _bayExtruded; }
            set
            {
                if (_bayExtruded != value)
                {
                    _bayExtruded = value;
                    MarkModified();
                }
            }
        }

        public float bayExtrusion
        {
            get { return _bayExtrusion; }
            set
            {
                value = Mathf.Clamp01(value);
                if (_bayExtrusion != value)
                {
                    _bayExtrusion = value;
                    MarkModified();
                }
            }
        }

        public float bayBevel
        {
            get { return _bayBevel; }
            set
            {
                value = Mathf.Clamp01(value);
                if (_bayBevel != value)
                {
                    _bayBevel = value;
                    MarkModified();
                }
            }
        }

        public void MarkModified()
        {

        }

        public Matrix4x4 OpeningMeshPosition(Vector2 size, float wallThickness)
        {
            HUtils.log();

            Matrix4x4 output = new Matrix4x4();
            float useOpeningWidth = openingWidth;
            float useOpeningHeight = openingHeight;
            float wallThicknessRat = wallThickness / size.y;
            size.y += -wallThickness;
            if (dimensionType == DimensionTypes.Relative)
            {
                useOpeningWidth = useOpeningWidth * size.x;
                useOpeningHeight = useOpeningHeight * size.y;
            }
            float width = useOpeningWidth;
            float height = useOpeningHeight;

            float widthOffset = 0;
            float heightOffset = 0;
            widthOffset += -width * (openingModel.userBounds.center.x / openingModel.userBounds.size.x);
            widthOffset += (size.x - useOpeningWidth) * (_openingWidthRatio - 0.5f);

            heightOffset += -height * (openingModel.userBounds.center.y / openingModel.userBounds.size.y);
            heightOffset += -size.y * wallThicknessRat * 0.5f;
            heightOffset += (size.y - useOpeningHeight) * (_openingHeightRatio - 0.5f);

            Vector3 center = new Vector3(widthOffset, heightOffset, 0);

            float depth = openingModel.userBounds.size.z;
            depth *= width / Mathf.Min(openingModel.userBounds.size.x, openingModel.userBounds.size.y);

            Vector3 balPos = Vector3.zero;
            balPos.x = center.x;
            balPos.y = center.y;
            balPos.z = openingDepth;
            Quaternion balRot = Quaternion.Euler(0, 0, 0) * openingModel.userRotationQuat;
            Vector3 balScl = Vector3.one;
            balScl.x = width / openingModel.userBounds.size.x;
            balScl.y = height / openingModel.userBounds.size.y;
            balScl.z = depth / openingModel.userBounds.size.z;

            output.SetTRS(balPos, balRot, balScl);
            return output;
        }

        public Matrix4x4 BalconyMeshPosition(Vector2 size, float wallThickness)
        {
            HUtils.log();

            Matrix4x4 output = new Matrix4x4();
            float useOpeningWidth = openingWidth;
            float useOpeningHeight = openingHeight;
            float wallThicknessRat = wallThickness / size.y;
            size.y += -wallThickness;
            if (dimensionType == DimensionTypes.Relative)
            {
                useOpeningWidth = useOpeningWidth * size.x;
                useOpeningHeight = useOpeningHeight * size.y;
            }
            float width = useOpeningWidth + balconySideOverhang * 2;
            float height = useOpeningHeight * balconyHeight;

            float widthOffset = 0;
            float heightOffset = 0;

            widthOffset += -width * (balconyModel.modelBounds.center.x / balconyModel.modelBounds.size.x);
            widthOffset += (size.x - useOpeningWidth) * (_openingWidthRatio - 0.5f);

            heightOffset += -height * (balconyModel.modelBounds.center.y / balconyModel.modelBounds.size.y);
            heightOffset += -size.y * wallThicknessRat * 0.5f;
            heightOffset += (size.y - useOpeningHeight) * (_openingHeightRatio - 0.5f);

            Vector3 center = new Vector3(widthOffset, (-useOpeningHeight + height) * 0.5f + heightOffset, 0);

            float depth = balconyModel.modelBounds.size.z;
            depth *= width / balconyModel.modelBounds.size.x;

            Vector3 balPos = Vector3.zero;//new Vector3(placeModelBounds.max.z);
            balPos.x = center.x;
            balPos.y = center.y;
            balPos.z = -depth * 0.5f;
            Quaternion balRot = Quaternion.Euler(0, 180, 0) * balconyModel.userRotationQuat;
            Vector3 balScl = Vector3.one;
            balScl.x = width / balconyModel.modelBounds.size.x;
            balScl.y = height / balconyModel.modelBounds.size.y;
            balScl.z = depth / balconyModel.modelBounds.size.z;

            output.SetTRS(balPos, balRot, balScl);
            return output;
        }

        public Matrix4x4 ShutterMeshPositionLeft(Vector2 size, float wallThickness)
        {
            HUtils.log();

            Matrix4x4 output = new Matrix4x4();
            float useOpeningWidth = openingWidth;
            float useOpeningHeight = openingHeight;
            float wallThicknessRat = wallThickness / size.y;
            size.y += -wallThickness;
            if (dimensionType == DimensionTypes.Relative)
            {
                useOpeningWidth = useOpeningWidth * size.x;
                useOpeningHeight = useOpeningHeight * size.y;
            }
            float width = useOpeningWidth * 0.5f;
            float height = useOpeningHeight;

            float widthOffset = 0;
            float heightOffset = 0;

            widthOffset += -width * (shutterModel.modelBounds.center.x / shutterModel.modelBounds.size.x);
            widthOffset += (size.x - useOpeningWidth) * (_openingWidthRatio - 0.5f);

            heightOffset += -height * (shutterModel.modelBounds.center.y / shutterModel.modelBounds.size.y);
            heightOffset += -size.y * wallThicknessRat * 0.5f;
            heightOffset += (size.y - useOpeningHeight) * (_openingHeightRatio - 0.5f);

            Vector3 center = new Vector3(-useOpeningWidth * 0.75f + widthOffset, heightOffset, 0);

            float depth = shutterModel.modelBounds.size.z;
            depth *= width / Mathf.Min(shutterModel.modelBounds.size.x, shutterModel.modelBounds.size.y);

            Vector3 balPos = Vector3.zero;//new Vector3(placeModelBounds.max.z);
            balPos.x = center.x;
            balPos.y = center.y;
            balPos.z = 0;
            Quaternion balRot = Quaternion.Euler(0, 180, 0) * shutterModel.userRotationQuat;
            Vector3 balScl = Vector3.one;
            balScl.x = width / shutterModel.modelBounds.size.x;
            balScl.y = height / shutterModel.modelBounds.size.y;
            balScl.z = depth / shutterModel.modelBounds.size.z;

            output.SetTRS(balPos, balRot, balScl);
            return output;
        }

        public Matrix4x4 ShutterMeshPositionRight(Vector2 size, float wallThickness)
        {
            HUtils.log();

            Matrix4x4 output = new Matrix4x4();
            float useOpeningWidth = openingWidth;
            float useOpeningHeight = openingHeight;
            float wallThicknessRat = wallThickness / size.y;
            size.y += -wallThickness;
            if (dimensionType == DimensionTypes.Relative)
            {
                useOpeningWidth = useOpeningWidth * size.x;
                useOpeningHeight = useOpeningHeight * size.y;
            }
            float width = useOpeningWidth * 0.5f;
            float height = useOpeningHeight;

            float widthOffset = 0;
            float heightOffset = 0;

            widthOffset += -width * (shutterModel.modelBounds.center.x / shutterModel.modelBounds.size.x);
            widthOffset += (size.x - useOpeningWidth) * (_openingWidthRatio - 0.5f);

            heightOffset += -height * (shutterModel.modelBounds.center.y / shutterModel.modelBounds.size.y);
            heightOffset += -size.y * wallThicknessRat * 0.5f;
            heightOffset += (size.y - useOpeningHeight) * (_openingHeightRatio - 0.5f);

            Vector3 center = new Vector3(useOpeningWidth * 0.75f + widthOffset, heightOffset, 0);

            float depth = shutterModel.modelBounds.size.z;
            depth *= width / Mathf.Min(shutterModel.modelBounds.size.x, shutterModel.modelBounds.size.y);

            Vector3 balPos = Vector3.zero;//new Vector3(placeModelBounds.max.z);
            balPos.x = center.x;
            balPos.y = center.y;
            balPos.z = 0;
            Quaternion balRot = Quaternion.Euler(0, 180, 0) * shutterModel.userRotationQuat;
            Vector3 balScl = new Vector3(-1, 1, 1);
            balScl.x = width / shutterModel.modelBounds.size.x;
            balScl.y = height / shutterModel.modelBounds.size.y;
            balScl.z = depth / shutterModel.modelBounds.size.z;

            output.SetTRS(balPos, balRot, balScl);
            return output;
        }

        public Texture2D previewTexture
        {
            get
            {
                if (_previewTexture == null)
                    LoadPreviewTexture();
                return _previewTexture;
            }
        }

        public void UpdatePreviewTexture(IconUtil.GUIDIconData iconData = null)
        {
#if UNITY_EDITOR
            Vector2Int size = new Vector2Int(64, 64);
            Texture2D iconTexture;
            if (_wallSurface == null && _ceilingSurface == null && _sillSurface == null && _openingSurface == null)
            {
                iconTexture = new Texture2D(1, 1);
            }
            else
            {
                Color32[] textureArray;
                WallSectionGenerator.Texture(out textureArray, this, size, Vector2.one * 2);
                iconTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
                iconTexture.SetPixels32(0, 0, 64, 64, textureArray);
                iconTexture.Apply(false, false);
            }

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            if (iconData == null)
                iconData = IconUtil.GenerateGUIDIconData(guid);

            IconUtil.SaveIcon(iconData.tempTexturePath, iconTexture);
            customIconPath = iconData.tempTexturePath;
#endif
        }

        public void LoadPreviewTexture()
        {
#if UNITY_EDITOR
            HUtils.log();
            
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

        public bool CanRender(Vector2 size)
        {
            if (_dimensionType == DimensionTypes.Relative) return true;
            if (size.x < openingWidth) return false;
            if (size.y < openingHeight) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}: opSuf:{1} wlSuf{2}", name, openingSurface, wallSurface);
        }

        private void OnEnable()
        {
            if (_previewTexture == null)
            {
                LoadPreviewTexture();
            }
        }

        public void GenereateData()
        {
            UpdatePreviewTexture();
            LoadPreviewTexture();
        }

        public void SaveData()
        {
#if UNITY_EDITOR
            //tell everyone the good news...
            Building[] buildings = FindObjectsOfType<Building>();
            foreach (Building building in buildings)
            {
                UnityEditor.Undo.RecordObject(building, "wall section modification");
                building.MarkModified();
            }
#endif
        }

        #region statics
        public static WallSection CreateWallSection(string name = null, string directory = null)
        {
            WallSection wallSection = CreateInstance<WallSection>();
            if (Application.isPlaying) return wallSection;
            if (name != null)
                wallSection.name = name;
#if UNITY_EDITOR
            if (directory == null)
                UnityEditor.AssetDatabase.CreateAsset(wallSection, AssetCreator.GeneratePath("newWallSection.asset", "WallSections"));
            else
                UnityEditor.AssetDatabase.CreateAsset(wallSection, Path.Combine(directory, "newWallSection.asset"));
#endif

            BuildRSettings settings = BuildRSettings.GetSettings();
            wallSection._openingWidthAbs = settings.defaultWindowWidth;
            wallSection._openingHeightAbs = settings.defaultWindowHeight;
            wallSection._openingDepth = settings.defaultWindowDepth;

            return wallSection;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Wall Section", false, ToolsMenuLevels.CREATE_WALLSECTION)]
        private static WallSection MenuCreateNewWallSection()
        {
            WallSection output = CreateWallSection();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Wall Section", false, ToolsMenuLevels.CREATE_WALLSECTION)]
        private static WallSection MenuCreateNewWallSectionB()
        {
            string activeFolder = AssetCreator.ActiveSelectionPath();
            WallSection output = CreateWallSection(null, activeFolder);
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }
}