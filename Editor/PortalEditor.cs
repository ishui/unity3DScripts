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
    [CustomEditor(typeof(Portal))]
    public class PortalEditor : Editor
    {
        private const float DIVISION_NAME_WIDTH = 150;
        private const float DIVISION_ATTRIBUTE_NAME_WIDTH = 120;
        private const float DIVISION_ATTRIBUTE_VALUE_WIDTH = 180;

        private Portal _portal;
        private PreviewRenderUtility _mPrevRender;
        private Vector2 _drag;
        private Mesh _plane;
        public Material _mat;
        private bool isModified;
        private Vector2 treeScroll;
        private static string nameError = "";
		private SubmeshLibrary _submeshLibrary;

		private void OnEnable()
        {
            _portal = (Portal)target;
            _plane = Primitives.Plane(10);
			//            _mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/BuildR2/Materials/Blueprint.mat");

			if (_submeshLibrary == null) _submeshLibrary = new SubmeshLibrary();
			_submeshLibrary.Clear();
			_submeshLibrary.Add(_portal);
			_portal.UpdatePreviewMesh(_submeshLibrary);
		}


        public override void OnInspectorGUI()
        {
            if (_portal.type == Portal.Types.Window)
            {
                Undo.RecordObject(_portal, "Window Modification");
                BuildingEditor.BuildRHeader("Window");
            }
            else
            {
                Undo.RecordObject(_portal, "Door Modification");
                BuildingEditor.BuildRHeader("Door");
            }

            GUILayout.Space(5);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(120));
            string newName = EditorGUILayout.DelayedTextField(_portal.name);
            if (newName != _portal.name)
                nameError = BuildingEditorUtils.RenameAsset(_portal, newName);
            EditorGUILayout.EndHorizontal();

            if (nameError.Length > 0)
                EditorGUILayout.HelpBox(nameError, MessageType.Error);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(120));
            _portal.type = (Portal.Types)EditorGUILayout.EnumPopup(_portal.type);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();
            if(_portal.defaultFrameTexture != null)
                GUILayout.Label(_portal.defaultFrameTexture.previewTexture, GUILayout.Width(75), GUILayout.Height(75));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Default Frame Texture");
            EditorGUILayout.BeginHorizontal();
            _portal.defaultFrameTexture = EditorGUILayout.ObjectField(_portal.defaultFrameTexture, typeof(Surface), false) as Surface;
            if (GUILayout.Button("?", GUILayout.Width(20)))
                EditorUtility.DisplayDialog("Help Text", "TODO", "ok");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Default Panel Texture");
            EditorGUILayout.BeginHorizontal();
            _portal.defaultPanelTexture = EditorGUILayout.ObjectField(_portal.defaultPanelTexture, typeof(Surface), false, GUILayout.Width(75), GUILayout.Height(75)) as Surface;
            if (GUILayout.Button("?", GUILayout.Width(20)))
                EditorUtility.DisplayDialog("Help Text", "TODO", "ok");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            treeScroll = EditorGUILayout.BeginScrollView(treeScroll);
            DivisionInspector(_portal.root, 0);
            EditorGUILayout.EndScrollView();

            _portal.CheckModification();

