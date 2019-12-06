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

namespace BuildR2
{
	[RequireComponent(typeof(BuildRPart))]
    public class Floorplan : MonoBehaviour, IFloorplan
	{
        public List<Room> rooms = new List<Room>();
		
        public struct WallIssueItem
        {
            public Room inRoom;
            public int wallIndex;
            public Vector2Int point;
        }
        public List<WallIssueItem> issueList = new List<WallIssueItem>();

        [SerializeField]
        private VisualPart _visualPart;

        public IVisualPart visualPart { get { return _visualPart; } }


        private bool _isModified;

        public void SetDefaultName(int floor)
        {
            name = string.Format("Floorplan ( floor {0} )", floor);
		}

		public void Clear()
		{
			throw new System.NotImplementedException();
		}

		public bool isModified
        {
            get
            {
                if (_isModified) return true;
                int numberOfRooms = rooms.Count;
                for (int r = 0; r < numberOfRooms; r++)
                    if (rooms[r].isModified) return true;
                return false;
            }
        }

        public void MarkModified()
        {
            if (!BuildRSettings.AUTO_UPDATE) return;
            Serialise();
            CheckWallIssues();
            _isModified = true;
        }

	    public void CalculatePlanWalls(Volume volume)
	    {
	        foreach(Room room in rooms)
	            room.CalculatePlanWalls(volume);
	    }
        
	    public void MarkUnmodified()
        {
            _isModified = false;
            int numberOfRooms = rooms.Count;
            for (int r = 0; r < numberOfRooms; r++)
                rooms[r].MarkUnmodified();
        }

        public List<RoomPortal> GetWallPortals(Room currentRoom, int wallIndex)
        {
            List<RoomPortal> output = new List<RoomPortal>();

            Vector2Int a = currentRoom[wallIndex].position;
            Vector2Int b = currentRoom[(wallIndex + 1) % currentRoom.numberOfPoints].position;

            int portalCount = currentRoom.numberOfPortals;
            RoomPortal[] currentRoomPortals = currentRoom.GetAllPortals();
            for (int p = 0; p < portalCount; p++)
            {
                if (currentRoomPortals[p].wallIndex == wallIndex)
                    output.Add(currentRoomPortals[p]);
            }

            int roomCount = rooms.Count;
            for (int r = 0; r < roomCount; r++)
            {
                Room room = rooms[r];
                if (room == currentRoom) continue;

                int otherWallIndex = room.ContainsWall(a, b);
                if (otherWallIndex != -1)
                {
                    //                    Debug.Log(rooms.IndexOf(currentRoom)+" "+wallIndex+" "+otherWallIndex);
                    RoomPortal[] roomPortals = room.GetAllPortals();
                    int roomPortalCount = roomPortals.Length;
                    for (int rp = 0; rp < roomPortalCount; rp++)
                    {
                        //                        Debug.Log(roomPortals[rp].wallIndex);
                        if (roomPortals[rp].wallIndex == otherWallIndex)
                        {
                            //                            Debug.Log("add portal");
                            output.Add(roomPortals[rp]);
                        }
                    }
                }

            }

            return output;
        }
        
