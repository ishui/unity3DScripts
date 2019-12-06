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

[System.Serializable]
public class Bitmask
{
    [SerializeField]
    private int _value;

    public bool this[int index]
    {
        get {return (_value & 1 << index) > 0;}
        set
        {
            if(value)
                _value = _value | 1 << index;
            else
                _value &= ~(1 << index);
        }
    }

    public int value
    {
        get { return _value; }
        set {_value = value;}
    }

    public void Clear()
    {
        _value = 0;
    }

    public int Max()
    {
        return Mathf.CeilToInt(Mathf.Sqrt(_value));
    }

    public int FirstFalse()
    {
        int max = Max();
        for(int i = 0; i < max; i++)
        {
            if((_value & 1 << i) == 0) return i;
        }
        return max;
    }

    public new string ToString()
    {
        string output = "";
        int size = Max();
        for(int i = 0; i < size; i++)
            output = output + ((_value & 1 << i) == 0 ? "0" : "1");
        return output;
    }
}