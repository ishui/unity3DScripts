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
using Random = UnityEngine.Random;

namespace BuildR2 {
    [Serializable]
    public class RandomGen
    {
        [SerializeField]
        private uint _seed;
        [SerializeField]
        private uint _mZ;
        [SerializeField]
        private uint _mW;

        public RandomGen()
        {
        }

        public RandomGen(uint newSeed)
        {
            _seed = newSeed;
            Reset();
        }

        public void Reset()
        {
            _mZ = _mW = _seed;
        }

        public float output
        {
            get
            {
                _mZ = 36969 * (_mZ & 65535) + (_mZ >> 16);
                _mW = 18000 * (_mW & 65535) + (_mW >> 16);
                uint u = (_mZ << 16) + _mW;
                float val = (u + 1.0f) * 2.328306435454494e-10f;
                return val;
            }
        }

        public float OneRange()
        {
            return Range(-1f, 1f);
        }

        public float OutputRange(float min, float max)
        {
            return min + output * (max - min);
        }

        public float Range(float min, float max)
        {
            return OutputRange(min, max);
        }

        public int OutputRange(int min, int max)
        {
            return min + Mathf.RoundToInt(output * (max - min));
        }

        public int Range(int min, int max)
        {
            return OutputRange(min, max);
        }

        public int Index(int length)
        {
            if(length == 0) return -1;
            return Mathf.RoundToInt(output * (length - 1));
        }

        public Vector2 Position(Rect bounds)
        {
            Vector2 pos = new Vector2();
            pos.x = bounds.size.x * output + bounds.xMin;
            pos.y = bounds.size.y * output + bounds.yMin;
            return pos;
        }

        public bool outputBool
        {
            get
            {
                return output<0.5f;
            }
        }

        public uint generateSeed
        {
            get
            {
                return (uint)(output * uint.MaxValue);
            }
        }

        public uint seed
        {
            get { return _seed; }

            set
            {
                _seed = value;
                Reset();
            }
        }

        public void GenerateNewSeed()
        {
            seed = (uint)Random.Range(1, 999999);
            Reset();
        }
    }
}