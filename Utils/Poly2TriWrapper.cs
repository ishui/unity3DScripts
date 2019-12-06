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
using BuildR2.Poly2Tri;
using UnityEngine;

namespace BuildR2
{
    public class Poly2TriWrapper
    {
        public static int[] Triangulate(Vector2[] shape, bool flipTri = false, Vector2[][] holes = null)
        {
            int shapeLength = shape.Length;
            if (shapeLength < 3) return new int[0];

            PolygonPoint[] shapePoints = new PolygonPoint[shapeLength];
            for (int p = 0; p < shapeLength; p++)
                shapePoints[p] = new PolygonPoint(shape[p].x, shape[p].y);
            Polygon mainShape = new Polygon(shapePoints);

//            Debug.Log("holes "+holes);
            if (holes != null)
            {
                int holeCount = holes.Length;
//                Debug.Log("holeCount " + holeCount);

                for (int h = 0; h < holeCount; h++)
                {
                    int holeSize = holes[h].Length;
                    for(int hs = 0; hs < holeSize; hs++)
                    {
                        Vector2 holePoint = holes[h][hs];
                        int holeIntersection = PointInShape(holePoint, shape);
//                        Debug.Log("Triangulate "+ holeIntersection);
                        if(holeIntersection != -1)
                        {
                            Vector2 holePointB = holes[h][(hs + 1) % holeSize];
                            Vector2 holePointC = holes[h][(hs - 1 + holeSize) % holeSize];
                            Vector2 dB = (holePointB - holePoint).normalized;
                            Vector2 dC = (holePointC - holePoint).normalized;
                            Vector2 dU = (dB + dC).normalized * 0.01f;
                            Vector3 cr = Vector3.Cross(new Vector3(dB.x, 0, dB.y), Vector3.up);
                            float sign = Mathf.Sign(Vector3.Dot(cr, -new Vector3(dC.x, 0, dC.y)));
                            holes[h][hs] += sign * dU;//fractionally move the point inwards so that 
//                            Debug.Log(sign+" "+dU);
//                            Debug.DrawLine(JMath.ToV3(holePoint), JMath.ToV3(holePoint + sign * dU), Color.red, 20);
                        }
                    }
                    
                    //use hole to clip shape
                    PolygonPoint[] holePoints = new PolygonPoint[holeSize];
                    for (int hp = 0; hp < holeSize; hp++)
                        holePoints[hp] = new PolygonPoint(holes[h][hp].x, holes[h][hp].y);
                    Polygon holePoly = new Polygon(holePoints);
                    mainShape.AddHole(holePoly);
                }
            }

            //collect all the stored points to index against the triangle list
            List<TriangulationPoint> tPoints = new List<TriangulationPoint>();
            for (int p = 0; p < shapeLength; p++)
                tPoints.Add(mainShape.Points[p]);


            if (holes != null && holes.Length > 0)
            {
                int pHoleCount = mainShape.Holes.Count;
                for (int h = 0; h < pHoleCount; h++)
                {
                    int holeSize = mainShape.Holes[h].Points.Count;
                    for (int hp = 0; hp < holeSize; hp++)
                        tPoints.Add(mainShape.Holes[h].Points[hp]);
                }
            }

            try
            {
                P2T.Triangulate(mainShape);
            }
            catch //(Exception e)
            {
//                Debug.LogWarning(e.Message);
            }
            int triCount = mainShape.Triangles.Count;
            int[] output = new int[triCount * 3];
            int index1 = !flipTri ? 1 : 2;
            int index2 = !flipTri ? 2 : 1;
            int maxVertIndex = tPoints.Count - 1;
            for (int t = 0; t < triCount; t++)
            {
                DelaunayTriangle tri = mainShape.Triangles[t];
                output[t * 3 + 0] = Mathf.Clamp(tPoints.IndexOf(tri.Points[0]), 0, maxVertIndex);
                output[t * 3 + index1] = Mathf.Clamp(tPoints.IndexOf(tri.Points[1]), 0, maxVertIndex);
                output[t * 3 + index2] = Mathf.Clamp(tPoints.IndexOf(tri.Points[2]), 0, maxVertIndex);
            }
            return output;
        }

