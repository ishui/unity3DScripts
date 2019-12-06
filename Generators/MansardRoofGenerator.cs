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

namespace BuildR2
{
    public class MansardRoofGenerator
    {
        public static bool Generate(BuildRMesh mesh, BuildRCollider collider, Vector2[] points, int[] facadeIndices, float roofBaseHeight, IVolume volume)
        {
            if(points.Length == 0) return false;

            if(BuildrUtils.SelfIntersectingPoly(points))
                return false;
            
            Roof design = volume.roof;
            float floorWidth = design.floorDepth;
            float roofDepth = design.depth;
            float roofHeight = design.height;

            Surface mainSurface = design.mainSurface;
            Surface floorSurface = design.floorSurface;
            int mainSubmesh = mesh.submeshLibrary.SubmeshAdd(mainSurface);
            int floorSubmesh = mesh.submeshLibrary.SubmeshAdd(floorSurface);

            //mansard floor
            if (floorWidth > 0)
            {
                OffsetSkeleton offsetFloorPoly = new OffsetSkeleton(points, null, floorWidth);
                offsetFloorPoly.direction = 1;
                offsetFloorPoly.Execute();
                Shape floorShape = offsetFloorPoly.shape;

                if(floorShape != null)
                {
                    ToMesh(ref mesh, ref floorShape, roofBaseHeight, 0, facadeIndices, volume, floorSubmesh, design.floorSurface);

                    points = new Vector2[floorShape.terminatedNodeCount];
                    for (int i = 0; i < floorShape.terminatedNodeCount; i++)
                        points[i] = floorShape.TerminatedNode(i).position;
                }
                else
                {
                    return false;
                    //todo
                }
            }
            
            if (points.Length == 0) return false;

            //mansard pitch
            OffsetSkeleton offsetRoofPoly = new OffsetSkeleton(points, null, roofDepth);
            offsetRoofPoly.direction = 1;
            offsetRoofPoly.Execute();
            Shape roofShape = offsetRoofPoly.shape;
            if (roofShape == null) return false;

            if(facadeIndices.Length < roofShape.baseEdges.Count) return false;
            
            ToMesh(ref mesh, ref roofShape, roofBaseHeight, roofHeight, facadeIndices, volume, mainSubmesh, mainSurface, design.hasDormers);

            points = new Vector2[roofShape.terminatedNodeCount];
            for (int i = 0; i < roofShape.terminatedNodeCount; i++)
                points[i] = roofShape.TerminatedNode(i).position;

            //mansard top
            ToMesh(ref mesh, points, roofBaseHeight + roofHeight, facadeIndices, volume, floorSubmesh, floorSurface);
            
            return true;
        }

        private static void ToMesh(ref BuildRMesh mesh, Vector2[] points, float height, int[] facadeIndices, IVolume volume, int submesh, Surface surface)
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
            int[] topTris = Poly2TriWrapper.Triangulate(points, true);
            mesh.AddData(verts, uvs, topTris, normals, tangents, submesh);
        }

