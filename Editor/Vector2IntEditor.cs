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
    public class Vector2IntEditor
    {
        public static Vector2Int GUI(Vector2Int val, string text, string xLabel = "x:", string yLabel = "y:")
        {
            float labelWidth = UnityEngine.GUI.skin.label.CalcSize(new GUIContent(text)).x;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(170+ labelWidth));
            EditorGUILayout.LabelField(text, GUILayout.Width(labelWidth));
            GUILayout.Space(10);
            EditorGUILayout.LabelField(xLabel, GUILayout.Width(20));
            val.x = EditorGUILayout.IntField(val.x, GUILayout.Width(50));
            EditorGUILayout.LabelField(yLabel, GUILayout.Width(20));
            val.y = EditorGUILayout.IntField(val.y, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            return val;
        }

        public static Vector2Int VGUI(Vector2Int val, string text, string xLabel = "x:", string yLabel = "y:")
        {
            float labelWidth = UnityEngine.GUI.skin.label.CalcSize(new GUIContent(text)).x;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(170 + labelWidth));
            EditorGUILayout.LabelField(text, GUILayout.Width(labelWidth));
            GUILayout.Space(10);
            EditorGUILayout.LabelField(xLabel, GUILayout.Width(20));
            val.vx = EditorGUILayout.FloatField(val.vx, GUILayout.Width(50));
            EditorGUILayout.LabelField(yLabel, GUILayout.Width(20));
            val.vy = EditorGUILayout.FloatField(val.vy, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            return val;
        }
    }
}