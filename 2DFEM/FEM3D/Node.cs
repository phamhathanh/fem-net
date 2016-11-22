using System;

namespace FEMSharp.FEM3D
{
    internal class Node
    {
        public Vector3 Position { get; }
        public int Index { get; }
        public bool IsInside { get; }

        public Node(Vector3 position, int index, bool isInside)
        {
            Position = position;
            Index = index;
            IsInside = isInside;
        }
    }
}
