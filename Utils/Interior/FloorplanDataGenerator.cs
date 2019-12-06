using System;
using UnityEngine;
using System.Collections.Generic;
using JaspLib;
using PolyPartition;

namespace BuildR2.Interior {
    [Serializable]
    public class FloorplanDataGenerator {

        public uint seed = 23;
        public List<Vector2Int> shape = new List<Vector2Int>();
        public List<RestrictedArea> restrictedAreas = new List<RestrictedArea>();
        [NonSerialized]
        public List<Partition> partitions = new List<Partition>();
        [NonSerialized]
        public List<Partition> calculatedPartitions = new List<Partition>();
        [NonSerialized]
        public List<FloorSegment> floorSegments = new List<FloorSegment>();
        public SplitSettings splitSettings = new SplitSettings(0);
        public float minimumWallUnitSpacing;
        public Floorplan floorplan;

        public FloorplanDataGenerator(Vector2Int[] points, float wallSpacing = 0) {
            shape.AddRange(points);
            minimumWallUnitSpacing = wallSpacing;
            splitSettings.minimumBoundaryPointDistance = wallSpacing;
        }

        public void Execute() {
            
            calculatedPartitions.Clear();
            
            
            //remvoe duplicate points
            for (int i = 0; i < shape.Count; i++) {
                Vector2Int p0 = shape[i];
                Vector2Int p1 = shape[i < shape.Count - 1 ? i + 1 : 0];
                float sqrM = Vector2Int.SqrMagnitudeFloat(p1, p0);
                if (sqrM < Mathf.Epsilon) {
                    shape.RemoveAt(i);
                    i--;
                }
            }

            //break poly down into convex shapes
            TPPLPoly poly = new TPPLPoly();
            for (int i = 0; i < shape.Count; i++)
                poly.Points.Add(new TPPLPoint(shape[i].x, shape[i].y));

            if (BuildrPolyClockwise.Check(shape))
                poly.SetOrientation(TPPLOrder.CW);
            else
                poly.SetOrientation(TPPLOrder.CCW);

            List<TPPLPoly> parts = new List<TPPLPoly>();
            TPPLPartition tpplPartition = new TPPLPartition();
            tpplPartition.ConvexPartition_HM(poly, parts);

            //generate an irregular grid upon each convex poly
            int partCount = parts.Count;
            PlotSplitter plotSplitter = new PlotSplitter();
            floorSegments.Clear();
            for (int p = 0; p < partCount; p++) {
                TPPLPoly partPoly = parts[p];
                int partSize = partPoly.Count;
                List<Vector2Int> plotPoints = new List<Vector2Int>();
                for (int w = 0; w < partSize; w++) {
                    TPPLPoint tpplPoint = partPoly[w];
                    Vector2Int p0 = new Vector2Int(tpplPoint.X, tpplPoint.Y);
                    plotPoints.Add(p0);
                }

                Plot plot = new Plot(seed, plotPoints, minimumWallUnitSpacing);
                plot.splitSettings = splitSettings;
                plotSplitter.Execute(plot, seed);
                int splitCount = plotSplitter.plots.Count;
                for (int s = 0; s < splitCount; s++) {
                    IPlot segmentPlot = plotSplitter.plots[s];
                    Vector2Int[] points = Vector2Int.Parse(segmentPlot.pointsV2);
                    FloorSegment segment = new FloorSegment(segmentPlot.area, segmentPlot.flatbounds, points);
                    floorSegments.Add(segment);
                }
            }

            int segmentCount = floorSegments.Count;            
            List<FloorSegment> availableSegments = new List<FloorSegment>(floorSegments);
            int restrictedAreaCount = restrictedAreas.Count;
            Partition restrictedPartition = null;
            for (int r = 0; r < restrictedAreaCount; r++)
            {
                RestrictedArea area = restrictedAreas[r];
                FlatBounds areaBounds = new FlatBounds();
                areaBounds.Encapsulate(Vector2Int.Parse(area.shape));
                for (int fs = 0; fs < segmentCount; fs++)
                {
                    FloorSegment segment = availableSegments[fs];
                    if (areaBounds.Overlaps(segment.bounds))
                    {
                        if (JMath.ShapesIntersect(Vector2Int.Parse(area.shape), Vector2Int.Parse(segment.points)))
                        {
                            if(restrictedPartition == null)
                                restrictedPartition = new Partition();
                            restrictedPartition.AddSegment(segment);
                            availableSegments.Remove(segment);
                            segmentCount--;
                            fs--;
                        }
                    }
                }
            }

            //Link up floor segments
            segmentCount = availableSegments.Count;
            for (int x = 0; x < segmentCount; x++) {
                FloorSegment subject = floorSegments[x];
                FlatBounds subjectBounds = subject.nBounds;
                for (int y = 0; y < segmentCount; y++) {
                    if (x == y) continue;

                    FloorSegment candidate = floorSegments[y];


                    FlatBounds candidateBounds = candidate.nBounds;
                    if (subjectBounds.Overlaps(candidateBounds)) {
                        if (candidate.neighbours.Contains(subject)) continue;
                        subject.neighbours.Add(candidate);
                        candidate.neighbours.Add(subject);
                    }
                }
            }

            //Grow out partitions to fill the available space
            List<PartitionGrowth> partitionGs = new List<PartitionGrowth>();
            Dictionary<FloorSegment, FloorSegmentClaim> segmentClaims = new Dictionary<FloorSegment, FloorSegmentClaim>();
            for (int i = 0; i < partitions.Count; i++)
                partitionGs.Add(new PartitionGrowth(partitions[i]));

            int it = 1000;
            while (true) {

                int growthCount = partitionGs.Count;
                int completePartitionGrowths = 0;
                int[] partitionGrowthAmount = new int[growthCount];
                segmentClaims.Clear();

                for (int g = 0; g < growthCount; g++) {
                    PartitionGrowth partitionG = partitionGs[g];
                    if (!partitionG.active) {
                        completePartitionGrowths++;
                        continue;
                    }

                    if (availableSegments.Count == 0)
                        break;

                    //assign inital segment to begin partition from
                    if (!partitionG.initialised) {
                        float nearestSqrMag = float.PositiveInfinity;
                        FloorSegment candidate = availableSegments[0];
                        for (int x = 0; x < availableSegments.Count; x++) {
                            FloorSegment subject = availableSegments[x];
                            float sqrMag = Vector2Int.SqrMagnitudeFloat(partitionG.subject.position, subject.position);
                            if (sqrMag < nearestSqrMag) {
                                candidate = subject;
                                nearestSqrMag = sqrMag;
                            }
                        }

                        partitionG.capturedSegments.Add(candidate);
                        partitionG.processSegments.Add(candidate);
                        availableSegments.Remove(candidate);
                        partitionG.initialised = true;
                        partitionGrowthAmount[g] = 1;
                        continue;//don't start growth until next iteration
                    }

                    //grow partition
                    if (partitionG.initialised) {
                        List<FloorSegment> neighbourCandiates = new List<FloorSegment>();
                        int processCount = partitionG.processSegments.Count;
//                        float additionalArea = 0;
                        for (int p = 0; p < processCount; p++) {
                            FloorSegment processSegment = partitionG.processSegments[p];
                            int processNeighbourCount = processSegment.neighbours.Count;
                            for (int n = 0; n < processNeighbourCount; n++) {

                                FloorSegment neighbour = processSegment.neighbours[n];
                                bool isAvailable = availableSegments.Contains(neighbour);
                                bool notDuplicateNeighbour = !neighbourCandiates.Contains(neighbour);
                                if (isAvailable && notDuplicateNeighbour) {
                                    neighbourCandiates.Add(neighbour);
                                    float fit = processSegment.BestFit(neighbour);
                                    if(fit > Mathf.Epsilon) {
                                        FloorSegmentClaim newClaim = new FloorSegmentClaim();
                                        newClaim.partition = partitionG;
                                        newClaim.growthIndex = g;
                                        newClaim.segment = neighbour;
                                        newClaim.priority = partitionG.subject.priority * fit;

                                        if (!segmentClaims.ContainsKey(neighbour)) {
                                            segmentClaims.Add(neighbour, newClaim);
                                        }
                                        else {
                                            FloorSegmentClaim currentClaim = segmentClaims[neighbour];
                                            if (currentClaim.priority < newClaim.priority)
                                                segmentClaims[neighbour] = newClaim;
                                        }
                                    }
//                                    additionalArea += neighbour.area;
                                }
                            }
                        }

//                        int neighbourCandiatesCount = neighbourCandiates.Count;
//                        for (int n = 0; n < neighbourCandiatesCount; n++) {
//                            FloorSegment segement = neighbourCandiates[n];
//
//                            if (segmentClaims.ContainsKey(segement)) {
//
//                            }
//                            else {
//
//                            }
//                        }
                        //                        if (neighbourCandiatesCount > 0) {
                        //
                        //                            bool canAddAll = partitionG.AvailableArea(additionalArea);
                        //                            if (canAddAll) {
                        //                                partitionG.processSegments.Clear();
                        //                                for (int n = 0; n < neighbourCandiatesCount; n++)
                        //                                    availableSegments.Remove(neighbourCandiates[n]);
                        ////                                partitionG.AddSegments(neighbourCandiates);
                        //                            }
                        //                            else {
                        //                                //                                TODO partial add (?)
                        ////                                partitionG.AddSegments(neighbourCandiates);
                        //                                partitionG.Complete();
                        //                            }
                        //                        }
                        //                        else {
                        //                            partitionG.Complete();
                        //                        }

//                        if (partitionG.processSegments.Count == 0)
//                            partitionG.Complete();
                    }
                }

                foreach(KeyValuePair<FloorSegment, FloorSegmentClaim> kv in segmentClaims)
                {
                    //TODO - support instance when new areas to add are too large
                    //TODO - fall back on partial adding of single side
                    FloorSegmentClaim claim = kv.Value;
                    claim.partition.AddSegment(claim.segment);
                    availableSegments.Remove(claim.segment);
                    partitionGrowthAmount[claim.growthIndex]++;
                }

                for(int g = 0; g < growthCount; g++)
                {
                    PartitionGrowth partitionG = partitionGs[g];
                    if(!partitionG.active) continue;
//                    Debug.Log(g+" "+ partitionG.AcceptableAreaUsed()+" " + partitionGrowthAmount[g]+" "+ partitionG.processSegments.Count);
                    if(partitionG.AcceptableAreaUsed() || partitionGrowthAmount[g] == 0 || partitionG.processSegments.Count == 0)
                    {
                        completePartitionGrowths++;
                        partitionG.Complete();
                    }
                }

                if (completePartitionGrowths == growthCount) //all partitions have been completed
                    break;

                if (availableSegments.Count == 0) {

                    foreach (PartitionGrowth part in partitionGs) {
                        if (part.active) {
                            part.Complete();
                        }
                    }

                    foreach (PartitionGrowth part in partitionGs) {
                        int childCount = part.subject.children.Count;
                        if (childCount > 0) {
                            for (int c = 0; c < childCount; c++)
                                partitionGs.Add(new PartitionGrowth(part.subject.children[c]));
                            part.subject.children.Clear();
                            availableSegments.AddRange(part.capturedSegments);
                            part.capturedSegments.Clear();
                            break;
                        }
                    }

                    if (availableSegments.Count == 0)
                        break;
                }

                it--;
                if (it == 0) {
                    Debug.Log(" MAX reached!");
                    Debug.Log(availableSegments.Count);
                    foreach (PartitionGrowth pg in partitionGs) {
                        Debug.Log(pg.processSegments.Count);
                        Debug.Log(pg.capturedSegments.Count);
                        pg.Complete();
                    }
                    break;
                }
            }

            
            foreach (PartitionGrowth part in partitionGs) {
                if (part.active) {
                    part.Complete();
                }
                calculatedPartitions.Add(part.subject);
            }

//            if (floorplan != null)
//            {
//                int roomCount = calculatedPartitions.Count;
//                
//                
//                
//                
//                Room floorplanRoom = new Room();
//                
//                
//                floorplan.rooms.Add();
//            }

            //            foreach (Partition part in partitions) {
            //                Debug.Log(part.segments.Count);
            //            }
        }

