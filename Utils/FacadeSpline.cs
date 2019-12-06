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
    public class FacadeSpline
    {
        public static Vector3 Calculate(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 a, b, c, d;
            float ct = 0;
            if (t < 0.5f)
            {
                a = p0;
                b = p0;
                c = p1;
                d = p2;
                ct = t * 2.0f;
            }
            else if (t > 0.5f)
            {
                a = p0;
                b = p1;
                c = p2;
                d = p2;
                ct = t * 2.0f - 1.0f;
            }
            else
            {
                return p1;
            }

            return SplineMaths.CalculateHermite(a, b, c, d, ct, -0.80f, 0);
//            return SplineMaths.CalculateBezierPoint(a, b, c, d, ct);
//            return SplineMaths.CalculateCatmullRom(a, b, c, d, ct);
        }

        public static Vector3 Calculate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 a, b, c, d;
            float ct = 0;
            if (t < 1f / 3f)
            {
                a = p0;
                b = p0;
                c = p1;
                d = p2;
                ct = t * 3.0f;
            }
            else if (t > 2f / 3f)
            {
                a = p1;
                b = p2;
                c = p3;
                d = p3;
                ct = t * 3.0f - 2.0f;
            }
            else
            {
                a = p0;
                b = p1;
                c = p2;
                d = p3;
                ct = t * 3.0f - 1.0f;
            }

            //            return SplineMaths.CalculateHermite(p0, p1, p2, p3, t, -0.80f, 0);
            //            return SplineMaths.CalculateBezierPoint(p0, p1, p2, p3, t);
            return SplineMaths.CalculateCatmullRom(a, b, c, d, ct);
        }

        public static Vector2 Calculate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            Vector2 a, b, c, d;
            float ct = 0;
            if (t < 1f / 3f)
            {
                a = p0;
                b = p0;
                c = p1;
                d = p2;
                ct = t * 3.0f;
            }
            else if (t > 2f / 3f)
            {
                a = p1;
                b = p2;
                c = p3;
                d = p3;
                ct = t * 3.0f - 2.0f;
            }
            else
            {
                a = p0;
                b = p1;
                c = p2;
                d = p3;
                ct = t * 3.0f - 1.0f;
            }
            
            return SplineMaths.CalculateCatmullRom(a, b, c, d, ct);
        }
    }
}