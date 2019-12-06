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
    [InitializeOnLoad]
    public class CustomIconPreviews
    {
        private static BuildRSettings settings = null;

        //[DidReloadScripts]
        static CustomIconPreviews()
        {
            //executes on startup
            IconUtil.CleanUpIcons();//clean up old icons that have collected

            EditorApplication.projectWindowItemOnGUI = ItemOnGui;//render out texture image when visible 
        }

        static void ItemOnGui(string guid, Rect rect)
        {
            if (settings == null)
                settings = BuildRSettings.GetSettings();
            if (!settings.iconPreviews)
                return;

            bool defaultFound = settings.defaultIcon != null;
            Rect squareRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            float defaultHeight = Mathf.Min(56, rect.height);
            float defaultWidth = Mathf.Min(44, rect.width, defaultHeight * 0.7857f);
            float defaultX = rect.x;
            float defaultY = rect.y;
            if(rect.height > 56)
            {
                defaultX = rect.x + (rect.width - 44) * 0.5f;
                defaultY = rect.y + (rect.height - 70) * 0.5f;
            }
            Rect defaultRect = new Rect(defaultX, defaultY, defaultWidth, defaultHeight);

//            IconUtil.GUIDIconData iconData = settings.GetCustomIconData(guid);
//            if(iconData == null)
//            {
//                iconData = IconUtil.GenerateGUIDIconData(guid);
//                settings.AddCustomIconData(guid, iconData);
//            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            WallSection wallSection = AssetDatabase.LoadAssetAtPath(assetPath, typeof(WallSection)) as WallSection;

            if (wallSection != null)
            {
                Texture2D wallSectionPreview = wallSection.previewTexture;
                if (wallSectionPreview != null)
                    GUI.DrawTexture(squareRect, wallSectionPreview);
                else if(defaultFound)
                    GUI.DrawTexture(defaultRect, settings.defaultIcon);
            }


            Facade facade = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Facade)) as Facade;
            if (facade != null)
            {
                Texture2D facadePreview = facade.previewTexture;
                if(facadePreview != null)
                    GUI.DrawTexture(squareRect, facadePreview);
                else if (defaultFound)
                    GUI.DrawTexture(defaultRect, settings.defaultIcon);
            }

            Surface surface = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Surface)) as Surface;

            if (surface != null && surface.previewTexture != null)
            {
                if (surface.previewTexture != null)
                    GUI.DrawTexture(squareRect, surface.previewTexture);
                else if (defaultFound)
                    GUI.DrawTexture(defaultRect, settings.defaultIcon);
            }

            Gable gable = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Gable)) as Gable;
            if (defaultFound && gable != null) GUI.DrawTexture(defaultRect, settings.defaultIcon);

            RoomStyle roomStyle = AssetDatabase.LoadAssetAtPath(assetPath, typeof(RoomStyle)) as RoomStyle;
            if (defaultFound && roomStyle != null) GUI.DrawTexture(defaultRect, settings.defaultIcon);

            Portal portal = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Portal)) as Portal;
            if (defaultFound && portal != null) GUI.DrawTexture(defaultRect, settings.defaultIcon);

            BuildRSettings settingsIcon = AssetDatabase.LoadAssetAtPath(assetPath, typeof(BuildRSettings)) as BuildRSettings;
            if (defaultFound && settingsIcon != null) GUI.DrawTexture(defaultRect, settings.defaultIcon);

        }
    }
}