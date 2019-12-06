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
using UnityEngine;

namespace BuildR2
{
    public class QuickPolyOffset
    {
        public static Vector2[] Execute(Vector2[] points, float distance)
        {
            int pointCount = points.Length;
//            Debug.Log("point count " + pointCount);
            Vector2[] output = new Vector2[pointCount];
            float[] pointAngles = new float[pointCount];
            Vector2[] pointDirections = new Vector2[pointCount];

            for(int p = 0; p < pointCount; p++)
            {
//                int px = (p + 1) % pointCount;
//                int py = (p - 1 + pointCount) % pointCount;
                int px = p + 1;
                if(px == pointCount) px = 0;
                int py = p - 1;
                if(py < 0) py = pointCount - 1;

                Vector2 a = points[p];
                Vector2 x = points[px];
                Vector2 y = points[py];

                Vector2 dirA = (x - a).normalized;
                Vector2 dirB = (a - y).normalized;

                Vector2 croA = Rotate(dirA, 90);
                Vector2 croB = Rotate(dirB, 90);
                Vector2 cross = (croA + croB).normalized;
                pointDirections[p] = cross;

                if (Vector2.Dot(dirA, dirB) > 1f - Mathf.Epsilon)//lines run in parallel
                {
                    pointAngles[p] = 180;
                }
                else
                {

                    Vector2 adirA = dirA;
                    Vector2 adirB = (y - a).normalized;
                    pointAngles[p] = SignAngleDirection(adirA, adirB);
                }
            }
            for(int p = 0; p < pointCount; p++)
            {
                float sin = Mathf.Sin(pointAngles[p] * 0.5f * Mathf.Deg2Rad);
                float angleDistance = Math.Abs(sin) > Mathf.Epsilon ? distance / sin : 1;
//                Debug.Log(points[p]+" "+pointDirections[p]+" "+angleDistance+" "+distance+" "+ pointAngles[p]);
//                Debug.DrawLine(JMath.ToV3(points[p])*10, JMath.ToV3(points[p] + pointDirections[p] * angleDistance * 10000), Color.red, 20);
                output[p] = points[p] + pointDirections[p] * angleDistance;
            }

//            output[0].x += 1;

            return output;
        }

        public static float SignAngleDirection(Vector2 dirForward, Vector2 dirAngle)
        {
            float angle = Vector2.Angle(dirForward, dirAngle);
            Vector2 cross = Rotate(dirForward, 90);
            float crossDot = Vector2.Dot(cross, dirAngle);
            if (crossDot < 0)
                angle = 360 - angle;
            return angle;
        }

        public static Vector2 Rotate(Vector2 input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            input.x = (cos * tx) - (sin * ty);
            input.y = (sin * tx) + (cos * ty);
            return input;
        }
    }
}