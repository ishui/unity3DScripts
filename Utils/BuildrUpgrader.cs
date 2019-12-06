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
    public class BuildrUpgrader
    {
        public static bool UpgradeData(Building data)
        {
            float currentVersion = BuildrVersion.NUMBER;
            float dataVersion = data.versionNumber;

            if (currentVersion == dataVersion)
            {
                //The data matches the current version of Buildr - do nothing.
                return true;
            }

            if (currentVersion < dataVersion)
            {
                Debug.LogError("BuildR v." + currentVersion + ": Great scot! This data is from the future! (version:" + dataVersion + ") - need to avoid contact to ensure the survival of the universe...");
                return false;//don't touch ANYTHING!
            }

            Debug.Log("BuildR v." + currentVersion + " Upgrading the data from version " + dataVersion + " to version " + currentVersion + "\nRemember to backup your data!");
            
            if (dataVersion < 2.0f)
            {
                //todo
            }

            data.versionNumber = BuildrVersion.NUMBER;//update the data version number once upgrade is complete
            return true;
        }
    }
}

//this class will deal with future updates to BuildR
//and will ensure appropriate updates to data passed through.