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
using BuildR2;
using UnityEngine;
using PolyPartition;

namespace JaspLib {
    public class JMath {
        public static float SignAngle(Vector2 from, Vector2 to) {
            Vector2 dir = (to - from).normalized;
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector2.up, dir);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngle(Vector2 dir) {
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector2.up, dir);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngleDirection(Vector2 dirForward, Vector2 dirAngle) {
            float angle = Vector2.Angle(dirForward, dirAngle);
            Vector2 cross = Rotate(dirForward, 90);
            float crossDot = Vector2.Dot(cross, dirAngle);
            if (crossDot < 0)
                angle = -angle;
            return angle;
        }

        public static bool Compare(Vector2 a, Vector2 b, float accuracy = 0.001f) {
            return (b - a).sqrMagnitude < accuracy;
        }

        public static Vector3 ToV3(Vector2 input) {
            return new Vector3(input.x, 0, input.y);
        }

        public static Vector2 ToV2(Vector3 input) {
            return new Vector2(input.x, input.z);
        }

        public static Vector3[] ToV3(Vector2[] input) {
            int inputLength = input.Length;
            Vector3[] output = new Vector3[inputLength];
            for (int i = 0; i < inputLength; i++)
                output[i] = new Vector3(input[i].x, 0, input[i].y);
            return output;
        }

        public static Vector2[] ToV2(Vector3[] input) {
            int inputLength = input.Length;
            Vector2[] output = new Vector2[inputLength];
            for (int i = 0; i < inputLength; i++)
                output[i] = new Vector2(input[i].x, input[i].z);
            return output;
        }

        public static Vector2 Rotate(Vector2 input, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            input.x = (cos * tx) - (sin * ty);
            input.y = (sin * tx) + (cos * ty);
            return input;
        }

        public static Vector2[] NearestPoints(Vector2 toPoint, Vector2[] points, int returnNumber) {
            int[] indicies = NearestPointsIndicies(toPoint, points, returnNumber);
            Vector2[] output = new Vector2[returnNumber];
            for (int r = 0; r < returnNumber; r++)
                output[r] = points[indicies[r]];

            return output;
        }

        public static int[] NearestPointsIndicies(Vector2 toPoint, Vector2[] points, int returnNumber) {
            int pointCount = points.Length;
            float[] sqrD = new float[pointCount];
            for (int i = 0; i < pointCount; i++)
                sqrD[i] = (points[i] - toPoint).sqrMagnitude;

            int[] output = new int[returnNumber];
            for (int r = 0; r < returnNumber; r++) {
                float minDist = Mathf.Infinity;
                int index = 0;
                for (int i = 0; i < pointCount; i++) {
                    if (sqrD[i] < Mathf.Epsilon)
                        continue;//don't consider the same point man!
                    if (minDist > sqrD[i]) {
                        index = i;
                        minDist = sqrD[i];
                    }
                }
                sqrD[index] = Mathf.Infinity;//make selection unselectedble in next round
                output[r] = index;
            }

            return output;
        }

        public static Vector2 RotateTowards(Vector2 from, Vector2 to, float maxDegrees) {
            float angleFrom = Vector2.Angle(Vector2.up, from);
            float angleTo = Vector2.Angle(Vector2.up, to);
            float deltaAngle = Mathf.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Mathf.Min(deltaAngle, maxDegrees);
            return Rotate(from, deltaAngle);
        }

        public static Vector2 ClampLerp(Vector2 from, Vector2 to, float maxDegrees, float lerp) {
            float angleFrom = Vector2.Angle(Vector2.up, from);
            float angleTo = Vector2.Angle(Vector2.up, to);
            float deltaAngle = Mathf.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Mathf.Min(deltaAngle, maxDegrees);
            Vector2 useTo = Rotate(from, deltaAngle);
            return Vector2.Lerp(from, useTo, lerp);
        }

        public static bool ParallelLines(Vector2 dirA, Vector2 dirB) {
            return (dirA.y * dirB.x - dirB.y * dirA.x) == 0;
        }

        /// <summary>
        /// Herons Formula
        /// Calculate the area of a triangle from three sides
        /// </summary>
        public static float TriangleAreaHeron(float lengthA, float lengthB, float lengthC) {
            float s = (lengthA + lengthB + lengthC) * 0.5f;
            return Mathf.Sqrt(s * (s - lengthA) * (s - lengthB) * (s - lengthB));
        }

