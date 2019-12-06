using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BuildR2.Genesis;
using UnityEditor;

namespace BuildR2
{
    [CustomEditor(typeof(Chimney))]
    public class ChimneyEditor : Editor
    {
        private Chimney _chimney;
        private Mesh _mesh;
        private List<Material> _materialList;

        private void OnEnable()
        {
            _chimney = target as Chimney;
            UpdatePreview();
            InteractivePreview.Reset();
        }

        private void UpdatePreview()
        {
            GenerationOutput output = GenerationOutput.CreateMeshOutput();
            ChimneyGenerator.Generate(_chimney, output);
            _mesh = output.mesh;
            if (_materialList == null) _materialList = new List<Material>();
            _materialList.Clear();
            _materialList.AddRange(ChimneyGenerator.DYNAMIC_MESH.materials);
        }

        public override void OnInspectorGUI()
        {
            BuildingEditor.BuildRHeader("Chimney");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Seed");
            _chimney.seed = (uint)EditorGUILayout.IntSlider((int)_chimney.seed, 1, 999999);
            EditorGUILayout.EndHorizontal();
            
            _chimney.noise = EditorGUILayout.Vector3Field("Noise",_chimney.noise);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Case");

            _chimney.caseSize = EditorGUILayout.Vector3Field("Case Size", _chimney.caseSize);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Case Surface");
            _chimney.caseSurface = EditorGUILayout.ObjectField(_chimney.caseSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Crown");

            _chimney.crownSize = EditorGUILayout.Vector3Field("Crown Size", _chimney.crownSize);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Crown Surface");
            _chimney.crownSurface = EditorGUILayout.ObjectField(_chimney.crownSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Flue");

            _chimney.flueSize = EditorGUILayout.Vector3Field("Flue Size", _chimney.flueSize);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Flue Shape");
            if (GUILayout.Button(_chimney.square ? "Square" : "Circle"))
                _chimney.square = !_chimney.square;
            EditorGUILayout.EndHorizontal();

            if (!_chimney.square)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Flue Segments");
				_chimney.segments = EditorGUILayout.IntSlider(_chimney.segments, 3, 20);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Flue Angle Offset");
				_chimney.angleOffset = EditorGUILayout.Slider(_chimney.angleOffset, 0, 360);
				EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Allow Multiple Flues");
            _chimney.allowMultiple = EditorGUILayout.Toggle(_chimney.allowMultiple);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Allow Multiple Flues Rows");
            _chimney.allowMultipleRows = EditorGUILayout.Toggle(_chimney.allowMultipleRows);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Flue Spacing");
            _chimney.flueSpacing = EditorGUILayout.Slider(_chimney.flueSpacing, 0.01f, 1);
            EditorGUILayout.EndHorizontal();

            SurfaceArrayEditor.Inspector(_chimney.flueSurfaces);
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Flue Surface");
//            _chimney.flueSurface = EditorGUILayout.ObjectField(_chimney.flueSurface, typeof(Surface), false) as Surface;
//            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Inner Surface");
            _chimney.innerSurface = EditorGUILayout.ObjectField(_chimney.innerSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cap Model");
            _chimney.cap = EditorGUILayout.ObjectField(_chimney.cap, typeof(Model), false) as Model;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_chimney);
                UpdatePreview();
                Repaint();
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            InteractivePreview.RESTRICT_ROTATION = false;
            InteractivePreview.OnInteractivePreviewGui(r, background, _mesh, _materialList.ToArray());
        }
    }
}