        public static void BMesh(BuildRMesh mesh, float height, Surface surface, int submesh, Vector2[] shape, Rect clampUV, bool flipTri = false, Vector2[][] holes = null, BuildRCollider collider = null)
        {
            int shapeSize = shape.Length;
            
            bool[] useHole = new bool[0];
            if (holes != null)
            {
                int holeCount = holes.Length;
//                Debug.Log("BMesh "+holeCount);
                useHole = new bool[holeCount];
                for (int flc = 0; flc < holeCount; flc++)
                {
                    useHole[flc] = true;
                    int holeSize = holes[flc].Length;

//                    for(int h = 0; h < holeSize; h++)
//                    {
//                        Vector2 holePoint = holes[flc][h];
//                        holeIntersections[h] = PointInShape(holePoint, shape);
////                        Debug.Log("intersection length " + intersections.Length);
//                        useHole[flc] = holeIntersections[h].Length == 0;
//                    }

                    //                    for(int flcp = 0; flcp < holeSize; flcp++)
                    //                    {
//                                            if(!PointInPolygon(holes[flc][flcp], shape))
                    //                        {
                    //                            useHole[flc] = false;
                    //                            break;
                    //                        }
                    //                    }
                    if (useHole[flc])
                        shapeSize += holeSize;
                }
            }

            Vector2[] allFloorPoints = new Vector2[shapeSize];
            int mainShapeLength = shape.Length;
            for (int ms = 0; ms < mainShapeLength; ms++)
                allFloorPoints[ms] = shape[ms];
            int cutPointIterator = mainShapeLength;
            if (holes != null)
            {
                for (int flc = 0; flc < holes.Length; flc++)
                {
                    if (!useHole[flc]) continue;
                    for (int flcp = 0; flcp < holes[flc].Length; flcp++)
                    {
                        allFloorPoints[cutPointIterator] = holes[flc][flcp];
                        cutPointIterator++;
                    }
                }
            }

            FlatBounds bounds = new FlatBounds();
            if (clampUV.width > 0)
            {
                for (int fvc = 0; fvc < mainShapeLength; fvc++)
                    bounds.Encapsulate(shape[fvc]);
            }

            Vector3[] floorPoints = new Vector3[shapeSize];
            Vector2[] floorUvs = new Vector2[shapeSize];
            Vector3[] floorNorms = new Vector3[shapeSize];
            Vector4[] floorTangents = new Vector4[shapeSize];
            Vector3 normal = flipTri ? Vector3.up : Vector3.down;
            Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);
            for (int rp = 0; rp < shapeSize; rp++)
            {
                floorPoints[rp] = new Vector3(allFloorPoints[rp].x, height, allFloorPoints[rp].y);
                if (clampUV.width > 0)
                {
                    Vector2 clampedUV = new Vector2();
                    clampedUV.x = ((floorPoints[rp].x - bounds.xMin) / bounds.width) * clampUV.width + clampUV.x;
                    clampedUV.y = ((floorPoints[rp].z - bounds.yMin) / bounds.height) * clampUV.height + clampUV.y;
                    floorUvs[rp] = clampedUV;
                }
                else
                {
                    if (surface != null)
                        floorUvs[rp] = surface.CalculateUV(allFloorPoints[rp]);
                    else
                        floorUvs[rp] = allFloorPoints[rp];
                }
                floorNorms[rp] = normal;
                floorTangents[rp] = tangent;
            }

            int[] tris = Triangulate(shape, flipTri, holes);

            //                Debug.Log(volumeFloor + " " + actualFloor + " " + floorPoints.Length + " " + tris.Length+" "+r);
            int useFloorSubmesh = submesh != -1 ? submesh : 0;
            mesh.AddData(floorPoints, floorUvs, tris, floorNorms, floorTangents, useFloorSubmesh);
            if (collider != null)
                collider.mesh.AddData(floorPoints, floorUvs, tris, floorNorms, floorTangents, 0);
        }
        //        public static int[] Triangulate(Vector2Int[] shape)
        //        {
        //            List<PolygonPoint> points = new List<PolygonPoint>();
        //            int shapeLength = shape.Length;
        //            for (int p = 0; p < shapeLength; p++)
        //                points.Add(new PolygonPoint(shape[p].vx, shape[p].vy));
        //            Polygon polyFloor = new Polygon(points);
        //            P2T.Triangulate(polyFloor);
        //            int triCount = polyFloor.Triangles.Count;
        //            int[] output = new int[triCount * 3];
        //            for (int t = 0; t < triCount; t++)
        //            {
        //                DelaunayTriangle tri = polyFloor.Triangles[t];
        //                output[t * 3 + 0] = polyFloor.Points.IndexOf(tri.Points[0]);
        //                output[t * 3 + 2] = polyFloor.Points.IndexOf(tri.Points[1]);
        //                output[t * 3 + 1] = polyFloor.Points.IndexOf(tri.Points[2]);
        //            }
        //            return output;
        //        }
        //
        public static int[] Triangulate(Vector3[] shapeXZ)
        {
            int shapeLength = shapeXZ.Length;
            Vector2[] shape = new Vector2[shapeLength];
            for (int s = 0; s < shapeLength; s++)
                shape[s] = new Vector2(shapeXZ[s].x, shapeXZ[s].z);
            return Triangulate(shape);
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

        public static int PointInShape(Vector2 point, Vector2[] shape)
        {
            int shapeSize = shape.Length;
//            for(int s = 0; s < shapeSize; s++)
//            {
//                Vector2 shapePoint = shape[s];
//                float sqMag = Vector2.SqrMagnitude(shapePoint - point);
//                if(sqMag < 0.01f)
//                    return s;
//            }
            for (int s = 0; s < shapeSize; s++)
            {
                Vector2 shapePoint = shape[s];
                int indexB = s < shapeSize - 1 ? s : 0;
                Vector2 shapePointB = shape[indexB];
                if (PointOnLine(point, shapePoint, shapePointB))
                    return s;
            }
            return -1;
        }

        public static bool PointOnLine(Vector2 p, Vector2 a, Vector2 b)
        {
            float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(cross) > Mathf.Epsilon) return false;
            float dot = (p.x - a.x) * (b.x - a.x) + (p.y - a.y) * (b.y - a.y);
            if (dot < 0) return false;
            float squaredlengthba = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
            if (dot > squaredlengthba) return false;
            return true;
        }

        public static bool Colinear(Vector2Int pa, Vector2Int pb, Vector2Int pc)
        {
            double detleft = (pa.x - pc.x) * (pb.y - pc.y);
            double detright = (pa.y - pc.y) * (pb.x - pc.x);
            double val = detleft - detright;
            return (val > -Mathf.Epsilon && val < Mathf.Epsilon);
        }
    }
}