        public static float TriangleSignedArea(Vector2 p0, Vector2 p1, Vector3 p2) {
            return 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
        }

        /// <summary>
        /// SAS Formula
        /// Calculate the area of a triangle from two sides and an angle
        /// Side, Angle, Side
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static float TriangleAreaSAS(float lengthA, float angle, float lengthB) {
            return 0.5f * lengthA * lengthB * Mathf.Sin(angle * Mathf.Deg2Rad);
        }

        public static float PolyArea(Vector2[] points) {
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            float output = 0;
            for (int t = 0; t < triCount; t += 3) {
                Vector2 v0 = points[tris[t]];
                Vector2 v1 = points[tris[t + 1]];
                Vector2 v2 = points[tris[t + 2]];

                float a = Vector2.Distance(v0, v1);
                float b = Vector2.Distance(v1, v2);
                float c = Vector2.Distance(v2, v0);

                output += TriangleAreaHeron(a, b, c);
            }
            return output;
        }

        public static float PolyArea2(Vector2[] points) {
            TPPLPoly poly = new TPPLPoly();
            for (int i = 0; i < points.Length; i++)
                poly.Points.Add(new TPPLPoint(points[i].x, points[i].y));

            if (BuildrPolyClockwise.Check(points))
                poly.SetOrientation(TPPLOrder.CW);
            else
                poly.SetOrientation(TPPLOrder.CCW);

            List<TPPLPoly> triangles = new List<TPPLPoly>();
            TPPLPartition part = new TPPLPartition();
            part.Triangulate_EC(poly, triangles);

            int triCount = triangles.Count;
            float output = 0;
            for (int t = 0; t < triCount; t += 3) {
                TPPLPoint tv0 = triangles[t][0];
                TPPLPoint tv1 = triangles[t][1];
                TPPLPoint tv2 = triangles[t][2];

                Vector2 v0 = new Vector2(tv0.X, tv0.Y);
                Vector2 v1 = new Vector2(tv1.X, tv1.Y);
                Vector2 v2 = new Vector2(tv2.X, tv2.Y);
                
                float a = Vector2.Distance(v0, v1);
                float b = Vector2.Distance(v1, v2);
                float c = Vector2.Distance(v2, v0);

                output += TriangleAreaHeron(a, b, c);
            }
            return output;
        }

        private static FlatBounds BOUNDS = new FlatBounds();
        public static float PolyAreaQuick(Vector2[] points) {
            BOUNDS.Clear();
            for (int i = 0; i < points.Length; i++)
                BOUNDS.Encapsulate(points[i]);
            float output = BOUNDS.Area();
            return output;
        }

        /// <summary>
        /// SAS Formula
        /// Calculate the opposite edge of a triangle from two sides and an angle
        /// Side, Angle, Side
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static float TriangleFindOpposite(float lengthA, float angle, float lengthB) {
            return Mathf.Sqrt((lengthA * lengthA) + (lengthB * lengthB) - 2 * (lengthA * lengthB) * Mathf.Cos(angle * Mathf.Deg2Rad));
        }

        public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2) {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
            if (A < 0.0) {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) < A;
        }

        public static bool IsConvex(Vector2[] points) {
            Vector2 center = GetCenter(points);
            int pointCount = points.Length;

            float furthestPointDistance = 0;
            int furthestPointIndex = 0;
            for (int p = 0; p < pointCount; p++) {
                float sqrMag = Vector2.SqrMagnitude(points[p] - center);
                if (sqrMag > furthestPointDistance) {
                    furthestPointDistance = sqrMag;
                    furthestPointIndex = p;
                }
            }

            float sign = 0;
            for (int p = 0; p < pointCount; p++) {
                Vector2 pa = points[(p + furthestPointIndex) % pointCount];
                Vector2 pb = points[(p + 1 + furthestPointIndex) % pointCount];
                Vector2 pc = points[(p + 2 + furthestPointIndex) % pointCount];
                float cross = Cross(pa, pb, pc);
                float crossSign = Mathf.Sign(cross);
                if (p == 0) {
                    sign = crossSign;
                    continue;
                }

                if (sign != crossSign) return false;
            }
            return true;
        }

        public static float Cross(Vector2 a, Vector2 b, Vector2 c) {
            float x1 = b.x - a.x;
            float y1 = b.y - a.y;
            float x2 = c.x - b.x;
            float y2 = c.y - b.y;
            return x1 * y2 - x2 * y1;
        }

