using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BuildR2
{
    public class Chimney : ScriptableObject
    {
        public uint seed = 1;
        public Vector3 noise = new Vector3(0.02f, 0.1f, 0.02f);
        
        [SerializeField]
        private Vector3 _caseSize = new Vector3(1, 3, 1);
        [SerializeField]
        private Vector3 _crownSize = new Vector3(1, 0.25f, 1f);
        [SerializeField]
        private Vector3 _flueSize = new Vector3(0.2f, 0.5f, 0.2f);
        [SerializeField]
        public bool square = true;
        [SerializeField]
        public int segments = 8;
		public float angleOffset = 0;
        [SerializeField]
        private bool _allowMultiple = true;
        [SerializeField]
        private bool _allowMultipleRows = true;
        [SerializeField]
        private float _flueSpacing = 0.1f;
        [SerializeField]
        private Model _cap;
        
        public Surface caseSurface;
        public Surface crownSurface;
        public List<Surface> flueSurfaces;
        public Surface innerSurface;

        public Model cap { get { return _cap; } set { _cap = value; } }

        public float flueSpacing { get { return _flueSpacing; } set { _flueSpacing = Mathf.Max(0.01f, value); } }

        public bool allowMultipleRows { get { return _allowMultipleRows; } set { _allowMultipleRows = value; } }

        public bool allowMultiple { get { return _allowMultiple; } set { _allowMultiple = value; } }

        public Vector3 flueSize
        {
            get
            {
                return _flueSize;
            }
            set
            {
                _flueSize = value;
                if(_flueSize.x < 0.1f) _flueSize.x = 0.1f;
                if(_flueSize.y < 0.1f) _flueSize.y = 0.1f;
                if(_flueSize.z < 0.1f) _flueSize.z = 0.1f;
                if (_flueSize.x > crownSize.x) _flueSize.x = crownSize.x;
                if (_flueSize.z > crownSize.z) _flueSize.z = crownSize.z;
            }
        }

        public Vector3 crownSize
        {
            get
            {
                return _crownSize;
            }
            set
            {
                _crownSize = value;
                if (_crownSize.x < 0.1f) _crownSize.x = 0.1f;
                if (_crownSize.y < 0.1f) _crownSize.y = 0.1f;
                if (_crownSize.z < 0.1f) _crownSize.z = 0.1f;
            }
        }

        public Vector3 caseSize
        {
            get
            {
                return _caseSize;
            }
            set
            {
                _caseSize = value;
                if (_caseSize.x < 0.1f) _caseSize.x = 0.1f;
                if (_caseSize.y < 0.1f) _caseSize.y = 0.1f;
                if (_caseSize.z < 0.1f) _caseSize.z = 0.1f;
            }
        }


        public List<Surface> UsedSurfaces()
        {
            List<Surface> usedSurfaces = new List<Surface>();

            if (caseSurface != null) usedSurfaces.Add(caseSurface);
            if (crownSurface != null) usedSurfaces.Add(crownSurface);
            if (flueSurfaces != null && flueSurfaces.Count > 0) usedSurfaces.AddRange(flueSurfaces);
            if (innerSurface != null) usedSurfaces.Add(innerSurface);

            return usedSurfaces;
        }

        #region statics
        public static Chimney CreateChimney(string name = null, string directory = null)
        {
            Chimney chimney = CreateInstance<Chimney>();
            if (name != null)
                chimney.name = name;
#if UNITY_EDITOR
            if (directory == null)
                UnityEditor.AssetDatabase.CreateAsset(chimney, AssetCreator.GeneratePath("newChimney.asset", ""));
            else
                UnityEditor.AssetDatabase.CreateAsset(chimney, Path.Combine(directory, "newChimney.asset"));
#endif
            return chimney;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Chimney", false, ToolsMenuLevels.CREATE_CHIMNEY)]
        private static Chimney MenuCreateNewChimney()
        {
            Chimney output = CreateChimney();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Chimney", false, ToolsMenuLevels.CREATE_CHIMNEY)]
        private static Chimney MenuCreateNewChimneynB()
        {
            string activeFolder = AssetCreator.ActiveSelectionPath();
            Chimney output = CreateChimney(null, activeFolder);
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }
}