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
using System.Collections.Generic;
using JaspLib;

namespace BuildR2 {
    public class QuadClipper
    {

        private static Vector2[] points;
        private static List<Vector2> pointList = new List<Vector2>();
        private static List<int> indexList = new List<int>();
        private static List<int[]> quads = new List<int[]>();
        private static int pointCount = pointList.Count;

        public static List<int[]> Quadrilate(Vector2[] points)
        {
            List<int[]> output = new List<int[]>();
            SetPoints(points);
            int it = 1000;
            while(pointCount > 4)
            {
                Execute();
                it--;
                if(it < 0) break;
            }

            if (indexList.Count > 2)
                output.Add(indexList.ToArray());//dump remaining indicies

            return output;
        }

        public static void SetPoints(Vector2[] newpoints)
        {
            points = newpoints;
            pointList = new List<Vector2>(points);
            pointCount = pointList.Count;
            indexList = new List<int>();
            string arrayOutput = "{ ";
            for(int p = 0; p < pointCount; p++)
            {
                arrayOutput += "new Vector2("+newpoints[p].x+"f, "+newpoints[p].y+"f)";
                if (p < pointCount - 1) arrayOutput += ", ";
                indexList.Add(p);
            }
            arrayOutput += " }";
            Debug.Log(arrayOutput);
            quads.Clear();
        }

