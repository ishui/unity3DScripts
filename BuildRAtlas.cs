using UnityEngine;

namespace BuildR2 {
    [System.Serializable]
    public class BuildRAtlas {
        public enum Dimensions {
            x16 = 16,
            x32 = 32,
            x64 = 64,
            x128 = 128,
            x256 = 256,
            x512 = 512,
            x1024 = 1024,
            x2048 = 2048,
            x4096 = 4096,
            x8192 = 8192
        }

        public Dimensions tileSize = Dimensions.x128;
        public Dimensions atlasSize = Dimensions.x4096;
        public int padding = 1;
        public TextureFormat atlasTextureFormat = TextureFormat.ARGB32;

        public WallSection[] wallSections = new WallSection[0];
        public Surface[] surfaces = new Surface[0];
        public Texture2D texture;
        public Rect[] packedRects = new Rect[0];
        public Material material;

        public void UpdateAtlas()
        {
            int wallSectionCount = wallSections.Length;
            int surfaceCount = surfaces.Length;
            int totalEntries = wallSectionCount + surfaceCount;
            if(totalEntries == 0)
                return;
            
            if(material == null)
                material = new Material(Shader.Find("Standard"));
             
            Texture2D[] textures = new Texture2D[totalEntries];
            int pixelWidth = (int)tileSize - padding;
            int atlasWidth = (int)atlasSize;
            for (int w = 0; w < wallSectionCount; w++)
            {
                Texture2D useTexture = wallSections[w].previewTexture;
                if (useTexture == null) continue;
                if (useTexture.width != pixelWidth)
                {
                    Color32[] textureArray;
                    Vector2Int pixelSize = new Vector2Int(pixelWidth, pixelWidth);
                    WallSectionGenerator.Texture(out textureArray, wallSections[w], pixelSize, Vector2.one * 2, true);
                    useTexture = new Texture2D(pixelWidth, pixelWidth, atlasTextureFormat, true);
                    useTexture.SetPixels32(textureArray);
                    useTexture.Apply(true, false);
                }
                textures[w] = useTexture;
            }

            for(int s = 0; s < surfaceCount; s++)
            {
                Texture2D useTexture = surfaces[s].previewTexture as Texture2D;
                if(useTexture == null) continue;
                int textureWidth = useTexture.width;
                int textureHeight = useTexture.height;
                if (textureWidth != pixelWidth) 
                {
                    Color32[] resizeColorArray = TextureScale.NearestNeighbourSample(useTexture.GetPixels32(), textureWidth, textureHeight, pixelWidth, pixelWidth);
                    useTexture.Resize(pixelWidth, pixelWidth, atlasTextureFormat, true);
                    useTexture.SetPixels32(resizeColorArray);
                    useTexture.Apply(true, false);
                }
                textures[wallSectionCount + s] = useTexture;
            }
            texture = new Texture2D(1, 1, atlasTextureFormat, true);
            packedRects = texture.PackTextures(textures, padding, atlasWidth, false);
            texture.Apply(true, false);
            material.mainTexture = texture;
        }

        public Rect GetRect(WallSection section) {
            int wallsectionCount = wallSections.Length;
            for (int w = 0; w < wallsectionCount; w++) {
                if (wallSections[w] == section)
                    return packedRects[w];
            }
            Debug.LogWarning(string.Format("Wall Section ({0}) not present in atlas", section));
            return new Rect();
        }

        public Rect GetRect(Surface surface) {
            int wallsectionCount = wallSections.Length;
            int surfaceCount = surfaces.Length;
            for (int s = 0; s < surfaceCount; s++) {
                if (surfaces[s] == surface)
                    return packedRects[wallsectionCount + s];
            }
            Debug.LogWarning(string.Format("Surface ({0}) not present in atlas", surface));
            return new Rect();
        }
    }
}