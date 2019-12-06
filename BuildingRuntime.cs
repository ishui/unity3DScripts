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
using BuildR2.ClipperLib;
using Object = UnityEngine.Object;

namespace BuildR2
{
	[Serializable]
	public class BuildingRuntime : IBuilding
	{
		private string _name;
		[SerializeField]
		private List<VolumeRuntime> _volumes = new List<VolumeRuntime>();
		private List<VerticalOpening> _openings = new List<VerticalOpening>();
		private float _foundationDepth = 0;
		private Surface _foundationSurface = null;
		private BuildingMeshTypes _meshTypes = BuildingMeshTypes.Full;
		private BuildingColliderTypes _colliderType = BuildingColliderTypes.Complex;
		private bool _generateInteriors = false;
		private bool _generateExteriors = true;
		private bool _cullDoors = false;
		private bool _showWireframes = false;
		private RandomGen _rGen;
		private Transform _transform;

		public uint seed = 1;
		public GameObject gameObject {get; private set;}
		public Transform transform {get {return _transform;} }
		public Vector3 transformposition {get {return _transform != null ? _transform.position : Vector3.zero;} }
		public Quaternion transformrotation {get {return _transform != null ? _transform.rotation : Quaternion.identity;} }
		public RandomGen rGen {get; private set; }

	    public void Move(Vector3 by)
	    {
	        foreach (VolumeRuntime volume in _volumes)
	            volume.visualPart.Move(by);
	    }

	    public void Place(Vector3 position)
	    {
	        foreach (VolumeRuntime volume in _volumes)
	            volume.visualPart.Place(position);
	    }

        [Obsolete]
		public int numberOfPlans { get { return _volumes.Count; } }
		public int numberOfVolumes{ get { return _volumes.Count; } }

		public string name { get { return _name;} set {_name = value;} }

		public BuildingMeshTypes meshType
		{
			get { return _meshTypes; }
			set{_meshTypes = value;}
		}

		public BuildingColliderTypes colliderType
		{
			get { return _colliderType; }
			set{ _colliderType = value; }
		}

		public bool generateExteriors
		{
			get { return _generateExteriors; }
			set { _generateExteriors = value; }
		}

		public bool generateInteriors
		{
			get {return _generateInteriors;}
			set { _generateInteriors = value; }
		}

		public bool cullDoors
		{
			get { return _cullDoors; }
			set{_cullDoors = value;}
		}


		public bool showWireframes
		{
			get { return _showWireframes; }
			set{_showWireframes = value;}
		}
		
		public Bounds designBounds {get; private set;}
		public Material[] materialList {get; set;}
		public int openingCount {get; private set;}
		public bool isModified {get; private set;}
		public bool regenerate {get; private set;}
		public BuildRSettings getSettings {get; private set;}


		public void Clear()
		{
			for(int v = 0; v < _volumes.Count; v++)
				_volumes[v].Clear();
		}

		public void GenerateRuntimeMesh()
		{
			if (_rGen == null) _rGen = new RandomGen();
			_rGen.seed = seed;
//		    Debug.Log("GenerateRuntimeMesh "+seed);
			GenerateMesh.Generate(this);
//		    Debug.Log(_volumes.Count);
			for (int v = 0; v < _volumes.Count; v++)
				_volumes[v].GenerateRuntimeMesh();
		}

		public void SetTransform(Transform useTransform)
		{
			_transform = useTransform;
		}

		public IVolume this[int index]
		{
			get { return _volumes[index]; }
		}

		public IVolume[] AllPlans()
		{
			return _volumes.ToArray();
		}

		public IVolume GetPlanByName(string planName)
		{
			for (int p = 0; p < numberOfVolumes; p++)
			{
				if (((Object)this[p]).name == planName)
					return this[p];
			}
			return null;
		}

		public IVolume NewPlan()
		{
			IVolume output = new VolumeRuntime();
			output.SetBuilding(this);
			return output;
		}

		public IVolume AddPlan()
		{
			return AddPlan(NewPlan());
		}

		public IVolume ClonePlan(IVolume original)
		{
			return AddPlan(original.Clone(), original);
		}

		public IVolume AddPlan(IVolume newPlan, IVolume abovePlan = null)
		{
			VolumeRuntime newVolume = newPlan as VolumeRuntime;
			if(newVolume == null) throw new NullReferenceException("Runtime Building Not Using Right Volume Type");
			_volumes.Add(newVolume);
			if (abovePlan != null)
			{
				abovePlan.AddAbovePlan(newPlan);
				abovePlan.HeightModified(abovePlan);
			}
			return newPlan;
		}

