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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

//TODO
//drag and drop within the facade design
//swapping elements
//drag and drop with control key (??) to copy elements

namespace BuildR2
{
    [CustomEditor(typeof(Facade))]
    public class FacadeEditor : Editor
    {
        private const int SECTION_PREVIEW_SIZE = 50;
        private Facade _facade;

        private static bool SHOW_BASE_PATTERN = true;
        private static string NAME_ERROR = "";
        private static Vector2 SCROLL_POS;
        
        private Mesh _mesh;
        private List<Material> _materialList;
        public static Vector2Int SIZE = new Vector2Int(5, 5);

        private void OnEnable()
        {
            _facade = (Facade)target;
            UpdatePreview();
            InteractivePreview.Reset();
        }

        private void UpdatePreview()
        {
            FacadeGenerator.FacadeData fData = new FacadeGenerator.FacadeData();
            Vector2 sectionSize = new Vector2(2,3);
            fData.baseA = new Vector3(0,0,0);
            fData.baseB = new Vector3(sectionSize.x * SIZE.x, 0, 0);
            fData.controlA = Vector3.zero;
            fData.controlB = Vector3.zero;
            List<Vector2Int> anchors = new List<Vector2Int>();
            fData.anchors = anchors;
            fData.isStraight = true;
            fData.curveStyle = VolumePoint.CurveStyles.Distance;
            fData.floorCount = SIZE.y;
            fData.facadeDesign = _facade;
            fData.startFloor = 0;
            fData.actualStartFloor = 0;
            fData.foundationDepth = 0;
            fData.wallThickness = 0.25f;
            fData.minimumWallUnitLength = sectionSize.x;
            fData.floorHeight = sectionSize.y;
            fData.meshType = BuildingMeshTypes.Full;
            fData.colliderType = BuildingColliderTypes.None;
            fData.cullDoors = false;
            fData.prefabs = null;

            BuildRMesh dMesh = new BuildRMesh("facade preview");
            FacadeGenerator.GenerateFacade(fData, dMesh);
            _mesh = new Mesh();
            dMesh.Build(_mesh);
            if (_materialList == null) _materialList = new List<Material>();
            _materialList.Clear();
            _materialList.AddRange(dMesh.materials);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            OnInspectorGUI_S(_facade);

//            GUILayout.Space(150);
//            DrawDefaultInspector();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Preview Settings");

            Vector2Int currentSize = new Vector2Int(SIZE);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X Units");
            if (GUILayout.Button("-"))
                if (currentSize.x > 1) currentSize.x--;
            currentSize.x = EditorGUILayout.IntField(currentSize.x);
            if (GUILayout.Button("+")) currentSize.x++;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Y Units");
            if (GUILayout.Button("-"))
                if (currentSize.y > 1) currentSize.y--;
            currentSize.y = EditorGUILayout.IntField(currentSize.y);
            if (GUILayout.Button("+")) currentSize.y++;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if(currentSize != SIZE)
            {
                SIZE = currentSize;
                UpdatePreview();
            }

            EditorGUILayout.EndVertical();

            if(GUI.changed)
                UpdatePreview();
        }


        public static void OnInspectorGUI_S(Facade facade)
        {
            BuildingEditor.BuildRHeader("Facade Design");


            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Name", GUILayout.Width(120));
            string newName = EditorGUILayout.DelayedTextField(facade.name);
            if (newName != facade.name)
                NAME_ERROR = BuildingEditorUtils.RenameAsset(facade, newName);
            EditorGUILayout.EndHorizontal();

            if (NAME_ERROR.Length > 0)
                EditorGUILayout.HelpBox(NAME_ERROR, MessageType.Error);

            BuildingEditor.GUIDivider();
            SHOW_BASE_PATTERN = EditorGUILayout.Foldout(SHOW_BASE_PATTERN, "Base Facade Pattern");
            if (SHOW_BASE_PATTERN)
            {
                EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                float bottomWidth = Mathf.Min((SECTION_PREVIEW_SIZE + 17) * facade.baseWidth, BuildingEditor.MAIN_GUI_WIDTH - SECTION_PREVIEW_SIZE);
                EditorGUILayout.BeginVertical(GUILayout.Width(bottomWidth));

                BasePatternEditor(facade);

                EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomWidth), GUILayout.Height(SECTION_PREVIEW_SIZE));
                if (GUILayout.Button("-", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE)))
                    facade.baseWidth--;
                EditorGUILayout.LabelField(String.Format("Pattern\nWidth:\n{0}", facade.baseWidth), GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
                GUILayout.Space(bottomWidth - SECTION_PREVIEW_SIZE * 3);
                if (GUILayout.Button("+", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE)))
                    facade.baseWidth++;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                float rightHeight = facade.baseHeight * SECTION_PREVIEW_SIZE + SECTION_PREVIEW_SIZE;
                EditorGUILayout.BeginVertical(GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(rightHeight));

