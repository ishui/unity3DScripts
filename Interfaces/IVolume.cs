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
	public interface IVolume
	{
		string name {get;}
		GameObject gameObject {get; }
		Transform transform { get; }
		void Initialise(List<VolumePoint> points, int newFloorCount = 1, float newFloorHeight = 3.0f);
		void LinkPlans(IVolume link);
		void UnlinkPlans(IVolume link);
		void SetBuilding(IBuilding building);
		VolumePoint this[int index] {get; set;}
		int numberOfPoints {get;}
		bool external {get; set;}
//		IBuilding building {get;}
		Dictionary<int, List<Vector2Int>> facadeWallAnchors {get;}
		Dictionary<Vector2Int, List<int>> externalFacadeWallAnchors {get;}
		List<Vector2Int> wallAnchors {get;}
		Roof roof {get; set;}
		Surface undersideSurafce {get; set;}
		bool isLegal {get;}
		Vector2Int[] illegalPoints {get;}
		int numberOfFacades {get;}
		float baseHeight {get; set;}
		float floorHeight {get; set;}
		float planHeight {get;}
		float volumeHeight {get;}
		float planTotalHeight {get;}
		float volumeTotalHeight {get;}
		float minimumWallUnitLength {get; set;}
		int floors {get; set;}
		float wallThickness {get; set;}
		Bounds bounds {get;}
		IVisualPart visualPart {get;}
		GameObject prefabs {get;}
		bool isModified {get;}
		VolumePoint[] AllFloorplanPoints();
		Vector2Int[] AllPoints();
		Vector2[] AllPointsV2();
		Vector3 BuildingPoint(int index);
		Vector3 WorldPoint(int index);
		Vector3[] AllBuildingPoints();
		IFloorplan[] InteriorFloorplans();
		int Floor(IFloorplan interior);
		void PointModified(List<VolumePoint> data);
		void HeightModified(IVolume subject);
		void CalculateExternalWallAnchors();
		Vector2Int GetControlPointA(int index);
		void SetControlPointA(int index, Vector2Int value);
		Vector2Int GetControlPointB(int index);
		void SetControlPointB(int index, Vector2Int value);
		Vector3 BuildingControlPointA(int index);
		Vector3 BuildingControlPointB(int index);
		Vector3 WorldControlPointA(int index);
		Vector3 WorldControlPointB(int index);
		Facade GetFacade(int index);
		void SetFacade(int index, Facade value);
		Facade[] GetAllFacades();
		float CalculateHeight(int floor);
		float CalculateFloorHeight(int floor);
		float CalculateHeight(IFloorplan floorplan);
		bool IsWallStraight(int index);
		IVolume Clone();
		List<VolumePoint> GetModifiedPoints();
		List<VolumePoint> GetMovedPoints();
//		void CalculateSubmeshes(SubmeshLibrary submeshLibrary);
		bool IsGabled();
		void AddPoint(Vector2Int newPosition);
		void AddPoint(Vector3 newPoint);
		void AddPoints(Vector2Int[] newPoints);
		void AddPoints(Vector3[] newPoints);
		void RemovePoint(Vector2Int point);
		void RemovePointAt(int index);
		void InsertPoint(int index, Vector2Int point);
		bool Contains(Vector2Int point);
		void CheckVolume();
		void MarkModified();
		void MarkUnmodified();
		bool PlanLegalityCheck();

		void AddAbovePlan(IVolume volume);
		void AddLinkPlan(IVolume volume);
		List<IVolume> AbovePlanList();
		List<IVolume> LinkPlanList();
		bool ContainsPlanAbove(IVolume volume);
		bool IsLinkedPlan(IVolume volume);
		int abovePlanCount {get;}
		int linkPlanCount {get; }
		void Clear();
	}
}