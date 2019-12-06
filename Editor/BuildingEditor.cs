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
using UnityEditor;

namespace BuildR2
{
    [CustomEditor(typeof(Building))]
    public class BuildingEditor : Editor
    {
        public Texture2D volumeIcon;
        public Texture2D floorplanIcon;
        public Texture2D surfacesIcon;
        public Texture2D facadesIcon;
        public Texture2D roofsIcon;
        public Texture2D detailsIcon;
        public Texture2D optionsIcon;
        public Texture2D exportIcon;
        public Texture2D logo;
        public Texture2D warning;

        public static float MAIN_GUI_WIDTH = 400;
        public static Color BLUE = new Color(0.13f, 0.3f, 0.72f);
        public static Color RED = new Color(0.7f, 0.12f, 0.14f, 1);
        private static GUIStyle SCENE_LABEL_STYLE;
        private static GUIStyle FACADE_LABEL_STYLE;

        public static BuildingEditor EDITOR;
        public static Building BUILDING;
        public static Volume VOLUME = null;
        public static Floorplan FLOORPLAN;
        public static Room ROOM = null;
        public static RoomPortal ROOM_PORTAL = null;
        public static VerticalOpening OPENING = null;

        private const int toolbarOptionCount = 7;
        private static string[] TOOLBAR_TEXT = { "external", "internal", "surfaces", "facades", "roof", "export", "options" };
        private static Texture2D[] STAGE_TOOLBAR_TEXTURES;
        public static Texture2D LOGO = null;
        public static Texture2D HEADER_TEXTURE = null;
        private static Vector2 volumeScroll = Vector2.zero;
        public static bool directionalLightIssueDetected = false;

        public static BuildingEditor editor
        {
            get
            {
                return EDITOR;
            }
        }

        public static Building building
        {
            get
            {
                return BUILDING;
            }
        }

        public static Volume volume
        {
            get
            {
                return VOLUME;
            }
            set
            {
                if (value != VOLUME)
                {
                    VOLUME = value;
                    FLOORPLAN = null;
                    ROOM = null;
                    ROOM_PORTAL = null;
                    if (EDITOR != null)
                        EDITOR.UpdateGui();
                }
            }
        }

        public static Floorplan floorplan
        {
            get
            {
                return FLOORPLAN;
            }
            set
            {
                if (value != FLOORPLAN)
                {
                    FLOORPLAN = value;
                    ROOM = null;
                    ROOM_PORTAL = null;
                    if (editor != null)
                        editor.UpdateGui();
                }
            }
        }

        public static Room room
        {
            get
            {
                return ROOM;
            }
            set
            {
                if (value != ROOM)
                {
                    ROOM = value;
                    ROOM_PORTAL = null;
                    OPENING = null;
                    EDITOR.UpdateGui();
                }
            }
        }

        public static RoomPortal roomPortal
        {
            get
            {
                return ROOM_PORTAL;
            }
            set
            {
                if (value != ROOM_PORTAL)
                {
                    ROOM_PORTAL = value;
                    OPENING = null;
                    EDITOR.UpdateGui();
                }
            }
        }

        public static VerticalOpening opening
        {
            get
            {
                return OPENING;
            }
            set
            {
                if (value != OPENING)
                {
                    OPENING = value;
                    ROOM = null;
                    ROOM_PORTAL = null;
                    EDITOR.UpdateGui();
                }
            }
        }