                if (GUILayout.Button("+", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE)))
                    facade.baseHeight++;
                EditorGUILayout.LabelField(String.Format("Pattern\nHeight:\n{0}", facade.baseHeight), GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
                GUILayout.Space(rightHeight - SECTION_PREVIEW_SIZE*3);
                if (GUILayout.Button("-", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE)))
                    facade.baseHeight--;
                GUILayout.Space(40);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //TODO add back in
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ground Floor Pattern");
                facade.hasGroundFloorPattern = EditorGUILayout.Toggle(facade.hasGroundFloorPattern);
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(!facade.hasGroundFloorPattern);
                GroundPatternEditor(facade);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Pattern Anchor");
                facade.patternAnchors = (Facade.PatternAnchors)EditorGUILayout.EnumPopup(facade.patternAnchors);
                EditorGUILayout.EndHorizontal();

                //                EditorGUILayout.BeginHorizontal();
                //                EditorGUILayout.LabelField("Tiled Pattern");
                //                facade.tiled = EditorGUILayout.Toggle(facade.tiled);
                //                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Stretch Mode");
                facade.stretchMode = (Facade.StretchModes)EditorGUILayout.EnumPopup(facade.stretchMode);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("String Course");
                facade.stringCourse = EditorGUILayout.Toggle(facade.stringCourse);
