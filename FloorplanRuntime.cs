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
using System.Collections.Generic;
using UnityEngine;

namespace BuildR2
{
	[Serializable]
	[RequireComponent(typeof(BuildRPart))]
    public class FloorplanRuntime : IFloorplan
	{
        public List<Room> rooms = new List<Room>();
		
        [SerializeField]
        private VisualPartRuntime _visualPart;

		public IVisualPart visualPart
		{
			get
			{
				if (_visualPart == null)
					_visualPart = VisualPartRuntime.GetPoolItem();
				return _visualPart;
			}
		}

		public void SetDefaultName(int floor)
        {
            name = string.Format("Floorplan ( floor {0} )", floor);
		}

		public string name { get; private set; }
		public GameObject gameObject { get; private set; }
		public Transform transform { get {return visualPart.transform;} }

		public int RoomCount
		{
			get { return rooms.Count; }
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

		public void Clear()
		{
			if(_visualPart != null)
			{
				_visualPart.Deactivate();
				VisualPartRuntimePool.Instance.Push(_visualPart);
			}
			_visualPart = null;
		}

		public void GenerateRuntimeMesh()
		{
			if (visualPart == null)
				_visualPart = VisualPartRuntime.GetPoolItem();
			_visualPart.GenerateFromDynamicMesh();
		}

		public bool isModified
        {
            get
            {
                return false;
            }
        }

        public void MarkModified()
        {

        }

        public void MarkUnmodified()
        {

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
		
//		public void CalculateSubmeshes(SubmeshLibrary submeshLibrary)
//		{
//			int roomCount = rooms.Count;
//			for(int r = 0; r < roomCount; r++)
//			{
//				Room room = rooms[r];
//				if (room.style != null)
//				{
//				    Surface[] roomSurfaces = room.style.usedSurfaces;
//				    int roomSurfaceCount = roomSurfaces.Length;
//				    for (int rs = 0; rs < roomSurfaceCount; rs++)
//					    submeshLibrary.Add(roomSurfaces[rs]);
//				}
//			}
//		}

		public void CheckWallIssues()
		{

		}

		public void Serialise()
		{

		}
		
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{

		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{

		}
	}
}