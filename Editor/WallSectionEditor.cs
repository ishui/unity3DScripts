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
using UnityEditor;

namespace BuildR2
{
    [CustomEditor(typeof(WallSection))]
    public class WallSectionEditor : Editor
    {
        private BuildRSettings _settings;

        public Material blueprintMaterial;

        private WallSection _wallSection;
        private Mesh _plane;
        private Mesh _mesh;
	    private List<Material> _materialList;

        public Vector2 previewMeshSize = new Vector2(2,3);
        private static int previewMode = 0;

        private void OnEnable()
        {
            _wallSection = (WallSection)target;
            _plane = Primitives.Plane(10);
            _settings = BuildingEditor.GetSettings();

            UpdatePreview();
//            InteractivePreview.Reset();
        }

        private void UpdatePreview()
        {
//            if (_submeshLibrary == null) _submeshLibrary = new SubmeshLibrary();
//            _submeshLibrary.Clear();
//            _submeshLibrary.Add(_wallSection);
//            _wallSection.UpdatePreviewMesh(_submeshLibrary);
//            if (_materialList == null) _materialList = new List<Material>();
//            _materialList.Clear();
//            _materialList.AddRange(WallSectionGenerator.DYNAMIC_MESH.materials);

//            BuildRMesh dMesh = new BuildRMesh("wallsection preview");
            GenerationOutput output = GenerationOutput.CreateMeshOutput();
            WallSectionGenerator.Generate(_wallSection, output, previewMeshSize, false, 0.2f);
            _mesh = output.mesh;
            if (_materialList == null) _materialList = new List<Material>();
            _materialList.Clear();
            _materialList.AddRange(WallSectionGenerator.DYNAMIC_MESH.materials);
        }

