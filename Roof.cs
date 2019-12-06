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
    [Serializable]
    public class Roof
    {
        #region Main Variables

        public enum Types
        {
            Flat,
            Pitched,
            //LeanTo,
            Mansard,
            //Gambrel,
            //Barrel,
            //Spire,
            //Sawtooth
        }

        [SerializeField]
        private bool _exists = true;
        [SerializeField]
        private bool _interiorOpen = false;
        [SerializeField]
        private Types _type = Types.Flat;
        [SerializeField]
        private float _height = 2.0f;
        [SerializeField]
        private float _heightB = 1.5f;
        [SerializeField]
        private float _depth = 1.0f;//used for mansard roofs
        [SerializeField]
        private float _floorDepth = 1.0f;//used for mansard roofs
        [SerializeField]
        private float _overhang = 0.0f;
        [SerializeField]
        private int _direction = 0;//used for placing ridges
        [SerializeField]
        private int _sawtoothTeeth = 4;
        [SerializeField]
        private int _barrelSegments = 20;

        public bool exists
        {
            get { return _exists; }
            set
            {
                if (_exists != value)
                {
                    _exists = value;
                    _modified = true;
                }
            }
        }

        public bool interiorOpen
        {
            get { return _interiorOpen; }
            set
            {
                if (_interiorOpen != value)
                {
                    _interiorOpen = value;
                    _modified = true;
                }
            }
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

        public float height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    _modified = true;
                }
            }
        }

        public float heightB
        {
            get { return _heightB; }
            set
            {
                if (_heightB != value)
                {
                    _heightB = value;
                    _modified = true;
                }
            }
        }

        public float depth
        {
            get { return _depth; }
            set
            {
                if (_depth != value)
                {
                    _depth = value;
                    _modified = true;
                }
            }
        }

        public float floorDepth
        {
            get { return _floorDepth; }
            set
            {
                if (_floorDepth != value)
                {
                    _floorDepth = value;
                    _modified = true;
                }
            }
        }

        public float overhang
        {
            get { return _overhang; }
            set
            {
                if (_overhang != value)
                {
                    _overhang = Mathf.Max(value, 0);
                    _modified = true;
                }
            }
        }

        public int direction
        {
            get { return _direction; }
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    _modified = true;
                }
            }
        }

        public int sawtoothTeeth
        {
            get { return _sawtoothTeeth; }
            set
            {
                if (_sawtoothTeeth != value)
                {
                    _sawtoothTeeth = value;
                    _modified = true;
                }
            }
        }

        public int barrelSegments
        {
            get { return _barrelSegments; }
            set
            {
                if (_barrelSegments != value)
                {
                    _barrelSegments = value;
                    _modified = true;
                }
            }
        }
        #endregion

        #region Surfaces


        [SerializeField]
        private Surface _mainSurface;
        [SerializeField]
        private Surface _wallSurface;
        [SerializeField]
        private Surface _floorSurface;

        public Surface mainSurface
        {
            get { return _mainSurface; }
            set
            {
                if (_mainSurface != value)
                {
                    _mainSurface = value;
                    _modified = true;
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
                    _modified = true;
                }
            }
        }

        public Surface floorSurface
        {
            get { return _floorSurface; }
            set
            {
                if (_floorSurface != value)
                {
                    _floorSurface = value;
                    _modified = true;
                }
            }
        }

        public bool hasBlankSurfaces
        {
            get
            {
                if(_mainSurface == null) return true;
                if(_wallSurface == null) return true;
                if(_floorSurface == null && _type == Types.Mansard) return true;
                return false;
            }
        }

        #endregion

        #region Parapet

        //parapet
        public enum ParapetStyles
        {
            Flat,
            Battlement
        }

        [SerializeField]
        private bool _parapet = true;//small wall extending the facade somewhat
        [SerializeField]
        private ParapetStyles _parapetStyle = ParapetStyles.Flat;
        [SerializeField]
        private float _parapetHeight = 0.25f;
        [SerializeField]
        private float _parapetFrontDepth = 0.1f;
        [SerializeField]
        private float _parapetBackDepth = 0.2f;
        [SerializeField]
        private float _battlementHeightRatio = 0.55f;
        [SerializeField]
        private float _battlementSpacing = 0.55f;

        public bool parapet
        {
            get { return _parapet; }
            set
            {
                if (_parapet != value)
                {
                    _parapet = value;
                    _modified = true;
                }
            }
        }

        public ParapetStyles parapetStyle
        {
            get { return _parapetStyle; }
            set
            {
                if (_parapetStyle != value)
                {
                    _parapetStyle = value;
                    _modified = true;
                }
            }
        }

        public float parapetHeight
        {
            get { return _parapetHeight; }
            set
            {
                if (_parapetHeight != value)
                {
                    _parapetHeight = value;
                    _modified = true;
                }
            }
        }

        public float parapetFrontDepth
        {
            get { return _parapetFrontDepth; }
            set
            {
                if (_parapetFrontDepth != value)
                {
                    _parapetFrontDepth = value;
                    _modified = true;
                }
            }
        }

        public float parapetBackDepth
        {
            get { return _parapetBackDepth; }
            set
            {
                if (_parapetBackDepth != value)
                {
                    _parapetBackDepth = value;
                    _modified = true;
                }
            }
        }

        public float battlementHeightRatio
        {
            get { return _battlementHeightRatio; }
            set
            {
                if (_battlementHeightRatio != value)
                {
                    _battlementHeightRatio = Mathf.Clamp01(value);
                    _modified = true;
                }
            }
        }

        public float battlementSpacing
        {
            get { return _battlementSpacing; }
            set
            {
                if (_battlementSpacing != value)
                {
                    _battlementSpacing = value;
                    _modified = true;
                }
            }
        }
        #endregion

        #region Dormers

        //dormer
        [SerializeField]
        private bool _hasDormers = false;
        [SerializeField]
        private float _dormerWidth = 1.25f;
        [SerializeField]
        private float _dormerHeight = 0.85f;
        [SerializeField]
        private float _dormerRoofHeight = 0.25f;
        [SerializeField]
        private float _minimumDormerSpacing = 0.5f;
        [SerializeField]
        private int _dormerRows = 1;
        [SerializeField]
        private WallSection _wallSection;

        public bool hasDormers
        {
            get { return _hasDormers; }
            set
            {
                if (_hasDormers != value)
                {
                    _hasDormers = value;
                    _modified = true;
                }
            }
        }

        public float dormerWidth
        {
            get { return _dormerWidth; }
            set
            {
                if (_dormerWidth != value)
                {
                    _dormerWidth = value;
                    _modified = true;
                }
            }
        }

        public float dormerHeight
        {
            get { return _dormerHeight; }
            set
            {
                if (_dormerHeight != value)
                {
                    _dormerHeight = value;
                    _modified = true;
                }
            }
        }

        public float dormerRoofHeight
        {
            get { return _dormerRoofHeight; }
            set
            {
                if (_dormerRoofHeight != value)
                {
                    _dormerRoofHeight = value;
                    _modified = true;
                }
            }
        }

        public float minimumDormerSpacing
        {
            get { return _minimumDormerSpacing; }
            set
            {
                if (_minimumDormerSpacing != value)
                {
                    _minimumDormerSpacing = value;
                    _modified = true;
                }
            }
        }

        public int dormerRows
        {
            get { return _dormerRows; }
            set
            {
                if (_dormerRows != value)
                {
                    _dormerRows = value;
                    _modified = true;
                }
            }
        }

        public WallSection wallSection
        {
            get { return _wallSection; }
            set
            {
                if (_wallSection != value)
                {
                    _wallSection = value;
                    _modified = true;
                }
            }
        }
        #endregion

        private bool _modified;

        public bool modified
        {
            get { return _modified; }
        }

        public void MarkUnmodified()
        {
            _modified = false;
        }

