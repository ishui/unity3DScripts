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
using System.Text;

namespace BuildR2.ShapeOffset
{
    public class OffsetShapeLog
    {
        public static bool enabled = false;
        public static bool liveLog = false;
        private static StringBuilder sb = new StringBuilder();
        private static StringBuilder tempsb = new StringBuilder();

        private static void AddString(string st)
        {
            if (!enabled) return;
            sb.Append(st);
        }

        private static void AddStringLine(string st)
        {
            if (!enabled) return;
            sb.AppendLine(st);
        }

        public static void Add(string st)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            AddString(st);
            if (liveLog)
                Debug.Log(st);
#endif
        }

        public static void Add(object st)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            Add(st.ToString());
#endif
        }

        public static void Add(params object[] st)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            int count = st.Length;
            tempsb.Remove(0, tempsb.Length);
            for (int s = 0; s < count; s++)
            {
                AddString(" ");
                string item = st[s].ToString();
                AddString(item);
                if (liveLog)
                {
                    tempsb.Append(" ");
                    tempsb.Append(item);
                }
            }
            if (liveLog)
                Debug.Log(tempsb.ToString());
#endif
        }

        public static void AddLine(string st)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            sb.AppendLine(st);
            if (liveLog)
                Debug.Log(st);
#endif
        }

        public static void AddLine(object st)
        {
#if UNITY_EDITOR
            AddLine(st.ToString());
#endif
        }

        public static void AddLine(params object[] st)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            tempsb.Remove(0, tempsb.Length);
            int count = st.Length;
            for (int s = 0; s < count; s++)
            {
                AddString(" ");
                string item = st[s].ToString();
                AddString(item);
                if (liveLog)
                {
                    tempsb.Append(" ");
                    tempsb.Append(item);
                }
            }
            if (liveLog)
                Debug.Log(tempsb.ToString());
#endif
        }

        public static void Output()
        {
#if UNITY_EDITOR
            if (!enabled) return;
            Debug.Log(sb.ToString());
            Clear();
#endif
        }

        public static void Clear()
        {
#if UNITY_EDITOR
            sb.Remove(0, sb.Length);
#endif
        }

        public static void DrawLine(Vector3 v0, Vector3 v1, Color col)
        {
#if UNITY_EDITOR
            if(!enabled && !liveLog) return;
            Debug.DrawLine(v0, v1, col);
#endif
        }

        public static void DrawLine(Vector2 v0, Vector2 v1, Color col)
        {
#if UNITY_EDITOR
            if (!enabled && !liveLog) return;
            Debug.DrawLine(Utils.ToV3(v0), Utils.ToV3(v1), col);
#endif
        }

        public static void DrawNode(Node node, Color col)
        {
#if UNITY_EDITOR
            //            if (!enabled) return;
            Debug.DrawLine(Utils.ToV3(node.position), Utils.ToV3(node.position) + Vector3.up, col);
#endif
        }

        public static void DrawEdge(Edge edge, Color col)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            Debug.DrawLine(Utils.ToV3(edge.nodeA.position), Utils.ToV3(edge.nodeB.position), col);
#endif
        }

        public static void DrawLabel(Vector3 p, string text)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            GizmoLabel.Label(text, p);
#endif
        }

        public static void DrawLabel(Vector2 p, string text)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            GizmoLabel.Label(text, Utils.ToV3(p));
#endif
        }

        public static void LabelNode(Node node)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            GizmoLabel.Label("Node " + node.id, Utils.ToV3(node.position));
#endif
        }

        public static void DrawDirection(Vector3 p, Vector3 d, string text, Color col)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            GizmoLabel.LabelDirection(text, p, d, col, 1);
