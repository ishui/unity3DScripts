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

namespace BuildR2 {
    public class DormerGenerator {
        private static BuildRMesh INTERNAL_B_MESH;
        //TODO support custom models coming in from roof design
        public static void Generate(ref BuildRMesh mesh, IVolume volume, List<Vector3[]> roofFaces) {
            Roof design = volume.roof;
            float roofDepth = design.depth;
            float roofHeight = design.height;

            float dormerWidth = design.dormerWidth;
            float dormerHeight = design.dormerHeight;
            int dormerRows = design.dormerRows;
            if (dormerHeight * dormerRows > roofHeight)
                dormerHeight = roofHeight / dormerRows;
            float dormerRoofHeight = design.dormerRoofHeight;
            float roofPitchRad = Mathf.Atan2(roofHeight, roofDepth);
            float roofHyp = Mathf.Sqrt(roofDepth * roofDepth + roofHeight * roofHeight);//todo make a proper calculation - this is incorrect
            float dormerDepth = Mathf.Cos(roofPitchRad) * dormerHeight;
            float dormerHyp = Mathf.Sqrt(dormerHeight * dormerHeight + dormerDepth * dormerDepth);
            float dormerRowSpace = roofHyp / dormerRows;
            dormerHyp = Mathf.Min(dormerHyp, dormerRowSpace);
            float dormerSpace = dormerRowSpace - dormerHyp;
            float dormerSpaceLerp = dormerSpace / roofHyp;

            if (INTERNAL_B_MESH == null) INTERNAL_B_MESH = new BuildRMesh("internal dormer");
            INTERNAL_B_MESH.Clear();

            INTERNAL_B_MESH.submeshLibrary.AddRange(mesh.submeshLibrary.MATERIALS.ToArray());

            Vector3 bpl = Vector3.left * dormerWidth * 0.5f;
            Vector3 bpr = Vector3.right * dormerWidth * 0.5f;
            Vector3 tpc = Vector3.up * dormerHeight;
            float dormerFaceHeight = dormerHeight - dormerHeight * dormerRoofHeight;
            Vector3 tpl = bpl + Vector3.up * dormerFaceHeight;
            Vector3 tpr = bpr + Vector3.up * dormerFaceHeight;
            Vector3 rpc = tpc + Vector3.back * dormerDepth;
            Vector3 rpl = tpl + Vector3.back * dormerDepth;
            Vector3 rpr = tpr + Vector3.back * dormerDepth;

            Surface mainSurface = design.mainSurface;
            Surface wallSurface = design.wallSurface;
            int mainSubmesh = mesh.submeshLibrary.SubmeshAdd(mainSurface);
            int wallSubmesh = mesh.submeshLibrary.SubmeshAdd(wallSurface);

            Vector2 sectionSize = new Vector2(dormerWidth, dormerFaceHeight);
            if (design.wallSection && design.wallSection.CanRender(sectionSize)) {

                //                mesh.submeshLibrary.Add(design.wallSection);
                mesh.submeshLibrary.Add(design.wallSection);

                GenerationOutput output = GenerationOutput.CreateRawOutput();
                WallSectionGenerator.Generate(design.wallSection, output, sectionSize, false, 0.02f, false, null, mesh.submeshLibrary);
                Vector3 sectionPos = new Vector3(0, dormerFaceHeight * 0.5f, 0);
                int[] mapping = new int[output.raw.materials.Count];
                for(int s = 0; s < output.raw.materials.Count; s++) {
                    mapping[s] = 0;
                }
                INTERNAL_B_MESH.AddDataKeepSubmeshStructure(output.raw, sectionPos, Quaternion.Euler(0, 180, 0), Vector3.one);
            }
            else {
                INTERNAL_B_MESH.AddPlane(bpr, bpl, tpr, tpl, wallSubmesh);//dormer front square
            }

            //front triangle

            INTERNAL_B_MESH.AddTri(tpl, tpr, tpc, Vector3.right, wallSubmesh);
            //roof
            Vector3 normalRoofRight = Vector3.Cross((tpr - tpc).normalized, (rpc - tpc).normalized);
            Vector4 tangentRoofRight = BuildRMesh.CalculateTangent(Vector3.back);
            Vector3 normalRoofLeft = Vector3.Cross((rpc - tpc).normalized, (tpl - tpc).normalized);
            Vector4 tangentRoofLeft = BuildRMesh.CalculateTangent(Vector3.forward);
            Vector2 roofUvMax = new Vector2(dormerDepth, Vector3.Distance(tpc, tpl));
            INTERNAL_B_MESH.AddPlane(rpr, tpr, rpc, tpc, Vector2.zero, roofUvMax, normalRoofRight, tangentRoofRight, mainSubmesh, mainSurface);
            INTERNAL_B_MESH.AddPlane(rpc, tpc, rpl, tpl, Vector2.zero, roofUvMax, normalRoofLeft, tangentRoofLeft, mainSubmesh, mainSurface);
            //side triangles
            INTERNAL_B_MESH.AddTri(bpr, rpr, tpr, Vector3.back, wallSubmesh);
            INTERNAL_B_MESH.AddTri(bpl, tpl, rpl, Vector3.back, wallSubmesh);

            RawMeshData data = RawMeshData.CopyBuildRMesh(INTERNAL_B_MESH);
            
            int roofFaceCount = roofFaces.Count;
            for (int r = 0; r < roofFaceCount; r++) {
                Vector3[] roofFace = roofFaces[r];
                Vector3 p0 = roofFace[0];
                Vector3 p1 = roofFace[1];
                Vector3 p2 = roofFace[2];
                Vector3 p3 = roofFace[3];

                //center line
                Vector3 pDB = Vector3.Lerp(p0, p1, 0.5f);
                Vector3 facadeVector = p1 - p0;
                Vector3 facadeDirection = facadeVector.normalized;
                Vector3 facadeNormal = Vector3.Cross(Vector3.up, facadeDirection);

                Vector3 projTL = p0 + Vector3.Project(p2 - p0, facadeDirection);
                Vector3 projTR = p1 + Vector3.Project(p3 - p1, facadeDirection);

                float sqrMagP0 = Vector3.SqrMagnitude(p0 - pDB);
                float sqrMagP1 = Vector3.SqrMagnitude(p1 - pDB);
                float sqrMagP2 = Vector3.SqrMagnitude(projTL - pDB);
                float sqrMagP3 = Vector3.SqrMagnitude(projTR - pDB);

                Vector3 dormerBaseLeft = sqrMagP0 < sqrMagP2 ? p0 : projTL;
                Vector3 dormerBaseRight = sqrMagP1 < sqrMagP3 ? p1 : projTR;

                Vector3 roofNormal = BuildRMesh.CalculateNormal(p0, p2, p1);
                Vector3 roofUp = Vector3.Cross(roofNormal, -facadeDirection);
                float actualHyp = sqrMagP0 < sqrMagP2 ? Vector3.Distance(p0, p2 + Vector3.Project(p0 - p2, facadeDirection)) : Vector3.Distance(projTL, p2);

                Vector3 dormerTopLeft = dormerBaseLeft + roofUp * actualHyp;
                Vector3 dormerTopRight = dormerBaseRight + roofUp * actualHyp;

                float topLength = Vector3.Distance(dormerBaseLeft, dormerBaseRight);
                int numberOfDormers = Mathf.FloorToInt((topLength - design.minimumDormerSpacing * 2) / (design.minimumDormerSpacing + dormerWidth));

                if(numberOfDormers == 0) {
                    if(topLength > sectionSize.x) numberOfDormers = 1;
                }

                for (int dr = 0; dr < dormerRows; dr++) {
                    float rowPercent = dr / (dormerRows + 0f) + dormerSpaceLerp * 0.5f;
                    //row vector
                    Vector3 rl = Vector3.Lerp(dormerBaseLeft, dormerTopLeft, rowPercent);
                    Vector3 rr = Vector3.Lerp(dormerBaseRight, dormerTopRight, rowPercent);

                    for (int dc = 0; dc < numberOfDormers; dc++) {
                        float columnPercent = (dc + 1f) / (numberOfDormers + 1f);
                        Vector3 dormerBegin = Vector3.Lerp(rl, rr, columnPercent);

                        Quaternion meshRot = Quaternion.LookRotation(facadeNormal, Vector3.up);
                        Vector3 meshPos = dormerBegin;
                        //TODO account for the mesh mode of the wall section - custom meshes
                        mesh.AddDataKeepSubmeshStructure(data, meshPos, meshRot, Vector3.one);
                    }
                }
            }
        }
    }
}