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

namespace BuildR2
{
    public class BuildRSettings : ScriptableObject
    {
        #region Static

        public static bool AUTO_UPDATE = true;

        #endregion

        #region Floorplans
        [Header("Minimum Room Width")]
        [Header("Floorplans")]//???
        public float minimumRoomWidth = 1f;

        public float newPlanSquareSize = 20f;
        #endregion

        #region Wall Section
        [Header("Opening Width")]
        [Header("Wall Section")]//???
        public float minimumWindowWidth = 0.1f;
        public float maximumWindowWidth = 6.0f;
        public float defaultWindowWidth = 0.8f;
        public float defaultDoorWidth = 1.25f;

        [Header("Opening Height")]
        public float minimumWindowHeight = 0.1f;
        public float maximumWindowHeight = 6.0f;
        public float defaultWindowHeight = 1.5f;
        public float defaultDoorHeight = 2.25f;

        [Header("Opening Depth")]
        public float minimumWindowDepth = 0.05f;
        public float maximumWindowDepth = 1.0f;
        public float defaultWindowDepth = 0.01f;

        #endregion

        #region Gable
        public float previewGableWidth = 2.75f;
        public float previewGableHeight = 2.0f;
        public float previewGableThickness = 0.25f;

        #endregion

        #region Colours
        [Header("COLOURS!")]
        public Color mainLineColour = new Color(1, 1, 1, 0.5f);
        public Color subLineColour = new Color(1, 1, 1, 0.25f);
        public Color invertLineColour = new Color(0, 0, 0.3f, 0.75f);
        public Color highlightedBlueprintColour = new Color(0.2f, 0.6f, 0.6f, 0.5f);
        public Color selectedBlueprintColour = new Color(0, 0, 0.8f, 0.5f);
        public Color unselectedBlueprintColour = new Color(0.1f, 0, 0.4f, 0.2f);
        public Color removeBlueprintColour = new Color(1.0f, 0, 0.4f, 0.2f);
        public Color removeBlueprintHighlightColour = new Color(1.0f, 0, 0.4f, 0.8f);
        public Color removalColour = new Color(1, 0, 0, 0.78f);
        public Color selectedPointColour = new Color(0, 1, 0, 0.78f);
        public Color warningColour = new Color(1, 0.7f, 0, 0.98f);
        public Color errorColour = new Color(1, 0, 0, 0.98f);
        public Color newElementColour = new Color(1, 1, 0.35f, 0.98f);

        public Color roomFloorColour = new Color(0, 0, 0.8f, 0.5f);
        public Color roomWallColour = new Color(0, 0.7f, 0.8f, 0.5f);
        public Color roomWallSelectedColour = new Color(0, 0.9f, 0.2f, 0.6f);

        public Color anchorColour = new Color(1, 1, 0, 0.25f);
        public Color linkedAnchorColour = new Color(1, 0, 0, 0.35f);
        #endregion

        #region Editor Settings
        [Header("EDITOR SETTINGS")]
        public bool snapToGrid = false;
        public bool highlightPerpendicularity = true;
        public Color highlightPerpendicularityColour = new Color(0, 1, 0, 0.6f);
        public Color highlightAngleColour = new Color(1, 0, 0.65f, 0.6f);
        public string pluginLocation = "Assets/BuildR2/";
        public string editorTextureLocation = "Internal/Materials/";
        public string newObjectLocation = "BuildR2/";
        public bool iconPreviews = true;

        [Header("Floorplan Edit Mode Key Bindings")]
        public KeyCode editModeAddRoom = KeyCode.Q;
        public KeyCode editModeAddRoomPoint = KeyCode.W;
        public KeyCode editModeAddDoor = KeyCode.E;
        public KeyCode editModeAddWindow = KeyCode.R;
        public KeyCode editModeAddVerticalSpace = KeyCode.T;
        #endregion

        #region Directional Light Settings

        [Header("Recommended Directional Light Settings")]
        public float recommendedBias = 0.01f;
        public float recommendedNormalBias = 0.001f;

        #endregion

        #region Internal Editor Settings

        [HideInInspector]
        public BuildREditmodes.Values editMode = BuildREditmodes.Values.Volume;
        [HideInInspector]
        public bool showFloorplanShortcutKeys = true;


        #endregion

        #region Other Settings
        public bool debug = false;
        public Texture2D defaultIcon;

//        [HideInInspector]
//        public List<string> customIconDataGUIDs = new List<string>();
//        [HideInInspector]
//        public List<IconUtil.GUIDIconData> customIconData = new List<IconUtil.GUIDIconData>();
//        private Dictionary<string, IconUtil.GUIDIconData> customIconDic = new Dictionary<string, IconUtil.GUIDIconData>();
//
//        public bool CustomIconDataExists(string guid)
//        {
//            return customIconDataGUIDs.Contains(guid);
//        }
//
//        private void CheckCustomIconData()
//        {
//            int guidLength = customIconDataGUIDs.Count;
//            int dataLength = customIconData.Count;
//            if(guidLength != dataLength)
//            {
//                customIconDataGUIDs.Clear();
//                customIconData.Clear();
//                return;
//            }
//            int dicLength = customIconDic.Count;
//            if(guidLength != dicLength)
//            {
//                customIconDic.Clear();
//                for(int g = 0; g < guidLength; g++)
//                    customIconDic.Add(customIconDataGUIDs[g], customIconData[g]);
//            }
//        }
//
//        public IconUtil.GUIDIconData GetCustomIconData(string guid)
//        {
//            CheckCustomIconData();
//            IconUtil.GUIDIconData output = null;
//            if(customIconDic.ContainsKey(guid))
//                output = customIconDic[guid];
//            return output;
//        }
//
//        public void AddCustomIconData(string guid, IconUtil.GUIDIconData data)
//        {
//            customIconDataGUIDs.Add(guid);
//            customIconData.Add(data);
//            customIconDic.Add(guid, data);
//        }
//
//        public void RemoveCustomIconData(string guid)
//        {
//            int index = customIconDataGUIDs.IndexOf(guid);
//            if(index == -1) return;
//            customIconDataGUIDs.RemoveAt(index);
//            customIconData.RemoveAt(index);
//            customIconDic.Remove(guid);
//        }

        //        static Dictionary<string, IconUtil.GUIDIconData> iconDataDic = new Dictionary<string, IconUtil.GUIDIconData>();
        #endregion

        private static string SETTINGS_PATH = "Assets/BuildR2/BuildR 2 Settings.asset";
        private static BuildRSettings INSTANCE = null;
        public static BuildRSettings GetSettings()
        {
            if(INSTANCE != null)
                return INSTANCE;
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BuildR2.BuildRSettings");//t: type BuildRSettings
            int settingsCount = guids.Length;
            if (settingsCount > 1)
            {
                string errorMessage = "Multiple BuildR Settings Files Found - please Delete one";
                for (int i = 0; i < settingsCount; i++)
                    errorMessage += "\n " + (i + 1) + ". " + UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                Debug.LogError(errorMessage);
            }
            if(settingsCount > 0)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                INSTANCE = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(BuildRSettings)) as BuildRSettings;
            }

            if (INSTANCE != null) return INSTANCE;
#endif
            //create new settings file
            INSTANCE = CreateInstance<BuildRSettings>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(INSTANCE, SETTINGS_PATH);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return INSTANCE;
        }
    }
}