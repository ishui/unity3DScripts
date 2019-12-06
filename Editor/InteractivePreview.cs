using UnityEngine;
using UnityEditor;

namespace BuildR2 {
    public class InteractivePreview {
        private enum InteractionModes {
            Orbit,
            Offset
        }

        public static Mesh PLANE;
        public static Material PLANE_MATERIAL;
        public static bool RESTRICT_ROTATION = true;


        private static PreviewRenderUtility PREV_RENDER;
        private static Vector2 DRAG = Vector2.zero;
        private static float ZOOM = 1;
        private static Vector3 OFFSET = Vector3.zero;
        private static InteractionModes MODE = InteractionModes.Orbit;
        private static float INTERACTION_SCALE = 1;
        private static Rect lastPreviewRect = new Rect();

        private static Camera camera {
            get {
#if UNITY_2017_1_OR_NEWER
                return  PREV_RENDER.camera;
#else
                return PREV_RENDER.m_Camera;
#endif
            }
        }

        private static Light[] lights {
            get {
#if UNITY_2017_1_OR_NEWER
                return  PREV_RENDER.lights;
#else
                return PREV_RENDER.m_Light;
#endif
            }
        }



        public static void Reset() {
            DRAG = Vector2.zero;
            ZOOM = 1;
            OFFSET = Vector3.zero;
        }

        //TODO support mesh siblings
        public static void OnInteractivePreviewGui(Rect r, GUIStyle background, Mesh mesh, Material[] materials, Model[] models = null, Matrix4x4[] modelMatricies = null) {
            UpdateInteraction(r);
            BuildScene(r, background, new[] { mesh });
            AddMesh(mesh, materials, Matrix4x4.identity);
            if (models != null)
                AddModels(models, modelMatricies);
            Render(r);
        }

        public static void OnInteractivePreviewGUI(Rect r, GUIStyle background, Model model, Matrix4x4 matrix) {
            UpdateInteraction(r);

            Matrix4x4 pos = new Matrix4x4();
            pos.SetTRS(Vector3.zero, model.userRotationQuat, Vector3.one);
            
            BuildScene(r, background, model.GetMeshes());
            AddModel(model, matrix);
            Render(r);

            // render the bounds using Handles.DrawLine
            Vector3 boundsMin = model.userBounds.min;
            Vector3 boundsMax = model.userBounds.max;

            Vector3 p0 = ConvertPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMin.z), camera);
            Vector3 p1 = ConvertPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z), camera);
            Vector3 p2 = ConvertPoint(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z), camera);
            Vector3 p3 = ConvertPoint(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z), camera);
            Vector3 p4 = ConvertPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z), camera);
            Vector3 p5 = ConvertPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMin.z), camera);
            Vector3 p6 = ConvertPoint(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z), camera);
            Vector3 p7 = ConvertPoint(new Vector3(boundsMax.x, boundsMax.y, boundsMax.z), camera);

            Handles.BeginGUI();
            GUILayout.BeginArea(lastPreviewRect);

            Handles.color = Color.yellow;
            //bottom
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p0, p2);
            Handles.DrawLine(p1, p3);
            Handles.DrawLine(p2, p3);
            //sides
            Handles.DrawLine(p0, p4);
            Handles.DrawLine(p1, p5);
            Handles.DrawLine(p2, p6);
            Handles.DrawLine(p3, p7);
            //top
            Handles.DrawLine(p4, p5);
            Handles.DrawLine(p4, p6);
            Handles.DrawLine(p5, p7);
            Handles.DrawLine(p6, p7);
