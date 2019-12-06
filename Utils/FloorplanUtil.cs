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
    public class FloorplanUtil
    {
        public static RoomWall CalculateNewWall(IVolume volume, Vector2Int startPoint, Vector2Int endPoint)
        {
            RoomWall output = new RoomWall();

            output.baseA = startPoint;
            output.baseB = endPoint;
            output.offsetPoints = new Vector2[0];
            output.offsetPointsInt = new Vector2Int[0];

            if (startPoint == endPoint)
            {
                return output;//not a wall mate
            }

            Dictionary<int, List<Vector2Int>> facadeWallAnchors = volume.facadeWallAnchors;
            List<Vector2Int> volumeWallAnchors = volume.wallAnchors;

            int start = volumeWallAnchors.IndexOf(startPoint);
            int end = volumeWallAnchors.IndexOf(endPoint);
            
            output.startVolumePointIndex = start;
            output.endVolumePointIndex = end;

            if (start == -1 || end == -1)//fully internal wall. two points need to represent
            {
                output.isExternal = false;
                output.offsetPointsInt = new Vector2Int[2];
                output.offsetPointsInt[0] = startPoint;
                output.offsetPointsInt[1] = endPoint;

                output.offsetPoints = new Vector2[2];
                output.offsetPoints[0] = startPoint.vector2;
                output.offsetPoints[1] = endPoint.vector2;
            }
            else
            {
                int index = start;
                int size = volumeWallAnchors.Count;
                int its = 0;

                int facadeCount = facadeWallAnchors.Count;
                int facadeIndex = -1;
                for (int f = 0; f < facadeCount; f++)
                {
                    if(facadeWallAnchors[f].Contains(startPoint) && facadeWallAnchors[f].Contains(endPoint))
                    {
                        facadeIndex = f;
                        break;//the exist on a single facade
                    }
                }

                if (facadeIndex == -1)//internal wall as points do not exist on same facade
                {
                    output.isExternal = false;
                    output.offsetPointsInt = new Vector2Int[2];
                    output.offsetPointsInt[0] = startPoint;
                    output.offsetPointsInt[1] = endPoint;
                    output.offsetPoints = new Vector2[2];
                    output.offsetPoints[0] = startPoint.vector2;
                    output.offsetPoints[1] = endPoint.vector2;
                }
                else
                {
                    output.isExternal = true;
                    output.facadeIndex = facadeIndex;
                    int direction = start < end ? 1 : -1;
                    int wallAnchorDiff = Mathf.Abs(start - end);
                    if(wallAnchorDiff > size / 2)
                        direction = -direction;
                    List<Vector2Int> wallPointsInt = new List<Vector2Int>();
                    List<Vector2> wallPoints = new List<Vector2>();
                    while (true)//count around the volume anchors to get the complete list of anchor points
                    {
                        if(wallPointsInt.Count > 0 && volumeWallAnchors[index] == wallPointsInt[wallPointsInt.Count - 1])
                        {
                            //skip it - this wall point has already been registered on this wall
                        }
                        else
                        {
                            wallPointsInt.Add(volumeWallAnchors[index]);
                            wallPoints.Add(volumeWallAnchors[index].vector2);
                        }
                        //                        Debug.Log(index);
                        index += direction;
                        if (index >= size) index = 0;
                        if (index < 0) index = size - 1;
                        if (index == end)
                        {
                            wallPointsInt.Add(volumeWallAnchors[index]);//last anchor point
                            wallPoints.Add(volumeWallAnchors[index].vector2);//last anchor point
                            break;
                        }
                        its++;
                        if (its > size) break;//only go around once...
                    }

                    output.offsetPointsInt = wallPointsInt.ToArray();
                    output.offsetPoints = wallPoints.ToArray();
                }
            }

            return output;
        }
        
        //TODO need to optimise
        //use of list and "index of" killing it
        public static RoomWall[] CalculatePoints(Room room, IVolume volume, bool debug = false)
        {
            Dictionary<int, List<Vector2Int>> facadeWallAnchors = volume.facadeWallAnchors;
//            List<Vector2Int> volumeWallAnchors = volume.wallAnchors;

            int pointCount = room.numberOfPoints;
            RoomWall[] output = new RoomWall[pointCount];
            
            for (int p = 0; p < pointCount; p++)
            {
                Vector2Int startPoint = room[p].position;
                int pb = (p + 1) % pointCount;
                Vector2Int endPoint = room[pb].position;
                RoomWall newWall = CalculateNewWall(volume, startPoint, endPoint);

//                int numberOfFacades = facadeWallAnchors.Count;

                int size = newWall.offsetPoints.Length;
                newWall.offsetPointWallSection = new int[size];
                newWall.anchorPointIndicies = new int[size];
                int currentFacade = newWall.facadeIndex;
//	            int facadePointCount = facadeWallAnchors[currentFacade].Count;
//				int wallSectionIndex = facadePointCount - facadeWallAnchors[currentFacade].IndexOf(newWall.offsetPointsInt[0]);

				for (int s = 0; s < size; s++)
                {
					//minus one as we're on the other side of the wall - the wall point index will be the wrong point - need to use the other section point
	                newWall.offsetPointWallSection[s] = facadeWallAnchors[currentFacade].IndexOf(newWall.offsetPointsInt[s]);
//	                Debug.Log(currentFacade+" "+s+" "+ newWall.offsetPointsInt[s]);
					//                    newWall.anchorPointIndicies[s] = volumeWallAnchors.IndexOf(newWall.offsetPointsInt[s]);

					//                    if (facadeWallAnchors[currentFacade].IndexOf(newWall.offsetPointsInt[s]) == facadeWallAnchors[currentFacade].Count - 1)
					//                        currentFacade = (currentFacade + 1) % numberOfFacades;
					//                    int it = numberOfFacades;
					//                    while (!facadeWallAnchors[currentFacade].Contains(newWall.offsetPointsInt[s]))//find the facade that this anchor point is part of
					//                    {
					//                        currentFacade = (currentFacade + 1) % numberOfFacades;
					//                        it--;
					//                        if (it < 0) break;
					//                    }
					//                    newWall.offsetPointWallSection[s] = facadeWallAnchors[currentFacade].IndexOf(newWall.offsetPointsInt[s]);
//					newWall.offsetPointWallSection[s] = wallSectionIndex - s;
//	                while(newWall.offsetPointWallSection[s] < 0) newWall.offsetPointWallSection[s] += facadePointCount;

                }

                output[p] = newWall;
            }
            return output;
        }

        public static bool[] CalculateExternalWall(Room room, Volume volume, bool debug = false)
        {
            int pointCount = room.numberOfPoints;
            bool[] output = new bool[pointCount];
            List<Vector2Int> volumeWallAnchors = volume.wallAnchors;

            for (int p = 0; p < pointCount; p++)
            {
                Vector2Int startPoint = room[p].position;
                int pb = (p + 1) % pointCount;
                Vector2Int endPoint = room[pb].position;
				bool bothBasePointsExternal = volumeWallAnchors.Contains(startPoint) && volumeWallAnchors.Contains(endPoint);
	            if(bothBasePointsExternal)
	            {
		            List<int> facadesA = volume.externalFacadeWallAnchors[startPoint];
					List<int> facadesB = volume.externalFacadeWallAnchors[endPoint];
		            int match = CompareLists(facadesA, facadesB);
					if (match != -1)
		            {
			            if(volume[match].IsWallStraight())
				            output[p] = true;
		            }
	            }
            }
            return output;
        }

	    private static int CompareLists(List<int> la, List<int> lb)
	    {
		    int output = -1;
		    int ca = la.Count;
		    int cb = lb.Count;
		    for(int a = 0; a < ca; a++)
			    for(int b = 0; b < cb; b++)
				    if(la[a] == lb[b]) return a;
		    return output;
	    }

        public static Vector2[] RoomArchorPoints(RoomWall[] walls)
        {
            int wallCount = walls.Length;
//            List<Vector2Int> intList = new List<Vector2Int>();
            List<Vector2> output = new List<Vector2>();
            for (int w = 0; w < wallCount; w++)
            {
                RoomWall wall = walls[w];
                int pointCount = wall.offsetPointsInt.Length - 1;//skip last point to avoid repetition
                for (int p = 0; p < pointCount; p++)
                {
//                    if(!intList.Contains(wall.offsetPointsInt[p]))
//                    {
                        output.Add(wall.offsetPoints[p]);
//                        intList.Add(wall.offsetPointsInt[p]);
//                    }
                }
            }
            
            return output.ToArray();
        }

        [Serializable]
        public struct RoomWall
        {
            public Vector2Int baseA;//initial wall point
            public Vector2Int baseB;//final wall point
            public int startVolumePointIndex;//the index of the first point against the volume wall anchors
            public int endVolumePointIndex;//the index of the first point against the volume wall anchors
            public Vector2Int[] offsetPointsInt;//all wall points including external wall anchors
            public Vector2[] offsetPoints;//all wall points including external wall anchors
            public int[] anchorPointIndicies;//all wall points including external wall anchors
            public int facadeIndex;
            public int[] offsetPointWallSection;
            public bool isExternal;//does this wall follow an external wall
            public Types type;//might not use this

            public enum Types
            {
                Internal,
                External,
                Cut
            }
        }

        private static bool Clockwise(Vector2Int[] poly)
        {
            int polySize = poly.Length;
            float polyEdgeSum = 0;
            for (int p = 0; p < polySize; p++)
            {
                Vector2Int p0 = poly[p];
                Vector2Int p1 = poly[(p + 1) % polySize];
                polyEdgeSum += (p1.x + p0.x) * (p1.y + p0.y);
            }
            return polyEdgeSum >= 0;
        }

	    public static void FillFloorWithSingleRoom(IBuilding building, IVolume volume, IFloorplan floorplan)
		{
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject((Floorplan)floorplan, "Room Creation");
#endif
			floorplan.ClearRooms();

//			for(int i = 0; i < volume.AllBuildingPoints().Length; i++)
//			{
//				Debug.Log(volume.AllBuildingPoints()[i].ToString());
//			}
			Room floorRoom = new Room(volume.AllBuildingPoints(), Vector3.zero);
			floorplan.AddRoom(floorRoom);
		    floorplan.MarkModified();
		    building.MarkModified();
		}
    }
}