        public static float Cross(Vector2 a, Vector2 b) {
            return a.x * b.y - b.x * a.y;
        }

        public static int[] ConvexHull(Vector2[] points) {
            int pointCount = points.Length;
            int[] plusMinus = new int[2];//-/+
            float[] pointCrosses = new float[pointCount];
            for (int p = 0; p < pointCount; p++) {
                Vector2 pa = points[(p - 1 + pointCount) % pointCount];
                Vector2 pb = points[p];
                Vector2 pc = points[(p + 1) % pointCount];
                float cross = Cross(pa, pb, pc);
                pointCrosses[p] = cross;
                if (cross < 0) plusMinus[0]++; else plusMinus[1]++;
            }
            float sign = (plusMinus[0] > plusMinus[1]) ? -1 : 1;

            List<int> output = new List<int>();

            for (int p = 0; p < pointCount; p++)
                output.Add(p);

            for (int p = 0; p < pointCount + 1; p++) {
                int index = p % pointCount;

                int ia = output[(index - 1 + pointCount) % pointCount];
                int ib = output[index];
                int ic = output[(index + 1) % pointCount];

                Vector2 pa = points[ia];
                Vector2 pb = points[ib];
                Vector2 pc = points[ic];
                float cross = Cross(pa, pb, pc);

                if (Mathf.Sign(cross) != sign) {
                    output.RemoveAt(index);
                    p = Mathf.Max(0, p - 2);
                    pointCount--;
                }
            }

            return output.ToArray();
        }

        private static Vector2 GetCenter(Vector2[] points) {
            FlatBounds bounds = new FlatBounds();
            int pointCount = points.Length;
            for (int i = 0; i < pointCount; i++)
                bounds.Encapsulate(points[i]);
            return bounds.center;
        }

        public static float ClampAngle360(float input) {
            float output = input % 360;
            if (output < 0) output += 360;
            return output;
        }

