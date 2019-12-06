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
    public class BuildRImporter : AssetPostprocessor
    {
        public void OnPreprocessTexture()
        {
            if (assetPath.Contains("BuildR2/Textures/"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.isReadable = true;
                textureImporter.npotScale = TextureImporterNPOTScale.ToLarger;
            }

            if (assetPath.Contains("BuildR2/Internal/Textures/")) 
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.isReadable = true;
                textureImporter.npotScale = TextureImporterNPOTScale.ToLarger;
            }

            if (assetPath.Contains("BuildR2/Exported/"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.maxTextureSize = 4096;
                textureImporter.filterMode = FilterMode.Trilinear;
            }

            if (assetPath.Contains("BuildR2/Internal/EditorImages"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.textureType = TextureImporterType.GUI;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
            }
        }

        public void OnPreprocessModel()
        {
            //
        }
    }
}