        public static void Execute()
        {
//        Color cola = new Color(1, 0, 1, 0.25f);
//        for (int l = 0; l < pointCount; l++)
//        {
//            int x = (l + 1) % pointCount;
//            Debug.DrawLine(JMath.ToV3(pointList[l]), JMath.ToV3(pointList[x]), cola, 88);
//        }

            float[] angles = new float[pointCount];
            float[] angleDeviations = new float[pointCount];
            for (int p = 0; p < pointCount; p++)//calculate angles and angle deviations
            {//the quad with the most perpendicular shape should be selected
                int indexA = (p - 1 + pointCount) % pointCount;
                int indexB = p;
                int indexC = (p + 1) % pointCount;

                Vector2 pA = pointList[indexA];
                Vector2 pB = pointList[indexB];
                Vector2 pC = pointList[indexC];

                Vector2 dAB = pB - pA;
                Vector2 dBC = pC - pB;

                angles[p] = JMath.SignAngle(dAB, dBC);
                angleDeviations[p] = Mathf.Abs(angles[p] - 90);
            }

            float[] quadDeviation = new float[pointCount];
            int minQuadDeviationIndex = -1;
            float minQuadDeviation = Mathf.Infinity;
            for (int p = 0; p < pointCount; p++)
            {
                int indexA = p;
                int indexB = (p + 1) % pointCount;
                int indexC = (p + 2) % pointCount;
                int indexD = (p + 3) % pointCount;
                int indexX = (p - 1 + pointCount) % pointCount;//the point before the quad start point
                int indexY = (p + 4) % pointCount;//the point after the forth point in the quad

                if (angles[indexA] > 135) continue;
                if (angles[indexB] > 135) continue;
                if (angles[indexC] > 135) continue;

                Vector2 pA = pointList[indexA];
                Vector2 pC = pointList[indexC];
                Vector2 pD = pointList[indexD];
                Vector2 pY = pointList[indexY];

                Vector2 dBC = pA - pD;
                Vector2 dBY = pY - pD;
                Vector2 dAB = pD - pC;
                float angleY = JMath.SignAngle(dBY, dBC);//continue shape angle
                float angleD = JMath.SignAngle(dAB, dBC);//cut angle
//            if (angleD > 135) continue;//not sure anymore...
                if (angleD <= angleY) continue;//does the cut run outside of the shape
            
                bool intersectionFound = false;
                for (int l = 0; l < pointCount; l++)
                {
                    if (l == indexA) continue;
                    if (l == indexX) continue;
                    if (l == indexD) continue;
                    if (l == indexC) continue;

                    if(JMath.Intersects(pD, pA, pointList[l], pointList[(l + 1) % pointCount]))
                    {
                        intersectionFound = true;
                        //                        Debug.DrawLine(JMath.ToV3(pD),JMath.ToV3(pA),Color.blue,88);
                        //                        Debug.DrawLine(JMath.ToV3(pointList[l]), JMath.ToV3(pointList[(l + 1) % pointCount]), Color.red, 88);
                        break;
                    }
                    //                    Debug.DrawLine(JMath.ToV3(pointList[l]), JMath.ToV3(pointList[(l + 1) % pointCount]), Color.yellow, 88);
                }
                if (intersectionFound) continue;

                float angleDevA = angleDeviations[indexA];
                float angleDevB = angleDeviations[indexB];
                float angleDevC = angleDeviations[indexC];
                float angleDevD = Mathf.Abs(angleD - 90);

                quadDeviation[p] = angleDevA + angleDevB + angleDevC + angleDevD;

                if (quadDeviation[p] < minQuadDeviation)
                {
                    minQuadDeviationIndex = p;
                    minQuadDeviation = quadDeviation[p];
                }
            }

            if(minQuadDeviationIndex == -1)
            {
                Debug.Log(pointCount);
                Debug.LogError("no quad found");
            }

            int[] outputEntry = new int[4];
            Debug.Log(indexList.Count+" "+minQuadDeviation);
            outputEntry[0] = indexList[minQuadDeviationIndex];
            outputEntry[1] = indexList[(minQuadDeviationIndex + 1) % pointCount];
            outputEntry[2] = indexList[(minQuadDeviationIndex + 2) % pointCount];
            outputEntry[3] = indexList[(minQuadDeviationIndex + 3) % pointCount];

//        Color col = new Color(0, 1, 0, 0.5f);
//        for (int i = 0; i < 4; i++)
//            Debug.DrawLine(JMath.ToV3(points[outputEntry[i]]), JMath.ToV3(points[outputEntry[(i + 1) % 4]]), col, 88);

            //            int removeIndexA = minQuadDeviationIndex;
            int removeIndexA = (minQuadDeviationIndex + 1) % pointCount;
            int removeIndexB = (minQuadDeviationIndex + 2) % pointCount;

            if (removeIndexA < removeIndexB)//discard the outer two points of the quad.
            {
                pointList.RemoveAt(removeIndexA);
                pointList.RemoveAt(removeIndexA);
                indexList.RemoveAt(removeIndexA);
                indexList.RemoveAt(removeIndexA);
            }
            else
            {
                pointList.RemoveAt(removeIndexB);
                pointList.RemoveAt(removeIndexA - 1);
                indexList.RemoveAt(removeIndexB);
                indexList.RemoveAt(removeIndexA - 1);
            }
            pointCount = pointList.Count;

            quads.Add(outputEntry);
//        return outputEntry;
        }

        public static void DrawOriginPoints()
        {
            Color col = new Color(0, 1, 0, 0.5f);
            int count = points.Length;
            for(int i = 0; i < count; i++)
                Debug.DrawLine(JMath.ToV3(points[i]), JMath.ToV3(points[(i + 1) % count]), col);
        }

        public static void DrawCurrentPoints()
        {
            Color col = new Color(1, 0, 0, 0.75f);
            int count = pointList.Count;
            for (int i = 0; i < count; i++)
                Debug.DrawLine(JMath.ToV3(pointList[i]), JMath.ToV3(pointList[(i + 1) % count]), col);
        }

        public static void DrawQuads()
        {
            Color col = new Color(1, 1, 0, 0.5f);
            foreach(int[] quad in quads)
            {
                int quadCount = quad.Length;
                for (int i = 0; i < quadCount; i++)
                    Debug.DrawLine(JMath.ToV3(points[quad[i]]), JMath.ToV3(points[quad[(i + 1) % quadCount]]), col);
            }
        }
    }
}