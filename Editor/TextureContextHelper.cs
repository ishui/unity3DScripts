using System.IO;
using UnityEngine;
using UnityEditor;

namespace BuildR2
{
    public class TextureContextHelper
    {
        [MenuItem("Assets/Create/Create Material from Texture", false, 301)]
        private static void CreateMaterial()
        {
            Texture2D tex = Selection.activeObject as Texture2D;
            if(tex == null) return;
            CreateMaterialSurface(tex, true, false);
        }

        [MenuItem("Assets/Create/Create BuildR Surface from Texture", false, 301)]
        private static void CreateSurfaceTexture()
        {
            Texture2D tex = Selection.activeObject as Texture2D;
            if (tex == null) return;
            CreateMaterialSurface(tex, true, true);
        }

        [MenuItem("Assets/Create/Create BuildR Surface from Material", false, 301)]
        private static void CreateSurfaceMaterial()
        {
            Material mat = Selection.activeObject as Material;
            if (mat == null) return;
            CreateSurface(mat);
        }

        // Note that we pass the same path, and also pass "true" to the second argument.
        [MenuItem("Assets/Create Material from Texture", true)]
        private static bool CreateSurfaceTextureValidation()
        {
            return isTexture();
        }

        // Note that we pass the same path, and also pass "true" to the second argument.
        [MenuItem("Assets/Create Surface from Texture", true)]
        private static bool CreateMaterialValidation()
        {
            return isTexture();
        }

        // Note that we pass the same path, and also pass "true" to the second argument.
        [MenuItem("Assets/Create Surface from Material", true)]
        private static bool CreateSurfaceMaterialValidation()
        {
            return isMaterial();
        }

        private static bool isTexture()
        {
            if(Selection.activeObject == null) return false;
            return Selection.activeObject.GetType() == typeof(Texture2D);
        }

        private static bool isMaterial()
        {
            if (Selection.activeObject == null) return false;
            return Selection.activeObject.GetType() == typeof(Material);
        }

        private static string materialPath(string filename)
        {
            BuildRSettings settings = BuildingEditor.GetSettings();
            string pluginLocation = settings.pluginLocation;
            string dirPath = Path.Combine(pluginLocation, "Materials/");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            return AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dirPath, filename));
        }

        private static string SurfacePath(string filename)
        {
            BuildRSettings settings = BuildingEditor.GetSettings();
            string pluginLocation = settings.pluginLocation;
            string dirPath = Path.Combine(pluginLocation, "Surfaces/");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            return AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dirPath, filename));
        }

        private static void CreateMaterialSurface(Texture2D tex, bool createMaterial, bool createSurface)
        {
            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(tex));
            string materialFilename = string.Format("{0}.mat", filename);
            string matPath = materialPath(materialFilename);
            if (createMaterial)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = tex;
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (createSurface)
            {
                Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
                if (mat != null)
                {
                    string surfaceFilename = string.Format("{0}.asset", filename);
                    string surfPath = SurfacePath(surfaceFilename);
                    Surface surface = ScriptableObject.CreateInstance<Surface>();
                    surface.material = mat;
                    AssetDatabase.CreateAsset(surface, surfPath);
                }
            }
            AssetDatabase.Refresh();
        }

        private static void CreateSurface(Material mat)
        {
            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mat));
            string surfaceFilename = string.Format("{0}.asset", filename);
            string surfPath = SurfacePath(surfaceFilename);
            Surface surface = ScriptableObject.CreateInstance<Surface>();
            surface.material = mat;
            AssetDatabase.CreateAsset(surface, surfPath);
            AssetDatabase.Refresh();
        }
    }
}