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
using System.IO;
using UnityEngine;
using JaspLib;
using UnityEditor;

namespace BuildR2
{
    public class BuildingEditorUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <returns>The point index of the wall extension. No point will return -1</returns>
        public static int OnSelectWall(Volume volume)
        {
            Vector3 position = volume.transform.position;
            Quaternion rotation = volume.transform.rotation;
            int numberOfPoints = volume.numberOfPoints;
            for (int p = 0; p < numberOfPoints; p++)
            {
                Vector3 p0 = rotation * volume.BuildingPoint(p) + position;
                Vector3 p1 = rotation * volume.BuildingPoint((p + 1) % numberOfPoints) + position;
                Vector3 wC = Vector3.Lerp(p0, p1, 0.5f);

                float pointHandleSize = HandleUtility.GetHandleSize(wC);
                if (UnityVersionWrapper.HandlesDotButton(wC, Quaternion.identity, pointHandleSize * 0.1f, pointHandleSize * 0.1f))
                {
                    return p;
                }
            }
            return -1;//no point selected
        }

        public static void ExtrudeWallWithNewPoints(Volume volume, int atIndex)
        {
            int numberOfPoints = volume.numberOfPoints;
            Vector2Int p0 = volume[atIndex].position;
            Vector2Int p1 = volume[(atIndex + 1) % numberOfPoints].position;
            float pointDistance = Vector2Int.DistanceWorld(p0, p1);
            Vector2 dir = (p1.vector2 - p0.vector2).normalized;
            Vector2 norm = JMath.Rotate(dir, -90);

            Vector2Int p2 = p0 + norm * pointDistance;
            Vector2Int p3 = p1 + norm * pointDistance;

            atIndex = (atIndex + 1 + numberOfPoints) % numberOfPoints;
            volume.InsertPoint(atIndex, p2);
            volume.InsertPoint(atIndex + 1, p3);
        }

        public static IVolume ExtrudeWallIntoNewVolume(Building building, Volume volume, int atIndex)
        {
            int numberOfPoints = volume.numberOfPoints;
            Vector2Int p0 = volume[atIndex].position;
            Vector2Int p1 = volume[(atIndex + 1) % numberOfPoints].position;
            float pointDistance = Vector2Int.DistanceWorld(p0, p1);
            Vector2 dir = (p1.vector2 - p0.vector2).normalized;
            Vector2 norm = JMath.Rotate(dir, -90);
            Vector2Int p2 = p0 + norm * pointDistance;
            Vector2Int p3 = p1 + norm * pointDistance;

            return building.AddPlan(new[] { p0, p2, p3, p1 });
        }

        public static void CurveWall(Volume volume, int atIndex)
        {
            int numberOfPoints = volume.numberOfPoints;
            Vector2Int p0 = volume[atIndex].position;
            Vector2Int p1 = volume[(atIndex + 1) % numberOfPoints].position;
            Vector2Int p2 = volume[(atIndex - 1 + numberOfPoints) % numberOfPoints].position;
            Vector2Int p3 = volume[(atIndex + 2) % numberOfPoints].position;
            Vector2 leftWallDirection = (p0.vector2 - p2.vector2).normalized;
            Vector2 rightWallDirection = (p1.vector2 - p3.vector2).normalized;
            float pointDistance = Vector2Int.DistanceWorld(p0, p1);

            Vector2Int x0 = p0 - (p2.vector2 - p0.vector2).normalized * pointDistance * 0.6f;
            Vector2Int x1 = p1 - (p3.vector2 - p1.vector2).normalized * pointDistance * 0.6f;
            Vector2 dir = (x1.vector2 - x0.vector2).normalized;
            float dot = Vector2.Dot(leftWallDirection, rightWallDirection);
            float dotVal = Mathf.Clamp(0.1f + dot * 0.2f, 0, 0.3f);
            x0 += dir * pointDistance * dotVal;
            x1 += -dir * pointDistance * dotVal;
            
            volume.SetControlPointA(atIndex, x0);
            volume.SetControlPointB(atIndex, x1);
        }

        public static string RenameAsset(ScriptableObject asset, string newName)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
            string output = AssetDatabase.RenameAsset(assetPath, newName);
            if (output.Length > 0) return output;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return "";
        }

      public static void OnInteractivePreviewGUI(ref PreviewRenderUtility _mPrevRender, ref Vector2 _drag, Mesh mesh, Material[] mats, Rect r, GUIStyle background, float radius, Mesh _plane = null, Material _blueprintMaterial = null)
      {
        
      }
