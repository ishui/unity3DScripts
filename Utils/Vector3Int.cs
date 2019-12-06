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

namespace BuildR2.Utils
{
    [Serializable]
    public struct Vector3Int
    {
        public int x;
        public int y;
        public int z;

        public Vector3Int(int x = 0, int y = 0, int z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3Int Zero { get { return new Vector3Int(); } }
        public static Vector3Int One { get { return new Vector3Int(1, 1, 1); } }
        public static Vector3Int Up { get { return new Vector3Int(0, 1); } }
        public static Vector3Int Down { get { return new Vector3Int(0, -1); } }
        public static Vector3Int Left { get { return new Vector3Int(-1); } }
        public static Vector3Int Right { get { return new Vector3Int(1); } }
        public static Vector3Int Forward { get { return new Vector3Int(0,0,1); } }
        public static Vector3Int Backward { get { return new Vector3Int(0,0,-1); } }

        public float magnitude
        {
            get { return Mathf.Sqrt(sqrMagnitude); }
        }

        public Vector3Int normalised
        {
            get
            {
                float div = x + y + z;
                return new Vector3Int(Mathf.RoundToInt(x / div), Mathf.RoundToInt(y / div), Mathf.RoundToInt(z / div));
            }
        }

        public float sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public void Normalise()
        {
            float div = x + y + z;
            x = Mathf.RoundToInt(x / div);
            y = Mathf.RoundToInt(y / div);
            z = Mathf.RoundToInt(z / div);
        }

        public override string ToString()
        {
            return string.Format("( {0} , {1} , {2} )", x, y, z);
        }

        public static float Angle(Vector3Int from, Vector3Int to)
        {
            return 0;
        }

        public static float Distance(Vector3Int from, Vector3Int to)
        {
            int xdist = to.x - from.x;
            int ydist = to.y - from.y;
            int zdist = to.z - from.z;
            return xdist * xdist + ydist * ydist + zdist * zdist;
        }
    }
}