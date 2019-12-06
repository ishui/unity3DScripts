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
using BuildR2.ClipperLib;
using UnityEngine;

namespace BuildR2
{

	public enum BuildingMeshTypes
	{
		None,
		Box,
		Simple,
		Full
	}

	public enum BuildingColliderTypes
	{
		None,
		Primitive,
		Simple,
		Complex
	}

	public interface IBuilding
	{
		GameObject gameObject { get; }
		Transform transform { get; }
		RandomGen rGen {get;}
		int numberOfPlans {get;}
		int numberOfVolumes {get;}
		BuildingMeshTypes meshType {get; set;}
		BuildingColliderTypes colliderType {get; set;}
		bool generateExteriors {get; set;}
		bool cullDoors {get; set;}
		bool showWireframes {get; set;}
		bool generateInteriors {get; set;}
		Bounds designBounds {get;}
		Material[] materialList {get; set;}
		int openingCount {get;}
		float foundationDepth {get; set;}
		Surface foundationSurface {get; set;}
		bool isModified {get;}
		bool regenerate {get;}
		BuildRSettings getSettings {get;}
		string name {get; set;}
		IVolume this[int index] {get;}
		IVolume[] AllPlans();
		IVolume GetPlanByName(string planName);
		IVolume NewPlan();
		IVolume AddPlan();
		IVolume ClonePlan(IVolume original);
		IVolume AddPlan(IVolume newPlan, IVolume abovePlan = null);
		IVolume AddPlan(Vector3 position, float size, IVolume abovePlan = null);
		IVolume AddPlan(Vector2Int[] points, IVolume abovePlan = null);
		IVolume AddPlan(Vector2[] points, IVolume abovePlan = null);
		IVolume AddPlan(List<Vector2> points, IVolume abovePlan = null);
		IVolume AddPlan(Vector3 start, Vector3 end);
		IVolume AddPlan(Rect rect);
		IVolume AddPlan(IntPoint[] points, IVolume abovePlan = null, float scale = 100);
		void RemovePlan(IVolume plan);
		void RemovePlanAt(int index);
		int IndexOf(IVolume plan);
		VerticalOpening AddOpening();
		void RemoveOpening(VerticalOpening opening);
		VerticalOpening GetOpening(int index);
		VerticalOpening[] GetAllOpenings();
		VerticalOpening[] GetOpenings(IVolume volume, IFloorplan inPlan);
		bool[] GetOpenings(IVolume volume, IFloorplan inPlan, int offset);
		int VolumeBaseFloor(IVolume vol);
		List<IVolume> AllAboveVolumes(IVolume input);
		bool IsBaseVolume(IVolume input);
		void CalculateSubmeshes(SubmeshLibrary submeshLibrary);
		void MarkModified();
		void MarkUnmodified();
		void MarkGenerated();
		void Clear();
	}
}