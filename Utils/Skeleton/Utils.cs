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



using System.Collections.Generic;
using UnityEngine;

namespace BuildR2.ShapeOffset
{
    public class Utils
    {
        public static void CalculateNodeDirAng(Shape data, Node node)
        {
            Edge edgeA = null;
            Edge edgeB = null;
            int liveEdgeCount = data.liveEdges.Count;
            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge edge = data.liveEdges[e];
                if (edge.Contains(node))
                {
                    if (edgeA == null)
                        edgeA = edge;
                    else if (edgeB == null)
                        edgeB = edge;
                    else
                        return;//wtf?
                }
            }
            if (edgeA != null && edgeB != null)
            {
                Node x = edgeA.GetOtherNode(node);
                Node y = edgeB.GetOtherNode(node);
                if (edgeA.nodeA == node)
                    CalculateNodeDirAng(node, x, y);
                else
                    CalculateNodeDirAng(node, y, x);
            }
        }

        public static void CalculateNodeDirAng(Node a, Node x, Node y)
        {
            if (x == y)
            {
                a.direction = Vector2.zero;
                return;
            }

            Vector2 dirA = (x.position - a.position).normalized;
            Vector2 dirB = (a.position - y.position).normalized;

            if (Vector2.Dot(dirA, dirB) > 1f - Mathf.Epsilon)
            {
                a.direction = Vector2.zero;
                return;
            }

            Vector2 croA = Rotate(dirA, 90);
            Vector2 croB = Rotate(dirB, 90);
            Vector2 cross = (croA + croB).normalized;
            a.direction = cross;

            Vector2 adirA = dirA;
            Vector2 adirB = (y.position - a.position).normalized;
            float nodeAngle = SignAngleDirection(adirA, adirB);
            a.angle = nodeAngle;
//            a.startTangent = (dirA + dirB).normalized;
        }

        public static void CalculateNodeDirAngLeftGable(Node a, Node x, Node y)
        {
            if (x == y)
            {
                a.direction = Vector2.zero;
                return;
            }

            Vector2 dirA = (x.position - a.position).normalized;
            Vector2 dirB = (a.position - y.position).normalized;

            if (Vector2.Dot(dirA, dirB) > 1f - Mathf.Epsilon)
            {
                a.direction = Vector2.zero;
                return;
            }
            
            a.direction = dirB;

            Vector2 adirA = dirA;
            Vector2 adirB = (y.position - a.position).normalized;
            float nodeAngle = SignAngleDirection(adirA, adirB);
            a.angle = nodeAngle;
//            a.startTangent = (dirA + dirB).normalized;
        }

        public static Edge[] GetABEdge(Shape data, Node node)
        {
            Edge[] output = new Edge[2];
            foreach (Edge cedge in data.liveEdges)
            {
                if (cedge.nodeA == node) output[0] = cedge;
                if (cedge.nodeB == node) output[1] = cedge;
            }
            if (output[0] == null || output[1] == null)
                data.liveNodes.Remove(node);
            return output;
        }

        public static void RetireFormingEdge(Shape data, Node liveNode, Node newStaticNode)
        {
            if (data.formingEdges.ContainsKey(liveNode))
            {
                Edge formingEdge = data.formingEdges[liveNode];
                data.formingEdges.Remove(liveNode);
                formingEdge.ReplaceNode(liveNode, newStaticNode);
                formingEdge.UpdateValues();
                data.edges.Add(formingEdge);
            }
        }

        public static Edge NewFormingEdge(Shape data, Node staticNode, Node liveNode)
        {
            Edge newFormingEdge = new Edge(staticNode, liveNode);
            newFormingEdge.UpdateValues();
            if (data.formingEdges.ContainsKey(liveNode))
                data.formingEdges.Remove(liveNode);
            data.formingEdges.Add(liveNode, newFormingEdge);
            return newFormingEdge;
        }

        public static void ReplaceNode(Shape data, Node[] nodes, Node withNode)
        {
            foreach (Node node in nodes)
            {
                if (data.liveNodes.Contains(node))
                {
                    data.liveNodes.Remove(node);
                    data.liveNodes.Add(withNode);
                    data.formingEdges[node].ReplaceNode(node, withNode);
                }

                foreach (Edge edge in data.liveEdges)
                    edge.ReplaceNode(node, withNode);

                if (data.formingEdges.ContainsKey(node))
                {
                    data.formingEdges.Add(withNode, data.formingEdges[node]);
                    data.formingEdges.Remove(node);
                }
            }
        }

