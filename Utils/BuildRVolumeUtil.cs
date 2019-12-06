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
using BuildR2.ClipperLib;
using UnityEngine;
using IntPoint = BuildR2.ClipperLib.IntPoint;
using Path = System.Collections.Generic.List<BuildR2.ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<BuildR2.ClipperLib.IntPoint>>;

namespace BuildR2
{
	public class BuildRVolumeUtil
	{
		private const int ACCURACY = 1000;

		public struct VolumeShape
		{
			public Vector2[] outer;
			public Vector2[][] holes;
		}

		public struct ParapetShape
		{
			public Vector2[] shape;
			public bool[] render;
		}

		public static VolumeShape[] GetBottomShape(IBuilding building, IVolume volume, Vector2[] usePoints = null)
		{
			VolumeShape[] output;
			Clipper clipper = new Clipper();

			if (usePoints == null)
			{
				List<Vector2Int> newPoints = new List<Vector2Int>();
				List<Vector2Int> wallAnchors = volume.wallAnchors;
				for (int w = 0; w < wallAnchors.Count; w++)
				{
					if (!newPoints.Contains(wallAnchors[w]))
						newPoints.Add(wallAnchors[w]);
				}
				usePoints = Vector2Int.Parse(newPoints.ToArray());
			}

			Paths subj = new Paths();
			//            if (usePoints == null)
			//                subj.Add(VolumeToPath(volume));
			//            else
			subj.Add(Vector2ToPath(usePoints));
			Paths clip = new Paths();

			IVolume[] belowVolumes = GetBelowVolumes(building, volume);
			int numberOfBelowVolumes = belowVolumes.Length;
			Vector2[][] belowVolumeShapes = new Vector2[numberOfBelowVolumes][];
			for (int op = 0; op < numberOfBelowVolumes; op++)
			{
				belowVolumeShapes[op] = belowVolumes[op].AllPointsV2();
				clip.Add(Vector2ToPath(belowVolumeShapes[op]));
			}

			if (clip.Count > 0)
			{
				PolyTree pTree = new PolyTree();
				clipper.AddPaths(subj, PolyType.ptSubject, true);
				clipper.AddPaths(clip, PolyType.ptClip, true);
				clipper.Execute(ClipType.ctDifference, pTree);

				int shapeCount = pTree.ChildCount;
				output = new VolumeShape[shapeCount];
				for (int s = 0; s < shapeCount; s++)
				{
					PolyNode node = pTree.Childs[s];
					output[s] = new VolumeShape();
					output[s].outer = PathToVector2(node.Contour);
					int holeCount = node.ChildCount;
					output[s].holes = new Vector2[holeCount][];
					for (int h = 0; h < holeCount; h++)
					{
						PolyNode holeNode = node.Childs[h];
						output[s].holes[h] = PathToVector2(holeNode.Contour);
					}
				}
			}
			else
			{
				output = new VolumeShape[1];
				output[0] = new VolumeShape();
				output[0].outer = PathToVector2(subj[0]);
				output[0].holes = new Vector2[0][];
			}

			return output;
		}

