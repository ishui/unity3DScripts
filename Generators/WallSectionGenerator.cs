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
using Object = UnityEngine.Object;

namespace BuildR2 {
    public class WallSectionGenerator {
        private const int PIXELS_PER_METER = 100;
        public static BuildRMesh DYNAMIC_MESH = new BuildRMesh("wall section mesh");
        public static BuildRMesh DYNAMIC_MESHB = new BuildRMesh("wall section mesh");
        public static BuildRMesh DYNAMIC_MESHC = new BuildRMesh("wall section mesh");

        private static Mesh TEMP_MESH_0 = null;
        private static Mesh TEMP_MESH_1 = null;

        private static int textureWidth;
        private static int textureHeight;

        public static void Generate(WallSection wallSection, GenerationOutput output, Vector2 size, bool interior = false, float wallThickness = 0, bool cullOpening = false, GenerationOutput colliderMesh = null, SubmeshLibrary submeshLibrary = null, float openingOffset = 0) {
            DYNAMIC_MESH.Clear();

            if (submeshLibrary == null)
                submeshLibrary = DYNAMIC_MESH.submeshLibrary;

            bool canRender = wallSection != null && wallSection.CanRender(size);
            if (interior && wallSection.bayExtruded) canRender = false;//TODO support interior bay window generation
            if (canRender && wallSection.hasOpening) {
                if (!wallSection.bayExtruded)
                    SquareOpening(wallSection, size, interior, wallThickness, cullOpening, submeshLibrary, openingOffset);
                else
                    WallBay(wallSection, size, interior, wallThickness, cullOpening, submeshLibrary, openingOffset);
            }
            else {
                WallSimple(wallSection, size, interior, submeshLibrary);
            }


            if (colliderMesh != null) {
                if (colliderMesh.raw != null) {
                    colliderMesh.raw.Copy(DYNAMIC_MESH);
                }

                if (output.mesh != null) {
                    output.mesh.Clear(false);
                    DYNAMIC_MESH.Build(output.mesh);
                }
            }

            //add details

            if (canRender) {
                if (wallSection.balconyModel != null) {
                    Model balcony = wallSection.balconyModel;
                    Mesh[] meshes = balcony.GetMeshes();
                    if (meshes.Length == 1 && balcony.type == Model.Types.Mesh) {
                        Mesh balconyMesh = meshes[0];
                        int submeshCount = balconyMesh.subMeshCount;
                        Material[] mats = balcony.GetMaterials()[0].materials;
                        int[] submeshList = new int[submeshCount];
                        for (int sm = 0; sm < submeshCount; sm++) {
                            submeshList[sm] = submeshLibrary.SubmeshAdd(mats[sm]);
                        }

                        DYNAMIC_MESH.AddData(balconyMesh, wallSection.BalconyMeshPosition(size, wallThickness), submeshList);
                    }
                }

                if (wallSection.shutterModel != null) {
                    Model shutter = wallSection.shutterModel;
                    Mesh[] meshes = shutter.GetMeshes();
                    if (meshes.Length == 1 && shutter.type == Model.Types.Mesh) {
                        Mesh shutterMesh = meshes[0];
                        int submeshCount = shutterMesh.subMeshCount;
                        Material[] mats = shutter.GetMaterials()[0].materials;
                        int[] submeshList = new int[submeshCount];
                        for (int sm = 0; sm < submeshCount; sm++)
                            submeshList[sm] = submeshLibrary.SubmeshAdd(mats[sm]);

                        DYNAMIC_MESH.AddData(shutterMesh, wallSection.ShutterMeshPositionLeft(size, wallThickness), submeshList, true);
                        DYNAMIC_MESH.AddData(shutterMesh, wallSection.ShutterMeshPositionRight(size, wallThickness), submeshList);
                    }
                }

                if (wallSection.openingModel != null) {
                    Model opening = wallSection.openingModel;
                    Mesh[] meshes = opening.GetMeshes();
                    if (meshes.Length == 1 && opening.type == Model.Types.Mesh) {
                        Mesh opengingMesh = meshes[0];
                        int submeshCount = opengingMesh.subMeshCount;
                        Material[] mats = opening.GetMaterials()[0].materials;
                        int[] submeshList = new int[submeshCount];
                        for (int sm = 0; sm < submeshCount; sm++) {
                            submeshList[sm] = submeshLibrary.SubmeshAdd(mats[sm]);
                        }

                        DYNAMIC_MESH.AddData(opengingMesh, wallSection.OpeningMeshPosition(size, wallThickness), submeshList);
                    }
                }
            }

            if (output.raw != null) {
                output.raw.Copy(DYNAMIC_MESH);
                output.raw.materials = submeshLibrary.MATERIALS;
            }

            if (output.mesh != null) {
                output.mesh.Clear(false);
                DYNAMIC_MESH.Build(output.mesh);
            }
        }

        public static GameObject[] InstantiatePrefabs(GameObject prefabs, WallSection wallSection, Vector2 size, Matrix4x4 matrix, float wallThickness = 0, bool cullOpening = false) {
            bool canRender = wallSection != null && wallSection.CanRender(size);
            List<GameObject> output = new List<GameObject>();
            if (canRender) {
                if (wallSection.balconyModel != null) {
                    Model balcony = wallSection.balconyModel;
                    if (balcony.type == Model.Types.Prefab) {
                        GameObject instance = Object.Instantiate(balcony.subject, prefabs.transform) as GameObject;
                        if (instance != null) {
                            Matrix4x4 balMat = matrix * wallSection.BalconyMeshPosition(size, wallThickness);
                            instance.transform.localPosition = balMat.GetColumn(3);
                            instance.transform.localScale = new Vector3(balMat.GetColumn(0).magnitude, balMat.GetColumn(1).magnitude, balMat.GetColumn(2).magnitude);
                            instance.transform.localRotation = Quaternion.LookRotation(balMat.GetColumn(2), balMat.GetColumn(1));
                            output.Add(instance);
                        }
                    }
                }

                if (wallSection.shutterModel != null) {
                    Model shutter = wallSection.shutterModel;
                    if (shutter.type == Model.Types.Prefab) {
                        GameObject instanceLeft = Object.Instantiate(shutter.subject, prefabs.transform) as GameObject;
                        if (instanceLeft != null) {
                            GameObject instanceRight = Object.Instantiate(instanceLeft, prefabs.transform) as GameObject;

                            Matrix4x4 shtMatLeft = matrix * wallSection.ShutterMeshPositionLeft(size, wallThickness);
                            instanceLeft.transform.localPosition = shtMatLeft.GetColumn(3);
                            instanceLeft.transform.localScale = new Vector3(shtMatLeft.GetColumn(0).magnitude, shtMatLeft.GetColumn(1).magnitude, shtMatLeft.GetColumn(2).magnitude);
                            instanceLeft.transform.localRotation = Quaternion.LookRotation(shtMatLeft.GetColumn(2), shtMatLeft.GetColumn(1));
                            output.Add(instanceLeft);

                            Matrix4x4 shtMatRight = matrix * wallSection.ShutterMeshPositionRight(size, wallThickness);
                            instanceRight.transform.localPosition = shtMatRight.GetColumn(3);
                            instanceRight.transform.localScale = new Vector3(shtMatRight.GetColumn(0).magnitude, shtMatRight.GetColumn(1).magnitude, shtMatRight.GetColumn(2).magnitude);
                            instanceRight.transform.localRotation = Quaternion.LookRotation(shtMatRight.GetColumn(2), shtMatRight.GetColumn(1));
                            output.Add(instanceRight);
                        }
                    }
                }

                if (wallSection.openingModel != null) {
                    Model opening = wallSection.openingModel;
                    if (opening.type == Model.Types.Prefab) {
                        GameObject instance = Object.Instantiate(opening.subject, prefabs.transform) as GameObject;
                        if (instance != null) {
                            Matrix4x4 opnMat = matrix * wallSection.OpeningMeshPosition(size, wallThickness);
                            instance.transform.localPosition = opnMat.GetColumn(3);
                            instance.transform.localScale = new Vector3(opnMat.GetColumn(0).magnitude, opnMat.GetColumn(1).magnitude, opnMat.GetColumn(2).magnitude);
                            instance.transform.localRotation = Quaternion.LookRotation(opnMat.GetColumn(2), opnMat.GetColumn(1));
                            output.Add(instance);
                        }
                    }
                }
            }
            return output.ToArray();
        }

