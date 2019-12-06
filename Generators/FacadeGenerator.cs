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
	public class FacadeGenerator
	{
		public static void GenerateFacade(FacadeData data, BuildRMesh dmesh, BuildRCollider collider = null)
		{
//		    Debug.Log("******************* "+data.facadeDesign.ToString());
			Vector3 facadeVector = data.baseB - data.baseA;
		    if(facadeVector.magnitude < Mathf.Epsilon)
                return;
			Vector3 facadeDirection = facadeVector.normalized;
			Vector3 facadeNormal = Vector3.Cross(facadeDirection, Vector3.up);
			Vector4 facadeTangent = BuildRMesh.CalculateTangent(facadeDirection);
		    RandomGen rGen = new RandomGen();
            rGen.GenerateNewSeed();
			float wallThickness = data.wallThickness;
			float foundation = data.foundationDepth;
			BuildingMeshTypes meshType = data.meshType;
			BuildingColliderTypes colliderType = data.colliderType;
			int wallSections = 0;
			Vector2 wallSectionSize;
			float facadeLength = 0;
			if (data.isStraight)
			{
				facadeLength = facadeVector.magnitude;
				wallSections = Mathf.FloorToInt(facadeLength / data.minimumWallUnitLength);
				if (wallSections < 1) wallSections = 1;
				wallSectionSize = new Vector2(facadeLength / wallSections, data.floorHeight);
			}
			else
			{
				wallSections = data.anchors.Count - 1;
				if (wallSections < 1) wallSections = 1;
				float sectionWidth = Vector2.Distance(data.anchors[0].vector2, data.anchors[1].vector2);
				wallSectionSize = new Vector2(sectionWidth, data.floorHeight);
			}
            
			Dictionary<WallSection, RawMeshData> generatedSections = new Dictionary<WallSection, RawMeshData>();
			Dictionary<WallSection, RawMeshData> generatedSectionMeshColliders = new Dictionary<WallSection, RawMeshData>();
			Dictionary<WallSection, BuildRCollider.BBox[]> generatedSectionPrimitiveColliders = new Dictionary<WallSection, BuildRCollider.BBox[]>();

			int startFloor = data.startFloor;
//		    Debug.Log("st fl "+startFloor);
//		    Debug.Log("fl ct "+ data.floorCount);
			for (int fl = startFloor; fl < data.floorCount; fl++)
			{
//			    Debug.Log(fl);
                if (data.facadeDesign.randomisationMode == Facade.RandomisationModes.RandomRows) generatedSections.Clear();//recalculate each row

//			    Debug.Log(wallSections);
                for (int s = 0; s < wallSections; s++)
				{
//				    Debug.Log(s);
					WallSection section = data.facadeDesign.GetWallSection(s, fl + data.actualStartFloor, wallSections, data.floorCount);
//				    Debug.Log(section);
					dmesh.submeshLibrary.Add(section);//add the wallsection to the main submesh library
				    RawMeshData generatedSection = null;
				    RawMeshData generatedSectionCollider = null;

					BuildRCollider.BBox[] bboxes = new BuildRCollider.BBox[0];

					if (section == null)
					{
					    GenerationOutput output = GenerationOutput.CreateRawOutput();
					    GenerationOutput outputCollider = null;
						if (colliderType == BuildingColliderTypes.Complex)
						{
                            outputCollider = GenerationOutput.CreateRawOutput();
                        }
                        if (colliderType == BuildingColliderTypes.Primitive)
						{
							BuildRCollider.BBox[] bbox = WallSectionGenerator.Generate(section, wallSectionSize, wallThickness);
							generatedSectionPrimitiveColliders.Add(section, bbox);
					    }
                        WallSectionGenerator.Generate(section, output, wallSectionSize, false, wallThickness, true, outputCollider, dmesh.submeshLibrary);

                        generatedSection = output.raw;
                        if(outputCollider != null)
                            generatedSectionCollider = outputCollider.raw;

					}
					else
					{
						if (generatedSections.ContainsKey(section))
						{
							generatedSection = generatedSections[section];
                            if(generatedSectionMeshColliders.ContainsKey(section))
							    generatedSectionCollider = generatedSectionMeshColliders[section];
						}
						else
						{
						    GenerationOutput output = GenerationOutput.CreateRawOutput();
						    GenerationOutput outputCollider = null;
                            bool cullOpening = data.cullDoors && section.isDoor;
							if (colliderType == BuildingColliderTypes.Complex)
							{
							    outputCollider = GenerationOutput.CreateRawOutput();
                            }
							if (colliderType == BuildingColliderTypes.Primitive)
							{
								BuildRCollider.BBox[] bbox = WallSectionGenerator.Generate(section, wallSectionSize, wallThickness, cullOpening);
								generatedSectionPrimitiveColliders.Add(section, bbox);
						    }
                            WallSectionGenerator.Generate(section, output, wallSectionSize, false, wallThickness, cullOpening, outputCollider, dmesh.submeshLibrary);
                            
							generatedSections.Add(section, output.raw);
						    if (generatedSectionCollider != null)
                                generatedSectionMeshColliders.Add(section, outputCollider.raw);
                           
						    generatedSection = output.raw;
                            if(generatedSectionCollider != null)
						        generatedSectionCollider = outputCollider.raw;
                        }

						if (generatedSectionPrimitiveColliders.ContainsKey(section))
							bboxes = generatedSectionPrimitiveColliders[section];
					}

//				    Debug.Log("data strt" + data.isStraight);
					if (data.isStraight)
					{
						Quaternion meshRot = Quaternion.LookRotation(facadeNormal, Vector3.up);
						Vector3 baseMeshPos = data.baseA + facadeDirection * wallSectionSize.x + Vector3.up * wallSectionSize.y;
						Vector3 wallSectionVector = new Vector3(wallSectionSize.x * s, wallSectionSize.y * fl, 0);
						baseMeshPos += meshRot * wallSectionVector;
						Vector3 meshPos = baseMeshPos + meshRot * -wallSectionSize * 0.5f;

					    Vector2 uvOffset = new Vector2(wallSectionSize.x * s, wallSectionSize.y * fl);
					    Vector2 uvOffsetScaled = uvOffset;
                        if(section != null && section.wallSurface != null)
	                        uvOffsetScaled = CalculateUv(uvOffsetScaled, section.wallSurface);

						//TODO account for the mesh mode of the wall section - custom meshes
						if (meshType == BuildingMeshTypes.Full)
						{
							dmesh.AddData(generatedSection, meshPos, meshRot, Vector3.one, uvOffsetScaled);
						}
						if (collider != null && generatedSectionCollider != null)
						{
							collider.mesh.AddData(generatedSectionCollider, meshPos, meshRot, Vector3.one);
						}
						if (collider != null && bboxes.Length > 0)
							collider.AddBBox(bboxes, meshPos, meshRot);

//					    Debug.Log("foundation");
						if (fl == 0 && foundation > Mathf.Epsilon)
						{
							Vector3 fp3 = baseMeshPos + Vector3.down * wallSectionSize.y;
							Vector3 fp2 = fp3 - facadeDirection * wallSectionSize.x;
							Vector3 fp0 = fp2 + Vector3.down * foundation;
							Vector3 fp1 = fp3 + Vector3.down * foundation;

							if (meshType == BuildingMeshTypes.Full)
							{
								Surface foundationSurface = data.foundationSurface != null ? data.foundationSurface : section.wallSurface;
								int foundationSubmesh = dmesh.submeshLibrary.SubmeshAdd(foundationSurface);//facadeSurfaces.IndexOf(section.wallSurface));
								dmesh.AddPlane(fp0, fp1, fp2, fp3, new Vector2(uvOffset.x, -foundation), new Vector2(uvOffset.x + wallSectionSize.x, 0), -facadeNormal, facadeTangent, foundationSubmesh, foundationSurface);
							}
							if (collider != null && generatedSectionCollider != null)
								collider.mesh.AddPlane(fp0, fp1, fp2, fp3, 0);
						}
					}
					else
					{
						//todo switch - support wall section based curves for now
					    
						Vector3 cp0 = data.anchors[s].vector3XZ;
						cp0.y = data.baseA.y;
						Vector3 cp1 = data.anchors[s + 1].vector3XZ;
						cp1.y = data.baseA.y;
						Vector3 curveVector = cp1 - cp0;
						Vector3 curveDirection = curveVector.normalized;
						Vector3 curveNormal = Vector3.Cross(curveDirection, Vector3.up);
						float actualWidth = curveVector.magnitude;

						Quaternion meshRot = Quaternion.LookRotation(curveNormal, Vector3.up);
						Vector3 meshPos = cp1 + Vector3.up * wallSectionSize.y;
						Vector3 wallSectionVector = new Vector3(0, wallSectionSize.y * fl, 0);
						meshPos += meshRot * wallSectionVector;
						meshPos += meshRot * -new Vector3(actualWidth, wallSectionSize.y, 0) * 0.5f;
						Vector3 meshScale = new Vector3(actualWidth / wallSectionSize.x, 1, 1);

						//Thanks Anthony Cuellar - issue #12 
						Vector2 uvOffset = new Vector2(wallSectionVector.x, wallSectionVector.y + (section.hasOpening ? 0 : wallSectionSize.y / 2f));
						Vector2 uvOffsetScaled = CalculateUv(uvOffset, section.wallSurface);
																			   //TODO account for the mesh mode of the wall section - custom meshes
						if (meshType == BuildingMeshTypes.Full)
							dmesh.AddData(generatedSection, meshPos, meshRot, meshScale, uvOffsetScaled);
						if (collider != null && generatedSectionCollider != null)
							collider.mesh.AddData(generatedSectionCollider, meshPos, meshRot, meshScale);
						if (collider != null && bboxes.Length > 0)
							collider.AddBBox(bboxes, meshPos, meshRot);

//					    Debug.Log("foundation");
                        if (fl == 0 && foundation > Mathf.Epsilon)
						{
							Vector3 fp3 = cp1;
							Vector3 fp2 = fp3 - curveDirection * actualWidth;
							Vector3 fp0 = fp2 + Vector3.down * foundation;
							Vector3 fp1 = fp3 + Vector3.down * foundation;

							if (meshType == BuildingMeshTypes.Full)
							{
								Surface foundationSurface = data.foundationSurface != null ? data.foundationSurface : section.wallSurface;
								int foundationSubmesh = dmesh.submeshLibrary.SubmeshAdd(foundationSurface);//facadeSurfaces.IndexOf(section.wallSurface);
								dmesh.AddPlane(fp0, fp1, fp2, fp3, new Vector2(uvOffset.x, -foundation), new Vector2(uvOffset.x + actualWidth, 0), -curveNormal, facadeTangent, foundationSubmesh, foundationSurface);
							}
							if (collider != null && generatedSectionCollider != null)
								collider.mesh.AddPlane(fp0, fp1, fp2, fp3, 0);
						}
					}
				}

				//string course is completely ignored for a collision
//			    Debug.Log("string");
				if (fl > 0 && data.facadeDesign.stringCourse && meshType == BuildingMeshTypes.Full)//no string course on ground floor
				{
					float baseStringCoursePosition = wallSectionSize.y * fl + wallSectionSize.y * data.facadeDesign.stringCoursePosition;
					Vector3 scBaseUp = baseStringCoursePosition * Vector3.up;
					Vector3 scTopUp = (data.facadeDesign.stringCourseHeight + baseStringCoursePosition) * Vector3.up;
					if (data.isStraight)
					{
						Vector3 scNm = data.facadeDesign.stringCourseDepth * facadeNormal;
						Vector3 p0 = data.baseA;
						Vector3 p1 = data.baseB;
						Vector3 p0o = data.baseA - scNm;
						Vector3 p1o = data.baseB - scNm;
						int submesh = dmesh.submeshLibrary.SubmeshAdd(data.facadeDesign.stringCourseSurface);//data.facadeDesign.stringCourseSurface != null ? facadeSurfaces.IndexOf(data.facadeDesign.stringCourseSurface) : 0;
						Vector2 uvMax = new Vector2(facadeLength, data.facadeDesign.stringCourseHeight);
						dmesh.AddPlane(p0o + scBaseUp, p1o + scBaseUp, p0o + scTopUp, p1o + scTopUp, Vector3.zero, uvMax, -facadeNormal, facadeTangent, submesh, data.facadeDesign.stringCourseSurface);//front
						dmesh.AddPlane(p0 + scBaseUp, p0o + scBaseUp, p0 + scTopUp, p0o + scTopUp, facadeNormal, facadeTangent, submesh);//left
						dmesh.AddPlane(p1o + scBaseUp, p1 + scBaseUp, p1o + scTopUp, p1 + scTopUp, facadeNormal, facadeTangent, submesh);//right
						float facadeAngle = BuildrUtils.CalculateFacadeAngle(facadeDirection);
						dmesh.AddPlaneComplexUp(p0 + scBaseUp, p1 + scBaseUp, p0o + scBaseUp, p1o + scBaseUp, facadeAngle, Vector3.down, facadeTangent, submesh, data.facadeDesign.stringCourseSurface);//bottom
						dmesh.AddPlaneComplexUp(p1 + scTopUp, p0 + scTopUp, p1o + scTopUp, p0o + scTopUp, facadeAngle, Vector3.up, facadeTangent, submesh, data.facadeDesign.stringCourseSurface);//top
					}
					else
					{
						int baseCurvePointCount = data.anchors.Count;//baseCurvepoints.Count;
						Vector3[] interSectionNmls = new Vector3[baseCurvePointCount];
						for (int i = 0; i < baseCurvePointCount - 1; i++)
						{
							Vector3 p0 = data.anchors[i].vector3XZ;//baseCurvepoints[i];
							Vector3 p1 = data.anchors[i + 1].vector3XZ;//baseCurvepoints[i + 1];
							Vector3 p2 = data.anchors[Mathf.Max(i - 1, 0)].vector3XZ;//baseCurvepoints[Mathf.Max(i - 1, 0)];
							interSectionNmls[i] = Vector3.Cross((p1 - p0 + p0 - p2).normalized, Vector3.up);
						}

						for (int i = 0; i < baseCurvePointCount - 1; i++)
						{
							Vector3 p0 = data.anchors[i].vector3XZ;//baseCurvepoints[i];
							Vector3 p1 = data.anchors[i + 1].vector3XZ;//baseCurvepoints[i + 1];
							Vector3 sectionVector = p1 - p0;
							Vector3 sectionDir = sectionVector.normalized;
							Vector3 sectionNml = Vector3.Cross(sectionDir, Vector3.up);
							Vector4 sectionTgnt = BuildRMesh.CalculateTangent(sectionDir);
							Vector3 scNmA = data.facadeDesign.stringCourseDepth * interSectionNmls[i + 0];
							Vector3 scNmB = data.facadeDesign.stringCourseDepth * interSectionNmls[i + 1];
							Vector3 p0o = p0 - scNmA;
							Vector3 p1o = p1 - scNmB;
							int submesh = dmesh.submeshLibrary.SubmeshAdd(data.facadeDesign.stringCourseSurface);//data.facadeDesign.stringCourseSurface != null ? facadeSurfaces.IndexOf(data.facadeDesign.stringCourseSurface) : 0;
							dmesh.AddPlane(p0o + scBaseUp, p1o + scBaseUp, p0o + scTopUp, p1o + scTopUp, sectionNml, sectionTgnt, submesh);
							dmesh.AddPlane(p0 + scBaseUp, p0o + scBaseUp, p0 + scTopUp, p0o + scTopUp, sectionNml, sectionTgnt, submesh);
							dmesh.AddPlane(p1o + scBaseUp, p1 + scBaseUp, p1o + scTopUp, p1 + scTopUp, sectionNml, sectionTgnt, submesh);
							float facadeAngle = BuildrUtils.CalculateFacadeAngle(sectionDir);
							dmesh.AddPlaneComplexUp(p0 + scBaseUp, p1 + scBaseUp, p0o + scBaseUp, p1o + scBaseUp, facadeAngle, Vector3.down, sectionTgnt, submesh, data.facadeDesign.stringCourseSurface);//bottom
							dmesh.AddPlaneComplexUp(p1 + scTopUp, p0 + scTopUp, p1o + scTopUp, p0o + scTopUp, facadeAngle, Vector3.up, sectionTgnt, submesh, data.facadeDesign.stringCourseSurface);//top
						}
					}
				}
			}
		}

		public static void GeneratePrefabs(FacadeData data)
		{
			if (data.meshType != BuildingMeshTypes.Full)
				return;
			Vector3 facadeVector = data.baseB - data.baseA;
			Vector3 facadeDirection = facadeVector.normalized;
			Vector3 facadeNormal = Vector3.Cross(facadeDirection, Vector3.up);
			float wallThickness = data.wallThickness;
			int wallSections = 0;
			Vector2 wallSectionSize;
			float facadeLength = 0;
			if (data.isStraight)
			{
				facadeLength = facadeVector.magnitude;
				wallSections = Mathf.FloorToInt(facadeLength / data.minimumWallUnitLength);
				if (wallSections < 1) wallSections = 1;
				wallSectionSize = new Vector2(facadeLength / wallSections, data.floorHeight);
			}
			else
			{
				wallSections = data.anchors.Count - 1;//baseCurvepoints.Count - 1;
				if (wallSections < 1) wallSections = 1;
				float sectionWidth = Vector2.Distance(data.anchors[0].vector2, data.anchors[1].vector2);//Vector3.Distance(baseCurvepoints[0], baseCurvepoints[1]));
				wallSectionSize = new Vector2(sectionWidth, data.floorHeight);
			}
            
			int startFloor = data.startFloor;
			for (int fl = startFloor; fl < data.floorCount; fl++)
			{
				for (int s = 0; s < wallSections; s++)
				{
					WallSection section = data.facadeDesign.GetWallSection(s, fl, wallSections, data.floorCount);

					if (section == null)
					{
						continue;//nothing to instanciate
					}

					bool cullOpening = data.cullDoors && section.isDoor;
					if (data.isStraight)
					{
						Quaternion meshRot = Quaternion.LookRotation(facadeNormal, Vector3.up);
						Vector3 baseMeshPos = data.baseA + facadeDirection * wallSectionSize.x + Vector3.up * wallSectionSize.y;
						Vector3 wallSectionVector = new Vector3(wallSectionSize.x * s, wallSectionSize.y * fl, 0);
						baseMeshPos += meshRot * wallSectionVector;
						Vector3 meshPos = baseMeshPos + meshRot * -wallSectionSize * 0.5f;

						Matrix4x4 matrix = Matrix4x4.TRS(meshPos, meshRot, Vector3.one);
						WallSectionGenerator.InstantiatePrefabs(data.prefabs, section, wallSectionSize, matrix, wallThickness, cullOpening);
					}
					else
					{
						//todo switch - support wall section based curves for now

						Vector3 cp0 = data.anchors[s].vector3XZ;//baseCurvepoints[s];
						cp0.y = data.baseA.y;
						Vector3 cp1 = data.anchors[s + 1].vector3XZ;//baseCurvepoints[s + 1];
						cp1.y = data.baseA.y;
						Vector3 curveVector = cp1 - cp0;
						Vector3 curveDirection = curveVector.normalized;
						Vector3 curveNormal = Vector3.Cross(curveDirection, Vector3.up);
						float actualWidth = curveVector.magnitude;

						Quaternion meshRot = Quaternion.LookRotation(curveNormal, Vector3.up);
						Vector3 meshPos = cp1 + Vector3.up * wallSectionSize.y;
						Vector3 wallSectionVector = new Vector3(0, wallSectionSize.y * fl, 0);
						meshPos += meshRot * wallSectionVector;
						meshPos += meshRot * -new Vector3(actualWidth, wallSectionSize.y, 0) * 0.5f;
						Vector3 meshScale = new Vector3(actualWidth / wallSectionSize.x, 1, 1);

						Matrix4x4 matrix = Matrix4x4.TRS(meshPos, meshRot, meshScale);
						WallSectionGenerator.InstantiatePrefabs(data.prefabs, section, wallSectionSize, matrix, wallThickness, cullOpening);

					}
				}
			}
		}

		private static Vector2 CalculateUv(Vector2 uv, Surface surface)
		{
			if (surface != null)
				return surface.CalculateUV(uv);
			return uv;
		}

		public struct FacadeData
		{
//			public IBuilding building;
//			public IVolume volume;
			public Vector3 baseA;
			public Vector3 baseB;
			public Vector3 controlA;//point space
			public Vector3 controlB;//point space
			public List<Vector2Int> anchors;
			public bool isStraight;
			public VolumePoint.CurveStyles curveStyle;
			public int floorCount;
			public int startFloor;
			public int actualStartFloor;
			public Facade facadeDesign;
			public Rect faciaUVs;
			public float foundationDepth;//log it here so we can do a volume wide calculation and set it
			public Surface foundationSurface;//log it here so we can do a volume wide calculation and set it

		    public float wallThickness;
		    public float minimumWallUnitLength;
		    public float floorHeight;
		    public int floors;
		    public BuildingMeshTypes meshType;
		    public BuildingColliderTypes colliderType;
		    public bool cullDoors;
		    public GameObject prefabs;
		}
	}
}