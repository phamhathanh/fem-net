using System;

namespace FEM_NET.FEM2D
{
    internal class Vertex
    {
        public Vector2 Position { get; }
        public int Reference { get; }

        public Vertex(Vector2 position, int reference)
        {
            Position = position;
            Reference = reference;
        }
    }
}
