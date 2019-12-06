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
    [CustomEditor(typeof(Surface))]
    [CanEditMultipleObjects]
    public class SurfaceEditor : Editor
    {
        private static string[] TYPE_STRINGS = { "Material", "Substance" };
        private static float LABEL_WIDTH = 100;

        private Surface _surface;
        private static string nameError = "";

        private void OnEnable()
        {
            _surface = (Surface)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
            OnInspectorGUI_S(_surface);
            OnInspectorGUI_SM(_surface);
            EditorGUILayout.EndVertical();
        }

        public static void OnInspectorGUI_S(Surface surface, string title = "Surface")
        {
            BuildingEditor.BuildRHeader(title);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(120));
            string newName = EditorGUILayout.DelayedTextField(surface.name);
            if (newName != surface.name)
                nameError = BuildingEditorUtils.RenameAsset(surface, newName);
            EditorGUILayout.EndHorizontal();

            if (nameError.Length > 0)
                EditorGUILayout.HelpBox(nameError, MessageType.Error);

            if (!surface.readable)
            {
                string warningContent = "Texture used is not set to readable";
                EditorGUILayout.HelpBox(warningContent, MessageType.Error);

                if (GUILayout.Button("Make surface readable"))
                {
                    string texturePath = AssetDatabase.GetAssetPath(surface.previewTexture);
                    TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (textureImporter != null)
                    {
                        textureImporter.isReadable = true;
                        AssetDatabase.ImportAsset(texturePath);
                        surface.MarkModified();
                    }
                }
            }

            EditorGUILayout.Space();
        }

        public static void OnInspectorGUI_SM(Surface surface)
        {

            int selectedType = (int)surface.surfaceType;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(LABEL_WIDTH));
            int toolbarSelection = GUILayout.Toolbar(selectedType, TYPE_STRINGS);
            if(toolbarSelection != selectedType)
            {
                surface.material = null;
 #if !UNITY_2017_3_OR_NEWER
                surface.substance = null;
#endif
                surface.surfaceType = (Surface.SurfaceTypes)toolbarSelection;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Material", GUILayout.Width(LABEL_WIDTH));
            switch (surface.surfaceType)
            {
                case Surface.SurfaceTypes.Material:

                    Material newMat = EditorGUILayout.ObjectField(surface.material, typeof(Material), false) as Material;
                    if(newMat != surface.material)
                        surface.material = newMat;

                    break;

#if !UNITY_2017_3_OR_NEWER
                case Surface.SurfaceTypes.Substance:

                    ProceduralMaterial newSub = EditorGUILayout.ObjectField(surface.substance, typeof(ProceduralMaterial), false) as ProceduralMaterial;
                    surface.substance = newSub;

                    break;
#endif
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture is Tiled", GUILayout.Width(LABEL_WIDTH));
            bool tiledValue = EditorGUILayout.Toggle(surface.tiled);
            if(tiledValue != surface.tiled)
                surface.tiled = tiledValue;
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(tiledValue);

            int tileX = surface.tiledX;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Repeat X", GUILayout.Width(LABEL_WIDTH));
            EditorGUI.BeginDisabledGroup(tileX <= 1);
            if (GUILayout.Button("-"))
                tileX--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.IntField(tileX);
            if (GUILayout.Button("+"))
                tileX++;
            if(tileX != surface.tiledX)
                surface.tiledX = tileX;

            EditorGUILayout.EndHorizontal();


            int tileY = surface.tiledY;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Repeat Y", GUILayout.Width(LABEL_WIDTH));
            EditorGUI.BeginDisabledGroup(tileY <= 1);
            if (GUILayout.Button("-"))
                tileY--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.IntField(tileY);
            if (GUILayout.Button("+"))
                tileY++;
            if (tileY != surface.tiledY)
                surface.tiledY = tileY;

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            Vector2 textureUnitSize = surface.textureUnitSize;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture Unit Size", GUILayout.Width(LABEL_WIDTH));
            textureUnitSize = EditorGUILayout.Vector2Field("", textureUnitSize);
            if(textureUnitSize != surface.textureUnitSize)
                surface.textureUnitSize = textureUnitSize;
            EditorGUILayout.EndHorizontal();

            if(surface.previewTexture != null)
            GUILayout.Label(surface.previewTexture, GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH), GUILayout.Height(BuildingEditor.MAIN_GUI_WIDTH));
        }
    }
}