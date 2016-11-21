using System;

namespace _2DFEM
{
    internal class Node
    {
        public Vector2 Position { get; }
        public int Index { get; }
        public bool IsInside { get; }

        public Node(Vector2 position, int index, bool isInside)
        {
            Position = position;
            Index = index;
            IsInside = isInside;
        }
    }
}