        public static BuildRCollider.BBox[] Generate(WallSection wallSection, Vector2 size, float wallThickness = 0, bool cullOpening = false) {

            bool canRender = wallSection != null && wallSection.CanRender(size) && wallSection.hasOpening;
            if (canRender) {
                float openingWidth = wallSection.openingWidth;
                float openingHeight = wallSection.openingHeight;
                float openingDepth = wallSection.openingDepth;
                float useWallThickness = wallThickness > Mathf.Epsilon ? wallThickness : openingDepth;
                float ceilingThickness = wallThickness;
                if (openingDepth > useWallThickness) openingDepth = useWallThickness * 0.9f;

                if (wallSection.dimensionType == WallSection.DimensionTypes.Relative) {
                    openingWidth = openingWidth * size.x;
                    openingHeight = openingHeight * size.y - wallThickness;
                }

                float sectionWidth = size.x;
                float sectionHeight = size.y;

                float openingWidthRatio = wallSection.openingWidthRatio;
                float openingHeightRatio = wallSection.openingHeightRatio;
                Vector2 openingBase = Vector2.zero;
                float wallLeftWidth = (sectionWidth - openingWidth) * openingWidthRatio;
                float wallRightWidth = size.x - wallLeftWidth - openingWidth;
                float wallBaseHeight = (sectionHeight - ceilingThickness - openingHeight) * openingHeightRatio;
                float wallTopHeight = size.y - openingHeight - wallBaseHeight;
                openingBase.x = wallLeftWidth - sectionWidth * 0.5f;
                openingBase.y = wallBaseHeight - sectionHeight * 0.5f;
                float zPosistion = wallThickness * 0.5f;

                BuildRCollider.BBox[] output = new BuildRCollider.BBox[cullOpening ? 4 : 5];
                Vector3 baseBoxSize = new Vector3(size.x, wallBaseHeight, wallThickness);
                Vector3 baseBoxPosition = new Vector3(0, -size.y * 0.5f + wallBaseHeight * 0.5f, zPosistion);
                output[0] = new BuildRCollider.BBox(baseBoxSize, baseBoxPosition);

                Vector3 leftBoxSize = new Vector3(wallLeftWidth, openingHeight, wallThickness);
                Vector3 leftBoxPosition = new Vector3((size.x - wallLeftWidth) * 0.5f, openingBase.y + openingHeight * 0.5f, zPosistion);
                output[1] = new BuildRCollider.BBox(leftBoxSize, leftBoxPosition);

                Vector3 rightBoxSize = new Vector3(wallRightWidth, openingHeight, wallThickness);
                float xPos2 = (-size.x + wallRightWidth) * 0.5f;
                Vector3 rightBoxPosition = new Vector3(xPos2, openingBase.y + openingHeight * 0.5f, zPosistion);
                output[2] = new BuildRCollider.BBox(rightBoxSize, rightBoxPosition);

                Vector3 topBoxSize = new Vector3(size.x, wallTopHeight, wallThickness);
                Vector3 topBoxPosition = new Vector3(0, size.y * 0.5f - wallTopHeight * 0.5f, zPosistion);
                output[3] = new BuildRCollider.BBox(topBoxSize, topBoxPosition);
                if (!cullOpening) {
                    Vector3 openingBoxSize = new Vector3(openingWidth, openingHeight, 0.05f);
                    float xPos4 = -size.x * 0.5f + wallLeftWidth + openingWidth * 0.5f;
                    float yPos4 = -size.y * 0.5f + wallBaseHeight + openingHeight * 0.5f;
                    Vector3 openingBoxPosition = new Vector3(xPos4, yPos4, openingDepth);
                    output[4] = new BuildRCollider.BBox(openingBoxSize, openingBoxPosition);
                }
                return output;
            }
            else {
                Vector3 sizeV3 = new Vector3(size.x, size.y, wallThickness);
                Vector3 position = new Vector3(0, 0, 0);
                return new[] { new BuildRCollider.BBox(sizeV3, position, Quaternion.identity) };
            }
        }

        private static void WallSimple(WallSection wallSection, Vector2 size, bool interior = false, SubmeshLibrary submeshLibrary = null) {
            float sectionWidth = size.x;
            float sectionHeight = size.y;
            Vector3 blno = new Vector3(-sectionWidth * 0.5f, -sectionHeight * 0.5f, 0);
            Vector3 brno = new Vector3(sectionWidth * 0.5f, -sectionHeight * 0.5f, 0);
            Vector3 tlno = new Vector3(-sectionWidth * 0.5f, sectionHeight * 0.5f, 0);
            Vector3 trno = new Vector3(sectionWidth * 0.5f, sectionHeight * 0.5f, 0);
            Vector3[] verts = { blno, brno, tlno, trno };

            Vector2[] uvs = new Vector2[4];
            Surface useWallSurface = (wallSection != null) ? wallSection.wallSurface : null;
            if (interior && submeshLibrary != null && submeshLibrary.SURFACES.Count > 1)//interior walls need to be modified
                useWallSurface = submeshLibrary.SURFACES[1];

            if (interior) {
                if (submeshLibrary == null) {
                    useWallSurface = null;
                }
                else {
                    if (submeshLibrary.SURFACES.Count == 1)//interior walls need to be modified
                        useWallSurface = submeshLibrary.SURFACES[0];
                    else if (submeshLibrary.SURFACES.Count > 1)
                        useWallSurface = submeshLibrary.SURFACES[1];
                }
            }

            bool tiled = useWallSurface != null ? useWallSurface.tiled : false;
            if (tiled) {
                Vector3 uvOffsetVector = trno;
                uvs[0] = CalculateUV(blno + uvOffsetVector, useWallSurface);
                uvs[1] = CalculateUV(brno + uvOffsetVector, useWallSurface);
                uvs[2] = CalculateUV(tlno + uvOffsetVector, useWallSurface);
                uvs[3] = CalculateUV(trno + uvOffsetVector, useWallSurface);
            }
            else {
                uvs[0] = CalculateUV(new Vector2(0, 0), useWallSurface);
                uvs[1] = CalculateUV(new Vector2(1, 0), useWallSurface);
                uvs[2] = CalculateUV(new Vector2(0, 1), useWallSurface);
                uvs[3] = CalculateUV(new Vector2(1, 1), useWallSurface);
            }
            int[] tris = { 0, 2, 1, 1, 2, 3 };
            Vector3 norm = Vector3.back;
            Vector3[] norms = { norm, norm, norm, norm };
            Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);
            Vector4[] tangents = { tangent, tangent, tangent, tangent };
            int wallSubmesh = 0;
            if (wallSection != null)
                wallSubmesh = Submesh(0, useWallSurface, 0, submeshLibrary);
            DYNAMIC_MESH.AddData(verts, uvs, tris, norms, tangents, wallSubmesh);

        }

