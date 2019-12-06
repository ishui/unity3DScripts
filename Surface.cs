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
    public class Surface : ScriptableObject
    {
        public enum SurfaceTypes
        {
            Material,
            Substance
        }

        [SerializeField]
        protected SurfaceTypes _type = SurfaceTypes.Material;
        [SerializeField]
        protected Material _material;
#if !UNITY_2017_3_OR_NEWER
        [SerializeField]
        protected ProceduralMaterial _substance;
#endif
        [SerializeField]
        protected bool _tiled = true;
        [SerializeField]
        protected int _tiledX = 1;//the amount of times the surface should be repeated along the x axis
        [SerializeField]
        protected int _tiledY = 1;//the amount of times the surface should be repeated along the y axis
        [SerializeField]
        protected Vector2 _textureUnitSize = Vector2.one;//the world size of the surface - default 1m x 1m

        [SerializeField]
        protected bool _flipped = false;

        [SerializeField]
        protected bool _isReadable = false;

        #region API

        public SurfaceTypes surfaceType
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    MarkModified();
                }
            }
        }

        public Material material
        {
            get
            {
                if (_material != null)
                    return _material;
#if !UNITY_2017_3_OR_NEWER
                if (_substance != null)
                    return _substance;
#endif
                return null;
            }
            set
            {
                if (_material != value)
                {
                    _material = value;
                    if (_material != null) {
#if !UNITY_2017_3_OR_NEWER
                        if (_material is ProceduralMaterial)
                        {
                            _type = SurfaceTypes.Substance;
                            _substance = _material as ProceduralMaterial;
                            _material = null;
                        }
#endif
                    }
                    MarkModified();
                }
            }
        }

        public Color colour
        {
            get
            {
                if(_material != null)
                    return _material.color;
                return Color.clear;
            }
            set
            {
                if (_material != null && _material.color != value)
                {
                    _material.color = value;
                    MarkModified();
                }
            }
        }

        public virtual bool tiled
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

        public int tiledX
        {
            get { return _tiledX; }
            set
            {
                if (_tiledX != value)
                {
                    _tiledX = value;
                    MarkModified();
                }
            }
        }

        public int tiledY
        {
            get { return _tiledY; }
            set
            {
                if (_tiledY != value)
                {
                    _tiledY = value;
                    MarkModified();
                }
            }
        }


        public Vector2 textureUnitSize
        {
            get { return _textureUnitSize; }
            set
            {
                if (_textureUnitSize != value)
                {
                    _textureUnitSize = value;
                    MarkModified();
                }
            }
        }

        public bool flipped
        {
            get
            {
                return _flipped;
            }
            set
            {
                if (_flipped != value)
                {
                    _flipped = value;
                    MarkModified();
                }
            }
        }

        public bool readable
        {
            get
            {
                return _isReadable;
            }
        }

        public bool onlyColour
        {
            get
            {
                switch (surfaceType)
                {
                    default:
                        return false;
                    case SurfaceTypes.Material:
                        if (_material != null)
                            if (_material.mainTexture == null)
                                return true;
                        return false;
                }
            }
        }

#if !UNITY_2017_3_OR_NEWER
        public ProceduralMaterial substance
        {
            get { return _substance; }
            set
            {
                if(_substance != value)
                {
                    _substance = value;
                    MarkModified();
                }
            }
        }
#else
        public Material substance
        {
            get{return material;}
            set{material=value;}
        }
