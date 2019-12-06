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
    public class FullMesh
    {
        public static void Generate(IBuilding building)
        {
            int numberOfVolumes = building.numberOfVolumes;

//            Debug.Log("n vol "+numberOfVolumes);
            for(int v = 0; v < numberOfVolumes; v++)
            {
                IVolume volume = building[v];
                volume.CheckVolume();
                if(!volume.isLegal)
                {
                    GenerateMesh.ClearVisuals(volume);
                    continue;
                }

                int numberOfPoints = volume.numberOfPoints;
                float totalPlanHeight = volume.planHeight;
                Vector3 planUp = totalPlanHeight * Vector3.up;
                VerticalOpening[] volumeOpenings = BuildrUtils.GetOpeningsQuick(building, volume);
                float foundation = building.IsBaseVolume(volume) ? building.foundationDepth : 0;//set suspended volumes foundation to 0
                IVisualPart visual = volume.visualPart;
                BuildRMesh dMesh = visual.dynamicMesh;
                BuildRCollider cMesh = visual.colliderMesh;
                BuildingMeshTypes meshType = building.meshType;
                BuildingColliderTypes colliderType = building.colliderType;
                dMesh.Clear();
                cMesh.Clear();
                cMesh.TogglePrimitives(colliderType == BuildingColliderTypes.Primitive);
                cMesh.thickness = volume.wallThickness;
                if(colliderType == BuildingColliderTypes.None) cMesh = null;
                Transform[] prefabs = volume.prefabs.GetComponentsInChildren<Transform>();
                int prefabCount = prefabs.Length;
                for(int p = 0; p < prefabCount; p++)
                {
                    if(prefabs[p] == volume.prefabs) continue;
                    if(prefabs[p] == null) continue;//gone already man
#if UNITY_EDITOR
                    Object.DestroyImmediate(prefabs[p].gameObject);
#else
                    Object.Destroy(prefabs[p].gameObject);
#endif
                }

                Dictionary<int, List<Vector2Int>> anchorPoints = volume.facadeWallAnchors;
                Texture2D facadeTexture = null;

                #region Exteriors

//                Debug.Log("ext");
                if(building.generateExteriors)
                {
                    for(int p = 0; p < numberOfPoints; p++)
                    {
                        if(!volume[p].render)
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
                        float facadeLength = facadeVector.magnitude;
                        if(facadeLength < Mathf.Epsilon)
                            continue;

//                        Debug.Log("flength "+facadeLength);
                        if(facade == null || colliderType == BuildingColliderTypes.Simple)
                        {
//                            Debug.Log("simple");
                            if(isStraight)
                            {
                                Vector3 normal = Vector3.Cross(Vector3.up, facadeDirection);
                                Vector4 tangent = BuildRMesh.CalculateTangent(facadeDirection);
                                if(facade == null)
                                {
                                    dMesh.AddPlane(p0, p1, p0u, p1u, normal, tangent, 0);
                                }

                                if(colliderType != BuildingColliderTypes.None)
                                {
                                    cMesh.AddPlane(p0, p1, p0u, p1u);
                                }

                                if(foundation > Mathf.Epsilon)
                                {
                                    Vector3 fp2 = p0;
                                    Vector3 fp3 = p1;
                                    Vector3 fp0 = fp2 + Vector3.down * foundation;
                                    Vector3 fp1 = fp3 + Vector3.down * foundation;
                                    if(facade == null)
                                    {
                                        Surface foundationSurface = building.foundationSurface != null ? building.foundationSurface : null;
                                        int foundationSubmesh = dMesh.submeshLibrary.SubmeshAdd(foundationSurface);
                                        Vector2 uxmax = new Vector2(Vector3.Distance(p0, p1), foundation);
                                        dMesh.AddPlane(fp0, fp1, fp2, fp3, Vector2.zero, uxmax, normal, tangent, foundationSubmesh, foundationSurface);
                                    }

                                    if(colliderType != BuildingColliderTypes.None)
                                        cMesh.mesh.AddPlane(fp0, fp1, fp2, fp3, 0);
                                }
                            }
                            else
                            {
                                List<Vector2Int> facadeAnchorPoints = anchorPoints[p];
                                int anchorCount = facadeAnchorPoints.Count;
                                for(int i = 0; i < anchorCount - 1; i++)
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
                                    if(facade == null) dMesh.AddPlane(c0, c1, c2, c3, normal, tangent, 0);
                                    if(colliderType != BuildingColliderTypes.None) cMesh.AddPlane(c0, c1, c2, c3);
                                    if(foundation > Mathf.Epsilon)
                                    {
                                        Vector3 fp2 = c0;
                                        Vector3 fp3 = c1;
                                        Vector3 fp0 = fp2 + Vector3.down * foundation;
                                        Vector3 fp1 = fp3 + Vector3.down * foundation;

                                        if(facade == null)
                                        {
                                            Surface foundationSurface = building.foundationSurface != null ? building.foundationSurface : null;
                                            int foundationSubmesh = dMesh.submeshLibrary.SubmeshAdd(foundationSurface);
                                            Vector2 uxmax = new Vector2(Vector3.Distance(c0, c1), foundation);
                                            dMesh.AddPlane(fp0, fp1, fp2, fp3, Vector2.zero, uxmax, normal, tangent, foundationSubmesh, foundationSurface);
                                        }

                                        if(colliderType != BuildingColliderTypes.None)
                                            cMesh.AddPlane(fp0, fp1, fp2, fp3);
                                    }
                                }
                            }

//                                                        Debug.Log("Generate facade " + p + " " + dMesh.vertexCount  );
                        }

//                        Debug.Log("fac "+p);
                        if(facade != null && (meshType == BuildingMeshTypes.Full || colliderType == BuildingColliderTypes.Primitive || colliderType == BuildingColliderTypes.Complex))
                        {
                            //generate the facade
//                            Debug.Log("full");
                            FacadeGenerator.FacadeData fData = new FacadeGenerator.FacadeData();
                            //                            fData.building = building;
                            //                            fData.volume = volume;
                            fData.baseA = p0;
                            fData.baseB = p1;
                            fData.controlA = cw0;
                            fData.controlB = cw1;
                            fData.anchors = anchorPoints[p];
                            fData.isStraight = isStraight;
                            fData.curveStyle = volume[p].curveStyle;
                            fData.floorCount = volume.floors;
                            fData.facadeDesign = facade;
                            //                            fData.submeshList = usedFloorplanSurfaces;
                            fData.startFloor = BuildRFacadeUtil.MinimumFloor(building, volume, p);
                            fData.actualStartFloor = building.VolumeBaseFloor(volume);
                            fData.foundationDepth = foundation;
                            fData.foundationSurface = building.foundationSurface;
                            fData.wallThickness = volume.wallThickness;
                            fData.minimumWallUnitLength = volume.minimumWallUnitLength;
                            fData.floorHeight = volume.floorHeight;
                            fData.floors = volume.floors;
                            fData.meshType = building.meshType;
                            fData.colliderType = building.colliderType;
                            fData.cullDoors = building.cullDoors;
                            fData.prefabs = volume.prefabs;

//                            Debug.Log("mesh");
                            FacadeGenerator.GenerateFacade(fData, dMesh, cMesh);
//                            Debug.Log("pref");
                            FacadeGenerator.GeneratePrefabs(fData);
//                                                        Debug.Log("Generate facade "+p+" "+dMesh.vertexCount);
                        }
                    }
                }

                #endregion

                #region Interiors

//                Debug.Log("int");
                bool generateInteriors = building.generateInteriors && meshType == BuildingMeshTypes.Full;
                if(generateInteriors)
                {
                    int floors = volume.floors;
                    IFloorplan[] floorplans = volume.InteriorFloorplans();
                    for(int fl = 0; fl < floors; fl++)
                    {
                        IFloorplan floorplan = floorplans[fl];
                        IVisualPart floorVisual = floorplan.visualPart;
                        BuildRMesh flMesh = floorVisual.dynamicMesh;
                        BuildRCollider flCollider = floorVisual.colliderMesh;
                        flMesh.Clear();
                        flCollider.Clear();
                        flCollider.TogglePrimitives(colliderType == BuildingColliderTypes.Primitive);
                        FloorplanGenerator.Generate(building, volume, floorplans[fl], fl, volumeOpenings, flMesh, flCollider);
                        floorVisual.GenerateFromDynamicMesh();
                        floorplan.transform.localPosition = Vector3.up * (fl * volume.floorHeight);
                        floorVisual.transform.localPosition = Vector3.zero;//
                        floorVisual.transform.localRotation = Quaternion.identity;
                    }
                }
                else
                {
                    IFloorplan[] floorplans = volume.InteriorFloorplans();
                    int floors = floorplans.Length;
                    for(int fl = 0; fl < floors; fl++)
                        floorplans[fl].visualPart.Clear();
                }

                #endregion

                #region Volume Underside Generation

//                Debug.Log("und");
                BuildRVolumeUtil.VolumeShape[] underShapes = BuildRVolumeUtil.GetBottomShape(building, volume);
                int underShapeCount = underShapes.Length;
                float volumeBaseHeight = volume.baseHeight - building.foundationDepth;
                for(int u = 0; u < underShapeCount; u++)
                {
                    if(underShapes[u].outer == null) continue;//no underside shape
                    int undersideSubmesh = dMesh.submeshLibrary.SubmeshAdd(volume.undersideSurafce);
                    Poly2TriWrapper.BMesh(dMesh, volumeBaseHeight, null, undersideSubmesh, underShapes[u].outer, new Rect(0, 0, 0, 0), false, underShapes[u].holes);
                }

                #endregion

//                Debug.Log("roof");
                if(building.generateExteriors)
                {
                    RoofGenerator.Generate(building, volume, dMesh, cMesh);
                    visual.GenerateFromDynamicMesh();
                }
                else
                {
                    visual.Clear();
                }

//                Debug.Log("mat");
                switch(meshType)
                {
                    case BuildingMeshTypes.None:
                        visual.materials = null;
                        break;
                    case BuildingMeshTypes.Box:
                        visual.materials = new[]{new Material(Shader.Find("Standard"))};
                        break;
                    case BuildingMeshTypes.Simple:
                        facadeTexture.filterMode = FilterMode.Bilinear;
                        facadeTexture.Apply(true, false);
                        Material simpleMaterial = new Material(Shader.Find("Standard"));
                        simpleMaterial.mainTexture = facadeTexture;
                        visual.materials = new[]{simpleMaterial};
                        break;
                    case BuildingMeshTypes.Full:
                        visual.materials = dMesh.materials.ToArray();
                        break;
                }
            }
        }
    }
}