        private static void WallBay(WallSection wallSection, Vector2 size, bool interior, float wallThickness, bool cullOpening, SubmeshLibrary submeshLibrary, float offset = 0) {

            float openingWidth = wallSection.openingWidth;
            float bayExtrusion = wallSection.bayExtrusion * size.x;
            float zDirection = interior ? -1 : 1;
            float bayBevel = wallSection.bayBevel * size.x * 0.5f;

            float sectionWidth = size.x;
            float sectionWidthOffset = size.x + Mathf.Abs(offset);
            float sectionHeight = size.y;
            float sectionHeightCut = size.y * 0.99f;

            if (wallSection.dimensionType == WallSection.DimensionTypes.Relative) {
                openingWidth = openingWidth * sectionWidthOffset;
            }

            float openingWidthRatio = wallSection.openingWidthRatio;
            if (interior) openingWidthRatio = 1 - openingWidthRatio;//flip the x for interior
            Vector2 openingBase = Vector2.zero;
            openingBase.x = (sectionWidth - openingWidth) * openingWidthRatio - sectionWidthOffset * 0.5f;
            if (offset > 0) openingBase.x += offset;

            Vector3 bl = new Vector3(-sectionWidth * 0.5f, -sectionHeightCut * 0.5f, 0);
            Vector3 br = new Vector3(sectionWidth * 0.5f, -sectionHeightCut * 0.5f, 0);
            Vector3 el = bl + new Vector3(bayBevel, 0, bayExtrusion * zDirection);
            Vector3 er = br + new Vector3(-bayBevel, 0, bayExtrusion * zDirection);
            Vector3 bm = Vector3.Lerp(bl, br, 0.5f);
            Vector3 extrusionPos = -Vector3.forward * bayExtrusion * zDirection;
            float bevelLength = Vector3.Distance(bl, el);
            Vector2 bevelSize = new Vector2(bevelLength, size.y);
            SquareOpening(wallSection, bevelSize, interior, wallThickness, cullOpening, submeshLibrary);
            if (TEMP_MESH_0 == null) TEMP_MESH_0 = new Mesh();
            DYNAMIC_MESH.submeshLibrary.AddRange(submeshLibrary.MATERIALS.ToArray());
            DYNAMIC_MESH.Build(TEMP_MESH_0);

            Vector3 leftPosition = Vector3.Lerp(bl, el, 0.5f) + Vector3.up * (sectionHeightCut * 0.5f) + extrusionPos;
            Vector3 leftDiff = el - bl;
            float leftAngle = Mathf.Atan2(leftDiff.z, leftDiff.x);
            Quaternion leftQ = Quaternion.Euler(0, leftAngle * Mathf.Rad2Deg, 0);
            Matrix4x4 leftBevelM = Matrix4x4.TRS(leftPosition, leftQ, Vector3.one);

            Vector3 rightPosition = Vector3.Lerp(br, er, 0.5f) + Vector3.up * (sectionHeightCut * 0.5f) + extrusionPos;
            Vector3 rightDiff = br - er;
            float rightAngle = Mathf.Atan2(rightDiff.z, rightDiff.x);
            Quaternion rightQ = Quaternion.Euler(0, rightAngle * Mathf.Rad2Deg, 0);
            Matrix4x4 rightBevelM = Matrix4x4.TRS(rightPosition, rightQ, Vector3.one);

            int[] submeshList = DYNAMIC_MESH.originalSubmeshMapping.ToArray();//new int[TEMP_MESH_0.subMeshCount];//submeshLibrary.MapSubmeshes(DYNAMIC_MESH.materials);
            DYNAMIC_MESH.ClearMeshData();

            Matrix4x4 bevelFrontM = Matrix4x4.identity;
            if (wallSection.bayBevel < 1) {
                bevelLength = Vector3.Distance(el, er);
                bevelSize = new Vector2(bevelLength, size.y);
                SquareOpening(wallSection, bevelSize, interior, wallThickness, cullOpening, submeshLibrary);
                Vector3 frontPosition = Vector3.Lerp(bl, br, 0.5f) + Vector3.up * (sectionHeightCut * 0.5f) + extrusionPos;
                bevelFrontM = Matrix4x4.TRS(frontPosition, Quaternion.identity, Vector3.one);
                if (TEMP_MESH_1 == null) TEMP_MESH_1 = new Mesh();
                DYNAMIC_MESH.Build(TEMP_MESH_1);
            }

            DYNAMIC_MESH.ClearMeshData();
            DYNAMIC_MESH.AddData(TEMP_MESH_0, leftBevelM, submeshList);
            DYNAMIC_MESH.AddData(TEMP_MESH_0, rightBevelM, submeshList);
            if (wallSection.bayBevel < 1)
                DYNAMIC_MESH.AddData(TEMP_MESH_1, bevelFrontM, submeshList);

            Vector4 basicTangent = BuildRMesh.CalculateTangent(Vector3.right);
            Surface useWallSurface = wallSection.wallSurface;
            int wallSubmesh = Submesh(0, useWallSurface, 0, submeshLibrary);

            int[] triSetA = { 0, 3, 1, 1, 3, 4, 1, 4, 2 };
            int[] triSetB = { 0, 1, 3, 1, 4, 3, 1, 2, 4 };

            int submeshSill = Submesh(1, wallSection.sillSurface, wallSubmesh, submeshLibrary);
            int submeshCeiling = Submesh(2, wallSection.ceilingSurface, wallSubmesh, submeshLibrary);

            //			Vector3 bottomV = !interior ? Vector3.zero : Vector3.up *  - wallThickness;
            Vector3[] bottomVs = { bl - extrusionPos, bm - extrusionPos, br - extrusionPos, el - extrusionPos, er - extrusionPos };
            if (!interior) {
                for (int i = 0; i < 5; i++)
                    bottomVs[i].z = bayExtrusion - bottomVs[i].z;
            }
            else
                for (int i = 0; i < 5; i++)
                    bottomVs[i].z = 0 - bottomVs[i].z - bayExtrusion;

            Vector3[] bottomNorms = { Vector3.down, Vector3.down, Vector3.down, Vector3.down, Vector3.down };
            Vector4[] bottomTangents = { basicTangent, basicTangent, basicTangent, basicTangent, basicTangent };
            int bottomSubmesh = interior ? submeshSill : submeshCeiling;

            Vector2[] basicTrapoidUVs = new Vector2[5];
            basicTrapoidUVs[0] = new Vector2(bottomVs[0].x, bottomVs[0].z);
            basicTrapoidUVs[1] = new Vector2(bottomVs[1].x, bottomVs[1].z);
            basicTrapoidUVs[2] = new Vector2(bottomVs[2].x, bottomVs[2].z);
            basicTrapoidUVs[3] = new Vector2(bottomVs[3].x, bottomVs[3].z);
            basicTrapoidUVs[4] = new Vector2(bottomVs[4].x, bottomVs[4].z);

            int[] bottomTriSet = interior ? triSetA : triSetA;
            DYNAMIC_MESH.AddData(bottomVs, basicTrapoidUVs, bottomTriSet, bottomNorms, bottomTangents, bottomSubmesh);

            Vector3 topV = !interior ? Vector3.up * sectionHeight : Vector3.up * (sectionHeight - wallThickness);
            Vector3 topVCut = !interior ? topV * 0.99f : topV;
            Vector3[] topVs = { bl + topVCut - extrusionPos, bm + topVCut - extrusionPos, br + topVCut - extrusionPos, el + topVCut - extrusionPos, er + topVCut - extrusionPos };
            if (!interior) {
                for (int i = 0; i < 5; i++)
                    topVs[i].z = bayExtrusion - topVs[i].z;
            }
            else
                for (int i = 0; i < 5; i++)
                    topVs[i].z = 0 - topVs[i].z - bayExtrusion;
            Vector3[] topNorms = { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            Vector4[] topTangents = { basicTangent, basicTangent, basicTangent, basicTangent, basicTangent };
            int topSubmesh = interior ? submeshCeiling : submeshSill;
            int[] topTriSet = interior ? triSetB : triSetB;
            DYNAMIC_MESH.AddData(topVs, basicTrapoidUVs, topTriSet, topNorms, topTangents, topSubmesh);
            DYNAMIC_MESH.AddPlane(bl + topVCut, br + topVCut, bl + topV, br + topV, Vector3.up, basicTangent, topSubmesh);


            TEMP_MESH_0.Clear(false);
            if (wallSection.bayBevel < 1)
                TEMP_MESH_1.Clear(false);

            TEMP_MESH_0 = null;
            TEMP_MESH_1 = null;
        }

        private static void SquareOpening(WallSection wallSection, Vector2 size, bool interior, float wallThickness, bool cullOpening, SubmeshLibrary submeshLibrary = null, float offset = 0) {
            float openingWidth = wallSection.openingWidth;
            float openingHeight = wallSection.openingHeight;
            float openingDepth = wallSection.openingDepth;
            float useWallThickness = wallThickness > Mathf.Epsilon ? wallThickness : openingDepth;
            float ceilingThickness = interior ? wallThickness : wallThickness;
            if (openingDepth > useWallThickness) openingDepth = useWallThickness * 0.9f;
            if (interior) openingDepth = useWallThickness - openingDepth;

            Surface useWallSurface = wallSection.wallSurface;
            if (interior && submeshLibrary != null && submeshLibrary.SURFACES.Count > 1) useWallSurface = submeshLibrary.SURFACES[1];
            int wallSubmesh = Submesh(0, useWallSurface, 0, submeshLibrary);
            int sillSubmesh = Submesh(1, wallSection.sillSurface, wallSubmesh, submeshLibrary);
            int ceilingSubmesh = Submesh(2, wallSection.ceilingSurface, wallSubmesh, submeshLibrary);
            int openingSubmesh = Submesh(3, wallSection.openingSurface, wallSubmesh, submeshLibrary);

            if (interior) {
                if (submeshLibrary == null || submeshLibrary.SURFACES.Count == 0) {
                    wallSubmesh = 0;
                    openingSubmesh = 0;
                }
                else {
                    if (submeshLibrary.SURFACES.Count == 1) {
                        if (wallSection.openingSurface != null) {
                            wallSubmesh = 0;
                            openingSubmesh = 1;
                        }
                        else {
                            wallSubmesh = 1;
                            openingSubmesh = 0;
                        }
                    }
                    if (submeshLibrary.SURFACES.Count > 1) {
                        wallSubmesh = 1;
                        openingSubmesh = 2;
                    }
                }
            }

            //use these values as the wall section will default to the wall surface if there are no specific surfaces set
            Surface useSillSurface = wallSection.sillSurface != null ? wallSection.sillSurface : useWallSurface;
            Surface useCeilingSurface = wallSection.ceilingSurface != null ? wallSection.ceilingSurface : useWallSurface;
            Surface useOpeningSurface = wallSection.openingSurface != null ? wallSection.openingSurface : useWallSurface;

            float sectionWidth = size.x;
            float sectionWidthOffset = size.x + Mathf.Abs(offset);
            float sectionHeight = size.y;

            if (wallSection.dimensionType == WallSection.DimensionTypes.Relative) {
                openingWidth = openingWidth * sectionWidthOffset;
                openingHeight = openingHeight * size.y - wallThickness;
            }


            float frameSize = wallSection.openingFrame && !interior ? wallSection.openingFrameSize : 0;
            float frameExtrusion = wallSection.openingFrame && !interior ? -wallSection.openingFrameExtrusion : 0;
            Vector3 extusionVector = Vector3.forward * frameExtrusion;

            float openingWidthRatio = wallSection.openingWidthRatio;
            float openingHeightRatio = wallSection.openingHeightRatio;
            if (interior) openingWidthRatio = 1 - openingWidthRatio;//flip the x for interior
            Vector2 openingBase = Vector2.zero;
            openingBase.x = (sectionWidth - openingWidth) * openingWidthRatio - sectionWidthOffset * 0.5f;
            if (offset > 0) openingBase.x += offset;

            openingBase.y = (sectionHeight - ceilingThickness - openingHeight) * openingHeightRatio - sectionHeight * 0.5f;

            //outer wall section verts
            Vector3 bl = new Vector3(-sectionWidth * 0.5f, -sectionHeight * 0.5f, 0);
            Vector3 br = new Vector3(sectionWidth * 0.5f, -sectionHeight * 0.5f, 0);
            Vector3 tl = new Vector3(-sectionWidth * 0.5f, sectionHeight * 0.5f, 0);
            Vector3 tr = new Vector3(sectionWidth * 0.5f, sectionHeight * 0.5f, 0);
            //internal verts for top and bottom adjacent to opening
            Vector3 ibl = new Vector3(openingBase.x, -sectionHeight * 0.5f, 0) + new Vector3(-frameSize, 0, 0);
            Vector3 ibr = new Vector3(openingBase.x + openingWidth, -sectionHeight * 0.5f, 0) + new Vector3(frameSize, 0, 0);
            Vector3 itl = new Vector3(openingBase.x, sectionHeight * 0.5f, 0) + new Vector3(-frameSize, 0, 0);
            Vector3 itr = new Vector3(openingBase.x + openingWidth, sectionHeight * 0.5f, 0) + new Vector3(frameSize, 0, 0);
            //opening verts
            Vector3 obl = new Vector3(openingBase.x, openingBase.y, 0);
            Vector3 obr = new Vector3(openingBase.x + openingWidth, openingBase.y, 0);
            Vector3 otl = new Vector3(openingBase.x, openingBase.y + openingHeight, 0);
            Vector3 otr = new Vector3(openingBase.x + openingWidth, openingBase.y + openingHeight, 0);
            //opening verts with frame offset
            Vector3 oblf = obl + new Vector3(openingWidthRatio > 0 ? -frameSize : 0, openingHeightRatio > 0 ? -frameSize : 0, 0);
            Vector3 obrf = obr + new Vector3(openingWidthRatio < 1 ? frameSize : 0, openingHeightRatio > 0 ? -frameSize : 0, 0);
            Vector3 otlf = otl + new Vector3(openingWidthRatio > 0 ? -frameSize : 0, openingHeightRatio < 1 ? frameSize : 0, 0);
            Vector3 otrf = otr + new Vector3(openingWidthRatio < 1 ? frameSize : 0, openingHeightRatio < 1 ? frameSize : 0, 0);

            Vector3 od = Vector3.forward * openingDepth;
            Vector3 wd = Vector3.forward * useWallThickness;

            Vector3 wallNormal = Vector3.back;//interior ? Vector3.left : Vector3.back;
            Vector4 wallTangent = BuildRMesh.CalculateTangent(Vector3.right);
            Vector4 leftFacingTangent = BuildRMesh.CalculateTangent(Vector3.forward);
            Vector4 rightFacingTangent = BuildRMesh.CalculateTangent(Vector3.back);

            if (interior)//pull top down for interior wall
            {
                tl.y += -wallThickness;
                tr.y += -wallThickness;
                itl.y += -wallThickness;
                itr.y += -wallThickness;
            }


            Vector2 uvOffset = new Vector2(tr.x, tr.y);
            bool isTiled = (useWallSurface != null) ? useWallSurface.tiled : true;
            if (isTiled) {
                //left face
                AddPlaneModified(bl, ibl, tl, itl, useWallSurface, uvOffset, wallNormal, wallTangent, wallSubmesh);
                //right face
                AddPlaneModified(ibr, br, itr, tr, useWallSurface, uvOffset, wallNormal, wallTangent, wallSubmesh);
                //under opening face
                AddPlaneModified(ibl, ibr, oblf, obrf, useWallSurface, uvOffset, wallNormal, wallTangent, wallSubmesh);
                //above opening face
                AddPlaneModified(otlf, otrf, itl, itr, useWallSurface, uvOffset, wallNormal, wallTangent, wallSubmesh);
            }
            else {
                //left face
                Vector2 bluv = useWallSurface.CalculateUV(new Vector2(0, 0));
                Vector2 itluv = useWallSurface.CalculateUV(new Vector2((openingBase.x + sectionWidthOffset * 0.5f - frameSize) / sectionWidth, 1));
                DYNAMIC_MESH.AddPlaneNoUVCalc(bl, ibl, tl, itl, bluv, itluv, wallNormal, wallTangent, wallSubmesh, useWallSurface);

                //right face
                Vector2 ibruv = useWallSurface.CalculateUV(new Vector2((openingBase.x + openingWidth + sectionWidthOffset * 0.5f + frameSize) / sectionWidth, 0));
                Vector2 truv = useWallSurface.CalculateUV(new Vector2(1, 1));
                DYNAMIC_MESH.AddPlaneNoUVCalc(ibr, br, itr, tr, ibruv, truv, wallNormal, wallTangent, wallSubmesh, useWallSurface);

                //under opening face
                Vector2 ibluv = useWallSurface.CalculateUV(new Vector2((openingBase.x + sectionWidthOffset * 0.5f - frameSize) / sectionWidth, 0));
                Vector2 obruv = useWallSurface.CalculateUV(new Vector2(ibruv.x, (openingBase.y + sectionHeight * 0.5f - frameSize) / sectionHeight));
                DYNAMIC_MESH.AddPlaneNoUVCalc(ibl, ibr, oblf, obrf, ibluv, obruv, wallNormal, wallTangent, wallSubmesh, useWallSurface);

                //above opening face
                Vector2 otluv = useWallSurface.CalculateUV(new Vector2(itluv.x, (openingBase.y + openingHeight + sectionHeight * 0.5f + frameSize) / sectionHeight));
                Vector2 itruv = useWallSurface.CalculateUV(new Vector2(obruv.x, 1));
                DYNAMIC_MESH.AddPlaneNoUVCalc(otlf, otrf, itl, itr, otluv, itruv, wallNormal, wallTangent, wallSubmesh, useWallSurface);

            }

            if (!interior) {
                //opening sill
                Vector2 endUv = new Vector2(openingWidth, useWallThickness);
                AddPlane(obl + extusionVector, obr + extusionVector, obl + wd, obr + wd, useSillSurface, Vector2.zero, endUv, Vector3.up, wallTangent, sillSubmesh);

                if (wallSection.extrudedSill) {
                    Vector3 sillDimensions = wallSection.extrudedSillDimentions;

                    Vector3 right = sillDimensions.x * Vector3.right;
                    Vector3 drop = sillDimensions.y * Vector3.down;
                    Vector3 extrusion = sillDimensions.z * -Vector3.forward;
                    Vector2 extrusionFaceStartUV = new Vector2(-sillDimensions.x, sillDimensions.y);
                    Vector2 extrusionFaceEndUV = new Vector2(openingWidth + sillDimensions.x, 0);

                    Vector2 extrusionTopStartUV = new Vector2(-sillDimensions.x, -sillDimensions.z);
                    Vector2 extrusionTopEndUV = new Vector2(openingWidth + sillDimensions.x, 0);

                    Vector2 extrusionSideEndUV = new Vector2(sillDimensions.z, sillDimensions.y);

                    Vector3 sl = obl - right;
                    Vector3 sr = obr + right;
                    Vector3 sdl = obl - right + drop;
                    Vector3 sdr = obr + right + drop;
                    Vector3 esl = sl + extrusion;
                    Vector3 esr = sr + extrusion;
                    Vector3 esdl = sdl + extrusion;
                    Vector3 esdr = sdr + extrusion;

                    //left
                    AddPlane(sdl, esdl, sl, esl, useSillSurface, Vector2.zero, extrusionSideEndUV, Vector3.left, leftFacingTangent, sillSubmesh);
                    //right
                    AddPlane(esdr, sdr, esr, sr, useSillSurface, Vector2.zero, extrusionSideEndUV, Vector3.right, rightFacingTangent, sillSubmesh);
                    //top
                    AddPlane(esl, esr, sl, sr, useSillSurface, extrusionTopStartUV, extrusionTopEndUV, Vector3.up, wallTangent, sillSubmesh);
                    //bottom
                    AddPlane(sdl, sdr, esdl, esdr, useSillSurface, extrusionTopStartUV, extrusionTopEndUV, Vector3.down, wallTangent, sillSubmesh);
                    //face
                    AddPlane(esr, esl, esdr, esdl, useSillSurface, extrusionFaceStartUV, extrusionFaceEndUV, Vector3.back, wallTangent, sillSubmesh);
                }


                //opening ceiling
                if (!wallSection.isArched) {
                    AddPlane(otl + wd, otr + wd, otl + extusionVector, otr + extusionVector, useCeilingSurface, Vector2.zero, endUv, Vector3.down, wallTangent, ceilingSubmesh);


                    if (wallSection.extrudedLintel) {
                        Vector3 lintelDimentions = wallSection.extrudedLintelDimentions;

                        Vector3 right = lintelDimentions.x * Vector3.right;
                        Vector3 up = lintelDimentions.y * Vector3.up;
                        Vector3 extrusion = lintelDimentions.z * -Vector3.forward;

                        Vector2 extrusionFaceStartUV = new Vector2(-lintelDimentions.x, lintelDimentions.y);
                        Vector2 extrusionFaceEndUV = new Vector2(openingWidth + lintelDimentions.x, 0);

                        Vector2 extrusionTopStartUV = new Vector2(-lintelDimentions.x, 0);
                        Vector2 extrusionTopEndUV = new Vector2(openingWidth + lintelDimentions.x, -lintelDimentions.z);

                        Vector2 extrusionSideEndUV = new Vector2(lintelDimentions.z, lintelDimentions.y);

                        Vector3 sl = otl - right;
                        Vector3 sr = otr + right;
                        Vector3 sul = otl - right + up;
                        Vector3 sur = otr + right + up;
                        Vector3 esl = sl + extrusion;
                        Vector3 esr = sr + extrusion;
                        Vector3 esul = sul + extrusion;
                        Vector3 esur = sur + extrusion;
                        //left
                        AddPlane(sl, esl, sul, esul, useCeilingSurface, Vector2.zero, extrusionSideEndUV, Vector3.left, leftFacingTangent, ceilingSubmesh);
                        //right
                        AddPlane(esr, sr, esur, sur, useCeilingSurface, Vector2.zero, extrusionSideEndUV, Vector3.right, rightFacingTangent, ceilingSubmesh);
                        //top
                        AddPlane(esul, esur, sul, sur, useCeilingSurface, extrusionTopStartUV, extrusionTopEndUV, Vector3.up, wallTangent, ceilingSubmesh);
                        //bottom
                        AddPlane(sl, sr, esl, esr, useCeilingSurface, extrusionTopStartUV, extrusionTopEndUV, Vector3.down, wallTangent, ceilingSubmesh);
                        //face
                        AddPlane(esl, esr, esul, esur, useCeilingSurface, extrusionFaceStartUV, extrusionFaceEndUV, Vector3.back, wallTangent, ceilingSubmesh);
                    }
                }

                int openingWallSubmesh = wallSection.openingFrame ? sillSubmesh : wallSubmesh;

                //opening left wall
                Vector2 sideStartUv = new Vector2(obl.x, obl.y) + uvOffset;
                Vector2 sideEndUv = new Vector2(obl.x + useWallThickness, obl.y + openingHeight) + uvOffset;
                Vector4 olTangent = BuildRMesh.CalculateTangent(Vector3.forward);
                if (wallSection.openingFrame)
                    AddPlane(otl + extusionVector, obl + extusionVector, otl + wd, obl + wd, useWallSurface, sideStartUv, sideEndUv, Vector3.right, olTangent, openingWallSubmesh);
                else
                    AddPlane(obl + extusionVector, obl + wd, otl + extusionVector, otl + wd, useWallSurface, sideStartUv, sideEndUv, Vector3.right, olTangent, openingWallSubmesh);
                //opening right wall
                sideStartUv.x = obr.x - useWallThickness * 2;
                sideEndUv.x = obr.x - useWallThickness;
                Vector4 orTangent = BuildRMesh.CalculateTangent(Vector3.back);
                if (wallSection.openingFrame)
                    AddPlane(otr + wd, obr + wd, otr + extusionVector, obr + extusionVector, useWallSurface, sideStartUv, sideEndUv, Vector3.left, orTangent, openingWallSubmesh);
                else
                    AddPlane(obr + wd, obr + extusionVector, otr + wd, otr + extusionVector, useWallSurface, sideStartUv, sideEndUv, Vector3.left, orTangent, openingWallSubmesh);

                //Opening Frame
                if (wallSection.openingFrame) {
                    bool extruded = Mathf.Abs(frameExtrusion) > Mathf.Epsilon;
                    if (openingWidthRatio > 0) {
                        Vector3[] leftFrameVerts = { otlf + extusionVector, oblf + extusionVector, otl + extusionVector, obl + extusionVector };
                        Vector2[] leftFrameUVs = ConvertToUvs(leftFrameVerts, true);
                        DYNAMIC_MESH.AddPlaneComplex(leftFrameVerts, leftFrameUVs, wallNormal, wallTangent, sillSubmesh, useSillSurface);
                        if (extruded) {
                            Vector3[] frameExtVerts = { leftFrameVerts[0] - extusionVector, leftFrameVerts[1] - extusionVector, leftFrameVerts[0], leftFrameVerts[1] };
                            Vector2[] frameExtUVs = ConvertToUvs(frameExtVerts, true);
                            DYNAMIC_MESH.AddPlaneComplex(frameExtVerts, frameExtUVs, Vector3.left, leftFacingTangent, sillSubmesh, useSillSurface);
                        }
                    }

                    if (openingHeightRatio < 1) {
                        Vector3[] topFrameVerts = { otlf + extusionVector, otl + extusionVector, otrf + extusionVector, otr + extusionVector };
                        Vector2[] topFrameUVs = ConvertToUvs(topFrameVerts);
                        DYNAMIC_MESH.AddPlaneComplex(topFrameVerts, topFrameUVs, wallNormal, wallTangent, sillSubmesh, useSillSurface);
                        if (extruded) {
                            Vector3[] frameExtVerts = { topFrameVerts[0], topFrameVerts[2], topFrameVerts[0] - extusionVector, topFrameVerts[2] - extusionVector };
                            Vector2[] frameExtUVs = ConvertToUvs(frameExtVerts);
                            DYNAMIC_MESH.AddPlaneComplex(frameExtVerts, frameExtUVs, Vector3.up, wallTangent, sillSubmesh, useSillSurface);
                        }
                    }


                    if (openingWidthRatio < 1) {
                        Vector3[] rightFrameVerts = { obr + extusionVector, obrf + extusionVector, otr + extusionVector, otrf + extusionVector };
                        Vector2[] rightFrameUVs = ConvertToUvs(rightFrameVerts, true);
                        DYNAMIC_MESH.AddPlaneComplex(rightFrameVerts, rightFrameUVs, wallNormal, wallTangent, sillSubmesh, useSillSurface);
                        if (extruded) {
                            Vector3[] frameExtVerts = { rightFrameVerts[1] - extusionVector, rightFrameVerts[3] - extusionVector, rightFrameVerts[1], rightFrameVerts[3] };
                            Vector2[] frameExtUVs = ConvertToUvs(frameExtVerts, true);
                            DYNAMIC_MESH.AddPlaneComplex(frameExtVerts, frameExtUVs, Vector3.right, rightFacingTangent, sillSubmesh, useSillSurface);
                        }
                    }

                    if (openingHeightRatio > 0) {
                        Vector3[] bottomFrameVerts = { oblf + extusionVector, obrf + extusionVector, obl + extusionVector, obr + extusionVector };
                        Vector2[] bottomFrameUVs = ConvertToUvs(bottomFrameVerts, false);
                        DYNAMIC_MESH.AddPlaneComplex(bottomFrameVerts, bottomFrameUVs, wallNormal, wallTangent, sillSubmesh, useSillSurface);
                        if (extruded) {
                            Vector3[] frameExtVerts = { bottomFrameVerts[1], bottomFrameVerts[0], bottomFrameVerts[1] - extusionVector, bottomFrameVerts[0] - extusionVector };
                            Vector2[] frameExtUVs = ConvertToUvs(frameExtVerts);
                            DYNAMIC_MESH.AddPlaneComplex(frameExtVerts, frameExtUVs, Vector3.down, olTangent, sillSubmesh, useSillSurface);
                        }
                    }
                }
            }
            //opening
            //			if(interior)
            //				Debug.Log(wallSection.name + " "+ wallSection.openingModel+" "+ wallSection.portal);
            if (wallSection.portal == null && wallSection.openingModel == null && !cullOpening) {
                //				if (interior)
                //					Debug.Log(wallSection.name);
                Vector2 endUv = (useOpeningSurface != null && !useOpeningSurface.tiled) ? Vector2.one : new Vector2(openingWidth, openingHeight);
                if (interior) {
                    Vector3 iod = Vector3.forward * Mathf.Min(useWallThickness - openingDepth, 0.2f);
                    AddPlane(obl + iod, obr + iod, otl + iod, otr + iod, useOpeningSurface, Vector3.zero, endUv, wallNormal, wallTangent, openingSubmesh);
                }
                else
                    AddPlane(obl + od, obr + od, otl + od, otr + od, useOpeningSurface, Vector3.zero, endUv, wallNormal, wallTangent, openingSubmesh);
            }
            else if (!interior) {
                Vector2 openingSize = new Vector2(openingWidth, openingHeight);
                Vector3 openingOffset = new Vector3(-obl.x - openingSize.x * 0.5f, obl.y + openingSize.y * 0.5f, -openingDepth);
                //                Vector2 openingOffset = new Vector2(obl.x + sectionWidth * 0.5f, obl.y + sectionHeight * 0.5f) + openingSize * 0.5f;
                //                Vector2 openingOffset = new Vector2(obl.x + sectionWidth * 0.5f, obl.y + sectionHeight * 0.5f) + openingSize * 0.5f;
                if (wallSection.portal != null) {
                    Portal portal = wallSection.portal;
                    PortalGenerator.Portal(ref DYNAMIC_MESH, portal, openingSize, openingOffset, true, submeshLibrary);
                }
                else { }
            }

            if (wallSection.isArched) {
                float archHeight = wallSection.archHeight;
                float baseHeight = openingHeight - archHeight;
                float archCurve = wallSection.archCurve;
                int archSegments = wallSection.archSegments;
                float archWidth = openingWidth * 0.5f;
                float openingCenter = openingBase.x + openingWidth * 0.5f;
                Vector4 acaTan = BuildRMesh.CalculateTangent(Vector3.forward);//Arch Tangent Ceiling Section Left
                Vector4 acbTan = BuildRMesh.CalculateTangent(Vector3.back);//Arch Tangent Ceiling Section Right

                Vector3 ac = new Vector3(openingCenter, openingBase.y + baseHeight, 0);//center
                Vector3 al = new Vector3(openingBase.x, openingBase.y + baseHeight, 0);
                Vector3 at = new Vector3(openingCenter, openingBase.y + openingHeight, 0);
                Vector3 ar = new Vector3(openingBase.x + openingWidth, openingBase.y + baseHeight, 0);

                int archVertCount = (archSegments + 1) * 2 + 2;//arch segments repeat the top one and add two more for the corners
                int archTriCount = archSegments * 6;
                Vector3[] av = new Vector3[archVertCount];
                Vector2[] auv = new Vector2[archVertCount];
                int[] atri = new int[archTriCount];
                Vector3[] anrm = new Vector3[archVertCount];
                Vector4[] atan = new Vector4[archVertCount];

                //ceiling
                int archCeilVertCount = (archSegments + 1) * 4;//arch segments repeat the top one
                int archCeilTriCount = archSegments * 12;
                Vector3[] acv = new Vector3[archCeilVertCount];
                Vector2[] acuv = new Vector2[archCeilVertCount];
                int[] actri = new int[archCeilTriCount];
                Vector3[] acnrm = new Vector3[archCeilVertCount];
                Vector4[] actan = new Vector4[archCeilVertCount];

                av[0] = otl;
                Vector2 auvL = new Vector2(otl.x, otl.y) + uvOffset;
                auv[0] = CalculateUV(auvL, useWallSurface);
                anrm[0] = wallNormal;
                atan[0] = wallTangent;

                av[1] = otr;
                Vector2 auvR = new Vector2(otr.x, otr.y) + uvOffset;
                auv[1] = CalculateUV(auvR, useWallSurface);
                anrm[1] = wallNormal;
                atan[1] = wallTangent;

                for (int a = 0; a < 2; a++)//left and right arcs generated simultaneously
                {
                    Vector3 start = a == 0 ? al : at;
                    Vector3 end = a == 0 ? at : ar;
                    float angleoffset = a == 0 ? Mathf.PI * -0.5f : 0;
                    for (int s = 0; s < archSegments + 1; s++) {
                        float percent = s / (float)(archSegments);
                        float x = Mathf.Sin(percent * (Mathf.PI * 0.5f) + angleoffset) * archWidth;
                        float y = Mathf.Cos(percent * (Mathf.PI * 0.5f) + angleoffset) * archHeight;
                        Vector3 archV = new Vector3(x, y, 0) + ac;
                        Vector3 lerpV = Vector3.Lerp(start, end, percent);
                        float archDistance = Vector3.Distance(start, end);

                        //external wall arch verts
                        Vector3 vert = Vector3.Lerp(lerpV, archV, archCurve);
                        int index = s + a * (archSegments + 1) + 2;
                        av[index] = vert;
                        auv[index] = CalculateUV(new Vector2(vert.x, vert.y) + uvOffset, useWallSurface);
                        anrm[index] = wallNormal;
                        atan[index] = wallTangent;

                        //inner ceiling
                        int cindex = 0;
                        if (!interior) {
                            cindex = s + a * (archSegments + 1);
                            Vector3 vertInner = vert + wd;
                            acv[cindex * 2] = vert;
                            acv[cindex * 2 + 1] = vertInner;
                            acuv[cindex * 2] = CalculateUV(new Vector2(a == 0 ? 0 : wallThickness, archDistance * percent) + uvOffset, useWallSurface);//new Vector2(0, 0);
                            acuv[cindex * 2 + 1] = CalculateUV(new Vector2(a == 0 ? wallThickness : 0, archDistance * percent) + uvOffset, useWallSurface);//new Vector2(1, 0);
                            Vector3 nrm = (ac - vert).normalized;
                            acnrm[cindex * 2] = nrm;
                            acnrm[cindex * 2 + 1] = nrm;
                            actan[cindex * 2] = a == 0 ? acaTan : acbTan;
                            actan[cindex * 2 + 1] = a == 0 ? acaTan : acbTan;
                        }

                        if (s > 0) {
                            int indexBase = (index - 3) * 3;//minus 2 for the two inital corner points. minus 1 for starting on 2nd vert
                            if (a == 1) indexBase -= 3;
                            atri[indexBase] = a == 0 ? 0 : 1;
                            atri[indexBase + 1] = index;
                            atri[indexBase + 2] = index - 1;

                            if (!interior) {
                                int indexCeilBase = (cindex - 1) * 6;
                                if (a == 1) indexCeilBase -= 6;
                                //                                Debug.Log(((cindex) * 2)+" "+ indexCeilBase+" "+ archCeilTriCount);
                                actri[indexCeilBase] = (cindex) * 2;
                                actri[indexCeilBase + 1] = (cindex) * 2 + 1;
                                actri[indexCeilBase + 2] = (cindex - 1) * 2;
                                actri[indexCeilBase + 3] = (cindex) * 2 + 1;
                                actri[indexCeilBase + 4] = (cindex - 1) * 2 + 1;
                                actri[indexCeilBase + 5] = (cindex - 1) * 2;
                            }
                        }
                    }
                }

                //                Debug.Log("SquareOpening add data");
                DYNAMIC_MESH.AddData(av, auv, atri, anrm, atan, wallSubmesh);//wall
                if (!interior)
                    DYNAMIC_MESH.AddData(acv, acuv, actri, acnrm, actan, ceilingSubmesh);//ceiling
            }
        }

        private static void AddPlane(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Surface surface, Vector3 normal, Vector4 tangent, int submesh) {
            if (surface != null) {
                DYNAMIC_MESH.AddPlane(v0, v1, v2, v3, v0, v3, normal, tangent, submesh, surface);
            }
            else {
                DYNAMIC_MESH.AddPlane(v0, v1, v2, v3, normal, tangent, submesh);
            }
        }

        private static void AddPlane(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Surface surface, Vector2 uv0, Vector2 uv1, Vector3 normal, Vector4 tangent, int submesh) {
            if (surface != null) {
                DYNAMIC_MESH.AddPlane(v0, v1, v2, v3, uv0, uv1, normal, tangent, submesh, surface);
            }
            else {
                DYNAMIC_MESH.AddPlane(v0, v1, v2, v3, normal, tangent, submesh);
            }
        }

        private static void AddPlaneModified(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Surface surface, Vector2 uvOffset, Vector3 normal, Vector4 tangent, int submesh) {
            DYNAMIC_MESH.AddPlane(v0, v1, v2, v3, new Vector2(v0.x, v0.y) + uvOffset, new Vector2(v3.x, v3.y) + uvOffset, normal, tangent, submesh, surface);
        }

        private static Vector2 CalculateUV(Vector2 uv, Surface surface) {
            if (surface != null)
                return surface.CalculateUV(uv);
            return uv;
        }

        private static int Submesh(int defaultSubmesh, Surface surface, int defaultNullSubmesh = 0, SubmeshLibrary submeshLibrary = null) {
            if (submeshLibrary != null && surface != null) {
                int output = -1;
                output = submeshLibrary.SubmeshAdd(surface);
                if (output != -1) return output;

                return defaultNullSubmesh;
            }
            if (surface != null) return defaultSubmesh;
            return defaultNullSubmesh;
        }

        private static Vector2[] ConvertToUvs(Vector3[] verts, bool flip = false) {
            int vertCount = verts.Length;
            Vector2[] output = new Vector2[vertCount];
            for (int v = 0; v < vertCount; v++) {
                if (!flip)
                    output[v] = new Vector2(verts[v].x, verts[v].y);
                else
                    output[v] = new Vector2(verts[v].y, verts[v].x);
            }
            return output;
        }

        private static Vector2 ConvertToUvs(Vector3 verts, bool flip = false) {
            if (!flip)
                return new Vector2(verts.x, verts.y);
            else
                return new Vector2(verts.y, verts.x);
        }

        public static void Texture(out Color32[] output, WallSection section, Vector2Int pixelSize, Vector2 physicalSize, bool transparent = false) {
            //            Debug.Log(pixelSize+" "+physicalSize);
            List<TexturePaintObject> sourceTextures = new List<TexturePaintObject>();
            Surface[] usedSurfaces = { section.wallSurface, section.sillSurface, section.ceilingSurface, section.openingSurface };
            foreach (Surface surface in usedSurfaces)//Gather the source textures, resized into Color32 arrays
            {

                TexturePaintObject texturePaintObject = new TexturePaintObject();
                Texture2D texture = null;
                Color32[] texturePixels = null;
                Vector2 tiles = Vector2.one;
                if (surface != null && !surface.onlyColour) {
                    texture = surface.previewTexture as Texture2D;
                    texturePixels = surface.pixels;
                    tiles = new Vector2(surface.tiledX, surface.tiledY);
                    if (!surface.readable)
                        continue;
                }
                if (texture == null || texturePixels == null) {
                    texture = new Texture2D(1, 1);
                    texture.Apply();
                    Color32 defaultColor = new Color32(0, 0, 0, 255);//
                    if (surface != null && surface.onlyColour)
                        defaultColor = surface.colour;
                    if (!transparent)
                        defaultColor.a = 255;//make sure this is not alpha-ed right now
                    texturePixels = new[] { defaultColor };
                    texturePaintObject.tiled = true;
                }

                texturePaintObject.pixels = texturePixels;
                texturePaintObject.width = texturePixels.Length > 1 ? texture.width : 1;
                texturePaintObject.height = texturePixels.Length > 1 ? texture.height : 1;
                texturePaintObject.tiles = tiles;

                if (surface != null && !surface.onlyColour) {
                    texturePaintObject.tiled = surface.tiled;
                    if (surface.tiled) {
                        int resizedTextureWidth = Mathf.RoundToInt(surface.textureUnitSize.x * PIXELS_PER_METER);
                        int resizedTextureHeight = Mathf.RoundToInt(surface.textureUnitSize.y * PIXELS_PER_METER);
                        texturePaintObject.pixels = TextureScale.NearestNeighbourSample(texturePaintObject.pixels, texturePaintObject.width, texturePaintObject.height, resizedTextureWidth, resizedTextureHeight);
                        texturePaintObject.width = resizedTextureWidth;
                        texturePaintObject.height = resizedTextureHeight;
                    }
                }
                sourceTextures.Add(texturePaintObject);
            }

            textureWidth = pixelSize.x;
            textureHeight = pixelSize.y;
            //            Debug.Log(textureWidth+" "+textureHeight);
            //            Color32[] colourArray = new Color32[textureWidth * textureHeight];
            output = new Color32[textureWidth * textureHeight];
            Vector2Int bayBase = Vector2Int.zero;

            Vector2Int bayDimensions;

            if (!section.hasOpening) {
                //                float bayWidth = (openingWidth + size.x);
                //                float bayHeight = floorHeight;
                //                bayDimensions = new Vector2Int(textureWidth, textureHeight);
                DrawFacadeTexture(sourceTextures[0], output, bayBase, pixelSize);
            }
            else {
                bool isAbsolute = section.dimensionType == WallSection.DimensionTypes.Absolute;
                float openingWidth = section.openingWidth;
                float openingHeight = section.openingHeight;
                float openingWidthRatio = section.openingWidthRatio;
                float openingHeightRatio = section.openingHeightRatio;
                int openingPixelWidth = isAbsolute ? Mathf.RoundToInt(textureWidth * (openingWidth / physicalSize.x)) : Mathf.RoundToInt(textureWidth * openingWidth);
                int leftPixelWidth = isAbsolute ? Mathf.RoundToInt(textureWidth * ((physicalSize.x - openingWidth) * openingWidthRatio) / physicalSize.x) : Mathf.RoundToInt(textureWidth * (1 - openingWidth) * openingWidthRatio);
                int rightPixelWidth = textureWidth - openingPixelWidth - leftPixelWidth;
                int openingPixelHeight = isAbsolute ? Mathf.RoundToInt(textureHeight * (openingHeight / physicalSize.y)) : Mathf.RoundToInt(textureHeight * openingHeight);
                int bottomPixelHeight = isAbsolute ? Mathf.RoundToInt(textureHeight * ((physicalSize.y - openingHeight) * openingHeightRatio) / physicalSize.y) : Mathf.RoundToInt(textureHeight * (1 - openingHeight) * openingHeightRatio);
                int topPixelHeight = textureHeight - openingPixelHeight - bottomPixelHeight;

                //Window
                if (!section.isArched) {
                    bayBase.x = leftPixelWidth;
                    bayBase.y = bottomPixelHeight;
                    bayDimensions = new Vector2Int(openingPixelWidth, openingPixelHeight);
                    //                    Debug.Log(bayBase+" "+bayDimensions+" "+sourceTextures.Count);
                    if (sourceTextures.Count > 3)
                        DrawFacadeTexture(sourceTextures[3], output, bayBase, bayDimensions);
                    else if (sourceTextures.Count > 1)
                        DrawFacadeTexture(sourceTextures[0], output, bayBase, bayDimensions);
                }
                else {
                    bayBase.x = leftPixelWidth;
                    bayBase.y = bottomPixelHeight;
                    bayDimensions = new Vector2Int(openingPixelWidth, openingPixelHeight);
                    if (sourceTextures.Count > 3) {
                        TexturePaintObject[] tpoa = { sourceTextures[3], sourceTextures[0], sourceTextures[1] };
                        DrawFacadeTextureArch(tpoa, output, bayBase, section, bayDimensions);//(tpoa, output, bayBase, section);
                    }
                }

                //Column Left
                if (leftPixelWidth > 0) {
                    bayBase.x = 0;
                    bayBase.y = 0;
                    bayDimensions = new Vector2Int(leftPixelWidth, textureHeight);
                    DrawFacadeTexture(sourceTextures[0], output, bayBase, bayDimensions);
                }

                //Column Right
                if (rightPixelWidth > 0) {
                    bayBase.x = leftPixelWidth + openingPixelWidth;
                    bayBase.y = 0;
                    bayDimensions = new Vector2Int(rightPixelWidth, textureHeight);
                    DrawFacadeTexture(sourceTextures[0], output, bayBase, bayDimensions);
                }

                //Row Bottom
                if (bottomPixelHeight > 0) {
                    bayBase.x = leftPixelWidth;
                    bayBase.y = 0;
                    bayDimensions = new Vector2Int(openingPixelWidth, bottomPixelHeight);
                    DrawFacadeTexture(sourceTextures[0], output, bayBase, bayDimensions);
                }

                //Row Top
                if (topPixelHeight > 0) {
                    bayBase.x = leftPixelWidth;
                    bayBase.y = bottomPixelHeight + openingPixelHeight;
                    bayDimensions = new Vector2Int(openingPixelWidth, topPixelHeight);
                    DrawFacadeTexture(sourceTextures[0], output, bayBase, bayDimensions);
                }
            }

            //            output = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, true);
            //            output.filterMode = FilterMode.Bilinear;
            //            output.SetPixels32(colourArray);
            //            output.Apply(true, false);
        }

        private struct TexturePaintObject {
            public Color32[] pixels;
            public int width;
            public int height;
            public bool tiled;
            public Vector2 tiles;
            public bool flipped;
        }

        private static void DrawFacadeTexture(TexturePaintObject sourceTexture, Color32[] colourArray, Vector2Int bayBase, Vector2Int bayDimensions) {
            int paintWidth = bayDimensions.x;//Mathf.RoundToInt(bayDimensions.x * PIXELS_PER_METER);
            int paintHeight = bayDimensions.y;//Mathf.RoundToInt(bayDimensions.y * PIXELS_PER_METER);

            TexturePaintObject paintObject = sourceTexture;
            Color32[] sourceColours = paintObject.pixels;
            int sourceWidth = paintObject.width;
            int sourceHeight = paintObject.height;
            int sourceSize = sourceColours.Length;
            Vector2 textureStretch = Vector2.one;
            if (!paintObject.tiled) {
                textureStretch.x = sourceWidth / (float)paintWidth;
                textureStretch.y = sourceHeight / (float)paintHeight;
            }
            int baseX = bayBase.x;//Mathf.RoundToInt((bayBase.x * PIXELS_PER_METER));
            int baseY = bayBase.y;//Mathf.RoundToInt((bayBase.y * PIXELS_PER_METER));
            int baseCood = baseX + baseY * textureWidth;
            bool flipped = sourceTexture.flipped;

            //fill in a little bit more to cover rounding errors
            paintWidth++;
            paintHeight++;

            int useWidth = !flipped ? paintWidth : paintHeight;
            int useHeight = !flipped ? paintHeight : paintWidth;
            int textureSize = textureWidth * textureHeight;
            for (int px = 0; px < useWidth; px++) {
                for (int py = 0; py < useHeight; py++) {
                    int six, siy;
                    if (paintObject.tiled) {
                        six = (baseX + px) % sourceWidth;
                        siy = (baseY + py) % sourceHeight;
                    }
                    else {
                        six = Mathf.RoundToInt(px * textureStretch.x * paintObject.tiles.x) % sourceWidth;
                        siy = Mathf.RoundToInt(py * textureStretch.y * paintObject.tiles.y) % sourceHeight;
                    }
                    int sourceIndex = Mathf.Clamp(six + siy * sourceWidth, 0, sourceSize - 1);
                    int paintPixelIndex = (!flipped) ? px + py * textureWidth : py + px * textureWidth;
                    int pixelCoord = Mathf.Clamp(baseCood + paintPixelIndex, 0, textureSize - 1);
                    Color32 sourceColour = sourceColours[sourceIndex];
                    if (pixelCoord >= colourArray.Length || pixelCoord < 0)
                        Debug.Log(pixelCoord + " " + textureWidth + " " + textureHeight + " " + textureSize + " " + px + " " + py);
                    colourArray[pixelCoord] = sourceColour;
                }
            }
        }


        private static void DrawFacadeTextureArch(TexturePaintObject[] sourceTexture, Color32[] colourArray, Vector2Int bayBase, WallSection section, Vector2Int bayDimensions) {
            //            int paintWidth = Mathf.RoundToInt(section.openingWidth * PIXELS_PER_METER);
            //            int paintHeight = Mathf.RoundToInt(section.openingHeight * PIXELS_PER_METER);
            int paintWidth = bayDimensions.x;//Mathf.RoundToInt(bayDimensions.x * PIXELS_PER_METER);
            int paintHeight = bayDimensions.y;//Mathf.RoundToInt(bayDimensions.y * PIXELS_PER_METER);

            TextureBrush brushA = new TextureBrush(sourceTexture[0], paintWidth, paintHeight);
            TextureBrush brushB = new TextureBrush(sourceTexture[1], paintWidth, paintHeight);
            TextureBrush brushX = new TextureBrush(sourceTexture[2], paintWidth, paintHeight);

            //            int baseX = Mathf.RoundToInt((bayBase.x * PIXELS_PER_METER));
            //            int baseY = Mathf.RoundToInt((bayBase.y * PIXELS_PER_METER));
            int baseX = bayBase.x;//Mathf.RoundToInt((bayBase.x * PIXELS_PER_METER));
            int baseY = bayBase.y;//Mathf.RoundToInt((bayBase.y * PIXELS_PER_METER));
            int baseCood = baseX + baseY * textureWidth;

            //fill in a little bit more to cover rounding errors
            paintWidth++;
            paintHeight++;
            float lowerHeight = section.openingHeight * (1 - section.archHeight);
            float archPercent = lowerHeight / section.openingHeight;

            int textureSize = textureWidth * textureHeight;
            for (int px = 0; px < paintWidth; px++) {
                for (int py = 0; py < paintHeight; py++) {
                    int six, siy;

                    float xPercent = px / (float)paintWidth;
                    float yPercent = py / (float)paintHeight;

                    TextureBrush useBrush = brushA;
                    if (yPercent > archPercent) {
                        float xCenter = (xPercent * 2) - 1;
                        float yAPercent = (yPercent - archPercent) / (1 - archPercent);
                        float dist_sqrt = Mathf.Sqrt(xCenter * xCenter + yAPercent * yAPercent);
                        float dist_mnht = Mathf.Abs(xCenter) + yAPercent;
                        float dist_lerp = Mathf.Lerp(dist_mnht, dist_sqrt, section.archCurve);
                        useBrush = dist_lerp < 1 ? brushA : brushB;

//                        float striaghtThreshold = 1 - Mathf.Abs(xPercent * 2 - 1);
//                        float arcInner = Mathf.Lerp(yAPercent, (1 - Mathf.Sin((yAPercent) * Mathf.PI)), section.archCurve);
//                        float arcThreshold = Mathf.Sin(xPercent * Mathf.PI);
//                        float threshold = Mathf.Lerp(striaghtThreshold, arcThreshold, section.archCurve);
//                        bool yArcBounds = yAPercent <= threshold;
//                        useBrush = dist < 1 ? brushA : brushB;
//                        useBrush = yAPercent < xPercent ? brushX : brushB;
                        //                        useBrush = brushA;
//                        useBrush = brushX;
                    }

                    if (useBrush.paintObject.tiled) {
                        six = (baseX + px) % useBrush.sourceWidth;
                        siy = (baseY + py) % useBrush.sourceHeight;
                    }
                    else {
                        six = Mathf.RoundToInt(px * useBrush.textureStretch.x * useBrush.paintObject.tiles.x) % useBrush.sourceWidth;
                        siy = Mathf.RoundToInt(py * useBrush.textureStretch.y * useBrush.paintObject.tiles.y) % useBrush.sourceHeight;
                    }
                    int sourceIndex = Mathf.Clamp(six + siy * useBrush.sourceWidth, 0, useBrush.sourceSize - 1);
                    int paintPixelIndex = px + py * textureWidth;
                    int pixelCoord = Mathf.Clamp(baseCood + paintPixelIndex, 0, textureSize - 1);
                    Color32 sourceColour = useBrush.sourceColours[sourceIndex];
                    if (pixelCoord >= colourArray.Length || pixelCoord < 0)
                        Debug.Log(pixelCoord + " " + textureWidth + " " + textureHeight + " " + textureSize + " " + px + " " + py);
                    colourArray[pixelCoord] = sourceColour;
                }
            }
        }

        private struct TextureBrush {
            public TexturePaintObject paintObject;
            public Color32[] sourceColours;
            public int sourceWidth;
            public int sourceHeight;
            public int sourceSize;
            public Vector2 textureStretch;

            public TextureBrush(TexturePaintObject paintObject, int paintWidth, int paintHeight) {
                this.paintObject = paintObject;
                sourceColours = paintObject.pixels;
                sourceWidth = paintObject.width;
                sourceHeight = paintObject.height;
                sourceSize = sourceColours.Length;
                textureStretch = Vector2.one;
                if (!paintObject.tiled) {
                    textureStretch.x = (float)sourceWidth / (float)paintWidth;
                    textureStretch.y = (float)sourceHeight / (float)paintHeight;
                }
            }
        }
    }
}