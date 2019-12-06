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
    public class SimpleBuildingGenerator
    {
        private const int PIXELS_PER_METER = 100;
        private const int ATLAS_PADDING = 16;
        private static int MAXIMUM_TEXTURESIZE = 1028;
        
        public static void Generate(IBuilding building)
        {
            int numberOfVolumes = building.numberOfPlans;

            for (int v = 0; v < numberOfVolumes; v++)
            {
                IVolume volume = building[v];
                volume.CheckVolume();
                if (!volume.isLegal)
                {
                    GenerateMesh.ClearVisuals(volume);
                    continue;
                }
                int numberOfPoints = volume.numberOfPoints;
                float totalPlanHeight = volume.planHeight;
                Vector3 planUp = totalPlanHeight * Vector3.up;
//                List<Surface> usedFloorplanSurfaces = volume.CalculateSurfaceArray();
                //                VerticalOpening[] volumeOpenings = BuildrUtils.GetOpeningsQuick(building, volume);

                IVisualPart visual = volume.visualPart;
                BuildRMesh dMesh = visual.dynamicMesh;
                BuildRCollider cMesh = visual.colliderMesh;
                BuildingMeshTypes meshType = building.meshType;
                BuildingColliderTypes colliderType = building.colliderType;
                dMesh.Clear();
                dMesh.ignoreSubmeshAssignment = true;
                cMesh.Clear();
                cMesh.TogglePrimitives(colliderType == BuildingColliderTypes.Primitive);
                cMesh.thickness = volume.wallThickness;

                if (meshType == BuildingMeshTypes.None && colliderType == BuildingColliderTypes.None)
                {
                    visual.Clear();
                    return;
                }

                Dictionary<int, List<Vector2Int>> anchorPoints = volume.facadeWallAnchors;
                Texture2D facadeTexture = null;
                Rect[] faciaRectangles = null;
                Rect[] faciaUVs = null;
                Rect roofRect = new Rect();
                Rect roofPixelRect = new Rect();

                #region Exteriors

                if (building.generateExteriors)
                {

                    //                    List<Rect> faciaRectangles = null;
                    faciaRectangles = new Rect[numberOfPoints + 1];//one additional for the roof
                    float foundation = building.IsBaseVolume(volume) ? building.foundationDepth : 0;//set suspended volumes foundation to 0

                    //                    faciaRectangles = new List<Rect>();
                    for (int p = 0; p < numberOfPoints; p++)
                    {
                        if (!volume[p].render)
                            continue;
                        int indexA = p;
                        int indexB = (p < numberOfPoints - 1) ? p + 1 : 0;
                        Vector2Int p0 = volume[indexA].position;
                        Vector2Int p1 = volume[indexB].position;

                        float facadeWidth = Vector2Int.DistanceWorld(p0, p1) * PIXELS_PER_METER;
                        int floorBase = BuildRFacadeUtil.MinimumFloor(building, volume, indexA);

                        int numberOfFloors = volume.floors - floorBase;
                        if (numberOfFloors < 1)//no facade - adjacent facade is taller and covers this one
                            continue;

                        float floorHeight = volume.floorHeight;
                        float facadeHeight = ((volume.floors - floorBase) * floorHeight) * PIXELS_PER_METER;
                        if (facadeHeight < 0)//??
                        {
                            facadeWidth = 0;
                            facadeHeight = 0;
                        }

                        Rect newFacadeRect = new Rect(0, 0, facadeWidth, facadeHeight);
                        faciaRectangles[p] = newFacadeRect;
                        //                        Debug.Log(newFacadeRect);
                        //                        faciaRectangles.Add(newFacadeRect);
                    }

                    roofRect = new Rect(0, 0, volume.bounds.size.x, volume.bounds.size.z);
                    roofPixelRect = new Rect(0, 0, volume.bounds.size.x * PIXELS_PER_METER, volume.bounds.size.z * PIXELS_PER_METER);
                    faciaRectangles[numberOfPoints] = roofPixelRect;
                    //                    Debug.Log(roofRect);

                    int currentWidth = RectanglePack.Pack(faciaRectangles, ATLAS_PADDING);
                    currentWidth = RectanglePack.CheckMaxScale(faciaRectangles, currentWidth, MAXIMUM_TEXTURESIZE);
                    faciaUVs = RectanglePack.ConvertToUVSpace(faciaRectangles, currentWidth);
                    facadeTexture = new Texture2D(currentWidth, currentWidth);


                    //                    float uvOffsetX = 0;
                    int rectIndex = 0;
                    for (int p = 0; p < numberOfPoints; p++)
                    {
                        if (!volume[p].render)
                            continue;

                        Vector3 p0 = volume.BuildingPoint(p);
                        Vector3 p1 = volume.BuildingPoint((p + 1) % numberOfPoints);

                        Vector3 p0u = p0 + planUp;
                        Vector3 p1u = p1 + planUp;
                        Vector3 cw0 = volume.BuildingControlPointA(p);
                        Vector3 cw1 = volume.BuildingControlPointB(p);
                        Facade facade = volume.GetFacade(p);
                        bool isStraight = volume.IsWallStraight(p);

                        Vector3 facadeVector = p1 - p0;
                        Vector3 facadeDirection = facadeVector.normalized;

                        FacadeGenerator.FacadeData fData = new FacadeGenerator.FacadeData();
                        fData.baseA = p0;
                        fData.baseB = p1;
                        fData.controlA = cw0;
                        fData.controlB = cw1;
                        fData.anchors = anchorPoints[p];
                        fData.isStraight = isStraight;
                        fData.curveStyle = volume[p].curveStyle;
                        fData.floorCount = volume.floors;
                        fData.facadeDesign = facade;

                        fData.wallThickness = volume.wallThickness;
                        fData.minimumWallUnitLength = volume.minimumWallUnitLength;
                        fData.floorHeight = volume.floorHeight;
                        fData.floors = volume.floors;
                        fData.meshType = building.meshType;
                        fData.colliderType = building.colliderType;
                        fData.cullDoors = building.cullDoors;
                        fData.prefabs = volume.prefabs;
                        //                        fData.submeshList = usedFloorplanSurfaces;
                        fData.startFloor = BuildRFacadeUtil.MinimumFloor(building, volume, p);

                        if (isStraight)
                        {
                            Vector3 normal = Vector3.Cross(Vector3.up, facadeDirection);
                            Vector4 tangent = BuildRMesh.CalculateTangent(facadeDirection);

                            Vector3 fp2 = p0;
                            Vector3 fp3 = p1;
                            Vector3 fp0 = fp2 + Vector3.down * foundation;
                            Vector3 fp1 = fp3 + Vector3.down * foundation;

                            if (meshType == BuildingMeshTypes.Simple)
                            {
                                //                                if(facade != null)
                                //                                {
                                if (facade != null)
                                    SimpleTextureGenerator.GenerateFacade(fData, facadeTexture, faciaRectangles[rectIndex]);
                                Vector3[] verts = { p0, p1, p0u, p1u };
                                Vector2[] uvs = new Vector2[4];
                                Rect uvRect = faciaUVs[rectIndex];
                                uvs[0] = new Vector2(uvRect.xMin, uvRect.yMin);
                                uvs[1] = new Vector2(uvRect.xMax, uvRect.yMin);
                                uvs[2] = new Vector2(uvRect.xMin, uvRect.yMax);
                                uvs[3] = new Vector2(uvRect.xMax, uvRect.yMax);
                                int[] tris = { 0, 2, 1, 1, 2, 3 };
                                Vector3[] norms = { normal, normal, normal, normal };
                                Vector4[] tangents = { tangent, tangent, tangent, tangent };
                                dMesh.AddData(verts, uvs, tris, norms, tangents, 0);

                                if (foundation > Mathf.Epsilon)
                                    dMesh.AddPlane(fp0, fp1, fp2, fp3, uvs[0], uvs[0], normal, tangent, 0, null);
                            }
                            else
                            {
                                dMesh.AddPlane(p0, p1, p0u, p1u, normal, tangent, 0);

                                if (foundation > Mathf.Epsilon)
                                    dMesh.AddPlane(fp0, fp1, fp2, fp3, normal, tangent, 0);
                            }

                            if(colliderType != BuildingColliderTypes.None)
                            {
                                cMesh.AddPlane(p0, p1, p0u, p1u);
                                if (foundation > Mathf.Epsilon)
                                    cMesh.mesh.AddPlane(fp0, fp1, fp2, fp3, 0);
                            }
                        }
                        else
                        {

                            List<Vector2Int> facadeAnchorPoints = anchorPoints[p];
                            int anchorCount = facadeAnchorPoints.Count;
                            for (int i = 0; i < anchorCount - 1; i++)
                            {
                                Vector3 c0 = facadeAnchorPoints[i].vector3XZ;
                                c0.y = p0.y;
                                Vector3 c1 = facadeAnchorPoints[i + 1].vector3XZ;
                                c1.y = p0.y;
                                Vector3 c2 = c0 + planUp;
                                Vector3 c3 = c1 + planUp;
                                Vector3 sectionDirection = (c1 - c0).normalized;
                                Vector3 normal = Vector3.Cross(Vector3.up, sectionDirection);
                                Vector4 tangent = BuildRMesh.CalculateTangent(sectionDirection);

                                Vector3 fp2 = c0;
                                Vector3 fp3 = c1;
                                Vector3 fp0 = fp2 + Vector3.down * foundation;
                                Vector3 fp1 = fp3 + Vector3.down * foundation;

                                if (meshType == BuildingMeshTypes.Simple)
                                {
                                    if (facade != null)
                                        SimpleTextureGenerator.GenerateFacade(fData, facadeTexture, faciaRectangles[rectIndex]);
                                    Rect uvRect = faciaUVs[rectIndex];
                                    float facadePercentA = i / (float)(anchorCount - 1);
                                    float facadePercentB = (i + 1) / (float)(anchorCount - 1);
                                    float uvxa = uvRect.xMin + uvRect.width * facadePercentA;
                                    float uvxb = uvRect.xMin + uvRect.width * facadePercentB;
                                    Vector3[] verts = { c0, c1, c2, c3 };
                                    Vector2[] uvs = new Vector2[4];
                                    uvs[0] = new Vector2(uvxa, uvRect.yMin);
                                    uvs[1] = new Vector2(uvxb, uvRect.yMin);
                                    uvs[2] = new Vector2(uvxa, uvRect.yMax);
                                    uvs[3] = new Vector2(uvxb, uvRect.yMax);
                                    int[] tris = { 0, 2, 1, 1, 2, 3 };
                                    Vector3[] norms = { normal, normal, normal, normal };
                                    Vector4[] tangents = { tangent, tangent, tangent, tangent };
                                    //                                        Vector2 uvMin = new Vector2(uvOffsetX, 0);
                                    //                                        Vector2 uvMax = new Vector2(uvOffsetX + facadeLength, totalPlanHeight);

                                    dMesh.AddData(verts, uvs, tris, norms, tangents, 0);
                                    //                                    dMesh.AddPlane(p0, p1, p0u, p1u, uvMin, uvMax, normal, tangent, 0);
                                    //todo simple mesh with textured facade
                                    //                                    rectIndex++;
                                    
                                    if (foundation > Mathf.Epsilon)
                                        dMesh.AddPlane(fp0, fp1, fp2, fp3, uvs[0], uvs[0], normal, tangent, 0, null);
                                }
                                else
                                {
                                    dMesh.AddPlane(p0, p1, p0u, p1u, normal, tangent, 0);

                                    if (foundation > Mathf.Epsilon)
                                        dMesh.AddPlane(fp0, fp1, fp2, fp3, normal, tangent, 0);
                                }

                                if(colliderType != BuildingColliderTypes.None)
                                {
                                    cMesh.AddPlane(c0, c1, c2, c3);

                                    if (foundation > Mathf.Epsilon)
                                        cMesh.mesh.AddPlane(fp0, fp1, fp2, fp3, 0);
                                }
                            }
                        }
                        rectIndex++;
                    }
                }

                #endregion

                #region Interiors

                IFloorplan[] floorplans = volume.InteriorFloorplans();
                int floors = volume.floors;
                for (int fl = 0; fl < floors; fl++)
                    floorplans[fl].visualPart.Clear();

                #endregion

                #region Volume Underside Generation

                BuildRVolumeUtil.VolumeShape[] underShapes = BuildRVolumeUtil.GetBottomShape(building, volume);
                int underShapeCount = underShapes.Length;
                //                Debug.Log(underShapeCount);
                float volumeBaseHeight = volume.baseHeight;
                for (int u = 0; u < underShapeCount; u++)
                {
                    //                    Debug.Log(underShapes[u].outer);
                    if (underShapes[u].outer == null) continue;//no underside shape
                    //                    Debug.Log(underShapes[u].outer.Length);

                    Poly2TriWrapper.BMesh(dMesh, volumeBaseHeight, null, 0, underShapes[u].outer, new Rect(0, 0, 0, 0), false, underShapes[u].holes);
                }

                #endregion

                if (building.generateExteriors)
                {
                    Surface roofSurface = volume.roof.mainSurface;
                    if (roofSurface != null)
                    {
                        SimpleTextureGenerator.GenerateTexture(facadeTexture, roofSurface, faciaRectangles[faciaRectangles.Length - 1], roofRect);
                    }
                    RoofGenerator.Generate(building, volume, dMesh, cMesh, faciaUVs[faciaUVs.Length - 1]);
                    visual.GenerateFromDynamicMesh();
                }
                else
                {
                    visual.Clear();
                }

                switch (meshType)
                {
                    case BuildingMeshTypes.Box:
                        visual.materials = new[] { new Material(Shader.Find("Standard")) };
                        break;
                    case BuildingMeshTypes.Simple:
                        facadeTexture.filterMode = FilterMode.Bilinear;
                        facadeTexture.Apply(true, false);
                        Material simpleMaterial = new Material(Shader.Find("Standard"));
                        simpleMaterial.mainTexture = facadeTexture;
                        visual.materials = new[] { simpleMaterial };
                        break;
                }

            }
        }
    }
}