//        public bool Equals(Roof p)
//        {
//            if (_exists != p._exists) return false;
//            if (_interiorOpen != p._interiorOpen) return false;
//            if (_type != p._type) return false;
//            if (_height != p._height) return false;
//            if (_heightB != p._heightB) return false;
//            if (_depth != p._depth) return false;
//            if (_floorDepth != p._floorDepth) return false;
//            if (_overhang != p._overhang) return false;
//            if (_direction != p._direction) return false;
//            if (_sawtoothTeeth != p._sawtoothTeeth) return false;
//            if (_barrelSegments != p._barrelSegments) return false;
//            if (_parapet != p._parapet) return false;
//            if (_parapetStyle != p._parapetStyle) return false;
//            if (_parapetHeight != p._parapetHeight) return false;
//            if (_parapetFrontDepth != p._parapetFrontDepth) return false;
//            if (_parapetBackDepth != p._parapetBackDepth) return false;
//            if (_hasDormers != p._hasDormers) return false;
//            if (_dormerWidth != p._dormerWidth) return false;
//            if (_dormerHeight != p._dormerHeight) return false;
//            if (_dormerRoofHeight != p._dormerRoofHeight) return false;
//            if (_minimumDormerSpacing != p._minimumDormerSpacing) return false;
//            if (_dormerHeightRatio != p._dormerHeightRatio) return false;
//            if (_wallSection != p._wallSection) return false;
//            return true;
//        }
//
//        public bool Equals(UnityEngine.Object a)
//        {
//            return Equals(a);// (Dot(this, a) > 0.999f);
//        }
//
//        public override bool Equals(object a)
//        {
//            return Equals(a);// (Dot(this, a) > 0.999f);
//        }
//
//        public static bool operator ==(Roof a, Roof b)
//        {
//            return a.Equals(b);
//        }
//
//        public static bool operator !=(Roof a, Roof b)
//        {
//            return !a.Equals(b);
//        }
    }
}