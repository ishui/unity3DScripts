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
using System.Collections.Generic;

namespace BuildR2
{
    public class SimpleTextureGenerator
    {
        public static void GenerateTexture(Texture2D texture, Surface surface, Rect space, Rect physicalSpace)
        {
            if (surface == null)
            {
                Debug.LogError("Null surface send to Generate Texture");
                return;
            }
            if (surface.previewTexture == null)
            {
                Debug.LogError(string.Format("Surface used ({0}) has no texture", surface.name));
                return;
            }

            int colour32Width = Mathf.RoundToInt(space.width);
            int colour32Height = Mathf.RoundToInt(space.height);
            Vector2 textureUnitSize = surface.textureUnitSize;

            Color32[] surfaceColor32 = surface.pixels;
            int surfaceWidth = surface.previewTexture.width;
            int surfaceHeight = surface.previewTexture.height;

            int pixelsPerMeter = Mathf.RoundToInt(space.width / physicalSpace.width);
            int resizeWidth = Mathf.RoundToInt(textureUnitSize.x * pixelsPerMeter);
            int resizeHeight = Mathf.RoundToInt(textureUnitSize.y * pixelsPerMeter);
            int tileX = Mathf.CeilToInt(colour32Width / (float)resizeWidth);
            int tileY = Mathf.CeilToInt(colour32Height / (float)resizeHeight);
            resizeWidth = colour32Width / tileX;
            resizeHeight = colour32Height / tileY;
//            Debug.Log(surfaceWidth+" "+ surfaceHeight+" "+ resizeWidth+" "+ resizeHeight);
            Color32[] resizedSurface = TextureScale.NearestNeighbourSample(surfaceColor32, surfaceWidth, surfaceHeight, resizeWidth, resizeHeight);

            int offsetX = Mathf.RoundToInt(space.x);
            int offsetY = Mathf.RoundToInt(space.y);

//            Debug.Log(tileX+" "+colour32Width+" "+surfaceWidth);

            for(int tx = 0; tx < tileX; tx++)
            {
                for(int ty = 0; ty < tileY; ty++)
                {
                    int xPaintPos = offsetX + tx * resizeWidth;
                    int yPaintPos = offsetY + ty * resizeHeight;
                    texture.SetPixels32(xPaintPos, yPaintPos, resizeWidth, resizeHeight, resizedSurface);
                }
            }
        }

        public static void GenerateFacade(FacadeGenerator.FacadeData data, Texture2D texture, Rect space)
        {
            Vector3 facadeVector = data.baseB - data.baseA;
            int wallSections = 0;
            Vector2 wallSectionPhyicalSize;
            Vector2Int wallSectionPixelSize;
            if (data.isStraight)
            {
                var facadeLength = facadeVector.magnitude;
                wallSections = Mathf.FloorToInt(facadeLength / data.minimumWallUnitLength);
            }
            else
            {
                wallSections = data.anchors.Count - 1;
            }
            wallSectionPhyicalSize = new Vector2(Vector2.Distance(data.anchors[0].vector2, data.anchors[1].vector2), data.floorHeight);
            wallSectionPixelSize = new Vector2Int(Mathf.RoundToInt(space.width / wallSections), Mathf.RoundToInt(space.height / data.floors));
            Dictionary<WallSection, Color32[]> generatedSections = new Dictionary<WallSection, Color32[]>();

            int startFloor = data.startFloor;
            for (int fl = startFloor; fl < data.floorCount; fl++)
            {
                for (int s = 0; s < wallSections; s++)
                {
                    //                    Debug.Log(s);
                    WallSection section = data.facadeDesign.GetWallSection(s, fl, wallSections, data.floorCount);
                    //                    Debug.Log(section);
                    if (section == null) continue;
                    //                    Texture2D generatedTexture;
                    Color32[] wallsectionColourArray;
                    if (generatedSections.ContainsKey(section))
                    {
                        wallsectionColourArray = generatedSections[section];
                    }
                    else
                    {
                        WallSectionGenerator.Texture(out wallsectionColourArray, section, wallSectionPixelSize, wallSectionPhyicalSize);
                        generatedSections.Add(section, wallsectionColourArray);
                    }

                    int xPosition = Mathf.RoundToInt(space.x) + wallSectionPixelSize.x * s;
                    int yPosition = Mathf.RoundToInt(space.y) + wallSectionPixelSize.y * fl;
                    texture.SetPixels32(xPosition, yPosition, wallSectionPixelSize.x, wallSectionPixelSize.y, wallsectionColourArray);
                }
                //note string courses ignored for now
            }
        }

        public static Color32[] GenerateFacadePreview(Facade design, Vector2Int pixelSize, Vector2Int patternSize)
        {
            var wallSectionPixelSize = new Vector2Int(Mathf.RoundToInt(pixelSize.x / (float)patternSize.x), Mathf.RoundToInt(pixelSize.y / (float)patternSize.y));
            int outputWidth = Mathf.RoundToInt(pixelSize.x);
            int outputHeight = Mathf.RoundToInt(pixelSize.y);
            int dataSize = outputWidth * outputHeight;
            Color32[] output = new Color32[dataSize];
            Dictionary<WallSection, Color32[]> generatedSections = new Dictionary<WallSection, Color32[]>();
            
            for (int fl = 0; fl < patternSize.y; fl++)
            {
                for (int s = 0; s < patternSize.x; s++)
                {
                    WallSection section = design.GetWallSection(s, fl, patternSize.x, patternSize.y);
                    if (section == null) continue;
                    Color32[] wallsectionColourArray;
                    if (generatedSections.ContainsKey(section))
                    {
                        wallsectionColourArray = generatedSections[section];
                    }
                    else
                    {
                        WallSectionGenerator.Texture(out wallsectionColourArray, section, wallSectionPixelSize, Vector2.one * 3);
                        generatedSections.Add(section, wallsectionColourArray);
                    }
                    int xPosition = wallSectionPixelSize.x * s;
                    int yPosition = wallSectionPixelSize.y * fl;

                    for(int x = 0; x < wallSectionPixelSize.x; x++)
                        for(int y = 0; y < wallSectionPixelSize.y; y++)
                            output[xPosition + x + (yPosition + y) * outputWidth] = wallsectionColourArray[x + y * wallSectionPixelSize.x];
                }
            }

            return output;
        }
    }
}