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
using JaspLib;
using UnityEngine;
using UnityEditor;

namespace BuildR2
{
    public class BuildingRoofEditor
    {
        private static Color MAIN_LINE_COLOUR = new Color(1, 1, 1, 0.5f);
        private static Color SUB_LINE_COLOUR = new Color(1, 1, 1, 0.25f);
        private static Color HIGHLIGHTED_BLUEPRINT_COLOUR = new Color(0.2f, 0.6f, 0.6f, 0.5f);
        private static Color SELECTED_BLUEPRINT_COLOUR = new Color(0, 0, 0.8f, 0.5f);
        private static Color UNSELECTED_BLUEPRINT_COLOUR = new Color(0.1f, 0, 0.4f, 0.2f);
        public static bool REPAINT = false;

        public static void Repaint()
        {
            GUI.changed = true;
            REPAINT = true;
        }

        public static void OnEnable()
        {

        }

        public static void Cleanup()
        {

        }

        public static void OnSceneGUI(Building _building)
        {
            Vector3 position = _building.transform.position;
//            Quaternion rotation = _building.transform.rotation;

            bool shiftIsDown = Event.current.shift;
//            bool controlIsDown = Event.current.control;
//            bool altIsDown = Event.current.alt;

//            Camera sceneCamera = Camera.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);//sceneCamera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
            Event current = Event.current;
            IVolume mouseOverPlan = BuildrUtils.OnFloorplanSelectionClick(_building, ray).volume;
//            Volume clickedPlan = null;

            switch (current.type)
            {
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;

                case EventType.MouseDown:
//                    if (current.button == 0)
//                        clickedPlan = mouseOverPlan;
                    break;

                case EventType.ScrollWheel:

                    break;
            }

            Vector3 basePosition = position;
            if (BuildingEditor.volume != null)
                basePosition += BuildingEditor.volume.baseHeight * Vector3.up;
            Plane buildingPlane = new Plane(Vector3.up, basePosition);

            Vector3 mousePlanePoint = Vector3.zero;
            float mouseRayDistance;
            if (buildingPlane.Raycast(ray, out mouseRayDistance))
                mousePlanePoint = ray.GetPoint(mouseRayDistance);
            if (shiftIsDown)
            {
                mousePlanePoint.x = Mathf.Round(mousePlanePoint.x);
                mousePlanePoint.y = Mathf.Round(mousePlanePoint.y);
                mousePlanePoint.z = Mathf.Round(mousePlanePoint.z);
            }
//            float mouseHandleSize = HandleUtility.GetHandleSize(mousePlanePoint) * 0.2f;

            DrawFloorplans(_building, mouseOverPlan);

//            if (clickedPlan != null && clickedPlan != SELECTED_VOLUME)
//            {
//                SELECTED_VOLUME = clickedPlan;
//                Repaint();
//                current.Use();
//            }
        }

        public static void OnInspectorGUI(Building _building)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            Volume volume = BuildingVolumeEditor.VolumeSelectorInspectorGUI(_building);
            if(volume != null)
                BuildingEditor.volume = volume;
            else
                volume = BuildingEditor.volume;

            if (volume != null)
            {
                Undo.RecordObject(BuildingEditor.volume, "Roof Modification");
                Roof roof = volume.roof;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Enable");
                roof.exists = EditorGUILayout.Toggle(roof.exists);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Open Interior");
                roof.interiorOpen = EditorGUILayout.Toggle(roof.interiorOpen);
                EditorGUILayout.EndHorizontal();

//                EditorGUI.BeginDisabledGroup(BuildingEditor.volume.abovePlans.Count > 0);
//                if (BuildingEditor.volume.abovePlans.Count > 0)
//                    roof.type = Roof.Types.Flat;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type");
                roof.type = (Roof.Types)EditorGUILayout.EnumPopup(roof.type);
                EditorGUILayout.EndHorizontal();
//                EditorGUI.EndDisabledGroup();

                if (roof.type != Roof.Types.Flat)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Height");
                    roof.height = EditorGUILayout.Slider(roof.height, 0, 20);//TODO setting for max building roof height
                    EditorGUILayout.EndHorizontal();
                }

//                if (roof.type == Roof.Types.Gambrel)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Sub Height");
//                    roof.heightB = EditorGUILayout.Slider(roof.heightB, 0, 20);//TODO setting for max building roof height
//                    EditorGUILayout.EndHorizontal();
//                }

