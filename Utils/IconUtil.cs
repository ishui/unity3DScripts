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
using System.IO;
using System.Text;
using UnityEngine;

namespace BuildR2
{
    public class IconUtil
    {
        private const string IconFolderPath = "Internal/Textures/Icons/";
        private static BuildRSettings settings = null;

        public static Texture2D GetIcon(string iconPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
#else
            return null;
#endif
        }

        public static void SaveIcon(string iconPath, Texture2D texture)
        {
#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToJPG();
            File.WriteAllBytes(iconPath, bytes);
#else
            Debug.LogWarning("Only use this in the Unity Editor");
#endif
        }

        public static void CleanUpIcons()
        {
#if UNITY_EDITOR
            List<string> guids = new List<string>();
            guids.AddRange(UnityEditor.AssetDatabase.FindAssets("t:BuildR2.Facade"));
            guids.AddRange(UnityEditor.AssetDatabase.FindAssets("t:BuildR2.WallSection"));

            GUIDIconData iconData = GenerateGUIDIconData("");
            DirectoryInfo di = new DirectoryInfo(iconData.tempTextureFolder);
            FileInfo[] smFiles = di.GetFiles("*.jpg");

            int iconFolderListLength = smFiles.Length;
            for (int i = 0; i < iconFolderListLength; i++)
            {
                string iconTempImageGuid = Path.GetFileNameWithoutExtension(smFiles[i].Name);
                if (!string.IsNullOrEmpty(iconTempImageGuid))
                {
                    if (guids.Contains(iconTempImageGuid))
                    {
                        guids.Remove(iconTempImageGuid);
                        continue;
                    }
                    if(i >= guids.Count) continue;
                    //else it has no buddie!
                    string guid = guids[i];
                    ICustomIconAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), typeof(UnityEngine.Object)) as ICustomIconAsset;
                    if(asset != null) {
                        string iconPath = asset.customIconPath;
                        if(!string.IsNullOrEmpty(iconPath)) {
                            if (UnityEditor.AssetDatabase.LoadAllAssetsAtPath(iconPath).Length == 0) continue;
                            UnityEditor.AssetDatabase.DeleteAsset(iconPath);
                            asset.customIconPath = "";
                        }
                    }

//                    if(settings == null)
//                        settings = BuildRSettings.GetSettings();
//                    GUIDIconData removeIconData = settings.GetCustomIconData(iconTempImageGuid);//GenerateGUIDIconData(iconTempImageGuid);
//                    if (removeIconData == null) continue;
//                    if (string.IsNullOrEmpty(removeIconData.tempTexturePath)) continue;
//                    if (UnityEditor.AssetDatabase.LoadAllAssetsAtPath(removeIconData.tempTexturePath).Length == 0) continue;
//                    UnityEditor.AssetDatabase.DeleteAsset(removeIconData.tempTexturePath);
//                    settings.RemoveCustomIconData(iconTempImageGuid);
                }

            }
#endif
        }

        public static StringBuilder SB = new StringBuilder();
        public static string Concat(params string[] items)
        {
            int length = SB.Length;
            SB.Remove(0, length);
            int itemCount = items.Length;
            for(int i = 0; i < itemCount; i++)
                SB.Append(items[i]);
            return SB.ToString();
        }

        private static bool TEMP_TEXTURE_FOLDER_EXISTS = false;
        public static GUIDIconData GenerateGUIDIconData(string guid)
        {
            GUIDIconData output = new GUIDIconData();
#if UNITY_EDITOR
            if (settings == null)
                settings = BuildRSettings.GetSettings();
            string settingsLocation = UnityEditor.AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(settingsLocation))
                return output;
            //            string settingsFolder = settingsLocation.Split(new[] { settings.name }, StringSplitOptions.RemoveEmptyEntries)[0];
            int dirLength = settingsLocation.Length - (settings.name.Length + 6);//+6 for .asset
            string settingsFolder = settingsLocation.Substring(0, dirLength);

            string tempTextureFolder = Concat(settingsFolder, IconFolderPath);//Path.Combine(settingsFolder, IconFolderPath);

            if(!TEMP_TEXTURE_FOLDER_EXISTS)
            {
                string dataPath = Application.dataPath;
                int dataPathLength = dataPath.Length;
                string projectPath = Application.dataPath.Substring(0, dataPathLength - 6);//cull assets folder reference
                string fullTempTextureFolder = Concat(projectPath, tempTextureFolder);//Path.Combine(projectPath, tempTextureFolder);
                if (!Directory.Exists(fullTempTextureFolder))
                    Directory.CreateDirectory(fullTempTextureFolder);
                TEMP_TEXTURE_FOLDER_EXISTS = true;
            }
//            string textureFilename = Concat(guid, ".jpg");
            string tempTexturePath = Concat(tempTextureFolder, guid, ".jpg");//Path.Combine(tempTextureFolder, textureFilename);
            bool tempTextureFileExists = File.Exists(tempTexturePath);

            string assetPath = string.IsNullOrEmpty(guid) ? "" : UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            output.assetPath = assetPath;
            output.tempTexturePath = tempTexturePath;
            output.tempTextureFolder = tempTextureFolder;
            output.tempTextureFileExists = tempTextureFileExists;
#endif

            return output;
        }

        [Serializable]
        public class GUIDIconData
        {
            public string assetPath;
            public string tempTextureFolder;
            public string tempTexturePath;
            public bool tempTextureFileExists;
        }
    }
}