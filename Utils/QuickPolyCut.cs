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
using UnityEngine;

namespace BuildR2
{
    public class QuickPolyCut
    {
        /// <summary>
        /// Return a shape with a defined shape cut out of it.
        /// Will test find the best closest non intersectional cut to make in the base shape
        /// Assumes the cut doesn't intersect the shape lines and is inside the base shape
        /// </summary>
        /// <param name="baseShape"></param>
        /// <param name="cut"></param>
        /// <returns></returns>
        public static Vector2[] Execute(Vector2[] baseShape, List<Vector2[]> cuts)
        {
            bool dbg = false;
            Vector2[] cut = (Vector2[])cuts[0].Clone();//todo multiple cuts
            int baseCount = baseShape.Length;
            int cutCount = cut.Length;
            int outputCount = baseCount + cutCount + 2;
            Vector2[] output = new Vector2[outputCount];
            FlatBounds bounds = new FlatBounds(cut);
            if (dbg) bounds.DrawDebug(Color.magenta);
            Vector2 center = bounds.center;
            float nrest = Mathf.Infinity;
            int nearestIndex = 0;
            Array.Reverse(cut);//reverse winding to create cut
            for(int b = 0; b < baseCount; b++)
            {
                float sqrMag = (baseShape[b] - center).sqrMagnitude;
                if(sqrMag < nrest)
                {
                    Vector2 a1 = center;
                    Vector2 a2 = baseShape[b];

                    bool intersectsShape = false;
                    for(int x = 0; x < baseCount; x++)
                    {
                        if(b == x) continue;
                        int x2 = (x + 1) % baseCount;
                        if (b == x2) continue;
                        Vector2 b1 = baseShape[x];
                        Vector2 b2 = baseShape[x2];
                        if (dbg) Debug.DrawLine(ToV3(b1), ToV3(b2), Color.yellow);
                        if (FastLineIntersection(a1, a2, b1, b2))
                        {
                            intersectsShape = true;
                            break;
                        }
                    }
                    if(!intersectsShape)
                    {
                        nearestIndex = b;
                        nrest = sqrMag;
                    }
                    //intersection check
                }
            }

            //find nearest cut point to base point
            int nearestCutIndex = 0;
            float nearestCut = Mathf.Infinity;
            Vector2 baseCut = baseShape[nearestIndex];
            for (int i = 0; i < cutCount; i++)
            {
                float sqrMag = (cut[i] - baseCut).sqrMagnitude;
                if(sqrMag < nearestCut)
                {
                    nearestCutIndex = i;
                    nearestCut = sqrMag;
                }
            }

            if(dbg)
            {
                Debug.Log(nearestIndex);
                Debug.DrawLine(ToV3(baseShape[nearestIndex]), ToV3(center), Color.red);
            }

            for(int o = 0; o < outputCount; o++)
            {
                if(o <= nearestIndex)
                {
                    output[o] = baseShape[o];
                    if(dbg && o > 0) Debug.DrawLine(ToV3(output[o]), ToV3(output[o-1]) + Vector3.up, Color.blue, 60);
                    if (dbg) Debug.Log("0x " + baseCount + " " + cutCount + " " + output.Length + " " + o);
                }
                else if(o > nearestIndex && o <= nearestIndex + cutCount + 1)
                {
                    int cutIndex = (nearestCutIndex + o - nearestIndex - 1) % cutCount;
                    output[o] = cut[cutIndex];
                    if (dbg && o > 0) Debug.DrawLine(ToV3(output[o]), ToV3(output[o-1]) + Vector3.up, Color.blue, 60);
                    if (dbg) Debug.Log("1x " + cutIndex + " " + baseCount + " " + cutCount + " " + output.Length + " " + o);
                }
                else
                {
                    int finalIndex = (o - cutCount - 2 + baseCount) % baseCount;
                    if(dbg) Debug.Log("2x " + finalIndex + " " + baseCount + " " + cutCount + " " + output.Length + " " + o);
                    output[o] = baseShape[finalIndex];
                    if (dbg && o > 0) Debug.DrawLine(ToV3(output[o]), ToV3(output[o-1]) + Vector3.up, Color.blue, 60);
                }
            }

            return output;
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        private static bool CCW(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }

        private static Vector3 ToV3(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
        }
    }
}