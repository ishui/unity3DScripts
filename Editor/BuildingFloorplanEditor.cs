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
using JaspLib;
using UnityEngine;
using UnityEditor;

namespace BuildR2
{
    /// <summary>
    /// 
    /// 
    //TODO
    /// Snap mouse to support perpendicularisation
    /// </summary>
    public class BuildingFloorplanEditor
    {
        public static BuildRSettings SETTINGS = null;
        private static Vector3 BASE_POSITION;

        public static BuildingEditorUtils.FloorplanClick mouseOverdata;
        public static BuildingEditorUtils.FloorplanClick clickdata;
        private static HandleShapeDrawUtil handleDraw = new HandleShapeDrawUtil();
        private static List<Vector2Int> _selectedPoints = new List<Vector2Int>();
        public static bool repaint = false;
        public static EditModes mode = EditModes.FloorplanSelection;

        private static Vector2 dragRoomStart = Vector2.zero;
        private static Vector2 dragRoomEnd = Vector2.zero;
        private static bool dragRoom = false;
        private static List<Vector2Int> newRoomPoints = new List<Vector2Int>();
        private static bool clickRoom = false;
        //        private static Vector2 planExtScroll = Vector2.zero;
        private static Vector2 planIntScroll = Vector2.zero;
        private static bool portalIsDoor = true;
        private static RoomPortal tempPortal = null;

        private static GUIStyle ROOM_STYLE;

        //        private static GUIStyle CENTERED_TEXT = null;

        public static bool editing = false;
        //		private static bool leftMouse = false;
        private static bool rightMouse = false;
        //        private static bool middleMouse = false;
        private static bool mouseOverSceneGUI = false;

        private static bool _captureButton = true;

        private static List<int> controlList = new List<int>();

        public enum EditModes
        {
            BuildFloorplanInterior,
            FloorplanSelection,
            DeleteRoom,
            AddPoint,
            AddPortal,
            AddVertical
        }

        public static void Repaint()
        {
            GUI.changed = true;
            repaint = true;

            if (BuildingEditor.floorplan != null)
            {
                BuildingEditor.floorplan.MarkModified();
                EditorUtility.SetDirty(BuildingEditor.floorplan);
            }

            if (BuildingEditor.building != null)
            {
                EditorUtility.SetDirty(BuildingEditor.building);
                BuildingEditor.building.MarkModified();
            }

            SceneMeshHandler.BuildFloorplan();
        }

        private static IFloorplan SelectedInteriorPlan()
        {
            if (BuildingEditor.volume != null)
                return BuildingEditor.volume.InteriorFloorplans()[0];
            else
                return null;
        }

        public static void ToggleEdit(bool value)
        {
            editing = value;
            mode = EditModes.FloorplanSelection;
            if (!BuildingEditor.BUILDING.generateInteriors)
            {
                Undo.RecordObject(BuildingEditor.BUILDING, "enable interior generation");
                BuildingEditor.BUILDING.generateInteriors = true;
            }
            Repaint();
        }

