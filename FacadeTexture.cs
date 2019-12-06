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



using UnityEngine;

namespace BuildR2
{
    public class FacadeTexture
    {
        private const int PIXELS_PER_METER = 100;

//        private static int textureWidth;
//        private static int textureHeight;

//        public static void Texture(out Texture2D output, FacadeGenerator.FacadeData data, Vector2 size)
//        {
////            List<TexturePaintObject> sourceTextures = new List<TexturePaintObject>();
//            Vector3 facadeVector = data.baseB - data.baseA;
////            Vector3 facadeDirection = facadeVector.normalized;
////            Vector3 facadeNormal = Vector3.Cross(facadeDirection, Vector3.up);
//            float facadeLength = 0;
//            int wallSections = 0;
//            Vector2 wallSectionPhysicalSize = new Vector2(0,0);
//            Vector2Int wallSectionPixelSize = new Vector2Int(1,1);
////            List<Vector3> baseCurvepoints = new List<Vector3>();
//            if (data.isStraight)
//            {
//                facadeLength = facadeVector.magnitude;
//                wallSections = Mathf.FloorToInt(facadeLength / data.volume.minimumWallUnitLength);
//                wallSectionPhysicalSize = new Vector2(facadeLength / wallSections, data.volume.floorHeight);
//                wallSectionPixelSize = new Vector2Int(Mathf.RoundToInt(wallSectionPhysicalSize.x), Mathf.RoundToInt(wallSectionPhysicalSize.y));
//            }
////            else
////            {
//////                baseCurvepoints = BuildRFacadeUtil.CalculateCurvedFacadeBasePointsNormalised(data, out facadeLength);
////                data.vol
////                wallSections = baseCurvepoints.Count - 1;
////                //                facadeLength = 0;
////                //                for(int fw = 0; fw < wallSections; fw++)
////                //                    facadeLength += Vector3.Distance(baseCurvepoints[fw], baseCurvepoints[fw + 1]);
////                float sectionWidth = Vector3.Distance(baseCurvepoints[0], baseCurvepoints[1]);
////                wallSectionSize = new Vector2(sectionWidth, data.floorheight);
////
////            }
//
//            textureWidth = Mathf.CeilToInt(PIXELS_PER_METER * size.x);
//            textureHeight = Mathf.CeilToInt(PIXELS_PER_METER * size.y);
//            Color32[] colourArray = new Color32[textureWidth * textureHeight];
////            Vector2 bayBase = Vector2.zero;
//
//            Dictionary<WallSection, TexturePaintObject> generatedSections = new Dictionary<WallSection, TexturePaintObject>();
//
//            for (int fl = 0; fl < data.floorCount; fl++)
//            {
//                for (int s = 0; s < wallSections; s++)
//                {
//                    WallSection section = data.facadeDesign.GetWallSection(s, fl, wallSections);
//                    TexturePaintObject sectionTexturePaintObject;
//                    if(generatedSections.ContainsKey(section))
//                    {
//                        sectionTexturePaintObject = generatedSections[section];
//                    }
//                    else
//                    {
//                        Color32[] sectionTexture;
//                        WallSectionGenerator.Texture(out sectionTexture, section, wallSectionSize);
//                        sectionTexturePaintObject = new TexturePaintObject();
//                        sectionTexturePaintObject.pixels = sectionTexture;
//                        sectionTexturePaintObject.width = wallSectionSize.x;
//                        sectionTexturePaintObject.height = wallSectionSize.y;
//                        sectionTexturePaintObject.tiles = Vector2.one;
//                        generatedSections.Add(section, sectionTexturePaintObject);
//                    }
//                    Vector2 sectionTextureBase = new Vector2(wallSectionSize.x * s, wallSectionSize.y * fl);
//                    DrawSectionTexture(sectionTexturePaintObject, colourArray, sectionTextureBase, wallSectionSize);
//                }
//            }
//
//            output = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, true);
//            output.filterMode = FilterMode.Bilinear;
//            output.SetPixels32(colourArray);
//            output.Apply(true, false);
//        }

        private struct TexturePaintObject
        {
            public Color32[] pixels;
            public int width;
            public int height;
            public bool tiled;
            public Vector2 tiles;
            public bool flipped;
        }

//        private static void DrawSectionTexture(TexturePaintObject sourceTexture, Color32[] colourArray, Vector2 bayBase, Vector2Int bayDimensions)
//        {
//            int paintWidth = bayDimensions.x;
//            int paintHeight = bayDimensions.y;
//
//            TexturePaintObject paintObject = sourceTexture;
//            Color32[] sourceColours = paintObject.pixels;
//            int sourceWidth = paintObject.width;
//            int sourceHeight = paintObject.height;
//            int sourceSize = sourceColours.Length;
//            Vector2 textureStretch = Vector2.one;
//            if (!paintObject.tiled)
//            {
//                textureStretch.x = (float)sourceWidth / (float)paintWidth;
//                textureStretch.y = (float)sourceHeight / (float)paintHeight;
//            }
//            int baseX = Mathf.RoundToInt((bayBase.x));
//            int baseY = Mathf.RoundToInt((bayBase.y));
//            int baseCood = baseX + baseY * textureWidth;
//            bool flipped = sourceTexture.flipped;
//
//            //fill in a little bit more to cover rounding errors
//            paintWidth++;
//            paintHeight++;
//
//            int useWidth = !flipped ? paintWidth : paintHeight;
//            int useHeight = !flipped ? paintHeight : paintWidth;
//            int textureSize = textureWidth * textureHeight;
//            for (int px = 0; px < useWidth; px++)
//            {
//                for (int py = 0; py < useHeight; py++)
//                {
//                    int six, siy;
//                    if (paintObject.tiled)
//                    {
//                        six = (baseX + px) % sourceWidth;
//                        siy = (baseY + py) % sourceHeight;
//                    }
//                    else
//                    {
//                        six = Mathf.RoundToInt(px * textureStretch.x * paintObject.tiles.x) % sourceWidth;
//                        siy = Mathf.RoundToInt(py * textureStretch.y * paintObject.tiles.y) % sourceHeight;
//                    }
//                    int sourceIndex = Mathf.Clamp(six + siy * sourceWidth, 0, sourceSize - 1);
//                    int paintPixelIndex = (!flipped) ? px + py * textureWidth : py + px * textureWidth;
//                    int pixelCoord = Mathf.Clamp(baseCood + paintPixelIndex, 0, textureSize - 1);
//                    Color32 sourceColour = sourceColours[sourceIndex];
//                    if (pixelCoord >= colourArray.Length || pixelCoord < 0)
//                        Debug.Log(pixelCoord + " " + textureWidth + " " + textureHeight + " " + textureSize + " " + px + " " + py);
//                    colourArray[pixelCoord] = sourceColour;
//                }
//            }
//        }
    }
}