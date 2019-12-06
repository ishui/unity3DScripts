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
using System.Collections.Generic;

namespace BuildR2
{
	public class SubmeshLibrary
	{
		public static Surface DEFAULT_SURFACE;

		public bool enabled = true;
		public Dictionary<Surface, int> SURFACE_SUBMESH = new Dictionary<Surface, int>();
        public Dictionary<Material, int> MATERIAL_SUBMESH = new Dictionary<Material, int>();

        public Dictionary<int, Object> SUBMESH = new Dictionary<int, Object>();
        public Dictionary<int, Material> SUBMESH_MATERIAL = new Dictionary<int, Material>();
	    public Dictionary<int, bool> TILED_SUBMESH = new Dictionary<int, bool>();
        public int SUBMESH_COUNT = 0;

		public List<Surface> SURFACES = new List<Surface>();
        public List<Material> MATERIALS = new List<Material>();

        public Dictionary<WallSection, int[]> WALLSECTION_SUBMESH_MAPPING = new Dictionary<WallSection, int[]>();
        public Dictionary<WallSection, bool[]> WALLSECTION_SUBMESH_TILING = new Dictionary<WallSection, bool[]>();

		public static Material BaseNull { get { return new Material(Shader.Find("Standard")); } }

		public SubmeshLibrary()
		{
//		    Debug.Log("new sml");
			Add(BaseNull);
		}

		public void Clear()
        {
	        enabled = true;
			SURFACES.Clear();
            MATERIALS.Clear();
            SURFACE_SUBMESH.Clear();
            MATERIAL_SUBMESH.Clear();
            TILED_SUBMESH.Clear();
            SUBMESH.Clear();
	        SUBMESH_MATERIAL.Clear();
            SUBMESH_COUNT = 0;
            WALLSECTION_SUBMESH_MAPPING.Clear();
            WALLSECTION_SUBMESH_TILING.Clear();

	        Add(BaseNull);
        }

        public void Clone(ref SubmeshLibrary clone)
        {
            clone.Clear();
            clone.SURFACES.AddRange(SURFACES);
            clone.MATERIALS.AddRange(MATERIALS);
            clone.SUBMESH = new Dictionary<int, Object>(SUBMESH);
            clone.SUBMESH_MATERIAL = new Dictionary<int, Material>(SUBMESH_MATERIAL);
            clone.TILED_SUBMESH = new Dictionary<int, bool>(TILED_SUBMESH);
            clone.SUBMESH_COUNT = SUBMESH_COUNT;
            clone.WALLSECTION_SUBMESH_MAPPING = new Dictionary<WallSection, int[]>(WALLSECTION_SUBMESH_MAPPING);
            clone.WALLSECTION_SUBMESH_TILING = new Dictionary<WallSection, bool[]>(WALLSECTION_SUBMESH_TILING);
		}

	    public bool isTiled(int submesh)
	    {
	        if(TILED_SUBMESH.ContainsKey(submesh)) return TILED_SUBMESH[submesh];
            return false;
	    }

	    public int[] MapSubmeshes(List<Material> mats)
	    {
		    int matCount = mats.Count;
			int[] output = new int[matCount];
		    for(int m = 0; m < matCount; m++)
		    {
			    Material mat = mats[m];
			    output[m] = SubmeshAdd(mat);
			    if(output[m] == -1) output[m] = 0;
		    }
		    return output;
	    }

        public int Submesh(Surface surface)
        {
            if (SURFACE_SUBMESH.ContainsKey(surface))
                return SURFACE_SUBMESH[surface];
            return -1;
        }

	    public int SubmeshAdd(Surface surface)
	    {
	        if (surface == null) return 0;
            if (SURFACE_SUBMESH.ContainsKey(surface))
	            return SURFACE_SUBMESH[surface];
	        return Add(surface);
	    }

        public int Submesh(Material material)
        {
            if (MATERIAL_SUBMESH.ContainsKey(material))
                return MATERIAL_SUBMESH[material];
            return -1;
		}

		public int SubmeshAdd(Material material)
		{
			if (material == null) return 0;
			if (MATERIAL_SUBMESH.ContainsKey(material))
				return MATERIAL_SUBMESH[material];
			return Add(material);
		}

		public int Add(Surface surface)
		{
			int output = -1;
			if (surface == null)
				return output;

			if (SURFACE_SUBMESH.ContainsKey(surface))//surface already present - return submesh index
				output = SURFACE_SUBMESH[surface];
			else
			{
				if (surface.material != null)
				{
					SURFACES.Add(surface);
					int materialSubmesh = Add(surface.material);
					SURFACE_SUBMESH.Add(surface, materialSubmesh);
                    if(!TILED_SUBMESH.ContainsKey(materialSubmesh))
                        TILED_SUBMESH.Add(materialSubmesh, surface.tiled);
					output = materialSubmesh;
				}
			}

			return output;
	    }

	    public int[] AddRange(Surface[] surfaces) {
	        int count = surfaces.Length;
	        int[] output = new int[count];
	        for (int s = 0; s < count; s++) {
	            output[s] = Add(surfaces[s]);
	        }
	        return output;
	    }

	    public int[] AddRange(Material[] materials) {
	        int count = materials.Length;
	        int[] output = new int[count];
	        for (int s = 0; s < count; s++) {
	            output[s] = Add(materials[s]);
	        }
	        return output;
	    }