//            GUILayout.Space(100);
//            base.OnInspectorGUI();
        }

        public void DivisionInspector(Division division, int depth)
        {
            if (division == null) return;

            bool isRoot = division == _portal.root;

            EditorGUILayout.BeginHorizontal();

            if (!division.expanded)
            {
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    division.expanded = true;
                    division.MarkModified();
                }
            }
            else
            {
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    division.expanded = false;
                    division.MarkModified();
                }
            }

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            if (!division.expanded)
            {
                if (isRoot)
                    EditorGUILayout.LabelField("Root", GUILayout.Width(DIVISION_NAME_WIDTH));
                else
                    EditorGUILayout.LabelField(division.name, GUILayout.Width(DIVISION_NAME_WIDTH));
            }
            else
            {
                if (isRoot)
                    EditorGUILayout.LabelField("Root", GUILayout.Width(DIVISION_NAME_WIDTH));
                else
                    division.name = EditorGUILayout.DelayedTextField(division.name, GUILayout.Width(DIVISION_NAME_WIDTH));
            }

            EditorGUILayout.Space();

            if (!isRoot)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(55)))
                {
                    _portal.Remove(division);
                    return;
                }
            }

            if (GUILayout.Button("Add Child", GUILayout.Width(65)))
            {
                division.children.Add(new Division());
                _portal.MarkModified();
            }

            EditorGUILayout.EndHorizontal();

            if (division.expanded)
            {
                EditorGUILayout.Space();

                if(division.hasChildren)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Frame", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                    GUILayout.FlexibleSpace();
                    division.frame = EditorGUILayout.Slider(division.frame, 0, 1, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Recess", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                    GUILayout.FlexibleSpace();
                    division.recess = EditorGUILayout.Slider(division.recess, 0, 1, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                    EditorGUILayout.EndHorizontal();
                }

                if(!isRoot)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Size", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                    GUILayout.FlexibleSpace();
                    division.size = EditorGUILayout.Slider(division.size, 0, 5, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                    EditorGUILayout.EndHorizontal();
                }

                //TODO add later when supported
//                EditorGUILayout.BeginHorizontal();
//                EditorGUILayout.LabelField("Shape", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
//                GUILayout.FlexibleSpace();
//                division.type = (Portal.ShapeTypes)EditorGUILayout.EnumPopup(division.type, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
//                EditorGUILayout.EndHorizontal();

//                if (division.frame > 0)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField(" Texture", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
//                    GUILayout.FlexibleSpace();
//                    division.surface = (Surface)EditorGUILayout.ObjectField(division.surface, typeof(Surface), false, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
//                    EditorGUILayout.EndHorizontal();
//                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Texture", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                GUILayout.FlexibleSpace();
                division.surface = (Surface)EditorGUILayout.ObjectField(division.surface, typeof(Surface), false, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                EditorGUILayout.EndHorizontal();

                if (division.hasChildren)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Division Type", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                    GUILayout.FlexibleSpace();
                    division.divisionType = (Portal.DivisionTypes)EditorGUILayout.EnumPopup(division.divisionType, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Idential Children", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                GUILayout.FlexibleSpace();
                division.identicalChildren = EditorGUILayout.Toggle(division.identicalChildren, GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                EditorGUILayout.EndHorizontal();
                
                if (division.identicalChildren)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Idential Child Count", GUILayout.Width(DIVISION_ATTRIBUTE_NAME_WIDTH));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(DIVISION_ATTRIBUTE_VALUE_WIDTH));
                    if (GUILayout.Button("-"))
                        division.identicalChildCount--;
                    division.identicalChildCount = EditorGUILayout.IntField(division.identicalChildCount);
                    if (GUILayout.Button("+"))
                        division.identicalChildCount++;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.Space();
            }
            
            int childCount = division.children.Count;
            if (division.identicalChildren)
            {
                if (childCount > 0)
                    DivisionInspector(division.children[0], depth + 1);
            }
            else
            {
                for (int c = 0; c < childCount; c++)
                {
                    DivisionInspector(division.children[c], depth + 1);
                    childCount = division.children.Count;//recheck this incase of modifications
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
	        List<Surface> surfaces = _portal.UsedSurfaces();
            int matCount = surfaces.Count;
//            if(_portal.hasBlankSurfaces) matCount++;
            Material[] mats = new Material[matCount];
//            int blankOffset = 0;
//            if(_portal.hasBlankSurfaces)
//            {
//                mats[0] = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                matCount--;
//                blankOffset++;
//            }
            for(int m = 0; m < matCount; m++)
            {
//                Debug.Log(_portal);
//                Debug.Log(m);
//                Debug.Log(m - blankOffset);
//                Debug.Log(_portal.usedSurfaces[m - blankOffset]);
//                Debug.Log(_portal.usedSurfaces[m - blankOffset].name);
//                mats[m + blankOffset] = _portal.usedSurfaces[m].material;
	            mats[m] = surfaces[m].material;
            }
            Vector3 max = _portal.previewMesh.bounds.size;
            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 1.333f;

            BuildingEditorUtils.OnInteractivePreviewGUI(ref _mPrevRender, ref _drag, _portal.previewMesh, mats, r, background, radius, _plane, _mat);
//            _drag = Drag2D(_drag, r);
//
//            if (_mPrevRender == null)
//                _mPrevRender = new PreviewRenderUtility();
//
//            Vector3 max = _portal.previewMesh.bounds.size;
//            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 1.333f;
//            float dist = radius / (Mathf.Sin(_mPrevRender.m_Camera.fieldOfView * Mathf.Deg2Rad));
//            _mPrevRender.m_Camera.transform.position = Vector2.zero;
//            _mPrevRender.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
//            _mPrevRender.m_Camera.transform.position = _mPrevRender.m_Camera.transform.forward * -dist;
//            _mPrevRender.m_Camera.nearClipPlane = 0.1f;
//            _mPrevRender.m_Camera.farClipPlane = 500;
//
//            _mPrevRender.m_Light[0].intensity = 0.5f;
//            _mPrevRender.m_Light[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
//            _mPrevRender.m_Light[1].intensity = 0.5f;
//            
//            _mPrevRender.BeginPreview(r, background);
//
//            if (_plane != null && _mat != null)//background
//            {
//                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(-25, -25, 1), Quaternion.identity, new Vector3(10, 10, 1));
//                _mPrevRender.DrawMesh(_plane, matrix, _mat, 0);
//                _mPrevRender.m_Camera.Render();
//            }
//
//            /*
//                    Material mat = _wallSection.usedMaterials[i];
//                    if(mat == null)
//                        mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");*/
//
//            Surface[] surfaces = _portal.usedSurfaces;
//            for(int m = 0; m < surfaces.Length; m++)
//            {
//                Material mat = surfaces[m].material;
//                if(mat == null)
//                    mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                _mPrevRender.DrawMesh(_portal.previewMesh, Matrix4x4.identity, mat, m);
//            }
//
//
//
//
//            //            for (int i = 0; i < 4; i++)
//            //            {
//            //                if (_portal.usedMaterials[0] != null)
//            ////                    continue;
////            _mPrevRender.DrawMesh(_portal.previewMesh, Matrix4x4.identity, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"), 0);
////            }
//
//            _mPrevRender.m_Camera.Render();
//            Texture texture = _mPrevRender.EndPreview();
//
//            GUI.DrawTexture(r, texture);
        }

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
