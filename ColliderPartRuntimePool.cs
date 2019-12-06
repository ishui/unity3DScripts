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
using UnityEngine;

namespace BuildR2
{
	public class ColliderPartRuntimePool : MonoBSingleton<ColliderPartRuntimePool>
	{
		private List<ColliderPartRuntime> _pool;
		private bool _initialised;

		[SerializeField]
		private int prewarmSize = 25;
        
	    public void ClearPool()
	    {
	        Transform[] children = transform.GetComponentsInChildren<Transform>();
	        int childCount = children.Length;
	        for (int c = 0; c < childCount; c++)
	        {
	            if (children[c] == transform) continue;
#if UNITY_EDITOR
	            DestroyImmediate(children[c].gameObject);
#else
                Destroy(children[c].gameObject);
#endif
	        }
	    }

        public ColliderPartRuntime Pull()
		{
			if (!_initialised) Init();
			return GetPoolItem();
		}

		public void Push(ColliderPartRuntime item)
		{
			if (!_initialised) Init();
			item.transform.parent = transform;
			_pool.Add(item);
		}

		public void Push(ColliderPartRuntime[] items)
		{
			foreach (ColliderPartRuntime item in items)
				Push(item);
		}

		public void Push(List<ColliderPartRuntime> items)
		{
			foreach (ColliderPartRuntime item in items)
				Push(item);
		}

		private void Init(int prewarmSize = 0)
		{
			_initialised = true;
			_pool = new List<ColliderPartRuntime>();
			for(int p = 0; p < prewarmSize; p++)
				Push(Instantiate());
		}
		
		private ColliderPartRuntime GetPoolItem()
		{
			if (_pool.Count > 0)
			{
				ColliderPartRuntime output = _pool[0];
				_pool.RemoveAt(0);
				return output;
			}
			else
			{
				return Instantiate();//there was nothing suitable in the pool - make a new one!
			}
		}

		private ColliderPartRuntime Instantiate()
		{
			return ColliderPartRuntime.Create(transform, "New Collider Part");
		}

		private void Awake()
		{
			for (int p = 0; p < prewarmSize; p++)
				Push(Instantiate());
		}
	}
}