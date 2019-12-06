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

namespace BuildR2
{
	public class StaircaseGenerator
	{
		public static void Generate(BuildRMesh mesh, VerticalOpening opening, Vector3 basePosition, float height, int floor, int wallSubmesh = -1, BuildRCollider collider = null)
		{
			if (collider != null)
				collider.thickness = VerticalOpening.WALL_THICKNESS;
			GenerateWall(mesh, opening, basePosition, height, wallSubmesh, collider);
			GenerateStairs(mesh, opening, basePosition, height, floor, wallSubmesh, collider);
		}

		public static void GenerateWall(BuildRMesh mesh, VerticalOpening opening, Vector3 basePosition, float height, int wallSubmesh = -1, BuildRCollider collider = null)
		{
			float stairWidth = 0.70f;//todo / calculate
			float wallThickness = VerticalOpening.WALL_THICKNESS;
			float doorWidth = 1.3f;
			float doorHeight = 2.04f;
			bool generateColldier = collider != null;
			//            bool generateMeshCollider = generateColldier && !collider.usingPrimitives;

			SubmeshLibrary submeshLibrary = mesh.submeshLibrary;
			int externalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceA);
			int internalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceB);
			int doorFrameSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceC);
            
			//base positions
			Quaternion rotation = Quaternion.Euler(0, opening.rotation, 0);
			Vector2Int openingSize = opening.size;
			Vector3 b0 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
			Vector3 b1 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
			Vector3 b2 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);
			Vector3 b3 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);

			//inner points
			Vector3 b0i = b0 + rotation * new Vector3(1, 0, 1) * wallThickness;
			Vector3 b1i = b1 + rotation * new Vector3(-1, 0, 1) * wallThickness;
			Vector3 b2i = b2 + rotation * new Vector3(1, 0, -1) * wallThickness;
			Vector3 b3i = b3 + rotation * new Vector3(-1, 0, -1) * wallThickness;

			Vector3 escalationFlatDir = (b2i - b0i).normalized;
			Vector3 wallUp = Vector3.up * height;

			//external walls
			Vector2 uv20_min = new Vector2(0, 0);
			Vector2 uv20_max = new Vector2(opening.size.vy, height);
			Vector3 normal02 = rotation * Vector3.left;
			Vector4 tangent02 = BuildRMesh.CalculateTangent((b0 - b2).normalized);
			mesh.AddPlane(b2, b0, b2 + wallUp, b0 + wallUp, uv20_min, uv20_max, normal02, tangent02, externalWallSubmesh, opening.surfaceA);

			Vector2 uv32_min = new Vector2(0, 0);
			Vector2 uv32_max = new Vector2(opening.size.vx, height);
			Vector3 normal32 = rotation * Vector3.forward;
			Vector4 tangent32 = BuildRMesh.CalculateTangent((b2 - b3).normalized);
			mesh.AddPlane(b3, b2, b3 + wallUp, b2 + wallUp, uv32_min, uv32_max, normal32, tangent32, externalWallSubmesh, opening.surfaceA);

			Vector2 uv13_min = new Vector2(0, 0);
			Vector2 uv13_max = new Vector2(opening.size.vy, height);
			Vector3 normal13 = rotation * Vector3.right;
			Vector4 tangent13 = BuildRMesh.CalculateTangent((b3 - b1).normalized);
			mesh.AddPlane(b1, b3, b1 + wallUp, b3 + wallUp, uv13_min, uv13_max, normal13, tangent13, externalWallSubmesh, opening.surfaceA);

			//internal walls
			Vector2 uv20i_min = new Vector2(wallThickness, 0);
			Vector2 uv20i_max = new Vector2(opening.size.vy - wallThickness, height);
			Vector3 normal02i = rotation * Vector3.right;
			Vector4 tangent02i = BuildRMesh.CalculateTangent((b2 - b0).normalized);
			mesh.AddPlane(b0i, b2i, b0i + wallUp, b2i + wallUp, uv20i_min, uv20i_max, normal02i, tangent02i, internalWallSubmesh, opening.surfaceB);

			Vector2 uv32i_min = new Vector2(0, 0);
			Vector2 uv32i_max = new Vector2(opening.size.vx - wallThickness, height);
			Vector3 normal32i = rotation * Vector3.back;
			Vector4 tangent32i = BuildRMesh.CalculateTangent((b3 - b2).normalized);
			mesh.AddPlane(b2i, b3i, b2i + wallUp, b3i + wallUp, uv32i_min, uv32i_max, normal32i, tangent32i, internalWallSubmesh, opening.surfaceB);

			Vector2 uv13i_min = new Vector2(0, 0);
			Vector2 uv13i_max = new Vector2(opening.size.vy - wallThickness, height);
			Vector3 normal13i = rotation * Vector3.left;
			Vector4 tangent13i = BuildRMesh.CalculateTangent((b1 - b3).normalized);
			mesh.AddPlane(b3i, b1i, b3i + wallUp, b1i + wallUp, uv13i_min, uv13i_max, normal13i, tangent13i, internalWallSubmesh, opening.surfaceB);

			if (generateColldier)
			{
				collider.AddPlane(b2, b0, b2 + wallUp, b0 + wallUp);
				collider.AddPlane(b3, b2, b3 + wallUp, b2 + wallUp);
				collider.AddPlane(b1, b3, b1 + wallUp, b3 + wallUp);

				if (!collider.usingPrimitives)
				{
					collider.mesh.AddPlane(b0i, b2i, b0i + wallUp, b2i + wallUp, 0);
					collider.mesh.AddPlane(b2i, b3i, b2i + wallUp, b3i + wallUp, 0);
					collider.mesh.AddPlane(b3i, b1i, b3i + wallUp, b1i + wallUp, 0);
				}
			}


			//door wall

			float internalWallLength = openingSize.vx - (wallThickness * 2f);
			float lerpA = Mathf.Max(stairWidth - doorWidth, 0.05f) / internalWallLength;
			float lerpB = (Mathf.Max(stairWidth - doorWidth, 0.05f) + doorWidth) / internalWallLength;

			Vector2 uvd_b0 = new Vector2(0, 0);
			Vector2 uvd_b1 = new Vector2(opening.size.vx * lerpA, 0);
			Vector2 uvd_b2 = new Vector2(opening.size.vx * lerpB, 0);
			Vector2 uvd_b3 = new Vector2(opening.size.vx, 0);
			Vector2 uvd_m0 = new Vector2(uvd_b0.x, doorHeight);
			Vector2 uvd_m1 = new Vector2(uvd_b1.x, doorHeight);
			Vector2 uvd_m3 = new Vector2(uvd_b3.x, doorHeight);
			Vector2 uvd_t0 = new Vector2(0, height);
			Vector2 uvd_t3 = new Vector2(opening.size.vx, height);

			//internal

			Vector3 bd0i = Vector3.Lerp(b0i, b1i, lerpA);
			Vector3 bd1i = Vector3.Lerp(b0i, b1i, lerpB);
			Vector3 normal01i = rotation * Vector3.forward;
			Vector4 tangent01i = BuildRMesh.CalculateTangent((b0 - b1).normalized);

			Vector3 doorUp = Vector3.up * doorHeight;
			//Right side
			mesh.AddPlane(bd0i, b0i, bd0i + doorUp, b0i + doorUp, uvd_b0, uvd_m1, normal01i, tangent01i, internalWallSubmesh, opening.surfaceB);
			//left side
			mesh.AddPlane(b1i, bd1i, b1i + doorUp, bd1i + doorUp, uvd_b2, uvd_m3, normal01i, tangent01i, internalWallSubmesh, opening.surfaceB);
			//top
			mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUp, b0i + wallUp, uvd_m3, uvd_t0, normal01i, tangent01i, internalWallSubmesh, opening.surfaceB);

			//external
			Vector3 doorOut = -escalationFlatDir * wallThickness;
			Vector3 normal01 = rotation * Vector3.back;
			Vector4 tangent01 = BuildRMesh.CalculateTangent((b1 - b0).normalized);
			//left
			mesh.AddPlane(b0, bd0i + doorOut, b0 + doorUp, bd0i + doorOut + doorUp, uvd_b0, uvd_m1, normal01, tangent01, externalWallSubmesh, opening.surfaceA);
			//right
			mesh.AddPlane(bd1i + doorOut, b1, bd1i + doorOut + doorUp, b1 + doorUp, uvd_b2, uvd_m3, normal01, tangent01, externalWallSubmesh, opening.surfaceA);
			//top
			mesh.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUp, b1 + wallUp, uvd_m0, uvd_t3, normal01, tangent01, externalWallSubmesh, opening.surfaceA);

			//frame
			//floor
			mesh.AddPlane(bd1i, bd0i, bd1i + doorOut, bd0i + doorOut, externalWallSubmesh);
			//left
			mesh.AddPlane(bd0i + doorOut, bd0i, bd0i + doorOut + doorUp, bd0i + doorUp, doorFrameSubmesh);
			//right
			mesh.AddPlane(bd1i, bd1i + doorOut, bd1i + doorUp, bd1i + doorOut + doorUp, doorFrameSubmesh);
			//top
			mesh.AddPlane(bd0i + doorUp, bd1i + doorUp, bd0i + doorOut + doorUp, bd1i + doorOut + doorUp, doorFrameSubmesh);

			if (generateColldier)
			{
				collider.AddPlane(b0, bd0i + doorOut, b0 + doorUp, bd0i + doorOut + doorUp);
				collider.AddPlane(bd1i + doorOut, b1, bd1i + doorOut + doorUp, b1 + doorUp);
				collider.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUp, b1 + wallUp);

				if (!collider.usingPrimitives)
				{
					collider.mesh.AddPlane(bd0i, b0i, bd0i + doorUp, b0i + doorUp, 0);
					collider.mesh.AddPlane(b1i, bd1i, b1i + doorUp, bd1i + doorUp, 0);
					collider.mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUp, b0i + wallUp, 0);

					collider.mesh.AddPlane(bd1i, bd0i, bd1i + doorOut, bd0i + doorOut, 0);
					collider.mesh.AddPlane(bd0i + doorOut, bd0i, bd0i + doorOut + doorUp, bd0i + doorUp, 0);
					collider.mesh.AddPlane(bd1i, bd1i + doorOut, bd1i + doorUp, bd1i + doorOut + doorUp, 0);
					collider.mesh.AddPlane(bd0i + doorUp, bd1i + doorUp, bd0i + doorOut + doorUp, bd1i + doorOut + doorUp, 0);
				}
			}
		}

		public static void GenerateStairs(BuildRMesh mesh, VerticalOpening opening, Vector3 basePosition, float height, int floor, int wallSubmesh = -1, BuildRCollider collider = null)
		{
			bool stepped = true;//todo
			float minimumWidth = 0.9f;//UK standard
			float stepHeight = 0.22f;
			float wallThickness = VerticalOpening.WALL_THICKNESS;
			bool generateColldier = collider != null;

			float minimumRunLength = 0.25f;
			float maximumRiserHeight = 0.2f;

			int internalWallSubmesh = mesh.submeshLibrary.SubmeshAdd(opening.surfaceB);
			int internalFloorSubmesh = mesh.submeshLibrary.SubmeshAdd(opening.surfaceD);

			bool isBottomFloor = opening.baseFloor == floor;
			bool isTopFloor = opening.baseFloor + opening.floors == floor;
			//            Debug.Log((opening.baseFloor + opening.floors - 1) +" "+ floor);

			//base positions
			Quaternion rotation = Quaternion.Euler(0, opening.rotation, 0);
			Vector2Int openingSize = opening.size;
			Vector3 b0 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
			Vector3 b1 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
			Vector3 b2 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);
			Vector3 b3 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);

			//inner points
			Vector3 b0i = b0 + rotation * new Vector3(1, 0, 1) * wallThickness;
			Vector3 b1i = b1 + rotation * new Vector3(-1, 0, 1) * wallThickness;
			Vector3 b2i = b2 + rotation * new Vector3(1, 0, -1) * wallThickness;
			Vector3 b3i = b3 + rotation * new Vector3(-1, 0, -1) * wallThickness;

			Vector2 internalSize = new Vector2(openingSize.vx - wallThickness * 2, openingSize.vy - wallThickness * 2);
			float stairWidthFromX = internalSize.x * 0.5f;
			float stairWidthFromY = internalSize.y - Mathf.Ceil(height / maximumRiserHeight) * minimumRunLength;

			float useLandingWidth = (stairWidthFromX + stairWidthFromY) * 0.5f;
			useLandingWidth = Mathf.Clamp(useLandingWidth, minimumWidth, opening.stairWidth);//Mathf.Max(stairWidth, internalSize.x * 0.5f));
			float useStairWidth = Mathf.Clamp(opening.stairWidth, minimumWidth, stairWidthFromX);


			float stairRun = internalSize.y - (useLandingWidth * 2);


			Vector3 escalationFlatDir = (b2i - b0i).normalized;
			Vector3 escalationRight = (b1i - b0i).normalized;
			Vector3 escalationVector = new Vector3(stairRun * escalationFlatDir.x, height * 0.5f, stairRun * escalationFlatDir.z);
			Vector3 escalationDirection = escalationVector.normalized;
			float escalationHypotenuse = escalationVector.magnitude;
			int numberOfSteps = Mathf.CeilToInt((height) / stepHeight);

			Vector3 escalationVectorB = new Vector3(stairRun * -escalationFlatDir.x, height * 0.5f, stairRun * -escalationFlatDir.z);
			Vector3 escalationDirectionB = escalationVectorB.normalized;

			Vector3 landingDrop = Vector3.down * wallThickness;
			Vector4 rightTangent = BuildRMesh.CalculateTangent(escalationRight);

			//lower landing
			if (!isBottomFloor)
			{
				Vector3 l0 = b0i;
				Vector3 l1 = b1i;
				Vector3 l2 = b0i + escalationFlatDir * useLandingWidth;
				Vector3 l3 = b1i + escalationFlatDir * useLandingWidth;

				Vector2 maxUVTop = new Vector2(internalSize.x, useLandingWidth);
				Vector2 maxUVSide = new Vector2(internalSize.x, stepHeight);
				//top
				mesh.AddPlane(l0, l1, l2, l3, Vector3.zero, maxUVTop, Vector3.up, rightTangent, internalFloorSubmesh, opening.surfaceD);
				//bottom
				mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, Vector3.zero, maxUVTop, Vector3.down, rightTangent, internalWallSubmesh, opening.surfaceB);
				//front
				mesh.AddPlane(l2, l3, l2 + landingDrop, l3 + landingDrop, escalationFlatDir, maxUVSide, Vector3.up, rightTangent, internalWallSubmesh, opening.surfaceB);

				if (generateColldier)
				{
					collider.mesh.AddPlane(l0, l1, l2, l3, 0);
					collider.mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, 0);
					collider.mesh.AddPlane(l2, l3, l2 + landingDrop, l3 + landingDrop, 0);
				}
			}

			if (!isTopFloor)
			{
				//mid landing

				if (true)//half landed
				{
					Vector3 up = Vector3.up * height * 0.5f;
					Vector3 l0 = b2i - escalationFlatDir * useLandingWidth + up;
					Vector3 l1 = b3i - escalationFlatDir * useLandingWidth + up;
					Vector3 l2 = b2i + up;
					Vector3 l3 = b3i + up;

					Vector2 maxUVTop = new Vector2(internalSize.x, useLandingWidth);
					Vector2 maxUVSide = new Vector2(internalSize.x, useLandingWidth);
					//top
					mesh.AddPlane(l0, l1, l2, l3, Vector3.zero, maxUVTop, Vector3.up, rightTangent, internalFloorSubmesh, opening.surfaceD);
					//bottom
					mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, Vector3.zero, maxUVTop, Vector3.down, rightTangent, internalWallSubmesh, opening.surfaceB);
					//front
					mesh.AddPlane(l1, l0, l1 + landingDrop, l0 + landingDrop, Vector3.zero, maxUVSide, -escalationFlatDir, rightTangent, internalWallSubmesh, opening.surfaceB);

					if (generateColldier)
					{
						collider.mesh.AddPlane(l0, l1, l2, l3, 0);
						collider.mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, 0);
						collider.mesh.AddPlane(l1, l0, l1 + landingDrop, l0 + landingDrop, 0);
					}
				}

				Vector3 flightABaseOutside = b0i + escalationFlatDir * useLandingWidth;
				Vector3 flightABaseInside = flightABaseOutside + escalationRight * useStairWidth;
				Vector3 flightATopOutside = flightABaseOutside + escalationDirection * escalationHypotenuse;
				Vector3 flightATopInside = flightABaseInside + escalationDirection * escalationHypotenuse;
				float dropThickness = wallThickness;//Mathf.Sin(Mathf.Atan2(height, stairRun)) * wallThickness;

				Vector3 flightABaseOutsideDrop = flightABaseOutside + Vector3.down * dropThickness;
				Vector3 flightABaseInsideDrop = flightABaseInside + Vector3.down * dropThickness;
				Vector3 flightATopOutsideDrop = flightATopOutside + Vector3.down * dropThickness;
				Vector3 flightATopInsideDrop = flightATopInside + Vector3.down * dropThickness;

				Vector3 flightBBaseOutside = b3i - escalationFlatDir * useLandingWidth + Vector3.up * height * 0.5f;
				Vector3 flightBBaseInside = flightBBaseOutside - escalationRight * useStairWidth;
				Vector3 flightBTopOutside = flightBBaseOutside + escalationDirectionB * escalationHypotenuse;
				Vector3 flightBTopInside = flightBBaseInside + escalationDirectionB * escalationHypotenuse;

				Vector3 flightBBaseOutsideDrop = flightBBaseOutside + Vector3.down * dropThickness;
				Vector3 flightBBaseInsideDrop = flightBBaseInside + Vector3.down * dropThickness;
				Vector3 flightBTopOutsideDrop = flightBTopOutside + Vector3.down * dropThickness;
				Vector3 flightBTopInsideDrop = flightBTopInside + Vector3.down * dropThickness;

				if (generateColldier)
				{
					collider.mesh.AddPlane(flightABaseOutside, flightABaseInside, flightATopOutside, flightATopInside, 0);
					collider.mesh.AddPlane(flightABaseInsideDrop, flightATopInsideDrop, flightABaseInside, flightATopInside, 0);
					collider.mesh.AddPlane(flightABaseInsideDrop, flightABaseOutsideDrop, flightATopInsideDrop, flightATopOutsideDrop, 0);
					collider.mesh.AddPlane(flightBBaseOutside, flightBBaseInside, flightBTopOutside, flightBTopInside, 0);
					collider.mesh.AddPlane(flightBBaseInsideDrop, flightBTopInsideDrop, flightBBaseInside, flightBTopInside, 0);
					collider.mesh.AddPlane(flightBBaseInsideDrop, flightBBaseOutsideDrop, flightBTopInsideDrop, flightBTopOutsideDrop, 0);
				}

				if (stepped)//todo, flat generation
				{
					float stepDepth = stairRun / (numberOfSteps);
					float skipStep = (stepDepth / (numberOfSteps - 1));
					stepDepth += skipStep;
					float stepRiser = height / numberOfSteps / 2;

					Vector2 stepUvTopMin = new Vector2(0, 0);
					Vector2 stepUvTopMax = new Vector2(useStairWidth, stepDepth);
					Vector2 stepUvSideMin = new Vector2(0, 0);
					Vector2 stepUvSideMax = new Vector2(useStairWidth, stepRiser);

					//flight one
					float lerpIncrement = 1.0f / (numberOfSteps - 1);
					Vector3 flightATopOutsideStep = flightATopOutside + Vector3.down * stepHeight * 0.5f;
					Vector3 flightATopInsideStep = flightATopInside + Vector3.down * stepHeight * 0.5f;
					for (int s = 0; s < numberOfSteps - 1; s++)
					{
						float lerpValueAA = lerpIncrement * s;
						Vector3 s0 = Vector3.Lerp(flightABaseOutside, flightATopOutsideStep, lerpValueAA);
						Vector3 s1 = Vector3.Lerp(flightABaseInside, flightATopInsideStep, lerpValueAA);
						Vector3 s2 = s0 + Vector3.up * stepRiser;
						Vector3 s3 = s1 + Vector3.up * stepRiser;
						Vector3 s4 = s2 + escalationFlatDir.normalized * stepDepth;
						Vector3 s5 = s3 + escalationFlatDir.normalized * stepDepth;

						//front
						mesh.AddPlane(s0, s1, s2, s3, stepUvSideMin, stepUvSideMax, -escalationFlatDir, rightTangent, internalWallSubmesh, opening.surfaceB);
						//top
						mesh.AddPlane(s2, s3, s4, s5, stepUvTopMin, stepUvTopMax, Vector3.up, rightTangent, internalFloorSubmesh, opening.surfaceD);
						//sides

						float lerpValueB = lerpIncrement * s;
						Vector3 normal = escalationRight;
						Vector3[] normals = { normal, normal, normal, normal };
						Vector4 tangent = BuildRMesh.CalculateTangent(escalationFlatDir);
						Vector4[] tangents = { tangent, tangent, tangent, tangent };
						Vector3 s8 = Vector3.Lerp(flightABaseInsideDrop, flightATopInsideDrop, lerpValueB);
						Vector3 s9 = Vector3.Lerp(flightABaseInsideDrop, flightATopInsideDrop, lerpValueB + lerpIncrement);
						Vector2 uv5, uv3, uv8, uv9;
						if (opening.surfaceB != null)
						{
							uv5 = opening.surfaceB.CalculateUV(new Vector2(s5.z, s5.y));
							uv3 = opening.surfaceB.CalculateUV(new Vector2(s3.z, s3.y));
							uv8 = opening.surfaceB.CalculateUV(new Vector2(s8.z, s8.y));
							uv9 = opening.surfaceB.CalculateUV(new Vector2(s9.z, s9.y));
						}
						else
						{
							uv5 = new Vector2();
							uv3 = new Vector2();
							uv8 = new Vector2();
							uv9 = new Vector2();
						}

						mesh.AddData(new[] { s3, s5, s8, s9 }, new[] { uv3, uv5, uv8, uv9 }, new[] { 0, 1, 2, 2, 1, 3 }, normals, tangents, internalWallSubmesh);

						if (opening.surfaceB.tiled)
						{
							stepUvSideMin.x += 0.11f;
							stepUvSideMin.y += 0.37f;
							stepUvSideMax.x += 0.11f;
							stepUvSideMax.y += 0.37f;
						}
						if (opening.surfaceD.tiled)
						{
							stepUvTopMin.x += 0.23f;
							stepUvTopMin.y += 0.13f;
							stepUvTopMax.x += 0.23f;
							stepUvTopMax.y += 0.13f;
						}
					}
					mesh.AddPlane(flightABaseInsideDrop, flightABaseOutsideDrop, flightATopInsideDrop, flightATopOutsideDrop, internalWallSubmesh);//underside

					//flight two
					Vector3 flightBTopOutsideStep = flightBTopOutside + Vector3.down * stepHeight * 0.5f;
					Vector3 flightBTopInsideStep = flightBTopInside + Vector3.down * stepHeight * 0.5f;
					for (int s = 0; s < numberOfSteps - 1; s++)
					{
						float lerpValue = lerpIncrement * s;
						Vector3 s0 = Vector3.Lerp(flightBBaseOutside, flightBTopOutsideStep, lerpValue);
						Vector3 s1 = Vector3.Lerp(flightBBaseInside, flightBTopInsideStep, lerpValue);
						Vector3 s2 = s0 + Vector3.up * stepRiser;
						Vector3 s3 = s1 + Vector3.up * stepRiser;
						Vector3 s4 = s2 - escalationFlatDir.normalized * stepDepth;
						Vector3 s5 = s3 - escalationFlatDir.normalized * stepDepth;
						//front
						mesh.AddPlane(s0, s1, s2, s3, stepUvSideMin, stepUvSideMax, escalationFlatDir, rightTangent, internalWallSubmesh, opening.surfaceB);
						//top
						mesh.AddPlane(s2, s3, s4, s5, stepUvTopMin, stepUvTopMax, Vector3.up, rightTangent, internalFloorSubmesh, opening.surfaceD);

						//sides
						float lerpValueB = lerpIncrement * s;
						Vector3 normal = escalationRight;
						Vector3[] normals = { normal, normal, normal, normal };
						Vector4 tangent = BuildRMesh.CalculateTangent(escalationFlatDir);
						Vector4[] tangents = { tangent, tangent, tangent, tangent };
						Vector3 s8 = Vector3.Lerp(flightBBaseInsideDrop, flightBTopInsideDrop, lerpValueB);
						Vector3 s9 = Vector3.Lerp(flightBBaseInsideDrop, flightBTopInsideDrop, lerpValueB + lerpIncrement);
						Vector2 uv5, uv3, uv8, uv9;
						if (opening.surfaceB != null)
						{
							uv5 = opening.surfaceB.CalculateUV(new Vector2(s5.z, s5.y));
							uv3 = opening.surfaceB.CalculateUV(new Vector2(s3.z, s3.y));
							uv8 = opening.surfaceB.CalculateUV(new Vector2(s8.z, s8.y));
							uv9 = opening.surfaceB.CalculateUV(new Vector2(s9.z, s9.y));
						}
						else
						{
							uv5 = new Vector2();
							uv3 = new Vector2();
							uv8 = new Vector2();
							uv9 = new Vector2();
						}

						mesh.AddData(new[] { s3, s5, s8, s9 }, new[] { uv3, uv5, uv8, uv9 }, new[] { 0, 1, 2, 2, 1, 3 }, normals, tangents, internalWallSubmesh);

					}
					mesh.AddPlane(flightBBaseInsideDrop, flightBBaseOutsideDrop, flightBTopInsideDrop, flightBTopOutsideDrop, internalWallSubmesh);//underside
				}
			}
		}


		public static void GenerateRoofAccess(BuildRMesh mesh, VerticalOpening opening, Vector3 basePosition, float height, int floor, int wallSubmesh = -1, BuildRCollider collider = null)
		{
			//            bool stepped = true;//todo
			float minimumWidth = 0.9f;//UK standard
//			float maximumWidth = 2.0f;
			//            float stepHeight = 0.22f;
			float wallThickness = VerticalOpening.WALL_THICKNESS;

			float stairWidth = 0.70f;//todo / calculate
			float doorWidth = 1.3f;
			float doorHeight = 2.04f;

			bool generateColldier = collider != null;

			float minimumRunLength = 0.25f;
			float maximumRiserHeight = 0.2f;

			SubmeshLibrary submeshLibrary = mesh.submeshLibrary;
			int externalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceA);
			int internalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceB);
			int doorFrameSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceC);
			int internalFloorSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceD);
            
			//base positions
			Quaternion rotation = Quaternion.Euler(0, opening.rotation, 0);
			Vector2Int openingSize = opening.size;
			Vector3 b0 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, height, -opening.size.vy * 0.5f);
			Vector3 b1 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, height, -opening.size.vy * 0.5f);
			Vector3 b2 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, height, opening.size.vy * 0.5f);
			Vector3 b3 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, height, opening.size.vy * 0.5f);

			//inner points
			Vector3 b0i = b0 + rotation * new Vector3(1, 0, 1) * wallThickness;
			Vector3 b1i = b1 + rotation * new Vector3(-1, 0, 1) * wallThickness;
			Vector3 b2i = b2 + rotation * new Vector3(1, 0, -1) * wallThickness;
			Vector3 b3i = b3 + rotation * new Vector3(-1, 0, -1) * wallThickness;

			Vector2 internalSize = new Vector2(openingSize.vx - wallThickness * 2, openingSize.vy - wallThickness * 2);
			float stairWidthFromX = internalSize.x * 0.5f;
			float stairWidthFromY = internalSize.y - Mathf.Ceil(height / maximumRiserHeight) * minimumRunLength;

			float useLandingWidth = (stairWidthFromX + stairWidthFromY) * 0.5f;
			useLandingWidth = Mathf.Clamp(useLandingWidth, minimumWidth, opening.stairWidth);

			Vector3 escalationFlatDir = (b2i - b0i).normalized;

			Vector3 landingDrop = Vector3.down * wallThickness;

			//landing
			Vector3 l0 = b0i;
			Vector3 l1 = b1i;
			Vector3 l2 = b0i + escalationFlatDir * useLandingWidth;
			Vector3 l3 = b1i + escalationFlatDir * useLandingWidth;
			//top
			mesh.AddPlane(l0, l1, l2, l3, internalFloorSubmesh);
			//bottom
			mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, internalWallSubmesh);
			//front
			mesh.AddPlane(l2, l3, l2 + landingDrop, l3 + landingDrop, internalWallSubmesh);

			if (generateColldier)
			{
				collider.mesh.AddPlane(l0, l1, l2, l3, 0);
				collider.mesh.AddPlane(l1 + landingDrop, l0 + landingDrop, l3 + landingDrop, l2 + landingDrop, 0);
				collider.mesh.AddPlane(l2, l3, l2 + landingDrop, l3 + landingDrop, 0);
			}

			//internal walls
			Vector3 wallUp = Vector3.up * height;
			wallUp.y += -wallThickness * 0.5f;
			Vector3 wallUpI = Vector3.up * (height - wallThickness);
			mesh.AddPlane(b0i, b2i, b0i + wallUpI, b2i + wallUpI, internalWallSubmesh);
			mesh.AddPlane(b2i, b3i, b2i + wallUpI, b3i + wallUpI, internalWallSubmesh);
			mesh.AddPlane(b3i, b1i, b3i + wallUpI, b1i + wallUpI, internalWallSubmesh);
			mesh.AddPlane(b1i + wallUpI, b0i + wallUpI, b3i + wallUpI, b2i + wallUpI, internalWallSubmesh);

			//external walls
			mesh.AddPlane(b2, b0, b2 + wallUp, b0 + wallUp, externalWallSubmesh);
			mesh.AddPlane(b3, b2, b3 + wallUp, b2 + wallUp, externalWallSubmesh);
			mesh.AddPlane(b1, b3, b1 + wallUp, b3 + wallUp, externalWallSubmesh);
			mesh.AddPlane(b0 + wallUp, b1 + wallUp, b2 + wallUp, b3 + wallUp, internalWallSubmesh);

			if (generateColldier)
			{
				collider.AddPlane(b2, b0, b2 + wallUp, b0 + wallUp);
				collider.AddPlane(b3, b2, b3 + wallUp, b2 + wallUp);
				collider.AddPlane(b1, b3, b1 + wallUp, b3 + wallUp);
				collider.mesh.AddPlane(b1i + wallUpI, b0i + wallUpI, b3i + wallUpI, b2i + wallUpI, 0);
				collider.mesh.AddPlane(b0 + wallUp, b1 + wallUp, b2 + wallUp, b3 + wallUp, 0);

				if (!collider.usingPrimitives)
				{
					collider.mesh.AddPlane(b0i, b2i, b0i + wallUpI, b2i + wallUpI, 0);
					collider.mesh.AddPlane(b2i, b3i, b2i + wallUpI, b3i + wallUpI, 0);
					collider.mesh.AddPlane(b3i, b1i, b3i + wallUpI, b1i + wallUpI, 0);
				}
			}


			//door wall
			//internal
			float internalWallLength = openingSize.vx - (wallThickness * 2f);
			float lerpA = Mathf.Max(stairWidth - doorWidth, 0.05f) / internalWallLength;
			float lerpB = (Mathf.Max(stairWidth - doorWidth, 0.05f) + doorWidth) / internalWallLength;

			Vector3 bd0i = Vector3.Lerp(b0i, b1i, lerpA);
			Vector3 bd1i = Vector3.Lerp(b0i, b1i, lerpB);

			Vector3 doorUp = Vector3.up * doorHeight;
			//Right side
			mesh.AddPlane(bd0i, b0i, bd0i + doorUp, b0i + doorUp, internalWallSubmesh);
			//left side
			mesh.AddPlane(b1i, bd1i, b1i + doorUp, bd1i + doorUp, internalWallSubmesh);
			//top
			mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUpI, b0i + wallUpI, internalWallSubmesh);

			//external
			Vector3 doorOut = -escalationFlatDir * wallThickness;
			//left
			mesh.AddPlane(b0, bd0i + doorOut, b0 + doorUp, bd0i + doorOut + doorUp, externalWallSubmesh);
			//right
			mesh.AddPlane(bd1i + doorOut, b1, bd1i + doorOut + doorUp, b1 + doorUp, externalWallSubmesh);
			//top
			mesh.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUp, b1 + wallUp, externalWallSubmesh);

			//frame
			//floor
			mesh.AddPlane(bd1i, bd0i, bd1i + doorOut, bd0i + doorOut, externalWallSubmesh);
			//left
			mesh.AddPlane(bd0i + doorOut, bd0i, bd0i + doorOut + doorUp, bd0i + doorUp, doorFrameSubmesh);
			//right
			mesh.AddPlane(bd1i, bd1i + doorOut, bd1i + doorUp, bd1i + doorOut + doorUp, doorFrameSubmesh);
			//top
			mesh.AddPlane(bd0i + doorUp, bd1i + doorUp, bd0i + doorOut + doorUp, bd1i + doorOut + doorUp, doorFrameSubmesh);

			if (generateColldier)
			{
				collider.AddPlane(b0, bd0i + doorOut, b0 + doorUp, bd0i + doorOut + doorUp);
				collider.AddPlane(bd1i + doorOut, b1, bd1i + doorOut + doorUp, b1 + doorUp);
				collider.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUp, b1 + wallUp);

				if (!collider.usingPrimitives)
				{
					collider.mesh.AddPlane(bd0i, b0i, bd0i + doorUp, b0i + doorUp, 0);
					collider.mesh.AddPlane(b1i, bd1i, b1i + doorUp, bd1i + doorUp, 0);
					collider.mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUpI, b0i + wallUpI, 0);

					collider.mesh.AddPlane(bd1i, bd0i, bd1i + doorOut, bd0i + doorOut, 0);
					collider.mesh.AddPlane(bd0i + doorOut, bd0i, bd0i + doorOut + doorUp, bd0i + doorUp, 0);
					collider.mesh.AddPlane(bd1i, bd1i + doorOut, bd1i + doorUp, bd1i + doorOut + doorUp, 0);
					collider.mesh.AddPlane(bd0i + doorUp, bd1i + doorUp, bd0i + doorOut + doorUp, bd1i + doorOut + doorUp, 0);
				}
			}
		}
	}
}