        private void OnDisable()
        {
//            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            BuildingEditor.BuildRHeader("Wall Section");

            Undo.RecordObject(_wallSection, "Wall Section Modification");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(80));
            string newName = EditorGUILayout.DelayedTextField(_wallSection.name);
            if (newName != _wallSection.name)
                BuildingEditorUtils.RenameAsset(_wallSection, newName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Has Opening");
            _wallSection.hasOpening = EditorGUILayout.Toggle(_wallSection.hasOpening);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Door");
            _wallSection.isDoor = EditorGUILayout.Toggle(_wallSection.isDoor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Vertical Restriction");
            _wallSection.verticalRestriction = (WallSection.VerticalRestrictions)EditorGUILayout.EnumPopup(_wallSection.verticalRestriction);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Custom Models");

            //TODO SOON!
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Wall Section Model");
//            _wallSection.model = EditorGUILayout.ObjectField(_wallSection.model, typeof(Model), false) as Model;
//            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening Model");
            EditorGUILayout.BeginVertical();
            _wallSection.portal = EditorGUILayout.ObjectField(_wallSection.portal, typeof(Portal), false) as Portal;
            _wallSection.openingModel = EditorGUILayout.ObjectField(_wallSection.openingModel, typeof(Model), false) as Model;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Balcony Model");
            _wallSection.balconyModel = EditorGUILayout.ObjectField(_wallSection.balconyModel, typeof(BuildR2.Model), false) as Model;
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(_wallSection.balconyModel == null);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Balcony Height");
            _wallSection.balconyHeight = EditorGUILayout.Slider(_wallSection.balconyHeight, 0.01f, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Balcony Side Overhang");
            _wallSection.balconySideOverhang = EditorGUILayout.Slider(_wallSection.balconySideOverhang, 0, 0.56f);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Shutter Model");
            _wallSection.shutterModel = EditorGUILayout.ObjectField(_wallSection.shutterModel, typeof(Model), false) as Model;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dimentions Type");
            _wallSection.dimensionType = (WallSection.DimensionTypes)EditorGUILayout.EnumPopup(_wallSection.dimensionType);
            EditorGUILayout.EndHorizontal();

            if (_wallSection.dimensionType == WallSection.DimensionTypes.Absolute)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Opening Width");
                _wallSection.openingWidth = EditorGUILayout.Slider(_wallSection.openingWidth, _settings.minimumWindowWidth, _settings.maximumWindowWidth);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Opening Height");
                _wallSection.openingHeight = EditorGUILayout.Slider(_wallSection.openingHeight, _settings.minimumWindowHeight, _settings.maximumWindowHeight);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Opening Width");
                _wallSection.openingWidth = EditorGUILayout.Slider(_wallSection.openingWidth, 0, 1);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Opening Height");
                _wallSection.openingHeight = EditorGUILayout.Slider(_wallSection.openingHeight, 0, 1);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening Depth");
            _wallSection.openingDepth = EditorGUILayout.Slider(_wallSection.openingDepth, _settings.minimumWindowDepth, _settings.maximumWindowDepth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening Width Ratio");
            _wallSection.openingWidthRatio = EditorGUILayout.Slider(_wallSection.openingWidthRatio, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening Height Ratio");
            _wallSection.openingHeightRatio = EditorGUILayout.Slider(_wallSection.openingHeightRatio, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening is Arched");
            _wallSection.isArched = EditorGUILayout.Toggle(_wallSection.isArched);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!_wallSection.isArched);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Arch Height");
            _wallSection.archHeight = EditorGUILayout.Slider(_wallSection.archHeight, 0.1f, _wallSection.openingHeight);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Arch Curve");
            _wallSection.archCurve = EditorGUILayout.Slider(_wallSection.archCurve, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Arch Segments");
            _wallSection.archSegments = EditorGUILayout.IntSlider(_wallSection.archSegments, 1, 20);
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

	        EditorGUILayout.BeginHorizontal();
	        EditorGUILayout.LabelField("Bay Extruded");
	        _wallSection.bayExtruded = EditorGUILayout.Toggle(_wallSection.bayExtruded);
	        EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!_wallSection.bayExtruded);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Extrusion");
            _wallSection.bayExtrusion = EditorGUILayout.Slider(_wallSection.bayExtrusion, 0.1f, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!_wallSection.bayExtruded);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bay Bevel");
            _wallSection.bayBevel = EditorGUILayout.Slider(_wallSection.bayBevel, 0.1f, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Sill");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enabled");
            _wallSection.extrudedSill = EditorGUILayout.Toggle(_wallSection.extrudedSill);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!_wallSection.extrudedSill);

            Vector3 sillDimentions = _wallSection.extrudedSillDimentions;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Overshoot");
            sillDimentions.x = EditorGUILayout.FloatField(sillDimentions.x);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height");
            sillDimentions.y = EditorGUILayout.FloatField(sillDimentions.y);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Depth");
            sillDimentions.z = EditorGUILayout.FloatField(sillDimentions.z);
            EditorGUILayout.EndHorizontal();

            _wallSection.extrudedSillDimentions = sillDimentions;

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Lintel");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enabled");
            _wallSection.extrudedLintel = EditorGUILayout.Toggle(_wallSection.extrudedLintel);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!_wallSection.extrudedLintel);

            Vector3 lintelDimenstions = _wallSection.extrudedLintelDimentions;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Overshoot");
            lintelDimenstions.x = EditorGUILayout.FloatField(lintelDimenstions.x);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height");
            lintelDimenstions.y = EditorGUILayout.FloatField(lintelDimenstions.y);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Depth");
            lintelDimenstions.z = EditorGUILayout.FloatField(lintelDimenstions.z);
            EditorGUILayout.EndHorizontal();

            _wallSection.extrudedLintelDimentions = lintelDimenstions;

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening Frame");
            _wallSection.openingFrame = EditorGUILayout.Toggle(_wallSection.openingFrame);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size");
            _wallSection.openingFrameSize = EditorGUILayout.Slider(_wallSection.openingFrameSize, 0.02f, 0.25f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Extrusion");
            _wallSection.openingFrameExtrusion = EditorGUILayout.Slider(_wallSection.openingFrameExtrusion, 0.0f, 0.1f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Surfaces");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wall");
            _wallSection.wallSurface = EditorGUILayout.ObjectField(_wallSection.wallSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opening");
            _wallSection.openingSurface = EditorGUILayout.ObjectField(_wallSection.openingSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sill");
            _wallSection.sillSurface = EditorGUILayout.ObjectField(_wallSection.sillSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ceiling");
            _wallSection.ceilingSurface = EditorGUILayout.ObjectField(_wallSection.ceilingSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            
			EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                Repaint();
                _wallSection.GenereateData();
                UpdatePreview();
                _wallSection.SaveData();
                
                UpdatePreview();

                EditorUtility.SetDirty(_wallSection);
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }
        
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            InteractivePreview.PLANE = _plane;
            InteractivePreview.PLANE_MATERIAL = blueprintMaterial;
            InteractivePreview.RESTRICT_ROTATION = false;
            InteractivePreview.OnInteractivePreviewGui(r, background, _mesh, _materialList.ToArray());

        }

//        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
//        {
//            _drag = Drag2D(_drag, r);
//
//            if (_mPrevRender == null)
//                _mPrevRender = new PreviewRenderUtility();
//
//            Vector3 max = _wallSection.previewMesh.bounds.size;
//            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 1.333f;
//            float dist = radius / (Mathf.Sin(_mPrevRender.m_Camera.fieldOfView * Mathf.Deg2Rad));
//            _mPrevRender.m_Camera.transform.position = Vector2.zero;
//            _mPrevRender.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
//            _mPrevRender.m_Camera.transform.position = _mPrevRender.m_Camera.transform.forward * -dist;
//            _mPrevRender.m_Camera.nearClipPlane = 0.1f;
//            _mPrevRender.m_Camera.farClipPlane = 500;
//
//            _mPrevRender.m_Light[0].intensity = 0.5f;
//            _mPrevRender.m_Light[0].transform.rotation = Quaternion.Euler(30f, 230f, 0f);
//
//            _mPrevRender.BeginPreview(r, background);
//
//            if (_plane != null && blueprintMaterial != null)
//            {
//                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(-25, -25, 1), Quaternion.identity, new Vector3(10, 10, 1));
//                _mPrevRender.DrawMesh(_plane, matrix, blueprintMaterial, 0);
//                _mPrevRender.m_Camera.Render();
//            }
//
//
//	        Material[] mats = _materialList.ToArray();//_submeshLibrary.GetMaterials();
//            int materialCount = mats.Length;//_submeshLibrary.MATERIALS.Count;//_wallSection.usedSurfaces.Length;
//            int submeshCount = _wallSection.previewMesh.subMeshCount;
//            int count = materialCount > 0 ? Mathf.Min(materialCount, submeshCount) : submeshCount;
////            Debug.Log(materialCount+" "+ submeshCount+" "+ _submeshLibrary.MATERIALS.Count+" "+ _submeshLibrary.SUBMESH_COUNT);
//            for (int c = 0; c < count; c++)
//            {
//                Material mat = c < materialCount ? mats[c] : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                _mPrevRender.DrawMesh(_wallSection.previewMesh, Matrix4x4.identity, mat, c);
//            }
//
//            if (_wallSection.balconyModel != null && _wallSection.balconyModel.type != Model.Types.Mesh)
//            {
//                DisplayModel(_wallSection.balconyModel, _wallSection.BalconyMeshPosition(_wallSection.PreviewMeshSize(), 0.1f));
//            }
//            _mPrevRender.m_Camera.Render();
//            Texture texture = _mPrevRender.EndPreview();
//
//            GUI.DrawTexture(r, texture);
//        }
//
//        private void DisplayModel(Model model, Matrix4x4 matrix)
//        {
//            Mesh[] meshes = model.GetMeshes();
//            Model.MaterialArray[] materials = model.GetMaterials();
//            int meshCount = meshes.Length;
//            for (int ms = 0; ms < meshCount; ms++)
//            {
//                Mesh mesh = meshes[ms];
//                Material[] mats = materials[ms].materials;
//                int materialCount = mats.Length;
//                int submeshCount = Mathf.Max(1, mesh.subMeshCount);
//                int count = Mathf.Max(materialCount, submeshCount);
//                for (int c = 0; c < count; c++)
//                {
//                    Material mat = c < materialCount ? mats[c] : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                    if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                    _mPrevRender.DrawMesh(mesh, matrix, mat, c);
//                }
//            }
//        }
//
//        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
//        {
//            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
//            Event current = Event.current;
//            switch (current.GetTypeForControl(controlID))
//            {
//                case EventType.MouseDown:
//                    if (position.Contains(current.mousePosition) && position.width > 50f)
//                    {
//                        GUIUtility.hotControl = controlID;
//                        current.Use();
//                        EditorGUIUtility.SetWantsMouseJumping(1);
//                    }
//                    break;
//                case EventType.MouseUp:
//                    if (GUIUtility.hotControl == controlID)
//                    {
//                        GUIUtility.hotControl = 0;
//                    }
//                    EditorGUIUtility.SetWantsMouseJumping(0);
//                    break;
//                case EventType.MouseDrag:
//                    if (GUIUtility.hotControl == controlID)
//                    {
//                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
//                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
//                        current.Use();
//                        GUI.changed = true;
//                    }
//                    break;
//            }
//            return scrollPosition;
//        }
    }
}