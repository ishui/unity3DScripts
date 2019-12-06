#region copyright
// BuildR 2.0
// Available on the Unity Asset Store https://www.assetstore.unity3d.com/#!/publisher/412
// Copyright (c) 2017 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE begin.
#endregion

using System;
using UnityEngine;
using System.Collections.Generic;
using BuildR2.ClipperLib;
using Object = UnityEngine.Object;

namespace BuildR2
{
    [ExecuteInEditMode]
    public class Building : MonoBehaviour, IBuilding
    {
        public float versionNumber = BuildrVersion.NUMBER;//log the version number this was created with

        [SerializeField]
        private List<Volume> _volumes = new List<Volume>();

        [SerializeField]
        private List<VerticalOpening> _openings = new List<VerticalOpening>();

        [SerializeField]
        private float _foundationDepth = 0;
        
        [SerializeField]
        private Surface _foundationSurface = null;
        
        [SerializeField]
        private BuildingMeshTypes _meshTypes = BuildingMeshTypes.Full;

        [SerializeField]
        private BuildingColliderTypes _colliderType = BuildingColliderTypes.Complex;

        [SerializeField]
        private Material[] _materialList = new Material[0];

        [SerializeField]
        private bool _generateInteriors = false;

        [SerializeField]
        private bool _generateExteriors = true;

        [SerializeField]
        private bool _cullDoors = false;

        [SerializeField]
        private bool _showWireframes = false;

        public string exportFilename = "exportedBuildRBuilding";
        public bool exportColliders = true;
        public bool placeIntoScene = false;

	    public uint seed = 1;
	    private RandomGen _rGen;

        public BuildRSettings settings;


//        [SerializeField]
        //        private bool _cullAllBays = false;
        private bool _isModified;
        private bool _regenerate;

		public RandomGen rGen { get {return rGen;} }

	    public void Clear()
		{
			throw new System.NotImplementedException();
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
            for (int p = 0; p < numberOfPlans; p++)
            {
                if (((Object)this[p]).name == planName)
                    return this[p];
            }
            return null;
        }


        public int numberOfPlans
        {
            get { return _volumes.Count; }
        }

        public int numberOfVolumes
        {
            get { return _volumes.Count; }
        }

        public IVolume NewPlan()
        {
            return Volume.Create(transform);
        }

        public IVolume AddPlan()
        {
            return AddPlan(NewPlan());
        }

        public IVolume ClonePlan(IVolume original)
        {
            return AddPlan(original.Clone(), original);
        }

        public IVolume AddPlan(Rect rect)
        {
            Debug.Log("AddPlan(Rect rect) rect.x=" + rect.x + "  rect.y=" + rect.y + "  rect.width=" + rect.width + " rect.h=" + rect.height );
	        IVolume output = AddPlan(NewPlan());
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            output.AddPoint(goInvRot * (new Vector3(rect.xMin, 0, rect.yMin) - goPos));
            output.AddPoint(goInvRot * (new Vector3(rect.xMax, 0, rect.yMin) - goPos));
            output.AddPoint(goInvRot * (new Vector3(rect.xMax, 0, rect.yMax) - goPos));
            output.AddPoint(goInvRot * (new Vector3(rect.xMin, 0, rect.yMax) - goPos));
            return output;
        }

        public IVolume AddPlan(IVolume newPlan, IVolume abovePlan = null)
        {
	        Volume newVolume = newPlan as Volume;
	        if (newVolume == null) throw new NullReferenceException("Runtime Building Not Using Right Volume Type");

            Debug.Log("Building.cs AddPlan(IVolume newPlan, IVolume abovePlan = null) newPlan=(" + newVolume.transform.position.x + "," + newVolume.transform.position.y +")");

			_volumes.Add(newVolume);
            if (abovePlan != null)
            {
                abovePlan.AddAbovePlan(newPlan);
                abovePlan.HeightModified(abovePlan);
            }
            MarkModified();
            return newPlan;
        }

        public IVolume AddPlan(Vector3 position, float size, IVolume abovePlan = null)
        {
	        IVolume output = AddPlan(NewPlan(), abovePlan);
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            output.AddPoint(goInvRot * (position - new Vector3(-0.5f, 0, -0.5f) * size - goPos));
            output.AddPoint(goInvRot * (position - new Vector3(0.5f, 0, -0.5f) * size - goPos));
            output.AddPoint(goInvRot * (position - new Vector3(0.5f, 0, 0.5f) * size - goPos));
            output.AddPoint(goInvRot * (position - new Vector3(-0.5f, 0, 0.5f) * size - goPos));
            return output;
        }

