using System.Collections.Generic;
using JaspLib;
using UnityEngine;

namespace BuildR2 {
    public abstract class BasePlot : IPlot
    {
        public enum SubdivisionModes
        {
            OBB,
            StraightSkeleton,
            Random
        }

        public Vector2 this[int index] { get { return _pointsV2[index]; } }
        public Vector2[] getAllPointsV2 { get { return _pointsV2; } }
        public Vector2[] pointsV2 { get { return _pointsV2; } }
        public Vector2[][] boundaryPoints { get { return _boundaryPoints; } }
        public int numberOfEdges { get { return _pointsV2.Length; } }
        public float area { get { return _area; } }
        public bool[] externals { get { return _externals; } }
        public Vector2 bounds { get { return _flatBounds.size; } }
        public FlatBounds flatbounds { get { return _flatBounds; } }
        public Vector2 center { get { return _flatBounds.center; } }
        public float longestExternalAccess { get { return _longestExternalAccess; } }
        public float plotCircumference { get { return _plotCircumference; } }
        public float plotAccessPercentage { get { return _plotAccessPercentage; } }
        public string notes { get { return _notes; } set { _notes = value; } }

        public SplitSettings splitSettings
        {
            get { return _splitSettings; }
            set { _splitSettings = value; }
        }

        [SerializeField]
        protected float _plotCircumference = 1f;
        [SerializeField]
        protected float _longestExternalAccess = 1f;
        [SerializeField]
        protected float _plotAccessPercentage = 1f;
        [SerializeField]
        protected int _pointLength;
        [SerializeField]
        protected Vector2[] _pointsV2;
        [SerializeField]
        protected Vector2[][] _boundaryPoints;
        [SerializeField]
        protected bool[] _externals;
        [SerializeField]
        protected float _area = 0;

        [SerializeField]
        protected string _notes = "";

        [SerializeField]
        protected FlatBounds _flatBounds = new FlatBounds();

        [SerializeField]
        protected SplitSettings _splitSettings;

        public BasePlot()
        {
            _pointsV2 = new Vector2[0];
            _boundaryPoints = new Vector2[0][];
            _externals = new bool[0];
            Init(_pointsV2);
        }

        public BasePlot(Vector2Int[] newPoints, float minimumBoundaryPointDistance = 0)
        {
            Init(newPoints);
            _splitSettings = new SplitSettings(minimumBoundaryPointDistance);
            CalculateBoundaryPoints();
        }

        public BasePlot(List<Vector2Int> newNodes, float minimumBoundaryPointDistance = 0)
        {
            Init(newNodes);
            _splitSettings = new SplitSettings(minimumBoundaryPointDistance);
            CalculateBoundaryPoints();
        }

        public BasePlot(Vector2[] newNodes, float minimumBoundaryPointDistance = 0)
        {
            Init(newNodes);
            _splitSettings = new SplitSettings(minimumBoundaryPointDistance);
            CalculateBoundaryPoints();
        }

        public BasePlot(List<Vector2> newNodes, float minimumBoundaryPointDistance = 0)
        {
            _pointsV2 = new Vector2[0];
            _externals = new bool[0];
            Init(newNodes);
            _splitSettings = new SplitSettings(minimumBoundaryPointDistance);
            CalculateBoundaryPoints();
        }

        protected void Init(List<Vector2Int> newNodes) {
            _pointLength = newNodes.Count;
            _pointsV2 = new Vector2[_pointLength];
            _externals = new bool[_pointLength];
            for (int n = 0; n < _pointLength; n++)
                _pointsV2[n] = newNodes[n].vector2;
        }

        protected void Init(Vector2Int[] newNodes) {
            _pointLength = newNodes.Length;
            _pointsV2 = new Vector2[_pointLength];
            _externals = new bool[_pointLength];
            for (int n = 0; n < _pointLength; n++)
                _pointsV2[n] = newNodes[n].vector2;
        }

        protected void Init(Vector2[] newNodes)
        {
            _pointLength = newNodes.Length;
            _pointsV2 = newNodes;
        }

        protected void Init(List<Vector2> newNodes) {
            _pointLength = newNodes.Count;
            _pointsV2 = new Vector2[_pointLength];
            _externals = new bool[_pointLength];
            for (int n = 0; n < _pointLength; n++)
                _pointsV2[n] = newNodes[n];
        }

        public void CalculateBoundaryPoints()
        {
            if (_splitSettings.minimumBoundaryPointDistance < Mathf.Epsilon) return;

            int boundaryCount = _pointsV2.Length;
            _boundaryPoints = new Vector2[boundaryCount][];
            for (int b = 0; b < boundaryCount; b++)
            {
                Vector2 p0 = pointsV2[b];
                Vector2 p1 = pointsV2[b < boundaryCount - 1 ? b + 1 : 0];
                float distance = Vector2.Distance(p0, p1);
                int boundaryPointCount = Mathf.CeilToInt(distance / _splitSettings.minimumBoundaryPointDistance) + 1;
                _boundaryPoints[b] = new Vector2[boundaryPointCount];
                for (int bp = 0; bp < boundaryPointCount; bp++)
                {
                    float percent = bp / (boundaryPointCount - 1f);
                    _boundaryPoints[b][bp] = (Vector2.Lerp(p0, p1, percent));
                }
            }
        }

        public virtual bool HasExternalAccess()
        {
            return true;
        }

        public void DebugDraw(Color col)
        {
            int pointCount = pointsV2.Length;
            for (int i = 0; i < pointCount; i++)
            {
                int ib = (i + 1) % pointCount;
                Debug.DrawLine(JMath.ToV3(pointsV2[i]), JMath.ToV3(pointsV2[ib]), col);
            }
        }
    }
}