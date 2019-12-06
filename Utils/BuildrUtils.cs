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
using JaspLib;
using UnityEngine;

namespace BuildR2
{
    public class BuildrUtils
    {

        public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 v1 = point - a;
            Vector3 v2 = (b - a).normalized;
            float distance = Vector3.Distance(a, b);
            float t = Vector3.Dot(v2, v1);

            if(t <= 0)
                return a;
            if(t >= distance)
                return b;
            Vector3 v3 = v2 * t;
            Vector3 closestPoint = a + v3;
            return closestPoint;
        }

        public static Vector2 ClosestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 v1 = point - a;
            Vector2 v2 = (b - a).normalized;
            float distance = Vector2.Distance(a, b);
            float t = Vector2.Dot(v2, v1);

            if(t <= 0)
                return a;
            if(t >= distance)
                return b;
            Vector2 v3 = v2 * t;
            Vector2 closestPoint = a + v3;
            return closestPoint;
        }

        public static bool FastLineIntersection(Vector2Int a1, Vector2Int a2, Vector2Int b1, Vector2Int b2)
        {
            if(a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if(a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return false;
            return (CCW(a1, b1, b2) != CCW(a2, b1, b2)) && (CCW(a1, a2, b1) != CCW(a1, a2, b2));
        }

        private static bool CCW(Vector2Int p1, Vector2Int p2, Vector2Int p3)
        {
            return ((p2.vx - p1.vx) * (p3.vy - p1.vy) > (p2.vy - p1.vy) * (p3.vx - p1.vx));
        }

        private static bool CCW(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }

        public static Vector2 FindIntersection(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
        {
            // if abs(angle)==1 then the lines are parallel,  
            // so no intersection is possible  
            if(Mathf.Abs(Vector2.Dot(lineA, lineB)) == 1.0f) return Vector2.zero;

            Vector2 intersectionPoint = IntersectionPoint(lineA, originA, lineB, originB);

            if(float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            {
                //flip the second line to find the intersection point
                intersectionPoint = IntersectionPoint(lineA, originA, -lineB, originB);
            }

            if(float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            {
                //            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
                intersectionPoint = originA + lineA;
            }

            return intersectionPoint;
        }

        private static Vector2 IntersectionPoint(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
        {

            float xD1, yD1, xD2, yD2, xD3, yD3;
            float ua, div;

            // calculate differences  
            xD1 = lineA.x;
            xD2 = lineB.x;
            yD1 = lineA.y;
            yD2 = lineB.y;
            xD3 = originA.x - originB.x;
            yD3 = originA.y - originB.y;

            // find intersection Pt between two lines    
            Vector2 pt = new Vector2(0, 0);
            div = yD2 * xD1 - xD2 * yD1;
            ua = (xD2 * yD3 - yD2 * xD3) / div;
            pt.x = originA.x + ua * xD1;
            pt.y = originA.y + ua * yD1;

            // return the valid intersection  
            return pt;
        }

        private static Vector2 IntersectionPoint2(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
        {

            Vector2 lineA2 = lineA + originA;
            Vector2 lineB2 = lineB + originB;

            Vector3 crossA = Vector3.Cross(new Vector3(lineA.x, lineA.y, 1), new Vector3(lineA2.x, lineA2.y, 1));
            Vector3 crossB = Vector3.Cross(new Vector3(lineB.x, lineB.y, 1), new Vector3(lineB2.x, lineB2.y, 1));
            Vector3 crossAB = Vector3.Cross(crossA, crossB);

            Vector2 pt = new Vector2(0, 0);
            pt.x = crossAB.x / crossAB.z;
            pt.x = crossAB.y / crossAB.z;

            // return the valid intersection  
            return pt;
        }

        public static Vector2Int FindIntersection(Vector2Int lineA, Vector2Int originA, Vector2Int lineB, Vector2Int originB)
        {
            Vector2 returnPoint = FindIntersection(lineA.vector2, originA.vector2, lineB.vector2, originB.vector2);
            return new Vector2Int(returnPoint, false);
        }

        public static bool PointInsidePoly(Vector2Int point, Vector2Int[] poly)
        {
            FlatBounds bounds = new FlatBounds();
            foreach(Vector2Int polyPoint in poly)
                bounds.Encapsulate(polyPoint.vector2);
            if(!bounds.Contains(point.vector2))
                return false;

            Vector2Int pointRight = point + new Vector2Int(bounds.width, 0);

            int numberOfPolyPoints = poly.Length;
            int numberOfCrossOvers = 0;
            for(int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2Int p0 = poly[i];
                Vector2Int p1 = poly[(i + 1) % numberOfPolyPoints];
                if(FastLineIntersection(point, pointRight, p0, p1))
                    numberOfCrossOvers++;
            }
            //            if(numberOfCrossOvers % 2 != 0) bounds.DrawDebug(Color.green);

            return numberOfCrossOvers % 2 != 0;
        }

        public static bool SelfIntersectingPoly(Vector2[] poly)
        {
            int numberOfPolyPoints = poly.Length;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[i < numberOfPolyPoints - 1 ? (i + 1) : 0];

                for(int j = 0; j < numberOfPolyPoints; j++)
                {
                    Vector2 p2 = poly[j];
                    Vector2 p3 = poly[j < numberOfPolyPoints - 1 ? (j + 1) : 0];

                    if(FastLineIntersection(p0, p1, p2, p3))
                        return true;
                }

            }
            return false;
        }

        public static FloorplanClick OnFloorplanSelectionClick(Building building, Ray mouseRay, bool includeInterior = false)
        {
            FloorplanClick output = new FloorplanClick();
            float nearestPointDistance = Mathf.Infinity;
            Vector3 basePosition = building.transform.position;
            Quaternion baseRotation = Quaternion.Inverse(building.transform.rotation);
            int floorplanCount = building.numberOfPlans;
            for(int f = 0; f < floorplanCount; f++)
            {
                Volume volume = building[f] as Volume;
                float baseHeight = volume.baseHeight;
                Vector3 testPoint = basePosition + Vector3.up * baseHeight;
                if(Vector3.Dot(mouseRay.direction, testPoint - mouseRay.origin) < 0)//building 
                    continue;

                Plane planPlane = new Plane(Vector3.up, testPoint);
                float rayDistance = 0;
                if(planPlane.Raycast(mouseRay, out rayDistance))
                {
                    if(rayDistance < nearestPointDistance)
                    {
                        Vector3 clickPos = baseRotation * (mouseRay.GetPoint(rayDistance) - basePosition);
                        Vector2Int planPos = new Vector2Int(clickPos, true);
                        Vector2Int[] planPoints = volume.AllPoints();
                        if(PointInsidePoly(planPos, planPoints))
                        {
                            nearestPointDistance = rayDistance;
                            output.volume = volume;
                        }

                        if(includeInterior)
                        {
                            IFloorplan[] intFloorplans = volume.InteriorFloorplans();
                            int floors = intFloorplans.Length;
                            for(int fl = 0; fl < floors; fl++)
                            {
                                Floorplan intFl = intFloorplans[fl] as Floorplan;
	                            Room[] rooms = intFl.AllRooms();
                                int roomCount = rooms.Length;
                                for(int rm = 0; rm < roomCount; rm++)
                                {
                                    Vector2Int[] roomPoints = rooms[rm].AllPoints();
                                    if(PointInsidePoly(planPos, roomPoints))
                                    {
                                        nearestPointDistance = rayDistance;
                                        output.volume = volume;
                                        output.floorplan = intFl;
                                        output.room = rooms[rm];
                                    }
                                }
                            }
                        }
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
                if(volume == null)
                    return "none";
                return string.Format("{0} {1} {2} {3} {4}", ((Object)volume).name, volume.Floor(floorplan), room, portal, opening);
            }
        }

        /// <summary>
        /// Method to compute the centroid of a polygon. This does NOT work for a complex polygon.
        /// </summary>
        /// <param name="poly">points that define the polygon</param>
        /// <returns>centroid point, or PointF.Empty if something wrong</returns>
        public static Vector2Int GetCentroid(Vector2Int[] poly)
        {
            int polyLength = poly.Length;
            if(polyLength == 0) return Vector2Int.zero;

            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for(int i = 0, j = polyLength - 1; i < poly.Length; j = i++)
            {
                float temp = poly[i].vx * poly[j].vy - poly[j].vx * poly[i].vy;
                accumulatedArea += temp;
                centerX += (poly[i].vx + poly[j].vx) * temp;
                centerY += (poly[i].vy + poly[j].vy) * temp;
            }

            if(accumulatedArea < Mathf.Epsilon)
                return poly[0];// Avoid division by zero

            accumulatedArea *= 3f;
            return new Vector2Int(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        /// <summary>
        /// Method to compute the centroid of a polygon. This does NOT work for a complex polygon.
        /// </summary>
        /// <param name="poly">points that define the polygon</param>
        /// <returns>centroid point, or PointF.Empty if something wrong</returns>
        public static Vector2Int GetCentroid(IVolume plan)
        {
            return new Vector2Int(plan.bounds.center.x, plan.bounds.center.z);
        }



        public static List<IVolume> GetLinkablePlans(IBuilding building, IVolume plan)
        {
            List<IVolume> output = new List<IVolume>(building.AllPlans());
            int planCount = building.numberOfPlans;
            for(int p = 0; p < planCount; p++)
            {
                if(building[p] == plan) continue;//cannot link to oneself

                if(building[p].ContainsPlanAbove(plan))
                    return building[p].AbovePlanList();

                int aPlans = building[p].abovePlanCount;
                for(int ap = 0; ap < aPlans; ap++)
                    output.Remove(building[p].AbovePlanList()[ap]);

                if(plan.IsLinkedPlan(building[p]))
                    output.Remove(building[p]);
            }

            return output;//return base plans
        }

        public static Vector3 CalculateFloorplanCenter(IVolume plan)
        {
            Vector2Int center = GetCentroid(plan);
            Vector2[] points = ConvertPoints(plan.AllPoints());
            Vector3 planUp = plan.baseHeight * Vector3.up;
            if(JMath.PointInsidePoly(center.vector2, points))
                return center.vector3XZ + planUp;

            int pointCount = points.Length;
            if(pointCount == 0)
                return Vector3.zero;
            float[] dists = new float[pointCount];
            float dist = Mathf.Infinity;
            int index = -1;
            for(int p = 0; p < pointCount; p++)
            {
                float currentDist = Vector2.Distance(center.vector2, points[p]);
                if(currentDist < dist)
                {
                    index = p;
                    dist = currentDist;
                }
                dists[p] = currentDist;
            }

            Vector2 pa = points[index];
            int indexA = (index - 1 + pointCount) % pointCount;
            int indexB = (index + 1) % pointCount;

            Vector2 pb = (dists[indexA] < dists[indexB]) ? points[indexA] : points[indexB];
            Vector2 pc = Vector2.Lerp(pa, pb, 0.5f);
            float cDist = Vector2.Distance(center.vector2, pc);
            Vector2 dir = (pc - center.vector2).normalized;
            Vector2 newCenter = pc + dir * cDist * 0.5f;

            if(JMath.PointInsidePoly(newCenter, points))
                return new Vector3(newCenter.x, planUp.y, newCenter.y);

            return center.vector3XZ + planUp;
        }

        public static Vector2[] ConvertPoints(Vector2Int[] input)
        {
            int count = input.Length;
            Vector2[] output = new Vector2[count];
            for(int i = 0; i < count; i++)
                output[i] = input[i].vector2;
            return output;
        }

        public static float CalculateFacadeAngle(Vector2 facadeDirection)
        {
            return JMath.SignAngle(new Vector2(facadeDirection.x, facadeDirection.y).normalized) + 90;
        }

        public static float CalculateFacadeAngle(Vector3 facadeDirection)
        {
            return JMath.SignAngle(new Vector2(facadeDirection.x, facadeDirection.z).normalized) + 90;
        }

        public static VerticalOpening[] GetOpenings(Building building, Volume volume)
        {
            List<VerticalOpening> output = new List<VerticalOpening>();
            int count = building.openingCount;
            VerticalOpening[] openings = building.GetAllOpenings();

            int volumeBaseFloor = building.VolumeBaseFloor(volume);
            int volumeTopFloor = volumeBaseFloor + volume.floors;

            for(int i = 0; i < count; i++)
            {
                VerticalOpening opening = openings[i];
                int openingBaseFloor = opening.baseFloor;
                int openingTopFloor = openingBaseFloor + opening.floors - 1;

                if(volumeBaseFloor < openingTopFloor && openingBaseFloor < volumeTopFloor)//opening and volume floors intersect
                {
                    FlatBounds volumeBounds = new FlatBounds(volume.bounds);
                    Vector2[] openingPoints = opening.PointsRotated();
                    FlatBounds openingBounds = new FlatBounds(openingPoints);

                    if(volumeBounds.Overlaps(openingBounds, true))// opening is within the AABB bounds of the volume
                    {
                        Vector2[] volumePoints = volume.AllPointsV2();
                        int volumePointCount = volumePoints.Length;
                        int openingPointCount = openingPoints.Length;
                        bool openingIntersects = false;

                        for(int op = 0; op < openingPointCount; op++)
                        {
                            Vector2 opa = openingPoints[op];
                            Vector2 opb = openingPoints[(op + 1) % openingPointCount];

                            for(int vp = 0; vp < volumePointCount; vp++)
                            {
                                Vector2 vpa = volumePoints[vp];
                                Vector2 vpb = volumePoints[(vp + 1) % volumePointCount];

                                if(FastLineIntersection(opa, opb, vpa, vpb))
                                    openingIntersects = true;

                                if(openingIntersects)
                                    break;
                            }

                            if(openingIntersects)
                                break;
                        }

                        if(!openingIntersects)//check that the opening is within the volume shape
                        {
                            Vector2 opa = openingPoints[0];
                            Vector2 opb = openingPoints[1];
                            int intersections = 0;//we should intersect an odd number of times
                            for(int vp = 0; vp < volumePointCount; vp++)
                            {
                                Vector2 vpa = volumePoints[vp];
                                Vector2 vpb = volumePoints[(vp + 1) % volumePointCount];

                                if(FastLineIntersection(opa, opb, vpa, vpb))
                                    intersections++;
                            }

                            if(intersections % 2 != 0)
                                output.Add(opening);
                        }
                    }
                }
            }
            return output.ToArray();
        }

        /// <summary>
        /// This one ignores finding openings within specific shapes as we'll be doing this for rooms in floorplan gen
        /// </summary>
        /// <param name="building"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static VerticalOpening[] GetOpeningsQuick(IBuilding building, IVolume volume)
        {
            List<VerticalOpening> output = new List<VerticalOpening>();
            int count = building.openingCount;
            VerticalOpening[] openings = building.GetAllOpenings();

            int volumeBaseFloor = building.VolumeBaseFloor(volume);
            int volumeTopFloor = volumeBaseFloor + volume.floors;

            for(int i = 0; i < count; i++)
            {
                VerticalOpening opening = openings[i];
                int openingBaseFloor = opening.baseFloor;
                int openingTopFloor = openingBaseFloor + opening.floors;

//                Debug.Log("CHECK IT " + openingTopFloor + " " + volume.name);
//                Debug.Log(volumeBaseFloor + " < " + openingTopFloor + " && " + openingBaseFloor + " < " + volumeTopFloor);

                if(volumeBaseFloor <= openingTopFloor && openingBaseFloor < volumeTopFloor)//opening and volume floors intersect
                {
//                    Debug.Log("opening " + openingTopFloor + " "+ volume.name);
                    FlatBounds volumeBounds = new FlatBounds(volume.bounds);
                    volumeBounds.Expand(VerticalOpening.WALL_THICKNESS);
                    Vector2[] openingPoints = opening.PointsRotated();
                    FlatBounds openingBounds = new FlatBounds(openingPoints);


//                    if(volume.name == "Roof Exit")
//                    {
//                        Debug.Log(volumeBounds.Overlaps(openingBounds, true));
//                        Debug.Log(openingBounds.Overlaps(volumeBounds, true));
//                        volumeBounds.DrawDebug(Color.red);
//                        openingBounds.DrawDebug(Color.green);
//                    }

                    if(openingBounds.Overlaps(volumeBounds, true))// opening is within the AABB bounds of the volume
                    {
                        output.Add(opening);
//                        Debug.Log("opening bounds " + openingTopFloor + " " + volume.name);
                    }
                    else
                    {
//                        Debug.Log("opening NOT " + openingTopFloor + " " + volume.name);
                    }
                }
//                Debug.Log("=================================");
            }
            return output.ToArray();
        }

        public static bool PointOnLine(Vector2Int p, Vector2Int a, Vector2Int b)
        {
            float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
            if(Mathf.Abs(cross) > Mathf.Epsilon) return false;
            float dot = (p.x - a.x) * (b.x - a.x) + (p.y - a.y) * (b.y - a.y);
            if(dot < 0) return false;
            float squaredlengthba = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
            if(dot > squaredlengthba) return false;
            return true;
        }

        public static bool Colinear(Vector2Int pa, Vector2Int pb, Vector2Int pc)
        {
            double detleft = (pa.x - pc.x) * (pb.y - pc.y);
            double detright = (pa.y - pc.y) * (pb.x - pc.x);
            double val = detleft - detright;
            return (val > -Mathf.Epsilon && val < Mathf.Epsilon);
        }

        public static int[] SortPointByAngle(Vector2Int[] points, Vector2Int center) {
            int pointCount = points.Length;
            int[] output = new int[pointCount];
            float[] angles = new float[pointCount];

            for (int a = 0; a < pointCount; a++) {
                angles[a] = SignAngle(points[a] - center);
            }

            bool[] used = new bool[pointCount];
            for (int a = 0; a < pointCount; a++) {
                float lowestAngle = 180;
                int index = -1;
                for (int ax = 0; ax < pointCount; ax++) {
                    if (used[ax]) continue;
                    if (angles[ax] <= lowestAngle) {
                        lowestAngle = angles[ax];
                        index = ax;
                    }
                }
                if (index != -1) {
                    output[a] = index;
                    used[index] = true;
                }
            }

            return output;
        }

        public static int[] SortPointByAngle(Vector2[] points, Vector2 center) {
            int pointCount = points.Length;
            int[] output = new int[pointCount];
            float[] angles = new float[pointCount];

            for (int a = 0; a < pointCount; a++) {
                angles[a] = SignAngle(points[a] - center);
            }

            bool[] used = new bool[pointCount];
            for (int a = 0; a < pointCount; a++) {
                float lowestAngle = 180;
                int index = -1;
                for (int ax = 0; ax < pointCount; ax++) {
                    if (used[ax]) continue;
                    if (angles[ax] <= lowestAngle) {
                        lowestAngle = angles[ax];
                        index = ax;
                    }
                }
                if (index != -1) {
                    output[a] = index;
                    used[index] = true;
                }
            }

            return output;
        }

        public static float SignAngle(Vector2Int dir) {
            float angle = Vector2Int.Angle(Vector2Int.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.up, dir.vector3XY);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }

        public static float SignAngle(Vector2 dir) {
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.up, dir);
            if (cross.z > 0)
                angle = -angle;
            return angle;
        }
    }
}