        //STRUCTS
        [Serializable]
        public class PartitionGrowth {
            public bool active = true;
            public Partition subject;
            public List<FloorSegment> capturedSegments = new List<FloorSegment>();
            public List<FloorSegment> processSegments = new List<FloorSegment>();
            public float currentArea = 0;
            public bool initialised;

            public PartitionGrowth(Partition usePartition) {
                subject = usePartition;
            }

            public void AddSegment(FloorSegment segment) {
                capturedSegments.Add(segment);
                processSegments.Add(segment);
                if (segment.area < 0)
                    Debug.Log("AddSegment minus area");
                currentArea += segment.area;
            }

            public void AddSegments(FloorSegment[] segments) {
                int length = segments.Length;
                for (int i = 0; i < length; i++)
                    AddSegment(segments[i]);
            }

            public void AddSegments(List<FloorSegment> segments) {
                int length = segments.Count;
                for (int i = 0; i < length; i++)
                    AddSegment(segments[i]);
            }

            public bool AvailableArea(float additionalArea) {
                if (!subject.constrainSpace) return true;
                return currentArea + additionalArea <= subject.maximumArea;
            }

            public bool AcceptableAreaUsed() {
                if (!subject.constrainSpace) return false;
                return currentArea > subject.minimumArea && currentArea < subject.maximumArea;
            }