        public IVolume AddPlan(Vector2Int[] points, IVolume abovePlan = null)
        {
	        IVolume output = NewPlan();
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            int pointCount = points.Length;
            if (!BuildrPolyClockwise.Check(points))
                Array.Reverse(points);
            for (int p = 0; p < pointCount; p++)
                output.AddPoint(goInvRot * (points[p].vector3XZ - goPos));
            return AddPlan(output, abovePlan);
        }

        public IVolume AddPlan(Vector2[] points, IVolume abovePlan = null)
        {
	        IVolume output = NewPlan();
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            int pointCount = points.Length;
            if (!BuildrPolyClockwise.Check(points))
                Array.Reverse(points);
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
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
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
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            output.AddPoint(goInvRot * (new Vector3(end.x, 0, start.z) - goPos));
            output.AddPoint(goInvRot * (new Vector3(start.x, 0, start.z) - goPos));
            output.AddPoint(goInvRot * (new Vector3(start.x, 0, end.z) - goPos));
            output.AddPoint(goInvRot * (new Vector3(end.x, 0, end.z) - goPos));
            return output;
        }



        public IVolume AddPlan(IntPoint[] points, IVolume abovePlan = null, float scale = 100)
        {
            HUtils.log();

	        IVolume output = AddPlan(NewPlan(), abovePlan);
            int pointCount = points.Length;
            Vector3 goPos = transform.position;
            Quaternion goInvRot = Quaternion.Inverse(transform.rotation);
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 v3 = goInvRot * (new Vector3(points[p].X / scale, 0, points[p].Y / scale) - goPos);
                Vector2Int v2Int = new Vector2Int(v3.x, v3.z);
                if (output.Contains(v2Int)) continue;
                output.AddPoint(v2Int);
            }
            return output;
        }

        public void RemovePlan(IVolume plan)
		{
			Volume newVolume = plan as Volume;
			if (newVolume == null) throw new NullReferenceException("Runtime Building Not Using Right Volume Type");
			_volumes.Remove(newVolume);
            List<IVolume> childPlans = new List<IVolume>(plan.AbovePlanList());
            int childPlanCount = childPlans.Count;
            for (int cp = 0; cp < childPlanCount; cp++)
            {
                //                childPlans--;
                //                cp--;
                RemovePlan(plan.AbovePlanList()[cp]);
            }
#if UNITY_EDITOR
            DestroyImmediate(plan.gameObject);
#else
            Destroy(plan.gameObject);
#endif

            MarkModified();
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
            Debug.Log( "IndexOf(IVolume plan) 我能到这里");
            if(plan == null){
                Debug.Log( "IndexOf(IVolume plan) 我是null所以不能向前了");
                return -1;
            } 
            Debug.Log( "IndexOf(IVolume plan) 我不是null能到这里");

            HUtils.log();

			Volume newVolume = plan as Volume;
//			if (newVolume == null) throw new NullReferenceException("Building Not Using Right Volume Type");
			return _volumes.IndexOf(newVolume);
        }

        public BuildingMeshTypes meshType
        {
            
            get { return _meshTypes; }
            set
            {
                if (value != _meshTypes)
                {
                    _meshTypes = value;
                    _regenerate = true;
                }
            }
        }

        public BuildingColliderTypes colliderType
        {
            get { return _colliderType; }
            set
            {
                if (value != _colliderType)
                {
                    _colliderType = value;
                    _regenerate = true;
                }
            }
        }

        public bool generateExteriors
        {
            get { return _generateExteriors; }
            set
            {
                if (_generateExteriors != value)
                {
                    _generateExteriors = value;
                    MarkModified();
                }
            }
        }

        public bool cullDoors
        {
            get { return _cullDoors; }
            set
            {
                if (_cullDoors != value)
                {
                    _cullDoors = value;
                    MarkModified();
                }
            }
        }

        public bool showWireframes
        {
            get { return _showWireframes; }
            set
            {
                if (_showWireframes != value)
                {
                    _showWireframes = value;
                    MarkModified();
                }
            }
        }