        /// <summary>
        /// Remove any live edges that are now parallel and will no longer serve a purpose
        /// </summary>
        /// <param name="data"></param>
        public static void CheckParrallel(Shape data)
        {
            int liveEdgeCount = data.liveEdges.Count;

            for (int a = 0; a < liveEdgeCount; a++)
            {
                for (int b = 0; b < liveEdgeCount; b++)
                {
                    if (a == b) continue;
                    Edge edgeA = data.liveEdges[a];
                    Edge edgeB = data.liveEdges[b];
                    Vector2 dirA = edgeA.direction;
                    Vector2 dirB = edgeB.direction;

                    if (ParallelLines(dirA, dirB))
                    {
                        Node nodeAA = edgeA.nodeA;
                        Node nodeAB = edgeA.nodeB;
                        Node nodeBA = edgeB.nodeA;
                        Node nodeBB = edgeB.nodeB;

                        bool connectedA = nodeAA == nodeBA || nodeAA == nodeBB;
                        bool connectedB = nodeAB == nodeBA || nodeAB == nodeBB;

                        if (connectedA && connectedB)//connected parallel lines serve no purpose
                        {
                            OffsetShapeLog.AddLine("Parallel Lines!");
                            OffsetShapeLog.AddLine(edgeA.ToString());
                            OffsetShapeLog.AddLine(edgeB.ToString());
                            OffsetShapeLog.DrawEdge(edgeA, new Color(1, 1, 0, 0.8f));
                            OffsetShapeLog.DrawEdge(edgeB, new Color(1, 0, 1, 0.8f));
                            OffsetShapeLog.LabelNode(nodeAA);
                            OffsetShapeLog.LabelNode(nodeAB);
                            OffsetShapeLog.LabelNode(nodeBA);
                            OffsetShapeLog.LabelNode(nodeBB);
                            Node newStaticNodeA = new Node(nodeAA.position, nodeAA.height);
                            Node newStaticNodeB = new Node(nodeAB.position, nodeAB.height);
                            data.liveEdges.Remove(edgeA);
                            data.liveEdges.Remove(edgeB);
                            data.liveNodes.Remove(nodeAA);
                            data.liveNodes.Remove(nodeAB);

                            newStaticNodeA = data.AddStaticNode(newStaticNodeA);
                            newStaticNodeB = data.AddStaticNode(newStaticNodeB);
                            data.edges.Add(edgeA);
                            data.edges.Add(edgeB);

                            RetireFormingEdge(data, nodeAA, newStaticNodeA);
                            RetireFormingEdge(data, nodeAB, newStaticNodeB);

                            NewFormingEdge(data, newStaticNodeA, nodeAA);
                            NewFormingEdge(data, newStaticNodeB, nodeAB);

                            CalculateNodeDirAng(data, nodeAA);
                            CalculateNodeDirAng(data, nodeAB);

                            a = 0;
                            b = 0;
                            liveEdgeCount = data.liveEdges.Count;
                        }
                        //                        else
                        //                        {
                        //                            OffsetShapeLog.AddLine("Parallel Lines but!");
                        //                            OffsetShapeLog.AddLine(edgeA.ToString());
                        //                            OffsetShapeLog.AddLine(edgeB.ToString());
                        //                        }
                    }
                }
            }
        }

        public static Vector3 ToV3(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
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

        public static float SignAngle(Vector2 from, Vector2 to)
        {
            Vector2 dir = (to - from).normalized;
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector2.up, dir);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngle(Vector2 dir)
        {
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector2.up, dir);
            if (cross.z > 0)
                angle = -angle;
            return angle;
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

        public static bool ParallelLines(Vector2 dirA, Vector2 dirB)
        {
            return (dirA.y * dirB.x - dirB.y * dirA.x) == 0;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

            return true;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            return true;
        }

        public static bool Intersects(Node nodeA, Node nodeB, out Vector2 intersection)
        {
            Vector2 a1 = nodeA.previousPosition;
            Vector2 a2 = nodeA.position;
            Vector2 b1 = nodeB.previousPosition;
            Vector2 b2 = nodeB.position;
            return Intersects(a1, a2, b1, b2, out intersection);
        }

        public static bool Intersects(Edge edgeA, Edge edgeB, out Vector2 intersection)
        {
            Vector2 a1 = edgeA.positionA;
            Vector2 a2 = edgeA.positionB;
            Vector2 b1 = edgeB.positionA;
            Vector2 b2 = edgeB.positionB;
            return Intersects(a1, a2, b1, b2, out intersection);
        }

        public static bool SweepIntersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, Vector2 l1, Vector2 l2, out Vector2 intersection, out float percent, float accuracy = 1)
        {
            intersection = Vector2.zero;
            percent = 0;
            Vector2 lineDelta = l2 - l1;
            Vector2 point = l2;

            b1 += -lineDelta;
            b2 += -lineDelta;
            Vector2[] poly = { a1, a2, b2, b1 };
            FlatBounds bounds = new FlatBounds(poly);
            if (!bounds.Contains(l1))
                return false;

//            bounds.DrawDebug(Color.yellow);

            Vector2 pointRight = point + new Vector2(Mathf.Max(bounds.width, bounds.height), 0);
            //            Vector2 pointRight = point - lineDelta * bounds.width * bounds.height;
//            Debug.DrawLine(ToV3(point), ToV3(pointRight), Color.cyan);

            int numberOfPolyPoints = poly.Length;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[(i + 1) % numberOfPolyPoints];
//                Debug.DrawLine(ToV3(p0), ToV3(p1), Color.blue);
                if (FastLineIntersection(point, pointRight, p0, p1))
                    numberOfCrossOvers++;
            }

