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
using UnityEditor;

namespace BuildR2
{
    [CustomEditor(typeof(BuildRPart))]
    public class BuildRPartEditor : Editor
    {
        private static BuildRPart PART;
        private static Building BUILDING;
        private bool _firstFrame;

        public void OnEnable()
        {
            _firstFrame = true;
        }

        public void OnSceneGUI()
        {
            if (!_firstFrame) return;
            _firstFrame = false;

            Event evt = Event.current;
            if (evt != null && evt.alt)//do not modify selection
                return;

            if (target != null)
                PART = (BuildRPart)target;

            if (PART != null)
            {
                if (PART.parent != null)
                {
                    Selection.activeGameObject = PART.parent.gameObject;
                    return;
                }

                BUILDING = PART.transform.GetComponentInParent<Building>();

                if (BUILDING != null)
                {
                    Selection.activeGameObject = BUILDING.gameObject;

                    Volume volume = PART.GetComponent<Volume>();
                    if (volume != null && BuildingEditor.volume == null)
                    {
//                        BuildingEditor.MODE = BuildingEditor.EditModes.Volume;
                        BuildingEditor.volume = volume;
                        return;
                    }

                    Floorplan floorplan = PART.GetComponent<Floorplan>();
                    if (floorplan != null && BuildingEditor.floorplan == null)
                    {
                        BUILDING.settings.editMode = BuildREditmodes.Values.Floorplan;
                        volume = PART.transform.parent.GetComponent<Volume>();
                        BuildingEditor.volume = volume;
                        BuildingEditor.floorplan = floorplan;
                        return;
                    }
                }
            }
        }
    }
}