//            _drag = Drag2D(_drag, r);
//
//            if (_mPrevRender == null)
//                _mPrevRender = new PreviewRenderUtility();
//
//            //            Vector3 max = _wallSection.previewMesh.bounds.size;
//            //            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 1.333f;
//            float dist = radius / (Mathf.Sin(_mPrevRender.camera.fieldOfView * Mathf.Deg2Rad));
//            _mPrevRender.camera.transform.position = Vector2.zero;
//            _mPrevRender.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
//            _mPrevRender.camera.transform.position = _mPrevRender.camera.transform.forward * -dist;
//            _mPrevRender.camera.nearClipPlane = 0.1f;
//            _mPrevRender.camera.farClipPlane = 500;
//
//            _mPrevRender.lights[0].intensity = 0.5f;
//            _mPrevRender.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
//            _mPrevRender.lights[1].intensity = 0.5f;
//
//            _mPrevRender.BeginPreview(r, background);
//
//            if (_plane != null && _blueprintMaterial != null)
//            {
//                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(-25, -25, 1), Quaternion.identity, new Vector3(10, 10, 1));
//                _mPrevRender.DrawMesh(_plane, matrix, _blueprintMaterial, 0);
//                _mPrevRender.camera.Render();
//            }
//
//
//            int materialCount = mats.Length;
//            int submeshCount = mesh.subMeshCount;
//            int count = Mathf.Min(materialCount, submeshCount);
//            for (int c = 0; c < count; c++)
//            {
//                Material mat = c < materialCount ? mats[c] : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
//                _mPrevRender.DrawMesh(mesh, Matrix4x4.identity, mat, c);
//            }
//
//            _mPrevRender.camera.Render();
//            Texture texture = _mPrevRender.EndPreview();
//
//            GUI.DrawTexture(r, texture);
//        }
//
//        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
//        {
//            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
//            Event current = Event.current;
//            switch (current.GetTypeForControl(controlID))
//            {
//                case EventType.MouseDown:
//                    if (position.Contains(current.mousePosition) && position.width > 50f)
//                    {
//                        GUIUtility.hotControl = controlID;
//                        current.Use();
//                        EditorGUIUtility.SetWantsMouseJumping(1);
//                    }
//                    break;
//                case EventType.MouseUp:
//                    if (GUIUtility.hotControl == controlID)
//                    {
//                        GUIUtility.hotControl = 0;
//                    }
//                    EditorGUIUtility.SetWantsMouseJumping(0);
//                    break;
//                case EventType.MouseDrag:
//                    if (GUIUtility.hotControl == controlID)
//                    {
//                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
//                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
//                        current.Use();
//                        GUI.changed = true;
//                    }
//                    break;
//            }
//            return scrollPosition;
//        }


        public static FloorplanClick OnInteriorSelectionClick(Ray mouseRay)
        {
            FloorplanClick output = new FloorplanClick();
            Building building = BuildingEditor.building;
            Volume volume = BuildingEditor.volume;
            Floorplan floorplan = BuildingEditor.floorplan;

            output.volume = volume;
            output.floorplan = floorplan;

            if(floorplan != null)
            {
                Vector3 basePosition = building.transform.position;
                float baseHeight = volume.baseHeight;
                Vector3 testPoint = basePosition + Vector3.up * baseHeight;
                if(Vector3.Dot(mouseRay.direction, testPoint - mouseRay.origin) < 0)//volume behind camera 
                    return output;

                float floorHeight = volume.floorHeight;
                List<Room> rooms = floorplan.rooms;
                int roomCount = rooms.Count;
                float minDistance = Mathf.Infinity;
                for (int rm = 0; rm < roomCount; rm++)
                {
                    Room room = rooms[rm];
                    RoomPortal[] portals = room.GetAllPortals();
                    int portalCount = portals.Length;
                    for (int p = 0; p < portalCount; p++)
                    {
                        RoomPortal portal = portals[p];

                        int wallIndex = portal.wallIndex;
                        Vector3 p0 = room[wallIndex].position.vector3XZ;
                        Vector3 p1 = room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
                        Vector3 baseUp = Vector3.up * (floorHeight - portal.height) * portal.verticalPosition;
                        Vector3 portalUp = baseUp + Vector3.up * portal.height;
                        Vector3 pointPos = SceneMeshHandler.PortalPosition(Quaternion.identity, room, portal);
                        pointPos.y = baseHeight;
                        Vector3 wallDirection = (p1 - p0).normalized;
                        float defaultWidth = portal.width * 0.5f;
//                        float defaultDepth = 0.1f;

                        Vector3 v0 = pointPos - wallDirection * defaultWidth;
                        Vector3 v1 = pointPos + wallDirection * defaultWidth;
                        Vector3 v2 = v0 + portalUp;
                        Vector3 v3 = v1 + portalUp;

//                        Debug.DrawLine(v0,v1,Color.red,20);
//                        Debug.DrawLine(v1,v3,Color.red,20);
//                        Debug.DrawLine(v3,v2,Color.red,20);
//                        Debug.DrawLine(v2,v0,Color.red,20);

                        float distance = 0;
                        if(RayTriangle.QuadIntersection(v0, v1, v2, v3, mouseRay, out distance, false))
                        {
                            if(minDistance > distance)
                            {
                                minDistance = distance;
                                output.room = room;
                                output.portal = portal;
                                output.opening = null;
                            }
                        }
                    }
                }

                VerticalOpening[] openings = building.GetAllOpenings();
                int openingCount = openings.Length;
                for (int o = 0; o < openingCount; o++)
                {
                    VerticalOpening opening = openings[o];

                    Vector3 openingPosition = opening.position.vector3XZ;
                    openingPosition.y = volume.floorHeight * opening.baseFloor;
                    Vector3 openingSize = opening.size.vector3XZ;
                    float openingWidth = openingSize.x;
                    float openingHeight = openingSize.z;
                    Quaternion openingRotation = Quaternion.Euler(0, opening.rotation, 0);
                    Vector3 p0 = openingPosition + openingRotation * new Vector3(-openingWidth, 0, -openingHeight) * 0.5f;
                    Vector3 p1 = openingPosition + openingRotation * new Vector3(openingWidth, 0, -openingHeight) * 0.5f;
                    Vector3 p2 = openingPosition + openingRotation * new Vector3(openingWidth, 0, openingHeight) * 0.5f;
                    Vector3 p3 = openingPosition + openingRotation * new Vector3(-openingWidth, 0, openingHeight) * 0.5f;
//                    Vector3 openingUp = Vector3.up * volume.floorHeight * (opening.floors + 1);

                    Vector3 floorUpA = Vector3.up * volume.CalculateFloorHeight(volume.Floor(BuildingEditor.floorplan));
//                    Vector3 floorUpB = floorUpA + Vector3.up * volume.floorHeight;

                    float distance = 0;
                    if (RayTriangle.QuadIntersection(p0 + floorUpA, p1 + floorUpA, p2 + floorUpA, p3 + floorUpA, mouseRay, out distance, false))
                    {
                        if (minDistance > distance)
                        {
                            minDistance = distance;
                            output.room = null;
                            output.portal = null;
                            output.opening = opening;
                        }
                    }
                }

                if (output.opening != null)
                    return output;

                if (output.portal != null)
                    return output;

                float intPlanBaseHeight = volume.CalculateFloorHeight(volume.Floor(floorplan));
                Vector3 baseUpV = Vector3.up * intPlanBaseHeight;
                Plane planPlane = new Plane(Vector3.up, baseUpV);
                float rayDistance = 0;
                if(planPlane.Raycast(mouseRay, out rayDistance))
                {
                    Vector3 clickPos = mouseRay.GetPoint(rayDistance) - basePosition;
                    Vector2Int floorClickPosition = new Vector2Int(clickPos, true);
                    
                    for (int rm = 0; rm < roomCount; rm++)
                    {
                        Vector2Int[] roomPoints = rooms[rm].AllPoints();
                        if (BuildrUtils.PointInsidePoly(floorClickPosition, roomPoints))
                            output.room = rooms[rm];
                    }
                }

            }
            return output;
        }

        public class FloorplanClick
        {
            public Volume volume;
            public Floorplan floorplan;
            public Room room;
            public VerticalOpening opening;
            public RoomPortal portal;

            public override string ToString()
            {
                if (volume == null)
                    return "none";
                return string.Format("{0} {1} {2} {3} {4}", ((Object)volume).name, volume.Floor(floorplan), room, portal, opening);
            }
        }
    }
}