                if (roof.type == Roof.Types.Mansard)//|| roof.type == Roof.Types.Gambrel)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Depth");
                    roof.depth = EditorGUILayout.Slider(roof.depth, 0, 5);
                    EditorGUILayout.EndHorizontal();
                }

                if (roof.type == Roof.Types.Mansard)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Floor Depth");
                    roof.floorDepth = EditorGUILayout.Slider(roof.floorDepth, 0, 5);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Overhang");
                roof.overhang = EditorGUILayout.Slider(roof.overhang, 0, 5);
                EditorGUILayout.EndHorizontal();

                //                EditorGUILayout.BeginHorizontal();
                //two directions of the ridge
                //                string[] options = { "Short", "Long" };
                //                roof.direction = EditorGUILayout.Popup(roof.direction, options);
                //                EditorGUILayout.EndHorizontal();

//                if (roof.type == Roof.Types.Sawtooth)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Overhang");
//                    roof.sawtoothTeeth = EditorGUILayout.IntSlider(roof.sawtoothTeeth, 0, 10);
//                    EditorGUILayout.EndHorizontal();
//                }

//                if (roof.type == Roof.Types.Barrel)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Segments");
//                    roof.barrelSegments = EditorGUILayout.IntSlider(roof.barrelSegments, 0, 10);
//                    EditorGUILayout.EndHorizontal();
//                }

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Textures");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Main");
                roof.mainSurface = EditorGUILayout.ObjectField(roof.mainSurface, typeof(Surface), false) as Surface;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Wall");
                roof.wallSurface = EditorGUILayout.ObjectField(roof.wallSurface, typeof(Surface), false) as Surface;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Floor");
                roof.floorSurface = EditorGUILayout.ObjectField(roof.floorSurface, typeof(Surface), false) as Surface;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Parapet");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Enabled");
                roof.parapet = EditorGUILayout.Toggle(roof.parapet);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type");
                roof.parapetStyle = (Roof.ParapetStyles)EditorGUILayout.EnumPopup(roof.parapetStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                roof.parapetHeight = EditorGUILayout.Slider(roof.parapetHeight, 0.01f, 3);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Front Extrusion");
                roof.parapetFrontDepth = EditorGUILayout.Slider(roof.parapetFrontDepth, 0.01f, 1);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Back Extrusion");
                roof.parapetBackDepth = EditorGUILayout.Slider(roof.parapetBackDepth, 0.01f, 1);
                EditorGUILayout.EndHorizontal();

                if (roof.parapetStyle == Roof.ParapetStyles.Battlement)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Battlement Height Ratio");
                    roof.battlementHeightRatio = EditorGUILayout.Slider(roof.battlementHeightRatio, 0.01f, 1);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Battlement Spacing");
                    roof.battlementSpacing = EditorGUILayout.Slider(roof.battlementSpacing, 0.5f, 5);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                //DORMERS
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Dormer");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Enabled");
                roof.hasDormers = EditorGUILayout.Toggle(roof.hasDormers);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width");
                roof.dormerWidth = EditorGUILayout.Slider(roof.dormerWidth, 0, 4);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                roof.dormerHeight = EditorGUILayout.Slider(roof.dormerHeight, 0, 3);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roof Height");
                roof.dormerRoofHeight = EditorGUILayout.Slider(roof.dormerRoofHeight, 0, 3);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Spacing");
                roof.minimumDormerSpacing = EditorGUILayout.Slider(roof.minimumDormerSpacing, 0, 5);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rows");
                roof.dormerRows = EditorGUILayout.IntSlider(roof.dormerRows, 1, 4);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Style");
                roof.wallSection = (WallSection)EditorGUILayout.ObjectField(roof.wallSection, typeof(WallSection), false);
                EditorGUILayout.EndHorizontal();

                if (roof.wallSection != null)
                    GUILayout.Label(roof.wallSection.previewTexture);

                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Gables");

                for(int g = 0; g < volume.numberOfPoints; g++)
                {
                    if(g > 0) EditorGUILayout.Space();
                    RoofFacadeInspectorGUI(volume, g);
                }
                
                EditorGUILayout.EndVertical();

                BuildingEditor.volume.roof = roof;


                if (BuildingEditor.volume != null && BuildingEditor.volume.isModified)
                    _building.MarkModified();

                if (_building.isModified || GUI.changed)
                    Repaint();
            }
            EditorGUILayout.EndVertical();
        }

        public static void RoofFacadeInspectorGUI(Volume volume, int index)
        {
            VolumePoint point = volume[index];

//            GUIStyle facadeLabelStyle = new GUIStyle();
//            facadeLabelStyle.normal.background = new Texture2D();
            EditorGUILayout.LabelField(string.Format("Facade: {0}", index + 1));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Gabled", GUILayout.Width(60));
            point.isGabled = EditorGUILayout.Toggle(point.isGabled, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!point.isGabled);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Thickness");
            point.gableThickness = EditorGUILayout.Slider(point.gableThickness, 0.1f, 5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Additional Height");
            point.gableHeight = EditorGUILayout.Slider(point.gableHeight, 0.1f, 5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Simple Gable");
            point.simpleGable = EditorGUILayout.Toggle(point.simpleGable);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(point.simpleGable);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gable Design", GUILayout.Width(80));
            point.gableStyle = (Gable)EditorGUILayout.ObjectField(point.gableStyle, typeof(Gable), false);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            //            EditorGUILayout.BeginHorizontal();
            //            EditorGUILayout.LabelField("Thickness");
            //            _gable.thickness = EditorGUILayout.Slider(_gable.thickness, 0.1f, 5);
            //            EditorGUILayout.EndHorizontal();
        }

        private static void DrawFloorplans(IBuilding _building, IVolume mouseOverPlan)
        {
            int numberOfFloorplans = _building.numberOfPlans;
            Vector3 position = _building.transform.position;
            Quaternion rotation = _building.transform.rotation;

            Camera sceneCamera = Camera.current;

            Vector3[] centerPoints = new Vector3[numberOfFloorplans];
            for (int f = 0; f < numberOfFloorplans; f++)
                centerPoints[f] = BuildrUtils.CalculateFloorplanCenter(_building[f]);
            //            Ray ray = sceneCamera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
            //            Floorplan mouseOverPlan = BuildrUtils.OnFloorplanSelectionClick(_building, ray);

            //Draw Floorplan outlines
            for (int f = 0; f < numberOfFloorplans; f++)
            {
                IVolume volume = _building[f];
                int numberOfPoints = volume.numberOfPoints;
                Vector2 leftPoint = Vector2.right;
                Vector3 labelPoint = volume.BuildingPoint(0) + position;

                int numberOfTopPoints = 0;
                for (int p = 0; p < numberOfPoints; p++)
                {
                    if (volume.IsWallStraight(p))
                        numberOfTopPoints++;
                    else
                        numberOfTopPoints += 9;
                }
                Vector3[] planVs = new Vector3[numberOfTopPoints];
                int planVIndex = 0;
                Vector3 planUp = Vector3.up * volume.planHeight;

                for (int p = 0; p < numberOfPoints; p++)
                {
                    Vector3 p0 = volume.WorldPoint(p) + planUp;//rotation * floorplan.BuildingPoint(p) + position);
                    Vector2 p0ss = sceneCamera.WorldToScreenPoint(p0);
                    if (p0ss.x < leftPoint.x)
                    {
                        leftPoint = p0ss;
                        labelPoint = p0;
                    }
                    //Render handles
                    if (volume.IsWallStraight(p))
                    {
                        planVs[planVIndex] = p0;
                        planVIndex++;
                    }
                    else
                    {
                        Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints) + planUp;//rotation * floorplan.BuildingPoint((p + 1) % numberOfPoints) + position);
                        Vector3 cw0 = volume.WorldControlPointA(p) + planUp;//rotation * floorplan.BuildingControlPoint(p) + position);
                        Vector3 cw1 = volume.WorldControlPointB(p) + planUp;//rotation * floorplan.BuildingControlPoint(p) + position);
                        Vector3[] curveWall = new Vector3[10];
                        for (int t = 0; t < 10; t++)
                        {
                            Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);
                            curveWall[t] = cp;
                            if (t < 9)
                            {
                                planVs[planVIndex] = cp;
                                planVIndex++;
                            }
                        }
                    }
                }

                if ((Volume)volume == BuildingEditor.volume)
                    Handles.color = SELECTED_BLUEPRINT_COLOUR;
                else if (mouseOverPlan == volume)
                    Handles.color = HIGHLIGHTED_BLUEPRINT_COLOUR;
                else
                    Handles.color = UNSELECTED_BLUEPRINT_COLOUR;

                //                Handles.DrawAAConvexPolygon(planVs);//draw plan outline
                DrawConcavePolygonHandle.Shape(planVs, Handles.color);
//                Vector2 textDimensions = GUI.skin.label.CalcSize(new GUIContent(volume.name));
                Handles.Label(labelPoint, volume.name);

	            int linkCount = volume.linkPlanCount;
                Vector3 thisCenter = centerPoints[f];
                Handles.color = Color.green;
                for (int l = 0; l < linkCount; l++)
                {
                    if (f == l) continue;
//                    Volume link = volume.linkedPlans[l];
                    Vector3 linkCenter = centerPoints[l];
                    Handles.DrawDottedLine(thisCenter, linkCenter, 5);
                }

                int numberOfFloors = volume.floors;
                float planHeight = volume.floorHeight;
                float totalPlanHeight = volume.planHeight;
                Vector3 planHeightV = totalPlanHeight * Vector3.up;

                Handles.color = new Color(0, 0.2f, 1, 0.5f);

                for (int p = 0; p < numberOfPoints; p++)
                {
                    Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                    Vector3 p1 = rotation * volume.BuildingPoint((p + 1) % numberOfPoints) + position;

                    //Render handles
                    if (volume.IsWallStraight(p))
                    {
                        int wallSections = Mathf.FloorToInt(Vector3.Distance(p0, p1) / volume.minimumWallUnitLength);
                        if (wallSections < 1) wallSections = 1;
                        for (int ws = 0; ws < wallSections + 1; ws++)
                        {
                            float lerp = ws / ((float)wallSections);
                            Vector3 basePos = Vector3.Lerp(p0, p1, lerp);
                            Handles.DrawLine(basePos, basePos + planHeightV);
                        }

                        for (int fl = 0; fl < numberOfFloors + 1; fl++)
                        {
                            Handles.color = fl == 0 || fl == numberOfFloors ? MAIN_LINE_COLOUR : SUB_LINE_COLOUR;
                            float lineHeight = fl * planHeight;
                            Vector3 lineHeightV = lineHeight * Vector3.up;
                            Handles.DrawLine(p0 + lineHeightV, p1 + lineHeightV);
                        }
                    }
                    else
                    {
                        Vector3 cw0 = volume.WorldControlPointA(p);
                        Vector3 cw1 = volume.WorldControlPointB(p);
                        Vector3[] curveWall = new Vector3[10];
                        float arcLength = 0;
                        for (int t = 0; t < 10; t++)
                        {
                            Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);
                            curveWall[t] = cp;
                            if (t > 0) arcLength += Vector3.Distance(curveWall[t - 1], curveWall[t]);
                        }

                        for (int fl = 0; fl < numberOfFloors + 1; fl++)
                        {
                            Handles.color = fl == 0 || fl == numberOfFloors ? MAIN_LINE_COLOUR : SUB_LINE_COLOUR;
                            float lineHeight = fl * planHeight;
                            Vector3 lineHeightV = lineHeight * Vector3.up;

                            for (int t = 0; t < 9; t++)
                            {
                                Vector3 cwp0 = curveWall[t];
                                Vector3 cwp1 = curveWall[t + 1];
                                Handles.DrawLine(cwp0 + lineHeightV, cwp1 + lineHeightV);
                            }
                            //                            Handles.DrawLine(p0 + lineHeightV, p1 + lineHeightV);
                        }
                    }

                    Handles.color = MAIN_LINE_COLOUR;
                    Handles.DrawLine(p0, p0 + Vector3.up * totalPlanHeight);

                    if ((Volume)volume == BuildingEditor.volume)
                    {
                        Vector3 facadeCenter = Vector3.Lerp(p0, p1, 0.5f) + planUp;
//                        float gtbSize = HandleUtility.GetHandleSize(facadeCenter) * 0.1f;
//                        Handles.Label(facadeCenter, "Is gabled");
//                        Handles.Button(facadeCenter, Quaternion.identity, gtbSize, gtbSize, Handles.DotCap);

                        Handles.BeginGUI();
                        Vector2 portalScreenPos = Camera.current.WorldToScreenPoint(facadeCenter);
                        portalScreenPos.y = Camera.current.pixelHeight - portalScreenPos.y;
                        Rect screenRect = new Rect(portalScreenPos, new Vector2(350, 500));
                        GUILayout.BeginArea(screenRect);
                        
                        EditorGUILayout.LabelField(string.Format("Facade: {0}", p + 1));
                        //                        RoofFacadeInspectorGUI(volume, p);

                        GUILayout.EndArea();
                        Handles.EndGUI();
                    }
                }
            }
        }
    }
}
