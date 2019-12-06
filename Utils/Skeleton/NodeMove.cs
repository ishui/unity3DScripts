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
using System.Collections;

namespace BuildR2.ShapeOffset
{
    public class NodeMove
    {
        private Node _originator;
        private Node _live;

        public NodeMove(Node origin, Node active)
        {
            _originator = origin;
            _live = active;
        }

        public Node originator { get { return _originator; } }

        public Node live { get { return _live; } }
    }
}