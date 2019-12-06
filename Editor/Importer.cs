using UnityEditor;

namespace BuildR2
{
    public class Importer : AssetPostprocessor
    {

        public void OnPreprocessModel()
        {
            if(assetPath.Contains("BuildR2/Exported"))
            {
                ModelImporter importer = (ModelImporter)assetImporter;
                importer.importMaterials = true;
                importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
                importer.importBlendShapes = false;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.None;
            }
        }
    }
}