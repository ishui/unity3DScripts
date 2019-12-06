using System.IO;
using System.Text;
using UnityEngine;

namespace BuildR2 {
    public class BuildRAtlasSO : ScriptableObject {

        private const string TEXTURE_FOLDER_PATH = "Internal/Textures/BuildRAtlas/";
        private const string MATERIAL_FOLDER_PATH = "Internal/Materials/BuildRAtlas/";

        public BuildRAtlas atlasData = new BuildRAtlas();

        public void UpdateAtlas() {
#if UNITY_EDITOR
            string textureFilePath = TextureFilePath();
            string materialFilePath = MaterialFilePath();
            atlasData.texture = UnityEditor.AssetDatabase.LoadAssetAtPath(textureFilePath, typeof(Texture2D)) as Texture2D;
            atlasData.material = UnityEditor.AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material)) as Material;
            
            atlasData.UpdateAtlas();

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(atlasData.texture);
            if (!UnityEditor.AssetDatabase.Contains(atlasData.texture)) {
                byte[] bytes = atlasData.texture.EncodeToJPG();
                File.WriteAllBytes(textureFilePath, bytes);
                UnityEditor.AssetDatabase.Refresh();
            }

            UnityEditor.EditorUtility.SetDirty(atlasData.material);
            if (!UnityEditor.AssetDatabase.Contains(atlasData.material))
                UnityEditor.AssetDatabase.CreateAsset(atlasData.material, materialFilePath);

            atlasData.texture = UnityEditor.AssetDatabase.LoadAssetAtPath(textureFilePath, typeof(Texture2D)) as Texture2D;
            atlasData.material = UnityEditor.AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material)) as Material;

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();


            if(atlasData.material != null)
                atlasData.material.mainTexture = atlasData.texture;
            UnityEditor.Selection.activeObject = this;

#endif
        }

#if UNITY_EDITOR
        private string TextureFilePath() {

            BuildRSettings settings = BuildRSettings.GetSettings();
            string settingsLocation = UnityEditor.AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(settingsLocation))
                return "";
            int dirLength = settingsLocation.Length - (settings.name.Length + 6);//+6 for .asset
            string settingsFolder = settingsLocation.Substring(0, dirLength);

            string tempTextureFolder = Concat(settingsFolder, TEXTURE_FOLDER_PATH);

            string dataPath = Application.dataPath;
            int dataPathLength = dataPath.Length;
            string projectPath = Application.dataPath.Substring(0, dataPathLength - 6);//cull assets folder reference
            string fullTempTextureFolder = Concat(projectPath, tempTextureFolder);
            if (!Directory.Exists(fullTempTextureFolder))
                Directory.CreateDirectory(fullTempTextureFolder);
            return Concat(tempTextureFolder, "buildr_atlas_", name, ".jpg");
        }

        private string MaterialFilePath() {

            BuildRSettings settings = BuildRSettings.GetSettings();
            string settingsLocation = UnityEditor.AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(settingsLocation))
                return "";
            int dirLength = settingsLocation.Length - (settings.name.Length + 6);//+6 for .asset
            string settingsFolder = settingsLocation.Substring(0, dirLength);

            string tempTextureFolder = Concat(settingsFolder, MATERIAL_FOLDER_PATH);

            string dataPath = Application.dataPath;
            int dataPathLength = dataPath.Length;
            string projectPath = Application.dataPath.Substring(0, dataPathLength - 6);//cull assets folder reference
            string fullTempTextureFolder = Concat(projectPath, tempTextureFolder);
            if (!Directory.Exists(fullTempTextureFolder))
                Directory.CreateDirectory(fullTempTextureFolder);
            return Concat(tempTextureFolder, "buildr_atlas_", name, ".mat");
        }

        public static StringBuilder SB = new StringBuilder();
        public static string Concat(params string[] items) {
            int length = SB.Length;
            SB.Remove(0, length);
            int itemCount = items.Length;
            for (int i = 0; i < itemCount; i++)
                SB.Append(items[i]);
            return SB.ToString();
        }
#endif

        #region statics
        public static BuildRAtlasSO CreateWallSectionAtlasAsset(string name = null, string directory = null) {
            BuildRAtlasSO buildRAtlas = CreateInstance<BuildRAtlasSO>();
            if (Application.isPlaying) return buildRAtlas;
            if (name != null)
                buildRAtlas.name = name;
#if UNITY_EDITOR
            if (directory == null)
                UnityEditor.AssetDatabase.CreateAsset(buildRAtlas, AssetCreator.GeneratePath("buildrAtlas.asset", "BuildRAtlases"));
            else
                UnityEditor.AssetDatabase.CreateAsset(buildRAtlas, Path.Combine(directory, "buildrAtlas.asset"));
#endif

            return buildRAtlas;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Building Atlas", false, ToolsMenuLevels.CREATE_WALLSECTION)]
        private static BuildRAtlasSO MenuCreateAtlas() {
            BuildRAtlasSO output = CreateWallSectionAtlasAsset();
            UnityEditor.Selection.activeObject = output;
            return output;
        }

        [UnityEditor.MenuItem("Assets/Create/BuildR/Create New Building Atlas", false, ToolsMenuLevels.CREATE_WALLSECTION)]
        private static BuildRAtlasSO MenuCreateAtlasB() {
            string activeFolder = AssetCreator.ActiveSelectionPath();
            BuildRAtlasSO output = CreateWallSectionAtlasAsset(null, activeFolder);
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif

        #endregion
    }
}