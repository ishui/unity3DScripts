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

namespace BuildR2 {
	public interface IFloorplan : ISerializationCallbackReceiver
	{
		string name { get; }
		GameObject gameObject { get; }
		Transform transform { get; }
		IVisualPart visualPart {get;}
		bool isModified {get;}
		void SetDefaultName(int floor);
		void MarkModified();
		void MarkUnmodified();
		List<RoomPortal> GetWallPortals(Room currentRoom, int wallIndex);
//		void CalculateSubmeshes(SubmeshLibrary submeshLibrary);
		void CheckWallIssues();
		void Serialise();
		int RoomCount {get;}
		Room[] AllRooms();
		void ClearRooms();
		void AddRoom(Room room);
		void Clear();
	}
}