        public static void OnSceneGUI(Building _building)
        {
            if (SETTINGS == null)
                SETTINGS = BuildRSettings.GetSettings();

            clickdata = null;
            Vector3 position = _building.transform.position;
            Quaternion rotation = _building.transform.rotation;
            handleDraw.Clear();
            controlList.Clear();

            bool shiftIsDown = Event.current.shift;

            Camera sceneCamera = Camera.current;
            Event current = Event.current;
            //            Vector2 mousePos = current.mousePosition;
            //            mousePos.y = sceneCamera.pixelHeight - mousePos.y;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);//sceneCamera.ScreenPointToRay(mousePos);
            mouseOverdata = BuildingEditorUtils.OnInteriorSelectionClick(ray);

            switch (current.type)
            {
                case EventType.MouseDown:
                    if (current.button == 0)
                    {
                        //						leftMouse = true;
                        if (mode == EditModes.FloorplanSelection)
                            clickdata = mouseOverdata;
                    }
                    if (current.button == 1)
                    {
                        rightMouse = true;
                        //                        HandleUtility.Repaint();
                    }
                    if (current.button == 2)
                    {
                        //                        middleMouse = true;
                        //                        HandleUtility.Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (current.button == 0)
                    {
                        //						leftMouse = false;
                        //                                                if (mode == EditModes.FloorplanSelection)
                        //                                                    clickdata = mouseOverdata;
                    }
                    if (current.button == 1)
                    {
                        rightMouse = false;
                        //                        HandleUtility.Repaint();
                    }
                    if (current.button == 2)
                    {
                        //                        middleMouse = false;
                        //                        HandleUtility.Repaint();
                    }
                    break;
            }

            if (!editing && current.type != EventType.Layout && current.type != EventType.Repaint)
            {
                if (current.control && current.type == EventType.KeyUp && current.keyCode == KeyCode.E)
                {
                    editing = true;
                    Repaint();
                }
            }

            Vector3 basePosition = position;
            if (BuildingEditor.volume != null)
            {
                basePosition = position + BuildingEditor.volume.baseHeight * Vector3.up;
            }
            if (BuildingEditor.floorplan != null)
            {
                int floor = BuildingEditor.volume.Floor(BuildingEditor.floorplan);
                basePosition = position + Vector3.up * BuildingEditor.volume.CalculateHeight(floor);
                Undo.RecordObject(BuildingEditor.floorplan, "Floorplan Modification");
            }
            float baseUp = basePosition.y;
            Vector3 baseUpV = Vector3.up * baseUp;
            Plane buildingPlane = new Plane(Vector3.up, basePosition);

            Vector3 mousePlanePoint = Vector3.zero;
            float mouseRayDistance;
            if (buildingPlane.Raycast(ray, out mouseRayDistance))
            {
                mousePlanePoint = ray.GetPoint(mouseRayDistance);
                mousePlanePoint.y = baseUp;
            }
            mousePlanePoint = CheckMouseSnap(mousePlanePoint);

            float mouseHandleSize = HandleUtility.GetHandleSize(mousePlanePoint) * 0.2f;
            //            if (editing && !rightMouse && !mouseOverSceneGUI)//lock mouse with hidden button to stop clicking out
            //            {
            //                UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, 1);
            //            }

            if (Event.current.type == EventType.Repaint) _captureButton = true;//reset on repaint

            if (_captureButton && !rightMouse && !mouseOverSceneGUI)
                UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, 1);

            SceneMeshHandler.DrawMesh(sceneCamera);

            //            useMode = mode;//modify this now
            if (!rightMouse)
                EscDel();

            //            if (editing && !rightMouse)
            Tools.current = Tool.None;

            if (!editing)
                mode = EditModes.FloorplanSelection;

            switch (mode)
            {
                case EditModes.FloorplanSelection:

                    dragRoom = false;
                    clickRoom = false;
                    newRoomPoints.Clear();

                    if (BuildingEditor.volume != null)
                    {
                        VolumeFloorSelector(sceneCamera);

                        if (mouseOverdata != null && _selectedPoints.Count == 0)
                        {
                            DrawMouseOverOutline(mouseOverdata);
                        }
                    }

                    //                    VolumeFloorSelector(sceneCamera.transform);

                    if (BuildingEditor.floorplan != null && BuildingEditor.volume != null)
                    {
                        Undo.RecordObject(BuildingEditor.volume, "Floorplan Modification");
                        Room[] rooms = BuildingEditor.floorplan.rooms.ToArray();
                        int roomCount = rooms.Length;
                        for (int rm = 0; rm < roomCount; rm++)
                        {
                            Room room = rooms[rm];
                            int roomPointCount = room.numberOfPoints;
                            for (int rp = 0; rp < roomPointCount; rp++)
                            {
                                Vector3 p0 = position + rotation * room[rp].position.vector3XZ + baseUpV;
                                if (!_selectedPoints.Contains(room[rp].position))
                                {
                                    float hSize = HandleUtility.GetHandleSize(p0) * 0.2f;
                                    Handles.color = new Color(1, 1, 1, 0.5f);
                                    Rect pointRect = HandleUtility.WorldPointToSizedRect(p0, new GUIContent(), new GUIStyle());
                                    pointRect.width = hSize;
                                    pointRect.height = hSize;
                                    if (UnityVersionWrapper.HandlesDotButton(p0, Quaternion.identity, hSize, hSize))
                                    {
                                        if (!shiftIsDown) _selectedPoints.Clear();//do not multi select
                                        _selectedPoints.Add(room[rp].position);
                                        clickdata = null;
                                        Repaint();
                                    }
                                }
                                else
                                {

                                }
                            }
                        }

                        Vector3 sliderPos = baseUpV;
                        int numberOfSelectedPoints = _selectedPoints.Count;
                        for (int sp = 0; sp < numberOfSelectedPoints; sp++)
                            sliderPos += position + rotation * _selectedPoints[sp].vector3XZ;


                        //selected point scene gui
                        if (numberOfSelectedPoints > 0)
                        {
                            BuildingEditor.room = null;
                            BuildingEditor.roomPortal = null;
                            BuildingEditor.opening = null;
                            clickdata = null;
                            sliderPos /= numberOfSelectedPoints;
                            float hSize = HandleUtility.GetHandleSize(sliderPos);
                            Vector3 dirX = (sliderPos.x < 0) ? Vector3.right : Vector3.left;
                            Vector3 dirZ = (sliderPos.z < 0) ? Vector3.forward : Vector3.back;
                            Handles.color = Color.green;
                            var newSliderPos = UnityVersionWrapper.HandlesFreeMoveHandle(sliderPos, Quaternion.identity, hSize * 0.1f, new Vector3(0.1f, 0, 0.1f));
                            if ((newSliderPos - sliderPos).sqrMagnitude > 0)
                                newSliderPos = mousePlanePoint;
                            Handles.color = Color.red;
                            newSliderPos = UnityVersionWrapper.HandlesSlider(newSliderPos, dirX, hSize * 0.666f, 0.0f);
                            Handles.color = Color.blue;
                            newSliderPos = UnityVersionWrapper.HandlesSlider(newSliderPos, dirZ, hSize * 0.666f, 0.0f);

                            Handles.BeginGUI();
                            GUILayout.BeginArea(new Rect(HandleUtility.WorldToGUIPoint(sliderPos) + new Vector2(20, -20), new Vector2(20, 20)));
                            if (GUILayout.Button("x"))
                                _selectedPoints.Clear();
                            if (IsMouseOver()) _captureButton = false;
                            GUILayout.EndArea();
                            Handles.EndGUI();
                            
                            mousePlanePoint = CheckMouseSnap(mousePlanePoint);

                            Vector3 sliderDiff = Quaternion.Inverse(rotation) * (newSliderPos - sliderPos);
                            if (sliderDiff.sqrMagnitude > 0)
                            {
                                clickdata = null;
                                Vector2Int sliderDiffV2 = new Vector2Int(sliderDiff.x, sliderDiff.z);
                                BuildingEditor.floorplan.MarkModified();
                                BuildingEditor.volume.MarkModified();
                                for (int sp = 0; sp < numberOfSelectedPoints; sp++)
                                {
                                    for (int rm = 0; rm < roomCount; rm++)
                                        rooms[rm].MovePoint(_selectedPoints[sp], sliderDiffV2);
                                    _selectedPoints[sp] += sliderDiffV2;
                                }
                                _building.MarkModified();
                            }
                        }
                    }
                    break;

                #region Draw Room
                case EditModes.BuildFloorplanInterior:

                    if (BuildingEditor.volume == null)
                    {
                        mode = EditModes.FloorplanSelection;
                        return;
                    }

                    Undo.RecordObject(BuildingEditor.volume, "Room Creation");
                    Handles.color = SETTINGS.selectedPointColour;
                    UnityVersionWrapper.HandlesDotCap(0, mousePlanePoint, Quaternion.identity, mouseHandleSize * 0.1f);

                    switch (current.type)
                    {
                        case EventType.MouseDown:
                            if (current.button == 0)
                            {
                                if (!clickRoom)
                                {
                                    dragRoomStart = new Vector2(mousePlanePoint.x, mousePlanePoint.z);
                                    dragRoom = true;
                                }
                            }
                            break;
                        case EventType.MouseUp:
                            if (current.button == 0)
                            {
                                if (dragRoom)
                                {
                                    clickRoom = false;
                                    dragRoomEnd = new Vector2(mousePlanePoint.x, mousePlanePoint.z);
                                    float dragRoomMag = Vector2.Distance(dragRoomStart, dragRoomEnd);
                                    dragRoom = false;

                                    if (dragRoomMag > SETTINGS.minimumRoomWidth)
                                    {
                                        FlatBounds newRoomBounds = new FlatBounds();
                                        newRoomBounds.Encapsulate(new[] { dragRoomStart, dragRoomEnd });
                                        if (newRoomBounds.width > SETTINGS.minimumRoomWidth && newRoomBounds.height > SETTINGS.minimumRoomWidth)
                                        {
                                            if (!BuildingEditor.volume)
                                            {
                                                Undo.RecordObject(BuildingEditor.volume, "Room Creation");
                                                IVolume newVolume = _building.AddPlan();
                                                BuildingEditor.volume = (Volume)newVolume;
                                            }
                                            if (BuildingEditor.floorplan == null)
                                                BuildingEditor.floorplan = (Floorplan)SelectedInteriorPlan();

                                            Undo.RecordObject(BuildingEditor.floorplan, "Room Creation");
                                            BuildingEditor.floorplan.rooms.Add(new Room(newRoomBounds, -position));
                                            BuildingEditor.floorplan.MarkModified();
                                            BuildingEditor.building.MarkModified();
                                        }
                                        else clickRoom = true;
                                    }
                                    else clickRoom = true;
                                }

                                if (clickRoom)
                                {
                                    Vector2Int newPoint = new Vector2Int(mousePlanePoint, true);
                                    int newPointCount = newRoomPoints.Count;
                                    bool legal = true;
                                    if (newPointCount > 2)
                                    {
                                        bool shapeSealed = newRoomPoints[0] == newPoint;
                                        Vector2 lastPoint = newRoomPoints[newRoomPoints.Count - 1].vector2;
                                        int startInt = shapeSealed ? 1 : 0;// don't consider the first line if we're closing the shape
                                        int endInt = newPointCount - 2;//don't consider the final line - it will always intersect with the new line
                                        for (int i = startInt; i < endInt; i++)
                                        {
                                            Vector2 x0 = newRoomPoints[i].vector2;
                                            Vector2 x1 = newRoomPoints[i + 1].vector2;
                                            if (JMath.FastLineIntersection(newPoint.vector2, lastPoint, x0, x1))
                                            {
                                                legal = false;
                                                break;
                                            }
                                        }
                                        if (shapeSealed && legal)
                                        {
                                            BuildingEditor.floorplan.rooms.Add(new Room(newRoomPoints, -position));
                                            BuildingEditor.floorplan.MarkModified();
                                            BuildingEditor.building.MarkModified();
                                            clickRoom = false;
                                            newRoomPoints.Clear();
                                        }
                                        if (!shapeSealed && legal)
                                            newRoomPoints.Add(newPoint);
                                    }
                                    else
                                        newRoomPoints.Add(newPoint);

                                }
                            }
                            break;

                        case EventType.KeyDown:

                            switch (current.keyCode)
                            {

                                case KeyCode.Backspace:
                                    int newRoomPointsCount = newRoomPoints.Count;
                                    if (newRoomPointsCount > 0)
                                        newRoomPoints.RemoveAt(newRoomPointsCount - 1);
                                    break;
                            }

                            break;
                    }

                    if (dragRoom)
                    {
                        Vector3 r0 = new Vector3(dragRoomStart.x, mousePlanePoint.y, dragRoomStart.y);
                        Vector3 r3 = mousePlanePoint;
                        Vector3 r1 = new Vector3(r3.x, mousePlanePoint.y, r0.z);
                        Vector3 r2 = new Vector3(r0.x, mousePlanePoint.y, r3.z);
                        Handles.color = SETTINGS.selectedBlueprintColour;
                        Handles.DrawAAConvexPolygon(r0, r1, r3, r2);
                        Handles.color = Color.red;
                        Handles.DrawLine(r0 + Vector3.up, r3 + Vector3.up);
                        SceneView.RepaintAll();
                    }

                    if (clickRoom)
                    {
                        int newPointCount = newRoomPoints.Count;
                        if (newPointCount > 0)
                        {
                            Vector2Int mouseInt = new Vector2Int(mousePlanePoint, true);
                            for (int r = 0; r < newPointCount; r++)
                            {
                                Handles.color = SETTINGS.mainLineColour;

                                Vector2Int pi0 = newRoomPoints[r];
                                Vector2Int pi1 = r < newPointCount - 1 ? newRoomPoints[r + 1] : mouseInt;

                                Vector2Int[] newWall = FloorplanUtil.CalculateNewWall(BuildingEditor.volume, pi0, pi1).offsetPointsInt;

                                Vector3 p0 = pi0.vector3XZ + baseUpV;
                                Vector3 p1 = r < newPointCount - 1 ? newRoomPoints[r + 1].vector3XZ + baseUpV : mousePlanePoint;
                                if (SETTINGS.highlightPerpendicularity)
                                {
                                    Vector3 diff = p1 - p0;
                                    if (diff.x * diff.x < 0.001f || diff.z * diff.z < 0.001f)
                                        Handles.color = SETTINGS.highlightPerpendicularityColour;
                                    else
                                        Handles.color = SETTINGS.highlightAngleColour;
                                }

                                //have to redo the intersection thing because it existed in the event loop
                                if (r == newPointCount - 1)
                                {
                                    bool shapeSealed = newRoomPoints[0] == mouseInt;
                                    Vector2 lastPoint = newRoomPoints[newRoomPoints.Count - 1].vector2;
                                    int startInt = shapeSealed ? 1 : 0;// don't consider the first line if we're closing the shape
                                    int endInt = newPointCount - 2;//don't consider the final line - it will always intersect with the new line
                                    for (int i = startInt; i < endInt; i++)
                                    {
                                        Vector2 x0 = newRoomPoints[i].vector2;
                                        Vector2 x1 = newRoomPoints[i + 1].vector2;
                                        if (JMath.FastLineIntersection(mouseInt.vector2, lastPoint, x0, x1))
                                        {
                                            Handles.color = SETTINGS.errorColour;
                                            break;
                                        }
                                    }
                                }

                                int newWallCount = newWall.Length - 1;
                                Vector3 wallUpV = Vector3.up * BuildingEditor.volume.floorHeight;
                                if (newWallCount > 0)
                                {
                                    for (int nw = 0; nw < newWallCount; nw++)
                                    {
                                        Vector3 wv0 = newWall[nw].vector3XZ;
                                        Vector3 wv1 = newWall[nw + 1].vector3XZ;
                                        Handles.DrawLine(wv0 + baseUpV, wv1 + baseUpV);
                                        Handles.DrawLine(wv0 + baseUpV, wv0 + baseUpV + wallUpV);
                                        Handles.DrawLine(wv1 + baseUpV, wv1 + baseUpV + wallUpV);
                                        Handles.DrawLine(wv0 + baseUpV + wallUpV, wv1 + baseUpV + wallUpV);
                                    }
                                }
                                else
                                {
                                    Handles.DrawLine(p0, p1);
                                    Handles.DrawLine(p0, p0 + wallUpV);
                                    Handles.DrawLine(p1, p1 + wallUpV);
                                    Handles.DrawLine(p0 + wallUpV, p1 + wallUpV);
                                }

                                float endHandleSize = HandleUtility.GetHandleSize(p0) * 0.4f;
                                if (r == 0)
                                {
                                    if (mouseInt == newRoomPoints[0]) Handles.color = SETTINGS.selectedPointColour;
                                    else Handles.color = SETTINGS.subLineColour;
                                }
                                else
                                {
                                    endHandleSize *= 0.5f;
                                    Handles.color = SETTINGS.subLineColour;
                                }
                                UnityVersionWrapper.HandlesDotCap(0, p0, Quaternion.identity, endHandleSize);
                            }
                        }
                    }

                    bool allowNewPoint = true;

                    if (!rightMouse && allowNewPoint)
                    {
                        if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, mouseHandleSize, mouseHandleSize))
                        {
                            //stop clicks from deselect
                        }
                    }