//                if(GUILayout.Button("?", GUILayout.Width(20)))
//                    EditorUtility.DisplayDialog("Help", "Me", "No");
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(!facade.stringCourse);


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                facade.stringCourseHeight = EditorGUILayout.Slider(facade.stringCourseHeight, 0.01f, 0.5f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Depth");
                facade.stringCourseDepth = EditorGUILayout.Slider(facade.stringCourseDepth, 0.01f, 0.5f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vertical Position");
                facade.stringCoursePosition = EditorGUILayout.Slider(facade.stringCoursePosition, 0, 1);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Surface");
                facade.stringCourseSurface = EditorGUILayout.ObjectField(facade.stringCourseSurface, typeof(Surface), false) as Surface;
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Random Mode");
                facade.randomisationMode = (Facade.RandomisationModes)EditorGUILayout.EnumPopup(facade.randomisationMode);
                EditorGUILayout.EndHorizontal();



            }
        }

        private static void BasePatternEditor(Facade facade)
        {

            EventType eventType = Event.current.type;
//            if (eventType == EventType.DragUpdated)
//                Debug.Log("DragUpdated");
//            if (eventType == EventType.DragPerform)
//                Debug.Log("DragPerform");


            List<List<Rect>> facadeRects = new List<List<Rect>>();

            float calWidth = Mathf.Min((SECTION_PREVIEW_SIZE + 20) * facade.baseWidth, BuildingEditor.MAIN_GUI_WIDTH - SECTION_PREVIEW_SIZE);
            float calHeight = (SECTION_PREVIEW_SIZE + 30) * facade.baseHeight;
            SCROLL_POS = EditorGUILayout.BeginScrollView(SCROLL_POS, GUILayout.Width(calWidth), GUILayout.Height(calHeight));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(calWidth));
            for (int x = 0; x < facade.baseWidth; x++)
            {
                facadeRects.Add(new List<Rect>());
                EditorGUILayout.BeginVertical(GUILayout.Width(SECTION_PREVIEW_SIZE));
                for (int y = 0; y < facade.baseHeight; y++)
                {
                    int inverseY = facade.baseHeight - y - 1;
                    Rect itemrect = WallSectionItem(facade, x, inverseY);
                    facadeRects[x].Add(itemrect);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            WallSection output = null;
//            EventType eventType = Event.current.type;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                Object[] objArray = DragAndDrop.objectReferences;
                if (objArray != null)
                {
                    if (objArray[0].GetType() == typeof(WallSection))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        if (eventType == EventType.DragPerform)
                        {
                            output = (WallSection)objArray[0];
                            DragAndDrop.AcceptDrag();

                            Vector2 mousePos = Event.current.mousePosition;

                            for (int x = 0; x < facade.baseWidth; x++)
                            {
                                for (int y = 0; y < facade.baseHeight; y++)
                                {
                                    if (facadeRects[x][y].Contains(mousePos))
                                    {
                                        int inverseY = facade.baseHeight - y - 1;
                                        WallSection wallSection = facade.GetBaseWallSection(x, inverseY);
                                        if(output != wallSection)
                                        {
                                            Undo.RecordObject(facade, "Add wall section to facade");
                                            facade.SetBaseWallSection(x, inverseY, output);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Rect WallSectionItem(Facade facade, int x, int inverseY, bool isGround = false)
        {
            WallSection wallSection = !isGround ? facade.GetBaseWallSection(x, inverseY) : facade.GetGroundWallSection(x);
            string wallSectionName = wallSection != null ? wallSection.name : "";
            Undo.RecordObject(facade, "Add wall section to facade");
            GUIContent texture;
            if (wallSection != null)
                texture = new GUIContent(wallSection.previewTexture);
            else
                texture = new GUIContent(wallSectionName);
            EditorGUILayout.BeginHorizontal("box", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
            EditorGUILayout.LabelField(texture, GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
            EditorGUILayout.EndHorizontal();
            Rect sectionRect = GUILayoutUtility.GetLastRect();
//            facadeRects[x].Add(sectionRect);
            WallSection newSection = EditorGUI.ObjectField(sectionRect, wallSection, typeof(WallSection), false) as WallSection;
            if(newSection != wallSection)
            {
                if(!isGround)
                    facade.SetBaseWallSection(x, inverseY, newSection);
                else
                    facade.SetGroundWallSection(x, newSection);
            }
            EditorGUI.DrawPreviewTexture(sectionRect, EditorGUIUtility.whiteTexture);
            EditorGUI.LabelField(sectionRect, texture);
            EditorGUI.DropShadowLabel(sectionRect, wallSectionName, BuildingEditor.FacadeLabel);

            return sectionRect;
        }

        private static void GroundPatternEditor(Facade facade)
        {
            if(!facade.hasGroundFloorPattern) return;
            EventType eventType = Event.current.type;
//            if (eventType == EventType.DragUpdated)
//                Debug.Log("DragUpdated");
//            if (eventType == EventType.DragPerform)
//                Debug.Log("DragPerform");

            List<Rect> facadeRects = new List<Rect>();

            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < facade.baseWidth; x++)
            {
                
                Rect itemrect = WallSectionItem(facade, x, 0, true);
                facadeRects.Add(itemrect);

//                WallSection wallSection = facade.GetGroundWallSection(x);
                
                //                if(wallSection != null)
                //                {
                //                    EditorGUILayout.BeginHorizontal("box");
                //                    GUIContent texture = new GUIContent(wallSection.PreviewTexture());
                //                    EditorGUILayout.LabelField(texture, GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
                //                    EditorGUILayout.EndHorizontal();
                //                }
                //                else
                //                {
                //                    EditorGUILayout.BeginHorizontal("box");
                //                    EditorGUILayout.LabelField("", GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
                //                    EditorGUILayout.EndHorizontal();
                //                }
                //                facadeRects.Add(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();

            WallSection output = null;
            //            EventType eventType = Event.current.type;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                Object[] objArray = DragAndDrop.objectReferences;
                if (objArray != null)
                {
                    if (objArray[0].GetType() == typeof(WallSection))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        if (eventType == EventType.DragPerform)
                        {
                            output = (WallSection)objArray[0];
                            DragAndDrop.AcceptDrag();

                            Vector2 mousePos = Event.current.mousePosition;

                            for (int x = 0; x < facade.baseWidth; x++)
                            {
                                    if (facadeRects[x].Contains(mousePos))
                                    {
                                        WallSection wallSection = facade.GetGroundWallSection(x);
                                        if (output != wallSection)
                                            facade.SetGroundWallSection(x, output);
                                    }
                            }
                        }
                    }
                }
            }
        }
        
        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            InteractivePreview.OnInteractivePreviewGui(r, background, _mesh, _materialList.ToArray());
        }
    }
}