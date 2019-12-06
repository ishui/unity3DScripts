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
	public class BuildRMesh
	{
		private static int LIST_SIZE = 65000;
		private static int LIST_SIZE_TRI = 99000;

		private string _name;
		public List<Vector3> vertices;
		public List<Vector2> uvs;
		public List<Vector3> normals;
		public List<int> triangles;
		public List<int> originalSubmeshMapping;
		public Bounds bounds;

		public List<Vector4> tangents;
		public bool ignoreSubmeshAssignment = false;
		private int _subMeshes;
		public Dictionary<int, List<int>> subTriangles;
		public List<Material> materials;

		public SubmeshLibrary submeshLibrary;

		private BuildRMesh _overflow;

		public BuildRMesh(string newName)
		{
			_name = newName;
			vertices = new List<Vector3>(LIST_SIZE);
			uvs = new List<Vector2>(LIST_SIZE);
			triangles = new List<int>(LIST_SIZE_TRI);
			normals = new List<Vector3>(LIST_SIZE);
			tangents = new List<Vector4>(LIST_SIZE);
			subTriangles = new Dictionary<int, List<int>>();
			bounds = new Bounds();
			originalSubmeshMapping = new List<int>();
			submeshLibrary = new SubmeshLibrary();
			materials = new List<Material>();
		}

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		public void Build(Mesh mesh, bool collapseSubmeshes = false, bool maintainSubmeshStructure = false)
		{
			if (mesh == null)
			{
				Debug.LogError("Mesh sent is null - where is this guy?");
				return;
			}

			mesh.Clear();

			if (vertices.Count == 0)
				return;

            mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetNormals(normals);
			mesh.SetTangents(tangents);
			mesh.bounds = bounds;
			mesh.name = _name;

			originalSubmeshMapping.Clear();
			materials.Clear();
            
			if (submeshLibrary.enabled) {
                List<Material> materialArray = submeshLibrary.MATERIALS;
				Dictionary<int, Material> submeshMaterials = submeshLibrary.SUBMESH_MATERIAL;

				if (!collapseSubmeshes)
				{
					mesh.subMeshCount = 0;//set base submesh count to 0 so we can get started
					_subMeshes = 0;
					int submeshKeys = subTriangles.Count;
					int matCount = materialArray.Count;

					if (submeshKeys > 0)
					{
						int submeshIt = 0;
						bool[] useSubmesh = new bool[submeshKeys];

						foreach (KeyValuePair<int, List<int>> var in subTriangles)
						{
							int submeshKey = var.Key;
							List<int> tris = subTriangles[submeshKey];
							if (tris.Count > 0 || maintainSubmeshStructure)//submesh actually has content so include data
							{
								if (submeshKey >= materialArray.Count || materialArray[submeshKey] == null)//exceeds material list
									continue;
								useSubmesh[submeshIt] = true;
								_subMeshes++;
							}
							submeshIt++;
						}
                        
						mesh.subMeshCount = _subMeshes;
						submeshIt = 0;
						foreach (KeyValuePair<int, List<int>> var in subTriangles)
						{
							if (!useSubmesh[submeshIt]) continue;
							int submeshKey = var.Key;
							mesh.SetTriangles(subTriangles[submeshKey], submeshIt);
							originalSubmeshMapping.Add(submeshKey);
						    if(submeshMaterials.ContainsKey(submeshKey))
						        materials.Add(submeshMaterials[submeshKey]);
							submeshIt++;
						}
					}
					else
					{
						mesh.subMeshCount = 0;
						mesh.SetTriangles(triangles.ToArray(), 0);
						originalSubmeshMapping.Add(0);
						if (matCount > 0)
							materials.Add(materialArray[0]);
					}
				}
				else
				{
					mesh.subMeshCount = 1;//set base submesh count to 0 so we can get started
					_subMeshes = 1;
					triangles.Clear();
					foreach (KeyValuePair<int, List<int>> triData in subTriangles)
						triangles.AddRange(triData.Value.ToArray());
					mesh.SetTriangles(triangles.ToArray(), 0);
					originalSubmeshMapping.Add(0);
					if (materials.Count > 0)
						materials.Add(materialArray[0]);
				}
			}
			else
			{
				int subtriCount = 0;
				foreach (KeyValuePair<int, List<int>> triData in subTriangles)
				{
					if (triData.Value.Count == 0) continue;
					subtriCount++;
				}
				mesh.subMeshCount = subtriCount;
				foreach (KeyValuePair<int, List<int>> triData in subTriangles)
				{
					if (triData.Value.Count == 0) continue;
					mesh.SetTriangles(triData.Value.ToArray(), _subMeshes);
					_subMeshes++;

				}
			}

			mesh.RecalculateBounds();//todo should we do this ourselves?

			if (hasOverflowed)
				submeshLibrary.Clone(ref _overflow.submeshLibrary);
		}

		public BuildRMesh overflow { get { return _overflow; } }

		public bool hasOverflowed { get { return _overflow != null; } }

		/// <summary>
		/// Clears the mesh data, ready for nextNormIndex new mesh build
		/// </summary>
		public void Clear()
		{
			vertices.Clear();
			uvs.Clear();
			triangles.Clear();
			normals.Clear();
			tangents.Clear();
			bounds.center = Vector3.zero;
			bounds.size = Vector3.zero;
			subTriangles.Clear();
			_subMeshes = 0;
			_overflow = null;
			ignoreSubmeshAssignment = false;
			originalSubmeshMapping.Clear();
			submeshLibrary.Clear();
			materials.Clear();
		}

		/// <summary>
		/// Clears the mesh data, ready for nextNormIndex new mesh build
		/// </summary>
		public void ClearMeshData()
		{
			vertices.Clear();
			uvs.Clear();
			triangles.Clear();
			normals.Clear();
			tangents.Clear();
			bounds.center = Vector3.zero;
			bounds.size = Vector3.zero;
			subTriangles.Clear();
			_subMeshes = 0;
			_overflow = null;
		}

		public int vertexCount
		{
			get { return vertices.Count; }
		}

		public int triangleCount
		{
			get { return triangles.Count; }
		}

		public int subMeshCount
		{
			get { return _subMeshes; }
		}

		public void AddDataRaw(Vector3[] verts, Vector2[] uvs, Vector3[] norms, Vector4[] tan, Dictionary<int, List<int>> subTriangles)
		{
			if (MeshOverflow(verts.Length))
			{
				_overflow.AddDataRaw(verts, uvs, norms, tan, subTriangles);
				return;
			}

			vertices.AddRange(verts);
			this.uvs.AddRange(uvs);
			normals.AddRange(norms);
			tangents.AddRange(tan);
			foreach (KeyValuePair<int, List<int>> kv in subTriangles)
			{
				int submesh = kv.Key;
				if (ignoreSubmeshAssignment) submesh = 0;
				if (!this.subTriangles.ContainsKey(submesh))
					this.subTriangles.Add(submesh, new List<int>());
				this.subTriangles[submesh].AddRange(kv.Value);
			}
		}

		public void AddDataRaw(List<Vector3> verts, List<Vector2> uvs, List<Vector3> norms, List<Vector4> tan, Dictionary<int, List<int>> subTriangles)
		{
			if (MeshOverflow(verts.Count))
			{
				_overflow.AddDataRaw(verts, uvs, norms, tan, subTriangles);
				return;
			}

			vertices.AddRange(verts);
			this.uvs.AddRange(uvs);
			normals.AddRange(norms);
			tangents.AddRange(tan);
			foreach (KeyValuePair<int, List<int>> kv in subTriangles)
			{
				int submesh = kv.Key;
				if (ignoreSubmeshAssignment) submesh = 0;
				if (!this.subTriangles.ContainsKey(submesh))
					this.subTriangles.Add(submesh, new List<int>());
				this.subTriangles[submesh].AddRange(kv.Value);
			}
		}
		private Vector2 CalculateUV(Vector2 uv, Surface surface)
		{
			if (surface != null)
				if (surface.tiled)
					return surface.CalculateUV(uv);
			return uv;
		}

		public void AddData(Vector3[] verts, Vector2[] uvs, int[] tris, Vector3[] norms, Vector4[] tan, int subMesh)
		{
			if (MeshOverflow(verts.Length))
			{
				_overflow.AddData(verts, uvs, tris, norms, tan, subMesh);
				return;
			}

			int indiceBase = vertices.Count;
			vertices.AddRange(verts);
			this.uvs.AddRange(uvs);
			normals.AddRange(norms);
			tangents.AddRange(tan);
			if (ignoreSubmeshAssignment) subMesh = 0;
			if (!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());

			int newTriCount = tris.Length;
			for (int t = 0; t < newTriCount; t++)
			{
				int newTri = (indiceBase + tris[t]);
				subTriangles[subMesh].Add(newTri);
			}
		}

		public void AddData(Mesh mesh, int[] submeshes, Vector3 translate, Quaternion rotate, Vector3 scale)
		{
			if (MeshOverflow(mesh.vertexCount))
			{
				_overflow.AddData(mesh, submeshes, translate, rotate, scale);
				return;
			}

			int indiceBase = vertices.Count;
			Vector3[] meshvertices = mesh.vertices;
			Vector3[] meshNormals = mesh.normals;
			Vector4[] meshTangents = mesh.tangents;
			int vertCount = meshvertices.Length;

			Vector3 rotateTangent = new Vector3();
			for (int v = 0; v < vertCount; v++)
			{
				vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
				normals.Add(rotate * meshNormals[v]);
				//				tangents.Add(rotate * meshTangents[v]);
				//				tangents.Add(RotateTangent(meshTangents[v], rotate));

				rotateTangent.x = meshTangents[v].x;
				rotateTangent.y = meshTangents[v].y;
				rotateTangent.z = meshTangents[v].z;
				rotateTangent = rotate * rotateTangent;
				meshTangents[v].x = rotateTangent.x;
				meshTangents[v].y = rotateTangent.y;
				meshTangents[v].z = rotateTangent.z;
				tangents.Add(meshTangents[v]);
			}
			uvs.AddRange(mesh.uv);

		    int submeshCount = submeshes.Length;
            if (submeshCount == 0) {
                
		        int submesh = 0;
		        for (int s = 0; ;) {

		            int[] submeshTris = mesh.GetTriangles(submesh);
		            int triCount = submeshTris.Length;
		            if (triCount > 0) {

		                if (!subTriangles.ContainsKey(submesh))
		                    subTriangles.Add(submesh, new List<int>());

		                for (int t = 0; t < triCount; t++) {
		                    int newTri = indiceBase + submeshTris[t];
		                    subTriangles[submesh].Add(newTri);
		                }
		                s++;

		                if (s == submeshCount)
		                    break;
		            }
		            submesh++;

		            if (submesh > 100)
		                break;
		        }
		    }
		    else {
		        for (int sm = 0; sm < submeshCount; sm++) {
                    if (mesh.subMeshCount <= sm) continue;
		            int submesh = submeshes[sm];
		            if (ignoreSubmeshAssignment) submesh = 0;
		            if (submesh == -1)
		                continue;
                    if (!subTriangles.ContainsKey(submesh))
		                subTriangles.Add(submesh, new List<int>());
		            int[] smTris = mesh.GetTriangles(sm);
		            int smTriCount = smTris.Length;
		            for (int t = 0; t < smTriCount; t++) {
		                int newTri = indiceBase + smTris[t];
		                subTriangles[submesh].Add(newTri);
		            }
		        }
		    }



//            int submeshCount = mappedSubmeshes.Length;
//			if (submeshCount == 0)
//			{
//				submeshCount = mesh.subMeshCount;
//				mappedSubmeshes = new int[submeshCount];
//				for (int s = 0; s < mesh.subMeshCount; s++)
//					mappedSubmeshes[s] = 0;
//			}
//			int meshSubmesh = 0;
//			for (int sm = 0; sm < submeshCount; sm++)
//			{
//				int submesh = mappedSubmeshes[sm];
//				if (ignoreSubmeshAssignment) submesh = 0;
//				if (submesh == -1)
//					continue;
//
//				if (mesh.subMeshCount <= meshSubmesh)
//					continue;
//
//				if (!subTriangles.ContainsKey(submesh))
//					subTriangles.Add(submesh, new List<int>());
//
//				int[] tris = mesh.GetTriangles(meshSubmesh);
//				int newTriCount = tris.Length;
//				for (int t = 0; t < newTriCount; t++)
//				{
//					int newTri = (indiceBase + tris[t]);
//					subTriangles[submesh].Add(newTri);
//				}
//				meshSubmesh++;
//			}
		}

        public void AddData(RawMeshData mesh, int[] mappedSubmeshes, Vector3 translate, Quaternion rotate, Vector3 scale) {
            int meshvertexCount = mesh.vertices.Length;
            if (MeshOverflow(meshvertexCount)) {
                _overflow.AddData(mesh, mappedSubmeshes, translate, rotate, scale);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int vertCount = meshvertices.Length;

            Vector3 rotateTangent = new Vector3();
            for (int v = 0; v < vertCount; v++) {
                vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
                normals.Add(rotate * meshNormals[v]);
                //				tangents.Add(rotate * meshTangents[v]);
                //				tangents.Add(RotateTangent(meshTangents[v], rotate));

                rotateTangent.x = meshTangents[v].x;
                rotateTangent.y = meshTangents[v].y;
                rotateTangent.z = meshTangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshTangents[v].x = rotateTangent.x;
                meshTangents[v].y = rotateTangent.y;
                meshTangents[v].z = rotateTangent.z;
                tangents.Add(meshTangents[v]);
            }
            uvs.AddRange(mesh.uvs);
            int submeshCount = mappedSubmeshes.Length;
            int meshsubMeshCount = mesh.subTriangles.Count;
            if (submeshCount == 0) {
                submeshCount = meshsubMeshCount;
                mappedSubmeshes = new int[submeshCount];
                for (int s = 0; s < meshsubMeshCount; s++)
                    mappedSubmeshes[s] = 0;
            }
            int meshSubmesh = 0;
            for (int sm = 0; sm < submeshCount; sm++) {
                int submesh = mappedSubmeshes[sm];
                if (ignoreSubmeshAssignment) submesh = 0;
                if (submesh == -1)
                    continue;

//                if (meshsubMeshCount <= meshSubmesh)
//                    continue;

                if (!mesh.subTriangles.ContainsKey(meshSubmesh)) {
                    meshSubmesh++;
                    continue;
                }

                if (!subTriangles.ContainsKey(submesh))
                    subTriangles.Add(submesh, new List<int>());

                int newTriCount = mesh.subTriangles[meshSubmesh].Count;
                for (int t = 0; t < newTriCount; t++) {
                    int newTri = (indiceBase + mesh.subTriangles[meshSubmesh][t]);
                    subTriangles[submesh].Add(newTri);
                }
                meshSubmesh++;
            }
        }

        public void AddData(Mesh mesh, Vector3 translate, Quaternion rotate, Vector3 scale)
		{
			if (MeshOverflow(mesh.vertexCount))
			{
				_overflow.AddData(mesh, translate, rotate, scale);
				return;
			}

			int indiceBase = vertices.Count;
			Vector3[] meshvertices = mesh.vertices;
			Vector3[] meshNormals = mesh.normals;
			Vector4[] meshTangents = mesh.tangents;
			int vertCount = meshvertices.Length;

			Vector3 rotateTangent = new Vector3();
			for (int v = 0; v < vertCount; v++)
			{
				vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
				normals.Add(rotate * meshNormals[v]);
				//				tangents.Add(rotate * meshTangents[v]);
				//				tangents.Add(RotateTangent(meshTangents[v], rotate));

				rotateTangent.x = meshTangents[v].x;
				rotateTangent.y = meshTangents[v].y;
				rotateTangent.z = meshTangents[v].z;
				rotateTangent = rotate * rotateTangent;
				meshTangents[v].x = rotateTangent.x;
				meshTangents[v].y = rotateTangent.y;
				meshTangents[v].z = rotateTangent.z;
				tangents.Add(meshTangents[v]);
			}
			uvs.AddRange(mesh.uv);
			//            normals.AddRange(new Vector3[vertCount]);

			int[] tris = mesh.triangles;
			int newTriCount = tris.Length;
			if (!subTriangles.ContainsKey(0))
				subTriangles.Add(0, new List<int>());
			for (int t = 0; t < newTriCount; t++)
			{
				int newTri = (indiceBase + tris[t]);
				subTriangles[0].Add(newTri);
			}
		}

	    public void AddData(RawMeshData mesh, int submesh = 0)
	    {
	        int meshvertexCount = mesh.vertices.Length;
	        if (MeshOverflow(meshvertexCount))
	        {
	            _overflow.AddData(mesh, submesh);
	            return;
	        }

	        int indiceBase = vertices.Count;
	        Vector3[] meshvertices = mesh.vertices;
	        Vector3[] meshNormals = mesh.normals;
	        Vector4[] meshTangents = mesh.tangents;
	        int vertCount = meshvertices.Length;
            
	        for (int v = 0; v < vertCount; v++)
	        {
	            vertices.Add(meshvertices[v]);
	            normals.Add(meshNormals[v]);
                tangents.Add(meshTangents[v]);
	        }
	        uvs.AddRange(mesh.uvs);

	        int[] tris = mesh.triangles;
	        int newTriCount = tris.Length;
	        if (!subTriangles.ContainsKey(submesh))
	            subTriangles.Add(submesh, new List<int>());
	        for (int t = 0; t < newTriCount; t++)
	        {
	            int newTri = (indiceBase + tris[t]);
	            subTriangles[submesh].Add(newTri);
	        }
	    }

	    public void AddData(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale) {
	        int meshvertexCount = mesh.vertices.Length;
	        if (MeshOverflow(meshvertexCount)) {
	            _overflow.AddData(mesh, translate, rotate, scale);
	            return;
	        }

	        int indiceBase = vertices.Count;
	        Vector3[] meshvertices = mesh.vertices;
	        Vector3[] meshNormals = mesh.normals;
	        Vector4[] meshTangents = mesh.tangents;
	        int vertCount = meshvertices.Length;

	        Vector3 rotateTangent = new Vector3();
	        for (int v = 0; v < vertCount; v++) {
	            vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
	            normals.Add(rotate * meshNormals[v]);
	            //				tangents.Add(rotate * meshTangents[v]);
	            //				tangents.Add(RotateTangent(meshTangents[v], rotate));

	            rotateTangent.x = meshTangents[v].x;
	            rotateTangent.y = meshTangents[v].y;
	            rotateTangent.z = meshTangents[v].z;
	            rotateTangent = rotate * rotateTangent;
	            meshTangents[v].x = rotateTangent.x;
	            meshTangents[v].y = rotateTangent.y;
	            meshTangents[v].z = rotateTangent.z;
	            tangents.Add(meshTangents[v]);
	        }
	        uvs.AddRange(mesh.uvs);
	        //            normals.AddRange(new Vector3[vertCount]);

	        int[] tris = mesh.triangles;
	        int newTriCount = tris.Length;
	        if (!subTriangles.ContainsKey(0))
	            subTriangles.Add(0, new List<int>());
	        for (int t = 0; t < newTriCount; t++) {
	            int newTri = (indiceBase + tris[t]);
	            subTriangles[0].Add(newTri);
	        }
	    }

	    public void AddDataKeepSubmeshStructure(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale) {
	        int meshvertexCount = mesh.vertices.Length;
	        if (MeshOverflow(meshvertexCount)) {
	            _overflow.AddData(mesh, translate, rotate, scale);
	            return;
	        }

	        int indiceBase = vertices.Count;
	        Vector3[] meshvertices = mesh.vertices;
	        Vector3[] meshNormals = mesh.normals;
	        Vector4[] meshTangents = mesh.tangents;
	        int vertCount = meshvertices.Length;

	        Vector3 rotateTangent = new Vector3();
	        for (int v = 0; v < vertCount; v++) {
	            vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
	            normals.Add(rotate * meshNormals[v]);
	            //				tangents.Add(rotate * meshTangents[v]);
	            //				tangents.Add(RotateTangent(meshTangents[v], rotate));

	            rotateTangent.x = meshTangents[v].x;
	            rotateTangent.y = meshTangents[v].y;
	            rotateTangent.z = meshTangents[v].z;
	            rotateTangent = rotate * rotateTangent;
	            meshTangents[v].x = rotateTangent.x;
	            meshTangents[v].y = rotateTangent.y;
	            meshTangents[v].z = rotateTangent.z;
	            tangents.Add(meshTangents[v]);
	        }
	        uvs.AddRange(mesh.uvs);

	        foreach(KeyValuePair<int, List<int>> var in mesh.subTriangles) {
	            int submeshKey = var.Key;
	            if (!subTriangles.ContainsKey(submeshKey))
	                subTriangles.Add(submeshKey, new List<int>());
                List<int> tris = var.Value;
	            int triCount = var.Value.Count;
	            for(int t = 0; t < triCount; t++) {
	                int newTri = (indiceBase + tris[t]);
	                subTriangles[submeshKey].Add(newTri);
	            }
	        }
	    }

	    public void AddData(Mesh mesh, Matrix4x4 m4, int[] submeshes = null, bool flipXUv = false)
		{
			if (MeshOverflow(mesh.vertexCount))
			{
				_overflow.AddData(mesh, m4, submeshes);
				return;
			}

			int indiceBase = vertices.Count;
			Vector3[] meshvertices = mesh.vertices;
			Vector3[] meshNormals = mesh.normals;
			Vector4[] meshTangents = mesh.tangents;
			int vertCount = meshvertices.Length;

			Vector3 rotateTangent = new Vector3();
			Quaternion rotate = Quaternion.LookRotation(m4.GetColumn(2), m4.GetColumn(1));
			for (int v = 0; v < vertCount; v++)
			{
				vertices.Add(m4.MultiplyPoint3x4(meshvertices[v]));
				normals.Add(rotate * meshNormals[v]);

				rotateTangent.x = meshTangents[v].x;
				rotateTangent.y = meshTangents[v].y;
				rotateTangent.z = meshTangents[v].z;
				rotateTangent = rotate * rotateTangent;
				meshTangents[v].x = rotateTangent.x;
				meshTangents[v].y = rotateTangent.y;
				meshTangents[v].z = rotateTangent.z;
				tangents.Add(meshTangents[v]);
			}

			if(mesh.uv.Length == vertCount)
			{
				Vector2[] meshUv = mesh.uv;
				if(!flipXUv)
					uvs.AddRange(meshUv);
				else
				{
					for(int v = 0; v < vertCount; v++)
					{
						Vector2 flippedUv = new Vector2(1-meshUv[v].x, meshUv[v].y);
						uvs.Add(flippedUv);
					}
				}
			}
			else
				uvs.AddRange(new Vector2[vertCount]);
            
			if (submeshes == null) {
                
			    int submeshCount = mesh.subMeshCount;
			    int submesh = 0;
			    for(int s = 0; ; ) {

			        int[] submeshTris = mesh.GetTriangles(submesh);
			        int triCount = submeshTris.Length;
			        if(triCount > 0) {

			            if (!subTriangles.ContainsKey(submesh))
			                subTriangles.Add(submesh, new List<int>());

                        for (int t = 0; t < triCount; t++) {
			                int newTri = indiceBase + submeshTris[t];
			                subTriangles[submesh].Add(newTri);
			            }
                        s++;

			            if(s == submeshCount)
			                break;
			        }
			        submesh++;

			        if(submesh > 100)
			            break;
			    }
			}
			else
			{
				int submeshCount = submeshes.Length;
				for (int sm = 0; sm < submeshCount; sm++)
				{
					if (mesh.subMeshCount <= sm) continue;
					int submesh = submeshes[sm];
					if (!subTriangles.ContainsKey(submesh))
						subTriangles.Add(submesh, new List<int>());
					int[] smTris = mesh.GetTriangles(sm);
					int smTriCount = smTris.Length;
					for (int t = 0; t < smTriCount; t++)
					{
						int newTri = indiceBase + smTris[t];
						subTriangles[submesh].Add(newTri);
					}
				}
			}
		}

		/// <summary>
		/// Assumption is that the vert data is flat. Y is constant
		/// </summary>
		/// <param name="verts"></param>
		/// <param name="tris"></param>
		/// <param name="submesh"></param>
		public void AddFlatMeshData(Vector3[] verts, int[] tris, int submesh)
		{
			if (tris.Length < 3) return;
			Vector3 v0 = verts[tris[0]];
			Vector3 v1 = verts[tris[1]];
			Vector3 v2 = verts[tris[2]];
			Vector3 normal = CalculateNormal(v0, v1, v2);
			Vector3 tangentV3 = Vector3.Cross(normal, Vector3.forward);
			Vector4 tangent = CalculateTangent(tangentV3);
			int vertCount = verts.Length;
			Vector2[] generatedUvs = new Vector2[vertCount];
			Vector3[] norms = new Vector3[vertCount];
			Vector4[] tans = new Vector4[vertCount];
			for (int v = 0; v < vertCount; v++)
			{
				generatedUvs[v] = new Vector2(verts[v].x, verts[v].z);
				norms[v] = normal;
				tans[v] = tangent;
			}
			AddData(verts, generatedUvs, tris, norms, tans, submesh);
		}

        public void AddData(Mesh mesh, int[] mappedSubmeshes, Vector3 translate, Quaternion rotate, Vector3 scale, Vector2 uvOffset, bool[] uvTransform)
        {
            if (MeshOverflow(mesh.vertexCount))
            {
                _overflow.AddData(mesh, mappedSubmeshes, translate, rotate, scale, uvOffset, uvTransform);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshnormals = mesh.normals;
            Vector4[] meshtangents = mesh.tangents;
            int vertCount = meshvertices.Length;
            Vector3 rotateTangent = new Vector3();
            for (int v = 0; v < vertCount; v++)
            {
                vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
                normals.Add(rotate * meshnormals[v]);

                //rotate tangents here
                rotateTangent.x = meshtangents[v].x;
                rotateTangent.y = meshtangents[v].y;
                rotateTangent.z = meshtangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshtangents[v].x = rotateTangent.x;
                meshtangents[v].y = rotateTangent.y;
                meshtangents[v].z = rotateTangent.z;
                tangents.Add(meshtangents[v]);
            }

            int submeshCount = mappedSubmeshes.Length;
            int meshSubmesh = 0;
            if (submeshCount == 0)
            {
                mappedSubmeshes = new[] { 0 };
                submeshCount = 1;
            }

            Vector2[] meshUvs = mesh.uv;
            bool[] set = new bool[vertCount];
            for (int sm = 0; sm < submeshCount; sm++)
            {
                int submesh = mappedSubmeshes[sm];
                if (ignoreSubmeshAssignment) submesh = 0;
                if (submesh == -1)
                    continue;

                if (mesh.subMeshCount <= meshSubmesh)
                    continue;

                if (!subTriangles.ContainsKey(submesh))
                    subTriangles.Add(submesh, new List<int>());

                int[] tris = mesh.GetTriangles(meshSubmesh);
                int newTriCount = tris.Length;

                if (uvTransform[sm])
                {
                    for (int t = 0; t < newTriCount; t++)
                    {
                        int uvIndex = tris[t];
                        if (set[uvIndex]) continue;

                        meshUvs[uvIndex] += uvOffset;
                        set[uvIndex] = true;
                    }
                }

                for (int t = 0; t < newTriCount; t++)
                    tris[t] += indiceBase;
                subTriangles[submesh].AddRange(tris);
                meshSubmesh++;
            }
            uvs.AddRange(meshUvs);
        }

        public void AddData(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale, Vector2 uvOffset)
        {
            int vertCount = mesh.vertCount;
            if (MeshOverflow(vertCount))
            {
                _overflow.AddData(mesh, translate, rotate, scale, uvOffset);
                return;
            }

            int indiceBase = vertices.Count;
//            Debug.Log(mesh.vertices.Length);
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshnormals = mesh.normals;
            Vector4[] meshtangents = mesh.tangents;
            Vector3 rotateTangent = new Vector3();
            for (int v = 0; v < vertCount; v++)
            {
                vertices.Add(rotate * Vector3.Scale(meshvertices[v], scale) + translate);
                normals.Add(rotate * meshnormals[v]);

                //rotate tangents here
                rotateTangent.x = meshtangents[v].x;
                rotateTangent.y = meshtangents[v].y;
                rotateTangent.z = meshtangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshtangents[v].x = rotateTangent.x;
                meshtangents[v].y = rotateTangent.y;
                meshtangents[v].z = rotateTangent.z;
                tangents.Add(meshtangents[v]);
            }

            Vector2[] meshuvs = new Vector2[vertCount];
            System.Array.Copy(mesh.uvs, meshuvs, vertCount);
            bool[] set = new bool[vertCount];
            int submeshIt = 0;
            foreach(KeyValuePair<int, List<int>> var in mesh.subTriangles)
            {
                int submesh = var.Key;
                if (ignoreSubmeshAssignment) submesh = 0;
                if (submesh == -1)
                    continue;
                
                if (!subTriangles.ContainsKey(submesh))
                    subTriangles.Add(submesh, new List<int>(LIST_SIZE_TRI));

                List<int> tris = var.Value;
//                List<int> tris = new List<int>(var.Value);
                int newTriCount = tris.Count;

                bool tiledSubmesh = submeshLibrary.isTiled(submesh);

                if (tiledSubmesh)
                {
                    for (int t = 0; t < newTriCount; t++)
                    {
                        int uvIndex = tris[t];
                        if (set[uvIndex]) continue;

                        meshuvs[uvIndex] += uvOffset;
                        set[uvIndex] = true;
                    }
                }
                submeshIt++;

//                for (int t = 0; t < newTriCount; t++)
//                    tris[t] += indiceBase;
//                subTriangles[submesh].AddRange(tris);
                for(int t = 0; t < newTriCount; t++)
                    subTriangles[submesh].Add(tris[t] + indiceBase);
            }
            uvs.AddRange(meshuvs);
        }

        public void AddData(Mesh mesh, int submesh)
		{
			if (mesh.subMeshCount > 1) Debug.LogError("Mesh contains more than one submesh, use AddData(Mesh mesh, int[] mappedSubmeshes)");
			if (MeshOverflow(mesh.vertexCount))
			{
				_overflow.AddData(mesh, submesh);
				return;
			}

			AddData(mesh.vertices, mesh.uv, mesh.triangles, mesh.normals, mesh.tangents, submesh);
		}

		public void AddPlaneBasic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subMesh)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlaneBasic(p0, p1, p2, p3, subMesh);
				return;
			}

			int indiceBase = vertices.Count;
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);

			uvs.Add(Vector2.zero);
			uvs.Add(Vector2.zero);
			uvs.Add(Vector2.zero);
			uvs.Add(Vector2.zero);

			normals.Add(Vector3.zero);
			normals.Add(Vector3.zero);
			normals.Add(Vector3.zero);
			normals.Add(Vector3.zero);

			tangents.Add(Vector4.zero);
			tangents.Add(Vector4.zero);
			tangents.Add(Vector4.zero);
			tangents.Add(Vector4.zero);

			if (ignoreSubmeshAssignment) subMesh = 0;
			if (!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());

			subTriangles[subMesh].Add(indiceBase);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 1);

			subTriangles[subMesh].Add(indiceBase + 1);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 3);
		}

		public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subMesh)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlane(p0, p1, p2, p3, subMesh);
				return;
			}

			AddPlane(p0, p1, p2, p3, Vector2.zero, Vector2.one, CalculateNormal(p0, p2, p1), CalculateTangent((p1 - p0).normalized), subMesh, null);
		}

		public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector4 tangent, int subMesh)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlane(p0, p1, p2, p3, normal, tangent, subMesh);
				return;
			}

			AddPlane(p0, p1, p2, p3, Vector2.zero, Vector2.one, normal, tangent, subMesh, null);
		}

		public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 minUv, Vector2 maxUv, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlane(p0, p1, p2, p3, minUv, maxUv, normal, tangent, subMesh, surface);
				return;
			}

			int indiceBase = vertices.Count;
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);

			if (surface != null)
			{
			    minUv = surface.CalculateUV(minUv);
			    maxUv = surface.CalculateUV(maxUv);
			}

			uvs.Add(new Vector2(minUv.x, minUv.y));
			uvs.Add(new Vector2(maxUv.x, minUv.y));
			uvs.Add(new Vector2(minUv.x, maxUv.y));
			uvs.Add(new Vector2(maxUv.x, maxUv.y));

			if (ignoreSubmeshAssignment) subMesh = 0;
			if (!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());

			subTriangles[subMesh].Add(indiceBase);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 1);

			subTriangles[subMesh].Add(indiceBase + 1);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 3);

			normals.AddRange(new[] { normal, normal, normal, normal });
			tangents.AddRange(new[] { tangent, tangent, tangent, tangent });
		}


	    public void AddPlaneNoUVCalc(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 minUv, Vector2 maxUv, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
	    {
	        if (MeshOverflow(4))
	        {
	            _overflow.AddPlaneNoUVCalc(p0, p1, p2, p3, minUv, maxUv, normal, tangent, subMesh, surface);
	            return;
	        }

	        int indiceBase = vertices.Count;
	        vertices.Add(p0);
	        vertices.Add(p1);
	        vertices.Add(p2);
	        vertices.Add(p3);

	        uvs.Add(new Vector2(minUv.x, minUv.y));
	        uvs.Add(new Vector2(maxUv.x, minUv.y));
	        uvs.Add(new Vector2(minUv.x, maxUv.y));
	        uvs.Add(new Vector2(maxUv.x, maxUv.y));

	        if (ignoreSubmeshAssignment) subMesh = 0;
	        if (!subTriangles.ContainsKey(subMesh))
	            subTriangles.Add(subMesh, new List<int>());

	        subTriangles[subMesh].Add(indiceBase);
	        subTriangles[subMesh].Add(indiceBase + 2);
	        subTriangles[subMesh].Add(indiceBase + 1);

	        subTriangles[subMesh].Add(indiceBase + 1);
	        subTriangles[subMesh].Add(indiceBase + 2);
	        subTriangles[subMesh].Add(indiceBase + 3);

	        normals.AddRange(new[] { normal, normal, normal, normal });
	        tangents.AddRange(new[] { tangent, tangent, tangent, tangent });
	    }

        public void AddPlaneComplex(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlaneComplex(p0, p1, p2, p3, normal, tangent, subMesh, surface);
				return;
			}

			Vector2 uv0 = new Vector2(p0.x, p0.z);
			Vector2 uv1 = new Vector2(p1.x, p1.z);
			Vector2 uv2 = new Vector2(p2.x, p2.z);
			Vector2 uv3 = new Vector2(p3.x, p3.z);
			AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, surface);
		}

		public void AddPlaneComplexUp(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float uvAngle, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlaneComplexUp(p0, p1, p2, p3, uvAngle, normal, tangent, subMesh, surface);
				return;
			}

			Vector2 uv0 = Rotate(new Vector2(p0.x, p0.z), uvAngle);
			Vector2 uv1 = Rotate(new Vector2(p1.x, p1.z), uvAngle);
			Vector2 uv2 = Rotate(new Vector2(p2.x, p2.z), uvAngle);
			Vector2 uv3 = Rotate(new Vector2(p3.x, p3.z), uvAngle);
			AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, surface);
	    }

	    private Vector2 Rotate(Vector2 input, float degrees)
	    {
	        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
	        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

	        float tx = input.x;
	        float ty = input.y;
	        input.x = (cos * tx) - (sin * ty);
	        input.y = (sin * tx) + (cos * ty);
	        return input;
	    }

        public void AddPlaneComplex(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
		{
			if (MeshOverflow(4))
			{
				_overflow.AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, surface);
				return;
			}

			int indiceBase = vertices.Count;
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);

			uvs.Add(CalculateUV(uv0, surface));
			uvs.Add(CalculateUV(uv1, surface));
			uvs.Add(CalculateUV(uv2, surface));
			uvs.Add(CalculateUV(uv3, surface));

			if (ignoreSubmeshAssignment) subMesh = 0;
			if (!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());

			subTriangles[subMesh].Add(indiceBase);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 1);

			subTriangles[subMesh].Add(indiceBase + 1);
			subTriangles[subMesh].Add(indiceBase + 2);
			subTriangles[subMesh].Add(indiceBase + 3);

			normals.AddRange(new[] { normal, normal, normal, normal });
			tangents.AddRange(new[] { tangent, tangent, tangent, tangent });
	    }

	    public void AddPlaneComplex(Vector3[] v, Vector2[] uv, Vector3 normal, Vector4 tangent, int subMesh, Surface surface)
	    {
            if(v.Length > 4) Debug.LogWarning("Expecting complex plane data - 4 verts");

	        if (MeshOverflow(4))
	        {
	            _overflow.AddPlaneComplex(v, uv, normal, tangent, subMesh, surface);
	            return;
	        }

	        int indiceBase = vertices.Count;
	        vertices.Add(v[0]);
	        vertices.Add(v[1]);
	        vertices.Add(v[2]);
	        vertices.Add(v[3]);

	        uvs.Add(CalculateUV(uv[0], surface));
	        uvs.Add(CalculateUV(uv[1], surface));
	        uvs.Add(CalculateUV(uv[2], surface));
	        uvs.Add(CalculateUV(uv[3], surface));

	        if (ignoreSubmeshAssignment) subMesh = 0;
	        if (!subTriangles.ContainsKey(subMesh))
	            subTriangles.Add(subMesh, new List<int>());

	        subTriangles[subMesh].Add(indiceBase);
	        subTriangles[subMesh].Add(indiceBase + 2);
	        subTriangles[subMesh].Add(indiceBase + 1);

	        subTriangles[subMesh].Add(indiceBase + 1);
	        subTriangles[subMesh].Add(indiceBase + 2);
	        subTriangles[subMesh].Add(indiceBase + 3);

	        normals.AddRange(new[] { normal, normal, normal, normal });
	        tangents.AddRange(new[] { tangent, tangent, tangent, tangent });
	    }


        public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 right, int subMesh)
		{
			if (MeshOverflow(3))
			{
				_overflow.AddTri(p0, p1, p2, right, subMesh);
				return;
			}

			int indiceBase = vertices.Count;
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);

			Vector3 normal = CalculateNormal(p0, p1, p2);
			Vector3 up = Vector3.Cross(right, normal);
			Vector4 tangent = CalculateTangent(right);

			uvs.Add(Vector2.zero);
			uvs.Add(new Vector2(Vector3.Dot(p1 - p0, right), Vector3.Dot(p1 - p0, up)));
			uvs.Add(new Vector2(Vector3.Dot(p2 - p0, right), Vector3.Dot(p2 - p0, up)));

			if (ignoreSubmeshAssignment) subMesh = 0;
			if (!subTriangles.ContainsKey(subMesh))
				subTriangles.Add(subMesh, new List<int>());

			subTriangles[subMesh].Add(indiceBase);
			subTriangles[subMesh].Add(indiceBase + 1);
			subTriangles[subMesh].Add(indiceBase + 2);

			normals.AddRange(new[] { normal, normal, normal });
			tangents.AddRange(new[] { tangent, tangent, tangent });
		}

		/// <summary>
		/// Collapse all the submeshes into a single submesh
		/// </summary>
		public void CollapseSubmeshes()
		{
			List<int> singleSubmesh = new List<int>();
			int numberOfSubmeshesToModify = subTriangles.Count;
			for (int s = 0; s < numberOfSubmeshesToModify; s++)
			{
				if (subTriangles.ContainsKey(s))
				{
					int[] submeshIndices = subTriangles[s].ToArray();
					singleSubmesh.AddRange(submeshIndices);
				}
			}
			subTriangles.Clear();
			subTriangles.Add(0, singleSubmesh);
		}

		/// <summary>
		/// Check if the vertex count exceeds Unity's 65000 limit
		/// If it does, we create an overflow that continues the mesh construction
		/// If there is already an overflow, we've already exceeded the mesh count
		/// </summary>
		/// <param name="numberOfNewVerts">Number of verts being added to current total</param>
		/// <returns></returns>
		private bool MeshOverflow(int numberOfNewVerts)
		{
			if (_overflow != null)
				return true;
			if (numberOfNewVerts + vertexCount >= 65000)
			{
				_overflow = new BuildRMesh(_name);
				return true;
			}
			return false;
		}

		public void ForceNewMesh()
		{
			if (_overflow != null)
				_overflow.ForceNewMesh();
			else
				_overflow = new BuildRMesh(_name);
		}

		/// <summary>
		/// Calcaulte the Tangent from a direction
		/// </summary>
		/// <param name="tangentDirection">the normalised right direction of the tangent</param>
		public static Vector4 CalculateTangent(Vector3 tangentDirection)
		{
			Vector4 tangent = new Vector4();
			tangent.x = tangentDirection.x;
			tangent.y = tangentDirection.y;
			tangent.z = tangentDirection.z;
			tangent.w = 1;//TODO: Check whether we need to flip the bi normal - I don't think we do with these planes
			return tangent;
		}

		public static Vector4 RotateTangent(Vector4 tangent, Quaternion rotation)
		{
			Vector3 tangentDirection = new Vector3(tangent.x, tangent.y, tangent.z);
			tangentDirection = rotation * tangentDirection;
			tangent.x = tangentDirection.x;
			tangent.y = tangentDirection.y;
			tangent.z = tangentDirection.z;
			return tangent;
		}

		/// <summary>
		/// Calculate the normal of a triangle
		/// </summary>
		/// <param name="points">Only three points will be used in calculation</param>
		public static Vector3 CalculateNormal(Vector3[] points)
		{
			if (points.Length < 3) return Vector3.down;//most likely to look wrong
			return CalculateNormal(points[0], points[1], points[2]);
		}

		/// <summary>
		/// Calculate the normal of a triangle
		/// </summary>
		public static Vector3 CalculateNormal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			return Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized).normalized;
		}

		private void verticesAddRange(Vector3[] verts)
		{
			int vertCount = verts.Length;
			for (int v = 0; v < vertCount; v++)
				vertices.Add(verts[v]);
		}
	}
}