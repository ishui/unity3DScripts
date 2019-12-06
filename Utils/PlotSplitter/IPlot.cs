using UnityEngine;

namespace BuildR2 {
    public interface IPlot {
        Vector2 this[int index] { get; }
        Vector2[] pointsV2 { get; }
        Vector2[][] boundaryPoints {get;}//used for internal subdivision
        bool[] externals { get; }
        Vector2 bounds { get; }
        FlatBounds flatbounds { get; }
        Vector2 center { get; }
        float area { get; }
        int numberOfEdges { get; }
        bool HasExternalAccess();
        float longestExternalAccess { get; }
        void DebugDraw(Color col);
        string notes { get; set; }
        SplitSettings splitSettings { get; set;}
    }
}