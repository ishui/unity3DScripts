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
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace BuildR2
{
    /// <summary>
    /// Uses the MonoBSingleton class for for a dollop of singleton antipattern goodness!
    /// Thi class allows you to add text to your editor scene window without having to write a whole bloody custom inspector
    /// You can also add lines and arrows which I've found helpful so far.
    /// Example:  GizmoLabel.Label("your text here", positon);
    /// </summary>

    [ExecuteInEditMode]
    public class GizmoLabel : MonoBSingleton<GizmoLabel>
    {
        public bool isEnabled = true;

        private readonly List<GizmoLabelItem> _items = new List<GizmoLabelItem>();

        /// <summary>
        /// Add a label to your scene view
        /// </summary>
        /// <param name="text">Text content</param>
        /// <param name="position">World position of text</param>
        /// <param name="frames">Optional time parameter, default is for a frame</param>
        public static void Label(string text, Vector3 position, int frames = 1)
        {
#if UNITY_EDITOR
            GizmoLabelItem input = new GizmoLabelItem();
            input.type = GizmoLabelItem.Types.Label;
            input.text = text;
            input.labelPos = position;
            input.frames = frames;
            Instance.AddItem(input);
#endif
        }

        /// <summary>
        /// Add a label to your scene view with a line originating from the position to a given point
        /// </summary>
        /// <param name="text">Text content</param>
        /// <param name="from">Draw a line from this point</param>
        /// <param name="to">Draw a line to this point and show label here</param>
        /// <param name="lineColour">Line Colour</param>
        /// <param name="frames">Optional time parameter, default is for a frame</param>
        public static void LabelLineTo(string text, Vector3 from, Vector3 to, Color lineColour, int frames = 1)
        {
#if UNITY_EDITOR
            GizmoLabelItem input = new GizmoLabelItem();
            input.type = GizmoLabelItem.Types.LabelLineTo;
            input.text = text;
            input.labelPos = to;
            input.frames = frames;
            input.pointA = from;
            input.pointB = to;
            input.colour = lineColour;
            Instance.AddItem(input);
#endif
        }

        /// <summary>
        /// Add a label to your scene view with an arrow originating from the position in a given direction
        /// </summary>
        /// <param name="text">Text content</param>
        /// <param name="position">World position of text and arrow</param>
        /// <param name="frames">Time parameter, default is for a frame</param>
        /// <param name="to">Direction of the arrow</param>
        /// <param name="lineColour">Colour of the arrow</param>
        public static void LabelDirection(string text, Vector3 position, Vector3 to, Color lineColour, int frames)
        {
#if UNITY_EDITOR
            GizmoLabelItem input = new GizmoLabelItem();
            input.type = GizmoLabelItem.Types.LabelDirection;
            input.text = text;
            input.labelPos = position;
            input.frames = frames;
            input.pointA = to;
            input.colour = lineColour;
            Instance.AddItem(input);
#endif
        }

        protected void AddItem(GizmoLabelItem newItem)
        {
#if UNITY_EDITOR
            if (!isEnabled)
                return;
            _items.Add(newItem);
            SceneView.RepaintAll();
#endif
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!isEnabled)
                return;
            int itemCount = _items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                GizmoLabelItem item = _items[i];
                switch (item.type)
                {
                    case GizmoLabelItem.Types.Label:
                        Handles.color = Color.white;
                        Handles.Label(item.labelPos, item.text);
                        break;
                    case GizmoLabelItem.Types.LabelLineTo:
                        Handles.color = item.colour;
                        Handles.Label(item.labelPos, item.text);
                        Handles.DrawLine(item.pointA, item.pointB);
                        break;
                    case GizmoLabelItem.Types.LabelDirection:
                        Handles.color = item.colour;
                        float handleSize = HandleUtility.GetHandleSize(item.labelPos);
                        Vector3 position = item.labelPos + item.pointA.normalized * handleSize;
                        Handles.Label(position, item.text);
                        UnityVersionWrapper.HandlesArrowCap(0, item.labelPos, Quaternion.LookRotation(item.pointA), handleSize);
                        break;
                }
            }
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!isEnabled)
                return;
            int itemCount = _items.Count;
            int remoeCount = 0;
            for (int i = 0; i < itemCount; i++)
            {
                GizmoLabelItem item = _items[i];

                item.frames--;
                if (item.frames == 0)
                {
                    remoeCount++;
                    _items.Remove(item);
                    itemCount--;
                    i--;
                }
            }
#endif
        }

        [Serializable]
        protected class GizmoLabelItem
        {
            public enum Types
            {
                Label,
                LabelLineTo,
                LabelDirection
            }

            public Types type;
            //lifetime
            public int frames;
            //label
            public string text;
            public Vector3 labelPos;
            //line
            public Vector3 pointA;
            public Vector3 pointB;
            public Color colour;
        }
    }
}

//    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
//    public static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
//    {
//        Handles.Label(transform.position, transform.gameObject.name);
//    }