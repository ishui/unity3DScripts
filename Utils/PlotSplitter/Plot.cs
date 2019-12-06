using System;
using System.Collections.Generic;
using JaspLib;
using UnityEngine;

namespace BuildR2
{
    [Serializable]
    public class Plot : BasePlot
    {
        [SerializeField]
        protected uint _seed;
        [SerializeField]
        protected List<IPlot> _subPlots = new List<IPlot>();
        [SerializeField]
        protected RandomGen _rdn = new RandomGen(1);

        public Plot(uint seed) : base()
        {
            _subPlots.Clear();
            _rdn.Reset();
            SetSeed(seed);
        }

        public Plot(uint seed, Vector2Int[] newNodes, float minimumBoundaryPointDistance = 0) : base(newNodes, minimumBoundaryPointDistance)
        {
            _subPlots.Clear();
            _rdn.Reset();
            SetSeed(seed);
            Init(newNodes);
            CalculateValues();
            _splitSettings.minimumBoundaryPointDistance = minimumBoundaryPointDistance;
            CalculateBoundaryPoints();
        }

        public Plot(uint seed, List<Vector2Int> newNodes, float minimumBoundaryPointDistance = 0) : base(newNodes, minimumBoundaryPointDistance)
        {
            _subPlots.Clear();
            _rdn.Reset();
            SetSeed(seed);
            Init(newNodes.ToArray());
            CalculateValues();
            _splitSettings.minimumBoundaryPointDistance = minimumBoundaryPointDistance;
            CalculateBoundaryPoints();
        }

        public Plot(uint seed, Vector2[] newNodes, float minimumBoundaryPointDistance = 0) : base(newNodes, minimumBoundaryPointDistance)
        {
            _subPlots.Clear();
            _rdn.Reset();
            SetSeed(seed);
            Init(newNodes);
            CalculateValues();
            _splitSettings.minimumBoundaryPointDistance = minimumBoundaryPointDistance;
            CalculateBoundaryPoints();
        }

        public Plot(uint seed, List<Vector2> newNodes, float minimumBoundaryPointDistance = 0) : base(newNodes, minimumBoundaryPointDistance)
        {
            _subPlots.Clear();
            _rdn.Reset();
            SetSeed(seed);
            Init(newNodes.ToArray());
            CalculateValues();
            _splitSettings.minimumBoundaryPointDistance = minimumBoundaryPointDistance;
            CalculateBoundaryPoints();
        }

        public void Split()
        {
            if (numberOfEdges == 0) return;
            PlotSplitter plotSplitter = new PlotSplitter();
            plotSplitter.Execute(this, _seed);
            if (plotSplitter.plots.Count > 1)
                _subPlots.AddRange(plotSplitter.plots);
            if(_splitSettings.debug)
                plotSplitter.OutputNotes();
        }

        public override bool HasExternalAccess()
        {
            return true;
        }

        public IPlot[] getSubplots
        {
            get { return _subPlots.ToArray(); }
        }

        public void ClearSubPlots()
        {
            _subPlots.Clear();
        }

        public virtual RandomGen random
        {
            get { return _rdn; }
        }

        public virtual uint seed
        {
            get { return _seed; }
            set
            {
                if (value != _seed)
                {
                    SetSeed(value);
                }
            }
        }

        protected void CalculateValues()
        {
            if (_pointsV2.Length == 0)
                return;
            _externals = new bool[_pointLength];

            int pointCount = _pointsV2.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 v2Point = _pointsV2[p];
                _flatBounds.Encapsulate(v2Point);
                _externals[p] = true;
            }

            if (_splitSettings.accurateAreaCalculation)
                _area = JMath.PolyArea(_pointsV2);
            else
                _area = JMath.PolyAreaQuick(_pointsV2);
//            _area = JMath.PolyArea(_pointsV2);
        }

        private void SetSeed(uint newSeed)
        {
            _seed = newSeed;
            if (_rdn == null) _rdn = new RandomGen(newSeed);
            else _rdn.seed = newSeed;
        }

        public static RawMeshData GeneratePlot(IPlot plot)
        {

            int pointSize = plot.numberOfEdges;
            int[] sortedIndexes = BuildrUtils.SortPointByAngle(plot.pointsV2, plot.center);
            Vector2[] sortedPoints = new Vector2[pointSize];
            for (int i = 0; i < pointSize; i++)
            {
                sortedPoints[i] = plot.pointsV2[sortedIndexes[i]];
            }
            bool clockwise = Clockwise(sortedPoints);

            int vertCount = pointSize + 1;
            int triCount = pointSize * 3;

            RawMeshData output = new RawMeshData(vertCount, triCount);
            for (int v = 0; v < vertCount - 1; v++)
            {
                output.vertices[v] = new Vector3(sortedPoints[v].x, 0, sortedPoints[v].y);
            }
            output.vertices[vertCount - 1] = new Vector3(plot.center.x, 0, plot.center.y);

            int vertIndex = 0;
            for (int t = 0; t < triCount; t += 3)
            {
                output.triangles[t + 0] = vertCount - 1;
                if (clockwise)
                {
                    output.triangles[t + 1] = vertIndex;
                    output.triangles[t + 2] = (vertIndex < vertCount - 2) ? vertIndex + 1 : 0;
                }
                else
                {
                    output.triangles[t + 1] = (vertIndex < vertCount - 2) ? vertIndex + 1 : 0;
                    output.triangles[t + 2] = vertIndex;
                }
                vertIndex++;
            }

            return output;
        }

        private static bool Clockwise(Vector2[] points)
        {
            int pointCount = points.Length;
            float value = 0;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = points[p];
                int pb = p + 1;
                if (pb == pointCount) pb = 0;
                Vector2 p1 = points[pb];
                value += (p1.x - p0.x) * (p1.y + p0.y);//(x2 − x1)(y2 + y1)
            }
            return value > 0;
        }
    }
}