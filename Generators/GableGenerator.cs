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
    public class GableGenerator
    {
        public static void Generate(ref BuildRMesh mesh, Gable design, Vector3 p0, Vector3 p1, float height, float thickness, Vector2 baseUV)
        {
            int gableSectionCount = design.count;
            Vector2 designSize = new Vector2();
            for (int g = 0; g < gableSectionCount; g++)
            {
                designSize += design[g].GetSize();
            }
            Vector2 actualSize = new Vector2(Vector3.Distance(p0, p1), height);
            Vector2 designScale = new Vector2((actualSize.x / 2) / designSize.x, actualSize.y / designSize.y);

            Vector2 basePosition = Vector2.zero;
            Vector3 facadeVector = p1 - p0;
            Vector3 facadeDirection = facadeVector.normalized;
            float facadeWidth = facadeVector.magnitude;
            Vector3 facadeNormal = Vector3.Cross(Vector3.up, facadeDirection);

            Vector4 facadeTangentForward = BuildRMesh.CalculateTangent(facadeDirection);
            Vector4 facadeTangentLeft = BuildRMesh.CalculateTangent(facadeNormal);
            Vector4 facadeTangentRight = BuildRMesh.CalculateTangent(-facadeNormal);
            Vector4 facadeTangentBack = BuildRMesh.CalculateTangent(-facadeDirection);

            Surface surface = design.surface;
            int submesh = mesh.submeshLibrary.SubmeshAdd(surface);//surfaceMapping.IndexOf(surface);
            if (submesh == -1) submesh = 0;
            Vector3 back = -facadeNormal * thickness;
            for (int g = 0; g < gableSectionCount; g++)
            {
                float sectionWidth = design[g].size.x * designScale.x;
                float sectionHeight = design[g].size.y * designScale.y;
                Vector3 g0, g1, g2, g3;

                switch (design[g].type)
                {
                    case GablePart.Types.Vertical:

                        g0 = p0 + facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        g1 = p1 - facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        g2 = g0 + Vector3.up * sectionHeight;
                        g3 = g1 + Vector3.up * sectionHeight;

                        Vector2 uvMax = baseUV + basePosition + new Vector2(facadeWidth - basePosition.x * 2, sectionHeight);
                        mesh.AddPlane(g0, g1, g2, g3, baseUV + basePosition, uvMax, facadeNormal, facadeTangentForward, submesh, surface);
                        Vector2 uvB0 = baseUV + basePosition + new Vector2(0,0);
                        Vector2 uvB1 = baseUV + basePosition + new Vector2(facadeWidth - basePosition.x * 2, sectionHeight);
                        mesh.AddPlane(g1 + back, g0 + back, g3 + back, g2 + back, uvB0, uvB1, -facadeNormal, facadeTangentBack, submesh, surface);

                        var gb0 = g0 + back;
                        var gb1 = g1 + back;
                        var gb2 = g2 + back;
                        var gb3 = g3 + back;
                        Vector2 baseVUV = new Vector2(0, basePosition.y);
                        mesh.AddPlane(gb0, g0, gb2, g2, baseVUV, new Vector2(thickness, basePosition.y + sectionHeight), -facadeDirection, facadeTangentLeft, submesh, surface);
                        mesh.AddPlane(g1, gb1, g3, gb3, baseVUV, new Vector2(thickness, basePosition.y + sectionHeight), facadeDirection, facadeTangentRight, submesh, surface);

                        basePosition.y += sectionHeight;
                        break;

                    case GablePart.Types.Horizonal:

                        g0 = p0 + facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        g1 = p1 - facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        g2 = g0 + facadeDirection * sectionWidth;
                        g3 = g1 - facadeDirection * sectionWidth;

                        Vector4 tangent = BuildRMesh.CalculateTangent(facadeDirection);
                        mesh.AddPlane(g0, g2, g0 + back, g2 + back, Vector3.zero, new Vector2(sectionWidth, thickness), Vector3.up, tangent, submesh, surface);
                        mesh.AddPlane(g3, g1, g3 + back, g1 + back, Vector3.zero, new Vector2(sectionWidth, thickness), Vector3.up, tangent, submesh, surface);

                        basePosition.x += sectionWidth;
                        break;

                    case GablePart.Types.Diagonal:

                        Vector3 gd0 = p0 + facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        Vector3 gd1 = p1 - facadeDirection * basePosition.x + Vector3.up * basePosition.y;
                        Vector3 gd2 = gd0 + facadeDirection * sectionWidth + Vector3.up * sectionHeight;
                        Vector3 gd3 = gd1 - facadeDirection * sectionWidth + Vector3.up * sectionHeight;

                        Vector3 gdb0 = gd0 + back;
                        Vector3 gdb1 = gd1 + back;
                        Vector3 gdb2 = gd2 + back;
                        Vector3 gdb3 = gd3 + back;

                        Vector2 uv0 = baseUV + basePosition;
                        Vector2 uv1 = baseUV + new Vector2(basePosition.x + facadeWidth - basePosition.x * 2, basePosition.y);
                        Vector2 uv2 = baseUV + new Vector2(basePosition.x + sectionWidth, basePosition.y + sectionHeight);
                        Vector2 uv3 = baseUV + new Vector2(basePosition.x + facadeWidth - basePosition.x * 2 - sectionWidth, basePosition.y + sectionHeight);
                        mesh.AddPlaneComplex(gd0, gd1, gd2, gd3, uv0, uv1, uv2, uv3, facadeNormal, facadeTangentForward, submesh, surface);//face
                        mesh.AddPlaneComplex(gdb1, gdb0, gdb3, gdb2, uv0, uv1, uv2, uv3, -facadeNormal, facadeTangentBack, submesh, surface);//face

                        Vector3 leftNorm = Vector3.Cross(-facadeNormal, (gd2 - gd0).normalized);
                        Vector3[] leftNorms = { leftNorm, leftNorm, leftNorm, leftNorm };
                        Vector4 leftTangent = facadeTangentLeft;
                        Vector4[] leftTangents = { leftTangent, leftTangent, leftTangent, leftTangent };

                        Vector3[] leftFace = { gdb0, gd0, gdb2, gd2 };
                        float faceWidth = Vector3.Distance(gd0, gd2);
                        Vector2 sideUV0 = Vector2.zero;
                        Vector2 sideUV1 = surface != null ? surface.CalculateUV(new Vector2(thickness, 0)) : new Vector2(1, 0);
                        Vector2 sideUV2 = surface != null ? surface.CalculateUV(new Vector2(0, faceWidth)) : new Vector2(0, 1);
                        Vector2 sideUV3 = surface != null ? surface.CalculateUV(new Vector2(thickness, faceWidth)) : new Vector2(1, 1);
                        Vector2[] leftFaceUV = { sideUV0, sideUV1, sideUV2, sideUV3 };
                        mesh.AddData(leftFace, leftFaceUV, new[] { 0, 2, 1, 2, 3, 1 }, leftNorms, leftTangents, submesh);

                        Vector3 rightNorm = Vector3.Cross(-facadeNormal, (gd1 - gd3).normalized);
                        Vector3[] rightNorms = { rightNorm, rightNorm, rightNorm, rightNorm };
                        Vector4 rightTangent = facadeTangentRight;
                        Vector4[] rightTangents = { rightTangent, rightTangent, rightTangent, rightTangent };

                        Vector3[] rightFace = { gd1, gdb1, gd3, gdb3 };
                        Vector2[] rightFaceUV = { sideUV0, sideUV1, sideUV2, sideUV3 };//todo
                        mesh.AddData(rightFace, rightFaceUV, new[] { 0, 2, 1, 2, 3, 1 }, rightNorms, rightTangents, submesh);

                        basePosition.x += sectionWidth;
                        basePosition.y += sectionHeight;
                        break;

                    case GablePart.Types.Concave:

                        Arc(ref mesh, design, new Vector3(sectionWidth, sectionHeight, thickness), p0, p1, basePosition, submesh, surface, false, baseUV);

                        basePosition.x += sectionWidth;
                        basePosition.y += sectionHeight;
                        break;

                    case GablePart.Types.Convex:

                        Arc(ref mesh, design, new Vector3(sectionWidth, sectionHeight, thickness), p0, p1, basePosition, submesh, surface, true, baseUV);

                        basePosition.x += sectionWidth;
                        basePosition.y += sectionHeight;
                        break;

                }
            }
        }

        private const float HPI = 1.570796f;//half PI
        private static void Arc(ref BuildRMesh mesh, Gable design, Vector3 sectorSize, Vector3 p0, Vector3 p1, Vector2 basePosition, int submesh, Surface surface, bool convex, Vector2 baseUV)
        {
            Vector3 facadeVector = p1 - p0;
            Vector3 facadeDirection = facadeVector.normalized;
            float facadeWidth = facadeVector.magnitude;
            Vector3 facadeNormal = Vector3.Cross(Vector3.up, facadeDirection);

            Vector4 facadeTangentForward = BuildRMesh.CalculateTangent(facadeDirection);
            Vector4 facadeTangentLeft = BuildRMesh.CalculateTangent(facadeNormal);
            Vector4 facadeTangentRight = BuildRMesh.CalculateTangent(-facadeNormal);
            Vector4 facadeTangentBack = BuildRMesh.CalculateTangent(-facadeDirection);

            float sectionWidth = sectorSize.x;
            float sectionHeight = sectorSize.y;
            float thickness = sectorSize.z;

            var segmentCount = design.segments;
            var vertCount = segmentCount * 8 + 4;
            var verts = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var normals = new Vector3[vertCount];
            var tangents = new Vector4[vertCount];
            int triPart = 24;
            //+ 12 for central section (6 front, 6 back)
            int triCount = (segmentCount - 1) * triPart + 12;
            var triangles = new int[triCount];
            Vector3 back = -facadeNormal * thickness;

            float arcLength = HPI * Mathf.Sqrt(2 * Mathf.Pow(sectorSize.x, 2) + 2 * Mathf.Pow(sectorSize.y, 2)) / 2f;

            //front

            //left
            verts[0] = p0 + facadeDirection * (basePosition.x + sectionWidth) + Vector3.up * basePosition.y;
            Vector2 leftBaseUV = baseUV + new Vector2(basePosition.x + sectionWidth, basePosition.y);
            uvs[0] = surface != null ? surface.CalculateUV(leftBaseUV) : new Vector2(0, 0);
            normals[0] = facadeNormal;
            tangents[0] = facadeTangentForward;
            //right
            verts[1] = p1 - facadeDirection * (basePosition.x + sectionWidth) + Vector3.up * basePosition.y;
            Vector2 rightBaseUV = baseUV + new Vector2(basePosition.x + facadeWidth - basePosition.x * 2 - sectionWidth, basePosition.y);
            uvs[1] = surface != null ? surface.CalculateUV(rightBaseUV) : new Vector2(1, 0);
            normals[1] = facadeNormal;
            tangents[1] = facadeTangentForward;

            //back
            //left
            int endVertIndexLeft = vertCount - 2;
            verts[endVertIndexLeft] = verts[0] + back;
            uvs[endVertIndexLeft] = uvs[1];
            normals[endVertIndexLeft] = -facadeNormal;
            tangents[endVertIndexLeft] = facadeTangentBack;
            //right
            int endVertIndexRight = vertCount - 1;
            verts[endVertIndexRight] = verts[1] + back;
            uvs[endVertIndexRight] = uvs[0];
            normals[endVertIndexRight] = -facadeNormal;
            tangents[endVertIndexRight] = facadeTangentBack;

            for (int i = 0; i < segmentCount; i++)
            {
                float percent = i / (segmentCount - 1f);
                float arcDistance = arcLength * percent;
                float arcPercent = convex ? percent : (1 - percent) + 2;
                float x = Mathf.Sin(arcPercent * HPI);
                float y = Mathf.Cos(arcPercent * HPI);

                if (!convex)
                {
                    x = (x + 1);
                    y = (y + 1);
                }

                Vector3 arcLeft = facadeDirection * (-x * sectionWidth) + Vector3.up * y * sectionHeight;
                Vector3 arcRight = facadeDirection * (x * sectionWidth) + Vector3.up * y * sectionHeight;
                Vector3 vertA = verts[0] + arcLeft;
                Vector3 vertB = vertA + back;
                Vector3 vertC = verts[1] + arcRight;
                Vector3 vertD = vertC + back;

                //left
                verts[i + 2] = vertA;//front
                verts[i + 2 + segmentCount] = vertA;//front top
                verts[i + 2 + segmentCount * 2] = vertB;//back top
                verts[i + 2 + segmentCount * 3] = vertB;//back

                uvs[i + 2] =                    surface != null ? surface.CalculateUV(leftBaseUV + new Vector2(-x * sectionWidth, y * sectionHeight)) : new Vector2(0, 0);
                uvs[i + 2 + segmentCount] =     surface != null ? surface.CalculateUV(new Vector2(thickness, arcDistance)) : new Vector2(1, 0);
                uvs[i + 2 + segmentCount * 2] = surface != null ? surface.CalculateUV(new Vector2(0, arcDistance)) : new Vector2(0, 1);
                uvs[i + 2 + segmentCount * 3] = surface != null ? surface.CalculateUV(rightBaseUV + new Vector2(x * sectionWidth, y * sectionHeight)) : new Vector2(1, 1);


                //right
                verts[i + 2 + segmentCount * 4] = vertC;//front
                verts[i + 2 + segmentCount * 5] = vertC;//front top
                verts[i + 2 + segmentCount * 6] = vertD;//back top
                verts[i + 2 + segmentCount * 7] = vertD;//back

                uvs[i + 2 + segmentCount * 4] = surface != null ? surface.CalculateUV(rightBaseUV + new Vector2(x * sectionWidth, y * sectionHeight)) : new Vector2(0, 0);
                uvs[i + 2 + segmentCount * 5] = surface != null ? surface.CalculateUV(new Vector2(0, arcDistance)) : new Vector2(1, 0);
                uvs[i + 2 + segmentCount * 6] = surface != null ? surface.CalculateUV(new Vector2(thickness, arcDistance)) : new Vector2(0, 1);
                uvs[i + 2 + segmentCount * 7] = surface != null ? surface.CalculateUV(leftBaseUV + new Vector2(-x * sectionWidth, y * sectionHeight)) : new Vector2(1, 1);

                if (i < segmentCount - 1)
                {
                    //left

                    //front
                    triangles[i * triPart] = 0;
                    triangles[i * triPart + 1] = i + 3;
                    triangles[i * triPart + 2] = i + 2;

                    //top
                    triangles[i * triPart + 3] = i + segmentCount + 2;
                    triangles[i * triPart + 4] = i + segmentCount + 3;
                    triangles[i * triPart + 5] = i + segmentCount * 2 + 2;
                    triangles[i * triPart + 6] = i + segmentCount + 3;
                    triangles[i * triPart + 7] = i + segmentCount * 2 + 3;
                    triangles[i * triPart + 8] = i + segmentCount * 2 + 2;

                    //back
                    triangles[i * triPart + 9] = endVertIndexLeft;
                    triangles[i * triPart + 10] = i + 2 + segmentCount * 3;
                    triangles[i * triPart + 11] = i + 3 + segmentCount * 3;

                    //right

                    //front
                    triangles[i * triPart + 12] = 1;
                    triangles[i * triPart + 13] = i + segmentCount * 4 + 2;
                    triangles[i * triPart + 14] = i + segmentCount * 4 + 3;

                    //top
                    triangles[i * triPart + 15] = i + segmentCount * 5 + 3;
                    triangles[i * triPart + 16] = i + segmentCount * 5 + 2;
                    triangles[i * triPart + 17] = i + segmentCount * 6 + 2;
                    triangles[i * triPart + 18] = i + segmentCount * 5 + 3;
                    triangles[i * triPart + 19] = i + segmentCount * 6 + 2;
                    triangles[i * triPart + 20] = i + segmentCount * 6 + 3;

                    //back
                    triangles[i * triPart + 21] = endVertIndexRight;
                    triangles[i * triPart + 22] = i + 3 + segmentCount * 7;
                    triangles[i * triPart + 23] = i + 2 + segmentCount * 7;
                }

                //left
                normals[i + 2] = facadeNormal;
                tangents[i + 2] = facadeTangentForward;

                Vector3 upNormalLeft = Vector3.Slerp(-facadeDirection, Vector3.up, percent);

                normals[i + 2 + segmentCount] = upNormalLeft;
                tangents[i + 2 + segmentCount] = facadeTangentLeft;

                normals[i + 2 + segmentCount * 2] = upNormalLeft;
                tangents[i + 2 + segmentCount * 2] = facadeTangentLeft;

                normals[i + 2 + segmentCount * 3] = -facadeNormal;
                tangents[i + 2 + segmentCount * 3] = facadeTangentBack;

                //right
                normals[i + 2 + segmentCount * 4] = facadeNormal;
                tangents[i + 2 + segmentCount * 4] = facadeTangentForward;

                Vector3 upNormalRight = Vector3.Slerp(facadeDirection, Vector3.up, percent);

                normals[i + 2 + segmentCount * 5] = upNormalRight;
                tangents[i + 2 + segmentCount * 5] = facadeTangentRight;

                normals[i + 2 + segmentCount * 6] = upNormalRight;
                tangents[i + 2 + segmentCount * 6] = facadeTangentRight;

                normals[i + 2 + segmentCount * 7] = -facadeNormal;
                tangents[i + 2 + segmentCount * 7] = facadeTangentBack;
            }

            //inter arc faces
            //front
            triangles[triCount - 12] = 1;
            triangles[triCount - 11] = 0;
            triangles[triCount - 10] = 2;
            triangles[triCount - 9] = 1;
            triangles[triCount - 8] = 2;
            triangles[triCount - 7] = segmentCount * 4 + 2;
            //back
            triangles[triCount - 6] = endVertIndexLeft;
            triangles[triCount - 5] = endVertIndexRight;
            triangles[triCount - 4] = 2 + segmentCount * 3;
            triangles[triCount - 3] = 2 + segmentCount * 3;
            triangles[triCount - 2] = endVertIndexRight;
            triangles[triCount - 1] = segmentCount * 7 + 2;//1;


            mesh.AddData(verts, uvs, triangles, normals, tangents, submesh);
        }
    }
}