            if (numberOfCrossOvers % 2 == 0)
                return false;//point not within shape
            bounds.DrawDebug(Color.green);

            Vector2 delta1 = b1 - a1;
            Vector2 delta2 = b2 - a2;
            float maxDelta = Mathf.Max(delta1.magnitude, delta2.magnitude);
            int iterations = Mathf.CeilToInt(Mathf.Sqrt(maxDelta / accuracy));

            for (int i = 0; i < iterations; i++)
            {
                Vector2 c1 = Vector2.Lerp(a1, b1, 0.5f);
                Vector2 c2 = Vector2.Lerp(a2, b2, 0.5f);
                if (!Ccw(c1, c2, point))
                {
                    a1 = c1;
                    a2 = c2;
                    percent = Mathf.Lerp(percent, 1, 0.5f);
                }
                else
                {
                    b1 = c1;
                    b2 = c2;
                    percent = Mathf.Lerp(0, percent, 0.5f);
                }
            }

//            Vector2 x1 = Vector2.Lerp(a1, b1, 0.5f);
//            Vector2 x2 = Vector2.Lerp(a2, b2, 0.5f);
//            float dist1 = Vector2.Distance(x1, point);
//            float dist2 = Vector2.Distance(x2, point);
//            float xpercent = dist1 / (dist1 + dist2);
//            intersection = Vector2.Lerp(x1, x2, xpercent);
            intersection = Vector2.Lerp(l1, l2, percent);//translate the point to the real movement point
            Debug.DrawLine(ToV3(intersection), ToV3(intersection) + Vector3.up * 5, Color.red);
            return true;
        }


        public static bool SweepIntersects2(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, Vector2 l1, Vector2 l2, out Vector2 intersection, out float percent, float accuracy = 1, bool debug = false)
        {
            intersection = Vector2.zero;
            percent = 0;
            Vector2[] poly = { a1, a2, b2, b1 };
            FlatBounds edgeBounds = new FlatBounds(poly);
            FlatBounds lineBounds = new FlatBounds(new []{ l1, l2 });
            if(!edgeBounds.Overlaps(lineBounds, true))
            {
                if (debug) edgeBounds.DrawDebug(Color.cyan);
                if (debug) lineBounds.DrawDebug(Color.green);
                if (debug) Debug.DrawLine(ToV3(l1), ToV3(l2), Color.cyan);
                return false;
            }
            
            int numberOfPolyPoints = poly.Length;
            bool lineIntersectsShape = false;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[(i + 1) % numberOfPolyPoints];
                if (debug) Debug.DrawLine(ToV3(p0), ToV3(p1), Color.blue);
                if (FastLineIntersection(p0, p1, l1, l2))
                {
                    lineIntersectsShape = true;
                    break;
                }
            }

            if (!lineIntersectsShape)//line never crosses shape
            {

                Vector2 right = new Vector2(Mathf.Max(edgeBounds.width, edgeBounds.height), 0);
                int shapeIntersections = 0;
                for (int i = 0; i < numberOfPolyPoints; i++)
                {
                    Vector2 p0 = poly[i];
                    Vector2 p1 = poly[(i + 1) % numberOfPolyPoints];
                    if (debug) Debug.DrawLine(ToV3(p0) + Vector3.up * 5, ToV3(p1) + Vector3.up * 5, Color.blue);
                    if (FastLineIntersection(l1, l1 + right, p0, p1))
                    {
                        if (debug) Debug.DrawLine(ToV3(l1), ToV3(l1) + Vector3.up * 5, Color.magenta);
                        shapeIntersections++;
                    }
                    if (FastLineIntersection(l2, l2 + right, p0, p1))
                    {
                        if (debug) Debug.DrawLine(ToV3(l1), ToV3(l1) + Vector3.up * 5, Color.magenta);
                        shapeIntersections++;
                        break;
                    }
                }

                if(shapeIntersections % 2 == 0)
                    return false;//line not within shape
            }
            if (debug) edgeBounds.DrawDebug(Color.green);

            Vector2 delta1 = b1 - a1;
            Vector2 delta2 = b2 - a2;
            float maxDelta = Mathf.Max(delta1.magnitude, delta2.magnitude);
            int iterations = Mathf.CeilToInt(maxDelta / accuracy);
            bool initalState = Ccw(a1, a2, l1);
            for (int i = 1; i < iterations; i++)
            {
                percent = i / (iterations-1f);
                
                Vector2 e1 = Vector2.Lerp(a1, b1, percent);
                Vector2 e2 = Vector2.Lerp(a2, b2, percent);
                if(debug) Debug.DrawLine(ToV3(e1), ToV3(e2), new Color(1,0,1,0.5f));
                Vector2 p = Vector2.Lerp(l1, l2, percent);

                bool currentState = Ccw(e1, e2, p);

                if(currentState != initalState || i == iterations-1)
                {
                    float dist1 = Vector2.Distance(e1, p);
                    float dist2 = Vector2.Distance(e2, p);
                    float xpercent = dist1 / (dist1 + dist2);
                    intersection = Vector2.Lerp(e1, e2, xpercent);
                    if (debug) Debug.DrawLine(ToV3(e1), ToV3(e2), Color.red);
                    if (debug) Debug.DrawLine(ToV3(intersection), ToV3(intersection) + Vector3.up*10, Color.red);

                    break;
                }
            }
            return true;
        }

        public static bool FastLineIntersection(Vector2Int a1, Vector2Int a2, Vector2Int b1, Vector2Int b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        private static bool Ccw(Vector2Int p1, Vector2Int p2, Vector2Int p3)
        {
            return ((p2.vx - p1.vx) * (p3.vy - p1.vy) > (p2.vy - p1.vy) * (p3.vx - p1.vx));
        }

        private static bool Ccw(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }
        //        public static IEnumerable<Sweep> WhenLineSweepsPoint(LineSegment pathOfLineStartPoint,
        //                                                     LineSegment pathOfLineEndPoint,
        //                                                     Point point)
        //        {
        //            var a = point - pathOfLineStartPoint.Start;
        //            var b = -pathOfLineStartPoint.Delta;
        //            var c = pathOfLineEndPoint.Start - pathOfLineStartPoint.Start;
        //            var d = pathOfLineEndPoint.Delta - pathOfLineStartPoint.Delta;
        //
        //            return from t in QuadraticRoots(b.Cross(d), a.Cross(d) + b.Cross(c), a.Cross(c))
        //                   where t >= 0 && t <= 1
        //                   let start = pathOfLineStartPoint.LerpAcross(t)
        //                   let end = pathOfLineEndPoint.LerpAcross(t)
        //                   let s = point.LerpProjectOnto(new LineSegment(start, end))
        //                   where s >= 0 && s <= 1
        //                   orderby t
        //                   select new Sweep(timeProportion: t, acrossProportion: s);
        //        }

        /// <summary>
        /// Calcaulte the Tangent from a direction
        /// </summary>
        /// <param name="tangentDirection">the normalised right direction of the tangent</param>
        public static Vector4 CalculateTangent(Vector3 tangentDirection)
        {
            Vector4 tangent = new Vector4();
            tangent.x = tangentDirection.x;
            tangent.y = tangentDirection.y;
            tangent.z = tangentDirection.z;
            tangent.w = 1;//TODO: Check whether we need to flip the bi normal - I don't think we do with these planes
            return tangent;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        /// <param name="points">Only three points will be used in calculation</param>
        public static Vector3 CalculateNormal(Vector3[] points)
        {
            if (points.Length < 3) return Vector3.down;//most likely to look wrong
            return CalculateNormal(points[0], points[1], points[2]);
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static Vector3 CalculateNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized).normalized;
        }

        public static bool Clockwise(Vector2[] poly)
        {
            int polySize = poly.Length;
            float polyEdgeSum = 0;
            for (int p = 0; p < polySize; p++)
            {
                Vector2 p0 = poly[p];
                Vector2 p1 = poly[(p + 1) % polySize];
                polyEdgeSum += (p1.x + p0.x) * (p1.y + p0.y);
            }
            return polyEdgeSum >= 0;
        }
    }
}