        private static void ToMesh(ref BuildRMesh mesh, ref Shape shape, float roofBaseHeight, float meshHeight, int[] facadeIndices, IVolume volume, int submesh, Surface surface, bool generateDormers = false)
        {
            //TODO fix this error properly
            if(shape == null)
            {
                Debug.Log("ToMesh: Error to fix");
                return;
            }
            List<Edge> edges = new List<Edge>(shape.edges);
            List<Edge> baseEdges = new List<Edge>(shape.baseEdges);

            float shapeHeight = shape.HeighestPoint();
            float heightScale = meshHeight / shapeHeight;
            bool isFloor = meshHeight < 0.00001f;

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
            List<Vector3[]> roofFaces = new List<Vector3[]>();
            for (int b = 0; b < baseEdgeCount; b++)
            {
                int facadeIndex = facadeIndices[b];
                bool isGabled = volume[facadeIndex].isGabled;
                if(!isGabled)
                {
                    int facadeIndexLeft = (facadeIndex - 1 + volume.numberOfFacades) % volume.numberOfFacades;
                    int facadeIndexRight = (facadeIndex + 1) % volume.numberOfFacades;
                    bool isGabledLeft = volume[facadeIndexLeft].isGabled;
                    bool isGabledRight = volume[facadeIndexRight].isGabled;
                    Edge baseEdge = baseEdges[b];
                    Node nodeA = baseEdge.nodeA;
                    Node nodeB = baseEdge.nodeB;

                    Node currentNode = nodeA;
                    Node lastNode = nodeB;
                    int itMax = 50;
                    List<Node> edgeShape = new List<Node>(){nodeA};

                    while(currentNode != nodeB)
                    {
                        List<Node> nodeConnections = shapeConnections[currentNode];
                        int nodeConnectionCount = nodeConnections.Count;
                        float minAngle = Mathf.Infinity;
                        Node nextNode = null;
                        Vector2 currentDirection = (currentNode.position - lastNode.position).normalized;
                        for(int n = 0; n < nodeConnectionCount; n++)
                        {
                            Node connectingNode = nodeConnections[n];
                            if(connectingNode == lastNode) continue;//end this circus!
                            Vector2 nextDirection = (connectingNode.position - currentNode.position).normalized;
                            float nodeAngle = SignAngleDirection(currentDirection, nextDirection);
                            if(nodeAngle < minAngle)
                            {
                                minAngle = nodeAngle;
                                nextNode = connectingNode;
                            }
                        }
                        if(nextNode != null)
                        {
                            edgeShape.Add(nextNode);
                            lastNode = currentNode;
                            currentNode = nextNode;
                        }


                        itMax--;
                        if(itMax < 0) break;
                    }

                    int edgeShapeCount = edgeShape.Count;

                    if(edgeShapeCount == 4 && generateDormers)
                    {
                        Vector3[] edgeShapeV3 = new Vector3[4];
                        edgeShapeV3[0] = new Vector3(edgeShape[0].position.x, roofBaseHeight, edgeShape[0].position.y);
                        edgeShapeV3[1] = new Vector3(edgeShape[3].position.x, roofBaseHeight, edgeShape[3].position.y);
                        edgeShapeV3[2] = new Vector3(edgeShape[1].position.x, roofBaseHeight + meshHeight, edgeShape[1].position.y);
                        edgeShapeV3[3] = new Vector3(edgeShape[2].position.x, roofBaseHeight + meshHeight, edgeShape[2].position.y);
                        roofFaces.Add(edgeShapeV3);
                    }

                    if((isGabledLeft || isGabledRight) && edgeShapeCount == 4)//modify shape if gables are detected
                    {

                        Vector3 p0 = edgeShape[0].position;
                        Vector3 p1 = edgeShape[3].position;
                        Vector3 p2 = edgeShape[1].position;
                        Vector3 vector = p1 - p0;
                        Vector3 dir = vector.normalized;
                        Vector3 cross = Vector3.Cross(Vector3.back, dir);

                        if(isGabledLeft)
                        {
                            float gableThickness = volume[facadeIndexLeft].gableThickness;
                            bool simpleGable = volume[facadeIndexLeft].simpleGable;
                            Gable gableStyle = volume[facadeIndexLeft].gableStyle;
                            if(!simpleGable && gableStyle == null || !isFloor) gableThickness = 0;
                            Vector3 newPointA = Vector3.Project(p2 - p1, cross) + dir * gableThickness;
                            edgeShape[1].position = edgeShape[0].position + new Vector2(newPointA.x, newPointA.y);
                        }
                        if(isGabledRight)
                        {
                            float gableThickness = volume[facadeIndexRight].gableThickness;
                            bool simpleGable = volume[facadeIndexRight].simpleGable;
                            Gable gableStyle = volume[facadeIndexRight].gableStyle;
                            if(!simpleGable && gableStyle == null || !isFloor) gableThickness = 0;
                            Vector3 newPointB = Vector3.Project(p2 - p1, cross) - dir * gableThickness;
                            edgeShape[2].position = edgeShape[3].position + new Vector2(newPointB.x, newPointB.y);
                        }
                    }


                    Vector3[] verts = new Vector3[edgeShapeCount];

                    Vector2[] uvs = new Vector2[edgeShapeCount];
                    Vector3 baseShapeDirection = ToV3(nodeB.position - nodeA.position).normalized;
                    float uvAngle = SignAngle(new Vector2(baseShapeDirection.x, baseShapeDirection.z).normalized) - 90;

                    Vector2[] faceShape = new Vector2[edgeShapeCount];
                    Vector3[] normals = new Vector3[edgeShapeCount];
                    Vector4[] tangents = new Vector4[edgeShapeCount];
                    Vector4 tangent = BuildRMesh.CalculateTangent(baseShapeDirection);
                    for(int i = 0; i < edgeShapeCount; i++)
                    {
                        Vector3 newVert = new Vector3(edgeShape[i].position.x, edgeShape[i].height * heightScale + roofBaseHeight, edgeShape[i].position.y);
                        verts[i] = newVert;

                        Vector2 baseUV = (i == 0) ? Vector2.zero : new Vector2(newVert.x - verts[0].x, newVert.z - verts[0].z);
                        Vector2 newUV = Rotate(baseUV, uvAngle);
                        float faceHeight = edgeShape[i].height * heightScale;
                        newUV.y = Mathf.Sqrt((newUV.y * newUV.y) + (faceHeight * faceHeight));
                        if(surface != null)
                            newUV = surface.CalculateUV(newUV);
                        uvs[i] = newUV;

                        faceShape[i] = edgeShape[i].position;//used for triangulation
                        //                    normals[i] = normal;
                        tangents[i] = tangent;
                    }
//                    int[] tris = EarClipper.Triangulate(faceShape, 0, -1);
                    int[] tris = Poly2TriWrapper.Triangulate(faceShape, true);
                    int triCount = tris.Length;
                    if(triCount < 3) continue;

                    Vector3 normal = BuildRMesh.CalculateNormal(verts[tris[0]], verts[tris[1]], verts[tris[2]]);
                    for(int i = 0; i < edgeShapeCount; i++)
                        normals[i] = normal;//normCal[i].normalized;

                    mesh.AddData(verts, uvs, tris, normals, tangents, submesh);

                    if(isGabled)
                    {
                        for(int t = 0; t < triCount; t += 3)
                        {
                            if(tris[t] == 0 || tris[t + 1] == 0 || tris[t + 2] == 0)
                            {
                                int beB = edgeShapeCount - 1;
                                if(tris[t] == beB || tris[t + 1] == beB || tris[t + 2] == beB)
                                {

                                    Vector3 b0 = verts[0];
                                    Vector3 b1 = verts[beB];
                                    int topIndex = 0;
                                    for(int tx = 0; tx < 3; tx++)
                                        if(tris[t + tx] != 0 && tris[t + tx] != beB) topIndex = tris[t + tx];
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
                }
                else if(isFloor)
                {
                    Roof roof = volume.roof;
                    Edge baseEdge = baseEdges[b];
                    Node nodeA = baseEdge.nodeA;
                    Node nodeB = baseEdge.nodeB;

                    Vector3 p0 = new Vector3(nodeA.position.x, heightScale + roofBaseHeight, nodeA.position.y);
                    Vector3 p1 = new Vector3(nodeB.position.x, heightScale + roofBaseHeight, nodeB.position.y);
                    
                    Vector3 baseV = p1 - p0;
                    Vector3 dir = baseV.normalized;
                    Vector3 face = Vector3.Cross(Vector3.up, dir).normalized;

                    Vector3 parapetEdgeModifier = dir * (roof.overhang - (roof.parapetFrontDepth + roof.parapetBackDepth)) * 1.05f;
                    p0 += parapetEdgeModifier;
                    p1 += -parapetEdgeModifier;
//                    p0 += face * (roof.parapetFrontDepth + roof.parapetBackDepth + roof.overhang);

                    VolumePoint volumePoint = volume[facadeIndices[b]];
                    bool simpleGable = volumePoint.simpleGable;
                    Gable gableStyle = volume[facadeIndices[b]].gableStyle;
                    if(!simpleGable && gableStyle == null) simpleGable = true;
                    float thickness = volume[facadeIndices[b]].gableThickness;
                    float additionalHeight = volume[facadeIndices[b]].gableHeight;
                    float height = roof.height + additionalHeight;

                    if(simpleGable)//generate a simple gable
                    {
                        int wallSubmesh = mesh.submeshLibrary.SubmeshAdd(roof.wallSurface);//surfaceMapping.IndexOf(roof.wallSurface);
                        if (wallSubmesh == -1) wallSubmesh = submesh;

                        Vector3 g0 = p0;
                        Vector3 g1 = p0 + Vector3.up * additionalHeight;
                        Vector3 g2 = g1 + dir * roof.floorDepth * 0.5f;
                        Vector3 g3 = g2 + dir * roof.depth * 0.5f + Vector3.up * roof.height;

                        Vector3 g7 = p1;
                        Vector3 g6 = p1 + Vector3.up * additionalHeight;
                        Vector3 g5 = g6 - dir * roof.floorDepth * 0.5f;
                        Vector3 g4 = g5 - dir * roof.depth * 0.5f + Vector3.up * roof.height;

                        Vector3 gF = -face * thickness;

                        mesh.AddPlane(g0, g7, g1, g6, wallSubmesh);//bottom front
                        mesh.AddPlane(g7 + gF, g0 + gF, g6 + gF, g1 + gF, wallSubmesh);//bottom back
                        mesh.AddPlane(g1, g6, g1 + gF, g6 + gF, wallSubmesh);//bottom top
                        mesh.AddPlane(g0, g1, g0 + gF, g1 + gF, wallSubmesh);//bottom sides
                        mesh.AddPlane(g6, g7, g6 + gF, g7 + gF, wallSubmesh);


                        mesh.AddPlane(g2, g5, g3, g4, wallSubmesh);//top front
                        mesh.AddPlane(g5 + gF, g2 + gF, g4 + gF, g3 + gF, wallSubmesh);//top back
                        mesh.AddPlane(g2 + gF, g2, g3 + gF, g3, wallSubmesh);//top sides
                        mesh.AddPlane(g5, g5 + gF, g4, g4 + gF, wallSubmesh);//top sides

                        mesh.AddPlane(g3 + gF, g3, g4 + gF, g4, wallSubmesh);//top top
                    }
                    else
                    {
                        Vector2 baseUV = new Vector2(0, volume.planHeight);
                        GableGenerator.Generate(ref mesh, gableStyle, p0, p1, height, thickness, baseUV);
                    }
                }
            }

            if(generateDormers)
                DormerGenerator.Generate(ref mesh, volume, roofFaces);
        }

        public static Vector3 ToV3(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
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
                angle = -angle;
            return angle;
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
    }
}