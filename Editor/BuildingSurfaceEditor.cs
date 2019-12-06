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
    public class BuildingSurfaceEditor
    {
        private static Vector2 scrollPos = new Vector2();
        private static int selectedTextureIndex = -1;
        private static Surface selectedSurface = null;
//        private static Vector2 surfLibScroll = new Vector2();

        public static void OnInspectoGUI()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildR2.Surface");
            int libraryCount = guids.Length;

            EditorGUILayout.LabelField("Surface Library");
            EditorGUILayout.BeginHorizontal("Box");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH), GUILayout.Height(200));
            List<GUIContent> toolbar = new List<GUIContent>();
            for(int s = 0; s < libraryCount; s++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[s]);
                Surface surface = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Surface)) as Surface;
                if(surface != null)
                {
                    Texture texture = surface.previewTexture;
                    if(texture != null)
                        toolbar.Add(new GUIContent(texture, surface.name));
                    else
                        toolbar.Add(new GUIContent(surface.name));

                    if (selectedTextureIndex == s)
                        selectedSurface = surface;
//                    GUILayout.Toolbar()
//                    if(GUILayout.Button(surface.previewTexture))
//                        selectedTextureIndex = s;
//                    GUILayout.Label(surface.name, GUILayout.Width(50), GUILayout.Height(50));
                }
            }
            float calWidth = BuildingEditor.MAIN_GUI_WIDTH - 35;
            float calHeight = (libraryCount / 4f) * (calWidth / 4f);
            selectedTextureIndex = GUILayout.SelectionGrid(selectedTextureIndex, toolbar.ToArray(), 4,  GUILayout.Width(calWidth), GUILayout.Height(calHeight));
            
            GUILayout.Space(5);
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            if(selectedSurface != null)
            {
                SurfaceEditor.OnInspectorGUI_S(selectedSurface);
                SurfaceEditor.OnInspectorGUI_SM(selectedSurface);
            }
        }
    }
}