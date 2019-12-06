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
using UnityEditor;

namespace BuildR2
{
    public class DrawConcavePolygonHandle
    {
        public static void Shape(Vector3[] points, Color fillColor)
        {
            Handles.color = fillColor;
//            int[] tris = EarClipper.Triangulate(points);
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
                Handles.DrawAAConvexPolygon(f0, f1, f2);
            }
        }

        public static void ShapeWithLines(Vector3[] points, Color lineColor, Color fillColor)
        {
            Handles.color = fillColor;
//            int[] tris = EarClipper.Triangulate(points);
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
                Handles.DrawAAConvexPolygon(f0, f1, f2);
            }

            Handles.color = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                Handles.DrawLine(p0, p1);
            }
        }

        public static void ShapeLines(Vector3[] points, Color lineColor, Color fillColor)
        {
//            Handles.color = fillColor;
//            int[] tris = EarClipper.Triangulate(points);
//            int triCount = tris.Length;
//            for (int t = 0; t < triCount; t += 3)
//            {
//                Vector3 f0 = points[tris[t]];
//                Vector3 f1 = points[tris[t + 1]];
//                Vector3 f2 = points[tris[t + 2]];
//                Handles.DrawAAConvexPolygon(f0, f1, f2);
//            }

            Handles.color = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                Handles.DrawLine(p0, p1);
            }
        }

        public static void ShapeWithLines(Vector3[] points, Color lineColor, Color fillColor, bool highlight, Color highlightcolour, Color angleColour)
        {
            Handles.color = fillColor;
//            int[] tris = EarClipper.Triangulate(points, 0, 1, false);
//            int[] tris = new int[0];
//            for(int p = 0; p < points.Length; p++)
//            {
//                Debug.Log(points[p]);
//            }
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
//            for(int p = 0; p < points.Length; p++)
//                Handles.Label(points[p], p.ToString());
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
                Handles.DrawAAConvexPolygon(f0, f1, f2);
            }

            Handles.color = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                if (highlight)
                {
                    Vector3 diff = p1 - p0;
                    if(diff.x * diff.x < 0.001f || diff.z * diff.z < 0.001f)
                        Handles.color = highlightcolour;
                    else
                        Handles.color = angleColour;
                }
                Handles.DrawLine(p0, p1);
            }
        }
    }
}