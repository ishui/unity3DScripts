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

namespace BuildR2
{
    public class UnityVersionWrapper
    {
#if UNITY_EDITOR

        public static void HandlesArrowCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            UnityEditor.Handles.ArrowHandleCap(controlId, position, rotation, size, EventType.Repaint);
#else
            UnityEditor.Handles.ArrowCap(controlId, position, rotation, size);
#endif
        }

        public static void HandlesDotCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            UnityEditor.Handles.DotHandleCap(controlId, position, rotation, size, EventType.Repaint);
#else
            UnityEditor.Handles.DotCap(controlId, position, rotation, size);
#endif
        }
        
        public static void HandlesSphereCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            UnityEditor.Handles.SphereHandleCap(controlId, position, rotation, size, EventType.Repaint);
#else
            UnityEditor.Handles.SphereCap(controlId, position, rotation, size);
#endif
        }
        
        public static bool HandlesDotButton(Vector3 position, Quaternion rotation, float size, float pickSize)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.Button(position, rotation, size, pickSize, UnityEditor.Handles.DotHandleCap);
#else
            return UnityEditor.Handles.Button(position, rotation, size, pickSize, UnityEditor.Handles.DotCap);
#endif
        }

        public static Vector3 HandlesSlider(Vector3 position, Vector3 direction, float size, float snap)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.Slider(position, direction, size, UnityEditor.Handles.ArrowHandleCap, snap);
#else
            return UnityEditor.Handles.Slider(position, direction, size, UnityEditor.Handles.ArrowCap, snap);
#endif
        }

        public static Vector3 HandlesFreeMoveHandle(Vector3 position, Quaternion rotation, float size, Vector3 snap)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.FreeMoveHandle(position, rotation, size, snap, UnityEditor.Handles.DotHandleCap);
#else
            return UnityEditor.Handles.FreeMoveHandle(position, rotation, size, snap, UnityEditor.Handles.DotCap);
#endif
        }


        public static void SetSelectedWireframeHidden(Renderer[] renderers, bool hide)
        {
            int rendLength = renderers.Length;
            for(int i = 0; i < rendLength; i++)
            {
#if UNITY_5_6_OR_NEWER
                UnityEditor.EditorSelectedRenderState state = hide ? UnityEditor.EditorSelectedRenderState.Hidden : UnityEditor.EditorSelectedRenderState.Highlight;
                UnityEditor.EditorUtility.SetSelectedRenderState(renderers[i], state);
#else
                UnityEditor.EditorUtility.SetSelectedWireframeHidden(renderers[i], hide);
#endif
            }
        }


#endif
    }
}