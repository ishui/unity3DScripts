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

namespace BuildR2
{
    public class GenerateMesh
    {
        public static void Generate(IBuilding building)
        {


            switch (building.meshType)
            {
                default:
                    FullMesh.Generate(building);
                    break;
                case BuildingMeshTypes.None:
                    ClearVisuals(building);
                    break;
                case BuildingMeshTypes.Box:
                    SimpleBuildingGenerator.Generate(building);
                    break;
                case BuildingMeshTypes.Simple:
                    SimpleBuildingGenerator.Generate(building);
                    break;
            }
        }

        public static void ClearVisuals(IBuilding building)
        {
            int numberOfVolumes = building.numberOfPlans;
            for (int v = 0; v < numberOfVolumes; v++)
                ClearVisuals(building[v]);
        }

        public static void ClearVisuals(IVolume volume)
        {
            volume.visualPart.Clear();
            volume.visualPart.dynamicMesh.Clear();
            volume.visualPart.colliderMesh.Clear();
        }
    }
}