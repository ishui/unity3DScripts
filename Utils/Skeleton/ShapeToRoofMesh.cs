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
using System.Collections.Generic;
using JaspLib;

namespace BuildR2.ShapeOffset
{
    public class ShapeToRoofMesh
    {
        public static void MansardRoof(ref BuildRMesh mesh, Vector2[] points, bool[] gabled, float roofBaseHeight, Roof design, List<Surface> surfaceMapping)
        {
            float floorWidth = design.floorDepth;
            float roofDepth = design.depth;
            float roofHeight = design.height;

            //mansard floor
            if (floorWidth > 0)
            {
                OffsetSkeleton offsetFloorPoly = new OffsetSkeleton(points, null, floorWidth);
                offsetFloorPoly.direction = 1;
                offsetFloorPoly.Execute();
                Shape floorShape = offsetFloorPoly.shape;
                int floorSubmesh = surfaceMapping.IndexOf(design.floorSurface);
                ToMesh(ref mesh, floorShape, gabled, roofBaseHeight, 0, floorSubmesh, design.floorSurface);

                points = new Vector2[floorShape.terminatedNodeCount];
                for (int i = 0; i < floorShape.terminatedNodeCount; i++)
                    points[i] = floorShape.TerminatedNode(i).position;
            }

            //mansard roof
            OffsetSkeleton offsetRoofPoly = new OffsetSkeleton(points, null, roofDepth);
            offsetRoofPoly.direction = 1;
            offsetRoofPoly.Execute();
            Shape roofShape = offsetRoofPoly.shape;
            int roofSubmesh = surfaceMapping.IndexOf(design.mainSurface);
            ToMesh(ref mesh, roofShape, gabled, roofBaseHeight, roofHeight, roofSubmesh, design.mainSurface);

            points = new Vector2[roofShape.terminatedNodeCount];
            for (int i = 0; i < roofShape.terminatedNodeCount; i++)
                points[i] = roofShape.TerminatedNode(i).position;

            //mansard top
            int topSubmesh = surfaceMapping.IndexOf(design.floorSurface);
            ToMesh(ref mesh, points, roofBaseHeight + roofHeight, topSubmesh, design.floorSurface);
        }

        public static void Gambrel(ref BuildRMesh mesh, Vector2[] points, bool[] gabled, float roofBaseHeight, Roof design, List<Surface> surfaceMapping)
        {
            float roofDepth = design.depth;
            float roofHeightB = design.heightB;
            float roofHeight = design.height - roofHeightB;
            int roofSubmesh = surfaceMapping.IndexOf(design.mainSurface);

            for (int p = 0; p < points.Length; p++)
            {
                Vector3 p0 = Utils.ToV3(points[p]);
                Vector3 p1 = Utils.ToV3(points[(p + 1) % points.Length]);
                Debug.DrawLine(p0, p1);
            }

            if (roofDepth > 0)
            {
                //slope one
                OffsetSkeleton offsetRoofPoly = new OffsetSkeleton(points, null, roofDepth);
                offsetRoofPoly.direction = 1;
                offsetRoofPoly.Execute();
                Shape roofShape = offsetRoofPoly.shape;
                ToMesh(ref mesh, roofShape, gabled, roofBaseHeight, roofHeightB, roofSubmesh, design.mainSurface);

                points = new Vector2[roofShape.terminatedNodeCount];
                for (int i = 0; i < roofShape.terminatedNodeCount; i++)
                    points[i] = roofShape.TerminatedNode(i).position;
            }
            else
            {
                roofHeight = design.height;
                roofHeightB = 0;
            }

            //slope two
            OffsetSkeleton offsetRoofPolyB = new OffsetSkeleton(points);
            offsetRoofPolyB.direction = 1;
            offsetRoofPolyB.Execute();
            Shape roofShapeB = offsetRoofPolyB.shape;
            ToMesh(ref mesh, roofShapeB, gabled, roofBaseHeight + roofHeightB, roofHeight, roofSubmesh, design.mainSurface);
        }

