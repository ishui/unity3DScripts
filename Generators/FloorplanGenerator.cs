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
using System.Collections.Generic;
using JaspLib;
using UnityEngine;

namespace BuildR2 {
    /// <summary>
    /// Generates the interior rooms of a specific floor
    /// </summary>
    public class FloorplanGenerator {

        public static void Generate(IBuilding building, IVolume volume, IFloorplan floorplan, int volumeFloor, VerticalOpening[] openings, BuildRMesh mesh, BuildRCollider collider) {
            SubmeshLibrary submeshLibrary = mesh.submeshLibrary;

            bool generateColliders = building.colliderType != BuildingColliderTypes.None;
            bool generateMeshColliders = building.colliderType != BuildingColliderTypes.Primitive && generateColliders;
            BuildRCollider sendCollider = (generateColliders) ? collider : null;
            collider.thickness = volume.wallThickness;
            if (!generateMeshColliders) collider = null;

            float wallThickness = volume.wallThickness;
            float wallUp = volume.floorHeight - wallThickness;
            Vector3 wallUpV = Vector3.up * wallUp;
            Vector3 floorBaseV = Vector3.up * volume.baseHeight;

            int roomCount = floorplan.RoomCount;

            int actualFloor = building.VolumeBaseFloor(volume) + volumeFloor;
            int openingCount = openings.Length;
            bool[] openingBelow = new bool[openingCount];
            bool[] openingAbove = new bool[openingCount];
            FlatBounds[] openingBounds = new FlatBounds[openingCount];
            Vector2[][] openingShapes = new Vector2[openingCount][];
            bool[] openingUsedInThisFloor = new bool[openingCount];
            for (int o = 0; o < openingCount; o++) {
                VerticalOpening opening = openings[o];
                if (!openings[o].FloorIsIncluded(actualFloor)) continue;
                openingBelow[o] = opening.FloorIsIncluded(actualFloor - 1);
                openingAbove[o] = opening.FloorIsIncluded(actualFloor + 1);
                openingShapes[o] = opening.PointsRotated();
                openingBounds[o] = new FlatBounds(openingShapes[o]);

                submeshLibrary.Add(opening.surfaceA);
                submeshLibrary.Add(opening.surfaceB);
                submeshLibrary.Add(opening.surfaceC);
                submeshLibrary.Add(opening.surfaceD);
            }

            Dictionary<int, List<Vector2Int>> externalWallAnchors = volume.facadeWallAnchors;

            Room[] rooms = floorplan.AllRooms();
            for (int r = 0; r < roomCount; r++) {
                Room room = rooms[r];
                int pointCount = room.numberOfPoints;

                Surface floorSurface = null;
                Surface wallSurface = null;
                Surface ceilingSurface = null;

                if (room.style != null) {
                    RoomStyle style = room.style;
                    floorSurface = style.floorSurface;
                    wallSurface = style.wallSurface;
                    ceilingSurface = style.ceilingSurface;
                }

                int floorSubmesh = submeshLibrary.SubmeshAdd(floorSurface);
                int wallSubmesh = submeshLibrary.SubmeshAdd(wallSurface);
                int ceilingSubmesh = submeshLibrary.SubmeshAdd(ceilingSurface);

                FloorplanUtil.RoomWall[] walls = FloorplanUtil.CalculatePoints(room, volume);
                Vector2[] roomArchorPoints = FloorplanUtil.RoomArchorPoints(walls);

                Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);

                Vector2[] offsetRoomAnchorPoints = QuickPolyOffset.Execute(roomArchorPoints, wallThickness);

                FlatBounds roomBounds = new FlatBounds(offsetRoomAnchorPoints);
                List<Vector2[]> floorCuts = new List<Vector2[]>();
                List<Vector2[]> ceilingCuts = new List<Vector2[]>();
                List<VerticalOpening> roomOpenings = new List<VerticalOpening>();
                for (int o = 0; o < openingCount; o++) {
                    if (openings[o].FloorIsIncluded(actualFloor)) {
                        if (roomBounds.Overlaps(openingBounds[o])) {
                            if (CheckShapeWithinRoom(offsetRoomAnchorPoints, openingShapes[o])) {
                                if (openingBelow[o]) floorCuts.Add(openingShapes[o]);
                                if (openingAbove[o]) ceilingCuts.Add(openingShapes[o]);
                                if (openingAbove[o] || openingBelow[o]) {
                                    roomOpenings.Add(openings[o]);
                                    openingUsedInThisFloor[o] = true;
                                }
                            }
                        }
                    }
                }

                int offsetPointBase = 0;
                for (int p = 0; p < pointCount; p++)//generate room walls
                {
                    FloorplanUtil.RoomWall wall = walls[p];
                    int wallPointCount = wall.offsetPoints.Length;

                    List<RoomPortal> wallPortals = floorplan.GetWallPortals(room, p);
                    int wallPortalCount = wallPortals.Count;

                    if (!wall.isExternal) {
                        int indexA = offsetPointBase;
                        int indexB = (offsetPointBase + 1) % roomArchorPoints.Length;
                        Vector2 origBaseA = roomArchorPoints[indexA];
                        Vector2 origBaseB = roomArchorPoints[indexB];
                        Vector2 baseA = offsetRoomAnchorPoints[indexA];
                        Vector2 baseB = offsetRoomAnchorPoints[indexB];
                        Vector3 v0 = new Vector3(origBaseA.x, 0, origBaseA.y) + floorBaseV;
                        Vector3 v1 = new Vector3(origBaseB.x, 0, origBaseB.y) + floorBaseV;
                        Vector3 vOffset0 = new Vector3(baseA.x, 0, baseA.y) + floorBaseV;
                        Vector3 vOffset1 = new Vector3(baseB.x, 0, baseB.y) + floorBaseV;
                        if (wallPortalCount == 0) {//just draw the wall - no portals to cut

                            Vector3 v2 = vOffset1 + wallUpV;
                            Vector3 v3 = vOffset0 + wallUpV;

                            Vector2 minUV = Vector2.zero;
                            Vector2 maxUV = new Vector2(Vector2.Distance(baseA, baseB), wallUp);
                            if (wallSurface != null) maxUV = wallSurface.CalculateUV(maxUV);
                            Vector3 wallDir = (vOffset0 - vOffset1).normalized;
                            Vector3 wallNormal = Vector3.Cross(Vector3.up, wallDir);
                            Vector4 wallTangent = BuildRMesh.CalculateTangent(wallDir);
                            mesh.AddPlane(vOffset1, vOffset0, v2, v3, minUV, maxUV, wallNormal, wallTangent, wallSubmesh, wallSurface);

                            if (generateColliders)
                                collider.AddPlane(vOffset1, vOffset0, v2, v3);
                        }
                        else {
                            List<float> useLaterals = new List<float>();
                            List<bool> hasPortals = new List<bool>();
                            for (int wp = 0; wp < wallPortalCount; wp++) {
                                RoomPortal portal = wallPortals[wp];
                                bool hasPortal = room.HasPortal(portal);
                                hasPortals.Add(hasPortal);
                                if (hasPortal)
                                    useLaterals.Add(portal.lateralPosition);
                                else
                                    useLaterals.Add(1 - portal.lateralPosition);//portal from other wall - wall orientation is flipped
                            }
                            
                            Vector3 wallVector = vOffset1 - vOffset0;
                            Vector3 wallDirection = wallVector.normalized;
                            Vector3 wallStart = vOffset0;
                            Vector4 wallTangent = BuildRMesh.CalculateTangent(wallDirection);
                            Vector3 wallNormal = Vector3.Cross(Vector3.up, wallDirection);
                            Vector4 wallNormalTangent = BuildRMesh.CalculateTangent(wallNormal);
                            Vector4 wallNormalTangentReverse = BuildRMesh.CalculateTangent(-wallNormal);

                            while (wallPortalCount > 0) {
                                int portalIndex = 0;
                                RoomPortal usePortal = wallPortals[0];
                                float lowestLat = useLaterals[0];
                                for (int wp = 1; wp < wallPortalCount; wp++) {
                                    if (useLaterals[wp] < lowestLat) {
                                        portalIndex = wp;
                                        usePortal = wallPortals[wp];
                                        lowestLat = useLaterals[wp];
                                    }
                                }

                                wallPortals.RemoveAt(portalIndex);
                                useLaterals.RemoveAt(portalIndex);
                                wallPortalCount--;

                                Vector3 vl0 = v0 + (-wallNormal + wallDirection) * wallThickness;
                                Vector3 vl1 = v1 + (-wallNormal - wallDirection) * wallThickness;

                                Vector3 portalCenter = Vector3.Lerp(vl0, vl1, lowestLat);
                                Vector3 portalHalfvector = wallDirection * (usePortal.width * 0.5f);
                                Vector3 portalBase = Vector3.up * (volume.floorHeight - usePortal.height) * usePortal.verticalPosition;
                                Vector3 portalUp = portalBase + Vector3.up * usePortal.height;
                                Vector3 portalStart = portalCenter - portalHalfvector;
                                Vector3 portalEnd = portalCenter + portalHalfvector;

                                Vector2 initalWallUVMin = new Vector2(Vector3.Dot(portalStart, wallDirection), 0);
                                Vector2 initalWallUVMax = new Vector2(Vector3.Dot(wallStart, wallDirection), wallUp);
                                mesh.AddPlane(portalStart, wallStart, portalStart + wallUpV, wallStart + wallUpV, initalWallUVMin, initalWallUVMax, wallNormal, wallTangent, wallSubmesh, wallSurface);//initial wall
                                if (generateColliders)
                                    collider.AddPlane(portalStart, wallStart, portalStart + wallUpV, wallStart + wallUpV);
                                if (usePortal.verticalPosition > 0) {
                                    Vector2 portalBaseUVMin = new Vector2(Vector3.Dot(portalEnd, wallDirection), 0);
                                    Vector2 portalBaseUVMax = new Vector2(Vector3.Dot(portalStart, wallDirection), portalBase.y);
                                    mesh.AddPlane(portalEnd, portalStart, portalEnd + portalBase, portalStart + portalBase, portalBaseUVMin, portalBaseUVMax, wallNormal, wallTangent, wallSubmesh, wallSurface);//bottom
                                    if (generateColliders)
                                        collider.AddPlane(portalEnd, portalStart, portalEnd + portalBase, portalStart + portalBase);
                                }
                                if (usePortal.verticalPosition < 1) {
                                    Vector2 portalBaseUVMin = new Vector2(Vector3.Dot(portalEnd, wallDirection), portalUp.y);
                                    Vector2 portalBaseUVMax = new Vector2(Vector3.Dot(portalStart, wallDirection), wallUp);
                                    mesh.AddPlane(portalEnd + portalUp, portalStart + portalUp, portalEnd + wallUpV, portalStart + wallUpV, portalBaseUVMin, portalBaseUVMax, wallNormal, wallTangent, wallSubmesh, wallSurface);//top
                                    if (generateColliders)
                                        collider.AddPlane(portalEnd + portalUp, portalStart + portalUp, portalEnd + wallUpV, portalStart + wallUpV);
                                }

                                if (hasPortals[portalIndex])//only do this once - from the room it's attached to
                                {
                                    //portal interior frame
                                    Vector3 portalDepth = wallNormal * wallThickness * 2;

                                    //sides
                                    mesh.AddPlane(portalStart + portalDepth + portalBase, portalStart + portalBase, portalStart + portalDepth + portalUp, portalStart + portalUp, wallDirection, wallNormalTangentReverse, wallSubmesh);
                                    mesh.AddPlane(portalEnd + portalBase, portalEnd + portalDepth + portalBase, portalEnd + portalUp, portalEnd + portalDepth + portalUp, -wallDirection, wallNormalTangent, wallSubmesh);

                                    if (generateMeshColliders) {
                                        collider.AddPlane(portalStart + portalDepth + portalBase, portalStart + portalBase, portalStart + portalDepth + portalUp, portalStart + portalUp);
                                        collider.AddPlane(portalEnd + portalBase, portalEnd + portalDepth + portalBase, portalEnd + portalUp, portalEnd + portalDepth + portalUp);
                                    }

                                    //floor
                                    Vector2 minFloorUv = new Vector2((portalEnd + portalBase).z, (portalEnd + portalBase).x);
                                    Vector2 maxFloorUv = minFloorUv + new Vector2(wallThickness, usePortal.width);
                                    mesh.AddPlane(portalStart + portalBase, portalStart + portalDepth + portalBase, portalEnd + portalBase, portalEnd + portalDepth + portalBase, minFloorUv, maxFloorUv, Vector3.up, wallTangent, floorSubmesh, floorSurface);
                                    if (generateMeshColliders)
                                        collider.AddPlane(portalStart + portalBase, portalStart + portalDepth + portalBase, portalEnd + portalBase, portalEnd + portalDepth + portalBase);

                                    //ceiling
                                    mesh.AddPlane(portalEnd + portalUp, portalEnd + portalDepth + portalUp, portalStart + portalUp, portalStart + portalDepth + portalUp, Vector3.down, wallTangent, wallSubmesh);
                                    if (generateMeshColliders)
                                        collider.AddPlane(portalEnd + portalUp, portalEnd + portalDepth + portalUp, portalStart + portalUp, portalStart + portalDepth + portalUp);
                                }

                                wallStart = portalEnd;//move the start for the next calculation
                            }

                            Vector2 finalWallUVMin = new Vector2(Vector3.Dot(vOffset1, wallDirection), 0);
                            Vector2 finalWallUVMax = new Vector2(Vector3.Dot(wallStart, wallDirection), wallUp);
                            mesh.AddPlane(vOffset1, wallStart, vOffset1 + wallUpV, wallStart + wallUpV, finalWallUVMin, finalWallUVMax, wallNormal, wallTangent, wallSubmesh, wallSurface);//final wall section
                            if (generateColliders)
                                collider.AddPlane(vOffset1, wallStart, vOffset1 + wallUpV, wallStart + wallUpV);
                        }
                        offsetPointBase += 1;
                    }
                    else//external anchored wall
                    {
                        int facadeIndex = wall.facadeIndex;
                        Facade facadeDesign = volume.GetFacade(facadeIndex);
                        int currentFacadeWallSectionLength = externalWallAnchors[facadeIndex].Count - 1;
                        int currentWallSectionIndex = wall.offsetPointWallSection[0];
                        int wallOffsetPoints = wall.offsetPoints.Length;
                        for (int w = 0; w < wallOffsetPoints - 1; w++) {
                            int roomPointIndex = offsetPointBase + w;
                            Vector2 baseA = offsetRoomAnchorPoints[roomPointIndex];
                            int offsetIndexB = (roomPointIndex + 1) % offsetRoomAnchorPoints.Length;
                            Vector2 baseB = offsetRoomAnchorPoints[offsetIndexB];
                            Vector3 v0 = new Vector3(baseA.x, 0, baseA.y) + floorBaseV;
                            Vector3 v1 = new Vector3(baseB.x, 0, baseB.y) + floorBaseV;
                            int wallSectionIndex = wall.offsetPointWallSection[w];

                            bool canGenerateWallSection = facadeDesign != null;

                            Vector3 wallVector = v0 - v1;
                            Vector3 wallDir = wallVector.normalized;
                            float wallLength = wallVector.magnitude;

                            if (!canGenerateWallSection) {
                                if (wallSurface != null) {
                                    submeshLibrary.Add(wallSurface);
                                }

                                Vector3 v2 = v1 + wallUpV;
                                Vector3 v3 = v0 + wallUpV;

                                Vector2 minUV = Vector2.zero;
                                Vector2 maxUV = new Vector2(Vector2.Distance(baseA, baseB), wallUp);
                                Vector3 wallNormal = Vector3.Cross(Vector3.up, wallDir);
                                Vector4 wallTangent = BuildRMesh.CalculateTangent(wallDir);
                                mesh.AddPlane(v1, v0, v2, v3, minUV, maxUV, wallNormal, wallTangent, wallSubmesh, wallSurface);

                                if (generateMeshColliders)
                                    collider.AddPlane(v1, v0, v2, v3);
                            }
                            else {
                                WallSection section = facadeDesign.GetWallSection(wallSectionIndex, volumeFloor, currentFacadeWallSectionLength, volume.floors);
                                if (section.model != null)
                                    continue;//cannot account for custom meshes assume custom mesh does include interior mesh or if does - will be generated with the exterior
                                GenerationOutput generatedSection = GenerationOutput.CreateRawOutput();
                                Vector2 wallSectionSize = new Vector2(wallLength, wallUp + wallThickness);
                                bool cullOpening = building.cullDoors && section.isDoor;
                                SubmeshLibrary sectionLib = new SubmeshLibrary();
                                
                                if (wallSurface != null) {
                                    sectionLib.Add(wallSurface);//add interior wall surface
                                    submeshLibrary.Add(wallSurface);
                                }

                                sectionLib.Add(section.openingSurface);//add windows - the only surface we'll use in the interior room
                                submeshLibrary.Add(section.openingSurface);
                                
                                float offset = 0;
                                if (w == 0) offset = wallThickness;
                                if (w == wallOffsetPoints - 2) offset = -wallThickness;
                                WallSectionGenerator.Generate(section, generatedSection, wallSectionSize, true, wallThickness, cullOpening, null, sectionLib, offset);
                                int[] mapping = submeshLibrary.MapSubmeshes(generatedSection.raw.materials);
                                Vector3 curveNormal = Vector3.Cross(wallDir, Vector3.up);

                                Quaternion meshRot = Quaternion.LookRotation(curveNormal, Vector3.up);
                                Vector3 meshPos = new Vector3(v1.x, volume.baseHeight, v1.z) + wallDir * wallSectionSize.x + Vector3.up * wallSectionSize.y;
                                meshPos += meshRot * -new Vector3(wallSectionSize.x, wallSectionSize.y, 0) * 0.5f;
                                mesh.AddData(generatedSection.raw, mapping, meshPos, meshRot, Vector3.one);
                            }


                            currentWallSectionIndex++;
                            if (currentWallSectionIndex >= currentFacadeWallSectionLength) {
                                //reached the end of the facade - move to the next one and continue
                                currentFacadeWallSectionLength = externalWallAnchors[facadeIndex].Count;
                                currentWallSectionIndex = 0;
                            }
                        }

                        offsetPointBase += wallPointCount - 1;
                    }
                }

                //FLOOR
                Vector2[] mainShape = offsetRoomAnchorPoints;
                Vector2[][] floorCutPoints = floorCuts.ToArray();
                int floorVertCount = mainShape.Length;
                for (int flc = 0; flc < floorCutPoints.Length; flc++)
                    floorVertCount += floorCutPoints[flc].Length;

                Vector2[] allFloorPoints = new Vector2[floorVertCount];
                int mainShapeLength = mainShape.Length;
                for (int ms = 0; ms < mainShapeLength; ms++)
                    allFloorPoints[ms] = mainShape[ms];
                int cutPointIterator = mainShapeLength;
                for (int flc = 0; flc < floorCutPoints.Length; flc++) {
                    for (int flcp = 0; flcp < floorCutPoints[flc].Length; flcp++) {
                        allFloorPoints[cutPointIterator] = floorCutPoints[flc][flcp];
                        cutPointIterator++;
                    }
                }

                Vector3[] floorPoints = new Vector3[floorVertCount];
                Vector2[] floorUvs = new Vector2[floorVertCount];
                Vector3[] floorNorms = new Vector3[floorVertCount];
                Vector4[] floorTangents = new Vector4[floorVertCount];
                for (int rp = 0; rp < floorVertCount; rp++) {
                    floorPoints[rp] = new Vector3(allFloorPoints[rp].x, 0, allFloorPoints[rp].y) + floorBaseV;
                    Vector2 uv = allFloorPoints[rp];
                    if (floorSurface != null) uv = floorSurface.CalculateUV(uv);
                    floorUvs[rp] = uv;
                    floorNorms[rp] = Vector3.up;
                    floorTangents[rp] = tangent;
                }

                int[] tris = Poly2TriWrapper.Triangulate(mainShape, true, floorCutPoints);
                
                mesh.AddData(floorPoints, floorUvs, tris, floorNorms, floorTangents, floorSubmesh);
                if (generateColliders)
                    collider.mesh.AddData(floorPoints, floorUvs, tris, floorNorms, floorTangents, 0);

                //CEILING!
                Vector2[][] ceilingCutPoints = ceilingCuts.ToArray();
                int ceilingVertCount = mainShape.Length;
                for (int flc = 0; flc < ceilingCutPoints.Length; flc++)
                    ceilingVertCount += ceilingCutPoints[flc].Length;

                Vector2[] allCeilingPoints = new Vector2[ceilingVertCount];
                for (int ms = 0; ms < mainShapeLength; ms++)
                    allCeilingPoints[ms] = mainShape[ms];
                cutPointIterator = mainShapeLength;
                for (int flc = 0; flc < ceilingCutPoints.Length; flc++) {
                    for (int flcp = 0; flcp < ceilingCutPoints[flc].Length; flcp++) {
                        allCeilingPoints[cutPointIterator] = ceilingCutPoints[flc][flcp];
                        cutPointIterator++;
                    }
                }

                Vector3[] ceilingPoints = new Vector3[ceilingVertCount];
                Vector2[] ceilingUvs = new Vector2[ceilingVertCount];
                Vector3[] ceilingNorms = new Vector3[ceilingVertCount];
                Vector4[] ceilingTangents = new Vector4[ceilingVertCount];
                for (int rp = 0; rp < ceilingVertCount; rp++) {
                    ceilingPoints[rp] = new Vector3(allCeilingPoints[rp].x, wallUp, allCeilingPoints[rp].y) + floorBaseV;
                    Vector2 uv = allCeilingPoints[rp];
                    if (floorSurface != null) uv = ceilingSurface.CalculateUV(uv);
                    ceilingUvs[rp] = uv;
                    ceilingNorms[rp] = Vector3.down;
                    ceilingTangents[rp] = tangent;
                }

                tris = Poly2TriWrapper.Triangulate(mainShape, false, ceilingCutPoints);
                mesh.AddData(ceilingPoints, ceilingUvs, tris, ceilingNorms, ceilingTangents, ceilingSubmesh);
                if (generateColliders)
                    collider.mesh.AddData(ceilingPoints, ceilingUvs, tris, ceilingNorms, ceilingTangents, 0);

                for (int ob = 0; ob < openingCount; ob++) {
                    VerticalOpening opening = openings[ob];
                    int openingIndex = Array.IndexOf(openings, opening);
                    Vector3 basePosition = openingBounds[openingIndex].center;
                    basePosition.z = basePosition.y;
                    basePosition.y = volume.baseHeight;

                    if (roomOpenings.Contains(opening))//opening used in this floorplan
                    {
                        int externalWallSubmesh = wallSubmesh != -1 ? wallSubmesh : -1;
                        switch (opening.usage) {
                            case VerticalOpening.Usages.Space:
                                if (ceilingCutPoints.Length <= ob) continue;
                                Vector3 ceilingCutUpV = Vector3.up * wallThickness;
                                Vector2[] ceilingCut = ceilingCutPoints[ob];
                                int custSize = ceilingCut.Length;
                                for (int cp = 0; cp < custSize; cp++) {
                                    int indexA = (cp + 1) % custSize;
                                    int indexB = cp;
                                    Vector3 cp0 = new Vector3(ceilingCut[indexA].x, wallUp, ceilingCut[indexA].y) + floorBaseV;
                                    Vector3 cp1 = new Vector3(ceilingCut[indexB].x, wallUp, ceilingCut[indexB].y) + floorBaseV;
                                    Vector3 cp2 = cp0 + ceilingCutUpV;
                                    Vector3 cp3 = cp1 + ceilingCutUpV;
                                    mesh.AddPlane(cp0, cp1, cp2, cp3, ceilingSubmesh);
                                    if (generateColliders)
                                        collider.AddPlane(cp0, cp1, cp2, cp3);
                                }
                                break;

                            case VerticalOpening.Usages.Stairwell:
                                StaircaseGenerator.Generate(mesh, opening, basePosition, volume.floorHeight, actualFloor, externalWallSubmesh, sendCollider);
                                if (volumeFloor == volume.floors - 1 && opening.baseFloor + opening.floors > building.VolumeBaseFloor(volume) + volume.floors - 1 && volume.abovePlanCount == 0)
                                    StaircaseGenerator.GenerateRoofAccess(mesh, opening, basePosition, volume.floorHeight, actualFloor, externalWallSubmesh, sendCollider);
                                break;

                            case VerticalOpening.Usages.Elevator:
                                ElevatorShaftGenerator.Generate(ref mesh, opening, actualFloor, basePosition, volume.floorHeight, externalWallSubmesh, sendCollider);
                                break;
                        }
                    }
                }
            }

