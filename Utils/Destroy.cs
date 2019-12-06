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
    public class Destroy
    {
        public static void Execute(Object obj)
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }
    }
}