                    break;
                #endregion
                case EditModes.AddPoint:
                    if (BuildingEditor.floorplan != null && !rightMouse)
                    {
                        float pointDistance = Mathf.Infinity;
                        //                        int pointIndex = -1;
                        Vector2 usePoint = Vector2.zero;
                        Room chosenRoom = null;
                        int chosenPoint = -1;
                        Vector2 mousePlanePointV2 = new Vector2(mousePlanePoint.x, mousePlanePoint.z);

                        int roomCount = BuildingEditor.floorplan.rooms.Count;
                        for (int rm = 0; rm < roomCount; rm++)
                        {
                            Room room = BuildingEditor.floorplan.rooms[rm];
                            int pointCount = room.numberOfPoints;
                            for (int p = 0; p < pointCount; p++)
                            {
                                Vector2 p0 = rotation * room[p].position.vector2;
                                int indexB = (p + 1) % pointCount;
                                Vector2 p1 = rotation * room[indexB].position.vector2;


                                Vector2 linePoint = BuildrUtils.ClosestPointOnLine(p0, p1, mousePlanePointV2);
                                float thisDist = Vector2.Distance(linePoint, mousePlanePointV2);
                                if (thisDist < pointDistance)
                                {
                                    chosenPoint = indexB;
                                    pointDistance = thisDist;
                                    usePoint = linePoint;
                                    chosenRoom = room;
                                }
                            }
                        }

                        if (chosenPoint != -1 && pointDistance < 10)
                        {
                            Vector3 pointPos = new Vector3(usePoint.x, BuildingEditor.volume.CalculateHeight(BuildingEditor.floorplan), usePoint.y);
                            float pointHandleSize = HandleUtility.GetHandleSize(pointPos);
                            if (UnityVersionWrapper.HandlesDotButton(pointPos, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f))
                            {
                                Undo.RecordObject(BuildingEditor.volume, "Add Room Point");
                                Vector2Int newPoint = new Vector2Int(usePoint);
                                int lastIndex = (chosenPoint + chosenRoom.numberOfPoints - 1) % chosenRoom.numberOfPoints;
                                Vector2Int opA = chosenRoom[lastIndex].position;
                                Vector2Int opB = chosenRoom[chosenPoint].position;
                                chosenRoom.InsertPoint(chosenPoint, newPoint);


                                for (int rm = 0; rm < roomCount; rm++)
                                {
                                    Room room = BuildingEditor.floorplan.rooms[rm];
                                    if (room == chosenRoom) continue;
                                    int pointCount = room.numberOfPoints;
                                    for (int p = 0; p < pointCount; p++)
                                    {
                                        Vector2Int p0 = room[p].position;
                                        int indexB = (p + 1) % pointCount;
                                        Vector2Int p1 = room[indexB].position;

                                        if (p0 == opA || p0 == opB)
                                            if (p1 == opA || p1 == opB)
                                                room.InsertPoint(indexB, newPoint);
                                    }
                                }

                                _selectedPoints.Clear();
                                _selectedPoints.Add(newPoint);
                                mode = EditModes.FloorplanSelection;
                                clickdata = null;

                                BuildingEditor.floorplan.MarkModified();
                                _building.MarkModified();
                            }
                        }
                        Handles.color = Color.white;
                        Repaint();
                    }

                    break;

                case EditModes.AddPortal:

