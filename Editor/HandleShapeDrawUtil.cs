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
using UnityEditor;

namespace BuildR2
{
    public class HandleShapeDrawUtil
    {
        private struct HandlePoly
        {
            public Vector3[] poly;
            public Color col;
        }


        private Dictionary<int, List<HandlePoly>> _content = new Dictionary<int, List<HandlePoly>>();
        private List<int> _levels = new List<int>();

        public void AddContent(int index, Color col, params Vector3[] poly)
        {
            HandlePoly newHandlePoly = new HandlePoly();
            newHandlePoly.poly = poly;
            newHandlePoly.col = col;
            if(!_content.ContainsKey(index))
            {
                _content.Add(index, new List<HandlePoly>());
                _levels.Add(index);
            }
            _content[index].Add(newHandlePoly);
        }

        public void AddContent(int index, Color col, List<Vector3> poly)
        {
            HandlePoly newHandlePoly = new HandlePoly();
            newHandlePoly.poly = poly.ToArray();
            newHandlePoly.col = col;
            if (!_content.ContainsKey(index))
            {
                _content.Add(index, new List<HandlePoly>());
                _levels.Add(index);
            }
            _content[index].Add(newHandlePoly);
        }

        public void Shape(int index, Vector3[] points, Color fillColor)
        {
//            int[] tris = EarClipper.Triangulate(points);
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
//                Handles.DrawAAConvexPolygon(f0, f1, f2);
                AddContent(index, fillColor, f0, f1, f2);
            }
        }

        public void ShapeWithLines(int index, Vector3[] points, Color lineColor, Color fillColor, bool highlight, Color highlightcolour, Color angleColour)
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
//                Handles.DrawAAConvexPolygon(f0, f1, f2);
                AddContent(index, fillColor, f0, f1, f2);
            }

            //TODO encapsulate this later
            Handles.color = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                if (highlight)
                {
                    Vector3 diff = p1 - p0;
                    if (diff.x * diff.x < 0.001f || diff.z * diff.z < 0.001f)
                        Handles.color = highlightcolour;
                    else
                        Handles.color = angleColour;
                }
                Handles.DrawLine(p0, p1);
            }
        }

        public void Clear()
        {
            _content.Clear();
            _levels.Clear();
        }

        public void Draw()
        {
            for(int i = 0; i < _levels.Count; i++)
            {
                int index = _levels[i];
                if (_content.ContainsKey(index))
                {
                    List<HandlePoly> indexContent = _content[index];
                    int contentCount = indexContent.Count;
                    for(int c = 0; c < contentCount; c++)
                    {
                        HandlePoly content = indexContent[c];
                        Handles.color = content.col;
                        Handles.DrawAAConvexPolygon(content.poly);
                    }
                }
            }
        }
    }
}