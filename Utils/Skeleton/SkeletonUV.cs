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

namespace BuildR2.ShapeOffset
{
    public class SkeletonUV
    {
        private System.Collections.Generic.List<UVNode> _uvnodes = new System.Collections.Generic.List<UVNode>();

        public void Build(SkeletonData data)
        {
            System.Collections.Generic.List<SkeletonTri> allTris = new System.Collections.Generic.List<SkeletonTri>(data.mesh.GetTriangles());
            System.Collections.Generic.List<SkeletonTri> startTris = new System.Collections.Generic.List<SkeletonTri>();
            int triCount = allTris.Count;

            for(int t = 0; t < triCount; t++)
            {
                SkeletonTri tri = allTris[t];
                if(tri.hasStartEdge)
                {
                    allTris.RemoveAt(t);
                    triCount--;
                    t--;
                    startTris.Add(tri);
                }
            }

            while(startTris.Count > 0)
            {
                SkeletonTri tri = startTris[0];
                startTris.RemoveAt(0);
                CalculateUVs(tri);
            }

            while(allTris.Count > 0)
            {
                SkeletonTri tri = allTris[0];
                allTris.RemoveAt(0);
                CalculateUVs(tri);
            }

            foreach(UVNode uvnode in _uvnodes)
            {
                OffsetShapeLog.DrawLabel(uvnode.node.position, "\n\n" + uvnode.uv);
            }
        }

        private void CalculateUVs(SkeletonTri tri)
        {
            int baseIndex = FindBaseIndex(tri);
            OffsetShapeLog.DrawLine(tri.centre, tri.positions[baseIndex], Color.red);
            Vector2 baseUV = Vector2.zero;
            OffsetShapeLog.AddLine("============");
            OffsetShapeLog.AddLine("triangle uv node find! ",tri.id," base node id ",tri[baseIndex].id," tangent ",tri.tangentV3);
            OffsetShapeLog.AddLine("============");
            foreach (UVNode uvnode in _uvnodes)
            {
                OffsetShapeLog.AddLine("uvnode! " , uvnode.node.id , uvnode.tangent);
                if (uvnode.node != tri[baseIndex]) continue;
                if(!uvnode.TangentCheck(tri.tangentV3)) continue;
                baseUV = uvnode.uv;
            }
            OffsetShapeLog.AddLine("!!!!!!!!!!!!");
            OffsetShapeLog.DrawLabel(tri.centre, baseUV.ToString());

            int indexB = (baseIndex + 1) % 3;
            int indexC = (baseIndex + 2) % 3;

            Vector3 p0 = tri.positions[baseIndex];
            Vector3 p1 = tri.positions[indexB];
            Vector3 p2 = tri.positions[indexC];

            Vector3 vA = p1 - p0;
            Vector3 vB = p2 - p0;

            Vector3 right = tri.tangentV3;
            Vector3 up = Vector3.Cross(right, tri.normal);
            Vector3 upVA = Vector3.Project(vA, up);
            Vector3 rightVA = Vector3.Project(vA, right);
            Vector3 upVB = Vector3.Project(vB, up);
            Vector3 rightVB = Vector3.Project(vB, right);
            
            float apexUVAX = rightVA.magnitude * Mathf.Sign(Vector3.Dot(right, rightVA));
            float apexUVAY = upVA.magnitude * Mathf.Sign(Vector3.Dot(up, upVA));
            Vector2 apexUVA = baseUV + new Vector2(apexUVAX, apexUVAY);
            float apexUVBX = rightVB.magnitude * Mathf.Sign(Vector3.Dot(right, rightVB));
            float apexUVBY = upVB.magnitude * Mathf.Sign(Vector3.Dot(up, upVB));
            Vector2 apexUVB = baseUV + new Vector2(apexUVBX, apexUVBY);

            _uvnodes.Add(new UVNode(tri[indexB], apexUVA, right));
            _uvnodes.Add(new UVNode(tri[indexC], apexUVB, right));

            Vector2[] uvs = new Vector2[3];
            uvs[baseIndex] = baseUV;
            uvs[indexB] = apexUVA;
            uvs[indexC] = apexUVB;
            tri.uvs = uvs;
        }

        private int FindBaseIndex(SkeletonTri tri)
        {
            System.Collections.Generic.List<int> startNodeIndices = new System.Collections.Generic.List<int>();
            for(int i = 0; i < 3; i++)
                if(tri[i].startNode) startNodeIndices.Add(i);
            int startNodeCount = startNodeIndices.Count;

            if (startNodeCount == 1) return startNodeIndices[0];

            if(startNodeCount > 1)
            {
                float lowestDot = 1;
                int output = 0;
                for(int i = 0; i < startNodeCount; i++)
                {
                    Vector3 dir = (tri.positions[startNodeIndices[i]] - tri.centre).normalized;
                    float dot = Vector3.Dot(dir, tri.tangentV3);
                    if(dot < lowestDot)
                    {
                        lowestDot = dot;
                        output = startNodeIndices[i];
                    }
                }
                return output;
            }

            float lowestY = Mathf.Infinity;
            int lowestIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                if (tri.positions[i].y < lowestY)
                {
                    lowestY = tri.positions[i].y;
                    lowestIndex = i;
                }
            }
            return lowestIndex;
        }
    }

    public class UVNode
    {
        public Node node;
        public Vector2 uv;
        public Vector3 tangent;

        public UVNode(Node node, Vector2 uv, Vector3 tangent)
        {
            this.node = node;
            this.uv = uv;
            this.tangent = tangent;
        }

        public bool TangentCheck(Vector3 tangent)
        {
            return Vector3.Dot(this.tangent, tangent) > 0.99f;
        }
    }
}