                    if (BuildingEditor.floorplan != null && !rightMouse)
                    {
                        float pointDistance = Mathf.Infinity;
                        Room chosenRoom = null;
                        int chosenPoint = -1;
                        float lateral = 0;
                        Vector2 mousePlanePointV2 = new Vector2(mousePlanePoint.x, mousePlanePoint.z);

                        int roomCount = BuildingEditor.floorplan.rooms.Count;
                        for (int rm = 0; rm < roomCount; rm++)
                        {
                            Room room = BuildingEditor.floorplan.rooms[rm];

//                            bool[] isExternals = FloorplanUtil.CalculateExternalWall(room, BuildingEditor.volume);
                            int pointCount = room.numberOfPoints;
                            //                            int pointCount = roomWalls.Length;
                            for (int p = 0; p < pointCount; p++)
                            {
                                //								if(isExternals[p])
                                //								{
                                //									Handles.color = Color.red;
                                //									Handles.DrawAAPolyLine();
                                //									continue;
                                //								}

                                Vector3 p0 = rotation * room[p].position.vector3XZ;
                                Vector3 p1 = rotation * room[(p + 1) % pointCount].position.vector3XZ;


                                if (room[p].wall.isExternal)
                                {
                                    Handles.color = Color.red;
                                    Handles.DrawAAPolyLine(p0, p1);
                                    continue;
                                }

//                                Debug.DrawLine(p0, p1, Color.red, 30);
                                Vector2 p02 = new Vector2(p0.x, p0.z);
                                Vector2 p12 = new Vector2(p1.x, p1.z);

                                //                                Debug.DrawLine(JMath.ToV3());
                                Vector2 linePoint = BuildrUtils.ClosestPointOnLine(p02, p12, mousePlanePointV2);
                                float thisDist = Vector2.Distance(linePoint, mousePlanePointV2);
                                float lineDistance = Vector2.Distance(p02, linePoint);
                                float wallDistance = Vector2.Distance(p02, p12);
                                if (thisDist < pointDistance)
                                {
                                    chosenPoint = p;//% pointCount;
                                    pointDistance = thisDist;
                                    lateral = lineDistance / wallDistance;
                                    //                                    usePoint = Vector3.Lerp(p0, p1, lineDistance / wallDistance);//linePoint);
                                    chosenRoom = room;
                                    //                                    wallDirection = new Vector3(p1.x - p0.x, 0, p1.y - p0.y).normalized;
                                }
                            }
                        }

                        if (chosenPoint != -1 && pointDistance < 10)
                        {
                            float baseHeight = BuildingEditor.volume.CalculateHeight(BuildingEditor.floorplan);
                            //                            Vector3 pointPos = usePoint;//new Vector3(usePoint.x, baseHeight, usePoint.y);
                            Vector3 p0 = rotation * chosenRoom[chosenPoint].position.vector3XZ;
                            Vector3 p1 = rotation * chosenRoom[(chosenPoint + 1) % chosenRoom.numberOfPoints].position.vector3XZ;

                            if (tempPortal == null)
                            {
                                tempPortal = new RoomPortal(chosenPoint);
                                tempPortal.height = portalIsDoor ? SETTINGS.defaultDoorHeight : SETTINGS.defaultWindowHeight;
                                tempPortal.verticalPosition = portalIsDoor ? 0 : 0.85f;
                                tempPortal.width = portalIsDoor ? SETTINGS.defaultDoorWidth : SETTINGS.defaultWindowWidth;
                            }

                            tempPortal.wallIndex = chosenPoint;
                            tempPortal.lateralPosition = lateral;//Vector3.Distance(p0, pointPos) / Vector3.Distance(p0, p1);

                            Color newColourFill = SETTINGS.newElementColour;
                            newColourFill.a *= 0.5f;

                            Handles.DrawAAPolyLine(mousePlanePoint + Vector3.left, mousePlanePoint + Vector3.right);
                            Handles.DrawAAPolyLine(mousePlanePoint + Vector3.forward, mousePlanePoint + Vector3.back);

                            Vector3 pointPos = Vector3.Lerp(p0, p1, lateral);
                            pointPos.y = baseHeight;
                            float pointHandleSize = HandleUtility.GetHandleSize(pointPos);
                            float mousePortalDistance = Vector3.Distance(mousePlanePoint, pointPos);
                            float threshold = 0.85f;

                            if (mousePortalDistance < threshold)
                            {
                                DrawPortal(rotation, baseHeight, BuildingEditor.volume.floorHeight, chosenRoom, tempPortal, SETTINGS.newElementColour, newColourFill);
                                DrawPortalBaseHandle(rotation, baseHeight, BuildingEditor.volume.floorHeight, chosenRoom, tempPortal, SETTINGS.newElementColour, newColourFill);
                            }

                            if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, pointHandleSize))
                            {
                                if (mousePortalDistance < threshold)
                                {
                                    Undo.RecordObject(BuildingEditor.volume, "Add Portal");
                                    //                                RoomPortal newPortal = new RoomPortal(chosenPoint);
                                    tempPortal.height = portalIsDoor ? SETTINGS.defaultDoorHeight : SETTINGS.defaultWindowHeight;
                                    //                                newPortal.lateralPosition = Vector3.Distance(p0, pointPos) / Vector3.Distance(p0, p1);
                                    tempPortal.verticalPosition = portalIsDoor ? 0 : 0.85f;
                                    tempPortal.width = portalIsDoor ? SETTINGS.defaultDoorWidth : SETTINGS.defaultWindowWidth;
                                    chosenRoom.AddPortal(tempPortal);
                                    BuildingEditor.floorplan.MarkModified();
                                    BuildingEditor.roomPortal = tempPortal;
                                    tempPortal = null;
                                    clickdata = null;
                                    _building.MarkModified();
                                }
                            }
                        }
                        Handles.color = Color.white;
                        Repaint();
                    }

                    break;

                case EditModes.AddVertical:
                    if (!rightMouse && BuildingEditor.volume != null)
                    {
                        float spaceSize = 2.0f;
                        Vector3 p0 = mousePlanePoint + new Vector3(-1, 0, -1) * spaceSize * 0.5f;
                        Vector3 p1 = mousePlanePoint + new Vector3(1, 0, -1) * spaceSize * 0.5f;
                        Vector3 p2 = mousePlanePoint + new Vector3(1, 0, 1) * spaceSize * 0.5f;
                        Vector3 p3 = mousePlanePoint + new Vector3(-1, 0, 1) * spaceSize * 0.5f;
                        Handles.color = SETTINGS.newElementColour;
                        Handles.DrawAAConvexPolygon(p0, p1, p2, p3);
                        Handles.color = Color.white;
                        Handles.DrawAAPolyLine(p0, p1, p2, p3, p0);

                        Handles.color = SETTINGS.mainLineColour;
                        UnityVersionWrapper.HandlesDotCap(0, mousePlanePoint, Quaternion.identity, mouseHandleSize * 0.1f);

                        if (UnityVersionWrapper.HandlesDotButton(mousePlanePoint, Quaternion.identity, 0, mouseHandleSize))
                        {
                            Undo.RecordObject(BuildingEditor.volume, "New Vertical Opening");
                            BuildingEditor.opening = BuildingEditor.building.AddOpening();
                            BuildingEditor.opening.position = new Vector2Int(mousePlanePoint.x, mousePlanePoint.z) - position;
                            BuildingEditor.opening.size = new Vector2Int(spaceSize, spaceSize);
                            BuildingEditor.opening.baseFloor = BuildingEditor.volume.Floor(BuildingEditor.floorplan);
                            clickdata = null;
                            //                            mode = EditModes.EditVertical;
                        }
                    }
                    break;
            }

            DrawRoomIssues();

            //			if(GUI.changed) clickdata = null;
            //	        int hotControl = EditorGUIUtility.hotControl;
            //			Handles.
            //	        if(leftMouse && controlList.Contains(hotControl))
            //	        {
            //		        clickdata = null;
            //		        Debug.Log("STOP "+ hotControl);
            //	        }

            if (clickdata != null)
            {
                if (clickdata.volume != null)
                {
                    BuildingEditor.volume = clickdata.volume;

                    if (clickdata.floorplan != null)
                    {
                        BuildingEditor.floorplan = clickdata.floorplan;

                        if (clickdata.room != null)
                        {
                            BuildingEditor.room = clickdata.room;
                            //                            Debug.Log("click room "+(Event.current.type == EventType.Layout));
                            if (clickdata.portal != null)
                            {
                                BuildingEditor.roomPortal = clickdata.portal;
                            }
                            else
                            {
                                BuildingEditor.roomPortal = null;
                            }
                        }
                    }

                    if (clickdata.opening != null)
                    {
                        BuildingEditor.room = null;
                        BuildingEditor.opening = clickdata.opening;
                    }

                    //                                        if(current.type == EventType.mouseDown) current.Use();
                    //                                        if(current.type == EventType.mouseUp) current.Use();
                }
            }



            handleDraw.Draw();
        }

        public static void OnInspectorGUI(Building _building)
        {
            if (SETTINGS == null)
                SETTINGS = BuildRSettings.GetSettings();

            if (_building == null) return;

            Undo.RecordObject(_building, "Building Modified");

            if (!_building.generateInteriors)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.HelpBox("Interiors are currently disabled", MessageType.Warning);
                if (GUILayout.Button("Enable Interiors"))
                    BuildingEditor.EnableInteriorGeneration();
                EditorGUILayout.EndVertical();
            }

            if (BuildingEditor.floorplan != null)
                Undo.RecordObject(BuildingEditor.floorplan, "Floorplan Modified");

            BuildingVolumeEditor.VolumeSelectorInspectorGUI(_building);
            Volume selectedVolume = BuildingEditor.volume;

            if (selectedVolume == null && _building.numberOfVolumes > 0)
            {
                selectedVolume = (Volume)_building[0];
                BuildingEditor.volume = selectedVolume;
            }

            if (selectedVolume != null)
            {

                BuildingVolumeEditor.VolumeInspectorGUI(BuildingEditor.volume, false);

                EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Wall Thickness: ", GUILayout.Width(110));
                BuildingEditor.volume.wallThickness = EditorGUILayout.Slider(BuildingEditor.volume.wallThickness, 0.1f, 1);
                EditorGUILayout.EndHorizontal();

                IFloorplan[] floorplans = BuildingEditor.volume.InteriorFloorplans();
                IFloorplan selectedFloorplan = BuildingEditor.floorplan;
                if (selectedFloorplan == null && floorplans.Length > 0)
                {
                    selectedFloorplan = floorplans[0];
                    BuildingEditor.floorplan = (Floorplan)selectedFloorplan;
                }
                int intFloorplanCount = floorplans.Length;
                if (intFloorplanCount > 0)
                    EditorGUILayout.LabelField("Interior Floors");
                if (intFloorplanCount > 10)
                    planIntScroll = EditorGUILayout.BeginScrollView(planIntScroll, false, true, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH), GUILayout.Height(150));
                EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                string[] interiorFloorplanNames = new string[intFloorplanCount];
                for (int p = 0; p < intFloorplanCount; p++)
                {
                    int rIndex = intFloorplanCount - 1 - p;
                    interiorFloorplanNames[p] = string.Format("Floor {0}{1}", rIndex + 1, floorplans[rIndex].name == "" ? "" : string.Format(": {0}", floorplans[rIndex].name));
                }
                int currentInteriorIndex = intFloorplanCount - Array.IndexOf(floorplans, BuildingEditor.floorplan) - 1;
                if (currentInteriorIndex == -1)
                    currentInteriorIndex = intFloorplanCount - 1;
                int newFloorIndex = GUILayout.SelectionGrid(currentInteriorIndex, interiorFloorplanNames, 1);
                if (newFloorIndex != currentInteriorIndex)
                {
                    int index = intFloorplanCount - newFloorIndex - 1;
                    BuildingEditor.floorplan = (Floorplan)floorplans[index];
                    //                    selectedFloorplan = floorplans[index];
                }
                if (intFloorplanCount > 10)
                    EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                //                #region Shortcut key guide
                //                SETTINGS.showFloorplanShortcutKeys = EditorGUILayout.Foldout(SETTINGS.showFloorplanShortcutKeys, "Shortcuts");
                //                if (SETTINGS.showFloorplanShortcutKeys)
                //                {
                //                    GUIStyle rightText = new GUIStyle("label");
                //                    rightText.alignment = TextAnchor.MiddleRight;
                //                    GUIStyle leftText = new GUIStyle("label");
                //                    leftText.alignment = TextAnchor.MiddleLeft;
                //                    GUIStyle middleText = new GUIStyle("label");
                //                    middleText.alignment = TextAnchor.MiddleCenter;
                //
                //                    float shortcutTextWidth = (BuildingEditor.MAIN_GUI_WIDTH - 50) * 0.5f;
                //                    //                    float equalTextWidth = 25;
                //                    EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                //
                //                    EditorGUILayout.BeginVertical(GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("LMB   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField(SETTINGS.editModeAddDoor + "   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField(SETTINGS.editModeAddWindow + "   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField(SETTINGS.editModeAddRoom + "   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField(SETTINGS.editModeAddRoomPoint + "   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField(SETTINGS.editModeAddVerticalSpace + "   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Escape   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Backspace   =", rightText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.EndVertical();
                //
                //                    EditorGUILayout.BeginVertical(GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Select Room Node", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Add Door", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Add Window", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Add Room", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Add Room Point", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Add Vertical Space", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Exit Edit Floorplan Mode", leftText, GUILayout.Width(shortcutTextWidth));
                //                    EditorGUILayout.LabelField("Remove Selected Point", leftText, GUILayout.Width(shortcutTextWidth));
                //
                //                    EditorGUILayout.Space();
                //                    EditorGUILayout.BeginHorizontal();
                //                    GUILayout.FlexibleSpace();
                //                    if (GUILayout.Button("Configure", GUILayout.Height(15)))
                //                        Selection.activeObject = SETTINGS;
                //                    EditorGUILayout.EndHorizontal();
                //
                //                    EditorGUILayout.EndVertical();
                //
                //                    EditorGUILayout.EndHorizontal();
                //                }
                //                #endregion

                if (!editing)
                {
                    if (GUILayout.Button("Edit Floorplan [Ctrl + E]", GUILayout.Width(150))) ToggleEdit(true);
                }
                else
                {
                    EscDel();
                    if (GUILayout.Button("Finish Editing [Esc]", GUILayout.Width(150))) ToggleEdit(false);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Edit mode: ");
                    EditorGUILayout.LabelField(mode.ToString());
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical("box", GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                    EditorGUILayout.LabelField("Add");

                    int currentEditMode = -1;
                    switch (mode)
                    {
                        case EditModes.BuildFloorplanInterior:
                            currentEditMode = 0;
                            break;
                        case EditModes.AddPoint:
                            currentEditMode = 1;
                            break;
                        case EditModes.AddVertical:
                            currentEditMode = 2;
                            break;
                        case EditModes.AddPortal:
                            if (portalIsDoor)
                                currentEditMode = 3;
                            else
                                currentEditMode = 4;
                            break;
                    }

                    string roomText = string.Format("Room [{0}]", SETTINGS.editModeAddRoom);
                    string pointText = string.Format("Room Point [{0}]", SETTINGS.editModeAddRoomPoint);
                    string verticalText = string.Format("Vertical Space [{0}]", SETTINGS.editModeAddVerticalSpace);
                    string doorText = string.Format("Door [{0}]", SETTINGS.editModeAddDoor);
                    string windowText = string.Format("Window [{0}]", SETTINGS.editModeAddWindow);
                    string[] editModeTitles = { roomText, pointText, verticalText, doorText, windowText };
                    currentEditMode = GUILayout.SelectionGrid(currentEditMode, editModeTitles, 3);

                    switch (currentEditMode)
                    {
                        case 0:
                            mode = EditModes.BuildFloorplanInterior;
                            break;
                        case 1:
                            mode = EditModes.AddPoint;
                            break;
                        case 2:
                            mode = EditModes.AddVertical;
                            break;
                        case 3:
                            mode = EditModes.AddPortal;
                            portalIsDoor = true;
                            break;
                        case 4:
                            mode = EditModes.AddPortal;
                            portalIsDoor = false;
                            break;
                    }

                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("Make Single Room From Floor Footprint"))
                    {
                        FloorplanUtil.FillFloorWithSingleRoom(BuildingEditor.building, selectedVolume, selectedFloorplan);
                        GUI.changed = true;
                    }
                }

                EditorGUILayout.Space();

                if (BuildingEditor.opening != null)
                    VerticalSpaceGUI();
                else if (BuildingEditor.roomPortal != null)
                    PortalGUI(BuildingEditor.roomPortal, true);
                else if (BuildingEditor.room != null)
                    RoomGUI(BuildingEditor.room);
            }
        }



        private static bool FloorplanSceneButtons(Building _building)
        {
            int numberOfVolumes = _building.numberOfPlans;
            Quaternion rotation = _building.transform.rotation;

            if (BuildingEditor.floorplan != null)
            {
                Volume volume = BuildingEditor.volume;
                Floorplan floorplan = BuildingEditor.floorplan;
                float intPlanBaseHeight = volume.CalculateFloorHeight(volume.Floor(floorplan));
                Vector3 baseUpV = Vector3.up * intPlanBaseHeight;
                Room[] rooms = floorplan.rooms.ToArray();
                int roomCount = rooms.Length;
                for (int r = 0; r < roomCount; r++)
                {
                    Room room = rooms[r];
                    RoomPortal[] portals = room.GetAllPortals();
                    int portalCount = portals.Length;
                    for (int pt = 0; pt < portalCount; pt++)
                    {
                        RoomPortal portal = portals[pt];
                        Vector3 portalPosition = PortalPosition(rotation, room, portal) + baseUpV + Vector3.up * portal.height * 0.5f;
                        Quaternion portalDirection = Quaternion.Euler(45, 90, 0);
                        float portalHandleSize = portal.width * 0.5f;
                        if (UnityVersionWrapper.HandlesDotButton(portalPosition, portalDirection, portalHandleSize, portalHandleSize))
                        {
                            BuildingEditor.volume = volume;
                            BuildingEditor.floorplan = floorplan;
                            BuildingEditor.room = room;
                            BuildingEditor.roomPortal = portal;
                            //                            mode = EditModes.EditPortal;
                            clickdata = null;
                            Repaint();
                            EditorGUIUtility.ExitGUI();
                            return true;
                        }
                    }
                }
            }



            for (int v = 0; v < numberOfVolumes; v++)
            {
                IVolume volume = _building[v];

                //Draw vertical openings
                VerticalOpening[] openings = _building.GetAllOpenings();
                //todo highlight volume openings
                int openingCount = openings.Length;
                for (int o = 0; o < openingCount; o++)
                {
                    VerticalOpening opening = openings[o];

                    Vector3 openingPosition = opening.position.vector3XZ;
                    openingPosition.y = volume.floorHeight * opening.baseFloor;
                    Vector3 openingSize = opening.size.vector3XZ;

                    float portalHandleSize = openingSize.magnitude * 0.5f;
                    if (UnityVersionWrapper.HandlesDotButton(openingPosition, Quaternion.identity, 0, portalHandleSize))
                    {
                        BuildingEditor.volume = (Volume)volume;
                        BuildingEditor.opening = opening;
                        //                        mode = EditModes.EditVertical;
                        clickdata = null;
                        EditorGUIUtility.ExitGUI();
                        return true;
                    }
                }
            }

            return false;
        }

        private static void DrawPortal(Quaternion rotation, float baseHeight, float floorHeight, Room room, RoomPortal portal, Color lineColour, Color fillColor)
        {
            int wallIndex = portal.wallIndex;
            Vector3 p0 = rotation * room[wallIndex].position.vector3XZ;
            Vector3 p1 = rotation * room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            Vector3 baseUp = Vector3.up * (floorHeight - portal.height) * portal.verticalPosition;
            Vector3 portalUp = baseUp + Vector3.up * portal.height;
            Vector3 pointPos = PortalPosition(rotation, room, portal);
            pointPos.y = baseHeight;
            Vector3 wallDirection = (p1 - p0).normalized;
            Vector3 portalCross = Vector3.Cross(Vector3.down, wallDirection);
            float defaultWidth = portal.width * 0.5f;
            float defaultDepth = 0.1f;

            Vector3 v0 = pointPos + wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v1 = pointPos + wallDirection * defaultWidth - portalCross * defaultDepth;
            Vector3 v2 = pointPos - wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v3 = pointPos - wallDirection * defaultWidth - portalCross * defaultDepth;


            Handles.color = lineColour;
            Handles.DrawAAPolyLine(v0 + baseUp, v1 + baseUp, v3 + baseUp, v2 + baseUp);
            Handles.DrawAAPolyLine(v2 + baseUp, v2 + portalUp, v0 + portalUp, v0 + baseUp);
            Handles.DrawAAPolyLine(v3 + baseUp, v3 + portalUp, v1 + portalUp, v1 + baseUp);
            Handles.DrawAAPolyLine(v0 + portalUp, v1 + portalUp, v3 + portalUp, v2 + portalUp);
            Handles.color = fillColor;
            Handles.DrawAAConvexPolygon(v0 + baseUp, v1 + baseUp, v1 + portalUp, v0 + portalUp);
            Handles.DrawAAConvexPolygon(v2 + baseUp, v3 + baseUp, v3 + portalUp, v2 + portalUp);
            Handles.DrawAAConvexPolygon(v0 + baseUp, v1 + baseUp, v3 + baseUp, v2 + baseUp);
            Handles.DrawAAConvexPolygon(v0 + portalUp, v1 + portalUp, v3 + portalUp, v2 + portalUp);
        }

        private static void DrawPortalBaseHandle(Quaternion rotation, float baseHeight, float floorHeight, Room room, RoomPortal portal, Color lineColour, Color fillColor)
        {
            int wallIndex = portal.wallIndex;
            Vector3 p0 = rotation * room[wallIndex].position.vector3XZ;
            Vector3 p1 = rotation * room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            Vector3 baseUp = Vector3.up * (floorHeight - portal.height) * portal.verticalPosition;
            Vector3 portalUp = baseUp + Vector3.up * portal.height;
            Vector3 pointPos = PortalPosition(rotation, room, portal);
            pointPos.y = baseHeight;
            Vector3 wallDirection = (p1 - p0).normalized;
            Vector3 portalCross = Vector3.Cross(Vector3.down, wallDirection);
            float defaultWidth = portal.width * 0.5f;
            float defaultDepth = 0.1f;

            Vector3 v0 = pointPos + wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v1 = pointPos + wallDirection * defaultWidth - portalCross * defaultDepth;
            Vector3 v2 = pointPos - wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v3 = pointPos - wallDirection * defaultWidth - portalCross * defaultDepth;

            Handles.color = lineColour;
            Handles.DrawAAPolyLine(pointPos - wallDirection, pointPos + wallDirection);
            Handles.DrawAAPolyLine(pointPos - portalCross, pointPos + portalCross);
            Handles.DrawAAPolyLine(pointPos, pointPos + baseUp);
            UnityVersionWrapper.HandlesArrowCap(0, pointPos + baseUp, Quaternion.LookRotation(Vector3.down), portal.height * 0.7f);

            Handles.DrawAAConvexPolygon(v0, v1, v3, v2);

        }

        private static Vector3 PortalPosition(Quaternion rotation, Room room, RoomPortal portal)
        {
            int wallIndex = portal.wallIndex;
            if (wallIndex == -1)
                return Vector3.zero;
            Vector3 p0 = rotation * room[wallIndex % room.numberOfPoints].position.vector3XZ;
            Vector3 p1 = rotation * room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            return Vector3.Lerp(p0, p1, portal.lateralPosition);
        }

        private static void PortalGUI(RoomPortal portal, bool showLateral = false)
        {
            BuildRSettings settings = BuildingEditor.GetSettings();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(350));
            EditorGUILayout.LabelField("Edit Portal");

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Width: ", GUILayout.Width(110));
            portal.width = EditorGUILayout.Slider(portal.width, settings.minimumWindowWidth, settings.maximumWindowWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Height: ", GUILayout.Width(110));
            portal.height = EditorGUILayout.Slider(portal.height, settings.minimumWindowHeight, BuildingEditor.volume.floorHeight);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Vertical Position: ", GUILayout.Width(110));
            portal.verticalPosition = EditorGUILayout.Slider(portal.verticalPosition, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (showLateral)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Wall Position: ", GUILayout.Width(110));
                portal.lateralPosition = EditorGUILayout.Slider(portal.lateralPosition, 0, 1);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            if (BuildingEditor.room != null)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    BuildingEditor.room.RemovePortal(BuildingEditor.roomPortal);
                    BuildingEditor.room = null;
                    BuildingEditor.roomPortal = null;
                    Repaint();
                    return;
                }
            }

            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                BuildingEditor.room = null;
                BuildingEditor.roomPortal = null;
                Repaint();
                return;
            }

            //            if (portal.modified)
            //                Event.current.Use();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (portal.modified)
                Repaint();

            //            Debug.Log("PortalGUI");
        }

        private static void VerticalSpaceGUI()
        {
            VerticalOpening opening = BuildingEditor.opening;
            if (opening == null) return;
            //            BuildRSettings settings = BuildRSettings.GetSettings();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(350));
            EditorGUILayout.LabelField("Edit Vertical Opening");

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Name: ", GUILayout.Width(110));
            opening.name = EditorGUILayout.DelayedTextField(opening.name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Usage: ", GUILayout.Width(110));
            opening.usage = (VerticalOpening.Usages)EditorGUILayout.EnumPopup(opening.usage);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            opening.position = Vector2IntEditor.VGUI(opening.position, "Position");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Rotation: ", GUILayout.Width(110));
            opening.rotation = EditorGUILayout.Slider(opening.rotation, -180, 180);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            opening.size = Vector2IntEditor.VGUI(opening.size, "Size");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Base Floor: ", GUILayout.Width(110));
            if (GUILayout.Button("-"))
                opening.baseFloor--;
            opening.baseFloor = EditorGUILayout.IntField(opening.baseFloor);
            if (GUILayout.Button("+"))
                opening.baseFloor++;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Floors: ", GUILayout.Width(110));
            if (GUILayout.Button("-"))
                opening.floors--;
            opening.floors = EditorGUILayout.IntField(opening.floors);
            if (GUILayout.Button("+"))
                opening.floors++;
            EditorGUILayout.EndHorizontal();

            if (opening.usage == VerticalOpening.Usages.Stairwell)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
                EditorGUILayout.LabelField("Stair Width: ", GUILayout.Width(110));
                opening.stairWidth = EditorGUILayout.Slider(opening.stairWidth, 0.9f, 10);
                EditorGUILayout.EndHorizontal();
            }

            switch (opening.usage)
            {
                case VerticalOpening.Usages.Space:
                    opening.surfaceA = SurfacePicker("External Wall", opening.surfaceA);
                    opening.surfaceB = SurfacePicker("Internal Wall", opening.surfaceB);
                    break;

                case VerticalOpening.Usages.Elevator:
                    opening.surfaceA = SurfacePicker("External Wall", opening.surfaceA);
                    opening.surfaceB = SurfacePicker("Internal Wall", opening.surfaceB);
                    opening.surfaceC = SurfacePicker("Door Frame", opening.surfaceC);
                    break;

                case VerticalOpening.Usages.Stairwell:
                    opening.surfaceA = SurfacePicker("External Wall", opening.surfaceA);
                    opening.surfaceB = SurfacePicker("Internal Wall", opening.surfaceB);
                    opening.surfaceD = SurfacePicker("Internal Floor", opening.surfaceD);
                    opening.surfaceC = SurfacePicker("Door Frame", opening.surfaceC);
                    break;
            }

            EditorGUILayout.EndVertical();

            if (opening.isModified)
                BuildingEditor.editor.UpdateGui();
            //            {
            //                BuildingEditor.
            //            }
        }

        private static Surface SurfacePicker(string title, Surface current)
        {
            EditorGUILayout.LabelField(title, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            Surface output = EditorGUILayout.ObjectField("", current, typeof(Surface), false, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH)) as Surface;
            return output;
        }

        private static void RoomGUI(Room room)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(string.Format("Editing Room: {0}", room.roomName));

            room.roomName = EditorGUILayout.DelayedTextField(room.roomName);
            //            if(room == null) return;

            room.style = (RoomStyle)EditorGUILayout.ObjectField("Room Style", room.style, typeof(RoomStyle), false, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));


            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            if (BuildingEditor.floorplan != null)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    BuildingEditor.floorplan.rooms.Remove(room);
                    BuildingEditor.room = null;
                    Repaint();
                    return;
                }
            }

            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                BuildingEditor.room = null;
                Repaint();
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private static Vector3 CheckMouseSnap(Vector3 mousePosition)//TODO
        {
            bool snapMouse = SETTINGS.snapToGrid ^ Event.current.shift;
            if (!snapMouse) return mousePosition;

            //snap output to grid
            Vector3 output = mousePosition;
            output.x = Mathf.Round(output.x);
            output.z = Mathf.Round(output.z);

            if (BuildingEditor.volume != null)
            {
                Dictionary<int, List<Vector2Int>> externalWallAnchors = BuildingEditor.volume.facadeWallAnchors;
                int facadeCount = externalWallAnchors.Count;

                //default closest values to current grid snap
                float closestX = Mathf.Abs(mousePosition.x - output.x);
                float closestZ = Mathf.Abs(mousePosition.z - output.z);
                for (int f = 0; f < facadeCount; f++)
                {
                    List<Vector2Int> wallAnchors = externalWallAnchors[f];
                    int anchorCount = wallAnchors.Count;
                    for (int a = 0; a < anchorCount; a++)
                    {
                        Vector3 anchor = wallAnchors[a].vector3XZ;
                        Vector3 flatMousePosition = new Vector3(mousePosition.x, 0, mousePosition.z);

                        if(Vector3.Distance(flatMousePosition, anchor) < HandleUtility.GetHandleSize(mousePosition) * 0.5f)
                        {
                            output.x = anchor.x;
                            output.z = anchor.z;
                            return output;
                        }

                        float thisClosestX = Mathf.Abs(mousePosition.x - anchor.x) / 10;
                        float thisClosestZ = Mathf.Abs(mousePosition.z - anchor.z) / 10;

                        if (thisClosestX < closestX)
                        {
                            closestX = thisClosestX;
                            output.x = anchor.x;
                        }
                        if (thisClosestZ < closestZ)
                        {
                            closestZ = thisClosestZ;
                            output.z = anchor.z;
                        }
                    }
                }
                return output;
            }
            return output;
        }

        private static void VolumeFloorSelector(Camera cam)
        {
            if (BuildingEditor.volume != null)
            {
                Building building = BuildingEditor.building;
                BuildingEditor.EnableInteriorGeneration();
                Vector3 bPosition = building.transform.position;
                //                Quaternion bRotation = building.transform.rotation;
                Volume volume = BuildingEditor.volume;
                int floorplanCount = volume.floors;
                Vector3 volumeCenter = bPosition + volume.bounds.center;
                Vector3 floorplanRight = cam.transform.right * volume.bounds.size.magnitude * 0.5f;
                Vector3 guiWorldPoint = volumeCenter + floorplanRight;
                float dot = Vector3.Dot((guiWorldPoint - cam.transform.position).normalized, cam.transform.forward);
                if (dot < 0) return;


                Vector2 portalScreenTop = Camera.current.WorldToScreenPoint(guiWorldPoint);

                //                if(portalScreenTop.x > cam.pixelWidth - 120) portalScreenTop.x = cam.pixelWidth - 120;
                int pixelHeight = floorplanCount * 25;//Mathf.RoundToInt(portalScreenTop.y - portalScreenBottom.y);
                portalScreenTop.y = Camera.current.pixelHeight - (portalScreenTop.y + pixelHeight + 20 + 25);
                Rect portalRect = new Rect(portalScreenTop, new Vector2(120, pixelHeight + 20 + 25));

                if (Event.current.type == EventType.Repaint)
                    mouseOverSceneGUI = portalRect.Contains(Event.current.mousePosition);

                Handles.BeginGUI();
                GUI.Box(portalRect, "");

                GUIContent[] guiContents = new GUIContent[floorplanCount];
                for (int i = 0; i < floorplanCount; i++)
                {
                    int rIndex = floorplanCount - 1 - i;
                    guiContents[i] = new GUIContent(string.Format("Floor {0}", rIndex));
                }
                IFloorplan[] floorplans = volume.InteriorFloorplans();
                int selectedInteriorIndex = floorplanCount - volume.Floor(BuildingEditor.floorplan) - 1;
                if (selectedInteriorIndex == -1)
                    selectedInteriorIndex = floorplanCount - 1;
                Rect selectGridRect = new Rect(portalRect.position, new Vector2(portalRect.width, pixelHeight));
                int newSelectionIndex = GUI.SelectionGrid(selectGridRect, selectedInteriorIndex, guiContents, 1);
                if (newSelectionIndex != selectedInteriorIndex)
                {
                    int index = floorplanCount - newSelectionIndex - 1;
                    BuildingEditor.volume = volume;
                    BuildingEditor.floorplan = (Floorplan)floorplans[index];
                    Repaint();
                }

                if (BuildingEditor.floorplan != null)
                {
                    Rect editRect = new Rect(new Vector2(portalRect.x, portalRect.y + pixelHeight + 20), new Vector2(portalRect.width, 25));
                    if (!editing)
                    {
                        if (GUI.Button(editRect, "Edit Floorplan")) ToggleEdit(true);
                    }
                    else
                    {
                        if (GUI.Button(editRect, "Finish Editing")) ToggleEdit(false);
                    }
                }

                Handles.EndGUI();
            }
        }

        private static void EscDel()
        {
            Event current = Event.current;
            if (current.type == EventType.Layout) return;
            if (current.type == EventType.Repaint) return;
            bool backspace = current.keyCode == KeyCode.Backspace;
            bool escIsDown = current.keyCode == KeyCode.Escape;
            bool mouseIsDown = false;
            switch (current.type)
            {
                case EventType.MouseDown:
                    mouseIsDown = true;
                    break;

                case EventType.KeyUp:

                    if (!editing || current.control) break;

                    EditModes newMode = mode;
                    if (current.keyCode == SETTINGS.editModeAddRoom)
                    {
                        newMode = mode == EditModes.BuildFloorplanInterior ? EditModes.FloorplanSelection : EditModes.BuildFloorplanInterior;
                        BuildingEditor.room = null;
                    }

                    if (current.keyCode == SETTINGS.editModeAddRoomPoint)
                    {
                        newMode = mode == EditModes.AddPoint ? EditModes.FloorplanSelection : EditModes.AddPoint;
                        BuildingEditor.room = null;
                    }

                    if (current.keyCode == SETTINGS.editModeAddDoor)
                    {
                        if (mode != EditModes.AddPortal || (mode == EditModes.AddPortal && !portalIsDoor))
                        {
                            newMode = EditModes.AddPortal;
                            portalIsDoor = true;
                        }
                        else
                            newMode = EditModes.FloorplanSelection;

                        tempPortal = null;
                        BuildingEditor.room = null;
                    }

                    if (current.keyCode == SETTINGS.editModeAddWindow)
                    {
                        if (mode != EditModes.AddPortal || (mode == EditModes.AddPortal && portalIsDoor))
                        {
                            newMode = EditModes.AddPortal;
                            portalIsDoor = false;
                        }
                        else
                            newMode = EditModes.FloorplanSelection;

                        tempPortal = null;
                        BuildingEditor.room = null;
                    }

                    if (current.keyCode == SETTINGS.editModeAddVerticalSpace)
                    {
                        if (mode != EditModes.AddVertical)
                            newMode = EditModes.AddVertical;
                        else
                            newMode = EditModes.FloorplanSelection;
                        BuildingEditor.room = null;
                    }

                    if (newMode != mode)
                    {
                        mode = newMode;
                        Repaint();
                    }

                    break;
            }
            if (!mouseIsDown)
            {
                if (escIsDown)
                {
                    mode = EditModes.FloorplanSelection;
                    ToggleEdit(false);
                }
                if (backspace)
                {
                    VerticalOpening opening = BuildingEditor.opening;
                    if (opening != null)
                    {
                        if (EditorUtility.DisplayDialog("Delete Vertical Space", "Do you wish to delete the selected vertical space?", "yes", "no"))
                        {
                            Undo.RecordObject(BuildingEditor.building, "Delete Vertical Space");
                            BuildingEditor.building.RemoveOpening(opening);
                            BuildingEditor.opening = null;
                            Repaint();
                            return;
                        }
                    }
                    Floorplan floorplan = BuildingEditor.floorplan;
                    Room room = BuildingEditor.room;
                    RoomPortal roomPortal = BuildingEditor.roomPortal;

                    if (floorplan != null && room != null && roomPortal != null)
                    {
                        if (EditorUtility.DisplayDialog("Delete Room Portal", "Do you wish to delete the selected room portal?", "yes", "no"))
                        {
                            Undo.RecordObject(floorplan, "Delete Room Portal");
                            room.RemovePortal(roomPortal);
                            BuildingEditor.roomPortal = null;
                            BuildingEditor.room = null;
                            Repaint();
                            return;
                        }
                    }

                    if (floorplan != null && room != null && roomPortal == null)
                    {
                        if (EditorUtility.DisplayDialog("Delete Room", "Do you wish to delete the selected room?", "yes", "no"))
                        {
                            Undo.RecordObject(floorplan, "Delete Room");
                            floorplan.rooms.Remove(room);
                            BuildingEditor.room = null;
                            Repaint();
                        }
                    }
                }
            }
        }

        private static void DrawRoomIssues()
        {
            Building building = BuildingEditor.building;
            if (building == null) return;
            Volume volume = BuildingEditor.volume;
            Floorplan floorplan = BuildingEditor.floorplan;
            Vector3 position = building.transform.position;
            if (volume != null && floorplan != null)
            {
                float intPlanBaseHeight = volume.CalculateFloorHeight(volume.Floor(floorplan));
                Vector3 baseUpV = Vector3.up * intPlanBaseHeight;
                int floorplanWallIssueCount = BuildingEditor.floorplan.issueList.Count;
                for (int flil = 0; flil < floorplanWallIssueCount; flil++)
                {
                    Floorplan.WallIssueItem issue = BuildingEditor.floorplan.issueList[flil];
                    FloorplanUtil.RoomWall[] roomWalls = FloorplanUtil.CalculatePoints(issue.inRoom, BuildingEditor.volume);
                    FloorplanUtil.RoomWall roomWall = roomWalls[issue.wallIndex];
                    if (roomWall.isExternal) continue;

                    Vector3 wallUp = Vector3.up * BuildingEditor.volume.floorHeight;
                    Vector3 ws0 = roomWall.baseA.vector3XZ + baseUpV + position;
                    Vector3 ws1 = roomWall.baseB.vector3XZ + baseUpV + position;
                    Vector3 p0 = issue.point.vector3XZ + baseUpV + wallUp * 0.5f + position;

                    Handles.color = SETTINGS.warningColour;
                    Handles.DrawAAPolyLine(ws0, ws1, ws1 + wallUp, ws0 + wallUp);
                    Color warnFillColor = SETTINGS.warningColour;
                    warnFillColor.a = 0.2f;
                    Handles.color = warnFillColor;
                    Handles.DrawAAConvexPolygon(ws0, ws1, ws1 + wallUp, ws0 + wallUp);

                    Handles.BeginGUI();
                    Vector2 guipoint = HandleUtility.WorldToGUIPoint(p0);
                    Rect warnRect = new Rect(guipoint, new Vector2(250, 130));
                    GUILayout.BeginArea(warnRect);
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.HelpBox("Room point is intersecting adjacent wall with no point", MessageType.Warning);
                    if (GUILayout.Button("Fix"))
                    {
                        Undo.RecordObject(BuildingEditor.floorplan, "Fix point");
                        int index = issue.wallIndex < issue.inRoom.numberOfPoints - 1 ? issue.wallIndex + 1 : 0;
                        issue.inRoom.InsertPoint(index, issue.point);
                        BuildingEditor.floorplan.CheckWallIssues();
                        break;
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.EndArea();
                    Handles.EndGUI();
                }
            }
        }

        private static void DrawMouseOverOutline(BuildingEditorUtils.FloorplanClick data)
        {
            if (data.portal != null)
                DrawPortalOutline(data);
            else if (data.opening != null)
                DrawOpeningOutline(data);
            else if (data.room != null)
                DrawRoomOutline(data);
        }

        private static void DrawPortalOutline(BuildingEditorUtils.FloorplanClick clickdata)
        {
            Vector3 position = BuildingEditor.building.transform.position;
            Quaternion rotation = BuildingEditor.building.transform.rotation;
            Volume volume = clickdata.volume;
            float floorHeight = volume.floorHeight;
            float baseHeight = volume.baseHeight;
            Room room = clickdata.room;
            RoomPortal portal = clickdata.portal;
            int wallIndex = portal.wallIndex;
            Vector3 p0 = room[wallIndex].position.vector3XZ;
            Vector3 p1 = room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            Vector3 baseUp = Vector3.up * (floorHeight - portal.height) * portal.verticalPosition;
            Vector3 portalUp = baseUp + Vector3.up * portal.height;
            Vector3 pointPos = PortalPosition(Quaternion.identity, room, portal);
            pointPos.y = baseHeight;
            Vector3 wallDirection = (p1 - p0).normalized;
            //            Vector3 portalCross = Vector3.Cross(Vector3.down, wallDirection);
            float defaultWidth = portal.width * 0.5f;
            //            float defaultDepth = 0.1f;

            Vector3 v0 = rotation * (pointPos - wallDirection * defaultWidth) + position;
            Vector3 v1 = rotation * (pointPos + wallDirection * defaultWidth) + position;

            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(6, v0 + baseUp, v1 + baseUp, v1 + portalUp, v0 + portalUp, v0 + baseUp);
        }

        private static void DrawRoomOutline(BuildingEditorUtils.FloorplanClick clickdata)
        {
            Vector3 position = BuildingEditor.building.transform.position;
            Quaternion rotation = BuildingEditor.building.transform.rotation;
            Volume volume = clickdata.volume;
            Vector3 vUp = Vector3.up * volume.floorHeight;
            Floorplan floorplan = clickdata.floorplan;
            float intPlanBaseHeight = volume.CalculateFloorHeight(volume.Floor(floorplan));
            Vector3 baseUpV = Vector3.up * intPlanBaseHeight;
            Room room = clickdata.room;
            FloorplanUtil.RoomWall[] roomWalls = FloorplanUtil.CalculatePoints(room, volume);
            int roomWallCount = roomWalls.Length;

            Handles.color = Color.yellow;

            for (int rwp = 0; rwp < roomWallCount; rwp++)
            {
                //                                roomPoints.Add(roomWall[rwp].baseA.vector2);
                //                                roomPoints.AddRange(roomWall[rwp].offsetPoints);
                FloorplanUtil.RoomWall roomWall = roomWalls[rwp];
                int offsetCount = roomWall.offsetPoints.Length;

                Vector2Int pi0 = roomWall.baseA;
                Vector2Int pi1 = roomWall.baseB;

                if (pi0 == pi1) continue;//not a wall

                for (int op = 0; op < offsetCount - 1; op++)
                {
                    Vector3 ws0 = rotation * (new Vector3(roomWall.offsetPoints[op].x, 0, roomWall.offsetPoints[op].y) + baseUpV) + position;
                    int nextIndex = (op + 1) % offsetCount;
                    Vector3 ws1 = rotation * (new Vector3(roomWall.offsetPoints[nextIndex].x, 0, roomWall.offsetPoints[nextIndex].y) + baseUpV) + position;
                    Vector3 ws2 = ws0 + vUp;
                    Vector3 ws3 = ws1 + vUp;

                    Handles.DrawAAPolyLine(6, ws0, ws1, ws3, ws2);
                }
            }
        }

        private static void DrawOpeningOutline(BuildingEditorUtils.FloorplanClick clickdata)
        {
            Vector3 position = BuildingEditor.building.transform.position;
            Quaternion rotation = BuildingEditor.building.transform.rotation;
            Building building = BuildingEditor.building;
            if (building == null) return;
            Volume volume = clickdata.volume;
            VerticalOpening[] openings = building.GetAllOpenings();
            int openingCount = openings.Length;
            Handles.color = Color.yellow;
            for (int o = 0; o < openingCount; o++)
            {
                VerticalOpening opening = openings[o];

                Vector3 openingPosition = opening.position.vector3XZ;
                openingPosition.y = volume.floorHeight * opening.baseFloor;
                Vector3 openingSize = opening.size.vector3XZ;
                float openingWidth = openingSize.x;
                float openingHeight = openingSize.z;
                Quaternion openingRotation = Quaternion.Euler(0, opening.rotation, 0);
                Vector3 p0 = rotation * (openingPosition + openingRotation * new Vector3(-openingWidth, 0, -openingHeight) * 0.5f) + position;
                Vector3 p1 = rotation * (openingPosition + openingRotation * new Vector3(openingWidth, 0, -openingHeight) * 0.5f) + position;
                Vector3 p2 = rotation * (openingPosition + openingRotation * new Vector3(openingWidth, 0, openingHeight) * 0.5f) + position;
                Vector3 p3 = rotation * (openingPosition + openingRotation * new Vector3(-openingWidth, 0, openingHeight) * 0.5f) + position;

                Vector3 floorUpA = Vector3.up * volume.CalculateFloorHeight(volume.Floor(BuildingEditor.floorplan));
                Vector3 floorUpB = floorUpA + Vector3.up * volume.floorHeight;
                Handles.DrawAAPolyLine(6, p0 + floorUpA, p1 + floorUpA, p2 + floorUpA, p3 + floorUpA, p0 + floorUpA);
                Handles.DrawAAPolyLine(6, p0 + floorUpB, p1 + floorUpB, p2 + floorUpB, p3 + floorUpB, p0 + floorUpB);
                Handles.DrawAAPolyLine(6, p0 + floorUpA, p0 + floorUpB);
                Handles.DrawAAPolyLine(6, p1 + floorUpA, p1 + floorUpB);
                Handles.DrawAAPolyLine(6, p2 + floorUpA, p2 + floorUpB);
                Handles.DrawAAPolyLine(6, p3 + floorUpA, p3 + floorUpB);
            }
        }

        static bool IsMouseOver()
        {
            return Event.current.type == EventType.Repaint &&
                   GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
        }
    }

}