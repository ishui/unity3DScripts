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
  [CustomEditor(typeof(Gable))]
  public class GableEditor : Editor
  {
    public Material blueprintMaterial;

    private Gable _gable;
    private PreviewRenderUtility _mPrevRender;
    private Vector2 _drag;
    private Mesh _plane;

    private void OnEnable()
    {
      _gable = (Gable)target;
      _plane = Primitives.Plane(10);
      //            _blueprintMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/BuildR2/Materials/Blueprint.mat");//TODO make independent of BuildR location
    }


    public override void OnInspectorGUI()
    {
      //            BuildRSettings settings = BuildRSettings.GetSettings();

      Undo.RecordObject(_gable, "Gable Modification");

      BuildingEditor.BuildRHeader("Gable");

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Name", GUILayout.Width(80));
      string newName = EditorGUILayout.DelayedTextField(_gable.name);
      if (newName != _gable.name)
        BuildingEditorUtils.RenameAsset(_gable, newName);
      EditorGUILayout.EndHorizontal();

      //            EditorGUILayout.BeginHorizontal();
      //            EditorGUILayout.LabelField("Thickness");
      //            _gable.thickness = EditorGUILayout.Slider(_gable.thickness, 0.1f, 5);
      //            EditorGUILayout.EndHorizontal();

      //            EditorGUILayout.BeginHorizontal();
      //            EditorGUILayout.LabelField("Additional Height");
      //            _gable.additionalHeight = EditorGUILayout.Slider(_gable.additionalHeight, 0.0f, 10f);
      //            EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Surface");
      _gable.surface = EditorGUILayout.ObjectField(_gable.surface, typeof(Surface), false) as Surface;
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Gable Segments");
      _gable.segments = EditorGUILayout.IntSlider(_gable.segments, 1, 24);
      EditorGUILayout.EndHorizontal();

      int partCount = _gable.count;
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(string.Format("Gable Parts: {0}", partCount));
      if (GUILayout.Button("Add Part", GUILayout.Width(90)))
      {
        _gable.AddNewPart();
        partCount = _gable.count;
      }
      EditorGUILayout.EndHorizontal();

      for (int p = 0; p < partCount; p++)
      {
        GablePart part = _gable[p];

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("Part {0}", p + 1));

        if (GUILayout.Button("Insert", GUILayout.Width(70)))
        {
          _gable.InsertNewPart(p);
          break;
        }

        if (GUILayout.Button("Remove", GUILayout.Width(70)))
        {
          _gable.RemovePartAt(p);
          break;//just kill the loop for now
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type", GUILayout.Width(50));
        part.type = (GablePart.Types)EditorGUILayout.EnumPopup(part.type, GUILayout.Width(90));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Size", GUILayout.Width(50));

        Vector2 size = part.size;
        switch (part.type)
        {
          case GablePart.Types.Horizonal:
            //                        size.x = EditorGUILayout.DelayedFloatField(size.x);
            size.x = EditorGUILayout.FloatField("\t", size.x);
            break;
          case GablePart.Types.Vertical:
            size.y = EditorGUILayout.FloatField("\t", size.y);
            break;
          default:
            size = EditorGUILayout.Vector2Field("", size);
            break;
        }
        part.size = size;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        _gable[p] = part;//reassign to check for modification
      }

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Add Part", GUILayout.Width(90)))
        _gable.AddNewPart();
      EditorGUILayout.EndHorizontal();

      if (GUI.changed)
      {
        Repaint();
        EditorUtility.SetDirty(_gable);
      }

      //            DrawDefaultInspector();
    }



    public override bool HasPreviewGUI()
    {
      return true;
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
      Material mat;
      if (_gable.surface != null) mat = _gable.surface.material;
      else mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

      InteractivePreview.PLANE = _plane;
      InteractivePreview.PLANE_MATERIAL = blueprintMaterial;
      InteractivePreview.RESTRICT_ROTATION = false;
      InteractivePreview.OnInteractivePreviewGui(r, background, _gable.previewMesh, new[] { mat });
    }



    //        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    //        {
    //            _drag = Drag2D(_drag, r);
    //
    //            if (_mPrevRender == null)
    //                _mPrevRender = new PreviewRenderUtility();
    //
    //            Vector3 max = _gable.previewMesh.bounds.size;
    //            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 0.999f;
    //            float dist = radius / (Mathf.Sin(_mPrevRender.camera.fieldOfView * Mathf.Deg2Rad));
    //            _mPrevRender.camera.transform.position = Vector2.zero;
    //            _mPrevRender.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
    //            _mPrevRender.camera.transform.position = _mPrevRender.camera.transform.forward * -dist;
    //            _mPrevRender.camera.nearClipPlane = 0.1f;
    //            _mPrevRender.camera.farClipPlane = 500;
    //
    //            _mPrevRender.lights[0].intensity = 0.5f;
    //            _mPrevRender.lights[0].transform.rotation = Quaternion.Euler(45f, 45f, 0f);
    //            _mPrevRender.lights[1].intensity = 0.5f;
    //
    //            _mPrevRender.BeginPreview(r, background);
    //
    //            if (_plane != null && blueprintMaterial != null)
    //            {
    //                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(-25, -25, 1), Quaternion.identity, new Vector3(10, 10, 1));
    //                _mPrevRender.DrawMesh(_plane, matrix, blueprintMaterial, 0);
    //                _mPrevRender.camera.Render();
    //            }
    //
    //            
    //            int submeshCount = _gable.previewMesh.subMeshCount;
    //            for (int c = 0; c < submeshCount; c++)
    //            {
    //                Material mat;
    //                if(_gable.surface != null) mat = _gable.surface.material;
    //                else mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
    //                _mPrevRender.DrawMesh(_gable.previewMesh, Matrix4x4.identity, mat, c);
    //            }
    //            _mPrevRender.camera.Render();
    //            Texture texture = _mPrevRender.EndPreview();
    //
    //            GUI.DrawTexture(r, texture);
    //        }
    //
    //        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    //        {
    //            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
    //            Event current = Event.current;
    //            switch (current.GetTypeForControl(controlID))
    //            {
    //                case EventType.MouseDown:
    //                    if (position.Contains(current.mousePosition) && position.width > 50f)
    //                    {
    //                        GUIUtility.hotControl = controlID;
    //                        current.Use();
    //                        EditorGUIUtility.SetWantsMouseJumping(1);
    //                    }
    //                    break;
    //                case EventType.MouseUp:
    //                    if (GUIUtility.hotControl == controlID)
    //                    {
    //                        GUIUtility.hotControl = 0;
    //                    }
    //                    EditorGUIUtility.SetWantsMouseJumping(0);
    //                    break;
    //                case EventType.MouseDrag:
    //                    if (GUIUtility.hotControl == controlID)
    //                    {
    //                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
    //                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
    //                        current.Use();
    //                        GUI.changed = true;
    //                    }
    //                    break;
    //            }
    //            return scrollPosition;
    //        }
  }
}
