using System;

namespace FEM_NET.FEM2D
{
    internal class Vertex
    {
        public Vector2 Position { get; }
        public int Index { get; }
        public int Reference { get; }

        public Vertex(Vector2 position, int index, int reference)
        {
            Position = position;
            Index = index;
            Reference = reference;
        }
    }
}
