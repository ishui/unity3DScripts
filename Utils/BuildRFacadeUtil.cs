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
using UnityEngine;

namespace BuildR2
{
    public class BuildRFacadeUtil
    {
        public static WallSection[,] GenerateFacadeArray(Facade facade, int width, int height)
        {
            WallSection[,] output = new WallSection[width, height];

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    output[x, y] = facade.GetWallSection(x, y);
                }
            }

            return output;
        }

        public static Vector3[] CalculateCurvedFacadeBasePoints(FacadeGenerator.FacadeData facade, int pointCount)
        {
            Vector3 p0 = facade.baseA;
            Vector3 p1 = facade.baseB;
            Vector3 cw0 = facade.controlA;
            Vector3 cw1 = facade.controlB;
            Vector3[] output = new Vector3[pointCount];
            for (int t = 0; t < pointCount; t++)
            {
                Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t / (pointCount - 1f));
                output[t] = cp;
            }
            return output;
        }
        
        public static float CalculateArcLength(FacadeGenerator.FacadeData facade)
        {
            float roughArcLength = 0;
            roughArcLength += Vector3.Distance(facade.baseA, facade.controlA);
            roughArcLength += Vector3.Distance(facade.controlA, facade.controlB);
            roughArcLength += Vector3.Distance(facade.controlB, facade.baseB);
            int subdivisions = Mathf.RoundToInt(roughArcLength);
            float output = 0;
            Vector3 p0 = facade.baseA;
            Vector3 p1 = facade.baseB;
            Vector3 cw0 = facade.controlA;
            Vector3 cw1 = facade.controlB;
            Vector3 last = p0;
            for(int i = 1; i < subdivisions + 1; i++)
            {
                float t = i / (float)subdivisions;
                Vector3 cp = FacadeSpline.Calculate(p0, cw0, cw1, p1, t);
                float dist = Vector3.Distance(last, cp);
                output += dist;
                last = cp;
            }
            return output;
        }

        public static bool HasNullFacades(IBuilding building)
        {
            int planCount = building.numberOfPlans;
            for(int pl = 0; pl < planCount; pl++)
            {
                IVolume plan = building[pl];
                int facadeCount = plan.numberOfPoints;
                for(int f = 0; f < facadeCount; f++)
                {
                    VolumePoint facadeData = plan[f];
                    if(facadeData.facade == null) return true;
                }
            }
            return false;
        }

        public static int MinimumFloor(IBuilding building, IVolume inPlan, int pointIndex)
        {
            int output = 0;

            int subjectActualBaseFloor = building.VolumeBaseFloor(inPlan);
            int subjectActualTopFloor = subjectActualBaseFloor + inPlan.floors;

            int pointIndexB = (pointIndex + 1) % inPlan.numberOfPoints;
            Vector2Int p0 = inPlan[pointIndex].position;
            Vector2Int p1 = inPlan[pointIndexB].position;

            List<IVolume> aboveVolumeList = building.AllAboveVolumes(inPlan);

            int volumeCount = building.numberOfVolumes;
            for(int f = 0; f < volumeCount; f++)
            {
	            IVolume volume = building[f];
//                if(inPlan == volume) continue;

                int actualBaseFloor = building.VolumeBaseFloor(volume);
                int actualTopFloor = actualBaseFloor + volume.floors;
                if (!(subjectActualBaseFloor < actualTopFloor && subjectActualTopFloor > actualBaseFloor)) continue; //volumes don't affect each other
                                                                                                                     //                if(volume.abovePlans.Contains(inPlan)) continue;
                if (aboveVolumeList.Contains(volume)) continue;//this volume is connected above the subject and cannot effect a facade
                if(inPlan.AbovePlanList().Contains(volume)) continue;//this volume is connected above the subject and cannot effect a facade
                if (building.AllAboveVolumes(volume).Contains(inPlan)) continue;//this volume is connected below the subject and cannot effect
//                if (building.AllAboveVolumes(inPlan).Contains(volume)) continue;//this volume is connected below the subject and cannot effect



                //                if(inPlan.abovePlans.Contains(volume)) continue;
                int pointCount = volume.numberOfPoints;
                for(int p = 0; p < pointCount; p++)
                {
                    if(volume == inPlan && p == pointIndex)//self connecting plans need to cut off facades
                        continue;

                    Vector2Int pB0 = volume[p].position;
                    if(pB0 != p0 && pB0 != p1) continue;
                    int pB = (p + 1) % volume.numberOfPoints;
                    Vector2Int pB1 = volume[pB].position;
                    if (pB1 != p0 && pB1 != p1) continue;

                    if(output == 0) output = volume.floors;
                    else output = Mathf.Min(volume.floors, output);
                }
            }
            return output;
        }

        public static bool HasParapet(IBuilding building, IVolume inVolume, int pointIndex)
        {

            if (inVolume[pointIndex].isGabled) return false;//gabled walls do not have a parapet

//            if(inVolume.abovePlans.Count > 0) return false;//todo calculate this

            int pointIndexB = (pointIndex + 1) % inVolume.numberOfPoints;
            Vector2Int p0 = inVolume[pointIndex].position;
            Vector2Int p1 = inVolume[pointIndexB].position;
            float volumeHeight = inVolume.planTotalHeight;

            int volumeCount = building.numberOfVolumes;
            for (int f = 0; f < volumeCount; f++)
            {
	            IVolume otherVolume = building[f];

                if(volumeHeight > otherVolume.planTotalHeight) continue;//other volume shorter - will not affect
                
                if (otherVolume.ContainsPlanAbove(inVolume)) continue;//lower floorplans do not effect parapet

                int pointCount = otherVolume.numberOfPoints;
                for (int p = 0; p < pointCount; p++)
                {
                    if (otherVolume == inVolume && p == pointIndex)//floorplan might join itself - ignore comparing facade to itself
                        continue;

                    Vector2Int pB0 = otherVolume[p].position;
                    if (pB0 != p0 && pB0 != p1) continue;//points don't match
                    int pB = (p + 1) % otherVolume.numberOfPoints;
                    Vector2Int pB1 = otherVolume[pB].position;
                    if (pB1 != p0 && pB1 != p1) continue;//points don't match

                    return false;
                }
            }

            return true;
        }
    }
}