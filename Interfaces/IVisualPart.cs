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

namespace BuildR2 {
	public interface IVisualPart
	{

		string name { get; }
		GameObject gameObject { get; }
		Transform transform { get; }

        Mesh mesh {get;}
		BuildRMesh dynamicMesh {get;}
		BuildRCollider colliderMesh {get;}
		IColliderPart colliderPart {get;}
		Material material {set;}
		Material[] materials {get; set;}
		void Clear();
		void GenerateFromDynamicMesh(BuildRMesh overflow = null);
//		void DestroyVisual();
		void UpdateMeshFilter();
        void Place(Vector3 atPosition);
        void Move(Vector3 by);
	}
}