            for (int ob = 0; ob < openingCount; ob++) {
                Vector2[] openingShape = openingShapes[ob];
                if (openingShape == null) continue;//opening not used by this floorplan
                if (openingUsedInThisFloor[ob]) continue;//opening already generated
                                                         //seal this opening from the void
                VerticalOpening opening = openings[ob];
                int openingIndex = Array.IndexOf(openings, opening);
                Vector3 basePosition = openingBounds[openingIndex].center;
                basePosition.z = basePosition.y;
                basePosition.y = 0;

                int cutSize = openingShape.Length;
                Vector3 sealingWallUpV = Vector3.up * volume.floorHeight;
                int sealWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceB);
                Vector2[] offsetOpeningShape = QuickPolyOffset.Execute(openingShape, wallThickness);
                for (int cp = 0; cp < cutSize; cp++) {
                    int indexA = (cp + 1) % cutSize;
                    int indexB = cp;
                    Vector2 p0 = opening.usage == VerticalOpening.Usages.Space ? openingShape[indexA] : offsetOpeningShape[indexA];
                    Vector2 p1 = opening.usage == VerticalOpening.Usages.Space ? openingShape[indexB] : offsetOpeningShape[indexB];
                    Vector3 cp0 = new Vector3(p0.x, 0, p0.y) + floorBaseV;
                    Vector3 cp1 = new Vector3(p1.x, 0, p1.y) + floorBaseV;
                    Vector3 cp2 = cp0 + sealingWallUpV;
                    Vector3 cp3 = cp1 + sealingWallUpV;
                    mesh.AddPlane(cp0, cp1, cp2, cp3, sealWallSubmesh);
                    if (generateColliders)
                        collider.AddPlane(cp0, cp1, cp2, cp3);
                }

