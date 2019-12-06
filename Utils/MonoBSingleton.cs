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
 
public abstract class MonoBSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _instantiated;
 
    public static T Instance
    {
        get
        {
            if (_instantiated) return _instance;
 
            var type = typeof(T);
            var objects = FindObjectsOfType<T>();
 
            if (objects.Length > 0)
            {
                _instance = objects[0];
                if (objects.Length > 1)
                {
                    Debug.LogWarning("There is more than one instance of Singleton of type \"" + type + "\". Keeping the first. Destroying the others.");
                    for (var i = 1; i < objects.Length; i++)
                        DestroyImmediate(objects[i].gameObject);
                }
                _instantiated = true;
                return _instance;
            }
 
            var gameObject = new GameObject();
            gameObject.name = type.ToString();
 
            _instance = gameObject.AddComponent<T>();
//            DontDestroyOnLoad(gameObject);
 
            _instantiated = true;
            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        _instance = null;
        _instantiated = false;
    }
}
