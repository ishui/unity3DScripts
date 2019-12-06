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
    public class BuildingVolumeEditor
    {

        private static Color MAIN_LINE_COLOUR = new Color(1, 1, 1, 0.5f);
        private static Color SUB_LINE_COLOUR = new Color(1, 1, 1, 0.25f);
        private static Color HIGHLIGHTED_BLUEPRINT_COLOUR = new Color(0.2f, 0.6f, 0.6f, 0.5f);
        private static Color SELECTED_BLUEPRINT_COLOUR = new Color(0, 0, 0.8f, 0.5f);
        private static Color UNSELECTED_BLUEPRINT_COLOUR = new Color(0.1f, 0, 0.4f, 0.2f);
        private static Color REMOVE_BLUEPRINT_COLOUR = new Color(1.0f, 0, 0.4f, 0.2f);
        private static Color REMOVE_BLUEPRINT_HIGHLIGHT_COLOUR = new Color(1.0f, 0, 0.4f, 0.8f);
        private static Color REMOVAL_COLOUR = new Color(1, 0, 0, 0.78f);
        private static Color SELECTED_POINT_COLOUR = new Color(0, 1, 0, 0.78f);

        public enum EditModes
        {
            NewVolumeDrawLines,
            NewVolumePlace,
            NewVolumeRect,
            SelectCurveWall,
            FloorplanSelection,
            RemovePlan,
            PointManipulation,
            LinkPoints,
            DelinkPoints,
            LinkFloorplans,
            UnlinkFloorplans,
            ControlPointManipulation,
            PointAddition,
            PointRemoval,
            WallExtrusion,
            WallExtrusionNewVolume,
            SwitchToInterior
        }
        
        private static List<int> _selectedPoints = new List<int>();
        private static int _selectedControlPoint = -1;
        public static EditModes mode = EditModes.PointManipulation;
        private static List<Vector2Int> NEW_FLOORPLAN_LINES = new List<Vector2Int>();
        private static Vector2Int newRectStart;
        private static float newPlanSquareSize = 20f;
        private static Vector2 newVolumeButtonScroll = Vector2.zero;
        private static Vector2 volumeScroll = Vector2.zero;
        private static VolumePoint unlinkedPoint = null;
        private static bool showVolumePoints = false;

        public static bool repaint = false;

        public static void Repaint()
        {
            GUI.changed = true;
            repaint = true;
        }

        public static void OnEnable()
        {
            NEW_FLOORPLAN_LINES.Clear();
            newRectStart = Vector2Int.zero;
            newPlanSquareSize = BuildingEditor.GetSettings().newPlanSquareSize;
            unlinkedPoint = null;
        }

        public static void Cleanup()
        {
            _selectedPoints.Clear();
        }

        private static bool canSelectOtherFloorplan
        {
            get
            {
                if (mode == EditModes.RemovePlan) return false;
                if (mode == EditModes.LinkPoints) return false;
                if (mode == EditModes.DelinkPoints) return false;
                if (mode == EditModes.PointAddition) return false;
                if (mode == EditModes.WallExtrusion) return false;
                if (mode == EditModes.WallExtrusionNewVolume) return false;
                if (mode == EditModes.PointRemoval) return false;
                return true;
            }
        }

        public static void OnSceneGUI(Building _building)
        {
            BuildRSettings settings = _building.settings;
            int numberOfFloorplans = _building.numberOfPlans;
            Vector3 position = _building.transform.position;
            Quaternion rotation = _building.transform.rotation;

            bool shiftIsDown = Event.current.shift;
            bool controlIsDown = Event.current.control;
            bool altIsDown = Event.current.alt;
            bool rightMouse = false;

//            Camera sceneCamera = Camera.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);//sceneCamera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
            Event current = Event.current;
            Volume mouseOverPlan = BuildrUtils.OnFloorplanSelectionClick(_building, ray).volume;
            Volume clickedVolume = null;

            switch (current.type)
            {
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;

                case EventType.MouseDown:
                    if (current.button == 0 && canSelectOtherFloorplan)
                    {
                        clickedVolume = mouseOverPlan;

                        if (mode == EditModes.RemovePlan)
                        {
                            if (EditorUtility.DisplayDialog("Deleting Floor Plan", "Are you sure you want to delete this Volume? \n Any connecting plans above will be remove also", "Delete", "Cancel"))
                            {
                                _building.RemovePlan(BuildingEditor.volume);
                                BuildingEditor.volume = null;
                                _selectedPoints.Clear();
                                _selectedControlPoint = -1;
                                Repaint();
                            }
                        }
                    }
                    if (current.button == 1)
                    {
                        rightMouse = true;
                        HandleUtility.Repaint();
                    }
                    break;

                case EventType.ScrollWheel:

                    if ((altIsDown || controlIsDown) && mode == EditModes.NewVolumePlace)
                    {
                        float delta = current.delta.y;
                        newPlanSquareSize = Mathf.Max(newPlanSquareSize + delta, 5);
                    }
                    break;

                case EventType.KeyDown:

                    switch (current.keyCode)
                    {
                        case KeyCode.Escape:
                            newRectStart = Vector2Int.zero;
                            break;
                    }

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
            bool snapToGrid = shiftIsDown || settings.snapToGrid;
            if (snapToGrid)
            {
                mousePlanePoint.x = Mathf.Round(mousePlanePoint.x);
                //mousePlanePoint.y = Mathf.Round(mousePlanePoint.y);
                mousePlanePoint.z = Mathf.Round(mousePlanePoint.z);

                newPlanSquareSize = Mathf.Round(newPlanSquareSize / 2) * 2;//ensure value it even
            }
            float mouseHandleSize = HandleUtility.GetHandleSize(mousePlanePoint) * 0.2f;
            //            for (int f = 0; f < numberOfFloorplans; f++)
            //            {

            if (_selectedPoints == null) unlinkedPoint = null;

            switch (mode)
            {
                case EditModes.NewVolumePlace:

                    Tools.current = Tool.None;
                    BuildingEditor.SimpleOrigin(_building);

                    Vector3 p0 = mousePlanePoint + new Vector3(-1, 0, -1) * newPlanSquareSize * 0.5f;
                    Vector3 p1 = mousePlanePoint + new Vector3(1, 0, -1) * newPlanSquareSize * 0.5f;
                    Vector3 p2 = mousePlanePoint + new Vector3(1, 0, 1) * newPlanSquareSize * 0.5f;
                    Vector3 p3 = mousePlanePoint + new Vector3(-1, 0, 1) * newPlanSquareSize * 0.5f;
                    Handles.color = settings.selectedBlueprintColour;
                    Handles.DrawAAConvexPolygon(p0, p1, p2, p3);
                    Handles.color = Color.white;
                    Handles.DrawAAPolyLine(p0, p1, p2, p3, p0);

                    Handles.color = settings.mainLineColour;
                    UnityVersionWrapper.HandlesDotCap(0, mousePlanePoint, Quaternion.identity, mouseHandleSize * 0.1f);

                    if (!rightMouse)
                    {
                        if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, mouseHandleSize))
                        {
                            Undo.SetCurrentGroupName("Create New Volume");
                            Undo.RecordObject(_building, "Create New Volume");
                            IVolume newPlan = _building.AddPlan(mousePlanePoint, newPlanSquareSize);
                            //                        if (FLOORPLAN != null)
                            //                            FLOORPLAN.LinkPlans(newPlan);
                            BuildingEditor.volume = (Volume)newPlan;
                            _selectedPoints.Clear();
                            _selectedControlPoint = -1;
                            mode = EditModes.PointManipulation;
                        }
                    }
                    break;

                case EditModes.NewVolumeRect:

                    Tools.current = Tool.None;
                    BuildingEditor.SimpleOrigin(_building);

                   UnityVersionWrapper.HandlesDotCap(0, mousePlanePoint, Quaternion.identity, mouseHandleSize * 0.1f);

                    if (newRectStart == Vector2Int.zero)
                    {
                        if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, mouseHandleSize))
                            newRectStart = new Vector2Int(mousePlanePoint, true);
                    }
                    else
                    {
                        FlatBounds bounds = new FlatBounds();
                        bounds.Encapsulate(mousePlanePoint.x, mousePlanePoint.z);
                        bounds.Encapsulate(newRectStart.vector2);

                        Vector3 r0 = new Vector3(bounds.xMax, mousePlanePoint.y, bounds.yMin);
                        Vector3 r1 = new Vector3(bounds.xMin, mousePlanePoint.y, bounds.yMin);
                        Vector3 r2 = new Vector3(bounds.xMin, mousePlanePoint.y, bounds.yMax);
                        Vector3 r3 = new Vector3(bounds.xMax, mousePlanePoint.y, bounds.yMax);
                        Handles.color = SELECTED_BLUEPRINT_COLOUR;
                        Handles.DrawAAConvexPolygon(r0, r1, r2, r3);
                        Handles.color = Color.white;
                        Handles.DrawAAPolyLine(r0, r1, r2, r3, r0);

                        if (!rightMouse)
                        {
                            if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, mouseHandleSize))
                            {
                                Undo.SetCurrentGroupName("Create New Volume");
                                Undo.RecordObject(_building, "Create New Volume");
                                IVolume newPlan = _building.AddPlan(bounds.rect);
                                BuildingEditor.volume = (Volume)newPlan;
                                _selectedPoints.Clear();
                                _selectedControlPoint = -1;
                                mode = EditModes.PointManipulation;
                            }
                        }
                    }
                    break;

                case EditModes.NewVolumeDrawLines:

                    Tools.current = Tool.None;
                    BuildingEditor.SimpleOrigin(_building);

                    UnityVersionWrapper.HandlesDotCap(0, mousePlanePoint, Quaternion.identity, mouseHandleSize * 0.25f);

                    Handles.color = Color.white;
                    int pointCount = NEW_FLOORPLAN_LINES.Count;
                    Vector3 baseUp = Vector3.up * position.y;
                    if (pointCount > 1)
                    {
                        for (int i = 1; i < pointCount; i++)
                        {
                            Vector3 l0 = NEW_FLOORPLAN_LINES[i - 1].vector3XZ + baseUp;
                            Vector3 l1 = NEW_FLOORPLAN_LINES[i].vector3XZ + baseUp;
                            Handles.DrawLine(l0, l1);
                        }
                    }

                    if (pointCount > 0)
                    {
                        Vector3 end = NEW_FLOORPLAN_LINES[pointCount - 1].vector3XZ + baseUp;
                        Handles.DrawLine(end, mousePlanePoint);
                    }

                    Handles.color = SELECTED_POINT_COLOUR;
                    bool allowNewPoint = true;
                    if (pointCount > 2)
                    {
                        Vector3 start = NEW_FLOORPLAN_LINES[0].vector3XZ + baseUp;
                        float hSize = HandleUtility.GetHandleSize(start) * 0.2f;
                        if (UnityVersionWrapper.HandlesDotButton(start, Quaternion.identity, hSize, hSize))
                        {
                            Undo.SetCurrentGroupName("Create New Volume");
                            Undo.RecordObject(_building, "Create New Volume");
                            IVolume newPlan = _building.AddPlan(NEW_FLOORPLAN_LINES.ToArray());
                            BuildingEditor.volume = (Volume)newPlan;
                            _selectedPoints.Clear();
                            _selectedControlPoint = -1;
                            mode = EditModes.PointManipulation;
                        }

                        float newPointDistance = Vector3.Distance(start, mousePlanePoint);
                        allowNewPoint = newPointDistance > 2;
                    }

                    if (allowNewPoint && UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, mouseHandleSize))
                    {
                        NEW_FLOORPLAN_LINES.Add(new Vector2Int(mousePlanePoint, true));
                    }

                    break;
            }

            DrawVolumes(_building, (Volume)mouseOverPlan);


            if (BuildingEditor.volume != null)
            {
                Undo.RecordObject(BuildingEditor.volume, "Volume Modification");
                Volume volume = BuildingEditor.volume;
                int numberOfPoints = volume.numberOfPoints;
                //                float baseHeight = volume.baseHeight;
                //                Vector3 baseHeightVector = baseHeight * Vector3.up;
                switch (mode)
                {
                    case EditModes.PointManipulation:

                        //Modification

                        Vector3 sliderPos = Vector3.zero;
                        for (int p = 0; p < numberOfPoints; p++)
                        {
                            Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                            Vector3 p1 = rotation * volume.BuildingPoint((p+1)%numberOfPoints) + position;
                            float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;

                            if (!_selectedPoints.Contains(p))
                            {
                                Handles.color = _selectedPoints.Count == 0 ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 0.2f);
                                if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize * 2))
                                {
                                    if (!shiftIsDown) _selectedPoints.Clear();//do not multi select
                                    _selectedPoints.Add(p);
                                    _selectedControlPoint = -1;
                                    Repaint();
                                }
                                Handles.color = new Color(1, 1, 1, 0.5f);
                            }
                            else
                            {
                                sliderPos += p0;
                            }

                            Vector3 c0V3 = volume.WorldControlPointA(p);//rotation * (c0.vector3XZ + baseHeightVector)) + position;
                            Vector3 c1V3 = volume.WorldControlPointB(p);//rotation * (c0.vector3XZ + baseHeightVector)) + position;
                            float hSizeA = HandleUtility.GetHandleSize(c0V3) * 0.2f;
                            float hSizeB = HandleUtility.GetHandleSize(c1V3) * 0.2f;

                            Vector3 facadeCenter = p0;
                            if (!volume[p].IsWallStraight())
                            {
                                facadeCenter = FacadeSpline.Calculate(p0, c0V3, c1V3, p1, 0.5f);
                                if (_selectedControlPoint != p)
                                {
                                    Handles.color = _selectedControlPoint == -1 ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 0.2f);
                                    if (UnityVersionWrapper.HandlesDotButton(c0V3, Quaternion.identity, hSizeA * 0.4f, hSizeA * 0.4f))
                                    {
                                        if (_selectedControlPoint != p)
                                        {
                                            _selectedControlPoint = p;
                                            _selectedPoints.Clear();
                                            clickedVolume = null;
                                        }
                                    }
                                    if (UnityVersionWrapper.HandlesDotButton(c1V3, Quaternion.identity, hSizeB * 0.4f, hSizeB * 0.4f))
                                    {
                                        if (_selectedControlPoint != p)
                                        {
                                            _selectedControlPoint = p;
                                            _selectedPoints.Clear();
                                            clickedVolume = null;
                                        }
                                    }
                                }
                                else
                                {
                                    Vector3 newPos = UnityVersionWrapper.HandlesFreeMoveHandle(c0V3, Quaternion.identity, hSizeA, new Vector3(Vector2Int.SCALE, 0, Vector2Int.SCALE));
                                    Handles.color = Color.red;
                                    newPos = Handles.Slider(newPos, Vector3.right);
                                    Handles.color = Color.blue;
                                    newPos = Handles.Slider(newPos, Vector3.forward);
                                    newPos = Quaternion.Inverse(rotation) * (newPos - position);
                                    Vector2Int newV2 = new Vector2Int(newPos, true);

                                    if (volume.GetControlPointA(p) != newV2)
                                    {
                                        volume.SetControlPointA(p, newV2);
                                        _building.MarkModified();
                                    }

                                    newPos = UnityVersionWrapper.HandlesFreeMoveHandle(c1V3, Quaternion.identity, hSizeB, new Vector3(Vector2Int.SCALE, 0, Vector2Int.SCALE));
                                    Handles.color = Color.red;
                                    newPos = Handles.Slider(newPos, Vector3.right);
                                    Handles.color = Color.blue;
                                    newPos = Handles.Slider(newPos, Vector3.forward);
                                    newPos = Quaternion.Inverse(rotation) * (newPos - position);
                                    newV2 = new Vector2Int(newPos, true);

                                    if (volume.GetControlPointB(p) != newV2)
                                    {
                                        volume.SetControlPointB(p, newV2);
                                        _building.MarkModified();
                                    }
                                }
                            }
                            else
                            {
                                facadeCenter = Vector3.Lerp(p0, p1, 0.5f);
                            }

                            Vector3 pointLabelPosition = p0;
                            pointLabelPosition.y += -volume.floorHeight * 0.5f;
                            Handles.Label(pointLabelPosition, string.Format("Point {0}", p + 1), BuildingEditor.SceneLabel);
                            facadeCenter.y += (volume.floorHeight * volume.floors) * 0.5f;
                            string facadeDesignName = volume.GetFacade(p) != null ? string.Format("\n{0}", volume.GetFacade(p).name) : "\nNo Facade Design Set(BulidingVolumeEditor)";
                            string facadeLabelContent = string.Format("Facade {0}{1}", p + 1, facadeDesignName);
                            Handles.Label(facadeCenter, facadeLabelContent, BuildingEditor.SceneLabel);

                        }

                        int numberOfSelectedPoints = _selectedPoints.Count;

                        //selected point scene gui
                        if (numberOfSelectedPoints > 0)
                        {
                            //                    Undo.SetSnapshotTarget(plan, "Floorplan Node Moved");
                            sliderPos /= numberOfSelectedPoints;

                            float hSize = HandleUtility.GetHandleSize(sliderPos);
                            Vector3 dirX = (sliderPos.x < 0) ? Vector3.right : Vector3.left;
                            Vector3 dirZ = (sliderPos.z < 0) ? Vector3.forward : Vector3.back;
                            //                            sliderPos += position;
                            Vector3 newSliderPos;
                            Handles.color = Color.green;
                            newSliderPos = UnityVersionWrapper.HandlesFreeMoveHandle(sliderPos, Quaternion.identity, hSize * 0.1f, new Vector3(0.1f, 0, 0.1f));
                            if ((newSliderPos - sliderPos).sqrMagnitude > 0)
                                newSliderPos = mousePlanePoint;
                            Handles.color = Color.red;
                            newSliderPos = UnityVersionWrapper.HandlesSlider(newSliderPos, dirX, hSize * 0.666f, 0.0f);
                            Handles.color = Color.blue;
                            newSliderPos = UnityVersionWrapper.HandlesSlider(newSliderPos, dirZ, hSize * 0.666f, 0.0f);

                            if (snapToGrid)
                            {
                                newSliderPos.x = Mathf.Round(newSliderPos.x);
                                newSliderPos.z = Mathf.Round(newSliderPos.z);
                            }

                            Vector3 sliderDiff = Quaternion.Inverse(rotation) * (newSliderPos - sliderPos);
                            if (sliderDiff.sqrMagnitude > 0)
                            {
                                BuildingEditor.volume.MarkModified();
                                for (int sp = 0; sp < numberOfSelectedPoints; sp++)
                                {
                                    volume[_selectedPoints[sp]].position = volume[_selectedPoints[sp]].position.Move(sliderDiff, true);
                                    if (unlinkedPoint != null)
                                        if (unlinkedPoint == volume[_selectedPoints[sp]]) volume[_selectedPoints[sp]].ClearMovement();
                                    _building.MarkModified();
                                }
                            }
                        }

                        break;

                    case EditModes.ControlPointManipulation:

                        break;

                    case EditModes.LinkPoints:

                        if (_selectedPoints.Count != 1)
                        {
                            if (_selectedPoints.Count > 1)
                                _selectedPoints.Clear();

                            for (int p = 0; p < numberOfPoints; p++)
                            {
                                Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                                float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;


                                Handles.color = _selectedPoints.Count == 0 ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 0.2f);
                                if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                                {
                                    _selectedPoints.Clear();//do not multi select
                                    _selectedPoints.Add(p);
                                    clickedVolume = null;
                                    Repaint();
                                }
                            }
                        }
                        else if (_selectedPoints.Count == 1)
                        {
                            int selectedPointIndex = _selectedPoints[0];
                            Vector3 sP = volume.WorldPoint(selectedPointIndex);
                            Handles.color = Color.green;
                            Handles.DrawLine(sP, mousePlanePoint);

                            for (int f = 0; f < numberOfFloorplans; f++)
                            {
                                IVolume other = _building[f];
                                int pCount = other.numberOfPoints;
                                if (BuildingEditor.volume == (Volume)_building[f])//link plan point to a point within the same plan
                                {
                                    for (int p = 0; p < pCount; p++)
                                    {
                                        Vector3 p0 = other.WorldPoint(p);
                                        float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;

                                        if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                                        {
                                            Undo.RecordObject((Volume)other, "Point Linkage");
                                            volume[selectedPointIndex] = other[p];
                                            _building.MarkModified();
                                            Repaint();
                                            clickedVolume = null;
                                            mode = EditModes.PointManipulation;
                                        }
                                    }
                                }
                                else//link to a point on another plan
                                {
                                    for (int p = 0; p < pCount; p++)
                                    {
                                        Vector3 p0 = other.WorldPoint(p);
                                        float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;

//                                        if (clickedVolume) Debug.Log("OnSceneGUI");
                                        if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                                        {
                                            //                                            Debug.Log("OnSceneGUI button click");
                                            Undo.RecordObject((Volume)other, "Point Linkage");
                                            volume[selectedPointIndex] = other[p];
                                            _building.MarkModified();
                                            Repaint();
                                            clickedVolume = null;
                                            mode = EditModes.PointManipulation;
                                            //                                            Debug.Log("link point " + clickedPlan);
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    case EditModes.DelinkPoints:

                        if (_selectedPoints.Count != 1)
                        {
                            if (_selectedPoints.Count > 1)
                                _selectedPoints.Clear();

                            for (int p = 0; p < numberOfPoints; p++)
                            {
                                Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                                float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;

                                Handles.color = _selectedPoints.Count == 0 ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 0.2f);
                                if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                                {
                                    _selectedPoints.Clear();//do not multi select
                                    _selectedPoints.Add(p);
                                    unlinkedPoint = volume[p];

                                    //                                    Vector2Int movement = new Vector2Int(-Camera.current.transform.forward);
                                    //                                    if (snapToGrid)
                                    //                                    {
                                    //                                        movement.x = Mathf.CeilToInt(Mathf.Sign(movement.x));
                                    //                                        movement.y = Mathf.CeilToInt(Mathf.Sign(movement.y));
                                    //                                    }
                                    //                                    volume[p].position += movement;
                                    //                                    volume[p].ClearMovement();
                                    //                                    _building.MarkModified();
                                    clickedVolume = null;
                                    Repaint();
                                    mode = EditModes.PointManipulation;
                                }
                            }
                        }
                        //                        else if (_selectedPoints.Count == 1)
                        //                        {
                        //                            int selectedPointIndex = _selectedPoints[0];
                        //                            Vector3 sP = volume.WorldPoint(selectedPointIndex);
                        //                            Handles.color = Color.green;
                        //                            Handles.DrawLine(sP, mousePlanePoint);
                        //
                        //                            for (int f = 0; f < numberOfFloorplans; f++)
                        //                            {
                        //                                Volume other = _building[f];
                        //                                int pCount = other.numberOfPoints;
                        //                                if (BuildingEditor.VOLUME == _building[f])//link plan point to a point within the same plan
                        //                                {
                        //                                    for (int p = 0; p < pCount; p++)
                        //                                    {
                        //                                        Vector3 p0 = other.WorldPoint(p);
                        //                                        float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;
                        //
                        //                                        if (Handles.Button(p0, Quaternion.identity, hSize, hSize, Handles.DotCap))
                        //                                        {
                        //                                            Undo.RecordObject(other, "Point Delink");
                        //                                            Vector2Int movement = new Vector2Int(-Camera.current.transform.forward);
                        //                                            if(snapToGrid)
                        //                                            {
                        //                                                movement.x = Mathf.CeilToInt(Mathf.Sign(movement.x));
                        //                                                movement.y = Mathf.CeilToInt(Mathf.Sign(movement.y));
                        //                                            }
                        //                                            volume[selectedPointIndex].position += movement;
                        //                                            volume[selectedPointIndex].ClearMovement();
                        //                                            _building.MarkModified();
                        //                                            Repaint();
                        //                                            clickedVolume = null;
                        //                                            mode = EditModes.PointManipulation;
                        //                                        }
                        //                                    }
                        //                                }
                        //                                else//link to a point on another plan
                        //                                {
                        //                                    for (int p = 0; p < pCount; p++)
                        //                                    {
                        //                                        Vector3 p0 = other.WorldPoint(p);
                        //                                        float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;
                        //
                        //                                        if (clickedVolume) Debug.Log("OnSceneGUI");
                        //                                        if (Handles.Button(p0, Quaternion.identity, hSize, hSize, Handles.DotCap))
                        //                                        {
                        //                                            //                                            Debug.Log("OnSceneGUI button click");
                        //                                            Undo.RecordObject(other, "Point Linkage");
                        //                                            Vector2Int movement = new Vector2Int(-Camera.current.transform.forward);
                        //                                            if (snapToGrid)
                        //                                            {
                        //                                                movement.x = Mathf.CeilToInt(Mathf.Sign(movement.x));
                        //                                                movement.y = Mathf.CeilToInt(Mathf.Sign(movement.y));
                        //                                            }
                        //                                            volume[selectedPointIndex].position += movement;
                        //                                            volume[selectedPointIndex].ClearMovement();
                        //                                            _building.MarkModified();
                        //                                            Repaint();
                        //                                            clickedVolume = null;
                        //                                            mode = EditModes.PointManipulation;
                        //                                            //                                            Debug.Log("link point " + clickedPlan);
                        //                                        }
                        //                                    }
                        //                                }
                        //                            }
                        //                        }

                        break;

                    case EditModes.PointAddition:

                        float pointDistance = Mathf.Infinity;
                        int pointIndex = -1;
                        Vector3 linePoint = Vector3.zero;
                        Vector3 usePoint = Vector3.zero;
                        for (int p = 0; p < numberOfPoints; p++)
                        {
                            Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                            Vector3 p1 = rotation * volume.BuildingPoint((p + 1) % numberOfPoints) + position;

                            linePoint = BuildrUtils.ClosestPointOnLine(p0, p1, mousePlanePoint);
                            float thisDist = Vector3.Distance(linePoint, mousePlanePoint);
                            if (thisDist < pointDistance)
                            {
                                pointIndex = (p + 1 + numberOfPoints) % numberOfPoints;
                                pointDistance = thisDist;
                                usePoint = linePoint;
                            }
                        }


                        if (pointIndex != -1 && pointDistance < 5)
                        {
                            float pointHandleSize = HandleUtility.GetHandleSize(usePoint);
                            if (UnityVersionWrapper.HandlesDotButton(usePoint, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f))
                            {
                                //                        Undo.RegisterUndo(plan.GetUndoObjects(), "Split Wall");
	                            Vector3 localPoint = volume.transform.InverseTransformPoint(usePoint);
                                volume.InsertPoint(pointIndex, new Vector2Int(localPoint, true));
                                _selectedPoints.Clear();
                                _selectedPoints.Add(pointIndex);
                                mode = EditModes.PointManipulation;
                                clickedVolume = null;
                                _building.MarkModified();
                            }
                        }
                        Handles.color = Color.white;
                        Repaint();

                        break;

                    case EditModes.PointRemoval:

                        Handles.color = REMOVAL_COLOUR;
                        for (int p = 0; p < numberOfPoints; p++)
                        {
                            Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                            float hSize = HandleUtility.GetHandleSize(p0) * 0.1f;
                            if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                            {
                                BuildingEditor.volume.RemovePointAt(p);
                                _building.MarkModified();
                                return;//let's just leave the GUI - simple, cheap.
                            }
                        }
                        break;

                    case EditModes.SelectCurveWall:

                        int curveIndex = BuildingEditorUtils.OnSelectWall(BuildingEditor.volume);
                        if (curveIndex != -1)
                        {
                            BuildingEditorUtils.CurveWall(BuildingEditor.volume, curveIndex);
                            mode = EditModes.PointManipulation;
                        }

                        break;

                    case EditModes.WallExtrusion:

                        int output = BuildingEditorUtils.OnSelectWall(BuildingEditor.volume);
                        if (output != -1)
                        {
                            BuildingEditorUtils.ExtrudeWallWithNewPoints(BuildingEditor.volume, output);
                            mode = EditModes.PointManipulation;
                        }

                        break;

                    case EditModes.WallExtrusionNewVolume:

                        int selectedPoint = BuildingEditorUtils.OnSelectWall(BuildingEditor.volume);
                        if (selectedPoint != -1) BuildingEditorUtils.ExtrudeWallIntoNewVolume(_building, BuildingEditor.volume, selectedPoint);

                        break;

                    case EditModes.LinkFloorplans:

                        if (mouseOverPlan != null && mouseOverPlan != BuildingEditor.volume)
                        {
                            if (BuildrUtils.GetLinkablePlans(_building, BuildingEditor.volume).Contains(mouseOverPlan))
                            {
                                Vector3 planACenter = BuildrUtils.CalculateFloorplanCenter(BuildingEditor.volume);
                                Vector3 planBCenter = BuildrUtils.CalculateFloorplanCenter(mouseOverPlan);
                                Handles.color = SELECTED_POINT_COLOUR;
                                Handles.DrawLine(planACenter, planBCenter);
                                float planAHsize = HandleUtility.GetHandleSize(planACenter) * 0.2f;
                                float planBHsize = HandleUtility.GetHandleSize(planBCenter) * 0.2f;
                                UnityVersionWrapper.HandlesSphereCap(0, planACenter, Quaternion.identity, planAHsize);
                                UnityVersionWrapper.HandlesSphereCap(0, planBCenter, Quaternion.identity, planBHsize);
                                if (clickedVolume != null)
                                {
                                    Undo.RecordObject(BuildingEditor.volume, "Volume Linkage");
                                    Undo.RecordObject((Volume)clickedVolume, "Volume Linkage");
                                    BuildingEditor.volume.LinkPlans(clickedVolume);
                                    mode = EditModes.PointManipulation;
                                }
                            }
                        }

                        break;

                    case EditModes.UnlinkFloorplans:

                        if (mouseOverPlan != null)
                        {
                            Vector3 planACenter = BuildrUtils.CalculateFloorplanCenter(BuildingEditor.volume);
                            Vector3 planBCenter = BuildrUtils.CalculateFloorplanCenter(mouseOverPlan);
                            Handles.color = REMOVAL_COLOUR;
                            Handles.DrawLine(planACenter, planBCenter);
                            float planAHsize = HandleUtility.GetHandleSize(planACenter) * 0.2f;
                            float planBHsize = HandleUtility.GetHandleSize(planBCenter) * 0.2f;
                            UnityVersionWrapper.HandlesSphereCap(0, planACenter, Quaternion.identity, planAHsize);
                            UnityVersionWrapper.HandlesSphereCap(0, planBCenter, Quaternion.identity, planBHsize);
                        }

                        if (clickedVolume != null)
                        {
                            List<Volume> linkedPlans = BuildingEditor.volume.linkedPlans;
                            if (linkedPlans.Contains(clickedVolume))
                            {
                                Undo.RecordObject(BuildingEditor.volume, "Volume Linkage");
                                Undo.RecordObject((Volume)clickedVolume, "Volume Linkage");
                                BuildingEditor.volume.UnlinkPlans(clickedVolume);
                                mode = EditModes.PointManipulation;
                            }
                        }

                        break;
                }
            }

            //            Debug.Log(Selection.activeObject);
            //            Object[] obs = Selection.activeObject;
            //            foreach (Object ob in obs)
            //            {
            //                Debug.Log(ob+" "+ob.name);
            //            }
            if (clickedVolume != null && clickedVolume != BuildingEditor.volume)
            {
                                BuildingEditor.volume = (Volume)clickedVolume;
                //                _selectedPoints.Clear();
                //                _selectedControlPoint = -1;
                //                Repaint();
                //                current.Use();
            }
        }

        public static void OnInspectorGUI(Building _building)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            if (!_building.generateExteriors)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Exteriors are currently disabled", MessageType.Warning);
                if (GUILayout.Button("Enable Exteriors"))
                {
                    Undo.RecordObject(_building, "enable exterior generation");
                    _building.generateExteriors = true;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical("Box", GUILayout.Width(400), GUILayout.Height(100));

//            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
//            EditorGUILayout.LabelField("Generate Exteriors: ", GUILayout.Width(110));
//            _building.generateExteriors = EditorGUILayout.Toggle(_building.generateExteriors);
//            EditorGUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
//            EditorGUILayout.LabelField("Generate Interiors: ", GUILayout.Width(110));
//            _building.generateInteriors = EditorGUILayout.Toggle(_building.generateInteriors);
//            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Add New Volume");
            newVolumeButtonScroll = EditorGUILayout.BeginScrollView(newVolumeButtonScroll);
            EditorGUILayout.BeginHorizontal();
            int addButtonWidth = 100;
            if (BuildingEditor.volume != null)
            {
                addButtonWidth = 95;
                if (GUILayout.Button("Add\nAbove\nSelected", GUILayout.Width(addButtonWidth)))
                {
                    Volume basePlan = BuildingEditor.volume;
                    Vector3 center = BuildrUtils.CalculateFloorplanCenter(basePlan) + _building.transform.position;
                    BuildingEditor.volume = (Volume)_building.AddPlan(center, 25, basePlan);
                    _selectedPoints.Clear();
                    _selectedControlPoint = -1;
                    mode = EditModes.PointManipulation;
                    Repaint();
                }
            }

            if (GUILayout.Button("Place\nSquare\nVolume", GUILayout.Width(addButtonWidth)))
            {
                mode = EditModes.NewVolumePlace;
                Repaint();
            }

            if (GUILayout.Button("Plot\nRectangle\nVolume", GUILayout.Width(addButtonWidth)))
            {
                newRectStart = Vector2Int.zero;
                mode = EditModes.NewVolumeRect;
                Repaint();
            }

            if (GUILayout.Button("Free\nDraw\nVolume", GUILayout.Width(addButtonWidth)))
            {
                mode = EditModes.NewVolumeDrawLines;
                Repaint();
            }

            if (GUILayout.Button("Build\nVolume\nFrom Floorplan", GUILayout.Width(addButtonWidth)))
            {
                mode = EditModes.SwitchToInterior;
                _building.generateInteriors = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            if (mode != EditModes.PointManipulation)
            {
                if (GUILayout.Button("Cancel"))
                    mode = EditModes.PointManipulation;
            }

            int floorplanCount = _building.numberOfPlans;
            if (floorplanCount > 0)
                EditorGUILayout.LabelField("Volumes");
            if (floorplanCount > 10)
                volumeScroll = EditorGUILayout.BeginScrollView(volumeScroll, false, true, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH), GUILayout.Height(150));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            string[] floorplanNames = new string[floorplanCount];
            for (int p = 0; p < floorplanCount; p++)
                floorplanNames[p] = _building[p].name;
            int currentIndex = _building.IndexOf(BuildingEditor.volume);

            int newIndex = GUILayout.SelectionGrid(currentIndex, floorplanNames, 2);
            if (newIndex != currentIndex)
            {
                BuildingEditor.volume = (Volume)_building[newIndex];
                _selectedPoints.Clear();
                _selectedControlPoint = -1;
            }

            GUILayout.Space(15);

            EditorGUILayout.EndHorizontal();
            if (floorplanCount > 10)
                EditorGUILayout.EndScrollView();

            if (BuildingEditor.volume != null)
            {
                GUILayout.Space(20);
                Undo.RecordObject(BuildingEditor.volume, "Volume Modification");
                EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));


                EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                switch (mode)
                {
                    case EditModes.PointManipulation:
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                        EditorGUILayout.LabelField("Editing Volume: ", GUILayout.Width(110));
                        BuildingEditor.volume.name = EditorGUILayout.DelayedTextField(BuildingEditor.volume.name);
                        EditorGUILayout.EndHorizontal();
                        break;

                    case EditModes.PointAddition:
                        EditorGUILayout.LabelField("Add Points");
                        break;

                    case EditModes.PointRemoval:
                        EditorGUILayout.LabelField("Remove Points");
                        break;

                    case EditModes.WallExtrusion:
                        EditorGUILayout.LabelField("Extruding Wall");
                        break;

                    case EditModes.WallExtrusionNewVolume:
                        EditorGUILayout.LabelField("Extruding Wall, Create New Volume");
                        break;

                    case EditModes.LinkPoints:
                        EditorGUILayout.LabelField("Linking Points");
                        break;

                    case EditModes.DelinkPoints:
                        EditorGUILayout.LabelField("Delinking Points");
                        break;

                    case EditModes.SelectCurveWall:
                        EditorGUILayout.LabelField("Curve Wall");
                        break;
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Points", GUILayout.Width(40));
                EditorGUILayout.Space();
                if (GUILayout.Button("Add Point", GUILayout.Width(75)))
                    mode = EditModes.PointAddition;

                if (GUILayout.Button("Link Points", GUILayout.Width(75)))
                    mode = EditModes.LinkPoints;

                if (GUILayout.Button("Delink Points", GUILayout.Width(90)))
                {
                    _selectedPoints.Clear();
                    mode = EditModes.DelinkPoints;
                }

                if (GUILayout.Button("Remove Point", GUILayout.Width(90)))
                    mode = EditModes.PointRemoval;
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Walls", GUILayout.Width(55));
                EditorGUILayout.Space();
                if (GUILayout.Button("Curve Wall", GUILayout.Width(80)))
                    mode = EditModes.SelectCurveWall;

                if (GUILayout.Button("Extrude", GUILayout.Width(70)))
                    mode = EditModes.WallExtrusion;

                if (GUILayout.Button("Extrude New Volume", GUILayout.Width(150)))
                    mode = EditModes.WallExtrusionNewVolume;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Volume", GUILayout.Width(70));
                EditorGUILayout.Space();

                if (GUILayout.Button("Clone", GUILayout.Width(70)))
                    BuildingEditor.volume = (Volume)_building.ClonePlan(BuildingEditor.volume);

                if (GUILayout.Button("Link", GUILayout.Width(70)))
                    mode = EditModes.LinkFloorplans;

                if (GUILayout.Button("Unlink", GUILayout.Width(70)))
                    mode = EditModes.UnlinkFloorplans;

                if (GUILayout.Button("Delete", GUILayout.Width(70)))
                {
                    if (EditorUtility.DisplayDialog("Delete Volume", string.Format("Delete Volume \"{0}\"?", BuildingEditor.volume.name), "Delete", "Cancel"))
                    {
                        _building.RemovePlan(BuildingEditor.volume);
                        BuildingEditor.volume = null;
                        _selectedPoints.Clear();
                        _selectedControlPoint = -1;
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();

                VolumeInspectorGUI(BuildingEditor.volume);

                EditorGUILayout.EndVertical();
            }


            switch (mode)
            {
                case EditModes.PointManipulation:

                    if (BuildingEditor.volume != null)
                    {

                        int selectedPointCount = _selectedPoints.Count;
                        if (selectedPointCount > 0)
                        {
                            EditorGUILayout.LabelField("Editing points:");
                            for (int sp = 0; sp < selectedPointCount; sp++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(sp + " : ");
                                int selectedPointIndex = _selectedPoints[sp];
                                Vector2Int point = BuildingEditor.volume[selectedPointIndex].position;
                                Vector2Int modPoint = new Vector2Int(EditorGUILayout.Vector2Field("position", point.vector2));
                                if (point != modPoint)
                                {
                                    BuildingEditor.volume[selectedPointIndex].position = modPoint;
                                    _building.MarkModified();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        if (_selectedControlPoint > BuildingEditor.volume.numberOfPoints)
                            _selectedControlPoint = -1;
                        if (_selectedControlPoint != -1)
                        {
                            EditorGUILayout.LabelField("Editing curve point: " + _selectedControlPoint);
                            Vector2Int point = BuildingEditor.volume.GetControlPointA(_selectedControlPoint);
                            Vector2Int modPoint = new Vector2Int(EditorGUILayout.Vector2Field("position", point.vector2));
                            if (point != modPoint)
                            {
                                BuildingEditor.volume.SetControlPointA(_selectedControlPoint, modPoint);
                                _building.MarkModified();
                            }

                            point = BuildingEditor.volume.GetControlPointB(_selectedControlPoint);
                            modPoint = new Vector2Int(EditorGUILayout.Vector2Field("position", point.vector2));
                            if (point != modPoint)
                            {
                                BuildingEditor.volume.SetControlPointB(_selectedControlPoint, modPoint);
                                _building.MarkModified();
                            }

                            if (GUILayout.Button("Straighten Curve"))
                            {
                                BuildingEditor.volume.SetControlPointA(_selectedControlPoint, Vector2Int.zero);
                                BuildingEditor.volume.SetControlPointB(_selectedControlPoint, Vector2Int.zero);
                                _building.MarkModified();
                            }
                        }

                        EditorGUILayout.LabelField(string.Format("base height: {0}", BuildingEditor.volume.baseHeight));
                    }
                    break;

            }

            if (BuildingEditor.volume != null && BuildingEditor.volume.isModified)
                _building.MarkModified();

            if (_building.isModified || GUI.changed)
                Repaint();

            GUILayout.EndVertical();
        }

        public static Volume VolumeSelectorInspectorGUI(Building building)
        {
            int floorplanCount = building.numberOfPlans;
            if (floorplanCount > 0)
                EditorGUILayout.LabelField("Volumes");
            if (floorplanCount > 10)
                volumeScroll = EditorGUILayout.BeginScrollView(volumeScroll, false, true, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH), GUILayout.Height(150));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            string[] floorplanNames = new string[floorplanCount];
            for (int p = 0; p < floorplanCount; p++)
                floorplanNames[p] = building[p].name;
            int currentIndex = building.IndexOf(BuildingEditor.volume);

            int newIndex = GUILayout.SelectionGrid(currentIndex, floorplanNames, 2);

            EditorGUILayout.EndHorizontal();
            if(newIndex != currentIndex)
            {
                Volume newSelectedVolume = (Volume)building[newIndex];
                BuildingEditor.volume = newSelectedVolume;
                return newSelectedVolume;
            }
            return null;
        }

        public static void VolumeInspectorGUI(Volume volume, bool showPoints = true)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField(string.Format("Number of floors : {0}", volume.floors));
            if (GUILayout.Button("-"))
                volume.floors--;
            if (GUILayout.Button("+"))
                volume.floors++;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Floor Height :");
            volume.floorHeight = EditorGUILayout.FloatField(volume.floorHeight);
            EditorGUILayout.LabelField("m", GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            volume.floorHeight = GUILayout.HorizontalSlider(volume.floorHeight, 0.5f, 20.0f, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Facade Wall Unit Minimum Width :");
            volume.minimumWallUnitLength = EditorGUILayout.FloatField(volume.minimumWallUnitLength);
            EditorGUILayout.LabelField("m");
            EditorGUILayout.EndHorizontal();
            volume.minimumWallUnitLength = GUILayout.HorizontalSlider(volume.minimumWallUnitLength, 1.0f, 20.0f, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
			EditorGUILayout.LabelField("Underside Surface");
			volume.undersideSurafce = EditorGUILayout.ObjectField(volume.undersideSurafce, typeof(Surface), false) as Surface;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Plan is external : ");
            volume.external = EditorGUILayout.Toggle(volume.external);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Plan Points");
            if (GUILayout.Button(showVolumePoints ? "-" : "+", GUILayout.Width(25)))
                showVolumePoints = !showVolumePoints;
            EditorGUILayout.EndHorizontal();
            if (showPoints && showVolumePoints)
            {
                int pointCount = volume.numberOfPoints;
                for (int pt = 0; pt < pointCount; pt++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Vector2IntEditor.GUI(volume[pt].position, string.Format("Point {0}", (pt + 1)));
                    EditorGUILayout.LabelField("mm");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("Render Facade {0} - {1}", pt + 1, (pt + 1) % pointCount + 1), GUILayout.Width(140));
                    volume[pt].render = EditorGUILayout.Toggle(volume[pt].render, GUILayout.Width(25));

                    if (GUILayout.Button("Delete Point", GUILayout.Width(120)))
                    {
                        Undo.RecordObject(volume, "Delete Volume Point");
                        volume.RemovePointAt(pt);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawVolumes(Building _building, Volume mouseOverPlan)
        {
            BuildRSettings settings = _building.settings;
            int numberOfFloorplans = _building.numberOfPlans;
            Vector3 position = _building.transform.position;
            Quaternion rotation = _building.transform.rotation;

            Camera sceneCamera = Camera.current;

            Vector3[] centerPoints = new Vector3[numberOfFloorplans];
            for (int f = 0; f < numberOfFloorplans; f++)
                centerPoints[f] = BuildrUtils.CalculateFloorplanCenter(_building[f]);

            //Draw Floorplan outlines
            for (int f = 0; f < numberOfFloorplans; f++)
            {
                Volume volume = (Volume)_building[f];
                if (volume == null)
                {
                    _building.RemovePlanAt(f);
                    continue;
                }
                int numberOfPoints = volume.numberOfPoints;
                Vector2 leftPoint = Vector2.right;
                if (numberOfPoints == 0)
                    continue;
                //                Vector3 labelPoint = volume.BuildingPoint(0) + position;

                int numberOfTopPoints = 0;
                for (int p = 0; p < numberOfPoints; p++)
                {
                    if (volume.IsWallStraight(p))
                        numberOfTopPoints++;
                    else
                        numberOfTopPoints += 9;
                }
                Vector3[] planVs = new Vector3[numberOfTopPoints];
                bool[] planLegalStatus = new bool[numberOfTopPoints];
                int planVIndex = 0;

                Handles.color = settings.subLineColour;
                for (int p = 0; p < numberOfPoints; p++)
                {
                    Vector3 p0 = volume.WorldPoint(p);//rotation * floorplan.BuildingPoint(p) + position);
                    float dotSize = HandleUtility.GetHandleSize(p0) * 0.05f;
                    UnityVersionWrapper.HandlesDotCap(0, p0, Quaternion.identity, dotSize);
                    Vector2 p0ss = sceneCamera.WorldToScreenPoint(p0);
                    if (p0ss.x < leftPoint.x)
                    {
                        leftPoint = p0ss;
                        //                        labelPoint = p0;
                    }
                    //Render handles
                    if (volume.IsWallStraight(p))
                    {
                        planVs[planVIndex] = p0;
                        planLegalStatus[planVIndex] = !volume[p].illegal;
                        planVIndex++;
                    }
                    else
                    {
                        Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);//rotation * floorplan.BuildingPoint((p + 1) % numberOfPoints) + position);
                        Vector3 cw0 = volume.WorldControlPointA(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                        Vector3 cw1 = volume.WorldControlPointB(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                        Vector3[] curveWall = new Vector3[10];
                        for (int t = 0; t < 10; t++)
                        {
                            Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);
                            curveWall[t] = cp;
                            if (t < 9)
                            {
                                planVs[planVIndex] = cp;
                                planLegalStatus[planVIndex] = !volume[p].illegal;
                                planVIndex++;
                            }
                        }
                    }
                }

                if (mode != EditModes.RemovePlan)
                {
                    if (volume == BuildingEditor.volume)
                        Handles.color = SELECTED_BLUEPRINT_COLOUR;
                    else if (mouseOverPlan == volume)
                        Handles.color = HIGHLIGHTED_BLUEPRINT_COLOUR;
                    else
                        Handles.color = UNSELECTED_BLUEPRINT_COLOUR;
                }
                else
                {
                    if (mouseOverPlan == volume)
                        Handles.color = REMOVE_BLUEPRINT_HIGHLIGHT_COLOUR;
                    else
                        Handles.color = REMOVE_BLUEPRINT_COLOUR;

                }

                if(!volume.isLegal) Handles.color = REMOVE_BLUEPRINT_COLOUR;

                volume.PlanLegalityCheck();
                if(volume.isLegal)
                    DrawConcavePolygonHandle.Shape(planVs, Handles.color);
                Vector3 volumeNamePosition = centerPoints[f];
                volumeNamePosition.y += (volume.floorHeight * volume.floors) * 0.5f;
                Handles.Label(volumeNamePosition, volume.name, BuildingEditor.SceneLabel);

                int linkCount = volume.linkedPlans.Count;
                Vector3 thisCenter = centerPoints[f];
                Handles.color = Color.green;
                for (int l = 0; l < linkCount; l++)
                {
                    if (f == l) continue;
                    Vector3 linkCenter = centerPoints[l];
                    Handles.DrawDottedLine(thisCenter, linkCenter, 5);
                }

                int numberOfFloors = volume.floors;
                float planHeight = volume.floorHeight;
                float totalPlanHeight = volume.planHeight;
                Vector3 planHeightV = totalPlanHeight * Vector3.up;
                Vector3 planBaseV = volume.baseHeight * Vector3.up;

                Handles.color = new Color(0, 0.2f, 1, 0.5f);

                Dictionary<int, List<Vector2Int>> anchorPoints = volume.facadeWallAnchors;//GetExternalWallAnchors();
                for (int p = 0; p < numberOfPoints; p++)
                {
                    List<Vector2Int> facadeAnchorPoints = anchorPoints[p];
                    int anchorCount = facadeAnchorPoints.Count;
                    for (int a = 0; a < anchorCount; a++)
                    {
                        Vector3 anchorPointA = facadeAnchorPoints[a].vector3XZ + planBaseV;
                        Vector3 anchorPointB = facadeAnchorPoints[(a + 1) % anchorCount].vector3XZ + planBaseV;
                        //convert to world
                        anchorPointA = rotation * anchorPointA + position;
                        anchorPointB = rotation * anchorPointB + position;

                        Handles.color = settings.subLineColour;
                        Handles.DrawLine(anchorPointA, anchorPointA + planHeightV);
                        for (int fl = 0; fl < numberOfFloors + 1; fl++)
                        {
                            Handles.color = fl == 0 || fl == numberOfFloors ? MAIN_LINE_COLOUR : SUB_LINE_COLOUR;
                            if(volume[p].illegal) Handles.color = Color.red;
                            float lineHeight = fl * planHeight;
                            Vector3 lineHeightV = lineHeight * Vector3.up;
                            Handles.DrawLine(anchorPointA + lineHeightV, anchorPointB + lineHeightV);
                        }
                        float anchorSize = HandleUtility.GetHandleSize(anchorPointA);
                        Handles.color = new Color(1, 1, 0, 0.25f);
                        UnityVersionWrapper.HandlesDotCap(0, anchorPointA, Quaternion.identity, anchorSize * 0.05f);
                    }
                }
            }
        }
    }
}