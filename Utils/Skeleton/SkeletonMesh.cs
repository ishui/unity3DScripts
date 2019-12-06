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
    public class SkeletonMesh
    {
        private List<SkeletonTri> _tris = new List<SkeletonTri>();
        private List<SkeletonFace> _faces = new List<SkeletonFace>();
        private List<Vector2> _topShape = new List<Vector2>();
        private Dictionary<Node, List<SkeletonFace>> _edgeDic = new Dictionary<Node, List<SkeletonFace>>();
        private Dictionary<Edge, SkeletonFace> _nodeDic = new Dictionary<Edge, SkeletonFace>();
        private float _height = 1;
        private int _triIndex = 1;

        public float height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    foreach (SkeletonTri tr in _tris)
                        tr.height = value;
                }
            }
        }

        public void Build(SkeletonData data)
        {
            int liveEdgeCount = data.liveEdges.Count;
            for (int e = 0; e < liveEdgeCount; e++)
            {
                Edge liveEdge = data.liveEdges[e];
                Edge formingA = data.formingEdges[liveEdge.nodeA];
                Edge formingB = data.formingEdges[liveEdge.nodeB];
                Edge staticEdge = null;
                foreach (Edge edge in data.edges)
                {
                    if (edge.Contains(formingA.nodeA) && edge.Contains(formingB.nodeA))
                    {
                        staticEdge = edge;
                        break;
                    }
                }
                if (formingA == null || formingB == null || staticEdge == null)
                    continue;

                AddFace(liveEdge, staticEdge.nodeA, staticEdge.nodeB);
            }

            BuildTop(data);
        }

        public void BuildTop(SkeletonData data)
        {
            _topShape.Clear();
            int terminatedNodeCount = data.terminatedNodeCount;
            if (terminatedNodeCount == 0) return;

            List<Edge> edges = data.liveEdges;

            while (edges.Count > 0)
            {
                List<Node> topNodes = new List<Node>();
                topNodes.Add(edges[0].nodeA);
                topNodes.Add(edges[0].nodeB);

                Node startNode = edges[0].nodeA;
                Node nextNode = edges[0].nodeB;

                bool completeShape = false;
                while (startNode != nextNode)
                {
                    Edge nextEdge = null;
                    foreach (Edge edge in edges)
                    {
                        if (edge.nodeA == nextNode)
                        {
                            nextEdge = edge;
                            break;
                        }
                    }
                    if(nextEdge == null)
                        break;
                    nextNode = nextEdge.nodeB;

                    if(nextNode == startNode)
                        completeShape = true;
                    else
                        topNodes.Add(nextEdge.nodeB);
                }

                if(completeShape)
                    foreach(Node topNode in topNodes)
                        _topShape.Add(topNode.position);
            }
        }

        public void Clean(SkeletonData data)
        {
            int triCount = _tris.Count;
            for (int t = 0; t < triCount; t++)
            {
                SkeletonTri tri = _tris[t];
                bool remove = false;
                if (!data.isStaticNode(tri[0])) remove = true;
                if (!data.isStaticNode(tri[1])) remove = true;
                if (!data.isStaticNode(tri[2])) remove = true;
                if (remove)
                {
                    _tris.RemoveAt(t);
                    triCount--;
                    t--;
                }
            }
        }

        private void AddFace(Edge liveEdge, Node staticNodeA, Node staticNodeB)
        {
            OffsetShapeLog.DrawEdge(liveEdge, Color.red);
            OffsetShapeLog.DrawLabel(liveEdge.nodeA.position, "new face");
            SkeletonFace face = new SkeletonFace(liveEdge, staticNodeA, staticNodeB);
            _faces.Add(face);

            if (!_edgeDic.ContainsKey(liveEdge.nodeA))
            {
                _edgeDic.Add(liveEdge.nodeA, new List<SkeletonFace>());
                _edgeDic[liveEdge.nodeA].Add(face);
            }
            else
                _edgeDic[liveEdge.nodeA].Add(face);

            if (!_edgeDic.ContainsKey(liveEdge.nodeB))
            {
                _edgeDic.Add(liveEdge.nodeB, new List<SkeletonFace>());
                _edgeDic[liveEdge.nodeB].Add(face);
            }
            else
                _edgeDic[liveEdge.nodeB].Add(face);

            if (!_nodeDic.ContainsKey(liveEdge))
                _nodeDic.Add(liveEdge, face);
            else
                _nodeDic[liveEdge] = face;
        }

        private void RemoveFace(SkeletonFace face)
        {
            _faces.Remove(face);
            Edge liveEdge = face.liveEdge;
            Node nodeA = liveEdge.nodeA;
            Node nodeB = liveEdge.nodeB;
            if (_edgeDic.ContainsKey(nodeA))
            {
                _edgeDic[nodeA].Remove(face);
                if (_edgeDic[nodeA].Count == 0)
                    _edgeDic.Remove(nodeA);
            }
            if (_edgeDic.ContainsKey(nodeB))
            {
                _edgeDic[nodeB].Remove(face);
                if (_edgeDic[nodeB].Count == 0)
                    _edgeDic.Remove(nodeB);
            }

            if (_nodeDic.ContainsKey(liveEdge))
                _nodeDic.Remove(liveEdge);
        }

        public void AddTriangle(Node a, Node b, Node c, Vector3 tangent)
        {
            SkeletonTri newTriangle = new SkeletonTri(a, b, c, tangent);
            AddTriangle(newTriangle);
        }

        public void AddTriangle(SkeletonTri newTriangle)
        {
            OffsetShapeLog.AddLine("Add Triangle: " + newTriangle[0].id + " " + newTriangle[1].id + " " + newTriangle[2].id);
            newTriangle.id = _triIndex;
            _triIndex++;
            _tris.Add(newTriangle);
        }

        public void AddTriangles(SkeletonTri[] newTriangles)
        {
            foreach (SkeletonTri tri in newTriangles)
                AddTriangle(tri);
        }

        public SkeletonTri[] GetTriangles()
        {
            return _tris.ToArray();
        }

        public List<Vector2> topShape()
        {
            return _topShape;
        }

        public void ReplaceNode(Node oldNode, Node newNode)
        {
            foreach (SkeletonFace face in _faces)
                face.ReplaceNode(oldNode, newNode);
            foreach (SkeletonTri tri in _tris)
                tri.ReplaceNode(oldNode, newNode);
        }

        public void CollapseEdge(Edge liveEdge, Node newLiveNode, Node newStaticNode)
        {
            Edge edgeA = null, edgeB = null;

            if (!_edgeDic.ContainsKey(newLiveNode))
                _edgeDic.Add(newLiveNode, new List<SkeletonFace>());
            foreach (SkeletonFace face in _faces)
            {
                Edge liveFaceEdge = face.liveEdge;

                if (liveEdge == liveFaceEdge) continue;

                if (liveFaceEdge.Contains(liveEdge.nodeA))
                {
                    edgeA = liveFaceEdge;
                    _edgeDic[newLiveNode].Add(face);
                }
                if (liveFaceEdge.Contains(liveEdge.nodeB))
                {
                    edgeB = liveFaceEdge;
                    _edgeDic[newLiveNode].Add(face);
                }
            }

            if (edgeA != null)
            {
                SkeletonFace faceA = _nodeDic[edgeA];
                Node n0 = faceA[0];
                Node n1 = faceA[1];
                Node n2 = newStaticNode;
                if (n0 != n2 && n1 != n2)
                {
                    SkeletonTri newtriangle = new SkeletonTri(n0, n1, n2, faceA.tangent);
                    AddTriangle(newtriangle);//remove static triangle from face - add to triangle stack
                    faceA.ReplaceNode(faceA[1], newStaticNode);
                    faceA.ReplaceNode(faceA[3], newLiveNode);
                }
            }
            if (edgeB != null)
            {
                SkeletonFace faceB = _nodeDic[edgeB];
                Node n0 = faceB[0];
                Node n1 = faceB[1];
                Node n2 = newStaticNode;
                if (n0 != n2 && n1 != n2)
                {
                    SkeletonTri newtriangle = new SkeletonTri(n0, n1, n2, faceB.tangent);
                    AddTriangle(newtriangle);
                    faceB.ReplaceNode(faceB[0], newStaticNode);
                    faceB.ReplaceNode(faceB[2], newLiveNode);
                }
            }

            if (_nodeDic.ContainsKey(liveEdge))
            {
                SkeletonFace liveFace = _nodeDic[liveEdge];
                Node lfn0 = liveFace[0];
                Node lfn1 = liveFace[1];
                Node lfn2 = newStaticNode;
                if (lfn0 != lfn2 && lfn1 != lfn2)
                {
                    SkeletonTri newFaceTriangle = new SkeletonTri(lfn0, lfn1, lfn2, liveFace.tangent);
                    AddTriangle(newFaceTriangle);
                }
                _nodeDic.Remove(liveEdge);
                RemoveFace(liveFace);
            }
        }

        public void SplitEdge(SplitEvent e)
        {
            Edge oldLiveEdge = e.edge;
            Vector3 splitTangent = Utils.ToV3(oldLiveEdge.direction);
            SkeletonFace oldFace = _nodeDic[oldLiveEdge];
            Node staticNode = e.newStaticNode;

            Edge newLiveEdgeA = e.newLiveEdgeA;
            Edge newLiveEdgeB = e.newLiveEdgeB;

            Node oldNode = e.node;
            Node newLiveNodeA = e.newLiveNodeA;
            Node newLiveNodeB = e.newLiveNodeB;

            //centre tri
            SkeletonTri newTriangle = new SkeletonTri(oldFace[0], oldFace[1], staticNode, splitTangent);
            AddTriangle(newTriangle);

            if (!_nodeDic.ContainsKey(e.nodeEdgeA))
                return;
            SkeletonFace faceA = _nodeDic[e.nodeEdgeA];
            faceA.ReplaceNode(oldNode, newLiveNodeA);
            //left tri
            SkeletonTri newTriangleA = new SkeletonTri(staticNode, faceA[0], newLiveNodeA, splitTangent);
            AddTriangle(newTriangleA);

            if (!_nodeDic.ContainsKey(e.nodeEdgeB))
                return;
            SkeletonFace faceB = _nodeDic[e.nodeEdgeB];
            faceB.ReplaceNode(oldNode, newLiveNodeB);
            //right tri
            SkeletonTri newTriangleB = new SkeletonTri(faceB[1], staticNode, newLiveNodeB, splitTangent);
            AddTriangle(newTriangleB);

            RemoveFace(oldFace);
            AddFace(newLiveEdgeA, oldFace[0], staticNode);
            AddFace(newLiveEdgeB, staticNode, oldFace[1]);

            int oldNodeReferenceCount = 0;
            if (_edgeDic.ContainsKey(oldNode))
                oldNodeReferenceCount = _edgeDic[oldNode].Count;
            for (int r = 0; r < oldNodeReferenceCount; r++)
            {
                SkeletonFace face = _edgeDic[oldNode][r];
                Edge liveEdge = face.liveEdge;

                if (liveEdge.Contains(newLiveNodeA))
                {
                    if (!_edgeDic.ContainsKey(newLiveNodeA))
                        _edgeDic.Add(newLiveNodeA, new List<SkeletonFace>());
                    _edgeDic[newLiveNodeA].Add(face);
                }
                if (liveEdge.Contains(newLiveNodeB))
                {
                    if (!_edgeDic.ContainsKey(newLiveNodeB))
                        _edgeDic.Add(newLiveNodeB, new List<SkeletonFace>());
                    _edgeDic[newLiveNodeB].Add(face);
                }
            }
        }

        public void EdgeComplete(Edge liveEdge)
        {
            if (!_nodeDic.ContainsKey(liveEdge))
            {
                Debug.Log("EdgeComplete error");
                return;
            }
            SkeletonFace face = _nodeDic[liveEdge];
            SkeletonTri[] tris = face.Convert();
            AddTriangles(tris);
            RemoveFace(face);
        }

        public void Clear()
        {
            _tris.Clear();
            _faces.Clear();
            _edgeDic.Clear();
            _nodeDic.Clear();
            //            _uvEdges.Clear();
        }

        public void DrawDebug()
        {
            foreach (SkeletonFace face in _faces)
                face.DrawDebug(new Color(1, 1, 0, 0.7f));
            foreach (SkeletonTri tri in _tris)
                tri.DrawDebug(new Color(0, 1, 0.3f, 0.1f));
        }
    }

    public class SkeletonTri
    {
        public int id = 0;
        private Node[] _nodes;
        public Vector3[] positions;
        public Vector2[] uvs;
        public Vector3 normal;
        public Vector4 tangent;
        private Vector3 _tangentV3;
        private Vector3 _centre;
        private float _unitHeight = 1;

        public Node this[int index]
        {
            get { return _nodes[index]; }
        }

        //        public Vector3[] positions { get { return _positions; } }
        //        public Vector2[] uvs { get {return _uvs;} }
        //        public Vector3 normal { get { return _normal; } }
        //        public Vector4 tangent { get { return _tangent; } }
        public Vector3 tangentV3 { get { return _tangentV3; } }
        public Vector3 centre { get { return _centre; } }

        public SkeletonTri(Node a, Node b, Node c, Vector3 tangent)
        {
            _nodes = new Node[3];
            _nodes[0] = a;
            _nodes[1] = b;
            _nodes[2] = c;
            positions = new Vector3[3];
            uvs = new Vector2[3];
            _tangentV3 = tangent;
            this.tangent = Utils.CalculateTangent(_tangentV3);
            Recalculate();
        }

        private void Recalculate()
        {
            positions[0] = new Vector3(_nodes[0].position.x, _nodes[0].height * _unitHeight, _nodes[0].position.y);
            positions[1] = new Vector3(_nodes[1].position.x, _nodes[1].height * _unitHeight, _nodes[1].position.y);
            positions[2] = new Vector3(_nodes[2].position.x, _nodes[2].height * _unitHeight, _nodes[2].position.y);
            normal = Utils.CalculateNormal(positions[0], positions[2], positions[1]);
            _centre = (positions[0] + positions[1] + positions[2]) / 3;
            //            CalculateUVs();
        }

        public bool hasStartNode
        {
            get
            {
                if (_nodes[0].startNode) return true;
                if (_nodes[1].startNode) return true;
                if (_nodes[2].startNode) return true;
                return false;
            }
        }

        public bool hasStartEdge
        {
            get
            {
                int startNodes = 0;
                if (_nodes[0].startNode) startNodes++;
                if (_nodes[1].startNode) startNodes++;
                if (_nodes[2].startNode) startNodes++;
                return startNodes > 1;
            }
        }

        public int IndexOf(Node node)
        {
            if (node == _nodes[0]) return 0;
            if (node == _nodes[1]) return 1;
            if (node == _nodes[2]) return 2;
            return -1;
        }

        //        private void CalculateUVs()
        //        {
        //            
        //            Vector2 baseUV = _nodes[0].maxUV;
        //            Vector3 vA = _positions[1] - _positions[0];
        //            Vector3 vB = _positions[2] - _positions[0];
        //
        //            Vector3 right = _tangentV3;
        //            Vector3 up = Vector3.Cross(right, _normal);
        //            Vector3 upVA = Vector3.Project(vA, up);
        //            Vector3 rightVA = Vector3.Project(vA, right);
        //            Vector3 upVB = Vector3.Project(vB, up);
        //            Vector3 rightVB = Vector3.Project(vB, right);
        //
        //            float apexUVAX = rightVA.magnitude * Mathf.Sign(Vector3.Dot(right, rightVA));
        //            float apexUVAY = upVA.magnitude * Mathf.Sign(Vector3.Dot(up, upVA));
        //            Vector2 apexUVA = baseUV + new Vector2(apexUVAX, apexUVAY);
        //            float apexUVBX = rightVB.magnitude * Mathf.Sign(Vector3.Dot(right, rightVB));
        //            float apexUVBY = upVB.magnitude * Mathf.Sign(Vector3.Dot(up, upVB));
        //            Vector2 apexUVB = baseUV + new Vector2(apexUVBX, apexUVBY);
        //            _uvs = new[] { baseUV, apexUVA, apexUVB };
        //            _nodes[1].maxUV = apexUVA;
        //            _nodes[2].maxUV = apexUVB;
        //        }

        public float height
        {
            set
            {
                _unitHeight = value;
                Recalculate();
            }
        }

        public void ReplaceNode(Node oldNode, Node newNode)
        {
            if (_nodes[0] == oldNode) _nodes[0] = newNode;
            if (_nodes[1] == oldNode) _nodes[1] = newNode;
            if (_nodes[2] == oldNode) _nodes[2] = newNode;
            Recalculate();
        }

        public void DrawDebug(Color col)
        {
            OffsetShapeLog.DrawLine(_nodes[0].position, _nodes[1].position, col);
            OffsetShapeLog.DrawLine(_nodes[1].position, _nodes[2].position, col);
            OffsetShapeLog.DrawLine(_nodes[2].position, _nodes[0].position, col);
            Vector3 center = Utils.ToV3((_nodes[0].position + _nodes[1].position + _nodes[2].position) / 3f);
            OffsetShapeLog.DrawDirection(center, _tangentV3, "tangent", col);
            OffsetShapeLog.DrawLabel(_centre, "\nTriangle ID " + id);
            //            Vector3 p0 = JMath.ToV3(_nodes[0].position);
            //            Vector3 p1 = JMath.ToV3(_nodes[1].position);
            //            Vector3 p2 = JMath.ToV3(_nodes[2].position);
            //
            //            Debug.DrawLine(p0, p1, col);
            //            Debug.DrawLine(p1, p2, col);
            //            Debug.DrawLine(p2, p0, col);
            ////
            //
            //            Color a = new Color(col.r, col.g, col.b, 0.25f);
            //            Vector3 v0 = p0 + Vector3.up * _nodes[0].height * 20;
            //            Vector3 v1 = p1 + Vector3.up * _nodes[1].height * 20;
            //            Vector3 v2 = p2 + Vector3.up * _nodes[2].height * 20;
            //
            //            Debug.DrawLine(positions[0], positions[1], col);
            //            Debug.DrawLine(positions[1], positions[2], col);
            //            Debug.DrawLine(positions[2], positions[0], col);
            //            GizmoLabel.Label("Node" + _nodes[0].id, v0);
            //            GizmoLabel.Label("Node" + _nodes[1].id, v1);
            //            GizmoLabel.Label("Node" + _nodes[2].id, v2);
        }
    }

    public class SkeletonFace
    {
        private List<Node> _nodes;
        public int size { get { return _nodes.Count; } }
        private Edge _liveEdge;
        private Vector3 _tangent;

        public Edge liveEdge { get { return _liveEdge; } }
        public Vector3 tangent { get { return _tangent; } }

        public Node this[int index]
        {
            get { return _nodes[index]; }
        }

        public bool Contains(Node node)
        {
            return _nodes.Contains(node);
        }

        public void ReplaceNode(Node oldNode, Node newNode)
        {
            if (!_nodes.Contains(oldNode)) return;

            int index = _nodes.IndexOf(oldNode);
            _nodes[index] = newNode;
        }

        public SkeletonFace(Edge liveEdge, Node staticNodeA, Node staticNodeB)
        {
            _liveEdge = liveEdge;
            _tangent = Utils.ToV3(liveEdge.direction);
            _nodes = new List<Node>();
            _nodes.Add(staticNodeA);
            _nodes.Add(staticNodeB);
            _nodes.Add(liveEdge.nodeA);
            _nodes.Add(liveEdge.nodeB);
        }

        public SkeletonTri RetireNode(Node node)
        {
            if (!_nodes.Contains(node))
                return null;

            SkeletonTri output = new SkeletonTri(_nodes[0], _nodes[1], node, _tangent);
            if (_nodes[2] == node)
            {
                _nodes.Remove(_nodes[0]);
                return output;
            }
            if (_nodes[3] == node)
            {
                _nodes.Remove(_nodes[1]);
                return output;
            }
            return null;
        }

        public SkeletonTri[] Convert()
        {
            SkeletonTri[] output;
            if (size == 3)
            {
                output = new[] { new SkeletonTri(_nodes[0], _nodes[1], _nodes[2], _tangent) };
            }
            else
            {
                output = new SkeletonTri[2];
                output[0] = new SkeletonTri(_nodes[0], _nodes[1], _nodes[2], _tangent);
                output[1] = new SkeletonTri(_nodes[2], _nodes[3], _nodes[1], _tangent);
            }
            return output;
        }

        public SkeletonTri ConvertToTri()
        {
            return new SkeletonTri(_nodes[0], _nodes[1], _nodes[2], _tangent);
        }

        public string toString()
        {
            return string.Format(_nodes[0].id + " , " + _nodes[1].id + " , " + _nodes[2].id + " , " + _nodes[3].id);
        }

        public void DrawDebug(Color col)
        {
            Vector2 center = Vector2.zero;
            for (int i = 0; i < size; i++)
            {
                int ib = (i + 1) % size;
                OffsetShapeLog.DrawLine(_nodes[i].position, _nodes[ib].position, col);
                center += _nodes[i].position;
            }
            center /= size;

            OffsetShapeLog.DrawEdge(_liveEdge, Color.magenta);
            OffsetShapeLog.DrawDirection(Utils.ToV3(center), _tangent, "tangent", col);
        }
    }
}