            public void Complete() {
                active = false;
                subject.ClearSegments();
                subject.AddSegments(capturedSegments);
            }

            public void MarkInactive() {
                active = false;
            }
        }

        //STRUCTS
        public class Partition {
            public string name;
            public Vector2Int position;
            public float priority = 1;
            public bool constrainSpace = true;
            public float minimumArea;
            public float maximumArea;
            public List<Partition> children = new List<Partition>();
            private List<FloorSegment> _segments = new List<FloorSegment>();
            private FlatBounds _bounds = new FlatBounds();

            public FloorSegment this[int index]
            {
                get
                {
                    if(_segments.Count == 0) return null;
                    if(_segments.Count >= index) return null;
                    return _segments[index];
                }
            }

            public int segmentCount {get {return _segments.Count;}}

            public void AddSegment(FloorSegment segment) {
                _segments.Add(segment);
                int size = segment.points.Length;
                for(int p = 0; p < size; p++)
                    _bounds.Encapsulate(segment.points[p].vx, segment.points[p].vy);
            }

            public void AddSegments(FloorSegment[] segments) {
                int segmentCount = segments.Length;
                for (int s = 0; s < segmentCount; s++)
                    AddSegment(segments[s]);
            }

            public void AddSegments(List<FloorSegment> segments) {
                int segmentCount = segments.Count;
                for (int s = 0; s < segmentCount; s++)
                    AddSegment(segments[s]);
            }

