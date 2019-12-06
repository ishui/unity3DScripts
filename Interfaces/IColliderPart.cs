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

namespace BuildR2
{
	public interface IColliderPart
	{
		string name { get; }
		GameObject gameObject { get; }
		Transform transform { get; }

		Mesh mesh { get; }
		void Deactivate();
		void Clear();
		void GenerateFromColliderMesh(BuildRCollider mesh);
		void GenerateFromDynamicMesh(BuildRMesh mesh);
	}
}