using System.Collections.Generic;
using System.Text;
using JaspLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BuildR2
{
    public class PlotSplitter
    {
        //final plots to use after division
        public List<IPlot> plots = new List<IPlot>();

        private Plot _parent;
        private OBBFit obbFit = new OBBFit();
        private RandomGen rGen;

        public List<ProcessPlot> processPlots = new List<ProcessPlot>();
        public List<DebugSplitInfo> debug = new List<DebugSplitInfo>();

        public void Execute(Plot plot, uint seed)
        {
            processPlots.Clear();
            plots.Clear();
            debug.Clear();

            int plotSize = plot.numberOfEdges;
            bool[] plotExternals = new bool[plotSize];
            for (int p = 0; p < plotSize; p++)
                plotExternals[p] = true;
            _parent = plot;
            ProcessPlot initialPlot = new ProcessPlot(_parent, obbFit.CreateSorted(plot.pointsV2));
            processPlots.Add(initialPlot);
            float initialArea = initialPlot.obbs[0].area;

            if(plot.splitSettings.autoArea)
            {
                FlatBounds pBounds = new FlatBounds();
                pBounds.Encapsulate(plot.getAllPointsV2);
                plot.splitSettings.minArea = Mathf.Min(pBounds.size.x, pBounds.size.y) * plot.splitSettings.autoAreaRatio;
                plot.splitSettings.maxArea = Mathf.Max(pBounds.size.x, pBounds.size.y) * plot.splitSettings.autoAreaRatio;
            }

            if (initialArea < plot.splitSettings.maxArea)// if the supplied plot is already small enough - return it
            {
                plots.Add(_parent);
                processPlots.Clear();
//                Debug.Log("Plot size (" + initialArea + ") below max area " + plot.splitSettings.maxArea);
                return;
            }

            rGen = new RandomGen(seed);

            int it = 0;
            while (processPlots.Count > 0)
            {
                ProcessPlot processPlot = processPlots[0];
                IPlot currentPlot = processPlot.plot;
                processPlots.RemoveAt(0);
                Subplot[] newPlots = SplitPlot(processPlot);

                bool earlyTermination = newPlots[0] == null;
                if (newPlots[1] == null) earlyTermination = true;
                if (rGen.output < plot.splitSettings.randomTerminationChance) earlyTermination = true;
                Subplot plotA = null, plotB = null;
                List<OBBox> obbsA = null;
                List<OBBox> obbsB = null;
                if (!earlyTermination)
                {
                    plotA = newPlots[0];
                    plotB = newPlots[1];

                    if (plotA.plotAccessPercentage < plot.splitSettings.minimumAccessLengthPercent)
                    {
                        if(plot.splitSettings.log)
                            plotA.notes = "insufficient access";
                        earlyTermination = true;
                    }
                    if (plotB.plotAccessPercentage < plot.splitSettings.minimumAccessLengthPercent)
                    {
                        if (plot.splitSettings.log)
                        plotB.notes = "insufficient access";
                        earlyTermination = true;
                    }
                    if (plotA.numberOfEdges < 4 && !plot.splitSettings.allowTrianglularPlots)
                    {
                        if (plot.splitSettings.log)
                        plotA.notes = "triangular split";
                        earlyTermination = true;
                    }
                    if (plotB.numberOfEdges < 4 && !plot.splitSettings.allowTrianglularPlots)
                    {
                        plotB.notes = "triangular split";
                        earlyTermination = true;
                    }

                    obbsA = obbFit.CreateSorted(plotA.pointsV2);
                    if (obbsA.Count == 0)
                    {
                        if (plot.splitSettings.log)
                        plotA.notes = "no obb generated";
                        earlyTermination = true;
                    }
                    else if (obbsA[0].aspect < plot.splitSettings.minimumAspect)
                    {
                        if (plot.splitSettings.log)
                        plotA.notes = "aspect issue";
                        earlyTermination = true;
                    }
                    else if (obbsA[0].area < plot.splitSettings.minArea)
                    {
                        if (plot.splitSettings.log)
                        plotA.notes = "area smaller than minimum";
                        earlyTermination = true;
                    }

                    obbsB = obbFit.CreateSorted(plotB.pointsV2);
                    if (obbsB.Count == 0)
                    {
                        if (plot.splitSettings.log)
                        plotB.notes = "no obb generated";
                        earlyTermination = true;
                    }
                    else if (obbsB[0].aspect < plot.splitSettings.minimumAspect)
                    {
                        if (plot.splitSettings.log)
                        plotB.notes = "aspect issue";
                        earlyTermination = true;
                    }
                    else if (obbsB[0].area < plot.splitSettings.minArea)
                    {
                        if (plot.splitSettings.log)
                        plotB.notes = "area smaller than minimum";
                        earlyTermination = true;
                    }

                }

                if (earlyTermination)
                {
                    if(plotA != null && plotB != null)
                    {
                        if (plot.splitSettings.log)
                        currentPlot.notes = string.Format("plotA:{0}  plotB:{1}  {2}", plotA.notes, plotB.notes, processPlot);

                        if(plot.splitSettings.debug)//output debug info
                        {
                            DebugSplitInfo info = new DebugSplitInfo();
                            info.plot = currentPlot;
                            info.plotA = plotA;
                            info.plotB = plotB;
                            debug.Add(info);
                        }
                    }
                    //figure on appropirate fallback
                    if(!processPlot.longSplit && plot.splitSettings.fallbackSecondaryDivision)//divide along the longer split
                    {
                        processPlot.longSplit = true;
                        processPlots.Insert(0, processPlot);
                    }
                    else
                    {
                        if(!processPlot.variationA && plot.splitSettings.fallbackVariations)//use a variation on the split
                        {
                            processPlot.variationA = true;
                            processPlots.Insert(0, processPlot);
                        }
                        else
                        {
                            if(!processPlot.variationB && plot.splitSettings.fallbackVariations)//use a variation on the split
                            {
                                processPlot.variationB = true;
                                processPlots.Insert(0, processPlot);
                            }
                            else
                            {
                                if(processPlot.obbs.Count > 1 && plot.splitSettings.fallbackAlternativeObb)//if there are other cut options - use them!
                                {
                                    processPlot.obbs.RemoveAt(0);
                                    processPlot.longSplit = false;
                                    processPlot.variationA = false;
                                    processPlot.variationB = false;
                                    processPlots.Insert(0, processPlot);
                                }
                                else
                                {
                                    plots.Add(currentPlot);//termination - all allowable fallbacks used
                                }
                            }
                        }
                    }
                    continue;//next
                }

                OBBox obbA = obbsA[0];
                OBBox obbB = obbsB[0];
                bool terminateA = obbA.area < plot.splitSettings.maxArea && rGen.output < 0.3f;
                if (!terminateA)
                {
                    processPlots.Add(new ProcessPlot(plotA, obbsA));
                }
                else
                {
                    if (plot.splitSettings.log)
                    plotA.notes = "small enough to end";
                    plots.Add(plotA);//termination
                }

                bool terminateB = obbB.area < plot.splitSettings.maxArea && rGen.output < 0.3f;
                if (!terminateB)
                {
                    processPlots.Add(new ProcessPlot(plotB, obbsB));
                }
                else
                {
                    if (plot.splitSettings.log)
                    plotB.notes = "small enough to end";
                    plots.Add(plotB);//termination
                }

                it++;
                if(it > 5000)
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }
            }
//            Profiler.EndSample();
        }

        private Subplot[] SplitPlot(ProcessPlot processPlot)
        {
//            Profiler.BeginSample("SplitPlot");
            Subplot[] output = new Subplot[2];

            IPlot plot = processPlot.plot;
            if (processPlot.obbs.Count == 0)
            {
                if (plot.splitSettings.log)
                plot.notes = "no obb split options left";
//                Profiler.EndSample();
                return output;
            }

            OBBox obBox = processPlot.obbs[0];
            if (obBox.area < plot.splitSettings.minArea)
            {
                if (plot.splitSettings.log)
                plot.notes = "area smaller than minimum";
//                Profiler.EndSample();
                return output;
            }

            Vector2[] points = plot.pointsV2;
            Vector2[][] boundaryPoints = plot.boundaryPoints;
            bool boundaryPointsUsed = boundaryPoints != null && boundaryPoints.Length > 0;
            bool[] externals = plot.externals;
            int shapeSize = points.Length;
            
            Vector2 cenExt = processPlot.shortSplit && !processPlot.longSplit ? obBox.longDir * obBox.longSize * 0.5f : obBox.shortDir * obBox.shortSize * 0.5f;
            float boxVariation = rGen.Range(-plot.splitSettings.variation * 0.5f, plot.splitSettings.variation * 0.5f) + 0.5f;
            if(processPlot.variationA && !processPlot.variationB) boxVariation *= 1 - processPlot.variationAmount;
            if(processPlot.variationA && processPlot.variationB) boxVariation += (1f - boxVariation) * processPlot.variationAmount;
            Vector2 cutCenter = Vector2.Lerp(obBox.center - cenExt, obBox.center + cenExt, boxVariation);
            Vector2 intExt = processPlot.shortSplit && !processPlot.longSplit ? obBox.shortDir * obBox.shortSize : obBox.longDir * obBox.longSize;
            Vector2 intP0 = cutCenter - intExt;
            Vector2 intP1 = cutCenter + intExt;
            //            if(plot is Plot)
            //                        Debug.DrawLine(JMath.ToV3(intP0), JMath.ToV3(intP1), Color.magenta);

            List<Vector2> intersectionPoints = new List<Vector2>();
            List<int> intersectionIndex = new List<int>();
            for (int p = 0; p < shapeSize; p++)
            {
                Vector2 p0 = points[p];
                Vector2 p1 = points[(p + 1) % shapeSize];
                Vector2 intersectionPoint;
                if (Intersects(intP0, intP1, p0, p1, out intersectionPoint))
                {
                    intersectionPoints.Add(intersectionPoint);
                    intersectionIndex.Add(p);
                    if (intersectionPoints.Count == 2)
                        break;
                }
            }

            List<Vector2> shapeA = new List<Vector2>();
            List<bool> externalsA = new List<bool>();
            List<Vector2> shapeB = new List<Vector2>();
            List<bool> externalsB = new List<bool>();
            float sqrMergeThreshold = plot.splitSettings.mergeThreashold * plot.splitSettings.mergeThreashold;
            if (intersectionPoints.Count == 2)
            {

                //rebuild shapes - from indicies
                int intersectionIndexA = intersectionIndex[0];
                int intersectionIndexB = intersectionIndex[1];
                int intersectionIndexAPlus = intersectionIndexA + 1 < shapeSize ? intersectionIndexA + 1 : 0;
                int intersectionIndexBPlus = intersectionIndexB + 1 < shapeSize ? intersectionIndexB + 1 : 0;
                int shapeAStartIndex = intersectionIndexBPlus;
                int shapeAEndIndex = intersectionIndexA;
                int shapeBStartIndex = intersectionIndexAPlus;
                int shapeBEndIndex = intersectionIndexB;

                //calculate boundary merge cases
                for (int i = 0; i < 2; i++)
                {
                    int boundaryIndex = intersectionIndex[i];
                    bool boundaryIsExternal = externals[boundaryIndex];
                    Vector2 intersectionPoint = intersectionPoints[i];

                    Vector2[] mergePoints;
                    if (plot.splitSettings.useBoundaryMergeOnExternalsOnly && !boundaryIsExternal || !boundaryPointsUsed)
                    {
                        mergePoints = new Vector2[2];
                        mergePoints[0] = points[boundaryIndex];
                        mergePoints[1] = points[boundaryIndex < shapeSize - 1 ? boundaryIndex + 1 : 0];
                    }
                    else
                    {
                        mergePoints = boundaryPoints[boundaryIndex];
                    }
                    int mergePointCount = mergePoints.Length;
                    float smallestsqrmag = float.PositiveInfinity;
                    int nearestMergePointIndex = -1;
                    for (int m = 0; m < mergePointCount; m++)
                    {
                        float mpSqrMag = (mergePoints[m] - intersectionPoint).sqrMagnitude;
                        if (mpSqrMag < sqrMergeThreshold && mpSqrMag < smallestsqrmag)
                        {
                            smallestsqrmag = mpSqrMag;
                            nearestMergePointIndex = m;
                        }
                    }

                    bool mergeOccured = nearestMergePointIndex != -1;
                    Vector2 mergePoint = mergeOccured ? mergePoints[nearestMergePointIndex] : intersectionPoint;

                    if (i == 0)
                    {
                        shapeA.Add(mergePoint);
                        externalsA.Add(false);
                        shapeB.Add(mergePoint);
                        externalsB.Add(boundaryIsExternal);
                    }
                    else
                    {
                        shapeA.Add(mergePoint);
                        externalsA.Add(boundaryIsExternal);
                        shapeB.Insert(0, mergePoint);
                        externalsB.Insert(0, false);
                    }

                    if (i == 0 && nearestMergePointIndex == 0)
                        shapeAEndIndex = intersectionIndexA % shapeSize;
                    if (i == 0 && nearestMergePointIndex == mergePointCount - 1)
                        shapeBStartIndex = (intersectionIndexA + 2) % shapeSize;
                    if (i == 1 && nearestMergePointIndex == 0)
                        shapeBEndIndex = intersectionIndexB % shapeSize;
                    if (i == 1 && nearestMergePointIndex == mergePointCount - 1)
                        shapeAStartIndex = (intersectionIndexB + 2) % shapeSize;

                }

                //build shape B
                while (true)
                {
                    shapeA.Add(points[shapeAStartIndex]);
                    externalsA.Add(externals[shapeAStartIndex]);

                    if (shapeAStartIndex == shapeAEndIndex) break;
                    shapeAStartIndex = (shapeAStartIndex + 1) % shapeSize;
                }

                //build shape B
                while (true)
                {
                    shapeB.Add(points[shapeBStartIndex]);
                    externalsB.Add(externals[shapeBStartIndex]);

                    if (shapeBStartIndex == shapeBEndIndex) break;
                    shapeBStartIndex = (shapeBStartIndex + 1) % shapeSize;
                }
            }
            else 
            {
                Debug.LogError("Whaaps!");
                Debug.DrawLine(JMath.ToV3(intP0), JMath.ToV3(intP1), Color.yellow);
                Debug.DrawLine(JMath.ToV3(intP0), JMath.ToV3(intP0) + Vector3.up * 10, Color.yellow, 20);
                Debug.Log(intersectionPoints.Count);
                Debug.Log(processPlot.obbs.Count);
                Debug.Log(obBox.area);
                Debug.Log(obBox.longSize);
                Debug.Log(obBox.shortSize);
                Debug.Log(obBox.longDir);
                Debug.Log(obBox.shortDir);
                obBox.DebugDrawPlotCut(boxVariation);
                obBox.DebugDraw();
                obBox.DebugMark();
                DebugDraw(points, Color.magenta);
                processPlots.Clear();
//                Profiler.EndSample();
                return output;
            }

            Subplot plotA = new Subplot(shapeA, _parent.splitSettings, externalsA);
            Subplot plotB = new Subplot(shapeB, _parent.splitSettings, externalsB);

            if (plot.splitSettings.minimumAccessLengthPercent < Mathf.Epsilon || (plotA.HasExternalAccess() && plotB.HasExternalAccess()))
            {
                output[0] = plotA;
                output[1] = plotB;
            }
            else//split was unsuccessful
            {
                if(!processPlot.longSplit && plot.splitSettings.fallbackSecondaryDivision)//restart split along long axis
                {
                    processPlot.longSplit = true;
                    processPlots.Insert(0, processPlot);
                }
                else
                {
                    if(processPlot.obbs.Count > 0 && plot.splitSettings.fallbackAlternativeObb)
                    {
                        processPlot.obbs.RemoveAt(0);
                        processPlot.longSplit = false;
                        processPlot.variationA = false;
                        processPlot.variationB = false;
                        processPlots.Insert(0, processPlot);
                    }
                    else
                    {
                        if (plot.splitSettings.log)
                        plot.notes = "insufficient external access - no alternative";
                    }
                }
            }

//            Profiler.EndSample();

            return output;
        }

        private bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
        {
//            Profiler.BeginSample("Intersects");
            intersection = Vector2.zero;

            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

//            Profiler.EndSample();

            return true;
        }


        public void OutputNotes()
        {
            int plotCount = plots.Count;
            if(plotCount == 0) return;
            if(!plots[0].splitSettings.log)
            {
                Debug.Log("Output Notes: Split Settings Log set to false so no notes were recorded");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Plot Splitter: Execution Notes");
            for (int p = 0; p < plotCount; p++)
            {
                sb.AppendLine("Plot ");
                sb.Append((p + 1).ToString());
                sb.Append(":");
                sb.Append(plots[p].notes);
                sb.Append(" (Area: ");
                sb.Append(plots[p].area);
                sb.Append(" )");
            }
            Debug.Log(sb.ToString());
        }

        public void DebugDraw()
        {
            foreach (ProcessPlot processPlot in processPlots)
            {
                Color col = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1);
                DebugDraw(processPlot.plot.pointsV2, col);
            }
        }

        public void DebugDrawExternals()
        {
            Color colI = Color.red;
            Color colE = Color.cyan;
            foreach (IPlot plot in plots)
            {
                int pointCount = plot.numberOfEdges;
                for (int i = 0; i < pointCount; i++)
                {
                    int ib = (i + 1) % pointCount;
                    bool external = plot.externals[i];
                    Debug.DrawLine(JMath.ToV3(plot[i]), JMath.ToV3(plot[ib]), external ? colE : colI);
                }
            }

            //            foreach(DebugSplitInfo info in debug)
            //            {
            //                info.plotA.DebugDraw(Color.yellow);
            //                info.plotB.DebugDraw(Color.yellow);
            //            }
        }

        public void GizmosDrawExternals()
        {
            Color colI = Color.red;
            Color colE = Color.cyan;
            foreach (IPlot plot in plots)
            {
                int pointCount = plot.numberOfEdges;
                for (int i = 0; i < pointCount; i++)
                {
                    int ib = (i + 1) % pointCount;
                    bool external = plot.externals[i];
                    Gizmos.color = external ? colE : colI;
                    Gizmos.DrawLine(JMath.ToV3(plot[i]), JMath.ToV3(plot[ib]));
                }
            }
        }

        public static void DebugDrawExternals(IPlot[] plots)
        {
            Color colI = Color.red;
            Color colE = Color.cyan;
            foreach (IPlot plot in plots)
            {
                int pointCount = plot.numberOfEdges;
                for (int i = 0; i < pointCount; i++)
                {
                    int ib = (i + 1) % pointCount;
                    bool external = plot.externals[i];
                    Debug.DrawLine(JMath.ToV3(plot[i]), JMath.ToV3(plot[ib]), external ? colE : colI);
                }
            }
        }

        public void DebugDraw(Vector2[] plot, Color col)
        {
            int pointCount = plot.Length;
            for (int i = 0; i < pointCount; i++)
            {
                int ib = (i + 1) % pointCount;
                Debug.DrawLine(JMath.ToV3(plot[i]), JMath.ToV3(plot[ib]), col);
            }
        }
    }
}