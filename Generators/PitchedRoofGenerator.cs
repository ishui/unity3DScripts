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
using BuildR2.ShapeOffset;
using JaspLib;

namespace BuildR2
{
    public class PitchedRoofGenerator
    {
        public static bool Generate(BuildRMesh mesh, BuildRCollider collider, Vector2[] points, int[] facadeIndices, float roofBaseHeight, IVolume volume, Rect clampUV)
        {
            Roof design = volume.roof;
            OffsetSkeleton offsetPoly = new OffsetSkeleton(points);
            offsetPoly.direction = 1;
            offsetPoly.Execute();
            Shape shape = offsetPoly.shape;
            int submesh = mesh.submeshLibrary.SubmeshAdd(design.mainSurface);// surfaceMapping.IndexOf(design.mainSurface);
            int wallSubmesh = mesh.submeshLibrary.SubmeshAdd(design.wallSurface);//surfaceMapping.IndexOf(design.wallSurface);

            if(shape == null) return false;

            List<Edge> edges = new List<Edge>(shape.edges);
            List<Edge> baseEdges = new List<Edge>(shape.baseEdges);

            float shapeHeight = shape.HeighestPoint();
            float designHeight = design.height;
            float heightScale = designHeight / shapeHeight;

            Vector2 clampUVScale = Vector2.one;
            if (clampUV.width > 0)
            {
                FlatBounds bounds = new FlatBounds();
                for (int fvc = 0; fvc < points.Length; fvc++)
                    bounds.Encapsulate(points[fvc]);
                clampUVScale.x = bounds.width / clampUV.width;
                clampUVScale.y = bounds.height / clampUV.height;
            }

            Dictionary<Node, int> shapeConnectionCount = new Dictionary<Node, int>();
            Dictionary<Node, List<Node>> shapeConnections = new Dictionary<Node, List<Node>>();
            int edgeCount = edges.Count;
            for (int e = 0; e < edgeCount; e++)
            {
                Edge edge = edges[e];

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
            }

            int baseEdgeCount = baseEdges.Count;
            for (int b = 0; b < baseEdgeCount; b++)
            {
                Edge baseEdge = baseEdges[b];
                Node nodeA = baseEdge.nodeA;
                Node nodeB = baseEdge.nodeB;

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
                }

                int edgeShapeCount = edgeShape.Count;
                if(edgeShapeCount < 3) continue;
//                Debug.Log("Generate edgeShapeCount "+ edgeShapeCount);

                Vector3[] verts = new Vector3[edgeShapeCount];

                Vector2[] uvs = new Vector2[edgeShapeCount];
                Vector3 baseShapeDirection = ShapeOffset.Utils.ToV3(nodeB.position - nodeA.position).normalized;
                float uvAngle = JMath.SignAngle(new Vector2(baseShapeDirection.x, baseShapeDirection.z).normalized) - 90;

                Vector2[] faceShape = new Vector2[edgeShapeCount];
                Vector3[] normals = new Vector3[edgeShapeCount];
                Vector4[] tangents = new Vector4[edgeShapeCount];
                //                Vector3 normal = Vector3.up;//BuildRMesh.CalculateNormal(); TODO
                Vector4 tangent = BuildRMesh.CalculateTangent(baseShapeDirection);
                for (int i = 0; i < edgeShapeCount; i++)//what on earth did I write here?
                {
                    Vector3 newVert = new Vector3(edgeShape[i].position.x, edgeShape[i].height * heightScale + roofBaseHeight, edgeShape[i].position.y);
                    verts[i] = newVert;
                    
                    Vector2 baseUV = new Vector2(newVert.x - verts[0].x, newVert.z - verts[0].z);
                    Vector2 newUV = Vector2.zero;
                    if(i != 0)
                        newUV = JMath.Rotate(baseUV, uvAngle);
                    if (clampUV.width > Mathf.Epsilon)
                    {
                        newUV.x = Mathf.Clamp(clampUV.x + newUV.x / clampUVScale.x, clampUV.xMin, clampUV.xMax);
                        newUV.y = Mathf.Clamp(clampUV.y + newUV.y / clampUVScale.y, clampUV.yMin, clampUV.yMax);
                    }
                    else
                    {
                        if(i != 0)
                        {
                            float faceHeight = edgeShape[i].height * heightScale;
                            newUV.y = Mathf.Sqrt((newUV.y * newUV.y) + (faceHeight * faceHeight));//hypotenuse of roof to give length of roof face
                            if(design.mainSurface != null)
                                newUV = design.mainSurface.CalculateUV(newUV);
                        }
                    }
                    uvs[i] = newUV;

                    faceShape[i] = edgeShape[i].position;//used for triangulation
                    //                    normals[i] = normal;
                    tangents[i] = tangent;
                }
//                int[] tris = EarClipper.Triangulate(faceShape, 0, -1);
                int[] tris = Poly2TriWrapper.Triangulate(faceShape, true);
                int triCount = tris.Length;
                
                Vector3 normal = (verts.Length > 2 && triCount > 2) ? BuildRMesh.CalculateNormal(verts[tris[0]], verts[tris[1]], verts[tris[2]]) : Vector3.up;
                for (int i = 0; i < edgeShapeCount; i++)
                    normals[i] = normal;

                mesh.AddData(verts, uvs, tris, normals, tangents, submesh);

                //gable
                bool isGabled = volume[facadeIndices[b]].isGabled;
                if (isGabled)
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
                                Vector3 g0 = b0;
                                Vector3 g1 = b1;
                                int topIndex = 0;
                                for (int tx = 0; tx < 3; tx++)
                                    if (tris[t + tx] != 0 && tris[t + tx] != beB) topIndex = tris[t + tx];
                                Vector3 b2 = verts[topIndex];

                                Vector3 baseV = b1 - b0;
                                Vector3 dir = baseV.normalized;
                                Vector3 face = Vector3.Cross(Vector3.up, dir).normalized;
                                Vector3 up = Vector3.Project(b2 - b0, Vector3.up);

                                //clear triangle
                                tris[t] = 0;
                                tris[t + 1] = 0;
                                tris[t + 2] = 0;

                                bool simpleGable = volume[facadeIndices[b]].simpleGable;
                                Gable gableStyle = volume[facadeIndices[b]].gableStyle;
                                float thickness = volume[facadeIndices[b]].gableThickness;
                                float additionalHeight = volume[facadeIndices[b]].gableHeight;
                                float height = up.magnitude + additionalHeight;

                                if (simpleGable || gableStyle != null)
                                {
                                    Vector3 pitchVectorA = (b2 - b0).normalized;
                                    Vector3 pitchVectorB = (b2 - b1).normalized;
                                    float angle = Vector3.Angle(-face, pitchVectorA);
                                    float scale = Mathf.Cos(angle / 57.2957795f);
                                    b0 += pitchVectorA * (thickness * (1 / scale));
                                    b1 += pitchVectorB * (thickness * (1 / scale));
                                }

                                Vector3 center = Vector3.Lerp(b0, b1, 0.5f);
                                up = Vector3.Project(b2 - b0, Vector3.up);//recalculate after b change(?)
                                Vector3 b3 = center + up;
                                if (simpleGable)//generate a simple gable
                                {
                                    //generate simple gable based on roof
                                    Vector3 gCenter = Vector3.Lerp(g0, g1, 0.5f);
                                    Vector3 gBaseUp = Vector3.up * additionalHeight;
                                    Vector3 gUp = up.normalized * height;
                                    Vector3 gBack = -face * thickness;
                                    //todo further calculations
                                    //face
                                    mesh.AddPlane(g0, g1, g0 + gBaseUp, g1 + gBaseUp, wallSubmesh);
                                    mesh.AddTri(g1 + gBaseUp, g0 + gBaseUp, gCenter + gUp, dir, wallSubmesh);
                                    //backface
                                    mesh.AddPlane(g1 + gBack, g0 + gBack, g1 + gBaseUp + gBack, g0 + gBaseUp + gBack, wallSubmesh);
                                    mesh.AddTri(g0 + gBack + gBaseUp, g1 + gBack + gBaseUp, b3 + gBaseUp, -dir, wallSubmesh);
                                    //left
                                    mesh.AddPlane(g0 + gBack, g0, g0 + gBaseUp + gBack, g0 + gBaseUp, wallSubmesh);
                                    mesh.AddPlane(g0 + gBaseUp + gBack, g0 + gBaseUp, b3 + gBaseUp, gCenter + gUp, wallSubmesh);
                                    //right
                                    mesh.AddPlane(g1, g1 + gBack, g1 + gBaseUp, g1 + gBaseUp + gBack, wallSubmesh);
                                    mesh.AddPlane(g1 + gBaseUp, g1 + gBaseUp + gBack, gCenter + gUp, b3 + gBaseUp, wallSubmesh);

                                }
                                else if (volume[facadeIndices[b]].gableStyle != null)
                                {
                                    Vector2 baseUV = new Vector2(0, volume.planHeight);
                                    GableGenerator.Generate(ref mesh, gableStyle, g0, g1, height, thickness, baseUV);
                                }
                                else
                                {
                                    mesh.AddTri(b0, b3, b1, dir, submesh);//face - no separate gable
                                }

                                mesh.AddTri(b0, b2, b3, face, submesh);//left
                                mesh.AddTri(b1, b3, b2, -face, submesh);//right
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}