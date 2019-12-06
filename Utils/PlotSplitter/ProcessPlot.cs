using System.Collections.Generic;

namespace BuildR2 {
    public struct ProcessPlot
    {
        public IPlot plot;
        public List<OBBox> obbs;
        public bool shortSplit;
        public bool longSplit;
        public bool variationA;
        public bool variationB;
        public float variationAmount;

        public ProcessPlot(IPlot newPlot)
        {
            plot = newPlot;
            obbs = new List<OBBox>();
            shortSplit = true;
            longSplit = false;
            variationA = false;
            variationB = false;
            variationAmount = 0.5f;
        }

        public ProcessPlot(IPlot newPlot, List<OBBox> obbs)
        {
            plot = newPlot;
            this.obbs = obbs;
            shortSplit = true;
            longSplit = false;
            variationA = false;
            variationB = false;
            variationAmount = 0.5f;
        }

        public override string ToString()
        {
            return string.Format("PlotSplitter.ProcessPlot {0}m2 {1} {2} {3} {4}", plot.area, shortSplit, longSplit, variationA, variationB);
        }
    }
}