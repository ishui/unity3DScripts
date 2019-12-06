using UnityEngine;
using UnityEditor;

namespace BuildR2
{

    [CustomEditor(typeof(BuildRAtlasSO))]
    public class BuildRAtlasEditor : Editor
    {
        private BuildRAtlasSO _wallSection;

        private void OnEnable()
        {
            _wallSection = (BuildRAtlasSO)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Export"))
                _wallSection.UpdateAtlas();
        }
    }
}
