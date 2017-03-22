using System;

namespace FEMSharp.FEM2D
{
    class Triangle
    {
        private readonly Vertex[] nodes;

        public Triangle(Vertex node1, Vertex node2, Vertex node3)
        {
            this.nodes = new Vertex[] { node1, node2, node3 };
        }

        public double Area
        {
            get
            {
                Vector2 u = nodes[1].Position - nodes[0].Position,
                        v = nodes[2].Position - nodes[0].Position;

                return Math.Abs(u.x * v.y - u.y * v.x) / 2;
            }
        }
    }
}
