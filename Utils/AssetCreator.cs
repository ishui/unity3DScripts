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
using System.IO;

namespace BuildR2
{
    public class AssetCreator
    {
        public static string GeneratePath(string filename, string foldername)
        {
#if UNITY_EDITOR
            string basePath = BuildRSettings.GetSettings().newObjectLocation;
            string folderPath = string.Format("{0}{1}", basePath, foldername);
            string absFolderPath = string.Format("{0}{1}", Application.dataPath, folderPath);
            if (!Directory.Exists(absFolderPath))
                Directory.CreateDirectory(absFolderPath);
            string adbPath = string.Format("{0}/{1}", "Assets", folderPath);
            string output = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(Path.Combine(adbPath, filename));
            return output;
#else
            return "";
#endif
        }
        
        public static string ActiveSelectionPath()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeObject == null) return null;
            string activePath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (Directory.Exists(activePath)) return activePath;

            string[] splitA = activePath.Split('.');
            string[] splitB = activePath.Split('/', '\\');
            if (splitA.Length < 2 || splitB.Length < 2) return null;
            string output = "";
            for (int s = 0; s < splitB.Length - 1; s++)
            {
                output += splitB[s];
                if (s < splitB.Length - 2) output += "/";
            }

            if (Directory.Exists(output)) return output;
            return null;
#else
            return "";
#endif
        }
    }
}