#endif

        public Texture previewTexture
        {
            get
            {
                switch (_type)
                {
                    case SurfaceTypes.Material:
                        if (_material != null)
                            return _material.mainTexture;
                        return null;

#if !UNITY_2017_3_OR_NEWER
                    case SurfaceTypes.Substance:
                        if (_substance != null)
                            return _substance.GetGeneratedTextures()[0];
                        return null;
#endif
                }

                return null;
            }
        }

        public Color32[] pixels
        {
            get
            {
                switch (_type)
                {
                    case SurfaceTypes.Material:
                        if (_material != null)
                        {
                            if (_material.mainTexture != null && _isReadable)
                            {
                                return ((Texture2D)(_material.mainTexture)).GetPixels32();
                            }
                            else
                            {
                                return new[] { (Color32)_material.color };
                            }
                        }
                        return new[] { new Color32(1, 0, 1, 1) };//error - send some magenta out there

#if !UNITY_2017_3_OR_NEWER
                    case SurfaceTypes.Substance:
                        if (_substance != null)
                        {
                            Texture2D substanceTexture = _substance.GetGeneratedTextures()[0] as Texture2D;
                            if (substanceTexture != null)
                                return substanceTexture.GetPixels32();
                            return new[] { new Color32(1, 0, 1, 1) };//error - send some magenta out there
                        }
                        return new[] { new Color32(1, 0, 1, 1) };//error - send some magenta out there
#endif
                }

                return new[] { new Color32(1, 0, 1, 1) };//error - send some magenta out there
            }
        }

        public void MarkModified()
        {
            CheckTextureIsReadable();
            SaveData();
        }


        /// <summary>
        /// Can calculate the UV based on the surface properties
        /// </summary>
        /// <param name="uv">The UV, world spaceetc mmm</param>
        /// <param name="index">ignore</param>
        /// <returns></returns>
        public virtual Vector2 CalculateUV(Vector2 uv)
        {
            Vector2 output = uv;

            if (_tiled)
            {
                output.x = output.x / _textureUnitSize.x;
                output.y = output.y / _textureUnitSize.y;
            }
            else
            {
                output.x *= _tiledX;
                output.y *= _tiledY;
            }

            if (_flipped)
            {
                Vector2 flip = new Vector2(output.y, output.x);
                output = flip;
            }

            return output;
        }

        #endregion

        private void SaveData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

#if UNITY_EDITOR
        public void Rename(string newName)
        {
            UnityEditor.Undo.RecordObject(this, "surface name change");
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
            UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
            SaveData();
        }
#endif

        private void CheckTextureIsReadable()
        {
            _isReadable = true;
            if(_material == null)
                return;
			
#if !UNITY_2017_3_OR_NEWER
            if (_material is ProceduralMaterial)
                _type = SurfaceTypes.Substance;
            else
                _type = SurfaceTypes.Material;
            if (_type == SurfaceTypes.Substance)
                return;
#endif
#if UNITY_EDITOR
            Texture texture = material.mainTexture;
            if (texture == null)
            {
                _isReadable = false;
                return;
            }
            string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture);
            UnityEditor.TextureImporter textureImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(texturePath);
            if (textureImporter == null)
            {
                _isReadable = false;
                return;
            }
            if (!textureImporter.isReadable)
                _isReadable = false;
#endif
        }

	    #region statics
	    public static Surface CreateSurface()
	    {
		    Surface newSurface = CreateInstance<Surface>();
#if UNITY_EDITOR
		    UnityEditor.AssetDatabase.CreateAsset(newSurface, AssetCreator.GeneratePath("newSurface.asset", "Surfaces"));
		    UnityEditor.AssetDatabase.SaveAssets();
		    UnityEditor.AssetDatabase.Refresh();
#endif
		    return newSurface;
		}

#if UNITY_EDITOR

		[UnityEditor.MenuItem("Tools/BuildR/Create New Surface", false, ToolsMenuLevels.CREATE_SURFACE)]
        private static Surface MenuCreateNewSurface()
        {
            Surface output = CreateSurface();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Surface", false, ToolsMenuLevels.CREATE_SURFACE)]
        private static Surface MenuCreateNewSurfaceB()
        {
            Surface output = CreateSurface();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        public string filePath
        {
            get
            {
                switch (_type)
                {
                    case SurfaceTypes.Material:
                        if (_material == null) return null;
                        return UnityEditor.AssetDatabase.GetAssetPath(_material);

#if !UNITY_2017_3_OR_NEWER
                    case SurfaceTypes.Substance:
                        if (_substance == null) return null;
                        return UnityEditor.AssetDatabase.GetAssetPath(_substance);
#endif

                    default:
                        return null;

                }
            }
        }
#endif
    }
    #endregion
}