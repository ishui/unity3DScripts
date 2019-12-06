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
using Object = UnityEngine.Object;

namespace BuildR2
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BuildRPart))]
    public class Volume : MonoBehaviour, IVolume
    {
        #region Variables
        
        private bool _isModified;
        [SerializeField]
        private List<VolumePoint> _points = new List<VolumePoint>();
        [SerializeField]
        private int _floors = 1;
        [SerializeField]
        private float _floorHeight = 3.0f;
        [SerializeField]
        private float _minimumWallUnitLength = 2.0f;
        [SerializeField]
        private float _baseHeight = 0;//the height which the plan sits upon - predicated upon which plan it's meant to be above.
        [SerializeField]
        private Bounds _bounds = new Bounds(Vector3.zero, Vector3.zero);

        [SerializeField]
        private float _wallThickness = 0.2f;

        [SerializeField]
        private bool _external = false;

        [SerializeField]
        private Roof _roof = new Roof();

	    [SerializeField]
	    private Surface _undersideSurafce = null;

        [SerializeField]
        private List<Floorplan> _interiorFloorplans = new List<Floorplan>();
        
        //above plans store any plans that sit above this one so that they will be positioned vertically correctly
        public List<Volume> abovePlans = new List<Volume>();
        //linked plans share each other's floor height and number data
        public List<Volume> linkedPlans = new List<Volume>();

        [SerializeField]
        private VisualPart _visualPart;

        [SerializeField]
        private GameObject _prefabs;

        private IBuilding _building = null;

        #endregion

        #region API

        public void Initialise(List<VolumePoint> points, int newFloorCount = 1, float newFloorHeight = 3.0f)
        {
            _points.Clear();
            _points.AddRange(points);
            _floors = newFloorCount;
            _floorHeight = newFloorHeight;
            _interiorFloorplans.Clear();
            for (int f = 0; f < newFloorCount; f++)
            {
                Floorplan floorplan = Floorplan.Create(transform);
                floorplan.SetDefaultName(f);
                _interiorFloorplans.Add(floorplan);
            }

        }

        public void LinkPlans(IVolume link)
        {
            Volume vLink = link as Volume;
            if (!linkedPlans.Contains(vLink))
            {
                linkedPlans.Add(vLink);
                link.LinkPlans(this);
            }
        }

        public void UnlinkPlans(IVolume link) 
            {
            Volume vLink = link as Volume;
            if (linkedPlans.Contains(vLink))
            {
                linkedPlans.Remove(vLink);
                link.UnlinkPlans(this);
            }
        }

	    public void SetBuilding(IBuilding building)
	    {
		    _building = building;
	    }

        #region Plan Points
        public VolumePoint this[int index]
        {
            get { return _points[index]; }
            set
            {
                if (_points[index] != value || value.modified)
                {
                    _points[index] = value;
                    MarkModified();
                }
            }
        }

        public int numberOfPoints
        {
            get { return _points.Count; }
        }

        public VolumePoint[] AllFloorplanPoints()
        {
            return _points.ToArray();
        }

        public Vector2Int[] AllPoints()
        {
            Vector2Int[] output = new Vector2Int[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                output[i] = this[i].position;
            return output;
        }

        public Vector2[] AllPointsV2()
        {
            Vector2[] output = new Vector2[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                output[i] = this[i].position.vector2;
            return output;
        }

        public Vector3 BuildingPoint(int index)
        {
            return this[index].position.vector3XZ + Vector3.up * baseHeight;
        }

        public Vector3 WorldPoint(int index)
        {
            return transform.rotation * this[index].position.vector3XZ + Vector3.up * baseHeight + transform.position;
        }

        public Vector3[] AllBuildingPoints()
        {
            Vector3[] output = new Vector3[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                output[i] = this[i].position.vector3XZ + Vector3.up * baseHeight;
            return output;
        }

        public IFloorplan[] InteriorFloorplans()
        {
            CheckInternalFloorplans();
            return _interiorFloorplans.ToArray();
        }

        public int Floor(IFloorplan interior) {
            Floorplan fInterior = interior as Floorplan;
            return _interiorFloorplans.IndexOf(fInterior);
        }

        public bool external
        {
            get { return _external; }
            set
            {
                if (value != _external)
                {
                    _external = value;
                    MarkModified();
                }
            }
		}

	    public void Clear()
	    {

	    }

		//inform the floorplan that a point has been modified
		//check to see if this point affects this plan
		public void PointModified(List<VolumePoint> data)
        {
            int dataCount = data.Count;
            if (data.Count == 0)
                return;
            int pointCount = numberOfPoints;
            for (int d = 0; d < dataCount; d++)
            {
                VolumePoint point = data[d];
                if (!point.moved) continue;
                for (int p = 0; p < pointCount; p++)
                {
                    if (point.lastPosition == _points[p].position)
                    {
                        _points[p].position = point.position;
                        _points[p].MarkUnmodified();
                    }
                }
            }
            if (isModified)
                MarkModified();
        }

        public void HeightModified(IVolume subject)
        {
            Volume vsubject = subject as Volume;
            if (vsubject != null && vsubject == this)
            {
                int linkedPlanCount = linkedPlans.Count;
                for (int l = 0; l < linkedPlanCount; l++)
                {
	                IVolume volume = linkedPlans[l];
                    if (volume == null)
                    {
                        linkedPlans.RemoveAt(l);
                        linkedPlanCount--;
                        l--;
                        continue;
                    }
                    volume.HeightModified(this);
                }
            }
            else
            {
                if (linkedPlans.Contains(vsubject))
                {
                    _floorHeight = vsubject.floorHeight;
                    _floors = vsubject.floors;
                }
                else
                {
                    baseHeight = vsubject.planTotalHeight;
                }
            }

            int abovePlanCount = abovePlans.Count;
            for (int a = 0; a < abovePlanCount; a++)
            {
	            IVolume volume = abovePlans[a];
                if (volume == null)
                {
                    abovePlans.RemoveAt(a);
                    abovePlanCount--;
                    a--;
                    continue;
                }
                volume.HeightModified(this);
            }
            MarkModified();
        }

        private IBuilding building
        {
            get
            {
                if (_building == null)
                    _building = GetComponent<Building>();
                if (_building == null)
                    _building = GetComponentInParent<Building>();
                return _building;
            }
        }

        #region External Wall Anchors

        //Wall Anchors are used by the interior rooms to anchor to external walls.
        //It ensures that there are places for interior rooms to connect to the wall - no need for intersection calculations
        //It also ensures that the rooms stick to the facade pattern so that interior walls don't cross external portals

        private Dictionary<int, List<Vector2Int>> _externalFacadeWallAnchors = new Dictionary<int, List<Vector2Int>>();
		private Dictionary<Vector2Int, List<int>> _externalWallAnchorsFacades = new Dictionary<Vector2Int, List<int>>();
		private List<Vector2Int> _externalWallAnchors = new List<Vector2Int>();

		public Dictionary<int, List<Vector2Int>> facadeWallAnchors
		{
			get
			{
				if (_externalFacadeWallAnchors == null || _externalFacadeWallAnchors.Count == 0)
					CalculateExternalWallAnchors();
				return _externalFacadeWallAnchors;
			}
		}

		public Dictionary<Vector2Int, List<int>> externalFacadeWallAnchors
		{
			get
			{
				if (_externalWallAnchorsFacades == null || _externalWallAnchorsFacades.Count == 0)
					CalculateExternalWallAnchors();
				return _externalWallAnchorsFacades;
			}
		}

	    private void AddFacadeWallAnchors(Vector2Int point, int facadeIndex)
		{
			if (!_externalWallAnchorsFacades.ContainsKey(point)) _externalWallAnchorsFacades.Add(point, new List<int>());
				_externalWallAnchorsFacades[point].Add(facadeIndex);
		}

		public List<Vector2Int> wallAnchors
        {
            get
            {
                if (_externalWallAnchors == null || _externalWallAnchors.Count == 0)
                    CalculateExternalWallAnchors();
                return _externalWallAnchors;
            }
        }

        public void CalculateExternalWallAnchors()
        {
            if (_externalFacadeWallAnchors == null)
                _externalFacadeWallAnchors = new Dictionary<int, List<Vector2Int>>();
            if (_externalWallAnchorsFacades == null)
				_externalWallAnchorsFacades = new Dictionary<Vector2Int, List<int>>();
            if (_externalWallAnchors == null)
                _externalWallAnchors = new List<Vector2Int>();
            _externalFacadeWallAnchors.Clear();
			_externalWallAnchorsFacades.Clear();
            _externalWallAnchors.Clear();
            int pointCount = _points.Count;
//            int anchorCount = 0;
            for (int p = 0; p < pointCount; p++)
            {

                VolumePoint a = _points[p];
                VolumePoint b = _points[(p + 1) % pointCount];
                Vector2 av = a.position.vector2;
                Vector2 bv = b.position.vector2;

				_externalFacadeWallAnchors.Add(p, new List<Vector2Int>());//new facade entry

				if (IsWallStraight(p))
                {
                    float length = Vector2.Distance(av, bv);
                    int wallSections = Mathf.FloorToInt(length / _minimumWallUnitLength);
                    if (wallSections < 1) wallSections = 1;
                    for (int ws = 0; ws < wallSections + 1; ws++)
                    {
                        float lerp = ws / ((float)wallSections);
                        Vector2Int point = new Vector2Int(Vector2.Lerp(av, bv, lerp));

						_externalFacadeWallAnchors[p].Add(point);
						AddFacadeWallAnchors(point, p);

						if (ws < wallSections)
                            _externalWallAnchors.Add(point);
                    }
                }
                else
                {
                    Vector2 cw0 = a.controlA.vector2;
                    Vector2 cw1 = a.controlB.vector2;
                    Vector2 last = Vector2.zero;
                    float arcLength = 0;
                    for (int t = 0; t < 10; t++)
                    {
                        Vector2 cp = FacadeSpline.Calculate(av, cw0, cw1, bv, t / 9f);
                        if(t > 0)
                            arcLength += Vector2.Distance(cp, last);
                        last = cp;
                    }

                    int wallSections = Mathf.FloorToInt(arcLength / _minimumWallUnitLength);
                    if (wallSections < 1) wallSections = 1;
                    float sectionLength = arcLength / Mathf.Max(wallSections - 1f, 1f);
                    int movement = wallSections * 10;
                    int currentIndex = 0;
                    Vector2 lastCP = av;
                    Vector2 lastA = av;
                    float lastDist = 0;
                    Vector2Int avi = new Vector2Int(av);
                    _externalFacadeWallAnchors[p].Add(avi);
					AddFacadeWallAnchors(avi, p);
					_externalWallAnchors.Add(avi);
                    for (int m = 0; m < movement; m++)
                    {
                        float percent = m / (float)movement;
                        Vector2 cp = FacadeSpline.Calculate(av, cw0, cw1, bv, percent);
                        float dist = Vector2.Distance(lastA, cp);
//                        Debug.Log(dist);
                        if (dist >= sectionLength)
                        {
                            float cpDist = dist - lastDist;
                            float overDist = dist - sectionLength;
                            float targetPercent = 1 - Mathf.Clamp01(overDist / cpDist);
                            Vector2 usePoint = Vector2.Lerp(lastCP, cp, targetPercent);
                            lastA = usePoint;
                            currentIndex++;
                            Vector2Int upi = new Vector2Int(usePoint);
                            _externalFacadeWallAnchors[p].Add(upi);
							AddFacadeWallAnchors(upi, p);
							_externalWallAnchors.Add(upi);
                            if (currentIndex == wallSections - 2)
                                break;
                        }

                        lastCP = cp;
                        lastDist = dist;
                    }
                    Vector2Int bvi = new Vector2Int(bv);
                    _externalFacadeWallAnchors[p].Add(bvi);
					AddFacadeWallAnchors(bvi, p);
					//                    _externalWallAnchors.Add(bvi);
				}
            }
        }
        #endregion

        #endregion

        #region Control Points

        //point space
        public Vector2Int GetControlPointA(int index)
        {
            return _points[index].controlA;
        }

        public void SetControlPointA(int index, Vector2Int value)
        {
            if (_points[index].controlA != value)
            {
                _points[index].controlA = value;
                MarkModified();
            }
        }

        public Vector2Int GetControlPointB(int index)
        {
            return _points[index].controlB;
        }

        public void SetControlPointB(int index, Vector2Int value)
        {
            if (_points[index].controlB != value)
            {
                _points[index].controlB = value;
                MarkModified();
            }
        }


        //building space
        public Vector3 BuildingControlPointA(int index)
        {
            return _points[index].controlA.vector3XZ + Vector3.up * baseHeight;
        }

        public Vector3 BuildingControlPointB(int index)
        {
            return _points[index].controlB.vector3XZ + Vector3.up * baseHeight;
        }

        //world space
        public Vector3 WorldControlPointA(int index)
        {
            return transform.rotation * _points[index].controlA.vector3XZ + Vector3.up * baseHeight + transform.position;
        }

        public Vector3 WorldControlPointB(int index)
        {
            return transform.rotation * _points[index].controlB.vector3XZ + Vector3.up * baseHeight + transform.position;
        }

        #endregion

        #region Facade Entries

        public Facade GetFacade(int index)
        {
            return _points[index].facade;
        }

        public void SetFacade(int index, Facade value)
        {
            Debug.Log("Volume.cs SetFacade(int index,Facade value) index=" + index + "  Facade=" + value);
            if (_points[index].facade != value)
            {
                _points[index].facade = value;
                MarkModified();
            }
        }

        public Facade[] GetAllFacades()
        {
            Facade[] output = new Facade[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                output[i] = this[i].facade;
            return output;
        }

		#endregion

		public Roof roof
		{
			get { return _roof; }
			set
			{
				if (_roof != value || value.modified)
				{
					_roof = value;
					MarkModified();
				}
			}
		}

		public Surface undersideSurafce
		{
			get { return _undersideSurafce; }
			set
			{
				if (_undersideSurafce != value)
				{
					_undersideSurafce = value;
					MarkModified();
				}
			}
		}

		public bool isLegal
        {
            get
            {
                for (int i = 0; i < numberOfPoints; i++)
                    if (this[i].illegal) return false;
                return true;
            }
        }

        public Vector2Int[] illegalPoints
        {
            get
            {
                Vector2Int[] output = new Vector2Int[numberOfPoints];
                for (int i = 0; i < numberOfPoints; i++)
                    if (this[i].illegal) output[i] = this[i].position;
                return output;
            }
        }

        public int numberOfFacades
        {
            get { return _points.Count; }
        }

        public float baseHeight
        {
            get { return _baseHeight; }
            set { _baseHeight = value; }
        }

        public float floorHeight
        {
            get
            {
                return _floorHeight;
            }
            set
            {
                if (_floorHeight != value)
                {
                    _floorHeight = value;
                    HeightModified(this);
                    MarkModified();
                }
            }
        }

        //TODO obsolete this
        public float planHeight { get { return _floors * _floorHeight; } }

        public float volumeHeight { get { return _floors * _floorHeight; } }

        //TODO obsolete this
        public float planTotalHeight { get { return _floors * _floorHeight + baseHeight; } }

        public float volumeTotalHeight { get { return _floors * _floorHeight + baseHeight; } }

        public float CalculateHeight(int floor)
        {
            return _baseHeight + floor * _floorHeight;
        }

        public float CalculateFloorHeight(int floor)
        {
            return _baseHeight + floor * _floorHeight;
        }

        public float CalculateHeight(IFloorplan floorplan)
        {
            Floorplan fPlan = floorplan as Floorplan;
            return CalculateHeight(_interiorFloorplans.IndexOf(fPlan));
        }

        public float minimumWallUnitLength
        {
            get { return _minimumWallUnitLength; }
            set
            {
                if (_minimumWallUnitLength != value)
                {
                    _minimumWallUnitLength = value;
                    MarkModified();
                }
            }
        }

        public bool IsWallStraight(int index)
        {
            return _points[index].IsWallStraight();
        }

        public int floors
        {
            get
            {
                return _floors;
            }
            set
            {
                if (float.IsNaN(value))
                    return;
                value = Mathf.Max(value, 1);
                if (_floors != value)
                {
                    _floors = value;
                    for(int av = 0; av < abovePlans.Count; av++)
                    {
                        if(abovePlans[av] == null)
                        {
                            abovePlans.RemoveAt(av);
                            av--;
                        }
                    }
                    HeightModified(this);
                    CheckInternalFloorplans();
                    MarkModified();
                }
            }
        }


        public float wallThickness
        {
            get
            {
                return _wallThickness;
            }
            set
            {
                if (_wallThickness != value)
                {
                    _wallThickness = value;
                    MarkModified();
                }
            }
        }

        public IVolume Clone()
        {
            HUtils.log();

            Volume output = Create(transform.parent);
            List<VolumePoint> newPoints = new List<VolumePoint>();
            foreach (VolumePoint point in _points)
                newPoints.Add(point.Clone());
            output.Initialise(newPoints, 1, _floorHeight);
            ((Object)output).name = ((Object)this).name + "_copy";
            output.baseHeight = _baseHeight + _floorHeight;
            output.MarkModified();
            return output;
        }

        public List<VolumePoint> GetModifiedPoints()
        {
            HUtils.log();
            List<VolumePoint> output = new List<VolumePoint>();
            for (int i = 0; i < numberOfPoints; i++)
                if (this[i].modified) output.Add(_points[i]);
            return output;
        }

        public List<VolumePoint> GetMovedPoints()
        {
            HUtils.log();
            
            List<VolumePoint> output = new List<VolumePoint>();
            for(int i = 0; i < numberOfPoints; i++)
            {
                if(this[i].moved)
                {
                    bool canMakeMove = true;
                    for (int j = 0; j < numberOfPoints; j++)
                    {
                        if(i==j)continue;
                        if(this[i].position == this[j].position)
                        {
                            canMakeMove = false;
                            break;
                        }
                    }
                    if(canMakeMove)
                        output.Add(_points[i]);
                    else
                        _points[i].MoveBack();
                }
            }
            return output;
        }

        public Bounds bounds { get { return _bounds; } }

        public IVisualPart visualPart { get {return _visualPart;} }

        public GameObject prefabs
        {
            get
            {
                if(_prefabs == null)
                {
                    _prefabs = new GameObject("Prefabs");
#if UNITY_EDITOR
                    UnityEditor.Undo.RegisterCreatedObjectUndo(_prefabs, "Created Volume Prefab GameObject");
                    UnityEditor.Undo.SetTransformParent(_prefabs.transform, transform, "Parent New Volume Prefab GameObject");
#endif
                }
                return _prefabs;
            }
        }
        
        public bool IsGabled()
        {
            int facaadeCount = numberOfFacades;
            for(int f = 0; f < facaadeCount; f++)
                if(this[f].isGabled) return true;
            return false;
        }

#endregion

        public void AddPoint(Vector2Int newPosition)
        {
            HUtils.log();
            Debug.Log("Volume.cs AddPoint(Vector2Int newPosition) newPosition=(" + newPosition.x + "," + newPosition.y+ ")");
            VolumePoint newPoint = new VolumePoint(newPosition);
            _points.Add(newPoint);
            CheckVolume();
            MarkModified();
        }

        public void AddPoint(Vector3 newPoint)
        {
            HUtils.log();
            Debug.Log("Volume.cs AddPoint(Vector3 newPoint)( " + newPoint.x + ","+ newPoint.y +"," + newPoint.z + " )");
            AddPoint(new Vector2Int(newPoint, true));
        }

        public void AddPoints(Vector2Int[] newPoints)
        {
            if (!BuildrPolyClockwise.Check(newPoints))
                Array.Reverse(newPoints);
            for (int i = 0; i < newPoints.Length; i++) AddPoint(newPoints[i]);
            CheckVolume();
            MarkModified();
        }

        public void AddPoints(Vector3[] newPoints)
        {
            HUtils.log();

            for (int i = 0; i < newPoints.Length; i++) AddPoint(newPoints[i]);
            CheckVolume();
            MarkModified();
        }

        public void RemovePoint(Vector2Int point)
        {
            if (numberOfPoints < 4)
                return;
            int index = -1;
            for (int i = 0; i < numberOfPoints; i++)
                if (this[i].position == point)
                {
                    index = i;
                    break;
                }
            if (index != -1)
            {
                RemovePointAt(index);
                CheckVolume();
                MarkModified();
            }
        }

        public void RemovePointAt(int index)
        {
            if (numberOfPoints < 4)
                return;
            _points.RemoveAt(index);
            CheckVolume();
            MarkModified();
        }

        public void InsertPoint(int index, Vector2Int point)
        {
            HUtils.log();
            _points.Insert(index, new VolumePoint(point));
            MarkModified();
        }

        public bool Contains(Vector2Int point)
        {
            int pointCount = numberOfPoints;
            for(int p = 0; p < pointCount; p++)
                if(_points[p].position == point) return true;
            return false;
		}

	    public void AddAbovePlan(IVolume volume)
	    {
            Volume vol  = volume as Volume;
	        abovePlans.Add(vol);
	    }

	    public List<IVolume> AbovePlanList()
	    {
	        if (abovePlans == null) abovePlans = new List<Volume>();
	        List<IVolume> output = new List<IVolume>();
	        for (int l = 0; l < abovePlanCount; l++) output.Add(abovePlans[l]);
	        return output;
        }

	    public bool ContainsPlanAbove(IVolume volume) {
	        Volume vol = volume as Volume;
            return abovePlans.Contains(vol);
	    }

		public void AddLinkPlan(IVolume volume) {
		    Volume vol = volume as Volume;
            linkedPlans.Add(vol);
	    }

	    public List<IVolume> LinkPlanList()
	    {
		    if (linkedPlans == null) linkedPlans = new List<Volume>();
	        List<IVolume> output = new List<IVolume>();
	        for(int l = 0; l < linkPlanCount; l++) output.Add(linkedPlans[l]);
            return output;
	    }

	    public bool IsLinkedPlan(IVolume volume) {
	        Volume vol = volume as Volume;
            return linkedPlans.Contains(vol);
		}

	    public int abovePlanCount
	    {
		    get { return abovePlans.Count; }
		}

	    public int linkPlanCount
	    {
		    get { return linkedPlans.Count; }
	    }

		#region Private

		public void CheckVolume()
        {
            CalculateExternalWallAnchors();
            PlanLegalityCheck();
            CalculatePlanBounds();

            List<VolumePoint> modifiedPoints = GetModifiedPoints();
            for (int p = 0; p < modifiedPoints.Count; p++)
                CheckInternalPointMovement(modifiedPoints[p]);
        }

        public virtual bool isModified
        {
            get
            {
                if (_isModified) return true;
                for (int p = 0; p < numberOfPoints; p++)
                    if (this[p].modified) return true;
                for(int f = 0; f < floors; f++)
                    if(_interiorFloorplans != null && _interiorFloorplans.Count > f && _interiorFloorplans[f] != null)
                    if (_interiorFloorplans[f].isModified) return true;
                return false;
            }
        }

        public virtual void MarkModified()
        {
            if(!BuildRSettings.AUTO_UPDATE) return;
            CheckVolume();
            foreach(Floorplan floorplan in _interiorFloorplans)
                floorplan.CalculatePlanWalls(this);
            SaveData();
            _isModified = true;
        }

        private void SaveData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);

            Building building = transform.GetComponentInParent<Building>();
            if (building != null)
                building.MarkModified();
#endif
        }

        public void MarkUnmodified()
        {
            _isModified = false;
            for (int p = 0; p < numberOfPoints; p++)
                this[p].MarkUnmodified();
            _roof.MarkUnmodified();
            for(int f = 0; f < floors; f++)
                if (_interiorFloorplans != null && _interiorFloorplans.Count > f && _interiorFloorplans[f] != null)
                    _interiorFloorplans[f].MarkUnmodified();
        }

        private void CalculatePlanBounds()
        {
            _bounds.size = Vector3.zero;
            if (numberOfPoints == 0)
            {
                _bounds.center = Vector3.zero;
                return;
            }

            Vector3 baseElevation = Vector3.up * _baseHeight;
            Vector3 planElevation = Vector3.up * planHeight;
            _bounds.center = _points[0].position.vector3XZ + baseElevation;

            int pointCount = _points.Count;
            for (int p = 0; p < pointCount; p++)
            {
                Vector3 lower = _points[p].position.vector3XZ + baseElevation;
                Vector3 upper = lower + planElevation;
                _bounds.Encapsulate(lower);
                _bounds.Encapsulate(upper);
            }
        }

        public bool PlanLegalityCheck()
        {
            for (int p = 0; p < numberOfPoints; p++)
                this[p].illegal = false;
            bool output = false;
            for (int indexAA = 0; indexAA < numberOfPoints; indexAA++)
            {
                Vector2Int p0 = _points[indexAA].position;
                int indexAB = (indexAA + 1) % numberOfPoints;
                Vector2Int p1 = _points[indexAB].position;
                for (int indexBA = 0; indexBA < numberOfPoints; indexBA++)
                {
                    if (indexAA == indexBA) continue;//skip testing wall on itself
                    int indexBB = (indexBA + 1) % numberOfPoints;
                    if (indexAA == indexBB) continue;//skip testing wall on itself

                    Vector2Int p2 = _points[indexBA].position;
                    Vector2Int p3 = _points[indexBB].position;


                    if(BuildrUtils.PointOnLine(p0, p2, p3))
                    {
                        _points[indexAA].illegal = true;
                        _points[indexBA].illegal = true;
                        output = true;
                        continue;
                    }
                    
                    if (indexAA == indexBB || indexAB == indexBA || indexAB == indexBB) continue;//don't test lines that connect

                    if (BuildrUtils.FastLineIntersection(p0, p1, p2, p3))
                    {
                        _points[indexAA].illegal = true;
                        _points[indexBA].illegal = true;
                        output = true;
                    }
                }
            }
            return output;
        }

        private void CheckInternalPointMovement(VolumePoint modifiedPoint)
        {
            int pointCount = numberOfPoints;
            for (int p = 0; p < pointCount; p++)
            {
                VolumePoint point = _points[p];
                if (modifiedPoint == point) continue;
                if (modifiedPoint.lastPosition == point.position)
                {
                    point.position = modifiedPoint.position;
                    _points[p].MarkUnmodified();
                }
            }
        }

        private void CheckInternalFloorplans()
        {
            if(!building.generateInteriors)
                return;
            int currentPlanCount = _interiorFloorplans.Count;
            if(currentPlanCount != _floors)
            {
                while(currentPlanCount < _floors)
                {
                    _interiorFloorplans.Add(null);
                    currentPlanCount++;
                }
                while(currentPlanCount > _floors)
                {
	                Floorplan floorplan = _interiorFloorplans[currentPlanCount - 1];
                    _interiorFloorplans.Remove(floorplan);
#if UNITY_EDITOR
                    if(floorplan != null)
                        UnityEditor.Undo.DestroyObjectImmediate(floorplan.gameObject);
#else
                if(floorplan != null)
                    Destroy(floorplan.gameObject);
#endif
                    currentPlanCount--;
                }
            }
            for(int i = 0; i < currentPlanCount; i++)
            {
                if(_interiorFloorplans[i] == null)
                {
                    Floorplan floorplan = Floorplan.Create(transform);
                    floorplan.SetDefaultName(i + 1);
                    _interiorFloorplans[i] = floorplan;
                }
            }
        }
#endregion

        
        public static Volume Create(Transform transform)
        {
            HUtils.log();
            Debug.Log("Create(Transform transform) transform=" + transform.name);
#if UNITY_EDITOR
            Debug.Log("这里是Volume.cs UNITY_EDITOR存在");
            GameObject newVolumeGO = new GameObject("Volume");
            UnityEditor.Undo.RegisterCreatedObjectUndo(newVolumeGO, "Created Volume GameObject");
            UnityEditor.Undo.SetTransformParent(newVolumeGO.transform, transform, "Parent New Volume GameObject");
            newVolumeGO.transform.localPosition = Vector3.zero;
            newVolumeGO.transform.localRotation = Quaternion.identity;

            Volume output = UnityEditor.Undo.AddComponent<Volume>(newVolumeGO);
            output._visualPart = VisualPart.Create(newVolumeGO.transform, "Volume Visual");

            output._prefabs = new GameObject("Prefabs");
            UnityEditor.Undo.RegisterCreatedObjectUndo(output._prefabs, "Created Volume Prefab GameObject");
            UnityEditor.Undo.SetTransformParent(output._prefabs.transform, newVolumeGO.transform, "Parent New Volume Prefab GameObject");

            output.CheckInternalFloorplans();
            
            return output;
#else
            Debug.Log("这里是Volume.cs UNITY_EDITOR不存在");

            GameObject newFloorplanGo = new GameObject("Volume");
            newFloorplanGo.transform.parent = transform;
           
            Volume output = newFloorplanGo.AddComponent<Volume>();
            output._visualPart = VisualPart.Create(newFloorplanGo.transform, "Volume Visual");

            return output;
#endif
        }
    }
}
