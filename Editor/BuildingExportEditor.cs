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
using System.IO;
using BuildR2;
using UnityEditor;
using UnityEngine;

namespace BuildR2
{
    //TODO merge all models into a single model mesh right!
    public class BuildingExportEditor
    {
        private const string PROGRESSBAR_TEXT = "Exporting Building";
        private const string ROOT_FOLDER = "Assets/Buildr2/Exported/";

        public static void OnInspectorGUI(Building building)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Export Filename");
            building.exportFilename = EditorGUILayout.TextField(building.exportFilename);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Place into Scene");
            building.placeIntoScene = EditorGUILayout.Toggle(building.placeIntoScene);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export", GUILayout.Height(55)))
                Export(building);

            //todo export json version

            EditorGUILayout.EndVertical();
        }

        private static void Export(Building building)
        {
            try
            {
                EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, "", 0.0f);

                int volumeCount = building.numberOfVolumes;
                string foldername = ROOT_FOLDER + building.exportFilename + "/";
                foldername = foldername.Replace(" ", "");

                //check overwrites...
                if (!CreateFolder(foldername))
                {//likely user cancelled export
                    EditorUtility.ClearProgressBar();
                    return;
                }

                int steps = 0;
                for (int v = 0; v < volumeCount; v++)
                {
                    steps++;
                    int floors = building[v].floors;
                    for (int fl = 0; fl < floors; fl++)
                        steps++;
                }

                //export meshes to project
                int stepsComplete = 1;
                Dictionary<Transform, string> transformData = new Dictionary<Transform, string>();
                for (int v = 0; v < volumeCount; v++)
                {
                    IVolume vol = building[v];
                    string progressReport = string.Format("Exporting Volume: {0}", vol.name);
                    float progressPercent = stepsComplete / (float)steps;
                    if (EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, progressReport, progressPercent)) return;
                    string filenameSuffix = vol.name;//todo check uniqueness
                    string filename = string.Format("{0}_{1}_{2}", building.exportFilename, "exterior", filenameSuffix);
                    filename = filename.Replace(" ", "");
                    transformData.Add(vol.visualPart.transform, Path.Combine(foldername, string.Format("{0}{1}", filename, ".fbx")));
                    ExportModelPart(vol.visualPart, filename, foldername);

                    if(building.exportColliders && building.colliderType != BuildingColliderTypes.None)
                    {             
                        string colliderFilename = string.Format("{0}_{1}_{2}", building.exportFilename, "exterior_collider", filenameSuffix);
                        colliderFilename = colliderFilename.Replace(" ", "");
                        transformData.Add(vol.visualPart.colliderPart.transform, Path.Combine(foldername, string.Format("{0}{1}", colliderFilename, ".fbx")));
                        ExportModelCollider(vol.visualPart.colliderPart.mesh, colliderFilename, foldername);
                    }

                    if (building.generateInteriors)
                    {
                        IFloorplan[] floorplans = vol.InteriorFloorplans();
                        int floors = floorplans.Length;
                        for (int fl = 0; fl < floors; fl++)
                        {
                            IFloorplan floorplan = floorplans[fl];

                            progressReport = string.Format("Exporting Floorplan: {0}", floorplan.name);
                            progressPercent = stepsComplete / (float)steps;
                            if (EditorUtility.DisplayCancelableProgressBar(PROGRESSBAR_TEXT, progressReport, progressPercent)) return;

                            string floorplanFilenameSuffix = floorplan.name;//todo check uniqueness
                            string floorplanFilename = string.Format("{0}_{1}_{2}_{3}_{4}", building.exportFilename, vol.name, "interior", (fl + 1), floorplanFilenameSuffix);
                            floorplanFilename = floorplanFilename.Replace(" ", "");
                            transformData.Add(floorplan.visualPart.transform, Path.Combine(foldername, string.Format("{0}{1}", floorplanFilename, ".fbx")));
                            ExportModelPart(floorplan.visualPart, floorplanFilename, foldername);
                            
                            if (building.exportColliders && building.colliderType != BuildingColliderTypes.None)
                            {
                                string colliderFilename = string.Format("{0}_{1}_{2}_{3}_{4}", building.exportFilename, vol.name, "interior_collider", (fl + 1), floorplanFilenameSuffix);
                                colliderFilename = colliderFilename.Replace(" ", "");
                                transformData.Add(floorplan.visualPart.colliderPart.transform, Path.Combine(foldername, string.Format("{0}{1}", colliderFilename, ".fbx")));
                                ExportModelCollider(floorplan.visualPart.colliderPart.mesh, colliderFilename, foldername);
                            }

                            stepsComplete++;
                        }
                    }

                    stepsComplete++;
                }

                //import meshes into new prefab
                AssetDatabase.Refresh();//ensure the database is up to date...
                GameObject baseObject = new GameObject(building.exportFilename);
                