        private void OnEnable()
        {
            BuildRSettings.AUTO_UPDATE = true;
            EditorUtility.ClearProgressBar();
            EDITOR = this;

            if (target != null)
            {
                if (BUILDING != (Building)target)
                {
                    Cleanup();
                    BUILDING = (Building)target;//directly set to avoid gui calls
                }

                if (building.settings == null) building.settings = GetSettings();

                if (building.numberOfVolumes > 0)
                {
                    if (volume == null)
                        VOLUME = (Volume)BUILDING[0];//directly set to avoid gui calls

                    if (volume != null)
                        if (floorplan == null && volume.floors > 0 && building.settings.editMode == BuildREditmodes.Values.Floorplan && volume.InteriorFloorplans().Length > 0)
                            FLOORPLAN = (Floorplan)volume.InteriorFloorplans()[0];//directly set to avoid gui calls

                }
            }

            STAGE_TOOLBAR_TEXTURES = new Texture2D[toolbarOptionCount];
            STAGE_TOOLBAR_TEXTURES[0] = volumeIcon;
            STAGE_TOOLBAR_TEXTURES[1] = floorplanIcon;
            STAGE_TOOLBAR_TEXTURES[2] = surfacesIcon;
            STAGE_TOOLBAR_TEXTURES[3] = facadesIcon;
            STAGE_TOOLBAR_TEXTURES[4] = roofsIcon;
            STAGE_TOOLBAR_TEXTURES[5] = exportIcon;
            STAGE_TOOLBAR_TEXTURES[6] = optionsIcon;

            HEADER_TEXTURE = new Texture2D(1, 1);
            HEADER_TEXTURE.SetPixel(0, 0, RED);
            HEADER_TEXTURE.Apply();

            LOGO = logo;//(Texture2D)AssetDatabase.LoadAssetAtPath("Assets/BuildR2/Internal/EditorImages/buildrLogo.png", typeof(Texture2D));

            BuildingVolumeEditor.OnEnable();

            IVolume[] volumes = BUILDING.AllPlans();
	        foreach(Volume volume1 in volumes)
	        {
				if(volume1 == null) continue;
		        volume1.hideFlags = building.settings.debug ? HideFlags.None : HideFlags.HideInInspector;
	        }

            directionalLightIssueDetected = false;
            Light[] lights = FindObjectsOfType<Light>();
            int lightCount = lights.Length;
            for (int l = 0; l < lightCount; l++)
            {
                Light light = lights[l];
                if (light.type != LightType.Directional) continue;

                if (light.shadowBias > building.settings.recommendedBias || light.shadowNormalBias > building.settings.recommendedNormalBias)
                    directionalLightIssueDetected = true;
            }
        }

        private void Cleanup()
        {
            BUILDING = null;
            VOLUME = null;
            FLOORPLAN = null;
            ROOM = null;
            ROOM_PORTAL = null;
            BuildingVolumeEditor.Cleanup();
            SceneMeshHandler.Clear();
        }

        private void OnSceneGUI()
        {
            bool repaint = false;
            CheckDragDrop();

            if (Event.current.type == EventType.ValidateCommand)
            {
                switch (Event.current.commandName)
                {
                    case "UndoRedoPerformed":
                        if (BUILDING == null)
                            BUILDING = (Building)target;
                        UpdateGui();

                        return;
                }

            }

            if (BUILDING == null)
                BUILDING = (Building)target;

            switch (building.settings.editMode)
            {
                case BuildREditmodes.Values.Volume:
                    BuildingVolumeEditor.OnSceneGUI(BUILDING);
                    if (BuildingVolumeEditor.repaint) repaint = true;
                    BuildingVolumeEditor.repaint = false;
                    break;

                case BuildREditmodes.Values.Floorplan:
                    BuildingFloorplanEditor.OnSceneGUI(BUILDING);
                    if (BuildingFloorplanEditor.repaint) repaint = true;
                    BuildingFloorplanEditor.repaint = false;
                    break;

                case BuildREditmodes.Values.Facades:
                    BuildingFacadeEditor.OnSceneGUI(BUILDING);
                    if (BuildingFacadeEditor.repaint) repaint = true;
                    BuildingFacadeEditor.repaint = false;
                    break;

                case BuildREditmodes.Values.Roofs:
                    BuildingRoofEditor.OnSceneGUI(BUILDING);
                    if (BuildingFacadeEditor.repaint) repaint = true;
                    BuildingFacadeEditor.repaint = false;
                    break;
            }

            if (repaint || building.regenerate) UpdateGui();
        }