		public IVolume AddPlan(Vector3 position, float size, IVolume abovePlan = null)
		{
			IVolume output = AddPlan(NewPlan(), abovePlan);
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			output.AddPoint(goInvRot * (position - new Vector3(-0.5f, 0, -0.5f) * size - goPos));
			output.AddPoint(goInvRot * (position - new Vector3(0.5f, 0, -0.5f) * size - goPos));
			output.AddPoint(goInvRot * (position - new Vector3(0.5f, 0, 0.5f) * size - goPos));
			output.AddPoint(goInvRot * (position - new Vector3(-0.5f, 0, 0.5f) * size - goPos));
			return output;
		}

		public IVolume AddPlan(Vector2Int[] points, IVolume abovePlan = null)
		{
			IVolume output = NewPlan();
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			int pointCount = points.Length;
			for (int p = 0; p < pointCount; p++)
				output.AddPoint(goInvRot * (points[p].vector3XZ - goPos));
			return AddPlan(output, abovePlan);
		}

		public IVolume AddPlan(Vector2[] points, IVolume abovePlan = null)
		{
			IVolume output = NewPlan();
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			int pointCount = points.Length;
			for (int p = 0; p < pointCount; p++)
			{
				Vector3 pV3 = new Vector3(points[p].x, 0, points[p].y);
				output.AddPoint(goInvRot * (pV3 - goPos));
			}
			return AddPlan(output, abovePlan);
		}

		public IVolume AddPlan(List<Vector2> points, IVolume abovePlan = null)
		{
			IVolume output = NewPlan();
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			int pointCount = points.Count;
			for (int p = 0; p < pointCount; p++)
			{
				Vector3 pV3 = new Vector3(points[p].x, 0, points[p].y);
				output.AddPoint(goInvRot * (pV3 - goPos));
			}
			return AddPlan(output, abovePlan);
		}

		public IVolume AddPlan(Vector3 start, Vector3 end)
		{
			IVolume output = AddPlan(NewPlan());
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			output.AddPoint(goInvRot * (new Vector3(end.x, 0, start.z) - goPos));
			output.AddPoint(goInvRot * (new Vector3(start.x, 0, start.z) - goPos));
			output.AddPoint(goInvRot * (new Vector3(start.x, 0, end.z) - goPos));
			output.AddPoint(goInvRot * (new Vector3(end.x, 0, end.z) - goPos));
			return output;
		}

		public IVolume AddPlan(Rect rect)
		{
			IVolume output = AddPlan(NewPlan());
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			output.AddPoint(goInvRot * (new Vector3(rect.xMin, 0, rect.yMin) - goPos));
			output.AddPoint(goInvRot * (new Vector3(rect.xMax, 0, rect.yMin) - goPos));
			output.AddPoint(goInvRot * (new Vector3(rect.xMax, 0, rect.yMax) - goPos));
			output.AddPoint(goInvRot * (new Vector3(rect.xMin, 0, rect.yMax) - goPos));
			return output;
		}

		public IVolume AddPlan(IntPoint[] points, IVolume abovePlan = null, float scale = 100)
		{
			IVolume output = AddPlan(NewPlan(), abovePlan);
			int pointCount = points.Length;
			Vector3 goPos = transformposition;
			Quaternion goInvRot = Quaternion.Inverse(transformrotation);
			for (int p = 0; p < pointCount; p++)
			{
				Vector3 v3 = goInvRot * (new Vector3(points[p].X / scale, 0, points[p].Y/ scale) - goPos);
				Vector2Int v2Int = new Vector2Int(v3.x, v3.z);
				if (output.Contains(v2Int)) continue;
				output.AddPoint(v2Int);
			}
			return output;
		}

		public void RemovePlan(IVolume plan)
		{
			VolumeRuntime newVolume = plan as VolumeRuntime;
			if (newVolume == null) throw new NullReferenceException("Runtime Building Not Using Right Volume Type");
			_volumes.Remove(newVolume);
			List<IVolume> childPlans = new List<IVolume>(plan.AbovePlanList());
			int childPlanCount = childPlans.Count;
			for (int cp = 0; cp < childPlanCount; cp++)
				RemovePlan(plan.AbovePlanList()[cp]);
		}

		public void RemovePlanAt(int index)
		{
			if (_volumes[index] != null)
				RemovePlan(_volumes[index]);
			else
				_volumes.RemoveAt(index);
		}

		public int IndexOf(IVolume plan)
		{
			VolumeRuntime newVolume = plan as VolumeRuntime;
			if (newVolume == null) throw new NullReferenceException("Runtime Building Not Using Right Volume Type");
			return _volumes.IndexOf(newVolume);
		}

		public VerticalOpening AddOpening()
		{
			VerticalOpening output = new VerticalOpening();
			_openings.Add(output);
			return output;
		}

