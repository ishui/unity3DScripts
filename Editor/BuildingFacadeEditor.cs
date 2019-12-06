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



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace BuildR2 {
    public class BuildingFacadeEditor {
        private static Color FACADE_HOVER = new Color(0, 1, 0, 0.25f);
        private static Vector3[] highlightQuad = new Vector3[0];
//        private static Vector2 scrollPos = new Vector2();
//        private static int selectedFacadeIndex = -1;
        private static Facade selectedFacade = null;
        private static float CLICK_TIME = 0;

        private static DragDropContent dragDropContent = new DragDropContent();

        public static bool repaint = false;

        private static int editMode = 0;

        public static void Repaint() {
            repaint = true;
        }

        public static void OnSceneGUI(Building _building) {
            EventType eventType = Event.current.type;
            switch (eventType) {
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;

                case EventType.MouseUp:
                    ResetHighlight();
                    CheckDragDrop();
                    break;

                case EventType.DragUpdated:
                    FacadeDragAndDrop(_building);
                    break;

                case EventType.DragPerform:
                    //                    Debug.Log("DragPerform");
                    FacadeDragAndDrop(_building);
                    break;

                case EventType.DragExited:
                    CheckDragDrop();
                    //                    FacadeDragAndDrop(_building);
                    break;

                case EventType.KeyDown:
                    //                    Debug.Log(Event.current.keyCode == KeyCode.Escape);
                    //                    FacadeDragAndDrop(_building);
                    break;
            }

            Handles.color = FACADE_HOVER;
            int highlightQuadSize = highlightQuad.Length;
            for (int h = 0; h < highlightQuadSize; h += 4) {
                Vector3[] quad = new Vector3[4];
                quad[0] = highlightQuad[h + 0];
                quad[1] = highlightQuad[h + 1];
                quad[2] = highlightQuad[h + 2];
                quad[3] = highlightQuad[h + 3];
                Handles.DrawAAConvexPolygon(quad);//draw facade highlight
            }

            DrawFloorplans(_building);
        }

        public static void OnInspectorGUI(Building _building) {

            string[] options = { "Facade Volume", "Facade Asset Library" };
            editMode = GUILayout.Toolbar(editMode, options, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            switch (editMode) {
                case 0:
                    OnInspectorGUI_VolumeFacade(_building);
                    break;

                case 1:
                    FacadeLibraryInspectorGUI();
                    if (selectedFacade != null) {
                        FacadeEditor.OnInspectorGUI_S(selectedFacade);
                    }
                    break;
            }
        }

        public static void FacadeLibraryInspectorGUI() {
            BuildingEditor.BuildRHeader("Facade Design Library");

//            string[] guids = AssetDatabase.FindAssets("t:BuildR2.Facade");//t: type Facade
//            int libraryCount = guids.Length;

            EditorGUILayout.LabelField("Facade Library");
            //            EditorGUILayout.BeginHorizontal("Box");
            //            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH - 10), GUILayout.Height(200));
            FacadePicker();

            //            List<GUIContent> toolbar = new List<GUIContent>();
            //            for (int s = 0; s < libraryCount; s++)
            //            {
            //                string assetPath = AssetDatabase.GUIDToAssetPath(guids[s]);
            //                Facade facade = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Facade)) as Facade;
            //                if (facade != null)
            //                {
            //                    Texture texture = facade.previewTexture;
            //                    GUIContent content = new GUIContent();
            //                    content.text = facade.name;
            //                    content.tooltip = "tooltip here";
            //                    if (texture != null) 
            //                        content.image = texture;
            //                    toolbar.Add(content);
            //
            //                    if (selectedFacadeIndex == s)
            //                    {
            //                        selectedFacade = facade;
            //
            //                        DragAndDrop.PrepareStartDrag();// reset data
            //                        DragAndDrop.SetGenericData("BuildR2.Facade", selectedFacade);
            //
            //                        Object[] objectReferences = { selectedFacade };// Careful, null values cause exceptions in existing editor code.
            //                        DragAndDrop.objectReferences = objectReferences;// Note: this object won't be 'get'-able until the next GUI event.
            //                    }
            //                }
            //            }
            //            
            //            float calWidth = BuildingEditor.MAIN_GUI_WIDTH - 35;
            //            float calHeight = (libraryCount / 4f) * (calWidth / 4f);
            //            selectedFacadeIndex = GUILayout.SelectionGrid(selectedFacadeIndex, toolbar.ToArray(), 4, GUILayout.Width(calWidth), GUILayout.Height(calHeight));

            //            EditorGUILayout.EndScrollView();
            //            EditorGUILayout.EndHorizontal();

            //            if (selectedFacade != null)
            //            {
            //                Event currentEvent = Event.current;
            //                EventType currentEventType = currentEvent.type;
            //
            //                // The DragExited event does not have the same mouse position data as the other events,
            //                // so it must be checked now:
            //                if (currentEventType == EventType.DragExited) DragAndDrop.PrepareStartDrag();// Clear generic data when user pressed escape. (Unfortunately, DragExited is also called when the mouse leaves the drag area)
            //
            //                switch (currentEventType)
            //                {
            //                    case EventType.MouseDown:
            //                        DragAndDrop.PrepareStartDrag();// reset data
            //                        DragAndDrop.SetGenericData("BuildR2.Facade", selectedFacade);
            //
            //                        Object[] objectReferences = { selectedFacade };// Careful, null values cause exceptions in existing editor code.
            //                        DragAndDrop.objectReferences = objectReferences;// Note: this object won't be 'get'-able until the next GUI event.
            //
            //                        currentEvent.Use();
            //
            //                        break;
            //
            //                    case EventType.MouseDrag:
            //                        // If drag was started here:
            //                        Facade existingDragData = DragAndDrop.GetGenericData("BuildR2.Facade") as Facade;
            //
            //                        if (existingDragData != null)
            //                        {
            //                            DragAndDrop.StartDrag("Dragging List ELement");
            //                            currentEvent.Use();
            //                        }
            //
            //                        break;
            //                }
            //            }

        }

        private const int SECTION_PREVIEW_SIZE = 75;
        private static void FacadePicker() {

            string[] guids = AssetDatabase.FindAssets("t:BuildR2.Facade");//t: type Facade
            int facadeCount = guids.Length;
            int xCount = Mathf.FloorToInt((BuildingEditor.MAIN_GUI_WIDTH - 10) / SECTION_PREVIEW_SIZE);
            int yCount = Mathf.CeilToInt((facadeCount) / (float)xCount);

            List<Rect> facadeRects = new List<Rect>();
            Facade[] facades = new Facade[facadeCount];
            for (int f = 0; f < facadeCount; f++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[f]);
                facades[f] = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Facade)) as Facade;
            }

            EditorGUILayout.BeginVertical();
            for (int y = 0; y < yCount; y++) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < xCount; x++) {
                    int index = y * xCount + x;
                    if(index < facadeCount) {
                        Facade facade = facades[index];

                        string facadeName = facade != null ? facade.name : "";
                        GUIContent content = new GUIContent(facadeName);
                        content.tooltip = "Drag and drop onto an active BuildR facade!";
                        if (facade != null)
                            content = new GUIContent(facade.previewTexture);
                        EditorGUILayout.LabelField(content, GUILayout.Width(SECTION_PREVIEW_SIZE), GUILayout.Height(SECTION_PREVIEW_SIZE));
                        Rect sectionRect = GUILayoutUtility.GetLastRect();
                        facadeRects.Add(sectionRect);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            Event currentEvent = Event.current;
            EventType currentEventType = currentEvent.type;

            // The DragExited event does not have the same mouse position data as the other events,
            // so it must be checked now:
            if (currentEventType == EventType.DragExited) DragAndDrop.PrepareStartDrag();// Clear generic data when user pressed escape. (Unfortunately, DragExited is also called when the mouse leaves the drag area)

            switch (currentEventType) {
                case EventType.MouseDown:

                    Facade grabFacade = null;
                    for (int f = 0; f < facadeCount; f++) {
                        if (facadeRects[f].Contains(Event.current.mousePosition))
                            grabFacade = facades[f];
                    }

                    if (grabFacade != null) {
                        float time = (float)EditorApplication.timeSinceStartup;
                        if(time - CLICK_TIME < 0.25f) {
                            Selection.activeObject = grabFacade;
                        }
                        else {
                            DragAndDrop.PrepareStartDrag();// reset data
                            DragAndDrop.SetGenericData("BuildR2.Facade", grabFacade);

                            Object[] objectReferences = { grabFacade };// Careful, null values cause exceptions in existing editor code.
                            DragAndDrop.objectReferences = objectReferences;// Note: this object won't be 'get'-able until the next GUI event.
                        }
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    // If drag was started here:
                    Facade existingDragData = DragAndDrop.GetGenericData("BuildR2.Facade") as Facade;

                    if (existingDragData != null) {
                        DragAndDrop.StartDrag("Dragging List ELement");
                        currentEvent.Use();
                    }

                    break;

                case EventType.MouseUp:
                    CLICK_TIME = (float)EditorApplication.timeSinceStartup;
                    break;
            }
        }

        private static void OnInspectorGUI_VolumeFacade(Building building) {
            BuildingEditor.BuildRHeader("Building Facades");

            BuildingEditor.VolumeSelectorInspectorGUI();
            Volume volume = BuildingEditor.volume;
            if (volume == null) return;
            int facadeCount = volume.numberOfFacades;
            for (int f = 0; f < facadeCount; f++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Facade {0}", f + 1), GUILayout.Width(75));
                if (GUILayout.Button("Look At")) {
                    Vector3 p0 = volume.WorldPoint(f);
                    Vector3 p1 = volume.WorldPoint((f + 1) % facadeCount);
                    Vector3 facadeBaseCenter = Vector3.Lerp(p0, p1, 0.5f);
                    float volumeHeight = volume.planHeight;
                    Vector3 facadeCenter = facadeBaseCenter + Vector3.up * volumeHeight * 0.5f;
                    Vector3 facadeVector = p1 - p0;
                    Vector3 facadeDirection = facadeVector.normalized;
                    float facadeLength = facadeVector.magnitude;
                    Vector3 facadeNormal = Vector3.Cross(Vector3.up, facadeDirection);

                    var view = SceneView.lastActiveSceneView;
                    if (view != null) {
                        Quaternion lookRotation = Quaternion.LookRotation(-facadeNormal) * Quaternion.Euler(15, 0, 0);
                        float useSize = Mathf.Max(facadeLength, volumeHeight) * 1.2f;
                        view.LookAt(facadeCenter, lookRotation, useSize);
                    }
                }
                Facade currentFacade = volume.GetFacade(f);
                Facade facadeDesign = EditorGUILayout.ObjectField(currentFacade, typeof(Facade), false) as Facade;
                if (facadeDesign != currentFacade) {
                    volume.SetFacade(f, facadeDesign);
                    building.MarkModified();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void FacadeDragAndDrop(Building _building) {
            //            EventType eventType = Event.current.type;

            dragDropContent.Reset();

            //            if (eventType == EventType.DragExited) return;
            //            Debug.Log("FacadeDragAndDrop");
            Object[] objArray = DragAndDrop.objectReferences;
            if (objArray != null && objArray.Length == 1) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                if (objArray[0].GetType() == typeof(Facade)) {
                    //                    if (eventType == EventType.DragExited) Debug.Log("facade");
                    Facade dragContent = (Facade)objArray[0];
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    float dist;

                    int numberOfFloorplans = _building.numberOfPlans;
                    bool overNothing = true;
                    for (int f = 0; f < numberOfFloorplans; f++) {
                        //                        if (eventType == EventType.DragExited) Debug.Log("fpl");
                        IVolume volume = _building[f];
                        List<Vector3[]> facadeTriangles = GetFacadeTriangles(volume);
                        List<Vector3[]> facadeQuads = GetFacadeQuads(volume);
                        int facadeTriangleCount = facadeTriangles.Count;

                        for (int facadeIndex = 0; facadeIndex < facadeTriangleCount; facadeIndex++) {
                            Vector3[] tris = facadeTriangles[facadeIndex];
                            Vector3[] quads = facadeQuads[facadeIndex];
                            int triCount = tris.Length;
                            for (int t = 0; t < triCount; t += 3) {
                                Vector3 f0 = tris[t];
                                Vector3 f1 = tris[t + 1];
                                Vector3 f2 = tris[t + 2];
                                bool overBuilding = RayTriangle.TriangleIntersection(f0, f2, f1, ray, out dist);//this bloody works!
                                if (overBuilding) {
                                    //                                if (eventType == EventType.DragExited) Debug.Log("FacadeDragAndDrop");
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                                    highlightQuad = quads;
                                    overNothing = false;

                                    dragDropContent.building = _building;
                                    dragDropContent.volume = volume;
                                    dragDropContent.facadeIndex = facadeIndex;
                                    dragDropContent.facade = dragContent;

                                    //                                    if (eventType == EventType.DragExited)
                                    //                                    {
                                    //                                        Undo.RecordObject(floorplan, "Change Facade Design");
                                    //                                        floorplan.SetFacade(facadeIndex, dragContent);
                                    //                                        _building.MarkModified();
                                    //                                        ResetHighlight();
                                    //                                    }
                                    break;
                                }
                            }
                        }
                        if (!overNothing)
                            break;
                    }

                    if (overNothing)
                        ResetHighlight();
                }
            }
        }

        private static void CheckDragDrop() {
            if (dragDropContent.volume == null)
                return;
            IVolume volume = dragDropContent.volume;
            //TODO! Figure this out. Is it a bug?
            //            Undo.RecordObject(floorplan, "Set Facade");
            //            Undo.RegisterCompleteObjectUndo(floorplan, "Set Facade");
            volume.SetFacade(dragDropContent.facadeIndex, dragDropContent.facade);
            //            dragDropContent.building.MarkModified();
            dragDropContent.Reset();
            ResetHighlight();
        }

        private static void ResetHighlight() {
            highlightQuad = new Vector3[0];//reset highlight
        }

        private static List<Vector3[]> GetFacadeTriangles(IVolume volume) {
            List<Vector3[]> output = new List<Vector3[]>();
            int numberOfPoints = volume.numberOfPoints;
            Vector3 pU = Vector3.up * volume.planHeight;

            for (int p = 0; p < numberOfPoints; p++) {
                if (volume[p].IsWallStraight()) {
                    Vector3 p0 = volume.WorldPoint(p);
                    Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);

                    Vector3[] triA = new Vector3[6];
                    //tri a
                    triA[0] = (p0);
                    triA[1] = (p1);
                    triA[2] = (p0 + pU);
                    //tri b
                    triA[3] = (p1);
                    triA[4] = (p1 + pU);
                    triA[5] = (p0 + pU);
                    output.Add(triA);
                }
                else {

                    Vector3 p0 = volume.WorldPoint(p);
                    Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);//rotation * floorplan.BuildingPoint((p + 1) % numberOfPoints) + position);
                    Vector3 cw0 = volume.WorldControlPointA(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                    Vector3 cw1 = volume.WorldControlPointB(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                    Vector3[] curveWall = new Vector3[10];
                    for (int t = 0; t < 10; t++)
                        curveWall[t] = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);

                    Vector3[] tris = new Vector3[6 * 9];
                    for (int t = 0; t < 9; t++) {
                        tris[t * 6 + 0] = curveWall[t];
                        tris[t * 6 + 1] = curveWall[t + 1];
                        tris[t * 6 + 2] = curveWall[t] + pU;
                        tris[t * 6 + 3] = curveWall[t + 1];
                        tris[t * 6 + 4] = curveWall[t + 1] + pU;
                        tris[t * 6 + 5] = curveWall[t] + pU;
                    }
                    output.Add(tris);
                }
            }
            return output;
        }

        private static List<Vector3[]> GetFacadeQuads(IVolume volume) {
            List<Vector3[]> output = new List<Vector3[]>();
            int numberOfPoints = volume.numberOfPoints;
            Vector3 pU = Vector3.up * volume.planHeight;

            for (int p = 0; p < numberOfPoints; p++) {
                if (volume[p].IsWallStraight()) {
                    Vector3 p0 = volume.WorldPoint(p);
                    Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);

                    Vector3[] quad = new Vector3[4];
                    quad[0] = p0;
                    quad[1] = p1;
                    quad[2] = p1 + pU;
                    quad[3] = p0 + pU;
                    output.Add(quad);
                }
                else {

                    Vector3 p0 = volume.WorldPoint(p);
                    Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);//rotation * floorplan.BuildingPoint((p + 1) % numberOfPoints) + position);
                    Vector3 cw0 = volume.WorldControlPointA(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                    Vector3 cw1 = volume.WorldControlPointB(p);//rotation * floorplan.BuildingControlPoint(p) + position);
                    Vector3[] curveWall = new Vector3[10];
                    for (int t = 0; t < 10; t++)
                        curveWall[t] = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);

                    Vector3[] quads = new Vector3[4 * 9];
                    for (int t = 0; t < 9; t++) {
                        quads[t * 4 + 0] = curveWall[t];
                        quads[t * 4 + 1] = curveWall[t + 1];
                        quads[t * 4 + 2] = curveWall[t + 1] + pU;
                        quads[t * 4 + 3] = curveWall[t] + pU;
                    }
                    output.Add(quads);
                }
            }
            return output;
        }

        private static void DrawFloorplans(Building _building) {
            int numberOfFloorplans = _building.numberOfPlans;
            //            Vector3 position = _building.transform.position;
            //            Quaternion rotation = _building.transform.rotation;

            Camera sceneCamera = Camera.current;
            //            Ray ray = sceneCamera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
            //            Volume mouseOverPlan = BuildrUtils.OnFloorplanSelectionClick(_building, ray).volume;

            for (int f = 0; f < numberOfFloorplans; f++) {
                IVolume volume = _building[f];
                int numberOfPoints = volume.numberOfPoints;
                int numberOfFloors = volume.floors;
                float planHeight = volume.floorHeight;
                float totalPlanHeight = volume.planHeight;
                Vector3 planHeightV = totalPlanHeight * Vector3.up;

                Handles.color = Color.magenta;


                for (int p = 0; p < numberOfPoints; p++) {
                    Vector3 p0 = volume.WorldPoint(p);
                    Vector3 p1 = volume.WorldPoint((p + 1) % numberOfPoints);

                    Vector3 facadeDirection = Vector3.Cross(Vector3.up, (p1 - p0).normalized);
                    Plane facadePlane = new Plane(facadeDirection, p0);

                    bool planeVisible = facadePlane.SameSide(p0 + facadeDirection, sceneCamera.transform.position);
                    if (!planeVisible)
                        continue;

                    //                    Vector3 cameraDirection = sceneCamera.transform.forward;
                    //                    float facadeDot = Vector3.Dot(facadeDirection, cameraDirection);

                    Facade facadeDesign = volume.GetFacade(p);
                    string facadeDesignTitle = (facadeDesign != null) ? facadeDesign.name : "no facade design set(BuildingFacadeEditor)";
                    Handles.Label(Vector3.Lerp(p0, p1, 0.5f), facadeDesignTitle);

                    //                    if (facadeDot > -0.1f)
                    //                        continue;

                    //Render handles
                    if (volume.IsWallStraight(p)) {
                        int wallSections = Mathf.FloorToInt(Vector3.Distance(p0, p1) / volume.minimumWallUnitLength);
                        if (wallSections < 1) wallSections = 1;
                        for (int ws = 0; ws < wallSections + 1; ws++) {
                            float lerp = ws / ((float)wallSections);
                            Vector3 basePos = Vector3.Lerp(p0, p1, lerp);
                            Handles.DrawLine(basePos, basePos + planHeightV);
                        }

                        for (int fl = 0; fl < numberOfFloors + 1; fl++) {
                            //                            Handles.color = fl == 0 || fl == numberOfFloors ? MAIN_LINE_COLOUR : SUB_LINE_COLOUR;
                            float lineHeight = fl * planHeight;
                            Vector3 lineHeightV = lineHeight * Vector3.up;
                            Handles.DrawLine(p0 + lineHeightV, p1 + lineHeightV);
                        }
                    }
                    else {
                        Vector3 cw0 = volume.WorldControlPointA(p);
                        Vector3 cw1 = volume.WorldControlPointB(p);
                        Vector3[] curveWall = new Vector3[10];
                        float arcLength = 0;
                        for (int t = 0; t < 10; t++) {
                            Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / 9f);
                            curveWall[t] = cp;
                            if (t > 0) arcLength += Vector3.Distance(curveWall[t - 1], curveWall[t]);
                        }

                        for (int fl = 0; fl < numberOfFloors + 1; fl++) {
                            //                            Handles.color = fl == 0 || fl == numberOfFloors ? MAIN_LINE_COLOUR : SUB_LINE_COLOUR;
                            float lineHeight = fl * planHeight;
                            Vector3 lineHeightV = lineHeight * Vector3.up;

                            for (int t = 0; t < 9; t++) {
                                Vector3 cwp0 = curveWall[t];
                                Vector3 cwp1 = curveWall[t + 1];
                                Handles.DrawLine(cwp0 + lineHeightV, cwp1 + lineHeightV);
                            }
                            //                            Handles.DrawLine(p0 + lineHeightV, p1 + lineHeightV);
                        }
                    }

                    //                    Handles.color = MAIN_LINE_COLOUR;
                    Handles.DrawLine(p0, p0 + Vector3.up * totalPlanHeight);
                }
            }
        }
    }

    internal class DragDropContent {
        public IBuilding building;
        public IVolume volume = null;
        public int facadeIndex = -1;
        public Facade facade = null;

        public void Reset() {
            volume = null;
            facadeIndex = -1;
            facade = null;
        }
    }
}