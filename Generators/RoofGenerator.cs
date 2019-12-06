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
using BuildR2.ShapeOffset;
using JaspLib;
using UnityEngine;

namespace BuildR2
{
    public class RoofGenerator
    {
        public static void Generate(IBuilding building, IVolume volume, BuildRMesh mesh, BuildRCollider collider)
        {
            Rect basicUV = new Rect(0, 0, 0, 0);
            Generate(building, volume, mesh, collider, basicUV);
        }

        public static void Generate(IBuilding building, IVolume volume, BuildRMesh mesh, BuildRCollider collider, Rect clampUV)
        {
            int numberOfPoints = volume.numberOfPoints;
            float totalPlanHeight = volume.planTotalHeight;
            Roof roof = volume.roof;
            bool generateColliders = building.colliderType != BuildingColliderTypes.None;

            if (!roof.exists)
                return;

            List<Vector2> roofPoints = new List<Vector2>();
            List<int> facadeIndicies = new List<int>();
            
            mesh.submeshLibrary.SubmeshAdd(roof.mainSurface);
            int wallSubmesh = mesh.submeshLibrary.SubmeshAdd(roof.wallSurface != null ? roof.wallSurface : roof.mainSurface);
            int floorSubmesh = mesh.submeshLibrary.SubmeshAdd(roof.floorSurface!=null?roof.floorSurface: roof.mainSurface);

            bool[] facadeParapets = new bool[numberOfPoints];
            for (int p = 0; p < numberOfPoints; p++)
            {
                Vector3 p0 = volume.BuildingPoint(p);

                roofPoints.Add(new Vector2(p0.x, p0.z));
                facadeIndicies.Add(p);

                if (!volume.IsWallStraight(p))
                {
                    int anchorCount = volume.facadeWallAnchors[p].Count;
                    for (int a = 1; a < anchorCount - 1; a++)
                    {
                        roofPoints.Add(volume.facadeWallAnchors[p][a].vector2);
                        facadeIndicies.Add(p);
                    }
                }

                facadeParapets[p] = BuildRFacadeUtil.HasParapet(building, volume, p);
            }

            int numberOfRoofPoints = roofPoints.Count;
            Vector3[] facadeNormals = new Vector3[numberOfRoofPoints];
            Vector3[] facadeDirections = new Vector3[numberOfRoofPoints];
            float[] facadeLengths = new float[numberOfRoofPoints];
            for (int p = 0; p < numberOfRoofPoints; p++)
            {
                Vector3 p0 = roofPoints[p];
                Vector3 p1 = roofPoints[(p + 1) % numberOfRoofPoints];

                Vector3 facadeVector = (p1 - p0);
                facadeDirections[p] = facadeVector.normalized;
                facadeNormals[p] = Vector3.Cross(Vector3.up, facadeDirections[p]);
                facadeLengths[p] = facadeVector.magnitude;
            }

            Vector2[] roofPointsA = roofPoints.ToArray();
            bool[] roofGables = new bool[numberOfPoints];
            for (int g = 0; g < numberOfPoints; g++)
                roofGables[g] = volume[g].isGabled;
            Vector2[] baseRoofPoints = new Vector2[0];
            if (roof.overhang > 0)
            {
                OffsetPoly polyOffset = new OffsetPoly(roofPointsA, -roof.overhang);
                polyOffset.Execute();
                baseRoofPoints = polyOffset.Shape();

                ShapeToRoofMesh.OverhangUnderside(ref mesh, roofPointsA, baseRoofPoints, totalPlanHeight, roof);
            }
            else
            {
                baseRoofPoints = roofPointsA;
            }

            if (baseRoofPoints.Length == 0) return;

            Vector2[] parapetExternalPoints = new Vector2[0];
            Vector2[] parapetInternalPoints = new Vector2[0];
            float parapetFrontDepth = roof.parapetFrontDepth;
            float parapetBackDepth = roof.parapetBackDepth;

            if (generateColliders)
                collider.thickness = parapetFrontDepth * 0.5f + parapetBackDepth;

            bool parapet = roof.parapet && building.meshType == BuildingMeshTypes.Full;
            if (parapet)
			{
				OffsetPoly polyOffset = new OffsetPoly(baseRoofPoints, -parapetFrontDepth);
                polyOffset.Execute();
                parapetExternalPoints = polyOffset.Shape();

                polyOffset = new OffsetPoly(baseRoofPoints, parapetBackDepth);
                polyOffset.Execute();
                parapetInternalPoints = polyOffset.Shape();
            }


            int roofPointCount = baseRoofPoints.Length;
            if (parapet && parapetExternalPoints.Length > 0 && parapetInternalPoints.Length > 0)
            {
				List<BuildRVolumeUtil.ParapetWallData> parapetShapes = BuildRVolumeUtil.GetParapetShapes(building, volume, baseRoofPoints);
				for (int p = 0; p < roofPointCount; p++)
				{
					BuildRVolumeUtil.ParapetWallData parapetWallData = parapetShapes[p];

					int facadeIndex = facadeIndicies[p];
					if (!facadeParapets[facadeIndex] || parapetWallData.type == BuildRVolumeUtil.ParapetWallData.Types.None) continue;

                    int pb = (p + 1) % roofPointCount;
                    int pbi = (p + 1) % parapetInternalPoints.Length;
                    int pbe = (p + 1) % parapetExternalPoints.Length;

                    int facadeIndexB = (facadeIndex + 1) % numberOfPoints;
                    int facadeIndexC = (facadeIndex - 1 + numberOfPoints) % numberOfPoints;
					
                    bool facadeParapetB = facadeParapets[facadeIndexB] && parapetShapes[facadeIndexB].type != BuildRVolumeUtil.ParapetWallData.Types.None;
                    bool facadeParapetC = facadeParapets[facadeIndexC] && parapetShapes[facadeIndexC].type != BuildRVolumeUtil.ParapetWallData.Types.None;

                    Vector3 p0 = new Vector3(baseRoofPoints[p].x, totalPlanHeight, baseRoofPoints[p].y);
                    Vector3 p1 = new Vector3(baseRoofPoints[pb].x, totalPlanHeight, baseRoofPoints[pb].y);
                    Vector3 facadeVector = (p1 - p0);
                    Vector3 facadeDirection = facadeVector.normalized;
                    Vector3 facadeNormal = Vector3.Cross(Vector3.up, facadeDirection);
				    int pCount = Mathf.Min(parapetExternalPoints.Length, parapetInternalPoints.Length);
                    if (p < pCount)
                    {
						float facadeLength = facadeVector.magnitude;

                        if (!facadeParapetC)//need to straighten the ends if no parapet exists
						{
                            Vector3 parapetEndExternalC = p0 + facadeNormal * parapetFrontDepth;
                            Vector3 parapetEndInternalC = p0 - facadeNormal * parapetBackDepth;
                            parapetExternalPoints[p] = new Vector2(parapetEndExternalC.x, parapetEndExternalC.z);
                            parapetInternalPoints[p] = new Vector2(parapetEndInternalC.x, parapetEndInternalC.z);
                        }
                        if (!facadeParapetB)//need to straighten the ends if no parapet exists
						{
                            Vector3 parapetEndExternalB = p1 + facadeNormal * parapetFrontDepth;
                            Vector3 parapetEndInternalB = p1 - facadeNormal * parapetBackDepth;
                            parapetExternalPoints[pbe] = new Vector2(parapetEndExternalB.x, parapetEndExternalB.z);
                            parapetInternalPoints[pbi] = new Vector2(parapetEndInternalB.x, parapetEndInternalB.z);
                        }

                        //external points
                        Vector3 p0e = new Vector3(parapetExternalPoints[p].x, totalPlanHeight, parapetExternalPoints[p].y);
                        Vector3 p1e = new Vector3(parapetExternalPoints[pbe].x, totalPlanHeight, parapetExternalPoints[pbe].y);
                        //internal points
                        Vector3 p0i = new Vector3(parapetInternalPoints[p].x, totalPlanHeight, parapetInternalPoints[p].y);
                        Vector3 p1i = new Vector3(parapetInternalPoints[pbi].x, totalPlanHeight, parapetInternalPoints[pbi].y);
                        float uvAngle = JMath.SignAngle(new Vector2(facadeDirection.x, facadeDirection.z).normalized) + 90;
                        Vector4 facadeTangent = BuildRMesh.CalculateTangent(facadeDirection);
                        Vector4 facadeTangentInverse = BuildRMesh.CalculateTangent(-facadeDirection);

						if (parapetWallData.type == BuildRVolumeUtil.ParapetWallData.Types.AtoIntersection)
						{
							Vector2 intV2 = parapetWallData.Int;
							Vector3 intV3 = new Vector3(intV2.x, totalPlanHeight, intV2.y);
							p1e = intV3 + facadeNormal * parapetFrontDepth;
							p1i = intV3 - facadeNormal * parapetBackDepth;
						}

						if (parapetWallData.type == BuildRVolumeUtil.ParapetWallData.Types.IntersectiontoB)
						{
							Vector2 intV2 = parapetWallData.Int;
							Vector3 intV3 = new Vector3(intV2.x, totalPlanHeight, intV2.y);
							p0e = intV3 + facadeNormal * parapetFrontDepth;
							p0i = intV3 - facadeNormal * parapetBackDepth;
						}

						if (roof.parapetStyle == Roof.ParapetStyles.Flat)
                        {

                            Vector3 parapetUp = Vector3.up * roof.parapetHeight;

                            Vector3 w0 = p0e;//front left
                            Vector3 w1 = p1e;//front right
                            Vector3 w2 = p0i;//back left
                            Vector3 w3 = p1i;//back right
                            Vector3 w6 = w2 + parapetUp;//front left top
                            Vector3 w7 = w3 + parapetUp;//front right top
                            Vector3 w4 = w0 + parapetUp;//back left top
                            Vector3 w5 = w1 + parapetUp;//back right top

                            mesh.AddPlane(w0, w1, w4, w5, Vector2.zero, new Vector2(facadeLength, roof.parapetHeight), facadeNormal, facadeTangent, wallSubmesh, roof.wallSurface);//front
                            mesh.AddPlane(w3, w2, w7, w6, Vector2.zero, new Vector2(facadeLength, roof.parapetHeight), -facadeNormal, facadeTangentInverse, wallSubmesh, roof.wallSurface);//back
                            mesh.AddPlaneComplexUp(w7, w6, w5, w4, uvAngle, Vector3.up, facadeTangent, wallSubmesh, roof.wallSurface);//top

                            if (generateColliders)
                            {
                                collider.AddPlane(w0, w1, w4, w5);
                                if (!collider.usingPrimitives)
                                {
                                    collider.mesh.AddPlane(w3, w2, w7, w6, 0);
                                    collider.mesh.AddPlane(w7, w6, w5, w4, 0);
                                }
                            }

                            if (parapetFrontDepth > 0)
                                mesh.AddPlaneComplexUp(p0, p1, w0, w1, uvAngle, Vector3.down, facadeTangent, wallSubmesh, roof.wallSurface);//bottom

                            bool leftParapet = facadeParapetB;
                            if (!leftParapet)
                            {
                                //todo proper calculations
                                Vector3 leftCapNormal = Vector3.forward;
                                mesh.AddPlane(w0, w2, w4, w6, Vector2.zero, new Vector2(parapetBackDepth + parapetFrontDepth, roof.parapetHeight), leftCapNormal, facadeTangent, wallSubmesh, roof.wallSurface);//left cap
                            }

                            bool rightParapet = facadeParapetC;
                            if (!rightParapet)
                            {
                                //todo proper calculations
                                Vector3 rightCapNormal = Vector3.forward;
                                mesh.AddPlane(w3, w1, w7, w5, Vector2.zero, new Vector2(parapetBackDepth + parapetFrontDepth, roof.parapetHeight), rightCapNormal, facadeTangent, wallSubmesh, roof.wallSurface);//right cap
                            }
                        }
                        else//battlements!
                        {
                            int battlementCount = Mathf.CeilToInt(facadeLength / roof.battlementSpacing) * 2 + 1;
                            for (int b = 0; b < battlementCount + 1; b++)
                            {
                                float percentLeft = b / (float)(battlementCount);
                                float percentRight = (b + 1f) / (battlementCount);
                                float parapetUVStart = percentLeft * facadeLength;
                                float parapetUVWidth = (percentRight - percentLeft) * facadeLength;

                                Vector3 b0 = Vector3.Lerp(p0e, p1e, percentLeft);
                                Vector3 b1 = Vector3.Lerp(p0e, p1e, percentRight);
                                Vector3 b2 = Vector3.Lerp(p0i, p1i, percentLeft);
                                Vector3 b3 = Vector3.Lerp(p0i, p1i, percentRight);
                                bool upperBattlement = b % 2 == 0;
                                float battlementUp = upperBattlement ? roof.parapetHeight : roof.parapetHeight * roof.battlementHeightRatio;
                                Vector3 battlementUpV = Vector3.up * battlementUp;

                                Vector3 b6 = b2 + battlementUpV;//front left top
                                Vector3 b7 = b3 + battlementUpV;//front right top
                                Vector3 b4 = b0 + battlementUpV;//back left top
                                Vector3 b5 = b1 + battlementUpV;//back right top

                                //front
                                mesh.AddPlane(b0, b1, b4, b5, new Vector2(parapetUVStart, 0), new Vector2(parapetUVStart + parapetUVWidth, battlementUp), facadeNormal, facadeTangent, wallSubmesh, roof.wallSurface);
                                //back
                                mesh.AddPlane(b3, b2, b7, b6, new Vector2(parapetUVStart, 0), new Vector2(parapetUVStart + parapetUVWidth, battlementUp), -facadeNormal, facadeTangentInverse, wallSubmesh, roof.wallSurface);
                                //top
                                mesh.AddPlaneComplexUp(b7, b6, b5, b4, uvAngle, Vector3.up, facadeTangent, wallSubmesh, roof.wallSurface);
                                if (parapetFrontDepth > 0)
                                    mesh.AddPlaneComplexUp(p0, p1, b0, b1, uvAngle, Vector3.down, facadeTangent, wallSubmesh, roof.wallSurface);//bottom

                                if (generateColliders)
                                {
                                    collider.AddPlane(b0, b1, b4, b5);
                                    if (!collider.usingPrimitives)
                                    {
                                        collider.mesh.AddPlane(b3, b2, b7, b6, 0);
                                        collider.mesh.AddPlane(b7, b6, b5, b4, 0);
                                    }
                                }

                                if (upperBattlement)
                                {
                                    //todo proper calculations
                                    float uvBattlementCapUp = roof.parapetHeight * roof.battlementHeightRatio;
                                    Vector3 leftCapNormal = -facadeDirection;
                                    Vector4 leftCapTangent = BuildRMesh.CalculateTangent(-facadeNormal);
                                    mesh.AddPlane(b2, b0, b6, b4, new Vector2(parapetUVStart, 0), new Vector2(parapetUVStart + roof.parapetBackDepth + parapetFrontDepth, uvBattlementCapUp), leftCapNormal, leftCapTangent, wallSubmesh, roof.wallSurface);//left cap
                                    Vector3 rightCapNormal = facadeDirection;
                                    Vector4 rightCapTangent = BuildRMesh.CalculateTangent(facadeNormal);
                                    mesh.AddPlane(b1, b3, b5, b7, new Vector2(parapetUVStart, 0), new Vector2(parapetUVStart + roof.parapetBackDepth + parapetFrontDepth, uvBattlementCapUp), rightCapNormal, rightCapTangent, wallSubmesh, roof.wallSurface);//right cap

                                    if (generateColliders)
                                    {
                                        if (!collider.usingPrimitives)
                                        {
                                            collider.mesh.AddPlane(b2, b0, b6, b4, 0);
                                            collider.mesh.AddPlane(b1, b3, b5, b7, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Vector2[] roofFloorBasePoints = (roof.parapet && roof.parapetBackDepth > 0 && parapetInternalPoints.Length > 0) ? parapetInternalPoints : baseRoofPoints;

            Roof.Types roofType = roof.type;
            if (volume.abovePlanCount > 0)
                roofType = Roof.Types.Flat;

            switch (roofType)
            {
                default:
                    Flat(building, volume, mesh, collider, roofFloorBasePoints, totalPlanHeight, roof, floorSubmesh, roof.floorSurface, clampUV);
                    break;

                case Roof.Types.Pitched:
                    if (!PitchedRoofGenerator.Generate(mesh, collider, roofFloorBasePoints, facadeIndicies.ToArray(), totalPlanHeight, volume, clampUV))
                        Flat(building, volume, mesh, collider, roofFloorBasePoints, totalPlanHeight, roof, floorSubmesh, roof.floorSurface, clampUV);
                    break;

                case Roof.Types.Mansard:
                    if(!MansardRoofGenerator.Generate(mesh, collider, roofFloorBasePoints, facadeIndicies.ToArray(), totalPlanHeight, volume))
                        Flat(building, volume, mesh, collider, roofFloorBasePoints, totalPlanHeight, roof, floorSubmesh, roof.floorSurface, clampUV);
                    //                    ShapeToRoofMesh.MansardRoof(ref mesh, roofFloorBasePoints, roofGables, totalPlanHeight, roof, surfaceMapping);
                    break;

                    //                case Roof.Types.Gambrel:
                    //                    ShapeToRoofMesh.Gambrel(ref mesh, roofFloorBasePoints, roofGables, totalPlanHeight, roof, surfaceMapping);
                    //                    break;
            }
        }

        private static void Flat(IBuilding building, IVolume volume, BuildRMesh mesh, BuildRCollider collider, Vector2[] points, float roofBaseHeight, Roof design, int submesh, Surface surface, Rect clampUV)
        {
            BuildRVolumeUtil.VolumeShape[] roofPoints = BuildRVolumeUtil.GetTopShape(building, volume, points);
            int roofShapeCount = roofPoints.Length;
            for (int r = 0; r < roofShapeCount; r++)
            {
                Poly2TriWrapper.BMesh(mesh, roofBaseHeight, surface, submesh, roofPoints[r].outer, clampUV, true, roofPoints[r].holes, collider);
            }
        }

        private static void Pitched(BuildRMesh mesh, Vector3[] points, Roof design, int submesh)
        {
            int numberOfVolumePoints = points.Length;
            Vector2[] volumePoints = new Vector2[numberOfVolumePoints];
            for (int i = 0; i < numberOfVolumePoints; i++)
                volumePoints[i] = new Vector2(points[i].x, points[i].z);

            OffsetSkeleton offsetPoly = new OffsetSkeleton(volumePoints);
            offsetPoly.Execute();
        }
    }
}