        public bool generateInteriors
        {
            get { return _generateInteriors; }
            set
            {
                if (_generateInteriors != value)
                {
                    _generateInteriors = value;
                    MarkModified();
                }
            }
        }

        public Bounds designBounds
        {
            get
            {
                if (this[0].numberOfPoints == 0)
                    return new Bounds();
                Bounds designBounds = new Bounds(this[0].BuildingPoint(0), Vector3.zero);
                for (int p = 0; p < numberOfPlans; p++)
                {
	                IVolume plan = this[p];
                    for (int v = 0; v < plan.numberOfPoints; v++)
                        designBounds.Encapsulate(plan.BuildingPoint(v));
                }
                return designBounds;
            }
        }

        public Material[] materialList
        {
            get { return _materialList; }
            set { _materialList = value; }
        }

        public int openingCount
        {
            get { return _openings.Count; }
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
            get {return _foundationDepth;}
            set
            {
                if(_foundationDepth != value)
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
            HUtils.log();

            int volumeCount = _volumes.Count;
            int output = 0;
            //            Debug.Log("VolumeBaseFloor "+vol.name);
            for (int v = 0; v < volumeCount; v++)
            {
                IVolume other = _volumes[v];
                if (other == vol) continue;
                if (other.ContainsPlanAbove(vol))
                {//start the loop again - register the floors below current plan - use parent plan to find other parents
                    v = -1;
                    vol = other;
                    output += vol.floors;
                    //                    Debug.Log("above plan "+ vol.name);
                }
            }
            //            Debug.Log("VolumeBaseFloor is " + output);
            return output;
        }

        public List<IVolume> AllAboveVolumes(IVolume input)
        {
            HUtils.log();

            List<IVolume> aboveVolumeList = new List<IVolume>();
            List<IVolume> aboveVolumePprocessor = new List<IVolume>(input.AbovePlanList());
            while (aboveVolumePprocessor.Count > 0)
            {
                if (aboveVolumePprocessor[0] != null)
                {
                    aboveVolumeList.Add(aboveVolumePprocessor[0]);
//                    if (aboveVolumePprocessor[0].abovePlans != null)
                        aboveVolumePprocessor.AddRange(aboveVolumePprocessor[0].AbovePlanList());
                }
                aboveVolumePprocessor.RemoveAt(0);
            }
            return aboveVolumeList;
        }

        public bool IsBaseVolume(IVolume input)
        {
            int volumeCount = numberOfVolumes;
            for(int v = 0; v < volumeCount; v++)
            {
	            IVolume subject = this[v];
                if(subject == input) continue;

                if(subject.ContainsPlanAbove(input)) return false;
            }
            return true;
        }


        public void CalculateSubmeshes(SubmeshLibrary submeshLibrary)
        {
//            bool blankSurfaces = false;
            for (int v = 0; v < numberOfVolumes; v++)
            {
	            IVolume volume = this[v];
                int numberOfPoints = volume.numberOfPoints;
                for (int p = 0; p < numberOfPoints; p++)
                {
                    Facade facadeDesign = volume.GetFacade(p);
                    if(facadeDesign != null)
                    {
                        submeshLibrary.Add(facadeDesign.stringCourseSurface);

                        List<WallSection> usedWallSections = facadeDesign.usedWallSections;
                        int sectionCount = usedWallSections.Count;
                        for(int u = 0; u < sectionCount; u++)
                            submeshLibrary.Add(usedWallSections[u]);
                    }

                    if(volume[p].isGabled && volume[p].gableStyle != null)
                        submeshLibrary.Add(volume[p].gableStyle.surface);
                }

                submeshLibrary.Add(volume.roof.mainSurface);
                submeshLibrary.Add(volume.roof.wallSurface);
                submeshLibrary.Add(volume.roof.floorSurface);

                submeshLibrary.Add(volume.roof.wallSection);
            }
        }

        private IVolume GetModifiedPlan()
        {
            HUtils.log();

            int planCount = numberOfPlans;
            //            Volume modifiedPlan = null;
            for (int p = 0; p < planCount; p++)
                if (_volumes[p].isModified)
                    return _volumes[p];
            return null;
        }

        private void CheckPlanHeights()
        {
            int planCount = _volumes.Count;
            List<IVolume> modPlans = new List<IVolume>();

            for (int p = 0; p < planCount; p++)
            {
	            IVolume plan = _volumes[p];
                if (plan.isModified)
                {
                    modPlans.Add(plan);
                    break;
                }
            }

            if (modPlans.Count == 0) return;//nothing modified

	        IVolume modPlan = modPlans[0];
	        int linkPlans = modPlan.linkPlanCount;
            for (int pl = 0; pl < linkPlans; pl++)
            {
	            IVolume linkedPlan = modPlan.LinkPlanList()[pl];
                linkedPlan.floorHeight = modPlan.floorHeight;
                linkedPlan.floors = modPlan.floors;
                if (!modPlans.Contains(linkedPlan))
                    modPlans.Add(linkedPlan);
            }

            while (modPlans.Count > 0)
            {
                modPlan = modPlans[0];
                modPlans.RemoveAt(0);
                int childPlans = modPlan.abovePlanCount;
                for (int cp = 0; cp < childPlans; cp++)
                {
                    IVolume childPlan = modPlan.AbovePlanList()[cp];
                    childPlan.baseHeight = modPlan.planTotalHeight;
                    modPlans.Add(childPlan);
                }
            }

        }

        //Assumption - only one plan will be modified at once
        private void CheckPointMovements(IVolume modifiedPlan)
        {
            HUtils.log();

            if (modifiedPlan == null)
                return;//nothing to register
            int planCount = numberOfPlans;

            List<VolumePoint> movedPoints = modifiedPlan.GetMovedPoints();
            if (movedPoints.Count > 0)
            {
                for (int p = 0; p < planCount; p++)
                {
                    if ((Volume)modifiedPlan == _volumes[p]) continue;
                    _volumes[p].PointModified(movedPoints);
                }
            }
        }

        private void CheckBuildingLegality()
        {
            for (int pA = 0; pA < numberOfPlans; pA++)
            {
                IVolume planA = _volumes[pA];
                for (int pB = 0; pB < numberOfPlans; pB++)
                {
                    if (pA == pB) continue;

	                IVolume planB = _volumes[pB];

                    if (planA.bounds.Intersects(planB.bounds))
                    {
                        //check for actual intersection
                        //do curves as far a double line to control point
                    }
                }
            }
        }

        public virtual bool isModified
        {
            get
            {
                for (int p = 0; p < numberOfPlans; p++)
                    if (_volumes[p].isModified)
                        return true;
                return _isModified;
            }
        }

        public virtual bool regenerate
        {
            get { return _regenerate; }
        }

        public virtual void MarkModified()
        {
            HUtils.log();

            if (!BuildRSettings.AUTO_UPDATE) return;
            _isModified = true;
            _regenerate = true;
			if(_rGen == null ) _rGen = new RandomGen();
	        _rGen.seed = seed;
	        IVolume modifiedPlan = GetModifiedPlan();
            if (modifiedPlan != null)
            {
                //                CheckPlanHeights();
                CheckPointMovements(modifiedPlan);
                CheckBuildingLegality();
            }
            SaveData();
            MarkUnmodified();
        }

        private void SaveData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
//            UnityEditor.AssetDatabase.SaveAssets();
//            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void MarkUnmodified()
        {
            _isModified = false;
            for (int p = 0; p < numberOfPlans; p++)
                if (_volumes[p].isModified)
                    _volumes[p].MarkUnmodified();
        }

        public void MarkGenerated()
        {
            HUtils.log();
            _regenerate = false;
        }

        public BuildRSettings getSettings
        {
            get
            {
                if (settings == null)
                    settings = BuildRSettings.GetSettings();
                return settings;
            }
        }
        
        #if UNITY_EDITOR
//        [UnityEditor.Callbacks.DidReloadScripts]
        private void OnScriptsReload()
        {
            foreach(Volume vol in _volumes)
                vol.MarkModified();
            MarkModified();
        }
        #endif

        #region statics
        public static Building CreateNewBuilding()
        {
            HUtils.log();

            GameObject newBuildingGO = new GameObject("NewBuilding");
            Building newBuilding = newBuildingGO.AddComponent<Building>();
            Visual visual = newBuildingGO.AddComponent<Visual>();
            visual.building = newBuilding;
            //todo Add support to create prefab here
            return newBuilding;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/BuildR/Create New Building", false, ToolsMenuLevels.CREATE_BUILDING)]
        private static Building MenuCreateNewFacade()
        {
            Building output = CreateNewBuilding();
            UnityEditor.Selection.activeObject = output;
            return output;
        }
#endif
        #endregion
    }
}