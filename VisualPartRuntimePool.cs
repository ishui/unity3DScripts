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
	public class VisualPartRuntimePool : MonoBSingleton<VisualPartRuntimePool>
	{
		private List<VisualPartRuntime> _pool;
		private bool _initialised;

		[SerializeField]
		private int prewarmSize = 25;

		private int _internalCounter = 0;

	    public void ClearPool()
	    {
	        Transform[] children = transform.GetComponentsInChildren<Transform>();
	        int childCount = children.Length;
	        for(int c = 0; c < childCount; c++)
	        {
                if(children[c] == transform) continue;
#if UNITY_EDITOR
                DestroyImmediate(children[c].gameObject);
#else
                Destroy(children[c].gameObject);
#endif
            }
	        _internalCounter = 0;
	    }

		public VisualPartRuntime Pull()
		{
			if (!_initialised) Init();
			VisualPartRuntime output = GetPoolItem();
			output.Activate();
			return output;
		}

		public void Push(VisualPartRuntime item)
		{
			if (!_initialised) Init();
			item.transform.parent = transform;
			_pool.Add(item);
		}

		public void Push(VisualPartRuntime[] items)
		{
			foreach (VisualPartRuntime item in items)
				Push(item);
		}

		public void Push(List<VisualPartRuntime> items)
		{
			foreach (VisualPartRuntime item in items)
				Push(item);
		}

		private void Init()
		{
			_initialised = true;
			_pool = new List<VisualPartRuntime>();
			for(int p = 0; p < prewarmSize; p++)
				Push(Instantiate());
		}
		
		private VisualPartRuntime GetPoolItem()
		{
			if (_pool.Count > 0)
			{
				VisualPartRuntime output = _pool[0];
				_pool.RemoveAt(0);
				return output;
			}
			else
			{
				return Instantiate();//there was nothing suitable in the pool - make a new one!
			}
		}

		private VisualPartRuntime Instantiate()
		{
			string newName = string.Format("New Visual Part {0}", _internalCounter);
			_internalCounter++;
			return VisualPartRuntime.Create(transform, newName);
		}

		private void Awake()
		{
			Init();
		}
	}
}