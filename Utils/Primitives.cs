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
using System.Collections;

namespace BuildR2
{
    public class Primitives
    {
        public static Mesh Plane(float size)
        {
            Mesh output = new Mesh();
            output.name = "Generated Plane";

            Vector3 v0 = new Vector3(-size * 0.5f, -size * 0.5f, 0);
            Vector3 v1 = new Vector3(size * 0.5f, -size * 0.5f, 0);
            Vector3 v2 = new Vector3(-size * 0.5f, size * 0.5f, 0);
            Vector3 v3 = new Vector3(size * 0.5f, size * 0.5f, 0);

            output.vertices = new[] { v0, v1, v2, v3 };

            Vector2 uv0 = new Vector2(0, 0);
            Vector2 uv1 = new Vector2(size, 0);
            Vector2 uv2 = new Vector2(0, size);
            Vector2 uv3 = new Vector2(size, size);

            output.uv = new[] { uv0, uv1, uv2, uv3 };

            output.triangles = new[] { 0, 2, 1, 1, 2, 3 };

            output.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            output.RecalculateBounds();

            return output;
        }
    }
}