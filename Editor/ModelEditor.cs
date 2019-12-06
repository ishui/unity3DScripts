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
	[CustomEditor(typeof(Model))]
	public class ModelEditor : Editor
	{
		private Model _model;
		private Mesh _cubeMesh = null;
		private Material _cubeMat = null;

		private void OnEnable()
		{
			_model = (Model)target;

			if (_cubeMesh == null)
			{
				_cubeMesh = new Mesh();
				Vector3[] verts = new Vector3[8];
				float halfSize = 0.5f;
				verts[0] = new Vector3(-halfSize, -halfSize, -halfSize);
				verts[1] = new Vector3(halfSize, -halfSize, -halfSize);
				verts[2] = new Vector3(-halfSize, -halfSize, halfSize);
				verts[3] = new Vector3(halfSize, -halfSize, halfSize);
				verts[4] = new Vector3(-halfSize, halfSize, -halfSize);
				verts[5] = new Vector3(halfSize, halfSize, -halfSize);
				verts[6] = new Vector3(-halfSize, halfSize, halfSize);
				verts[7] = new Vector3(halfSize, halfSize, halfSize);
				_cubeMesh.vertices = verts;
				_cubeMesh.uv = new Vector2[8];
				int[] tris = {
									  0, 1, 2, 2, 1, 3,
									  0, 4, 1, 4, 5, 1,
									  2, 6, 0, 6, 4, 0,
									  1, 5, 3, 5, 7, 3,
									  3, 7, 2, 7, 6, 2,
									  4, 6, 5, 6, 7, 5
								  };
				_cubeMesh.triangles = tris;
				_cubeMesh.RecalculateBounds();
				_cubeMesh.RecalculateNormals();
			}

			if (_cubeMat == null)
			{
				_cubeMat = new Material(Shader.Find("Unlit/Color"));
				_cubeMat.color = new Color(1, 1, 1, 0.025f);
			}
		}

		public override void OnInspectorGUI()
		{
			Undo.RecordObject(_model, "Model Modification");

			BuildingEditor.BuildRHeader("Model");

			EditorGUILayout.BeginVertical(GUILayout.Width(BuildingEditor.MAIN_GUI_WIDTH));

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.Width(80));
			string newName = EditorGUILayout.DelayedTextField(_model.name);
			if (newName != _model.name)
				BuildingEditorUtils.RenameAsset(_model, newName);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Type", GUILayout.Width(80));
			_model.type = (Model.Types)EditorGUILayout.EnumPopup(_model.type);
			EditorGUILayout.EndHorizontal();

			if (_model.type == Model.Types.Mesh && _model.GetMeshes().Length > 1)
				EditorGUILayout.HelpBox("Mesh mode only available for subjects with a single mesh", MessageType.Warning);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Subject", GUILayout.Width(80));
			_model.subject = EditorGUILayout.ObjectField(_model.subject, typeof(GameObject), false) as GameObject;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Model Anchor", GUILayout.Width(150));
			_model.anchor = EditorGUILayout.Vector3Field("", _model.anchor);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Bounds", GUILayout.Width(150));
			_model.userBounds = EditorGUILayout.BoundsField(_model.userBounds);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if(GUILayout.Button("Reset Bounds"))
				_model.userBounds = _model.modelBounds;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Model Normal", GUILayout.Width(150));
			_model.normal = EditorGUILayout.Vector3Field("", _model.normal);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Rotation Offset", GUILayout.Width(150));
			_model.userRotation = EditorGUILayout.Vector3Field("", _model.userRotation);
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Refresh Data"))
				_model.UpdateInternalData();

			EditorGUILayout.EndVertical();
		}

		public override bool HasPreviewGUI()
		{
			return _model.GetMeshes().Length > 0;
	    }

	    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {
	        InteractivePreview.RESTRICT_ROTATION = false;
	        InteractivePreview.OnInteractivePreviewGUI(r, background, _model, Matrix4x4.identity);
        }
        //    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        //		{
        //			_drag = Drag2D(_drag, r);
        //
        //			if (_mPrevRender == null)
        //				_mPrevRender = new PreviewRenderUtility();
        //
        //			Vector3 bMax = _model.userBounds.size;
        //			Vector3 mMax = _model.userBounds.size;
        //			float max = Mathf.Max(Mathf.Max(bMax.x, Mathf.Max(bMax.y, bMax.z)), Mathf.Max(mMax.x, Mathf.Max(mMax.y, mMax.z)));
        //			float radius = max * 1.333f;
        //			float dist = radius / (Mathf.Sin(_mPrevRender.m_Camera.fieldOfView * Mathf.Deg2Rad));
        //			_mPrevRender.m_Camera.transform.position = Vector2.zero;
        //			_mPrevRender.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
        //			_mPrevRender.m_Camera.transform.position = _mPrevRender.m_Camera.transform.forward * -dist;
        //			_mPrevRender.m_Camera.nearClipPlane = 0.1f;
        //			_mPrevRender.m_Camera.farClipPlane = 500;
        //
        //			_mPrevRender.m_Light[0].intensity = 0.5f;
        //			_mPrevRender.m_Light[0].transform.rotation = Quaternion.Euler(30f, 120f, 0f);
        //			_mPrevRender.m_Light[1].intensity = 0.5f;
        //
        //			_mPrevRender.BeginPreview(r, background);
        //
        //			Matrix4x4 pos = new Matrix4x4();
        //			pos.SetTRS(Vector3.zero, _model.userRotationQuat, Vector3.one);
        //
        //			Mesh[] meshes = _model.GetMeshes();
        //			Model.MaterialArray[] materials = _model.GetMaterials();
        //			int meshCount = meshes.Length;
        //			for (int ms = 0; ms < meshCount; ms++)
        //			{
        //				Mesh mesh = meshes[ms];
        //				Material[] mats = materials[ms].materials;
        //				int materialCount = 0;
        //				if (mats != null) materialCount = mats.Length;
        //				int submeshCount = Mathf.Max(1, mesh.subMeshCount);
        //				int count = Mathf.Max(materialCount, submeshCount);
        //				for (int c = 0; c < count; c++)
        //				{
        //					Material mat = c < materialCount ? mats[c] : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        //					if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        //					_mPrevRender.DrawMesh(mesh, pos, mat, c);
        //				}
        //			}
        //
        //			_mPrevRender.m_Camera.Render();
        //			_mPrevRender.EndAndDrawPreview(r);
        //
        //			// render the bounds using Handles.DrawLine
        //			Vector3 boundsMin = _model.userBounds.min;
        //			Vector3 boundsMax = _model.userBounds.max;
        //
        //			Vector3 p0 = ConvertPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMin.z), _mPrevRender.m_Camera);
        //			Vector3 p1 = ConvertPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z), _mPrevRender.m_Camera);
        //			Vector3 p2 = ConvertPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z), _mPrevRender.m_Camera);
        //			Vector3 p3 = ConvertPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z), _mPrevRender.m_Camera);
        //			Vector3 p4 = ConvertPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z), _mPrevRender.m_Camera);
        //			Vector3 p5 = ConvertPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMin.z), _mPrevRender.m_Camera);
        //			Vector3 p6 = ConvertPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z), _mPrevRender.m_Camera);
        //			Vector3 p7 = ConvertPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMax.z), _mPrevRender.m_Camera);
        //
        //
        //			Handles.BeginGUI();
        //			GUILayout.BeginArea(lastPreviewRect);
        //
        //			Handles.color = Color.yellow;
        //			//bottom
        //			Handles.DrawLine(p0 ,p1);
        //			Handles.DrawLine(p0 ,p2);
        //			Handles.DrawLine(p1 ,p3);
        //			Handles.DrawLine(p2 ,p3);
        //			//sides
        //			Handles.DrawLine(p0 ,p4);
        //			Handles.DrawLine(p1 ,p5);
        //			Handles.DrawLine(p2 ,p6);
        //			Handles.DrawLine(p3 ,p7);
        //			//top
        //			Handles.DrawLine(p4 ,p5);
        //			Handles.DrawLine(p4 ,p6);
        //			Handles.DrawLine(p5 ,p7);
        //			Handles.DrawLine(p6 ,p7);
        //			Handles.ArrowCap(0, Vector3.zero, Quaternion.identity, 100);
        //
        //			GUILayout.EndArea();
        //			Handles.EndGUI();
        //
        ////			Handles.
        //
        //			//			}
        //			if (Event.current.type == EventType.Repaint)
        //				lastPreviewRect = r;
        //		}
        //
        //		private Vector3 ConvertPoint(Vector3 input, Camera cam)
        //		{
        //			Vector3 output = input;
        //			//WTF! *sigh*
        //			output = cam.WorldToScreenPoint(output) * 0.5f;
        //			output.y = -(output.y - cam.pixelHeight * 0.5f);
        //			output.z = 0;
        //			return output;
        //		}
        //
        //		public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        //		{
        //			int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        //			Event current = Event.current;
        //			switch (current.GetTypeForControl(controlID))
        //			{
        //				case EventType.MouseDown:
        //					if (position.Contains(current.mousePosition) && position.width > 50f)
        //					{
        //						GUIUtility.hotControl = controlID;
        //						current.Use();
        //						EditorGUIUtility.SetWantsMouseJumping(1);
        //					}
        //					break;
        //				case EventType.MouseUp:
        //					if (GUIUtility.hotControl == controlID)
        //					{
        //						GUIUtility.hotControl = 0;
        //					}
        //					EditorGUIUtility.SetWantsMouseJumping(0);
        //					break;
        //				case EventType.MouseDrag:
        //					if (GUIUtility.hotControl == controlID)
        //					{
        //						scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
        //						scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
        //						current.Use();
        //						GUI.changed = true;
        //					}
        //					break;
        //			}
        //			return scrollPosition;
        //		}
    }
}