		public static VolumeShape[] GetTopShape(IBuilding building, IVolume volume, Vector2[] usePoints = null)
		{
			VolumeShape[] output;
			Clipper clipper = new Clipper();

			Paths subj = new Paths();
			if (usePoints == null)
				subj.Add(VolumeToPath(volume));
			else
				subj.Add(Vector2ToPath(usePoints));
			Paths clip = new Paths();

			int numberOfAbovePlans = volume.abovePlanCount;
			Vector2[][] aboveVolumeShapes = new Vector2[numberOfAbovePlans][];
			for (int op = 0; op < numberOfAbovePlans; op++)
			{
				if (volume.AbovePlanList()[op] == null) continue;
				aboveVolumeShapes[op] = volume.AbovePlanList()[op].AllPointsV2();
				clip.Add(Vector2ToPath(aboveVolumeShapes[op]));
			}

			VerticalOpening[] volumeOpenings = BuildrUtils.GetOpeningsQuick(building, volume);
			int numberOfOpenings = volumeOpenings.Length;
			int volumeTopFloor = volume.floors + building.VolumeBaseFloor(volume);
			for (int op = 0; op < numberOfOpenings; op++)
			{
				if (!volumeOpenings[op].FloorIsIncluded(volumeTopFloor)) continue;

				bool isInOtherVolume = false;
				Vector2[] openingPoints = volumeOpenings[op].Points();
				int openingSize = openingPoints.Length;
				for (int v = 0; v < numberOfAbovePlans; v++)
				{
					if (volume.AbovePlanList()[v] == null) continue;
					for (int p = 0; p < openingSize; p++)
					{
						if (PointInPolygon(openingPoints[p], aboveVolumeShapes[v]))
						{
							isInOtherVolume = true;
							break;
						}
					}
					if (isInOtherVolume) break;
				}

				if (!isInOtherVolume)
					clip.Add(OpeningToPath(volumeOpenings[op]));
			}


			if (clip.Count > 0)
			{
				PolyTree pTree = new PolyTree();
				clipper.AddPaths(subj, PolyType.ptSubject, true);
				clipper.AddPaths(clip, PolyType.ptClip, true);
				clipper.Execute(ClipType.ctDifference, pTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

				int shapeCount = pTree.ChildCount;
				//                Debug.Log("GetTopShape "+shapeCount);
				output = new VolumeShape[shapeCount];
				for (int s = 0; s < shapeCount; s++)
				{
					PolyNode node = pTree.Childs[s];
					output[s] = new VolumeShape();
					output[s].outer = PathToVector2(node.Contour);
					int holeCount = node.ChildCount;
					output[s].holes = new Vector2[holeCount][];
					for (int h = 0; h < holeCount; h++)
					{
						PolyNode holeNode = node.Childs[h];
						output[s].holes[h] = PathToVector2(holeNode.Contour);
					}
				}
			}
			else
			{
				output = new VolumeShape[1];
				output[0] = new VolumeShape();
				output[0].outer = usePoints;
				output[0].holes = new Vector2[0][];
			}

			SplitSelfIntersecting(output);

			return output;
		}

		public struct ParapetWallData
		{
			//base points
			public int index;
			public Vector2 pA;
			public Vector2 pB;
			public Vector2 Int;

			public enum Types
			{
				None,
				Full,
				AtoIntersection,
				IntersectiontoB
			}

			public Types type;
			
			public override string ToString()
			{
				return String.Format("Parapet Wall {0} : {1}",index, type);
			}
		}

		public static List<ParapetWallData> GetParapetShapes(IBuilding building, IVolume volume, Vector2[] points)
		{
			List<ParapetWallData> output = new List<ParapetWallData>();
			int pointSize = points.Length;

			int numberOfAbovePlans = volume.abovePlanCount;
			Vector2[][] aboveVolumeShapes = new Vector2[numberOfAbovePlans][];
			for (int op = 0; op < numberOfAbovePlans; op++)
			{
				if (volume.AbovePlanList()[op] == null) continue;
				aboveVolumeShapes[op] = volume.AbovePlanList()[op].AllPointsV2();
			}

			for (int p = 0; p < pointSize; p++)
			{
				Vector2 p0 = points[p];
				Vector2 p1 = p < pointSize - 1 ? points[p + 1] : points[0];

				ParapetWallData data = new ParapetWallData();
				data.index = p;
				data.type = ParapetWallData.Types.Full;
				data.pA = p0;
				data.pB = p1;

				for (int op = 0; op < numberOfAbovePlans; op++)
				{
					Vector2[] shape = aboveVolumeShapes[op];
					bool p0Intr = PointInPolygon(p0, shape);
					bool p1Intr = PointInPolygon(p1, shape);

					if (p0Intr && p1Intr)
					{
						data.type = ParapetWallData.Types.None;
						break;//points within a shape
					}

					if (p0Intr || p1Intr)
					{
						int size = shape.Length;
						for (int s = 0; s < size; s++)
						{
							Vector2 px0 = shape[s];
							int sb = s + 1;
							if (s == size - 1) sb = 0;
							Vector2 px1 = shape[sb];

							if(PointOnLine(px0, p0, p1))
							{
								data.Int = px0;
								data.type = p0Intr ? ParapetWallData.Types.IntersectiontoB : ParapetWallData.Types.AtoIntersection;
								break;
							}
							if(PointOnLine(px1, p0, p1))
							{
								data.Int = px1;
								data.type = p0Intr ? ParapetWallData.Types.IntersectiontoB : ParapetWallData.Types.AtoIntersection;
								break;
							}

							if (FastLineIntersection(p0, p1, px0, px1))
							{
								Vector2 ip = FindIntersection(p0, p1, px0, px1);

								data.Int = ip;
								data.type = p0Intr ? ParapetWallData.Types.IntersectiontoB : ParapetWallData.Types.AtoIntersection;
								break;
							}
						}
					}
				}
				output.Add(data);
			}

			return output;
		}

		private static void SplitSelfIntersecting(VolumeShape[] shape)
		{
			int shapeCount = shape.Length;
			for (int s = 0; s < shapeCount; s++)
			{
				int shapeSize = shape[s].outer.Length;

				for (int p = 0; p < shapeSize; p++)
				{
					Vector2 p0 = shape[s].outer[p];
					int pB = p < shapeSize - 1 ? p + 1 : 0;
					Vector2 p1 = shape[s].outer[pB];

					for (int px = 0; px < shapeSize; px++)
					{
						if (px == p) continue;
						if (px == pB) continue;

						Vector2 pXp = shape[s].outer[px];

                        if (PointOnLine(pXp, p0, p1))
						{
							int pBx = px < shapeSize - 1 ? px + 1 : 0;
							int pCx = px > 0 ? px - 1 : shapeSize - 1;
							Vector2 holePointB = shape[s].outer[pBx];
							Vector2 holePointC = shape[s].outer[pCx];
							Vector2 dB = (holePointB - pXp).normalized;
							Vector2 dC = (holePointC - pXp).normalized;
							Vector2 dU = (dB + dC).normalized * 0.01f;
							Vector3 cr = Vector3.Cross(new Vector3(dB.x, 0, dB.y), Vector3.up);
							float sign = Mathf.Sign(Vector3.Dot(cr, -new Vector3(dC.x, 0, dC.y)));
							shape[s].outer[px] += sign * dU;//fractionally move the point inwards so that 
						}
					}
				}
			}
		}

		private static Path Vector2ToPath(Vector2[] points, bool reverse = false)
		{
			Path output = new Path();
			int numberOfPoints = points.Length;

			for (int p = 0; p < numberOfPoints; p++)
			{
				int index = reverse ? numberOfPoints - 1 - p : p;
				output.Add(Vector2ToIntpoint(points[index]));
			}
			return output;
		}

		private static Path VolumeToPath(IVolume volume, bool reverse = false)
		{
			Path output = new Path();
			Vector2[] points = volume.AllPointsV2();
			int numberOfPoints = points.Length;

			for (int p = 0; p < numberOfPoints; p++)
			{
				int index = reverse ? numberOfPoints - 1 - p : p;
				output.Add(Vector2ToIntpoint(points[index]));
			}
			return output;
		}

		private static Path OpeningToPath(VerticalOpening opening)
		{
			Path output = new Path();
			Vector2[] points = opening.PointsRotated();
			points = QuickPolyOffset.Execute(points, VerticalOpening.WALL_THICKNESS);
			int numberOfPoints = points.Length;
			for (int p = 0; p < numberOfPoints; p++)
				output.Add(Vector2ToIntpoint(points[p]));
			return output;
		}

		private static IntPoint Vector2ToIntpoint(Vector2 input)
		{
			return new IntPoint(input.x * ACCURACY, input.y * ACCURACY);
		}

		private static Vector2 IntpointToVector2(IntPoint input)
		{
			return new Vector2(input.X / (float)ACCURACY, input.Y / (float)ACCURACY);
		}

		private static Vector2[] PathToVector2(Path input)
		{
			int inputLength = input.Count;
			Vector2[] output = new Vector2[inputLength];
			for (int i = 0; i < inputLength; i++)
				output[i] = IntpointToVector2(input[i]);
			return output;
		}

		public static bool PointInPolygon(Vector2 point, Vector2[] points)
		{
			int i, j;
			bool c = false;

			for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
			{
				if ((((points[i].y) >= point.y) != (points[j].y >= point.y)) && (point.x <= (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
					c = !c;
			}

			return c;
		}

		public static bool PointInPolygon(Vector2Int point, Vector2Int[] points)
		{
			int i, j;
			bool c = false;

			for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
			{
				if ((((points[i].y) >= point.y) != (points[j].y >= point.y)) && (point.x <= (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
					c = !c;
			}

			return c;
		}

		public static IVolume[] GetBelowVolumes(IBuilding building, IVolume volume)
		{
			List<IVolume> output = new List<IVolume>();
			IVolume[] volumeList = building.AllPlans();
			int volumeCount = volumeList.Length;
			for (int v = 0; v < volumeCount; v++)
			{
				IVolume other = volumeList[v];
				if (other == volume) continue;

				if (other.ContainsPlanAbove(volume))
				{
					output.Add(other);
					output.AddRange(other.LinkPlanList());
					break;
				}
			}
			output.Remove(volume);
			return output.ToArray();
		}

		private static void DrawPaths(Paths paths, Color colour, float height = 0)
		{
			foreach (Path path in paths)
				DrawPath(path, colour, height);
		}

		private static void DrawPath(Path path, Color colour, float height = 0)
		{
			int pathLength = path.Count;
			for (int i = 0; i < pathLength; i++)
			{
				Vector2 p0 = IntpointToVector2(path[i]);
				Vector2 p1 = IntpointToVector2(path[(i + 1) % pathLength]);
				Vector3 v0 = new Vector3(p0.x, height, p0.y);
				Vector3 v1 = new Vector3(p1.x, height, p1.y);
				Debug.DrawLine(v0, v1, colour, 1.0f);
			}
		}

		private static bool PointOnLine(Vector2 p, Vector2 a, Vector2 b)
		{
			float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
			if (Mathf.Abs(cross) > Mathf.Epsilon) return false;
			float dot = (p.x - a.x) * (b.x - a.x) + (p.y - a.y) * (b.y - a.y);
			if (dot < 0) return false;
			float squaredlengthba = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
			if (dot > squaredlengthba) return false;
			return true;
		}



		public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
				return false;
			return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
		}

		private static bool Ccw(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
		}

		public static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			if (Mathf.Abs(Vector2.Dot(a2, b2)) > 1f - Mathf.Epsilon) return Vector2.zero;

			Vector2 intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);

			if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
			{
				//flip the second line to find the intersection point
				intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
			}

			if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
			{
				//            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
				intersectionPoint = a1 + a2;
			}

			return intersectionPoint;
		}

		public static Vector2 IntersectionPoint4(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			Vector2 intersection = new Vector2();
			float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
			float x1lo, x1hi, y1lo, y1hi;
			Ax = p2.x - p1.x;
			Bx = p3.x - p4.x;
			// X bound box test/
			if (Ax < 0)
			{
				x1lo = p2.x; x1hi = p1.x;
			}
			else
			{
				x1hi = p2.x; x1lo = p1.x;
			}

			if (Bx > 0)
			{
				if (x1hi < p4.x || p3.x < x1lo) return Vector2.zero;
			}
			else
			{
				if (x1hi < p3.x || p4.x < x1lo) return Vector2.zero;
			}

			Ay = p2.y - p1.y;
			By = p3.y - p4.y;
			// Y bound box test//
			if (Ay < 0)
			{
				y1lo = p2.y; y1hi = p1.y;
			}
			else
			{
				y1hi = p2.y; y1lo = p1.y;
			}

			if (By > 0)
			{
				if (y1hi < p4.y || p3.y < y1lo) return Vector2.zero;
			}
			else
			{
				if (y1hi < p3.y || p4.y < y1lo) return Vector2.zero;
			}

			Cx = p1.x - p3.x;
			Cy = p1.y - p3.y;
			d = By * Cx - Bx * Cy;  // alpha numerator//
			f = Ay * Bx - Ax * By;  // both denominator//

			// alpha tests//
			if (f > 0)
			{
				if (d < 0 || d > f) return Vector2.zero;
			}
			else
			{
				if (d > 0 || d < f) return Vector2.zero;
			}
			e = Ax * Cy - Ay * Cx;  // beta numerator//

			// beta tests //
			if (f > 0)
			{
				if (e < 0 || e > f) return Vector2.zero;
			}
			else
			{
				if (e > 0 || e < f) return Vector2.zero;
			}

			// check if they are parallel
			if (Math.Abs(f) < Mathf.Epsilon) return Vector2.zero;
			// compute intersection coordinates //
			num = d * Ax; // numerator //
			intersection.x = p1.x + num / f;
			num = d * Ay;
			intersection.y = p1.y + num / f;
			return intersection;
		}
	}
}