            public void ClearSegments()
            {
                _segments.Clear();
                _bounds.Clear();
            }

            public void DrawDebug(Color col) {
                Gizmos.color = col;
                Gizmos.DrawLine(position.vector3XZ, position.vector3XZ + Vector3.up * 10);
                if (_segments.Count > 0)
                    Gizmos.DrawLine(position.vector3XZ, _segments[0].position.vector3XZ);

                foreach (FloorSegment segment in _segments) {
                    segment.DebugDraw(col);
                }
            }
        }

        public struct RestrictedArea
        {
            public Vector2Int[] shape;
            public bool isEgress;
        }

        public struct PartitionRoom
        {
            public FloorSegment[] segments;
            public Vector2Int[] shape;

//            public static PartitionRoom Create(Partition partition)
//            {
//                JMath.ConvexHull()
//                int segmentCount = partition.segmentCount;
//                FloorSegment startSegment = null;
//                int startPoint = 
//                float maxX = float.NegativeInfinity;
//                for (int i = 0; i < segmentCount; i++)
//                {
//                    FloorSegment fs = partition[i];
//                    int size = fs.points.Length;
//                    for(int p = 0; p < size; p++)
//                    {
//                        if(maxX < fs.points[p].x)
//                        {
//                            maxX = fs.points[p].x;
//                            startSegment = fs;
//                        }
//                    }
//                }
//            }
        }