        public override void OnInspectorGUI()
        {
	        if(EditorApplication.isPlaying)
	        {
				EditorGUILayout.HelpBox("BuildR editor disabled in play mode", MessageType.Warning);
		        return;
	        }

            bool repaint = false;
            CheckDragDrop();

            EditorGUILayout.BeginHorizontal();
            int currentMode = (int)building.settings.editMode;
            GUIContent[] guiContent = new GUIContent[toolbarOptionCount];
            for (int i = 0; i < toolbarOptionCount; i++)
                guiContent[i] = new GUIContent(STAGE_TOOLBAR_TEXTURES[i], TOOLBAR_TEXT[i]);
            int newStage = GUILayout.Toolbar(currentMode, guiContent, GUILayout.Width(MAIN_GUI_WIDTH), GUILayout.Height(50));
            if (directionalLightIssueDetected)
            {
                Rect toolbarRect = GUILayoutUtility.GetLastRect();
                Rect warningRect = new Rect();
                float warningIconSize = 20;
                warningRect.x = toolbarRect.x + toolbarRect.width - warningIconSize - 4;
                warningRect.y = toolbarRect.y + 4;
                warningRect.width = warningIconSize;
                warningRect.height = warningIconSize;
                GUI.Label(warningRect, warning);
            }
            if (newStage != currentMode)
            {
                building.settings.editMode = (BuildREditmodes.Values)newStage;
                repaint = true;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Regenerate Building", GUILayout.Width(MAIN_GUI_WIDTH)))
            {
                Undo.RegisterCompleteObjectUndo(BUILDING, "Rebuild Building Meshes");
                BUILDING.MarkModified();
                UpdateGui();
            }

            switch (building.settings.editMode)
            {
                case BuildREditmodes.Values.Volume:
                    if (BuildingVolumeEditor.mode == BuildingVolumeEditor.EditModes.SwitchToInterior)
                    {
                        BuildingVolumeEditor.mode = BuildingVolumeEditor.EditModes.FloorplanSelection;
                        BuildingFloorplanEditor.mode = BuildingFloorplanEditor.EditModes.BuildFloorplanInterior;
                        building.settings.editMode = BuildREditmodes.Values.Floorplan;
                        BuildingFloorplanEditor.ToggleEdit(false);
                    }
                    BuildRHeader("Volumes");
                    BuildingVolumeEditor.OnInspectorGUI(BUILDING);
                    if (BuildingVolumeEditor.repaint) repaint = true;
                    BuildingVolumeEditor.repaint = false;
                    break;

                case BuildREditmodes.Values.Floorplan:
                    BuildRHeader("Floorplans");
                    BuildingFloorplanEditor.OnInspectorGUI(BUILDING);
                    if (BuildingFloorplanEditor.repaint) repaint = true;
                    BuildingFloorplanEditor.repaint = false;
                    break;

                case BuildREditmodes.Values.TextureLibrary:
                    BuildRHeader("Surface Library");
                    BuildingSurfaceEditor.OnInspectoGUI();
                    break;

                case BuildREditmodes.Values.Facades:
                    BuildingFacadeEditor.OnInspectorGUI(BUILDING);
                    if (BuildingFacadeEditor.repaint) repaint = true;
                    break;

                case BuildREditmodes.Values.Roofs:
                    BuildRHeader("Roof Designs");
                    BuildingRoofEditor.OnInspectorGUI(BUILDING);
                    if (BuildingRoofEditor.REPAINT) repaint = true;
                    break;

                case BuildREditmodes.Values.Export:
                    BuildRHeader("Export");
                    BuildingExportEditor.OnInspectorGUI(BUILDING);
                    break;

                //                case EditModes.Detailing:
                //                    BuildRHeader("Details");
                //                    EditorGUILayout.LabelField("To Be Implemented");
                //                    break;

                case BuildREditmodes.Values.Options:
                    BuildRHeader("Options");
                    BuildingOptionsEditor.OnInspectorGUI(BUILDING);
                    break;
            }

            if (repaint) UpdateGui();
        }

        public void UpdateGui()
        {
            Repaint();
            HandleUtility.Repaint();
            SceneView.RepaintAll();
            GUI.changed = true;

            switch (building.settings.editMode)
            {
                case BuildREditmodes.Values.Volume:

                    break;

                case BuildREditmodes.Values.Floorplan:
                    if (floorplan != null)
                        floorplan.CheckWallIssues();
                    SceneMeshHandler.BuildFloorplan();
                    break;

                case BuildREditmodes.Values.Facades:
                    break;

                case BuildREditmodes.Values.Roofs:
                    break;
            }
        }

        private void CheckDragDrop()
        {
            EventType eventType = Event.current.type;
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                Object[] objArray = DragAndDrop.objectReferences;
                if (objArray != null)
                {
                    //if dragging a facade design - change mode to facades
                    if (objArray[0].GetType() == typeof(Facade) && building.settings.editMode != BuildREditmodes.Values.Facades)
                    {
                        building.settings.editMode = BuildREditmodes.Values.Facades;
                        UpdateGui();
                    }
                }
            }
        }

