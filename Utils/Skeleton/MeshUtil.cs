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

namespace BuildR2.ShapeOffset
{
    public class MeshUtil
    {
        public static void ConvertSkeletonMeshToBuildRMesh(OffsetPolyCore skeleton, ref BuildRMesh mesh, float baseHeight, float height, int submesh)
        {
//            //hipped shape
//            SkeletonTri[] tris = skeleton.data.mesh.GetTriangles();
//            foreach (SkeletonTri tri in tris)
//            {
//                Vector3 p0 = new Vector3(tri[0].position.x, tri[0].height * height + baseHeight, tri[0].position.y);
//                Vector3 p1 = new Vector3(tri[1].position.x, tri[1].height * height + baseHeight, tri[1].position.y);
//                Vector3 p2 = new Vector3(tri[2].position.x, tri[2].height * height + baseHeight, tri[2].position.y);
//                Vector3[] verts = { p0, p1, p2 };
//                Vector3[] norms = { tri.normal, tri.normal, tri.normal };
//                Vector4[] tangents = { tri.tangent, tri.tangent, tri.tangent };
//                mesh.AddData(verts, tri.uvs, new[] { 0, 2, 1 }, norms, tangents, submesh);
//            }
//
//            //top shape
//            Vector2[] topShape = skeleton.data.mesh.topShape().ToArray();
//            int shapeSize = topShape.Length;
//            int[] topTris = EarClipper.Triangulate(topShape);
//            Vector3[] topVerts = new Vector3[shapeSize];
//            Vector2[] topUVs = new Vector2[shapeSize];
//            Vector3[] topNormals = new Vector3[shapeSize];
//            Vector4[] topTangents = new Vector4[shapeSize];
//            Vector4 tangent = DynamicMesh.CalculateTangent(Vector3.right);
//            for (int t = 0; t < shapeSize; t++)
//            {
//                topVerts[t] = new Vector3(topShape[t].x, height, topShape[t].y);
//                topUVs[t] = topShape[t];
//                topNormals[t] = Vector3.up;
//                topTangents[t] = tangent;
//            }
//            mesh.AddData(topVerts, topUVs, topTris, topNormals, topTangents, submesh);
        }
    }
}