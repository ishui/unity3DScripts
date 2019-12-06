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



using UnityEditor;
using UnityEngine;

namespace BuildR2
{
    public class BuildingOptionsEditor
    {
        private static int mode = 0;
        private static BuildRSettings settings;

        public static void OnInspectorGUI(Building building)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(string.Format("BuildR Version {0}", BuildrVersion.NUMBER));

            BuildingEditor.GUIDivider();

            if (settings == null)
                settings = building.settings;

            Undo.RecordObject(settings, "Settings Modified");
            
            GUIContent[] guiContent = {new GUIContent("Building Settings"), new GUIContent("BuildR Settings")};
            mode = GUILayout.Toolbar(mode, guiContent, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            switch (mode)
            {
                case 0:
                    BuildingSettings(building);
                    break;

                case 1:
                    Editor editor = Editor.CreateEditor(settings);
                    editor.DrawDefaultInspector();
                    break;
            }

            if(BuildingEditor.directionalLightIssueDetected)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Directional camera bias values may cause real-time shadows to render gaps", MessageType.Warning);
                if(GUILayout.Button("Fix"))
                {
                    Light[] lights = GameObject.FindObjectsOfType<Light>();
                    int lightCount = lights.Length;
                    for (int l = 0; l < lightCount; l++)
                    {
                        Light light = lights[l];
                        if (light.type != LightType.Directional) continue;

                        light.shadowBias = building.settings.recommendedBias;
                        light.shadowNormalBias = building.settings.recommendedNormalBias;
                    }
                    BuildingEditor.directionalLightIssueDetected = false;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        public static void BuildingSettings(Building building)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Mesh Detail Level: ", GUILayout.Width(110));
            
//            int selectionIndex = (int)building.meshType;
//            if (selectionIndex == 3) selectionIndex = 2;
//            string[] options = { "none", "box", "full" };
//            int newSelection = EditorGUILayout.Popup(selectionIndex, options);
//            if (newSelection == 2) newSelection = 3;
//            if (newSelection != selectionIndex)
//                building.meshType = (Building.MeshTypes)newSelection;
                        building.meshType = (BuildingMeshTypes)EditorGUILayout.EnumPopup(building.meshType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Collider Detail Level: ", GUILayout.Width(110));
            building.colliderType = (BuildingColliderTypes)EditorGUILayout.EnumPopup(building.colliderType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Generate Exteriors: ", GUILayout.Width(110));
            building.generateExteriors = EditorGUILayout.Toggle(building.generateExteriors);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Generate Interiors: ", GUILayout.Width(110));
            building.generateInteriors = EditorGUILayout.Toggle(building.generateInteriors);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Cull External Doors: ", GUILayout.Width(110));
            building.cullDoors = EditorGUILayout.Toggle(building.cullDoors);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Foundation Depth: ", GUILayout.Width(110));
            building.foundationDepth = EditorGUILayout.DelayedFloatField(building.foundationDepth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Foundation Surface Override: ", GUILayout.Width(110));
            building.foundationSurface = EditorGUILayout.ObjectField(building.foundationSurface, typeof(Surface), false) as Surface;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            EditorGUILayout.LabelField("Show Wireframes: ", GUILayout.Width(110));
            building.showWireframes = EditorGUILayout.Toggle(building.showWireframes);
            EditorGUILayout.EndHorizontal();
        }
    }
}