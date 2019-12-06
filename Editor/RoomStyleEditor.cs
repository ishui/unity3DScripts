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
using System.Collections;
using UnityEditor;

namespace BuildR2
{
  [CustomEditor(typeof(RoomStyle))]
  public class RoomStyleEditor : Editor
  {
    private RoomStyle _roomStyle;
    private static string nameError = "";
    private static BuildRMesh bMesh;
    private static Mesh mesh;
    private PreviewRenderUtility _mPrevRender;
    private Vector2 _drag;

    private void OnEnable()
    {
      _roomStyle = (RoomStyle)target;

      bMesh = new BuildRMesh("preview mesh");
      float width = 3;
      float height = 1.5f;
      Vector3 v0 = new Vector3(-width, -height, -width);
      Vector3 v1 = new Vector3(width, -height, -width);
      Vector3 v2 = new Vector3(-width, -height, width);
      Vector3 v3 = new Vector3(width, -height, width);
      Vector3 v4 = new Vector3(-width, height, -width);
      Vector3 v5 = new Vector3(width, height, -width);
      Vector3 v6 = new Vector3(-width, height, width);
      Vector3 v7 = new Vector3(width, height, width);
      bMesh.AddPlane(v0, v1, v2, v3, 0);
      bMesh.AddPlane(v1, v0, v5, v4, 1);
      bMesh.AddPlane(v3, v1, v7, v5, 1);
      bMesh.AddPlane(v0, v2, v4, v6, 1);
      bMesh.AddPlane(v2, v3, v6, v7, 1);
      bMesh.AddPlane(v6, v7, v4, v5, 2);
      mesh = new Mesh();
      bMesh.submeshLibrary.enabled = false;
      bMesh.Build(mesh);
    }

    private void OnDisable()
    {
      if (bMesh != null)
        bMesh.Clear();
      bMesh = null;
      if (mesh != null)
        mesh.Clear();
      mesh = null;
    }

    public override void OnInspectorGUI()
    {
      EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));
      OnInspectorGUI_S(_roomStyle);

      EditorGUILayout.EndVertical();
    }


    public static void OnInspectorGUI_S(RoomStyle roomStyle)
    {
      BuildingEditor.BuildRHeader("Room Style");


      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Name", GUILayout.Width(120));
      string newName = EditorGUILayout.DelayedTextField(roomStyle.name);
      if (newName != roomStyle.name)
        nameError = BuildingEditorUtils.RenameAsset(roomStyle, newName);
      EditorGUILayout.EndHorizontal();

      if (nameError.Length > 0)
        EditorGUILayout.HelpBox(nameError, MessageType.Error);

      EditorGUILayout.BeginVertical("Box");
      EditorGUILayout.LabelField("Surfaces");
      roomStyle.floorSurface = (Surface)EditorGUILayout.ObjectField("Floor", roomStyle.floorSurface, typeof(Surface), false);
      roomStyle.wallSurface = (Surface)EditorGUILayout.ObjectField("Wall", roomStyle.wallSurface, typeof(Surface), false);
      roomStyle.ceilingSurface = (Surface)EditorGUILayout.ObjectField("Ceiling", roomStyle.ceilingSurface, typeof(Surface), false);
      EditorGUILayout.EndVertical();
    }



    public override bool HasPreviewGUI()
    {
      return true;
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
      Material[] mats = new Material[3];

      if (_roomStyle.floorSurface != null)
        mats[0] = _roomStyle.floorSurface.material;
      else
        mats[0] = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

      if (_roomStyle.wallSurface != null)
        mats[1] = _roomStyle.wallSurface.material;
      else
        mats[1] = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

      if (_roomStyle.ceilingSurface != null)
        mats[2] = _roomStyle.ceilingSurface.material;
      else
        mats[2] = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
      InteractivePreview.RESTRICT_ROTATION = false;
      InteractivePreview.OnInteractivePreviewGui(r, background, mesh, mats);
    }

    //    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    //        {
    //
    //            BuildingEditorUtils.OnInteractivePreviewGUI(ref _mPrevRender, ref _drag, mesh, mats, r, background, 7);
    //            
    //        }
  }
}