        public struct FloorSegmentClaim {
            public FloorSegment segment;
            public PartitionGrowth partition;
            public int growthIndex;
            public float priority;
        }

        public class FloorSegment {
            public Vector2Int[] points;
            public FlatBounds bounds;
            public FlatBounds nBounds;
            public float area;
            public Vector2Int position;
            public List<FloorSegment> neighbours;

            public FloorSegment(float area, params Vector2Int[] input) {
                int inputCount = input.Length;
                points = new Vector2Int[inputCount];
                bounds = new FlatBounds();
                for (int p = 0; p < inputCount; p++) {
                    points[p] = input[p];
                    bounds.Encapsulate(input[p].vx, input[p].vy);
                }
                //                CalculateNormals();
                position = new Vector2Int(bounds.center);
                nBounds = new FlatBounds(bounds);
                nBounds.Expand(0.25f);
                this.area = area;
                neighbours = new List<FloorSegment>();
            }

            public FloorSegment(float area, FlatBounds bounds, params Vector2Int[] input) {
                int inputCount = input.Length;
                points = new Vector2Int[inputCount];
                this.bounds = bounds;
                for (int p = 0; p < inputCount; p++)
                    points[p] = input[p];
                //                CalculateNormals();
                position = new Vector2Int(bounds.center);
                nBounds = new FlatBounds(bounds);
                nBounds.Expand(0.25f);
                this.area = area;
                neighbours = new List<FloorSegment>();
            }

            public float BestFit(FloorSegment target) {
                if (target == this) return -1;
                int targetSize = target.points.Length;
                int pointSize = points.Length;
                float output = 0;//default is there is no fit

                for (int p = 0; p < pointSize; p++) {
                    Vector2Int point = points[p];
                    for (int t = 0; t < targetSize; t++) {
                        Vector2Int targetPoint = target.points[t];
                        float sqrMag = (targetPoint - point).SqrMagnitudeFloat();
                        if(sqrMag < 0.1f)
                            output += 0.5f;
                        else {//test for two lines joined
                            Vector2Int pointB = points[p < pointSize - 1 ? p + 1 : 0];
                            if(Colinear(point, pointB, targetPoint)) {
                                Vector2Int targetPointB = target.points[t < targetSize - 1 ? t + 1 : 0];
                                if(Colinear(point, pointB, targetPointB))
                                    output += 1f;
                            }
                        }
                    }
                }

                return Mathf.Clamp01(output);
            }

            //            private void CalculateNormals() {
            //                int pointCount = points.Length;
            //                normals = new Vector2[pointCount];
            //                for (int p = 0; p < pointCount; p++) {
            //                    int pb = p < pointCount - 1 ? p + 1 : 0;
            //                    Vector2 p0 = points[p];
            //                    Vector2 p1 = points[pb];
            //                    Vector2 dir = p1 - p0; ;
            //                    normals[p] = Rotate(dir, -90).normalized;
            //                }
            //            }

