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

using System.Collections.Generic;

namespace BuildR2 {
    public class GenerationUtil
    {
        public static Surface GetSurface(List<Surface> surfaceList, RandomGen rGen, Surface defaultSurface = null)
        {
            int listSize = surfaceList.Count;
            if(surfaceList == null || listSize == 0)
                return defaultSurface;
            int index = rGen.Index(listSize);
            return surfaceList[index];
        }
    }
}