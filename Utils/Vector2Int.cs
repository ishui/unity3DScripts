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
    [Serializable]
    public struct Vector2Int
    {
        public static float SCALE = 0.01f;//cm

        public int x;
        public int y;

        //scale conversion values
        public float vx
        {
            get { return x * SCALE; }
            set { x = ConvertFromWorldUnits(value); }
        }

        public float vy
        {
            get { return y * SCALE; }
            set { y = ConvertFromWorldUnits(value); }
        }

        public Vector2Int(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int(float x = 0, float y = 0)
        {
            this.x = ConvertFromWorldUnits(x);
            this.y = ConvertFromWorldUnits(y);
        }

        public Vector2Int(Vector2 vector2)
        {
            x = ConvertFromWorldUnits(vector2.x);
            y = ConvertFromWorldUnits(vector2.y);
        }

        public Vector2Int(Vector2Int vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public Vector2Int(Vector3 vector3, bool flat)
        {
            x = ConvertFromWorldUnits(vector3.x);
            if(!flat)
                y = ConvertFromWorldUnits(vector3.y);
            else
                y = ConvertFromWorldUnits(vector3.z);
        }

        public static Vector2Int zero { get { return new Vector2Int(); } }
        public static Vector2Int one { get { return new Vector2Int(1, 1); } }
        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }

        public float magnitude
        {
            get { return vector2.magnitude; }
        }

        public int SqrMagnitudeInt()
        {
            return x * x + y * y;
        }

        public float SqrMagnitudeFloat()
        {
            return vector2.sqrMagnitude;
        }

        public override string ToString()
        {
            return string.Format("( {0} , {1} )", vx, vy);
        }

        public static float DistanceWorld(Vector2Int from, Vector2Int to)
        {
            return Vector2.Distance(from.vector2, to.vector2);
        }

        public bool Equals(Vector2Int p)
        {

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public bool Equals(UnityEngine.Object a)
        {
            return base.Equals(a);// (Dot(this, a) > 0.999f);
        }

        public override bool Equals(object a)
        {
            return base.Equals(a);// (Dot(this, a) > 0.999f);
        }

        public Vector2Int Move(Vector3 amount, bool isFlat)
        {
            x += Mathf.RoundToInt(amount.x / SCALE);
            if(isFlat)
                y += Mathf.RoundToInt(amount.z / SCALE);
            else
                y += Mathf.RoundToInt(amount.y / SCALE);
            return this;
        }
        
        public Vector2Int Move(Vector2 amount)
        {
            x += Mathf.RoundToInt(amount.x / SCALE);
            y += Mathf.RoundToInt(amount.y / SCALE);
            return this;
        }

        public Vector2 vector2
        {
            get { return new Vector2(x * SCALE, y * SCALE); }
            set
            {
                x = Mathf.RoundToInt(value.x / SCALE);
                y = Mathf.RoundToInt(value.y / SCALE);
            }
        }

        public Vector3 vector3XY
        {
            get { return new Vector3(x * SCALE, y * SCALE, 0); }
            set
            {
                x = Mathf.RoundToInt(value.x / SCALE);
                y = Mathf.RoundToInt(value.y / SCALE);
            }
        }

        public Vector3 vector3XZ
        {
            get { return new Vector3(x * SCALE, 0, y * SCALE); }
            set
            {
                x = Mathf.RoundToInt(value.x / SCALE);
                y = Mathf.RoundToInt(value.z / SCALE);
            }
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator +(Vector2Int a, Vector2 b)
        {
            return new Vector2Int(a.x + ConvertFromWorldUnits(b.x), a.y + ConvertFromWorldUnits(b.y));
        }

        public static Vector2Int operator +(Vector2 a, Vector2Int b)
        {
            return new Vector2Int(ConvertFromWorldUnits(a.x) + b.x, ConvertFromWorldUnits(a.y) + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2 b)
        {
            return new Vector2Int(a.x - ConvertFromWorldUnits(b.x), a.y - ConvertFromWorldUnits(b.y));
        }

        public static Vector2Int operator -(Vector2 a, Vector2Int b)
        {
            return new Vector2Int(ConvertFromWorldUnits(a.x) - b.x, ConvertFromWorldUnits(a.y) - b.y);
        }

        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }

        public static Vector2Int operator *(Vector2Int a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector2Int(a.x * bM, a.y * bM);
        }

        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }

        public static Vector2Int operator /(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x / b.x, a.y / b.y);
        }

        public static Vector2Int operator /(Vector2Int a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector2Int(a.x / bM, a.y / bM);
        }

        public static Vector2Int operator /(Vector2Int a, int b)
        {
            return new Vector2Int(a.x / b, a.y / b);
        }

        public static float Dot(Vector2Int a, Vector2Int b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        public static Vector2Int Lerp(Vector2Int a, Vector2Int b, float t)
        {
            Vector2Int output = new Vector2Int();
            output.x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, t));
            output.y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
            return output;
        }

        public static float Angle(Vector2Int from, Vector2Int to)
        {
            return Mathf.Atan2(to.y - from.y, to.x - from.x);
        }
        
        public static float SignAngle(Vector2Int from, Vector2Int to)
        {
            Vector2Int dir = new Vector2Int((Vector2)(to.vector2 - from.vector2).normalized);
            float angle = Angle(up, dir);
            Vector3 cross = Vector3.Cross(Vector3.forward, dir.vector3XZ);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngle(Vector2Int dir)
        {
            float angle = Angle(up, dir);
            Vector3 cross = Vector3.Cross(Vector3.forward, dir.vector3XZ);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngleDirection(Vector2Int dirForward, Vector2Int dirAngle)
        {
            float angle = Angle(dirForward, dirAngle);
            Vector2Int cross = Rotate(dirForward, 90);
            float crossDot = Dot(cross, dirAngle);
            if (crossDot < 0)
                angle = -angle;
            return angle;
        }

        public static Vector2Int Rotate(Vector2Int input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            Vector2Int output = new Vector2Int();
            output.x = Mathf.RoundToInt((cos * tx) - (sin * ty));
            output.y = Mathf.RoundToInt((sin * tx) + (cos * ty));
            return output;
        }

        public static Vector2Int Rotate(Vector2 input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            Vector2Int output = new Vector2Int();
            output.x = Mathf.RoundToInt((cos * tx) - (sin * ty));
            output.y = Mathf.RoundToInt((sin * tx) + (cos * ty));
            return output;
        }

        public static int ConvertFromWorldUnits(float input)
        {
            return Mathf.RoundToInt(input / SCALE);
        }

        public static float ConvertToWorldUnits(int input)
        {
            return input * SCALE;
        }
        
        public static int SqrMagnitudeInt(Vector2Int a, Vector2Int b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static float SqrMagnitudeFloat(Vector2Int a, Vector2Int b)
        {
            return (a.vector2 - b.vector2).sqrMagnitude;
        }

        public static Vector2[] Parse(Vector2Int[] input) {
            int length = input.Length;
            Vector2[] output = new Vector2[length];
            for (int p = 0; p < length; p++)
                output[p] = input[p].vector2;
            return output;
        }

        public static Vector2Int[] Parse(Vector2[] input) {
            int length = input.Length;
            Vector2Int[] output = new Vector2Int[length];
            for(int p = 0; p < length; p++)
            {
                output[p].vx = input[p].x;
                output[p].vy = input[p].y;
            }
            return output;
        }
    }
}