		public void RemoveOpening(VerticalOpening opening)
		{
			_openings.Remove(opening);
		}

		public VerticalOpening GetOpening(int index)
		{
			return _openings[index];
		}

		public VerticalOpening[] GetAllOpenings()
		{
			return _openings.ToArray();
		}

		public VerticalOpening[] GetOpenings(IVolume volume, IFloorplan inPlan)
		{
			List<VerticalOpening> output = new List<VerticalOpening>();
			int count = _openings.Count;
			int planFloor = volume.Floor(inPlan);
			for (int i = 0; i < count; i++)
			{
				VerticalOpening opening = _openings[i];
				int baseFloor = opening.baseFloor;
				int topFloor = baseFloor + opening.floors;
				if (planFloor >= baseFloor && planFloor <= topFloor)
					output.Add(opening);
			}
			return output.ToArray();
		}

		public bool[] GetOpenings(IVolume volume, IFloorplan inPlan, int offset)
		{
			List<bool> output = new List<bool>();
			int count = _openings.Count;
			int planFloor = volume.Floor(inPlan);
			int offsetFloor = planFloor + offset;
			for (int i = 0; i < count; i++)
			{
				VerticalOpening opening = _openings[i];
				int baseFloor = opening.baseFloor;
				int topFloor = baseFloor + opening.floors;
				if (planFloor >= baseFloor && planFloor <= topFloor)
				{
					bool isOpen = offsetFloor >= baseFloor && offsetFloor <= topFloor;
					output.Add(isOpen);
				}
			}
			return output.ToArray();
		}

		public float foundationDepth
		{
			get { return _foundationDepth; }
			set
			{
				if (_foundationDepth != value)
				{
					_foundationDepth = value;
					MarkModified();
				}
			}
		}

		public Surface foundationSurface
		{
			get {return _foundationSurface;}
			set
			{
				if(_foundationSurface != value)
				{
					_foundationSurface = value;
					MarkModified();
				}
			}
		}

		public int VolumeBaseFloor(IVolume vol)
		{
			int volumeCount = _volumes.Count;
			int output = 0;
			for (int v = 0; v < volumeCount; v++)
			{
				IVolume other = _volumes[v];
				if (other == vol) continue;
				if (other.ContainsPlanAbove(vol))
				{//start the loop again - register the floors below current plan - use parent plan to find other parents
					v = -1;
					vol = other;
					output += vol.floors;
				}
			}
			return output;
		}

		public List<IVolume> AllAboveVolumes(IVolume input)
		{
			List<IVolume> aboveVolumeList = new List<IVolume>();
			List<IVolume> aboveVolumePprocessor = new List<IVolume>(input.AbovePlanList());
			while (aboveVolumePprocessor.Count > 0)
			{
				if (aboveVolumePprocessor[0] != null)
				{
					aboveVolumeList.Add(aboveVolumePprocessor[0]);
					aboveVolumePprocessor.AddRange(aboveVolumePprocessor[0].AbovePlanList());
				}
				aboveVolumePprocessor.RemoveAt(0);
			}
			return aboveVolumeList;
		}

		public bool IsBaseVolume(IVolume input)
		{
			int volumeCount = numberOfVolumes;
			for (int v = 0; v < volumeCount; v++)
			{
				IVolume subject = this[v];
				if (subject == input) continue;

				if (subject.ContainsPlanAbove(input)) return false;
			}
			return true;
		}


		public void CalculateSubmeshes(SubmeshLibrary submeshLibrary)
		{
			for (int v = 0; v < numberOfVolumes; v++)
			{
				IVolume volume = this[v];
				int numberOfPoints = volume.numberOfPoints;
				for (int p = 0; p < numberOfPoints; p++)
				{
					Facade facadeDesign = volume.GetFacade(p);
					if (facadeDesign != null)
					{
						submeshLibrary.Add(facadeDesign.stringCourseSurface);

						List<WallSection> usedWallSections = facadeDesign.usedWallSections;
						int sectionCount = usedWallSections.Count;
						for (int u = 0; u < sectionCount; u++)
							submeshLibrary.Add(usedWallSections[u]);
					}

					if (volume[p].isGabled && volume[p].gableStyle != null)
						submeshLibrary.Add(volume[p].gableStyle.surface);
				}

				submeshLibrary.Add(volume.roof.mainSurface);
				submeshLibrary.Add(volume.roof.wallSurface);
				submeshLibrary.Add(volume.roof.floorSurface);

				submeshLibrary.Add(volume.roof.wallSection);
			}
		}

		public void MarkModified()
		{

		}

		public void MarkUnmodified()
		{

		}

		public void MarkGenerated()
		{

		}

		public override string ToString()
		{
			return "Runtime Building";
		}
	}
}