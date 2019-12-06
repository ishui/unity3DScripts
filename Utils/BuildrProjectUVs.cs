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
using System.Collections.Generic;

namespace BuildR2
{
    /// <summary>
    /// Buildr project U vs.
    /// </summary>
    public class BuildrProjectUVs 
    {
        /// <summary>
        /// Project the specified Base UVs to find the appropriate 2D shape from 3D space - mainly used for angled roofs
        /// </summary>
        /// <param name='verts'>
        /// 3 verticies that define the polygon
        /// </param>
        /// <param name='baseUV'>
        /// The 3 source UV coordinates.
        /// </param>
        /// <param name='forward'>
        /// The direction of the facade forward normal
        /// </param>
        public static Vector2[] Project(Vector3[] verts, Vector2 baseUV, Vector3 forward)
        {
            int vertCount = verts.Length;
            Vector2[] uvs = new Vector2[vertCount];
            if(vertCount < 3)
                return null;
            List<Vector3> normals = new List<Vector3>();
            for(int i=2; i<vertCount; i++)
            {
                normals.Add(Vector3.Cross(verts[0]-verts[i],verts[1]-verts[i]));
            }
            int normalCount = normals.Count;
            Vector3 planeNormal = normals[0];
            for(int n=1; n<normalCount; n++)
                planeNormal += normals[1];
            planeNormal /= vertCount;
		
            Quaternion normalToFacFront = Quaternion.FromToRotation(planeNormal, forward);
            planeNormal = normalToFacFront*planeNormal;
            Quaternion normalToFront = Quaternion.FromToRotation(planeNormal, Vector3.forward);
            Quaternion moveFace = normalToFront*normalToFacFront;
            
            uvs[0] = baseUV;
            for(int p=0; p<vertCount; p++)
            {
                Vector3 newRelativePosition = moveFace * (verts[p]-verts[0]);
                uvs[p] = new Vector2(newRelativePosition.x,newRelativePosition.y)+baseUV;
            }
		
            return uvs;
        }


        /// <summary>
        /// Project the specified Base UVs to find the appropriate 2D shape from 3D space - mainly used for angled roofs
        /// </summary>
        public static Vector2[] Project(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 baseUV)
        {
            Vector2[] uvs = new Vector2[3];
            Vector3 normal = BuildRMesh.CalculateNormal(p0, p1, p2);

            Quaternion normalToFaceUp = Quaternion.FromToRotation(normal, Vector3.up);

            Vector3 pC = (p0 + p1 + p2) / 3f;
            p0 = normalToFaceUp * (p0 - pC);
            p1 = normalToFaceUp * (p1 - pC);
            p2 = normalToFaceUp * (p2 - pC);

            uvs[0] = new Vector2(p0.x, p0.z);
            uvs[1] = new Vector2(p1.x, p1.z);
            uvs[2] = new Vector2(p2.x, p2.z);

            float minX = Mathf.Min(uvs[0].x, uvs[1].x, uvs[2].x);
            float minY = Mathf.Min(uvs[0].y, uvs[1].y, uvs[2].y);

            if (minX < 0)
            {
                uvs[0].x += -minX;
                uvs[1].x += -minX;
                uvs[2].x += -minX;
            }
            if (minY < 0)
            {
                uvs[0].y += -minY;
                uvs[1].y += -minY;
                uvs[2].y += -minY;
            }

            return uvs;
        }
    }
}