        public static void BuildRHeader(string title)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(MAIN_GUI_WIDTH));
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fixedHeight = 60;
            titleStyle.fixedWidth = MAIN_GUI_WIDTH;
            titleStyle.alignment = TextAnchor.UpperLeft;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 18;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(" ", titleStyle);
            Texture2D facadeTexture = new Texture2D(1, 1);
            facadeTexture.SetPixel(0, 0, BLUE);
            facadeTexture.Apply();
            Rect sqrPos = new Rect(0, 0, 0, 0);
            if (Event.current.type == EventType.Repaint)
                sqrPos = GUILayoutUtility.GetLastRect();
            sqrPos.height += 10;
            GUI.DrawTexture(sqrPos, facadeTexture);
            string showTitle = string.Format(" {0}", title);
            EditorGUI.LabelField(sqrPos, showTitle, titleStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(sqrPos.height + 5);
        }

        public static GUIStyle SceneLabel
        {
            get
            {
                if (SCENE_LABEL_STYLE == null)
                {
                    SCENE_LABEL_STYLE = new GUIStyle();
                    Texture2D tempTexture = new Texture2D(1, 1);
                    tempTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.25f));
                    tempTexture.Apply();
                    SCENE_LABEL_STYLE.normal.background = tempTexture;
                    SCENE_LABEL_STYLE.normal.textColor = Color.white;
                    SCENE_LABEL_STYLE.alignment = TextAnchor.MiddleCenter;
                }
                return SCENE_LABEL_STYLE;
            }
        }

        public static GUIStyle FacadeLabel
        {
            get
            {
                if (FACADE_LABEL_STYLE == null)
                {
                    FACADE_LABEL_STYLE = new GUIStyle();
                    FACADE_LABEL_STYLE.normal.textColor = Color.white;
                    FACADE_LABEL_STYLE.fontSize = 10;
                    FACADE_LABEL_STYLE.alignment = TextAnchor.LowerLeft;
                    FACADE_LABEL_STYLE.clipping = TextClipping.Clip;
                    FACADE_LABEL_STYLE.wordWrap = true;
                    FACADE_LABEL_STYLE.border = new RectOffset(2, 2, 2, 2);
                }
                return FACADE_LABEL_STYLE;
            }
        }

        public static void SimpleOrigin(Building building)
        {
            Vector3 origin = building.transform.position;
            Vector3 camFor = Camera.current.transform.forward;
            float alpha = 0.5f;

            Handles.color = new Color(1, 0, 0, alpha);
            Vector3 xDir = Vector3.Dot(camFor, Vector3.right) < 0 ? Vector3.right : Vector3.left;
            Handles.DrawLine(origin, origin + xDir * 10);

            Handles.color = new Color(0, 1, 0, alpha);
            Vector3 yDir = Vector3.Dot(camFor, Vector3.up) < 0 ? Vector3.up : Vector3.down;
            Handles.DrawLine(origin, origin + yDir * 10);

            Handles.color = new Color(0, 0, 1, alpha);
            Vector3 zDir = Vector3.Dot(camFor, Vector3.forward) < 0 ? Vector3.forward : Vector3.back;
            Handles.DrawLine(origin, origin + zDir * 10);
        }

        public static void VolumeSelectorInspectorGUI()
        {
            HUtils.log();
            Debug.Log("这里也能调用得到吗？");
            
            int floorplanCount = building.numberOfPlans;
            if (floorplanCount > 0)
                EditorGUILayout.LabelField("Volumes");
            if (floorplanCount > 10)
                volumeScroll = EditorGUILayout.BeginScrollView(volumeScroll, false, true, GUILayout.Width(MAIN_GUI_WIDTH), GUILayout.Height(150));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(MAIN_GUI_WIDTH));
            string[] floorplanNames = new string[floorplanCount];
            for (int p = 0; p < floorplanCount; p++)
                floorplanNames[p] = building[p].name;
            int currentIndex = building.IndexOf(volume);

            int newIndex = GUILayout.SelectionGrid(currentIndex, floorplanNames, 2);

            EditorGUILayout.EndHorizontal();
            if (newIndex != currentIndex)
                volume = (Volume)building[newIndex];
        }

        public static void GUIDivider()
        {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
        }

        public static int GUI_INT_Control(string content, int value, int min = 0, int max = int.MaxValue)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField(string.Format("{0} : {1}", content, value));

            EditorGUI.BeginDisabledGroup(value <= min);
            if (GUILayout.Button("-"))
                value--;
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(value >= max);
            if (GUILayout.Button("+"))
                value++;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static BuildRSettings GetSettings()
        {
            if (building != null)
                return building.getSettings;
            return BuildRSettings.GetSettings();
        }

	    public static void EnableInteriorGeneration()
	    {
		    if(building.generateInteriors) return;
			
		    Undo.RecordObject(building, "enable interior generation");
		    building.generateInteriors = true;
		}
    }
}