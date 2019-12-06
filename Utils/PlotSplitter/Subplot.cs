using System;
using System.Collections.Generic;
using JaspLib;
using UnityEngine;

namespace BuildR2
{
    [Serializable]
    public class Subplot : BasePlot
    {
        public Subplot(Vector2[] newPoints, SplitSettings splitSettings, bool[] externals = null) : base(newPoints) 
            {
            if (externals != null)
                _externals = externals;
            else
                _externals = new bool[_pointsV2.Length];

            _splitSettings = splitSettings;

            CalculateSubplotValues();
        }

        public Subplot(List<Vector2> newPoints, SplitSettings splitSettings, List<bool> externals = null) : base(newPoints)
        {

            if(externals != null) {
                int dataSize = newPoints.Count;
                bool[] ext = new bool[dataSize];
                for (int i = 0; i < dataSize; i++)
                    ext[i] = externals[i];
                _externals = ext;
            }
            else
                _externals = new bool[_pointsV2.Length];

            _splitSettings = splitSettings;

            CalculateSubplotValues();
        }

        public Vector2[] Edge(int index)
        {
            Vector2[] output = new Vector2[2];
            output[0] = _pointsV2[index];
            output[1] = _pointsV2[(index + 1) % _pointLength];
            return output;
        }

        public bool IsRoadEdge(int index)
        {
            if (externals.Length == 0)
                return false;
            return externals[index];
        }

        protected void CalculateSubplotValues()
        {
            int pointLength = numberOfEdges;
            if (pointLength == 0)
                return;
            
            for (int p = 0; p < pointLength; p++)
            {
                Vector2 v2Point = _pointsV2[p];
                _flatBounds.Encapsulate(v2Point);
            }

            _area = JMath.PolyAreaQuick(_pointsV2);

            _plotCircumference = 0;
            float currentLongestExternalAccess = 0;
            _longestExternalAccess = 0;
            bool canLoop = false;
            for (int i = 0; i < pointLength * 2; i++)
            {
                int p = i % pointLength;
                int p2 = (p + 1) % pointLength;
                Vector2 pV0 = _pointsV2[p];
                Vector2 pV1 = _pointsV2[p2];
                float pointDistance = Vector2.SqrMagnitude(pV0 - pV1);
                _plotCircumference += pointDistance;
                if (externals[p])
                {
                    currentLongestExternalAccess += pointDistance;
                }
                else
                {
                    if (currentLongestExternalAccess > _longestExternalAccess)
                        _longestExternalAccess = currentLongestExternalAccess;
                    currentLongestExternalAccess = 0;
                    canLoop = true;
                }

                if (i >= pointLength && !externals[p])
                    break;

                if (i >= pointLength && externals[p] && !canLoop)
                    break;
            }
            //final check
            if (currentLongestExternalAccess > _longestExternalAccess)
                _longestExternalAccess = currentLongestExternalAccess;

            _plotAccessPercentage = _longestExternalAccess / _plotCircumference;

            int plotPointSize = _pointsV2.Length;
            if (plotPointSize == 0)
                return;

            _flatBounds.Clear();
            for (int p = 0; p < plotPointSize; p++)
                _flatBounds.Encapsulate(_pointsV2[p]);
            if(_splitSettings.accurateAreaCalculation)
                _area = JMath.PolyArea(_pointsV2);
            else
                _area = JMath.PolyAreaQuick(_pointsV2);

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
    }
}