        public static void OverhangUnderside(ref BuildRMesh mesh, Vector2[] facadePoints, Vector2[] outerPoints, float roofBaseHeight, Roof design)
        {
            Vector2[][] facadeHole = new Vector2[1][];
            facadeHole[0] = outerPoints;
            int[] topTris = Poly2TriWrapper.Triangulate(outerPoints, false, facadeHole);
            //            Array.Reverse(topTris);

            int facadePointCount = facadePoints.Length;
            int outerPointCount = outerPoints.Length;
            int usePoints = facadePointCount + outerPointCount;
            Vector2[] points = new Vector2[usePoints];
            for (int f = 0; f < facadePointCount; f++)
                points[f] = facadePoints[f];
            for (int o = 0; o < outerPointCount; o++)
                points[o + facadePointCount] = outerPoints[o];

            int submesh = mesh.submeshLibrary.SubmeshAdd(design.floorSurface);

            int vertCount = points.Length;
            Vector3[] verts = new Vector3[vertCount];
            for (int i = 0; i < vertCount; i++)
                verts[i] = new Vector3(points[i].x, roofBaseHeight, points[i].y);
            Vector2[] uvs = new Vector2[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector4[] tangents = new Vector4[vertCount];
            Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);
            Surface surface = design.floorSurface;
            for (int v = 0; v < vertCount; v++)
            {
                if (surface != null)
                    uvs[v] = surface.CalculateUV(points[v]);
                normals[v] = Vector3.down;
                tangents[v] = tangent;
            }

            mesh.AddData(verts, uvs, topTris, normals, tangents, submesh);
        }

        private static void ToMesh(ref BuildRMesh mesh, Vector2[] points, float height, int submesh, Surface surface)
        {
            int vertCount = points.Length;
            Vector3[] verts = new Vector3[vertCount];
            for (int i = 0; i < vertCount; i++)
                verts[i] = new Vector3(points[i].x, height, points[i].y);
            Vector2[] uvs = new Vector2[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector4[] tangents = new Vector4[vertCount];
            Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);
            for (int v = 0; v < vertCount; v++)
            {
                if (surface != null)
                    uvs[v] = surface.CalculateUV(points[v]);
                normals[v] = Vector3.up;
                tangents[v] = tangent;
            }

//            int[] topTris = EarClipper.Triangulate(points);
            int[] topTris = Poly2TriWrapper.Triangulate(points);
            for (int t = 0; t < topTris.Length; t += 3)
            {
                int ia = topTris[t];
                int ib = topTris[t + 1];
                int ic = topTris[t + 2];
                Debug.DrawLine(verts[topTris[ia]], verts[topTris[ib]]);
                Debug.DrawLine(verts[topTris[ib]], verts[topTris[ic]]);
                Debug.DrawLine(verts[topTris[ic]], verts[topTris[ia]]);
            }
            mesh.AddData(verts, uvs, topTris, normals, tangents, submesh);
        }

