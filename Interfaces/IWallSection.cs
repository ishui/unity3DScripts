using UnityEngine;

namespace BuildR2 {
    public interface IWallSection {
        bool hasOpening {get; set;}
        bool isDoor {get; set;}
        WallSection.VerticalRestrictions verticalRestriction {get; set;}
        Portal portal {get; set;}
        Model model {get; set;}
        Model openingModel {get; set;}
        Model balconyModel {get; set;}
        float balconyHeight {get; set;}
        float balconySideOverhang {get; set;}
        Model shutterModel {get; set;}
        WallSection.DimensionTypes dimensionType {get; set;}
        float openingWidth {get; set;}
        float openingHeight {get; set;}
        float openingDepth {get; set;}
        float openingWidthRatio {get; set;}
        float openingHeightRatio {get; set;}
        float openingDepthRatio {get; set;}
        bool openingFrame { get; set;}
        float openingFrameSize { get; set;}
        float openingFrameExtrusion { get; set;}
        bool isArched {get; set;}
        float archHeight {get; set;}
        float archCurve {get; set;}
        int archSegments {get; set;}
        bool extrudedSill {get; set;}
        Vector3 extrudedSillDimentions {get; set;}
        bool extrudedLintel {get; set;}
        Vector3 extrudedLintelDimentions {get; set;}
        Surface openingSurface {get; set;}
        Surface wallSurface {get; set;}
        Surface sillSurface {get; set;}
        Surface ceilingSurface {get; set;}
        bool bayExtruded {get; set;}
        float bayExtrusion {get; set;}
        float bayBevel {get; set;}
        Texture2D previewTexture {get;}
        Matrix4x4 OpeningMeshPosition(Vector2 size, float wallThickness);
        Matrix4x4 BalconyMeshPosition(Vector2 size, float wallThickness);
        Matrix4x4 ShutterMeshPositionLeft(Vector2 size, float wallThickness);
        Matrix4x4 ShutterMeshPositionRight(Vector2 size, float wallThickness);
        void UpdatePreviewTexture(IconUtil.GUIDIconData iconData = null);
        void LoadPreviewTexture();
        bool CanRender(Vector2 size);
        void GenereateData();
    }
}