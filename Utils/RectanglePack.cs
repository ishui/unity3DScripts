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

public class RectanglePack
{

    public static int Pack(Rect[] rects, int padding)
    {
        int numberOfRects = rects.Length;

        //sort rects based on size - large to small
        float[] rectSizes = new float[numberOfRects];
        for (int i = 0; i < numberOfRects; i++)
            rectSizes[i] = rects[i].width;// only judge based on width   // *rects[i].height;
        List<int> rectSizeOrder = new List<int>();
        for (int i = 0; i < numberOfRects; i++)
        {
            float largestSize = 0;
            int largestSizeIndex = 0;
            for (int j = 0; j < numberOfRects; j++)
            {
                if (rectSizeOrder.Contains(j))
                    continue;
                float thisSize = rectSizes[j];
                if (thisSize > largestSize)
                {
                    largestSize = thisSize;
                    largestSizeIndex = j;
                }
            }
            rectSizeOrder.Add(largestSizeIndex);
        }


        int currentSize = 0;
        List<int> sliceWidths = new List<int>() { };
        List<int> sliceHeights = new List<int>() { };
        
        for (int i = 0; i < numberOfRects; i++)
        {
            Rect thisRect = rects[rectSizeOrder[i]];
            int thisRectWidth = (int)thisRect.width;
            int thisRectHeight = (int)thisRect.height;

            bool placed = false;
            //try to fit into a slice
            int slices = sliceWidths.Count;
            int slicePosition = 0;
            for (int s = 0; s < slices; s++)
            {
                //does the rect fit ontop of a slice?
                if (thisRectHeight + sliceHeights[s] + padding*2 < currentSize && thisRectWidth+padding*2 <= sliceWidths[s])
                {
                    thisRect.x = slicePosition+padding;
                    thisRect.y = sliceHeights[s]+padding;

                    if (thisRectWidth != sliceWidths[s])//split the slice
                    {
                        //amend the slices
                        sliceWidths.Insert(s + 1, sliceWidths[s] - (thisRectWidth+padding*2));
                        sliceHeights.Insert(s + 1, sliceHeights[s]);
                        sliceWidths[s] = thisRectWidth+padding*2;
                    }

                    sliceHeights[s] += thisRectHeight+padding*2;
                    placed = true;
                    break;
                }
                slicePosition += sliceWidths[s];
            }

            if (!placed)
            {
                //else start a new slice
                thisRect.x = currentSize+padding;
                thisRect.y = padding;
                sliceWidths.Add(thisRectWidth+padding*2);
                sliceHeights.Add(thisRectHeight+padding*2);
                currentSize += thisRectWidth + padding*2;
            }

            rects[rectSizeOrder[i]] = thisRect;//reassign back
        }

        return currentSize;
    }

    public static int CheckMaxScale(Rect[] rects, int currentWidth, int maxSize)
    {
        //determine the resize scale and apply that to the rects
        float packedScale = 1;
        int numberOfRects = rects.Length;
        if (currentWidth > maxSize)
        {
            packedScale = maxSize / (float)currentWidth;
            for (int i = 0; i < numberOfRects; i++)
            {
                Rect thisRect = rects[i];
                thisRect.x *= packedScale;
                thisRect.y *= packedScale;
                thisRect.width *= packedScale;
                thisRect.height *= packedScale;
                rects[i] = thisRect;
            }
            currentWidth = Mathf.RoundToInt(packedScale * currentWidth);
        }
        else
        {
            currentWidth = (int)Mathf.Pow(2, (Mathf.FloorToInt(Mathf.Log(currentWidth - 1, 2)) + 1));//find the next power of two
        }
        return currentWidth;
    }

    public static Rect[] ConvertToUVSpace(Rect[] pixelSpaceRect, int imageSize)
    {
        int arraySize = pixelSpaceRect.Length;
        Rect[] uvSpace = new Rect[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            Rect newRect = new Rect();
            newRect.x = pixelSpaceRect[i].x / imageSize;
            newRect.y = pixelSpaceRect[i].y / imageSize;
            newRect.width = pixelSpaceRect[i].width / imageSize;
            newRect.height = pixelSpaceRect[i].height / imageSize;
            uvSpace[i] = newRect;
        }
        return uvSpace;
    }
}