		public void CheckWallIssues()
        {
            issueList.Clear();
            int roomCount = rooms.Count;
            for (int rm = 0; rm < roomCount; rm++)
            {
                Room subjectRoom = rooms[rm];
                Vector2Int[] subjectPoints = subjectRoom.AllPoints();
                int subjectPointCount = subjectPoints.Length;

                for (int rmo = 0; rmo < roomCount; rmo++)
                {
                    if (rm == rmo) continue;
                    Room otherRoom = rooms[rmo];
                    Vector2Int[] otherPoints = otherRoom.AllPoints();
                    int otherPointCount = otherPoints.Length;

                    for (int i = 0; i < subjectPointCount; i++)
                    {
                        Vector2Int sp0 = subjectPoints[i];
                        int subjectIndexB = i < subjectPointCount - 1 ? i + 1 : 0;
                        Vector2Int sp1 = subjectPoints[subjectIndexB];
                        bool lastPointOnLine = false;
                        for (int j = 0; j < otherPointCount; j++)
                        {
                            Vector2Int op = otherPoints[j];
                            //                            if (op == sp0 || op == sp1)
                            //                            {
                            //                                lastPointOnLine = false;
                            //                                continue;
                            //                            }
                            //                            Vector2Int opb = otherPoints[j < otherPointCount - 1 ? j + 1 : 0];

                            //                            if((op == sp0 || op == sp1) && (opb == sp0 || opb == sp1))
                            //                            {
                            //                                lastPointOnLine = false;
                            //                                continue;
                            //                            }

                            float cross = (op.y - sp0.y) * (sp1.x - sp0.x) - (op.x - sp0.x) * (sp1.y - sp0.y);
                            if (Mathf.Abs(cross) > Mathf.Epsilon)
                            {
                                lastPointOnLine = false;
                                continue;
                            }
                            float dot = (op.x - sp0.x) * (sp1.x - sp0.x) + (op.y - sp0.y) * (sp1.y - sp0.y);
                            if (dot < 0)
                            {
                                lastPointOnLine = false;
                                continue;
                            }
                            float squaredlengthba = (sp1.x - sp0.x) * (sp1.x - sp0.x) + (sp1.y - sp0.y) * (sp1.y - sp0.y);
                            if (dot > squaredlengthba)
                            {
                                lastPointOnLine = false;
                                continue;
                            }

                            if (!lastPointOnLine)
                            {
                                lastPointOnLine = true;
                            }
                            else
                            {
                                Vector2Int opb = otherPoints[j - 1];
                                if ((op == sp0 || op == sp1) && (opb == sp0 || opb == sp1))
                                {
                                    lastPointOnLine = false;
                                    continue;
                                }

                                //report room wall
                                WallIssueItem issue = new WallIssueItem();
                                issue.inRoom = subjectRoom;
                                issue.wallIndex = i;//loop always through once so guaranteed it's > 0
                                if (op != sp0 && op != sp1)
                                    issue.point = op;
                                else
                                    issue.point = opb;
                                issueList.Add(issue);
                            }
                        }
                    }
                }
            }
        }

        #region Serialisation

        public void OnBeforeSerialize()
        {
            //ignore calls - serialisation handled internally when data has changed
        }

        public void OnAfterDeserialize()
        {
            int roomCount = rooms.Count;
            for (int r = 0; r < roomCount; r++)
                rooms[r].OnAfterDeserialize();
        }

        public void Serialise()
        {
            int roomCount = rooms.Count;
            for (int r = 0; r < roomCount; r++)
                rooms[r].Serialise();
        }

		public int RoomCount
		{
			get {return rooms.Count;}
		}

		public Room[] AllRooms()
		{
			return rooms.ToArray();
		}

		public void AddRoom(Room room)
		{
			rooms.Add(room);
		}

		public void ClearRooms()
		{
			rooms.Clear();
		}

		#endregion

		public static Floorplan Create(Transform transform)
        {
#if UNITY_EDITOR
            GameObject newFloorplanGo = new GameObject("Floorplan");
            UnityEditor.Undo.RegisterCreatedObjectUndo(newFloorplanGo, "Created Floorplan GameObject");
            UnityEditor.Undo.SetTransformParent(newFloorplanGo.transform, transform, "Parent New Floorplan GameObject");
            newFloorplanGo.transform.localPosition = Vector3.zero;
            newFloorplanGo.transform.localRotation = Quaternion.identity;

            Floorplan output = UnityEditor.Undo.AddComponent<Floorplan>(newFloorplanGo);
            output._visualPart = VisualPart.Create(newFloorplanGo.transform, "Floorplan Visual");

            return output;
#else
            GameObject newFloorplanGo = new GameObject("Interior Floorplan");
            newFloorplanGo.transform.parent = transform;

            Floorplan output = newFloorplanGo.AddComponent<Floorplan>();
            output._visualPart = VisualPart.Create(newFloorplanGo.transform, "Interior Floorplan Visual");
            return output;
#endif
        }
    }
}