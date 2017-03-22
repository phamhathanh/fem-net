using System;

namespace FEMSharp.FEM2D
{
    internal class Node
    {
        public Vector2 Position { get; }
        public int Index { get; set; }
        // TODO: Provide encapsulation.

        public int Reference { get; }

        public Node(Vector2 position, int reference)
        {
            Position = position;
            Reference = reference;
        }
    }
}