            public void DebugDraw() {
                Vector3 c = position.vector3XZ;
                for (int w = 0; w < points.Length; w++) {
                    Vector3 p0 = points[w].vector3XZ;
                    Vector3 p1 = points[w < points.Length - 1 ? w + 1 : 0].vector3XZ;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(p0, p1);
                    Gizmos.DrawLine(p0, c);
                }
            }

            public void DebugDraw(Color color) {
                Vector3 c = position.vector3XZ;
                for (int w = 0; w < points.Length; w++) {
                    Vector3 p0 = points[w].vector3XZ;
                    Vector3 p1 = points[w < points.Length - 1 ? w + 1 : 0].vector3XZ;
                    Gizmos.color = color;
                    Gizmos.DrawLine(p0, p1);
                    Gizmos.DrawLine(p0, c);
                }
            }

            public void DebugDrawBounds(Color color) {

                Vector3 p0 = new Vector3(bounds.xMin, 0, bounds.yMin);
                Vector3 p1 = new Vector3(bounds.xMax, 0, bounds.yMin);
                Vector3 p2 = new Vector3(bounds.xMax, 0, bounds.yMax);
                Vector3 p3 = new Vector3(bounds.xMin, 0, bounds.yMax);

                Gizmos.color = color;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p0);
            }

            public void DebugDrawBoundsN(Color color) {

                Vector3 p0 = new Vector3(nBounds.xMin, 0, nBounds.yMin);
                Vector3 p1 = new Vector3(nBounds.xMax, 0, nBounds.yMin);
                Vector3 p2 = new Vector3(nBounds.xMax, 0, nBounds.yMax);
                Vector3 p3 = new Vector3(nBounds.xMin, 0, nBounds.yMax);

                Gizmos.color = color;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p0);
            }

            public void DebugDrawConnections(Color color) {

                foreach (FloorSegment seg in neighbours)
                {
                    Vector3 p0 = position.vector3XZ;
                    Vector3 p1 = seg.position.vector3XZ;
                    Gizmos.color = color;
                    Gizmos.DrawLine(p0, p1);
                }
            }

            private Vector2Int Rotate(Vector2Int input, float degrees) {
                float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
                float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

                float tx = input.vx;
                float ty = input.vy;
                input.vx = (cos * tx) - (sin * ty);
                input.vy = (sin * tx) + (cos * ty);
                return input;
            }


            private bool PointOnLine(Vector2Int p, Vector2Int a, Vector2Int b) {
                float cross = (p.vy - a.vy) * (b.vx - a.vx) - (p.vx - a.vx) * (b.vy - a.vy);
                if (Mathf.Abs(cross) > Mathf.Epsilon) return false;
                float dot = (p.vx - a.vx) * (b.vx - a.vx) + (p.vy - a.vy) * (b.vy - a.vy);
                if (dot < 0) return false;
                float squaredlengthba = (b.vx - a.vx) * (b.vx - a.vx) + (b.vy - a.vy) * (b.vy - a.vy);
                if (dot > squaredlengthba) return false;
                return true;
            }

            private bool Colinear(Vector2Int pa, Vector2Int pb, Vector2Int pc) {
                float detleft = (pa.vx - pc.vx) * (pa.vy - pb.vy);
                float detright = (pa.vy - pb.vy) * (pa.vx - pc.vx);
                float val = detleft - detright;
                return (val > -Mathf.Epsilon && val < Mathf.Epsilon);
            }

            private float Parallel(Vector2Int la, Vector2Int lb)
            {
                return Mathf.Abs((la.vx * lb.vx + la.vy * lb.vy) / Mathf.Sqrt((la.vx * la.vx + la.vy * la.vy) * (lb.vx * lb.vx + lb.vy * lb.vy)));
            }

        }
    }
}