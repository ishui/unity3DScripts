using System;

namespace BuildR2 {
    [Serializable]
    public class SplitSettings
    {
        public float mergeThreashold;
        public float minArea;
        public float maxArea;
        public bool autoArea;
        public float autoAreaRatio;
        public float variation;
        public float minimumAspect;
        public float minimumAccessLengthPercent;
        public bool allowTrianglularPlots;
        public float randomTerminationChance;
        public bool useBoundaryMergeOnExternalsOnly;
        public float minimumBoundaryPointDistance;
        public bool accurateAreaCalculation;

        public bool fallbackSecondaryDivision;
        public bool fallbackVariations;
        public bool fallbackAlternativeObb;

        public bool debug;
        public bool log;

        public SplitSettings(float minimumBoundaryPointDistance)
        {
            mergeThreashold = 1.0f;
            minArea = 1.0f;
            maxArea = 10.0f;
            autoArea = false;
            autoAreaRatio = 0.1f;
            variation = 0.5f;
            minimumAspect = 0.5f;
            minimumAccessLengthPercent = 0.0f;
            allowTrianglularPlots = false;
            randomTerminationChance = 0.01f;
            useBoundaryMergeOnExternalsOnly = true;
            this.minimumBoundaryPointDistance = minimumBoundaryPointDistance;

            fallbackSecondaryDivision = true;
            fallbackVariations = false;
            fallbackAlternativeObb = true;
            accurateAreaCalculation = false;

            debug = false;
            log = false;
        }
    }
}