        public int Add(Material material)
        {
            int output = -1;
            if (material == null)
				return output;
            if(material.name == "Standard" && SUBMESH_COUNT > 0)
                return 0;

            if (MATERIAL_SUBMESH.ContainsKey(material))//material already present - return submesh index
                output = MATERIAL_SUBMESH[material];
            else
            {
                MATERIAL_SUBMESH.Add(material, SUBMESH_COUNT);//add new material instance to library
                SUBMESH_MATERIAL.Add(SUBMESH_COUNT, material);
                SUBMESH.Add(SUBMESH_COUNT, material);
                MATERIALS.Add(material);
                output = SUBMESH_COUNT;
//                Debug.Log(output + " " + material.name);
                SUBMESH_COUNT++;
                //                }
            }

            return output;
		}

		public int[] Add(Model model)
		{
			if (model != null && model.type == Model.Types.Mesh)
			{
				Model.MaterialArray[] mats = model.GetMaterials();
				int matArrCount = mats.Length;
				if (matArrCount == 1)
				{
					int matCount = mats[0].materials.Length;
					int[] output = new int[matCount];
					for (int mt = 0; mt < matCount; mt++)
					{
						output[mt] = Add(mats[0].materials[mt]);
					}
					return output;
				}
			}
			return new int[0];
		}

		public int[] Add(Portal portal)
		{
			if (portal != null)
			{
				List<Surface> usedSurfaces = portal.UsedSurfaces();
				int usedSurfaceCount = usedSurfaces.Count;
				int[] submeshes = new int[usedSurfaceCount];
				for(int s = 0; s < usedSurfaceCount; s++)
					submeshes[s] = Add(usedSurfaces[s]);
				return submeshes;
			}
			return new int[0];
		}

		public void Add(WallSection section)
        {
            if (section == null)
                return;
			
            if (WALLSECTION_SUBMESH_MAPPING.ContainsKey(section))
                return;

            List<int> submeshMapping = new List<int>();
            submeshMapping.Add(Add(section.wallSurface));
            if (section.sillSurface != null)
                submeshMapping.Add(Add(section.sillSurface));
            if (section.ceilingSurface != null)
                submeshMapping.Add(Add(section.ceilingSurface));
            if (section.openingSurface != null)
                submeshMapping.Add(Add(section.openingSurface));

            List<bool> submeshTiling = new List<bool>();
            submeshTiling.Add(section.wallSurface != null && section.wallSurface.tiled);
            if(section.sillSurface != null)
                submeshTiling.Add(section.sillSurface != null && section.sillSurface.tiled);
            if (section.ceilingSurface != null)
                submeshTiling.Add(section.ceilingSurface != null && section.ceilingSurface.tiled);
            if (section.openingSurface != null)
                submeshTiling.Add(section.openingSurface != null && section.openingSurface.tiled);
			
            int[] balconySubmeshes = Add(section.balconyModel);
            submeshMapping.AddRange(balconySubmeshes);
            for (int b = 0; b < balconySubmeshes.Length; b++)
                submeshTiling.Add(false);//no tiling

            int[] shutterSubmeshes = Add(section.shutterModel);
            submeshMapping.AddRange(shutterSubmeshes);
            for (int b = 0; b < shutterSubmeshes.Length; b++)
                submeshTiling.Add(false);//no tiling

			int[] openingSubmeshes = Add(section.openingModel);
			submeshMapping.AddRange(openingSubmeshes);
			for (int b = 0; b < openingSubmeshes.Length; b++)
				submeshTiling.Add(false);//no tiling

			int[] portalSubmeshes = Add(section.portal);
			int portalSubmeshCount = portalSubmeshes.Length;
			for(int s = 0; s < portalSubmeshCount; s++)
			{
				if(!submeshMapping.Contains(portalSubmeshes[s]))
				{
					submeshMapping.Add(portalSubmeshes[s]);
					Surface surf = SUBMESH[portalSubmeshes[s]] as Surface;
					if (surf != null)
						submeshTiling.Add(surf.tiled);
					else
						submeshTiling.Add(false);
				}
			}

			WALLSECTION_SUBMESH_MAPPING.Add(section, submeshMapping.ToArray());
            WALLSECTION_SUBMESH_TILING.Add(section, submeshTiling.ToArray());
        }
        
        public override string ToString()
        {
            string output = "";
			output = "Materials:\n";
			for (int i = 0; i < MATERIALS.Count; i++)
			{
				output += MATERIALS[i].name;
				if (i < MATERIALS.Count - 1)
					output += "\n";
			}

			output += "\nSubmeshes:\n";
			output += "Submesh count: " + SUBMESH_COUNT + "\n";
			for (int i = 0; i < SUBMESH_COUNT; i++)
			{
				if (SUBMESH.ContainsKey(i))
				{
					output += "submesh " + i + " surface: " + SUBMESH[i].name;
					if (i < SUBMESH_COUNT)
						output += "\n";
				}
			}

			output += "\nSurfaces:\n";
			output += "Surface count: " + SURFACES.Count + "\n";
			for (int i = 0; i < SURFACES.Count; i++)
			{
				output += "Surface " + i + " : " + SURFACES[i];
				if (i < SUBMESH_COUNT)
					output += "\n";
			}
			return output;
        }

	    public int[] Add(Chimney chimney)
	    {
	        if (chimney != null)
	        {
                List<Surface> usedSurfaces = chimney.UsedSurfaces();
	            int usedSurfaceCount = usedSurfaces.Count;
	            int[] submeshes = new int[usedSurfaceCount];
	            for (int s = 0; s < usedSurfaceCount; s++)
	                submeshes[s] = Add(usedSurfaces[s]);
	            return submeshes;
            }
	        return new int[0];
        }
	}
}