        public static float ClampAngle(float input) {
            float output = input % 360;
            if (output > 180) output += -360;
            if (output < -180) output += 360;
            return output;
        }



        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection) {
            intersection = Vector2.zero;

            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

            return true;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            return true;
        }

        public static Vector3 FromPlanPoint(Vector2 point) {
            return new Vector3(point.x, 0, point.y);
        }

        public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point) {
            Vector3 v1 = point - a;
            Vector3 v2 = (b - a).normalized;
            float distance = Vector3.Distance(a, b);
            float t = Vector3.Dot(v2, v1);

            if (t <= 0)
                return a;
            if (t >= distance)
                return b;
            Vector3 v3 = v2 * t;
            Vector3 closestPoint = a + v3;
            return closestPoint;
        }

        public static Vector3 ClosestPointOnLine2(Vector3 a, Vector3 b, Vector3 point) {
            Vector3 v1 = point - a;
            Vector3 v2 = b - a;
            float sqrMag = Vector3.SqrMagnitude(v2);
            float dot = Vector3.Dot(v1, v2);
            float t = dot / sqrMag;

            Vector3 v3 = a + v2 * t;
            return v3;
        }


        public static Vector2 ClosestPointOnLine(Vector2 A, Vector2 B, Vector2 P) {
            Vector2 aToP = P - A;
            Vector2 aToB = B - A;
            float aToB2 = aToB.x * aToB.x + aToB.y * aToB.y;
            float aToPDotaToB = Vector2.Dot(aToP, aToB);
            float t = aToPDotaToB / aToB2;

            return new Vector2(A.x + aToB.x * t, A.y + aToB.y * t);
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        private static bool CCW(Vector2 p1, Vector2 p2, Vector2 p3) {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }

        public static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            if (Mathf.Abs(Vector2.Dot(a2, b2)) == 1.0f) return Vector2.zero;

            Vector2 intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);

            if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y)) {
                //flip the second line to find the intersection point
                intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
            }

            if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y)) {
                //            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
                intersectionPoint = a1 + a2;
            }

            return intersectionPoint;
        }

        public static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, Vector3 debugOrigin) {
            Vector2 intersectionPoint = FindIntersection(a1, a2, b1, b2);
            // if abs(angle)==1 then the lines are parallel,  
            // so no intersection is possible  
            //        if (Mathf.Abs(Vector2.Dot(a2, b2)) == 1.0f) return Vector2.zero;
            //
            ////        Vector2 intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
            //
            //        if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            //        {
            //            //flip the second line to find the intersection point
            //            intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
            //        }
            //
            //        if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            //        {
            //            //            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
            //            intersectionPoint = a1 + a2;
            //        }

            Vector3 ta1 = new Vector3(a1.x, 0, a1.y) + debugOrigin;
            Vector3 ta2 = new Vector3(a2.x, 0, a2.y) + debugOrigin;
            Vector3 tb1 = new Vector3(b1.x, 0, b1.y) + debugOrigin;
            Vector3 tb2 = new Vector3(b2.x, 0, b2.y) + debugOrigin;
            Debug.DrawLine(ta1, ta2, Color.magenta, .5f);
            Debug.DrawLine(tb1, tb2, Color.yellow, .5f);
            Vector3 intersectionPointV3 = new Vector3(intersectionPoint.x, 0, intersectionPoint.y) + debugOrigin;
            Debug.DrawLine(intersectionPointV3, intersectionPointV3 + Vector3.up, Color.red, .5f);

            return intersectionPoint;
        }

        public static bool FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection) {
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;
            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;
            // X bound box test/
            if (Ax < 0) {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0) {
                if (x1hi < p4.x || p3.x < x1lo) return false;
            }
            else {
                if (x1hi < p3.x || p4.x < x1lo) return false;
            }
            Ay = p2.y - p1.y;
            By = p3.y - p4.y;
            // Y bound box test//
            if (Ay < 0) {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else {
                y1hi = p2.y;
                y1lo = p1.y;
            }

            if (By > 0) {
                if (y1hi < p4.y || p3.y < y1lo) return false;
            }
            else {
                if (y1hi < p3.y || p4.y < y1lo) return false;
            }

            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//
                                    // alpha tests//
            if (f > 0) {
                if (d < 0 || d > f) return false;
            }
            else {
                if (d > 0 || d < f) return false;
            }
            e = Ax * Cy - Ay * Cx;  // beta numerator//

            // beta tests //
            if (f > 0) {
                if (e < 0 || e > f) return false;
            }
            else {
                if (e > 0 || e < f) return false;
            }

            // check if they are parallel
            if (f == 0) return false;
            // compute intersection coordinates //
            num = d * Ax; // numerator //
                          //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
                          //    intersection.x = p1.x + (num+offset) / f;
            intersection.x = p1.x + num / f;
            num = d * Ay;
            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
            //    intersection.y = p1.y + (num+offset) / f;
            intersection.y = p1.y + num / f;
            return true;
        }

        private static Vector2 IntersectionPoint(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB) {

            float xD1, yD1, xD2, yD2, xD3, yD3;
            float ua, div;

            // calculate differences  
            xD1 = lineA.x;
            xD2 = lineB.x;
            yD1 = lineA.y;
            yD2 = lineB.y;
            xD3 = originA.x - originB.x;
            yD3 = originA.y - originB.y;

            // find intersection Pt between two lines    
            Vector2 pt = new Vector2(0, 0);
            div = yD2 * xD1 - xD2 * yD1;
            ua = (xD2 * yD3 - yD2 * xD3) / div;
            pt.x = originA.x + ua * xD1;
            pt.y = originA.y + ua * yD1;

            // return the valid intersection  
            return pt;
        }

        private static Vector2 IntersectionPoint2(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB) {

            Vector2 lineA2 = lineA + originA;
            Vector2 lineB2 = lineB + originB;

            Vector3 crossA = Vector3.Cross(new Vector3(lineA.x, lineA.y, 1), new Vector3(lineA2.x, lineA2.y, 1));
            Vector3 crossB = Vector3.Cross(new Vector3(lineB.x, lineB.y, 1), new Vector3(lineB2.x, lineB2.y, 1));
            Vector3 crossAB = Vector3.Cross(crossA, crossB);

            Vector2 pt = new Vector2(0, 0);
            pt.x = crossAB.x / crossAB.z;
            pt.x = crossAB.y / crossAB.z;

            // return the valid intersection  
            return pt;
        }

        //        public static Vector2 IntersectionPoint3(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        //        {
        //            Vector2 intersection = new Vector2();
        //            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num, offset;
        //            float x1lo, x1hi, y1lo, y1hi;
        //
        //            Ax = p2.x - p1.x;
        //            Bx = p3.x - p4.x;
        //
        //            // X bound box test/
        //            if (Ax < 0)
        //            {
        //                x1lo = p2.x; x1hi = p1.x;
        //            }
        //            else
        //            {
        //                x1hi = p2.x; x1lo = p1.x;
        //            }
        //
        //            if (Bx > 0)
        //            {
        //                if (x1hi < p4.x || p3.x < x1lo) return intersection;
        //            }
        //            else
        //            {
        //                if (x1hi < p3.x || p4.x < x1lo) return intersection;
        //            }
        //
        //            Ay = p2.y - p1.y;
        //            By = p3.y - p4.y;
        //
        //            // Y bound box test//
        //            if (Ay < 0)
        //            {
        //                y1lo = p2.y; y1hi = p1.y;
        //            }
        //            else
        //            {
        //                y1hi = p2.y; y1lo = p1.y;
        //            }
        //
        //            if (By > 0)
        //            {
        //                if (y1hi < p4.y || p3.y < y1lo) return intersection;
        //            }
        //            else
        //            {
        //                if (y1hi < p3.y || p4.y < y1lo) return intersection;
        //            }
        //
        //            Cx = p1.x - p3.x;
        //            Cy = p1.y - p3.y;
        //            d = By * Cx - Bx * Cy;  // alpha numerator//
        //            f = Ay * Bx - Ax * By;  // both denominator//
        //
        //            // alpha tests//
        //            if (f > 0)
        //            {
        //                if (d < 0 || d > f) return intersection;
        //            }
        //            else
        //            {
        //                if (d > 0 || d < f) return intersection;
        //            }
        //
        //            e = Ax * Cy - Ay * Cx;  // beta numerator//
        //
        //            // beta tests //
        //            if (f > 0)
        //            {
        //                if (e < 0 || e > f) return intersection;
        //            }
        //            else
        //            {
        //                if (e > 0 || e < f) return intersection;
        //            }
        //
        //            // check if they are parallel
        //            if (f == 0) return intersection;
        //
        //            // compute intersection coordinates //
        //            num = d * Ax; // numerator //
        //            offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;   // round direction //
        //            intersection.x = p1.x + (num + offset) / f;
        //
        //            num = d * Ay;
        //            offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;
        //            intersection.y = p1.y + (num + offset) / f;
        //
        //            return intersection;
        //        }

        public static Vector2 IntersectionPoint4(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            Vector2 intersection = new Vector2();
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;
            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;
            // X bound box test/
            if (Ax < 0) {
                x1lo = p2.x; x1hi = p1.x;
            }
            else {
                x1hi = p2.x; x1lo = p1.x;
            }

            if (Bx > 0) {
                if (x1hi < p4.x || p3.x < x1lo) return Vector2.zero;
            }
            else {
                if (x1hi < p3.x || p4.x < x1lo) return Vector2.zero;
            }

            Ay = p2.y - p1.y;
            By = p3.y - p4.y;
            // Y bound box test//
            if (Ay < 0) {
                y1lo = p2.y; y1hi = p1.y;
            }
            else {
                y1hi = p2.y; y1lo = p1.y;
            }

            if (By > 0) {
                if (y1hi < p4.y || p3.y < y1lo) return Vector2.zero;
            }
            else {
                if (y1hi < p3.y || p4.y < y1lo) return Vector2.zero;
            }

            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//

            // alpha tests//
            if (f > 0) {
                if (d < 0 || d > f) return Vector2.zero;
            }
            else {
                if (d > 0 || d < f) return Vector2.zero;
            }
            e = Ax * Cy - Ay * Cx;  // beta numerator//

            // beta tests //
            if (f > 0) {
                if (e < 0 || e > f) return Vector2.zero;
            }
            else {
                if (e > 0 || e < f) return Vector2.zero;
            }

            // check if they are parallel
            if (f == 0) return Vector2.zero;
            // compute intersection coordinates //
            num = d * Ax; // numerator //
            intersection.x = p1.x + num / f;
            num = d * Ay;
            intersection.y = p1.y + num / f;
            return intersection;
        }

        public static bool PointInsidePoly(Vector2 point, Vector2[] poly) {
            Rect polyBounds = new Rect(0, 0, 0, 0);
            foreach (Vector2 polyPoint in poly) {
                if (polyBounds.xMin > polyPoint.x)
                    polyBounds.xMin = polyPoint.x;
                if (polyBounds.xMax < polyPoint.x)
                    polyBounds.xMax = polyPoint.x;
                if (polyBounds.yMin > polyPoint.y)
                    polyBounds.yMin = polyPoint.y;
                if (polyBounds.yMax < polyPoint.y)
                    polyBounds.yMax = polyPoint.y;
            }
            if (!polyBounds.Contains(point))
                return false;

            Vector2 pointRight = point + new Vector2(polyBounds.width, 0);

            int numberOfPolyPoints = poly.Length;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++) {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[(i + 1) % numberOfPolyPoints];
                if (FastLineIntersection(point, pointRight, p0, p1))
                    numberOfCrossOvers++;
            }

            return numberOfCrossOvers % 2 != 0;
        }

        public static int[] IndexCoords(int index, int width) {
            int xIndex = Mathf.RoundToInt((index % width));
            float yRaw = index / (float)width;
            int yIndex = Mathf.RoundToInt(Mathf.Sign(yRaw) * Mathf.Floor(Mathf.Abs(yRaw)));
            return new[] { xIndex, yIndex };
        }

        public static int[] GenerateTriangleIndices(int size) {
            if (size < 3)
                return null;
            int faces = size - 2;
            int[] output = new int[faces * 3];
            for (int f = 0; f < faces; f++) {
                output[f * 3] = 0;
                output[f * 3 + 1] = f + 1;
                output[f * 3 + 2] = f + 2;
            }
            return output;
        }

        private static bool same_sign(float a, float b) {
            return ((a * b) >= 0f);
        }

        public static bool CorectlyWound(Vector3[] points, Vector3 normal) {
            Vector3 dirA = (points[1] - points[0]).normalized;
            Vector3 dirB = (points[2] - points[0]).normalized;
            Vector3 cross = Vector3.Cross(dirA, dirB);
            return Vector3.Dot(cross, normal) > 0;
        }

        /// <summary>
        /// Calcaulte the Tangent from a direction
        /// </summary>
        /// <param name="tangentDirection">the normalised right direction of the tangent</param>
        public static Vector4 CalculateTangent(Vector3 tangentDirection) {
            Vector4 tangent = new Vector4();
            tangent.x = tangentDirection.x;
            tangent.y = tangentDirection.y;
            tangent.z = tangentDirection.z;
            tangent.w = 1;//TODO: Check whether we need to flip the bi normal - I don't think we do with these planes
            return tangent;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        /// <param name="points">Only three points will be used in calculation</param>
        public static Vector3 CalculateNormal(Vector3[] points) {
            if (points.Length < 3) return Vector3.down;//most likely to look wrong
            return CalculateNormal(points[0], points[1], points[2]);
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static Vector3 CalculateNormal(Vector3 p0, Vector3 p1, Vector3 p2) {
            return Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized).normalized;
        }

        public static Vector3 ProjectVectorOnPlane(Vector3 point, Vector3 normal) {
            Vector3 offset = Vector3.Project(point, normal);
            return point - offset;
        }

        public static bool ShapesIntersect(Vector2[] a, Vector2[] b)
        {
            int aSize = a.Length;
            int bSize = b.Length;

            for (int ax = 0; ax < aSize; ax++)
            {
                Vector2 p0 = a[ax];
                int ay = ax < aSize - 1 ? ax + 1 : 0;
                Vector2 p1 = a[ay];
                for (int bx = 0; bx < bSize; bx++)
                {
                    Vector2 p2 = b[bx];
                    int by = bx < bSize - 1 ? bx + 1 : 0;
                    Vector2 p3 = b[by];

                    if (FastLineIntersection(p0, p1, p2, p3)) return true;
                }
            }
            
            if (PointInsidePoly(a[0], b)) return true;
            if (PointInsidePoly(b[0], a)) return true;

            return false;
        }
    }


    public class IntPoint : Object {
        protected bool Equals(IntPoint other) {
            return base.Equals(other) && x == other.x && y == other.y;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ x;
                hashCode = (hashCode * 397) ^ y;
                return hashCode;
            }
        }

        public int x;
        public int y;

        public IntPoint(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return string.Format("< x : {0} , y : {1} >", x, y);
        }

        public override bool Equals(object obj) {
            IntPoint p = obj as IntPoint;
            if ((object)p == null)
                return false;
            return x == p.x && y == p.y;
        }

        public static bool operator ==(IntPoint a, IntPoint b) {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((Object)a == null) || ((Object)b == null))
                return false;

            // Return true if the fields match:
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(IntPoint a, IntPoint b) {
            return !(a == b);
        }
    }
}