//            Handles.ArrowCap(0, Vector3.zero, Quaternion.identity, 100);

            GUILayout.EndArea();
            Handles.EndGUI();
            
            if (Event.current.type == EventType.Repaint)
                lastPreviewRect = r;
        }

        public static void UpdateInteraction(Rect rect) {
            int controlId = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId)) {
                case EventType.MouseDown:
                    if (rect.Contains(current.mousePosition) && rect.width > 50f && current.isMouse) {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);

                        switch (current.button) {
                            default:
                                MODE = InteractionModes.Orbit;
                                break;

                            case 1:
                                MODE = InteractionModes.Offset;
                                break;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId) {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId) {
                        switch (MODE) {
                            case InteractionModes.Orbit:
                                DRAG -= current.delta * (!current.shift ? 1 : 0.3f) / Mathf.Min(rect.width, rect.height) * 140f;
                                DRAG.y = Mathf.Clamp(DRAG.y, -90f, 90f);
                                if (RESTRICT_ROTATION)
                                    DRAG.x = Mathf.Clamp(DRAG.x, -90f, 90f);

                                break;

                            case InteractionModes.Offset:
                                OFFSET.x += -current.delta.x * INTERACTION_SCALE;
                                OFFSET.y += current.delta.y * INTERACTION_SCALE;
                                break;
                        }
                        current.Use();
                        GUI.changed = true;
                    }
                    break;

                case EventType.ScrollWheel:
                    if (rect.Contains(current.mousePosition) && rect.width > 50f) {
                        ZOOM = Mathf.Clamp(ZOOM + current.delta.y * 0.01f, 0.1f, 2);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
        }

        private static void BuildScene(Rect r, GUIStyle background, Mesh[] meshes) {
            int meshCount = meshes.Length;

            UpdateInteraction(r);

            if (PREV_RENDER == null)
                PREV_RENDER = new PreviewRenderUtility();

            Bounds sceneBounds = new Bounds(meshes[0].bounds.center, Vector3.zero);
            sceneBounds.Expand(Vector3.one * 0.1f);
            for (int m = 0; m < meshCount; m++)
                sceneBounds.Encapsulate(meshes[m].bounds);
            Vector3 max = sceneBounds.size;
            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z)) * 1.05f;
            INTERACTION_SCALE = radius * 0.001f;
            float dist = radius / (Mathf.Sin(camera.fieldOfView * Mathf.Deg2Rad)) * ZOOM;
            camera.transform.position = Vector2.zero;
            camera.transform.rotation = Quaternion.Euler(new Vector3(-DRAG.y, -DRAG.x, 0));
            camera.transform.position = camera.transform.forward * -dist;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500;

#if UNITY_2017_1_OR_NEWER
            float baseColour = 0.4f;
            PREV_RENDER.ambientColor = new Color(baseColour,baseColour,baseColour);
#endif

            lights[0].intensity = 0.65f;
            lights[0].transform.rotation = Quaternion.Euler(30f, -20f, 0f);

            PREV_RENDER.BeginPreview(r, background);

            if (PLANE != null && PLANE_MATERIAL != null) {
#if UNITY_2017_1_OR_NEWER
                Vector3 position = new Vector3(0, 0, Mathf.Max(0.2f, max.z)) - OFFSET;
                Vector3 scale = max * 3000;
#else
                Vector3 position = new Vector3(-max.x * 2, -max.y * 2, Mathf.Max(0.5f, max.z * 1.5f)) - OFFSET;
                Vector3 scale = max * 3;
#endif
                Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);
                PREV_RENDER.DrawMesh(PLANE, matrix, PLANE_MATERIAL, 0);
            }
        }

        private static void AddMeshes(Mesh[] meshes, Material[][] materials, Matrix4x4[] matricies) {
            int meshCount = meshes.Length;
            for (int m = 0; m < meshCount; m++) {
                Mesh mesh = meshes[m];
                Material[] meshMaterials = materials[m];
                Matrix4x4 matrix = matricies[m];
                AddMesh(mesh, meshMaterials, matrix);
            }
        }

        private static void AddMesh(Mesh mesh, Material[] materials, Matrix4x4 matrix) {
            int materialCount = materials.Length;
            int submeshCount = mesh.subMeshCount;
            for (int c = 0; c < submeshCount; c++) {
                Material mat = c < materialCount ? materials[c] : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                if (mat == null) mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                Matrix4x4 useMatrix = matrix * Matrix4x4.TRS(-mesh.bounds.center - OFFSET, Quaternion.identity, Vector3.one);
                PREV_RENDER.DrawMesh(mesh, useMatrix, mat, c);
            }
        }

        private static void AddModels(Model[] models, Matrix4x4[] matricies) {
            int modelCount = models.Length;
            for (int m = 0; m < modelCount; m++) {
                AddModel(models[m], matricies[m]);
            }
        }

        private static void AddModel(Model model, Matrix4x4 matrix) {
            Mesh[] meshes = model.GetMeshes();
            int meshCount = meshes.Length;
            Material[][] materials = Model.MaterialArray.ToArray(model.GetMaterials());
            Matrix4x4[] matricies = new Matrix4x4[meshCount];
            for(int m = 0; m < meshCount; m++)
                matricies[m] = matrix;
            AddMeshes(meshes, materials, matricies);
        }

        private static void Render(Rect r) {
            camera.Render();
            Texture texture = PREV_RENDER.EndPreview();

            GUI.DrawTexture(r, texture);

            PLANE = null;
            PLANE_MATERIAL = null;
        }

        private static Vector3 ConvertPoint(Vector3 input, Camera cam) {
            Vector3 output = input - OFFSET;
            //WTF! *sigh*
            output = cam.WorldToScreenPoint(output) * 0.5f;
            output.y = -(output.y - cam.pixelHeight * 0.5f);
            output.z = 0;
            return output;
        }
    }
}