        private static void ToMesh(ref BuildRMesh mesh, Shape shape, bool[] gabled, float roofBaseHeight, float meshHeight, int submesh, Surface surface)
        {
            List<Edge> edges = new List<Edge>(shape.edges);
            List<Edge> baseEdges = new List<Edge>(shape.baseEdges);

            float shapeHeight = shape.HeighestPoint();
            float designHeight = meshHeight;
            float heightScale = designHeight / shapeHeight;

            Dictionary<Node, int> shapeConnectionCount = new Dictionary<Node, int>();
            Dictionary<Node, List<Node>> shapeConnections = new Dictionary<Node, List<Node>>();
            int edgeCount = edges.Count;
            for (int e = 0; e < edgeCount; e++)
            {
                Edge edge = edges[e];


                //                                                Node nodeA = edge.nodeA;
                //                                                Node nodeB = edge.nodeB;
                //                                                Vector3 na = new Vector3(nodeA.position.x, roofBaseHeight * 1.5f, nodeA.position.y);
                //                                                Vector3 nb = new Vector3(nodeB.position.x, roofBaseHeight * 1.5f, nodeB.position.y);
                //                                                Debug.DrawLine(na, nb, Color.blue);

                if (edge.length < Mathf.Epsilon)
                    continue;

                if (!shapeConnectionCount.ContainsKey(edge.nodeA))
                {
                    shapeConnectionCount.Add(edge.nodeA, 0);//start at zero - we need two edges to make a shape...
                    shapeConnections.Add(edge.nodeA, new List<Node> { edge.nodeB });
                }
                else
                {
                    shapeConnectionCount[edge.nodeA]++;
                    if (!shapeConnections[edge.nodeA].Contains(edge.nodeB))
                        shapeConnections[edge.nodeA].Add(edge.nodeB);
                }

                if (!shapeConnectionCount.ContainsKey(edge.nodeB))
                {
                    shapeConnectionCount.Add(edge.nodeB, 0);//start at zero - we need two edges to make a shape...
                    shapeConnections.Add(edge.nodeB, new List<Node> { edge.nodeA });
                }
                else
                {
                    shapeConnectionCount[edge.nodeB]++;
                    if (!shapeConnections[edge.nodeB].Contains(edge.nodeA))
                        shapeConnections[edge.nodeB].Add(edge.nodeA);
                }


                //                Vector3 na = new Vector3(edge.nodeA.position.x + 75, roofBaseHeight * 2 + edge.nodeA.height, edge.nodeA.position.y);
                //                Vector3 nb = new Vector3(edge.nodeB.position.x + 75, roofBaseHeight * 2 + edge.nodeB.height, edge.nodeB.position.y);
                //                Debug.DrawLine(na, nb, new Color(1,0,1,0.24f));
                //
                //                GizmoLabel.Label(edge.nodeA.ToString(), na);
                //                GizmoLabel.Label(edge.nodeB.ToString(), nb);
            }

            int baseEdgeCount = baseEdges.Count;
            for (int b = 0; b < baseEdgeCount; b++)
            {
                Edge baseEdge = baseEdges[b];
                Node nodeA = baseEdge.nodeA;
                Node nodeB = baseEdge.nodeB;
                //                Color col = new Color(Random.value, Random.value, Random.value, 0.5f);
                //                Vector3 na = new Vector3(nodeA.position.x + 75, roofBaseHeight * 2, nodeA.position.y);
                //                Vector3 nb = new Vector3(nodeB.position.x + 75, roofBaseHeight * 2, nodeB.position.y);
                //                Debug.DrawLine(na, nb, col);//base edge

                Node currentNode = nodeA;
                Node lastNode = nodeB;
                int itMax = 50;
                List<Node> edgeShape = new List<Node>() { nodeA };

                while (currentNode != nodeB)
                {
                    List<Node> nodeConnections = shapeConnections[currentNode];
                    int nodeConnectionCount = nodeConnections.Count;
                    float minAngle = Mathf.Infinity;
                    Node nextNode = null;
                    Vector2 currentDirection = (currentNode.position - lastNode.position).normalized;
                    for (int n = 0; n < nodeConnectionCount; n++)
                    {
                        Node connectingNode = nodeConnections[n];
                        if (connectingNode == lastNode) continue;
                        Vector2 nextDirection = (connectingNode.position - currentNode.position).normalized;
                        float nodeAngle = JMath.SignAngleDirection(currentDirection, nextDirection);
                        if (nodeAngle < minAngle)
                        {
                            minAngle = nodeAngle;
                            nextNode = connectingNode;
                        }
                    }
                    if (nextNode != null)
                    {
                        edgeShape.Add(nextNode);
                        lastNode = currentNode;
                        currentNode = nextNode;
                    }


                    itMax--;
                    if (itMax < 0) break;
                    //                    if(edgeShape.Count == 3) break;
                }

                int edgeShapeCount = edgeShape.Count;

                Vector3[] verts = new Vector3[edgeShapeCount];

                Vector2[] uvs = new Vector2[edgeShapeCount];
                Vector3 baseShapeDirection = Utils.ToV3(nodeB.position - nodeA.position).normalized;
                float uvAngle = JMath.SignAngle(new Vector2(baseShapeDirection.x, baseShapeDirection.z).normalized) - 90;

                Vector2[] faceShape = new Vector2[edgeShapeCount];
                Vector3[] normals = new Vector3[edgeShapeCount];
                Vector4[] tangents = new Vector4[edgeShapeCount];
//                Vector3 normal = Vector3.up;//BuildRMesh.CalculateNormal(); TODO
                Vector4 tangent = BuildRMesh.CalculateTangent(baseShapeDirection);
                for (int i = 0; i < edgeShapeCount; i++)
                {
                    float testHAdd = 0;//5 + b;
                    Vector3 newVert = new Vector3(edgeShape[i].position.x, edgeShape[i].height * heightScale + roofBaseHeight + testHAdd, edgeShape[i].position.y);
                    verts[i] = newVert;

                    Vector2 baseUV = (i == 0) ? Vector2.zero : new Vector2(newVert.x - verts[0].x, newVert.z - verts[0].z);
                    Vector2 newUV = JMath.Rotate(baseUV, uvAngle);
                    float faceHeight = edgeShape[i].height * heightScale;
                    newUV.y = Mathf.Sqrt((newUV.y * newUV.y) + (faceHeight * faceHeight));
                    if (surface != null)
                        newUV = surface.CalculateUV(newUV);
                    uvs[i] = newUV;

                    faceShape[i] = edgeShape[i].position;//used for triangulation
//                    normals[i] = normal;
                    tangents[i] = tangent;
                }
//                int[] tris = EarClipper.Triangulate(faceShape, 0, -1);
                int[] tris = Poly2TriWrapper.Triangulate(faceShape);
                int triCount = tris.Length;

                Vector3 normal = BuildRMesh.CalculateNormal(verts[tris[0]], verts[tris[1]], verts[tris[2]]);
//                Vector3[] normCal = new Vector3[edgeShapeCount];
//                for (int t = 0; t < triCount; t += 3)
//                {
//                    int[] triIndicies = {tris[t], tris[t + 1], tris[t + 2]};
//                    Vector3 newNormal = BuildRMesh.CalculateNormal(verts[triIndicies[0]], verts[triIndicies[1]], verts[triIndicies[2]]);
//                    for(int i = 0; i < 3; i++)
//                        normCal[triIndicies[i]] = newNormal;
//                }
                for(int i = 0; i < edgeShapeCount; i++)
                    normals[i] = normal;//normCal[i].normalized;

                mesh.AddData(verts, uvs, tris, normals, tangents, submesh);


                if (gabled[b])
                {
                    for (int t = 0; t < triCount; t += 3)
                    {
                        if (tris[t] == 0 || tris[t + 1] == 0 || tris[t + 2] == 0)
                        {
                            int beB = edgeShapeCount - 1;
                            if (tris[t] == beB || tris[t + 1] == beB || tris[t + 2] == beB)
                            {

                                Vector3 b0 = verts[0];
                                Vector3 b1 = verts[beB];
                                int topIndex = 0;
                                for (int tx = 0; tx < 3; tx++)
                                    if (tris[t + tx] != 0 && tris[t + tx] != beB) topIndex = tris[t + tx];
                                Vector3 b2 = verts[topIndex];

                                Vector3 baseV = b1 - b0;
                                Vector3 dir = baseV.normalized;
                                Vector3 face = Vector3.Cross(Vector3.up, dir);
                                //                                float length = baseV.magnitude;
                                Vector3 center = Vector3.Lerp(b0, b1, 0.5f);
                                Vector3 up = Vector3.Project(b2 - b0, Vector3.up);
                                Vector3 b3 = center + up;
                                mesh.AddTri(b0, b2, b3, face, submesh);//left
                                mesh.AddTri(b1, b3, b2, -face, submesh);//right
                                mesh.AddTri(b0, b3, b1, dir, submesh);//face

                                //clear triangle
                                tris[t] = 0;
                                tris[t + 1] = 0;
                                tris[t + 2] = 0;
                            }
                        }
                    }
                }

                //                                for (int i = 0; i < edgeShapeCount; i++)
                //                                {
                //                                    Node nodeAS = edgeShape[i];
                //                                    Node nodeBS = edgeShape[(i + 1) % edgeShapeCount];
                //                                    Vector3 nas = new Vector3(nodeAS.position.x + 75, roofBaseHeight * 5 + b, nodeAS.position.y);
                //                                    Vector3 nbs = new Vector3(nodeBS.position.x + 75, roofBaseHeight * 5 + b, nodeBS.position.y);
                //                                    Debug.DrawLine(nas, nbs + Vector3.up, col);//Color.yellow);
                //                                }
            }

            //Assumption - each based edge is a single shape
            //There are no shapes without a base edge
            //Enumerate through the base edges
            //Build the shape
            //use angle to find shape clockwise or something
            //triangulate the shape and add to mesh
            //node data will provide height information
            //???
            //profit



            //            int itMax = 5000;
            //            while(unmappedNodes.Count > 0)
            //            {
            //                Node currentNode = unmappedNodes[0];
            //                unmappedNodes.RemoveAt(0);
            //
            //
            //                itMax--;
            //                if(itMax < 0)
            //                    return;
            //            }
        }
    }
}