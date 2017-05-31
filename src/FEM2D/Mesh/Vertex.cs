using System;

namespace FEM_NET.FEM2D
{
    internal class Vertex
    {
        public Vector2 Position { get; }
        public int Label { get; }

        public Vertex(Vector2 position, int label)
        {
            Position = position;
            Label = label;
        }
    }
}
