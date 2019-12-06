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

namespace BuildR2
{
    public class PortalGenerator
    {
        public static BuildRMesh DYNAMIC_MESH = new BuildRMesh("portal mesh");

        public static void Generate(Portal portal, ref Mesh mesh, Vector2 size, SubmeshLibrary submeshLibrary = null)
        {
            mesh.Clear(false);
            DYNAMIC_MESH.Clear();

	        if(submeshLibrary != null)
		        DYNAMIC_MESH.submeshLibrary.AddRange(submeshLibrary.SURFACES.ToArray());//Inject(ref submeshLibrary);
			else
				DYNAMIC_MESH.submeshLibrary.Add(portal);
			
			Portal(ref DYNAMIC_MESH, portal, size, Vector2.zero, true, submeshLibrary);
            DYNAMIC_MESH.Build(mesh);
        }

        public static void Portal(ref BuildRMesh dynamicMesh, Portal portal, Vector2 size, Vector3 offset, bool interior = true, SubmeshLibrary submeshLibrary = null)
		{
			if (submeshLibrary == null)
			{
				submeshLibrary = new SubmeshLibrary();
				submeshLibrary.Add(portal);
			}

			Division root = portal.root;

            List<Panel> processNodes = new List<Panel>();
            Dictionary<Panel, Panel[]> dataDic = new Dictionary<Panel, Panel[]>();
            List<Panel> data = new List<Panel>();
            Panel rootPanel = new Panel(root, new Rect(0, 0, size.x, size.y), 0);
            processNodes.Add(rootPanel);
            float totalDepth = 0;
            while (processNodes.Count > 0)
            {
                Panel current = processNodes[0];
                Division division = current.division;
                List<Division> children = division.GetChildren;
                int childCount = children.Count;

                data.Add(current);//dump processed node into data.
                dataDic.Add(current, new Panel[childCount]);
                if(current.recess > totalDepth) totalDepth = current.recess;

                float childRatio = 0;
                for (int c = 0; c < childCount; c++)
                    childRatio += children[c].size;

                for (int c = 0; c < childCount; c++)
                {
                    Division child = children[c];
                    Rect newPanelrect = current.rect;
                    float ratio = children[c].size / childRatio;

                    if (division.divisionType == BuildR2.Portal.DivisionTypes.Horizontal)
                    {
                        newPanelrect.width = Mathf.Max(newPanelrect.width - division.frame * 2 - division.frame * (childCount - 1), 0) * ratio;
                        newPanelrect.height = Mathf.Max(newPanelrect.height - division.frame * 2, 0);
                        if (c > 0)
                        {
                            Panel lastPanel = processNodes[processNodes.Count - 1];
                            newPanelrect.x = lastPanel.rect.xMax + division.frame;
                        }
                        else
                            newPanelrect.x = current.rect.xMin + division.frame;
                        newPanelrect.y = current.rect.yMin + division.frame;
                    }
                    else
                    {
                        newPanelrect.width = Mathf.Max(newPanelrect.width - division.frame * 2, 0);
                        newPanelrect.height = Mathf.Max(newPanelrect.height - division.frame * 2 - division.frame * (childCount - 1), 0) * ratio;
                        if (c > 0)
                        {
                            Panel lastPanel = processNodes[processNodes.Count - 1];
                            newPanelrect.y = lastPanel.rect.y + lastPanel.rect.height + division.frame;
                        }
                        else
                            newPanelrect.y = current.rect.yMin + division.frame;
                        newPanelrect.x = current.rect.xMin + division.frame;
                    }

                    Panel childPanel = new Panel(child, newPanelrect, current.recess + division.recess);
                    dataDic[current][c] = childPanel;

                    processNodes.Add(childPanel);
                }

                processNodes.RemoveAt(0);
            }

            int dataCount = data.Count;
            Vector3 norm = Vector3.back;
            Vector4 tangent = BuildRMesh.CalculateTangent(Vector3.right);
            Vector4 tangentForward = BuildRMesh.CalculateTangent(Vector3.forward);
            Vector4 tangentBack = BuildRMesh.CalculateTangent(Vector3.back);
            Vector4 tangentInvert = BuildRMesh.CalculateTangent(Vector3.left);
            Vector3 useOffset = size * 0.5f;
            useOffset += offset;
            useOffset.y = -useOffset.y;//inverse - UX resaons
            for (int i = 0; i < dataCount; i++)
            {
                Panel panel = data[i];
                Division division = panel.division;

                Rect panelRect = panel.rect;

                if (panelRect.width == 0 || panelRect.height == 0)
                    continue;

                Vector3 v0 = new Vector3(panelRect.xMin, -panelRect.yMin, panel.recess) - useOffset;
                Vector3 v1 = new Vector3(panelRect.xMax, -panelRect.yMin, panel.recess) - useOffset;
                Vector3 v2 = new Vector3(panelRect.xMin, -panelRect.yMax, panel.recess) - useOffset;
                Vector3 v3 = new Vector3(panelRect.xMax, -panelRect.yMax, panel.recess) - useOffset;

                Surface usedSurface = GetSurface(portal, division);
	            int useSubmesh = submeshLibrary.SubmeshAdd(usedSurface);
//                int useSubmesh = usedSurface != null ? Array.IndexOf(usedSurfaces, usedSurface) : 0;

                Vector2 uv0 = CalculateUV(usedSurface, v0);
                Vector2 uv1 = CalculateUV(usedSurface, v1);
                Vector2 uv2 = CalculateUV(usedSurface, v2);
                Vector2 uv3 = CalculateUV(usedSurface, v3);

                if (!division.hasChildren)//simple panel
                {
                    Vector3[] verts = { v0, v1, v2, v3 };
                    Vector2[] uvs = { uv0, uv1, uv2, uv3 };
                    int[] tris = { 0, 1, 2, 1, 3, 2 };
                    Vector3[] norms = { norm, norm, norm, norm };
                    Vector4[] tangents = { tangent, tangent, tangent, tangent };
                    dynamicMesh.AddData(verts, uvs, tris, norms, tangents, useSubmesh);
                    
                    if(interior)
                    {
                        Vector3 interiorOffset = new Vector3(0, 0, (totalDepth - panel.recess) * 2);
                        verts = new[] { v0 + interiorOffset, v1 + interiorOffset, v2 + interiorOffset, v3 + interiorOffset };
                        uvs = new[] { uv1, uv0, uv3, uv2 };
                        tris = new[] { 0, 2, 1, 1, 2, 3 };
                        norms = new[] { -norm, -norm, -norm, -norm };
                        tangents = new[] { tangentInvert, tangentInvert, tangentInvert, tangentInvert };

                        dynamicMesh.AddData(verts, uvs, tris, norms, tangents, useSubmesh);
                    }
                }
                else//build a frame
                {
                    Vector3 v0f = v0 + new Vector3(division.frame, -division.frame, 0);
                    Vector3 v1f = v1 + new Vector3(-division.frame, -division.frame, 0);
                    Vector3 v2f = v2 + new Vector3(division.frame, division.frame, 0);
                    Vector3 v3f = v3 + new Vector3(-division.frame, division.frame, 0);

                    Vector3 recessV = Vector3.forward * (division.recess);
                    Vector3 v0r = v0f + recessV;
                    Vector3 v1r = v1f + recessV;
                    Vector3 v2r = v2f + recessV;
                    Vector3 v3r = v3f + recessV;

                    Vector2 uv0f = CalculateUV(usedSurface, v0f);
                    Vector2 uv1f = CalculateUV(usedSurface, v1f);
                    Vector2 uv2f = CalculateUV(usedSurface, v2f);
                    Vector2 uv3f = CalculateUV(usedSurface, v3f);

//                    Vector2 uv0r = CalculateUV(usedSurface, v0r);
//                    Vector2 uv1r = CalculateUV(usedSurface, v1r);
//                    Vector2 uv2r = CalculateUV(usedSurface, v2r);
//                    Vector2 uv3r = CalculateUV(usedSurface, v3r);

                    Vector3[] verts = {
                        v0, v1, v2, v3,
                        v0f, v1f, v2f, v3f
                    };
                    Vector2[] uvs ={
                        uv0, uv1, uv2, uv3,
                        uv0f, uv1f, uv2f, uv3f
                                   };
                    Vector3[] norms ={
                        norm, norm, norm, norm,
                        norm, norm, norm, norm
                                     };
                    Vector4[] tangents ={
                        tangent, tangent, tangent, tangent,
                        tangent, tangent, tangent, tangent
                                        };

                    int[] tris ={
                                    0, 4, 2, 4, 6, 2,//left
                                    0, 1, 4, 1, 5, 4,//top
                                    5, 1, 3, 5, 3, 7,//right
                                    2, 6, 3, 3, 6, 7,//bottom
                                };

                    dynamicMesh.AddData(verts, uvs, tris, norms, tangents, useSubmesh);
                    Vector2 uvUp = CalculateUV(usedSurface, new Vector2(0, division.recess));
                    Vector2 uvRight = CalculateUV(usedSurface, new Vector2(division.recess, 0));
                    dynamicMesh.AddPlaneComplex(v1f, v0f, v1r, v0r, uv1f, uv0f, uv1f + uvUp, uv0f + uvUp, Vector3.down, tangent, useSubmesh, usedSurface);//top
                    dynamicMesh.AddPlaneComplex(v2f, v3f, v2r, v3r, uv2f, uv3f, uv2f + uvUp, uv3f + uvUp, Vector3.up, tangent, useSubmesh, usedSurface);//bottom
                    dynamicMesh.AddPlaneComplex(v0f, v2f, v0r, v2r, uv0f, uv2f, uv0f + uvRight, uv2f + uvRight, Vector3.right, tangentForward, useSubmesh, usedSurface);//left
                    dynamicMesh.AddPlaneComplex(v3f, v1f, v3r, v1r, uv3f, uv1f, uv3f + uvRight, uv1f + uvRight, Vector3.left, tangentBack, useSubmesh, usedSurface);//right

                    if(interior)
                    {
                        Vector3 interiorOffset = new Vector3(0, 0, (totalDepth - panel.recess) * 2);
                        Vector3 interiorOffsetr = new Vector3(0, 0, (totalDepth - panel.recess - division.recess) * 2);
                        verts = new[]{v0 + interiorOffset, v1 + interiorOffset, v2 + interiorOffset, v3 + interiorOffset, v0f + interiorOffset, v1f + interiorOffset, v2f + interiorOffset, v3f + interiorOffset };
                        uvs = new []{uv1, uv0, uv3, uv2, uv1f, uv0f, uv3f, uv2f};
                        Array.Reverse(tris);
                        norms = new[]{-norm, -norm, -norm, -norm, -norm, -norm, -norm, -norm};
                        tangents = new[]{tangentInvert, tangentInvert, tangentInvert, tangentInvert, tangentInvert, tangentInvert, tangentInvert, tangentInvert };

                        dynamicMesh.AddData(verts, uvs, tris, norms, tangents, useSubmesh);
                        dynamicMesh.AddPlaneComplex(v0f + interiorOffset, v1f + interiorOffset, v0r + interiorOffsetr, v1r + interiorOffsetr, uv0f, uv1f, uv0f + uvUp, uv1f + uvUp, Vector3.down, tangentInvert, useSubmesh, usedSurface);//top
                        dynamicMesh.AddPlaneComplex(v3f + interiorOffset, v2f + interiorOffset, v3r + interiorOffsetr, v2r + interiorOffsetr, uv3f, uv2f, uv3f + uvUp, uv2f + uvUp, Vector3.up, tangentInvert, useSubmesh, usedSurface);//bottom
                        dynamicMesh.AddPlaneComplex(v2f + interiorOffset, v0f + interiorOffset, v2r + interiorOffsetr, v0r + interiorOffsetr, uv2f, uv0f, uv2f + uvRight, uv0f + uvRight, Vector3.right, tangentBack, useSubmesh, usedSurface);//left
                        dynamicMesh.AddPlaneComplex(v3f + interiorOffset, v3f + interiorOffset, v3r + interiorOffsetr, v3r + interiorOffsetr, uv1f, uv3f, uv1f + uvRight, uv3f + uvRight, Vector3.left, tangentForward, useSubmesh, usedSurface);//right
                    }

                    List<Division> children = division.GetChildren;
                    int childCount = children.Count;
                    if (childCount > 1 && division.frame > 0)
                    {
                        for (int c = 0; c < childCount - 1; c++)
                        {
                            Panel childPanel = dataDic[panel][c];
                            if (division.divisionType == BuildR2.Portal.DivisionTypes.Horizontal)
                            {
                                Vector3 v0d = v0 + new Vector3(childPanel.rect.xMax - panelRect.xMin, -division.frame, 0);
                                Vector3 v1d = v0d + new Vector3(division.frame, 0, 0);
                                Vector3 v2d = v0d + new Vector3(0, -panelRect.height + division.frame * 2, 0);
                                Vector3 v3d = v1d + new Vector3(0, -panelRect.height + division.frame * 2, 0);
                                
                                Vector2 uv0d = CalculateUV(usedSurface, v0d);
                                Vector2 uv1d = CalculateUV(usedSurface, v1d);
                                Vector2 uv2d = CalculateUV(usedSurface, v2d);
                                Vector2 uv3d = CalculateUV(usedSurface, v3d);

                                dynamicMesh.AddPlaneComplex(v1d, v0d, v3d, v2d, uv0d, uv1d, uv2d, uv3d, norm, tangent, useSubmesh, usedSurface);//divider face

                                dynamicMesh.AddPlaneComplex(v2d, v0d, v2d + recessV, v0d + recessV, uv2d, uv0d, uv2d + uvRight, uv0d + uvRight, Vector3.left, tangentBack, useSubmesh, usedSurface);//divider left
                                dynamicMesh.AddPlaneComplex(v1d, v3d, v1d + recessV, v3d + recessV, uv1d, uv3d, uv1d - uvRight, uv3d - uvRight, Vector3.right, tangentBack, useSubmesh, usedSurface);//divider right

                                if(interior)
                                {
                                    Vector3 interiorOffset = new Vector3(0, 0, (totalDepth - panel.recess) * 2);
                                    Vector3 interiorOffsetr = new Vector3(0, 0, (totalDepth - panel.recess - division.recess) * 2);
                                    dynamicMesh.AddPlaneComplex(v0d + interiorOffset, v1d + interiorOffset, v2d + interiorOffset, v3d + interiorOffset, uv1d, uv0d, uv3d, uv2d, -norm, tangentInvert, useSubmesh, usedSurface);//divider face
                                    dynamicMesh.AddPlaneComplex(v0d + interiorOffset, v2d + interiorOffset, v0d + recessV + interiorOffsetr, v2d + recessV + interiorOffsetr, uv0d, uv2d, uv0d + uvRight, uv2d + uvRight, Vector3.left, tangentForward, useSubmesh, usedSurface);//divider left
                                    dynamicMesh.AddPlaneComplex(v3d + interiorOffset, v1d + interiorOffset, v3d + recessV + interiorOffsetr, v1d + recessV + interiorOffsetr, uv3d, uv1d, uv3d - uvRight, uv1d - uvRight, Vector3.right, tangentForward, useSubmesh, usedSurface);//divider right
                                }
                            }
                            else
                            {
                                Vector3 v0d = v0 + new Vector3(division.frame, -childPanel.rect.yMax + panelRect.yMin, 0);
                                Vector3 v1d = v0d + new Vector3(0, -division.frame, 0);
                                Vector3 v2d = v0d + new Vector3(panelRect.width - division.frame * 2, 0, 0);
                                Vector3 v3d = v1d + new Vector3(panelRect.width - division.frame * 2, 0, 0);

                                Vector2 uv0d = CalculateUV(usedSurface, v0d);
                                Vector2 uv1d = CalculateUV(usedSurface, v1d);
                                Vector2 uv2d = CalculateUV(usedSurface, v2d);
                                Vector2 uv3d = CalculateUV(usedSurface, v3d);

                                dynamicMesh.AddPlaneComplex(v0d, v1d, v2d, v3d, uv0d, uv1d, uv2d, uv3d, norm, tangent, useSubmesh, usedSurface);//divider face

                                dynamicMesh.AddPlaneComplex(v0d, v2d, v0d + recessV, v2d + recessV, uv0d, uv2d, uv0d + uvUp, uv2d + uvUp, Vector3.up, tangent, useSubmesh, usedSurface);//divider top
                                dynamicMesh.AddPlaneComplex(v3d, v1d, v3d + recessV, v1d + recessV, uv3d, uv1d, uv3d - uvUp, uv1d - uvUp, Vector3.down, tangent, useSubmesh, usedSurface);//divider bottom

                                if(interior)
                                {
                                    Vector3 interiorOffset = new Vector3(0, 0, (totalDepth - panel.recess) * 2);
                                    Vector3 interiorOffsetr = new Vector3(0, 0, (totalDepth - panel.recess - division.recess) * 2);
                                    dynamicMesh.AddPlaneComplex(v1d + interiorOffset, v0d + interiorOffset, v3d + interiorOffset, v2d + interiorOffset, uv1d, uv0d, uv3d, uv2d, -norm, tangentInvert, useSubmesh, usedSurface);//divider face
                                    dynamicMesh.AddPlaneComplex(v2d + interiorOffset, v0d + interiorOffset, v2d + recessV + interiorOffsetr, v0d + recessV + interiorOffsetr, uv2d, uv0d, uv2d + uvUp, uv0d + uvUp, Vector3.up, tangentInvert, useSubmesh, usedSurface);//divider top
                                    dynamicMesh.AddPlaneComplex(v1d + interiorOffset, v3d + interiorOffset, v1d + recessV + interiorOffsetr, v3d + recessV + interiorOffsetr, uv1d, uv3d, uv1d - uvUp, uv3d - uvUp, Vector3.down, tangentInvert, useSubmesh, usedSurface);//divider bottom
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Surface GetSurface(Portal portal, Division division)
        {
            if (division.surface != null)
                return division.surface;

            if (division.hasChildren)
            {
                if (portal.defaultFrameTexture) return portal.defaultFrameTexture;
            }
            else
            {
                if (portal.defaultPanelTexture) return portal.defaultPanelTexture;
            }

            return null;
        }

        private static Vector2 CalculateUV(Surface surface, Vector2 uv)
        {
            if(surface == null) return uv;
            return surface.CalculateUV(uv);
        }

        private struct Panel
        {
            public Rect rect;
            public float recess;
            public Division division;

            public Panel(Division div, Rect rct, float rcs)
            {
                division = div;
                rect = rct;
                recess = rcs;
            }
        }
    }
}