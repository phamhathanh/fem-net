using System;

namespace _2DFEM
{
    class Triangle
    {
        private readonly Node[] nodes;

        public Triangle(Node node1, Node node2, Node node3)
        {
            this.nodes = new Node[] { node1, node2, node3 };
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
