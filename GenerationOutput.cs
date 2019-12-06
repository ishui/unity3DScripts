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
using BuildR2;
using UnityEngine;

public class GenerationOutput
{
	public Mesh mesh = null;
	public RawMeshData raw = null;

	public static GenerationOutput CreateMeshOutput()
	{
		GenerationOutput output = new GenerationOutput();
		output.mesh = new Mesh();
		return output;
	}

	public static GenerationOutput CreateRawOutput()
	{
		GenerationOutput output = new GenerationOutput();
		output.raw = new RawMeshData();
		return output;
	}

	public static GenerationOutput CreateCombinedOutput()
	{
		GenerationOutput output = new GenerationOutput();
		output.mesh = new Mesh();
		output.raw = new RawMeshData();
		return output;
	}
}

public class RawMeshData
{
	public Vector3[] vertices;
	public Vector2[] uvs;
	public int[] triangles;
	public Dictionary<int, List<int>> subTriangles;
	public Vector3[] normals;
	public Vector4[] tangents;
    public int vertCount;
    public int submeshCount;
    public List<Material> materials;

    public RawMeshData()
    {
        
    }

    public RawMeshData(int vertCount, int triCount)
    {
        vertices = new Vector3[vertCount];
        uvs = new Vector2[vertCount];
        triangles = new int[triCount];
        subTriangles = new Dictionary<int, List<int>>();
        normals = new Vector3[vertCount];
        tangents = new Vector4[vertCount];
        submeshCount = 1;
        this.vertCount = vertCount;
    }

	public void Copy(BuildRMesh data)
	{
		vertices = data.vertices.ToArray();
		uvs = data.uvs.ToArray();
		triangles = data.triangles.ToArray();
		normals = data.normals.ToArray();
		tangents = data.tangents.ToArray();
		subTriangles = new Dictionary<int, List<int>>(data.subTriangles);
	    vertCount = data.vertexCount;
	    submeshCount = subTriangles.Count;
	    materials = new List<Material>(data.submeshLibrary.MATERIALS);
	}

	public void Copy(Mesh data)
	{
		vertices = data.vertices;
		uvs = data.uv;
		triangles = data.triangles;
		normals = data.normals;
		tangents = data.tangents;

		subTriangles = new Dictionary<int, List<int>>();
		for (int s = 0; s < data.subMeshCount; s++)
			subTriangles.Add(s, new List<int>(data.GetTriangles(s)));

	    vertCount = data.vertexCount;
	    submeshCount = subTriangles.Count;
    }

    public static RawMeshData CopyBuildRMesh(BuildRMesh data) {
        RawMeshData output = new RawMeshData(data.vertices.Count, data.triangles.Count);
        output.Copy(data);
        return output;
    }

    public static RawMeshData CopMesh(Mesh data) {
        RawMeshData output = new RawMeshData(data.vertices.Length, data.triangles.Length);
        output.Copy(data);
        return output;
    }

    public override string ToString()
    {
        return string.Format("vert count {0} tri count {1} sm count {2} ", vertices.Length, triangles.Length, subTriangles.Count);
    }
}