                switch (opening.usage) {
                    case VerticalOpening.Usages.Space:
                        //nothing to implement
                        break;

                    case VerticalOpening.Usages.Stairwell:
                        //need stairs to connect used floors
                        StaircaseGenerator.GenerateStairs(mesh, opening, basePosition, volume.floorHeight, actualFloor, -1, sendCollider);
                        if (volumeFloor == volume.floors - 1)
                            StaircaseGenerator.GenerateRoofAccess(mesh, opening, basePosition, volume.floorHeight, actualFloor, -1, sendCollider);
                        break;

                    case VerticalOpening.Usages.Elevator:
                        //nothing to implement
                        break;
                }
            }
        }

        private static bool CheckShapeWithinRoom(Vector2[] roomShape, Vector2[] cutShape) {
            int shapeSize = roomShape.Length;
            int cutSize = cutShape.Length;
            for (int s = 0; s < shapeSize; s++) {
                Vector2 a1 = roomShape[s];
                Vector2 a2 = roomShape[(s + 1) % shapeSize];

                for (int c = 0; c < cutSize; c++) {
                    Vector2 b1 = cutShape[c];
                    Vector2 b2 = cutShape[(c + 1) % cutSize];

                    if (FastLineIntersection(a1, a2, b1, b2)) {
                        Debug.DrawLine(JMath.ToV3(a1), JMath.ToV3(a2), Color.red);
                        Debug.DrawLine(JMath.ToV3(b1), JMath.ToV3(b2), Color.magenta);
                        return false;
                    }
                }
            }
            return true;
        }

        public static Vector2[] RoomCut(Vector2[] roomShape, List<Vector2[]> cutShape) {
            Vector2[] cut = cutShape[0];
            int baseCount = roomShape.Length;
            int cutCount = cut.Length;
            int outputCount = baseCount + cutCount;//todo add 2 and align
            Vector2[] output = new Vector2[outputCount];
            FlatBounds bounds = new FlatBounds(cut);
            Vector2 center = bounds.center;
            float nrest = Mathf.Infinity;
            int nearestIndex = 0;
            Array.Reverse(cut);//reverse winding to create cut
            for (int b = 0; b < baseCount; b++) {
                float sqrMag = (roomShape[b] - center).sqrMagnitude;
                if (sqrMag < nrest) {
                    Vector2 a1 = center;
                    Vector2 a2 = roomShape[b];
                    bool intersectsShape = false;
                    for (int x = 0; x < baseCount; x++) {
                        if (b == x) continue;
                        int x2 = (x + 1) % baseCount;
                        if (b == x2) continue;
                        Vector2 b1 = roomShape[x];
                        Vector2 b2 = roomShape[x2];
                        if (FastLineIntersection(a1, a2, b1, b2)) {
                            intersectsShape = true;
                            break;
                        }
                    }
                    if (!intersectsShape) {
                        nearestIndex = b;
                        nrest = sqrMag;
                    }
                    //intersection check
                }
            }

            for (int o = 0; o < outputCount; o++) {
                if (o < nearestIndex)
                    output[o] = roomShape[o];
                else if (o >= nearestIndex && o < nearestIndex + cutCount) {
                    int cutIndex = o - nearestIndex;
                    output[o] = cut[cutIndex];
                }
                else {
                    int finalIndex = o - cutCount;
                    output[o] = roomShape[finalIndex];
                }
            }

            return output;
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        private static bool CCW(Vector2 p1, Vector2 p2, Vector2 p3) {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }
    }
}