                for(int v = 0; v < volumeCount; v++)
                {
                    IVolume vol = building[v];
                    string externalMeshPath = transformData[vol.visualPart.transform];
                    if(!string.IsNullOrEmpty(externalMeshPath))
                    {
                        GameObject externalModel = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(externalMeshPath));
                        externalModel.name = vol.name;
                        externalModel.transform.parent = baseObject.transform;
                        externalModel.transform.localPosition = vol.transform.localPosition;
                        externalModel.transform.localRotation = vol.transform.localRotation;
                        
                        if (building.exportColliders && building.colliderType != BuildingColliderTypes.None)
                        {
                            string externalColliderPath = transformData[vol.visualPart.colliderPart.transform];
                            if(!string.IsNullOrEmpty(externalColliderPath))
                            {
                                GameObject colliderObject = new GameObject("collider");
                                colliderObject.AddComponent<MeshCollider>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(externalColliderPath, typeof(Mesh));
                                colliderObject.transform.parent = externalModel.transform;
                                colliderObject.transform.localPosition = Vector3.zero;
                                colliderObject.transform.localRotation = Quaternion.identity;
                            }
                        }

                        if (building.generateInteriors)
                        {
							IFloorplan[] floorplans = vol.InteriorFloorplans();
                            int floors = floorplans.Length;
                            for (int fl = 0; fl < floors; fl++)
                            {
                                IFloorplan floorplan = floorplans[fl];
                                string internalMeshPath = transformData[floorplan.visualPart.transform];
                                if (!string.IsNullOrEmpty(internalMeshPath))
                                {
                                    GameObject internalMesh = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(internalMeshPath));
                                    internalMesh.name = floorplan.name;
                                    internalMesh.transform.parent = externalModel.transform;
                                    internalMesh.transform.localPosition = floorplan.transform.localPosition;
                                    internalMesh.transform.localRotation = floorplan.transform.localRotation;

                                    string internalColliderPath = transformData[floorplan.visualPart.colliderPart.transform];
                                    if (!string.IsNullOrEmpty(internalColliderPath))
                                    {
                                        GameObject colliderObject = new GameObject("collider");
                                        colliderObject.AddComponent<MeshCollider>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(internalColliderPath, typeof(Mesh));
                                        colliderObject.transform.parent = internalMesh.transform;
                                        colliderObject.transform.localPosition = Vector3.zero;
                                        colliderObject.transform.localRotation = Quaternion.identity;
                                    }
                                }
                            }
                        }
                    }
                }

                string prefabPath = ROOT_FOLDER + building.exportFilename + "/" + building.exportFilename + ".prefab";
                Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                if (prefab == null)
                    prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                PrefabUtility.ReplacePrefab(baseObject, prefab, ReplacePrefabOptions.ConnectToPrefab);

                if (!building.placeIntoScene)
                    Object.DestroyImmediate(baseObject);

                EditorUtility.ClearProgressBar();
                EditorUtility.UnloadUnusedAssetsImmediate();
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Please report this error in full to Jasper (email@jasperstocker.com) Thanks! : " + e);
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ExportModelPart(IVisualPart part, string filename, string foldername)
        {
            Mesh volMesh = part.mesh;
            Material[] volumeMaterials = part.materials;
            int matCount = volumeMaterials.Length;
            ExportMaterial[] exportMaterials = new ExportMaterial[matCount];
            for (int m = 0; m < matCount; m++)
            {
                exportMaterials[m] = new ExportMaterial();
                exportMaterials[m].name = volumeMaterials[m].name;
                exportMaterials[m].filepath = AssetDatabase.GetAssetPath(volumeMaterials[m]);
                exportMaterials[m].material = volumeMaterials[m];
                exportMaterials[m].generated = false;
            }
            //            string filenameSuffix = vol.name;//todo check uniqueness
            //            string filename = string.Format("{0}_{1}", building.exportFilename, filenameSuffix);
            FBXExporter.Export(foldername, filename, volMesh, exportMaterials, false);
        }

        private static void ExportModelCollider(Mesh mesh, string filename, string foldername)
        {
            ExportMaterial[] exportMaterials = new ExportMaterial[1];
            exportMaterials[0] = new ExportMaterial();
            exportMaterials[0].name = "Standard";
            exportMaterials[0].filepath = "";
            exportMaterials[0].generated = true;
            FBXExporter.Export(foldername, filename, mesh, exportMaterials, false);
        }

        private static bool CreateFolder(string newDirectory)
        {
            if (Directory.Exists(newDirectory))
            {
                if (EditorUtility.DisplayDialog("File directory exists", "Are you sure you want to overwrite the contents of this file?", "Cancel", "Overwrite"))
                {
                    return false;
                }
            }

            try
            {
                Directory.CreateDirectory(newDirectory);
            }
            catch
            {
                EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
                return false;
            }

            return true;
        }
    }
}