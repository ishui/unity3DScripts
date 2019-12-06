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

[ExecuteInEditMode]
public class MeshDeBugger : MonoBehaviour
{
    public enum Modes
    {
        All,
        Selected
    }

    public Modes mode = Modes.All;

    [SerializeField]
    private bool _showNormals = false;
    [SerializeField]
    private bool _showTangents = false;

    private int[] selectedFace = new int[0];

    private MeshFilter[] _filters;

    private void OnEnable()
    {
        _filters = transform.GetComponentsInChildren<MeshFilter>();
    }

    private void OnDrawGizmos()
    {
        int filterCount = _filters.Length;
        for (int f = 0; f < filterCount; f++)
        {
            MeshFilter filter = _filters[f];
            Mesh mesh = filter.sharedMesh;

            if (mode == Modes.All)
            {
                int vertCount = mesh.vertexCount;

                if (_showNormals)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                    for (int n = 0; n < vertCount; n++)
                    {
                        Vector3 pos = mesh.vertices[n] + transform.position;
                        Vector3 norm = mesh.normals[n] * 0.5f;
                        if (norm.sqrMagnitude > Vector3.kEpsilon)
                            Gizmos.DrawLine(pos, pos + norm);
                        else
                            Gizmos.DrawSphere(pos, 1);
                    }
                }

                if (_showTangents)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.4f);
                    for (int n = 0; n < vertCount; n++)
                    {
                        Vector3 pos = mesh.vertices[n] + transform.position;
                        Vector4 tan = mesh.tangents[n] * 0.4f;
                        Vector3 tanV3 = new Vector3(tan.x, tan.y, tan.z);
                        if (tanV3.sqrMagnitude > Vector3.kEpsilon)
                            Gizmos.DrawLine(pos, pos + tanV3);
                        else
                            Gizmos.DrawSphere(pos, 1);
                    }
                }
            }
            else
            {
                List<int> tris = new List<int>();
                tris.AddRange(mesh.triangles);
                for (int sm = 0; sm < mesh.subMeshCount; sm++)
                    tris.AddRange(mesh.GetTriangles(sm));
                Vector3[] verts = mesh.vertices;
                for (int i = 0; i < tris.Count; i += 3)
                {
                    Vector3 v0 = verts[tris[i]];
                    Vector3 v1 = verts[tris[i + 1]];
                    Vector3 v2 = verts[tris[i + 2]];

                    if (MouseOver(v0, v1, v2))
                    {
                        DrawFace(mesh, tris[i], tris[i + 1], tris[i + 2]);

                        if (_showNormals)
                        {
                            DrawNormal(mesh, tris[i]);
                            DrawNormal(mesh, tris[i + 1]);
                            DrawNormal(mesh, tris[i + 2]);
                        }
                        if (_showTangents)
                        {
                            DrawTangent(mesh, tris[i]);
                            DrawTangent(mesh, tris[i + 1]);
                            DrawTangent(mesh, tris[i + 2]);
                        }

                        if (Event.current.type == EventType.MouseDown)
                            selectedFace = new[] { tris[i], tris[i + 1], tris[i + 2] };
                    }
                }
            }

            if (selectedFace.Length == 3)
            {
                DrawFace(mesh, selectedFace[0], selectedFace[1], selectedFace[2]);

                if (_showNormals)
                {
                    DrawNormal(mesh, selectedFace[0]);
                    DrawNormal(mesh, selectedFace[1]);
                    DrawNormal(mesh, selectedFace[2]);
                }
                if (_showTangents)
                {
                    DrawTangent(mesh, selectedFace[0]);
                    DrawTangent(mesh, selectedFace[1]);
                    DrawTangent(mesh, selectedFace[2]);
                }
            }
        }


        //        if (_filter == null)
        //            _filter = transform.GetComponentInChildren<MeshFilter>();
        //        if (_filter != null)
        //        {
        //            Mesh mesh = _filter.sharedMesh;
        //
        //            if (mode == Modes.All)
        //            {
        //                int vertCount = mesh.vertexCount;
        //
        //                if (_showNormals)
        //                {
        //                    Gizmos.color = new Color(0, 1, 0, 0.4f);
        //                    for (int n = 0; n < vertCount; n++)
        //                    {
        //                        Vector3 pos = mesh.vertices[n] + transform.position;
        //                        Vector3 norm = mesh.normals[n];
        //                        if (norm.sqrMagnitude > Vector3.kEpsilon)
        //                            Gizmos.DrawLine(pos, pos + norm);
        //                        else
        //                            Gizmos.DrawSphere(pos, 1);
        //                    }
        //                }
        //
        //                if (_showTangents)
        //                {
        //                    Gizmos.color = new Color(1, 0, 0, 0.4f);
        //                    for (int n = 0; n < vertCount; n++)
        //                    {
        //                        Vector3 pos = mesh.vertices[n] + transform.position;
        //                        Vector4 tan = mesh.tangents[n];
        //                        Vector3 tanV3 = new Vector3(tan.x, tan.y, tan.z);
        //                        if (tanV3.sqrMagnitude > Vector3.kEpsilon)
        //                            Gizmos.DrawLine(pos, pos + tanV3);
        //                        else
        //                            Gizmos.DrawSphere(pos, 1);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                List<int> tris = new List<int>();
        //                tris.AddRange(mesh.triangles);
        //                for(int sm = 0; sm < mesh.subMeshCount; sm++)
        //                    tris.AddRange(mesh.GetTriangles(sm));
        //                Vector3[] verts = mesh.vertices;
        //                for (int i = 0; i < tris.Count; i += 3)
        //                {
        //                    Vector3 v0 = verts[tris[i]];
        //                    Vector3 v1 = verts[tris[i + 1]];
        //                    Vector3 v2 = verts[tris[i + 2]];
        //
        //                    if (MouseOver(v0, v1, v2))
        //                    {
        //                        DrawFace(mesh, tris[i], tris[i+1], tris[i+2]);
        //
        //                        if (_showNormals)
        //                        {
        //                            DrawNormal(mesh, tris[i]);
        //                            DrawNormal(mesh, tris[i + 1]);
        //                            DrawNormal(mesh, tris[i + 2]);
        //                        }
        //                        if (_showTangents)
        //                        {
        //                            DrawTangent(mesh, tris[i]);
        //                            DrawTangent(mesh, tris[i + 1]);
        //                            DrawTangent(mesh, tris[i + 2]);
        //                        }
        //
        //                        if(Event.current.type == EventType.MouseDown)
        //                            selectedFace = new []{tris[i], tris[i + 1], tris[i + 2]};
        //                    }
        //                }
        //            }
        //
        //            if(selectedFace.Length == 3)
        //            {
        //                DrawFace(mesh, selectedFace[0], selectedFace[1], selectedFace[2]);
        //
        //                if (_showNormals)
        //                {
        //                    DrawNormal(mesh, selectedFace[0]);
        //                    DrawNormal(mesh, selectedFace[1]);
        //                    DrawNormal(mesh, selectedFace[2]);
        //                }
        //                if (_showTangents)
        //                {
        //                    DrawTangent(mesh, selectedFace[0]);
        //                    DrawTangent(mesh, selectedFace[1]);
        //                    DrawTangent(mesh, selectedFace[2]);
        //                }
        //            }
        //        }
    }

    private void DrawFace(Mesh mesh, int index0, int index1, int index2)
    {
        Gizmos.color = Color.yellow;

        Vector3 p0 = mesh.vertices[index0] + transform.position;
        Vector3 p1 = mesh.vertices[index1] + transform.position;
        Vector3 p2 = mesh.vertices[index2] + transform.position;

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p0);
        Gizmos.DrawLine(p2, p0);
    }

    private void DrawNormal(Mesh mesh, int index)
    {
        Gizmos.color = Color.green;
        Vector3 pos = mesh.vertices[index] + transform.position;
        Vector3 norm = mesh.normals[index];
        if (norm.sqrMagnitude > Vector3.kEpsilon)
        {
            Gizmos.DrawLine(pos, pos + norm);
            Gizmos.DrawLine(pos, pos + norm);
        }
        else
            Gizmos.DrawSphere(pos, 1);
    }

    private void DrawTangent(Mesh mesh, int index)
    {
        Gizmos.color = Color.red;
        Vector3 pos = mesh.vertices[index] + transform.position;
        Vector4 tan = mesh.tangents[index];
        Vector3 tanV3 = new Vector3(tan.x, tan.y, tan.z);
        if (tanV3.sqrMagnitude > Vector3.kEpsilon)
        {
            Gizmos.DrawLine(pos, pos + tanV3);
            Gizmos.DrawLine(pos, pos + tanV3);
            Gizmos.DrawLine(pos, pos + tanV3);
        }
        else
            Gizmos.DrawSphere(pos, 1);
    }

    private bool MouseOver(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Event current = Event.current;
        Vector3 mousePos = Vector3.zero;
        switch (current.type)
        {
            case EventType.MouseMove:
                mousePos = Event.current.mousePosition;
                mousePos.y = Camera.current.pixelHeight - mousePos.y;
                break;

            case EventType.MouseDown:
                //
                break;
        }
        Ray mouseRay = Camera.current.ScreenPointToRay(mousePos);
        float distance;
        if (TriangleIntersection(p0, p1, p2, mouseRay, out distance)) return true;
        return false;
    }

    private bool TriangleIntersection(Vector3 v0, Vector3 v1, Vector3 v2, Ray ray, out float distance)
    {
        distance = 0;
        Vector3 v0v1 = v1 - v0;
        Vector3 v0v2 = v2 - v0;
        Vector3 pvec = Vector3.Cross(ray.direction, v0v2);
        var det = Vector3.Dot(v0v1, pvec);

        if (det < Vector3.kEpsilon) return false;//backfacing

        if (Mathf.Abs(det) < Vector3.kEpsilon) return false;//parallel

        var invDet = 1 / det;

        Vector3 tvec = ray.origin - v0;
        var u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0 || u > 1) return false;

        Vector3 qvec = Vector3.Cross(tvec, v0v1);
        var v = Vector3.Dot(ray.direction, qvec) * invDet;
        if (v < 0 || u + v > 1) return false;

        distance = Vector3.Dot(v0v2, qvec) * invDet;

        return true;
    }
}
