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

public class SplineMaths : MonoBehaviour
{

    //Calculate the Bezier spline position
    //t - the time (0-1) of the curve to sample
    //p0 - the start point of the curve
    //p1 - control point from p0
    //p2 - control point from p3
    //p3 - the end point of the curve
    public static Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float u = 1.0f - t;
        float u2 = u * u;
        float u3 = u2 * u;

        Vector3 p = u3 * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t3 * p3;

        return p;
    }

    public static Vector3 CalculateCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }


    public static Vector3 CalculateHermite(Vector3 p, Vector3 a, Vector3 b, Vector3 q, float t, float tension, float bias)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 m0 = (a - p) * (1 + bias) * (1 - tension) / 2;
        m0 += (b - a) * (1 - bias) * (1 - tension) / 2;
        Vector3 m1 = (b - a) * (1 + bias) * (1 - tension) / 2;
        m1 += (q - b) * (1 - bias) * (1 - tension) / 2;
        float a0 = 2 * t3 - 3 * t2 + 1;
        float a1 = t3 - 2 * t2 + t;
        float a2 = t3 - t2;
        float a3 = -2 * t3 + 3 * t2;

        return (a0 * a + a1 * m0 + a2 * m1 + a3 * b);
    }

    public static Vector3 CalculateCatmullRom(Vector3 p, Vector3 a, Vector3 b, Vector3 q, float t)
    {
        var t2 = t * t;

        var a0 = -0.5f * p + 1.5f * a - 1.5f * b + 0.5f * q;
        var a1 = p - 2.5f * a + 2f * b - 0.5f * q;
        var a2 = -0.5f * p + 0.5f * b;
        var a3 = a;

        return (a0 * t * t2) + (a1 * t2) + (a2 * t) + a3;
    }


    public static Vector2 CalculateBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float u = 1.0f - t;
        float u2 = u * u;
        float u3 = u2 * u;

        Vector2 p = u3 * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t3 * p3;

        return p;
    }

    public static Vector2 CalculateHermite(Vector2 p, Vector2 a, Vector2 b, Vector2 q, float t, float tension, float bias)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector2 m0 = (a - p) * (1 + bias) * (1 - tension) / 2;
        m0 += (b - a) * (1 - bias) * (1 - tension) / 2;
        Vector2 m1 = (b - a) * (1 + bias) * (1 - tension) / 2;
        m1 += (q - b) * (1 - bias) * (1 - tension) / 2;
        float a0 = 2 * t3 - 3 * t2 + 1;
        float a1 = t3 - 2 * t2 + t;
        float a2 = t3 - t2;
        float a3 = -2 * t3 + 3 * t2;

        return (a0 * a + a1 * m0 + a2 * m1 + a3 * b);
    }

    public static Vector2 CalculateCatmullRom(Vector2 p, Vector2 a, Vector2 b, Vector2 q, float t)
    {
        var t2 = t * t;

        var a0 = -0.5f * p + 1.5f * a - 1.5f * b + 0.5f * q;
        var a1 = p - 2.5f * a + 2f * b - 0.5f * q;
        var a2 = -0.5f * p + 0.5f * b;
        var a3 = a;

        return (a0 * t * t2) + (a1 * t2) + (a2 * t) + a3;
    }
    //Calculate Cubic Rotation
    //p - point we start with
    //q - next point
    //nextNormIndex - the point immediately before p
    //prevNormIndex - the point immediately after q
    //t - time (0-1) of the curve pq to sample
    public static Quaternion CalculateCubicRotation(Quaternion p, Quaternion a, Quaternion b, Quaternion q, float t)
    {
        Quaternion a1 = SquadTangent(a, p, q);
        Quaternion b1 = SquadTangent(p, q, b);
        float slerpT = 2.0f * t * (1.0f - t);
        Quaternion sl = Slerp(Slerp(p, q, t), Slerp(a1, b1, t), slerpT);
        return sl;
    }

    public static float CalculateHermite(float val)
    {
        return val * val * (3.0f - 2.0f * val);
    }

    /////////////////////////////////////
    //Warning - here be crazy maths stuff
    /////////////////////////////////////

    //calculate the Squad tangent for use in Cubic Rotation Interpolation
    public static Quaternion SquadTangent(Quaternion before, Quaternion center, Quaternion after)
    {
        Quaternion l1 = lnDif(center, before);
        Quaternion l2 = lnDif(center, after);
        Quaternion e = Quaternion.identity;
        for (int i = 0; i < 4; ++i)
        {
            e[i] = -0.25f * (l1[i] + l2[i]);
        }
        return center * (exp(e));
    }

    public static Quaternion lnDif(Quaternion a, Quaternion b)
    {
        Quaternion dif = Quaternion.Inverse(a) * b;
        Normalize(dif);
        return log(dif);
    }

    public static Quaternion Normalize(Quaternion q)
    {
        float norm = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (norm > 0.0f)
        {
            q.x /= norm;
            q.y /= norm;
            q.z /= norm;
            q.w /= norm;
        }
        else
        {
            q.x = (float)0.0f;
            q.y = (float)0.0f;
            q.z = (float)0.0f;
            q.w = (float)1.0f;
        }
        return q;
    }

    public static Quaternion exp(Quaternion q)
    {
        float theta = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);

        if (theta < 1E-6)
        {
            return new Quaternion(q[0], q[1], q[2], Mathf.Cos(theta));
        }
        else
        {
            float coef = Mathf.Sin(theta) / theta;
            return new Quaternion(q[0] * coef, q[1] * coef, q[2] * coef, Mathf.Cos(theta));
        }
    }

    public static Quaternion log(Quaternion q)
    {
        float len = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);

        if (len < 1E-6)
        {
            return new Quaternion(q[0], q[1], q[2], 0.0f);
        }
        else
        {
            float coef = Mathf.Acos(q[3]) / len;
            return new Quaternion(q[0] * coef, q[1] * coef, q[2] * coef, 0.0f);
        }
    }

    //based on [Shoe87] implementation
    public static Quaternion Slerp(Quaternion p, Quaternion q, float t)
    {
        Quaternion ret;
        float cos = Quaternion.Dot(p, q);

        float omega, somega, invSin, fCoeff0, fCoeff1;
        if ((1.0f + cos) > 0.00001f)
        {
            if ((1.0f - cos) > 0.00001f)
            {
                omega = Mathf.Acos(cos);
                somega = Mathf.Sin(omega);
                invSin = (Mathf.Sign(somega) * 1.0f) / somega;
                fCoeff0 = Mathf.Sin((1.0f - t) * omega) * invSin;
                fCoeff1 = Mathf.Sin(t * omega) * invSin;
            }
            else
            {
                fCoeff0 = 1.0f - t;
                fCoeff1 = t;
            }
            ret.x = fCoeff0 * p.x + fCoeff1 * q.x;
            ret.y = fCoeff0 * p.y + fCoeff1 * q.y;
            ret.z = fCoeff0 * p.z + fCoeff1 * q.z;
            ret.w = fCoeff0 * p.w + fCoeff1 * q.w;
        }
        else
        {
            fCoeff0 = Mathf.Sin((1.0f - t) * Mathf.PI * 0.5f);
            fCoeff1 = Mathf.Sin(t * Mathf.PI * 0.5f);

            ret.x = fCoeff0 * p.x - fCoeff1 * p.y;
            ret.y = fCoeff0 * p.y + fCoeff1 * p.x;
            ret.z = fCoeff0 * p.z - fCoeff1 * p.w;
            ret.w = p.z;
        }
        return ret;
    }

    public static Quaternion Nlerp(Quaternion p, Quaternion q, float t)
    {
        Quaternion ret;

        float w1 = 1.0f - t;

        ret.x = w1 * p.x + t * q.x;
        ret.y = w1 * p.y + t * q.y;
        ret.z = w1 * p.z + t * q.z;
        ret.w = w1 * p.w + t * q.w;
        Normalize(ret);

        return ret;
    }

    public static Quaternion GetQuatConjugate(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    public static float ClampAngle(float angle, float from, float to)
    {
        if (angle > 180) angle = 360 - angle;
        angle = Mathf.Clamp(angle, from, to);
        if (angle < -180) angle = 360 + angle;

        return angle;
    }

    public static float ClampAngle(float angle)
    {
        if (angle > 180) angle = angle - 360;
        if (angle < -180) angle = angle + 360;

        return angle;
    }
}
