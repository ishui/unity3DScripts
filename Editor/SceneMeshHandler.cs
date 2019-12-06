using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace BuildR2
{
    public class SceneMeshHandler
    {
        private struct SceneMeshLine
        {
            public Vector3 a;
            public Vector3 b;
            public Color col;

            public SceneMeshLine(Vector3 a, Vector3 b, Color col)
            {
                this.a = bPosition + bRotation * a;
                this.b = bPosition + bRotation * b;
                this.col = col;
            }
        }

        private struct SceneMeshShape
        {
            public Vector3[] points;
            public Color col;

            public SceneMeshShape(Vector3[] points, Color col)
            {
                int pointCount = points.Length;
                this.points = new Vector3[pointCount];
                for (int p = 0; p < pointCount; p++)
                    this.points[p] = bPosition + bRotation * points[p];
                //                this.points = points;
                this.col = col;
            }

            public SceneMeshShape(Color col, params Vector3[] points)
            {
                int pointCount = points.Length;
                this.points = new Vector3[pointCount];
                for (int p = 0; p < pointCount; p++)
                    this.points[p] = bPosition + bRotation * points[p];
                //                this.points = points;
                this.col = col;
            }
        }

        private struct SceneMeshLabel
        {
            public Vector3 position;
            public string content;

            public SceneMeshLabel(Vector3 position, string content)
            {
                this.position = position;
                this.content = content;
            }
        }

        private struct SceneMeshDot
        {
            public Vector3 position;
            public float size;
            public Color col;

            public SceneMeshDot(Vector3 position, float size, Color col)
            {
                this.position = bPosition + bRotation * position;
                this.size = size;
                this.col = col;
            }
        }

        private class SceneMeshLayer
        {
            public List<SceneMeshLine> lines = new List<SceneMeshLine>();
            public List<SceneMeshShape> shapes = new List<SceneMeshShape>();
            public List<SceneMeshLabel> labels = new List<SceneMeshLabel>();
            public List<SceneMeshDot> dots = new List<SceneMeshDot>();

            public void Clear()
            {
                lines.Clear();
                shapes.Clear();
                labels.Clear();
                dots.Clear();
            }
        }

        private static List<SceneMeshLayer> LAYERS = new List<SceneMeshLayer>();
        private static Vector3 bPosition;
        private static Quaternion bRotation;

        public static void Clear()
        {
            LAYERS.Clear();
        }

        public static void DrawMesh(Camera cam = null)
        {
            int layerCount = LAYERS.Count;
            GUIStyle gStyle = BuildingEditor.SceneLabel;
            for (int l = 0; l < layerCount; l++)
            {
                SceneMeshLayer layer = LAYERS[l];

                int shapeCount = layer.shapes.Count;
                for (int s = 0; s < shapeCount; s++)
                {
                    Handles.color = layer.shapes[s].col;
                    Handles.DrawAAConvexPolygon(layer.shapes[s].points);
                }

                int lineCount = layer.lines.Count;
                for (int ln = 0; ln < lineCount; ln++)
                {
                    Handles.color = layer.lines[ln].col;
                    Handles.DrawLine(layer.lines[ln].a, layer.lines[ln].b);
                }

                int dotCount = layer.dots.Count;
                for (int d = 0; d < dotCount; d++)
                {
                    Handles.color = layer.dots[d].col;
                    float size = layer.dots[d].size * HandleUtility.GetHandleSize(layer.dots[d].position);
                    UnityVersionWrapper.HandlesDotCap(0, layer.dots[d].position, Quaternion.identity, size);
                }

                int labelCount = layer.labels.Count;
                for (int lb = 0; lb < labelCount; lb++)
                {
                    Vector3 position = layer.labels[lb].position;
                    if (cam != null)
                    {
                        float dot = Vector3.Dot((position - cam.transform.position).normalized, cam.transform.forward);
                        if (dot < 0) continue;
                    }
                    Handles.Label(position, layer.labels[lb].content, gStyle);
                }
            }
        }

        public static void BuildFloorplan()
        {
            LAYERS.Clear();
            SceneMeshLayer layerOpengings = new SceneMeshLayer();
            SceneMeshLayer layer0 = new SceneMeshLayer();
            SceneMeshLayer layer1 = new SceneMeshLayer();
            SceneMeshLayer layer2 = new SceneMeshLayer();
            LAYERS.Add(layerOpengings);
            LAYERS.Add(layer0);
            LAYERS.Add(layer1);
            LAYERS.Add(layer2);

            Building building = BuildingEditor.building;
            bPosition = building.transform.position;
            bRotation = building.transform.rotation;
            BuildRSettings settings = building.settings;
            int numberOfVolumes = building.numberOfPlans;
            Quaternion rotation = building.transform.rotation;

            Vector3[] centerPoints = new Vector3[numberOfVolumes];
            for (int f = 0; f < numberOfVolumes; f++)
                centerPoints[f] = BuildrUtils.CalculateFloorplanCenter(building[f]);
            for (int v = 0; v < numberOfVolumes; v++)
            {
                Volume volume = (Volume)building[v];
                bool isSelectedVolume = BuildingEditor.volume == volume;
                int numberOfPoints = volume.numberOfPoints;
                Vector3 vUp = Vector3.up * volume.floorHeight;
                Dictionary<int, List<Vector2Int>> anchorPoints = volume.facadeWallAnchors;

                IFloorplan[] floorplans = volume.InteriorFloorplans();
                int floorplanCount = floorplans.Length;
                for (int f = 0; f < floorplanCount; f++)//floors
                {
                    IFloorplan floorplan = floorplans[f];
                    bool isSelectedFloorplan = BuildingEditor.floorplan == (Floorplan)floorplan;
                    float intPlanBaseHeight = volume.CalculateFloorHeight(f);
                    Vector3 baseUpV = Vector3.up * intPlanBaseHeight;

                    //draw external outline of selected floor
                    if (numberOfPoints > 0 && isSelectedVolume)
                    {
                        SceneMeshLayer useLayer = isSelectedFloorplan ? layer1 : layer0;
                        List<Vector2Int> planVs = new List<Vector2Int>();
                        Color fillCol = settings.subLineColour;
                        Color lineCol = settings.mainLineColour;
                        for (int p = 0; p < numberOfPoints; p++)
                        {
                            if (volume.IsWallStraight(p))
                            {
                                if (!planVs.Contains(volume[p].position))
                                    planVs.Add(volume[p].position);
                                Vector3 p0 = volume[p].position.vector3XZ + baseUpV;
                                Vector3 p1 = volume[(p + 1) % numberOfPoints].position.vector3XZ + baseUpV;
                                Vector3 p2 = p0 + vUp;
                                Vector3 p3 = p1 + vUp;

                                useLayer.shapes.Add(new SceneMeshShape(fillCol, p0, p1, p3, p2));
                                useLayer.lines.Add(new SceneMeshLine(p2, p3, lineCol));
                                useLayer.lines.Add(new SceneMeshLine(p0, p2, lineCol));
                                useLayer.lines.Add(new SceneMeshLine(p1, p3, lineCol));
                                if(isSelectedFloorplan)
                                {
                                    useLayer.lines.Add(new SceneMeshLine(p0, p1, Color.red));

                                    List<Vector2Int> anchors = anchorPoints[p];
                                    int wallSections = anchors.Count;
                                    for (int w = 0; w < wallSections - 1; w++)
                                    {
                                        Vector3 a = anchors[w].vector3XZ + baseUpV;
                                        float anchorSize = 0.05f;
                                        if (w == 0) anchorSize *= 2;
                                        useLayer.dots.Add(new SceneMeshDot(a, anchorSize, settings.anchorColour));
                                    }
                                }
                            }
                            else
                            {
                                List<Vector2Int> anchors = anchorPoints[p];
                                int wallSections = anchors.Count;
                                for (int w = 0; w < wallSections - 2; w++)
                                {
                                    if (!planVs.Contains(anchors[w]))
                                        planVs.Add(anchors[w]);
                                    Vector3 p0 = anchors[w].vector3XZ + baseUpV;
                                    Vector3 p1 = anchors[w + 1].vector3XZ + baseUpV;
                                    Vector3 p2 = p0 + vUp;
                                    Vector3 p3 = p1 + vUp;

                                    useLayer.lines.Add(new SceneMeshLine(p2, p3, lineCol));
                                    if (w == 0)
                                        useLayer.lines.Add(new SceneMeshLine(p0, p2, lineCol));
                                    if (w == wallSections - 2)
                                        useLayer.lines.Add(new SceneMeshLine(p1, p3, lineCol));

                                    useLayer.shapes.Add(new SceneMeshShape(fillCol, p0, p1, p3, p2));

                                    if(isSelectedFloorplan)
                                    {
                                        useLayer.lines.Add(new SceneMeshLine(p0, p1, Color.red));

                                        float anchorSize = 0.05f;
                                        if (w == 0) anchorSize *= 2;
                                        if (w < wallSections - 1)
                                            useLayer.dots.Add(new SceneMeshDot(p0, anchorSize, settings.anchorColour));
                                    }
                                }
                            }

                        }

                        if(isSelectedFloorplan)
                        {
                            int planVCount = planVs.Count;
                            Vector3[] planV3 = new Vector3[planVCount];
                            for(int pv = 0; pv < planVCount; pv++)
                                planV3[pv] = planVs[pv].vector3XZ + baseUpV;
                            ShapeWithLines(useLayer, planV3, Color.red, new Color(1, 1, 1, 0.9f));
                        }
                    }

                    if (isSelectedFloorplan)
                    {
	                    Room[] rooms = floorplan.AllRooms();
                        int roomCount = rooms.Length;
                        List<Vector2Int> shapePoints = new List<Vector2Int>();
                        for (int r = 0; r < roomCount; r++)
                        {
                            Room room = rooms[r];
                            bool isRoomSelected = room == BuildingEditor.room && BuildingEditor.roomPortal == null && BuildingEditor.opening == null;
                            shapePoints.Clear();

                            FloorplanUtil.RoomWall[] roomWalls = FloorplanUtil.CalculatePoints(room, volume);
                            int roomWallCount = roomWalls.Length;

                            Color wallCol = isRoomSelected ? settings.roomWallSelectedColour : settings.roomWallColour;
                            Color floorCol = isRoomSelected ? settings.roomWallSelectedColour : settings.roomFloorColour;
                            Color mainLineCol = isRoomSelected ? settings.selectedPointColour : settings.mainLineColour;
                            Color subLineCol = isRoomSelected ? settings.selectedPointColour : settings.subLineColour;

                            for (int rwp = 0; rwp < roomWallCount; rwp++)
                            {
                                FloorplanUtil.RoomWall roomWall = roomWalls[rwp];
                                int offsetCount = roomWall.offsetPoints.Length;

                                Vector2Int pi0 = roomWall.baseA;
                                Vector2Int pi1 = roomWall.baseB;

                                if (pi0 == pi1) continue;//not a wall

                                for (int op = 0; op < offsetCount - 1; op++)
                                {
                                    Vector2Int wsint0 = new Vector2Int(roomWall.offsetPoints[op]);
                                    if (!shapePoints.Contains(wsint0))
                                        shapePoints.Add(wsint0);
                                    Vector3 ws0 = new Vector3(roomWall.offsetPoints[op].x, 0, roomWall.offsetPoints[op].y) + baseUpV;

                                    if (isSelectedFloorplan)//draw anchor points
                                    {
                                        float anchorSize = 0.05f;
                                        if (op == 0) anchorSize *= 2;
                                        layer1.dots.Add(new SceneMeshDot(ws0, anchorSize * 0.05f, settings.linkedAnchorColour));
                                    }

                                    int nextIndex = (op + 1) % offsetCount;
                                    Vector3 ws1 = new Vector3(roomWall.offsetPoints[nextIndex].x, 0, roomWall.offsetPoints[nextIndex].y) + baseUpV;
                                    Vector3 ws2 = ws0 + vUp;
                                    Vector3 ws3 = ws1 + vUp;

                                    layer1.lines.Add(new SceneMeshLine(ws0, ws1, mainLineCol));
                                    layer1.lines.Add(new SceneMeshLine(ws0, ws2, subLineCol));
                                    layer1.lines.Add(new SceneMeshLine(ws1, ws3, subLineCol));

                                    layer1.shapes.Add(new SceneMeshShape(wallCol, ws0, ws1, ws3, ws2));
                                }
                            }

                            int shapePointsCount = shapePoints.Count;
                            Vector3[] planV3 = new Vector3[shapePointsCount];
                            for (int pv = 0; pv < shapePointsCount; pv++)
                                planV3[pv] = shapePoints[pv].vector3XZ + baseUpV;
                            ShapeWithLines(layer1, planV3, mainLineCol, floorCol, settings.highlightPerpendicularity, settings.highlightPerpendicularityColour, settings.highlightAngleColour);

                            RoomPortal[] portals = room.GetAllPortals();
                            int portalCount = portals.Length;
                            for (int pt = 0; pt < portalCount; pt++)
                            {
                                RoomPortal portal = portals[pt];
                                bool isSelected = BuildingEditor.roomPortal == portal;
                                Color portalLineColour = isSelectedFloorplan ? settings.mainLineColour : settings.subLineColour;
                                Color portalFillColour = isSelected ? settings.selectedPointColour : settings.mainLineColour;
                                if (!isSelectedFloorplan) portalFillColour = Color.clear;
                                DrawPortal(layer2, rotation, intPlanBaseHeight, volume.floorHeight, room, portal, portalLineColour, portalFillColour);
                            }

                        }

                        for (int r = 0; r < roomCount; r++)
                        {
                            Room room = rooms[r];
                            Vector3 roomCenter = room.center.vector3XZ + baseUpV + Vector3.up * volume.floorHeight;
                            layer1.labels.Add(new SceneMeshLabel(roomCenter, string.Format("Room {0}", (r + 1))));
                        }
                    }

                    //Draw vertical openings
                    if (BuildingEditor.floorplan != null)
                    {
                        VerticalOpening[] openings = building.GetAllOpenings();
                        int openingCount = openings.Length;
                        for (int o = 0; o < openingCount; o++)
                        {
                            VerticalOpening opening = openings[o];
                            bool isSelectedOpening = BuildingEditor.opening == opening;

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
                            Vector3 openingUp = Vector3.up * volume.floorHeight * (opening.floors + 1);

                            //"Phil" Mitchels
                            Color fillCol = settings.subLineColour;
                            fillCol.a = 0.05f;
                            layerOpengings.shapes.Add(new SceneMeshShape(fillCol, p0, p1, p2, p3));
                            layerOpengings.shapes.Add(new SceneMeshShape(fillCol, p0, p1, p1 + openingUp, p0 + openingUp));
                            layerOpengings.shapes.Add(new SceneMeshShape(fillCol, p1, p2, p2 + openingUp, p1 + openingUp));
                            layerOpengings.shapes.Add(new SceneMeshShape(fillCol, p2, p3, p3 + openingUp, p2 + openingUp));
                            layerOpengings.shapes.Add(new SceneMeshShape(fillCol, p3, p0, p0 + openingUp, p3 + openingUp));

                            //lines
                            Color lineCol = settings.invertLineColour;
                            layer0.lines.Add(new SceneMeshLine(p0, p1, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p1, p2, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p2, p3, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p3, p0, lineCol));

                            layer0.lines.Add(new SceneMeshLine(p0 + openingUp, p1 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p1 + openingUp, p2 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p2 + openingUp, p3 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p3 + openingUp, p0 + openingUp, lineCol));

                            layer0.lines.Add(new SceneMeshLine(p0, p0 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p1, p1 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p2, p2 + openingUp, lineCol));
                            layer0.lines.Add(new SceneMeshLine(p3, p3 + openingUp, lineCol));


                            layer0.labels.Add(new SceneMeshLabel(openingPosition + openingUp, string.Format("Opening {0}", o + 1)));

                            if (volume == BuildingEditor.volume && BuildingEditor.floorplan != null)
                            {
                                Vector3 floorUpA = Vector3.up * volume.CalculateFloorHeight(volume.Floor(BuildingEditor.floorplan));
                                Vector3 floorUpB = floorUpA + Vector3.up * volume.floorHeight;

                                Color col = isSelectedOpening ? Color.green : Color.red;
                                SceneMeshLayer useLayer = isSelectedOpening ? layer2 : layer2;

                                useLayer.lines.Add(new SceneMeshLine(p0 + floorUpA, p1 + floorUpA, col));
                                useLayer.lines.Add(new SceneMeshLine(p1 + floorUpA, p2 + floorUpA, col));
                                useLayer.lines.Add(new SceneMeshLine(p2 + floorUpA, p3 + floorUpA, col));
                                useLayer.lines.Add(new SceneMeshLine(p3 + floorUpA, p0 + floorUpA, col));

                                useLayer.lines.Add(new SceneMeshLine(p0 + floorUpB, p1 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p1 + floorUpB, p2 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p2 + floorUpB, p3 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p3 + floorUpB, p0 + floorUpB, col));

                                useLayer.lines.Add(new SceneMeshLine(p0 + floorUpA, p0 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p1 + floorUpA, p1 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p2 + floorUpA, p2 + floorUpB, col));
                                useLayer.lines.Add(new SceneMeshLine(p3 + floorUpA, p3 + floorUpB, col));
                            }
                        }
                    }
                }
            }
        }

        private static void DrawPortal(SceneMeshLayer layer, Quaternion rotation, float baseHeight, float floorHeight, Room room, RoomPortal portal, Color lineColour, Color fillColor)
        {
            int wallIndex = portal.wallIndex;
            Vector3 p0 = room[wallIndex].position.vector3XZ;
            Vector3 p1 = room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            Vector3 baseUp = Vector3.up * (floorHeight - portal.height) * portal.verticalPosition;
            Vector3 portalUp = baseUp + Vector3.up * portal.height;
            Vector3 pointPos = PortalPosition(Quaternion.identity, room, portal);
            pointPos.y = baseHeight;
            Vector3 wallDirection = (p1 - p0).normalized;
            Vector3 portalCross = Vector3.Cross(Vector3.down, wallDirection);
            float defaultWidth = portal.width * 0.5f;
            float defaultDepth = 0.1f;

            Vector3 v0 = pointPos + wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v1 = pointPos + wallDirection * defaultWidth - portalCross * defaultDepth;
            Vector3 v2 = pointPos - wallDirection * defaultWidth + portalCross * defaultDepth;
            Vector3 v3 = pointPos - wallDirection * defaultWidth - portalCross * defaultDepth;

            layer.lines.Add(new SceneMeshLine(v0 + baseUp, v1 + baseUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v1 + baseUp, v3 + baseUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v3 + baseUp, v2 + baseUp, lineColour));

            layer.lines.Add(new SceneMeshLine(v2 + baseUp, v2 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v2 + portalUp, v0 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v0 + portalUp, v0 + baseUp, lineColour));

            layer.lines.Add(new SceneMeshLine(v3 + baseUp, v3 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v3 + portalUp, v1 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v1 + portalUp, v1 + baseUp, lineColour));

            layer.lines.Add(new SceneMeshLine(v0 + portalUp, v1 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v1 + portalUp, v3 + portalUp, lineColour));
            layer.lines.Add(new SceneMeshLine(v3 + portalUp, v2 + portalUp, lineColour));

            layer.shapes.Add(new SceneMeshShape(fillColor, v0 + baseUp, v1 + baseUp, v3 + baseUp, v2 + baseUp));
            layer.shapes.Add(new SceneMeshShape(fillColor, v0 + baseUp, v1 + baseUp, v1 + portalUp, v0 + portalUp));
            layer.shapes.Add(new SceneMeshShape(fillColor, v2 + baseUp, v3 + baseUp, v3 + portalUp, v2 + portalUp));
            layer.shapes.Add(new SceneMeshShape(fillColor, v0 + portalUp, v1 + portalUp, v3 + portalUp, v2 + portalUp));
        }

        public static Vector3 PortalPosition(Quaternion rotation, Room room, RoomPortal portal)
        {
            int wallIndex = portal.wallIndex;
            if (wallIndex == -1)
                return Vector3.zero;
            Vector3 p0 = rotation * room[wallIndex % room.numberOfPoints].position.vector3XZ;
            Vector3 p1 = rotation * room[(wallIndex + 1) % room.numberOfPoints].position.vector3XZ;
            return Vector3.Lerp(p0, p1, portal.lateralPosition);
        }

        private static void ShapeWithLines(SceneMeshLayer layer, Vector3[] points, Color lineColor, Color fillColor, bool highlight, Color highlightcolour, Color angleColour)
        {
            //            Handles.color = fillColor;
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
                //                Handles.DrawAAConvexPolygon(f0, f1, f2);
                layer.shapes.Add(new SceneMeshShape(fillColor, f0, f1, f2));
            }

            //            Handles.color = lineColor;
            Color useColour = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                if (highlight)
                {
                    Vector3 diff = p1 - p0;
                    if (diff.x * diff.x < 0.001f || diff.z * diff.z < 0.001f)
                        useColour = highlightcolour;
                    else
                        useColour = angleColour;
                }
                //                Handles.DrawLine(p0, p1);
                layer.lines.Add(new SceneMeshLine(p0, p1, useColour));
            }
        }

        private static void ShapeWithLines(SceneMeshLayer layer, Vector3[] points, Color lineColor, Color fillColor)
        {
            //            Handles.color = fillColor;
            //            int[] tris = EarClipper.Triangulate(points);
            int[] tris = Poly2TriWrapper.Triangulate(points);
            int triCount = tris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 f0 = points[tris[t]];
                Vector3 f1 = points[tris[t + 1]];
                Vector3 f2 = points[tris[t + 2]];
                //                Handles.DrawAAConvexPolygon(f0, f1, f2);
                layer.shapes.Add(new SceneMeshShape(fillColor, f0, f1, f2));
            }

            //            Handles.color = lineColor;
            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 p0 = points[p];
                Vector3 p1 = points[(p + 1) % pointCount];
                //                Handles.DrawLine(p0, p1);
                layer.lines.Add(new SceneMeshLine(p0, p1, lineColor));
            }
        }
    }
}