#endif
        }

        public static void DrawDirection(Vector2 p, Vector2 d, string text, Color col)
        {
#if UNITY_EDITOR
            if (!enabled) return;
            GizmoLabel.LabelDirection(text, Utils.ToV3(p), Utils.ToV3(d), col, 1);
#endif
        }

        public static void DrawShape(Shape shape)
        {
#if UNITY_EDITOR
            if (!enabled) return;

            int baseShapeSize = shape.baseShape.Length;
            for (int i = 0; i < baseShapeSize; i++)
                DrawLine(shape.baseShape[i], shape.baseShape[(i + 1) % baseShapeSize], new Color(1, 1, 1, 0.3f));

            int liveEdgeCount = shape.liveEdges.Count;
            for (int i = 0; i < liveEdgeCount; i++)
                DrawEdge(shape.liveEdges[i], Color.cyan);

            int edgeCount = shape.edges.Count;
            for (int i = 0; i < edgeCount; i++)
                DrawEdge(shape.edges[i], Color.green);

            for (int i = 0; i < shape.staticNodeCount; i++)
                DrawNode(shape.StaticNode(i), Color.blue);

            for (int i = 0; i < shape.liveNodeCount; i++)
            {
                DrawNode(shape.LiveNode(i), Color.red);
                if (shape.LiveNode(i).earlyTemination)
                    DrawNode(shape.LiveNode(i), Color.yellow);
            }
#endif
        }

        public static void DrawShapeNodes(Shape shape)
        {
#if UNITY_EDITOR
            if (shape == null)
                return;
            int baseShapeSize = shape.baseShape.Length;
            for (int i = 0; i < baseShapeSize; i++)
                Debug.DrawLine(Utils.ToV3(shape.baseShape[i]), Utils.ToV3(shape.baseShape[(i + 1) % baseShapeSize]), new Color(1, 1, 1, 0.5f));//base shape
            //            for (int i = 0; i < shape.liveNodeCount; i++)
            //                Debug.DrawLine(Utils.ToV3(shape.liveNodes[i].position), Utils.ToV3(shape.liveNodes[(i + 1) % shape.liveNodeCount].position), new Color(0, 1, 0, 0.3f));

            //            if(shape.terminatedNodeCount == shape.staticNodeCount)
            //            Debug.Log(shape.staticNodeCount);

            for (int i = 0; i < shape.liveNodeCount; i++)
            {
                Node nodeA = shape.LiveNode(i);
                //                Node nodeB = shape.LiveNode((i + 1) % shape.liveNodeCount);
                //                if (nodeA.earlyTemination || nodeB.earlyTemination)
                //                    Debug.DrawLine(Utils.ToV3(nodeA.position), Utils.ToV3(nodeB.position), new Color(1, 1, 0, 0.8f));
                if(shape.formingEdges.ContainsKey(nodeA))
                    Debug.DrawLine(Utils.ToV3(shape.formingEdges[nodeA].nodeA.position), Utils.ToV3(shape.formingEdges[nodeA].nodeB.position), new Color(1, 0, 1, 0.5f));//forming edges
            }

            for (int e = 0; e < shape.liveEdges.Count; e++)
            {
                Edge liveEdge = shape.liveEdges[e];
                Debug.DrawLine(Utils.ToV3(liveEdge.nodeA.position), Utils.ToV3(liveEdge.nodeB.position), new Color(1, 0, 0, 0.5f));//live edges
            }

            for (int s = 0; s < shape.edges.Count; s++)
            {
                Node nodeA = shape.edges[s].nodeA;
                Node nodeB = shape.edges[s].nodeB;
                Debug.DrawLine(Utils.ToV3(nodeA.position), Utils.ToV3(nodeB.position), new Color(0, 1, 1, 0.5f));//internal shape
                GizmoLabel.Label(nodeA.ToString(), Utils.ToV3(nodeA.position) + Vector3.up * s * 0.1f, 2);//static nodes
                GizmoLabel.Label(nodeB.ToString(), Utils.ToV3(nodeB.position) + Vector3.up * s * 0.1f, 2);//static nodes
            }

            for (int i = 0; i < shape.liveNodeCount; i++)
            {
                GizmoLabel.Label("Live Node " + shape.LiveNode(i).id, Utils.ToV3(shape.LiveNode(i).position - shape.LiveNode(i).direction), 2);//live nodes
                Debug.DrawLine(Utils.ToV3(shape.LiveNode(i).position), Utils.ToV3(shape.LiveNode(i).position - shape.LiveNode(i).direction), Color.yellow);
                Debug.DrawLine(Utils.ToV3(shape.LiveNode(i).position), Utils.ToV3(shape.LiveNode(i).position + shape.LiveNode(i).direction * shape.LiveNode(i).distance), new Color(1,1,0,0.2f));
                Debug.DrawLine(Utils.ToV3(shape.LiveNode(i).previousPosition), Utils.ToV3(shape.LiveNode(i).previousPosition) + Vector3.up, Color.green);
            }

            for (int i = 0; i < shape.staticNodeCount; i++)
            {
                GizmoLabel.Label("Static Node " + shape.StaticNode(i).id, Utils.ToV3(shape.StaticNode(i).position) + Vector3.up, 2);//static nodes
                                                                                                                                    //                Debug.DrawLine(Utils.ToV3(shape.StaticNode(i).position), Utils.ToV3(shape.StaticNode(i).position) + Vector3.up, Color.white);
            }

//            for (int i = 0; i < shape.edges.Count; i++)
//            {
//                Debug.DrawLine(Utils.ToV3(shape.baseEdges[i].nodeA.position), Utils.ToV3(shape.baseEdges[i].nodeB.position), Color.green);//live edges
//            }
#endif
        }
    }
}