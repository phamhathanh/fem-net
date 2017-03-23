using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal sealed class P1Element : IFiniteElement
    {
        public class Node : INode
        {
            public Vertex Vertex { get; }
            public Func<Vector2, double> Phi { get; }
            public Func<Vector2, Vector2> GradPhi { get; }

            public Node(Vertex thisNode, Vertex thatNode, Vertex thatOtherNode)
            {
                Vertex = thisNode;

                // Linear interpolation, using Cramer's rule.
                double x1 = thisNode.Position.x, y1 = thisNode.Position.y,
                    x2 = thatNode.Position.x, y2 = thatNode.Position.y,
                    x3 = thatOtherNode.Position.x, y3 = thatOtherNode.Position.y,
                    a = y3 - y2,
                    b = x2 - x3,
                    c = y2 * x3 - x2 * y3,
                    denominator = a * x1 + b * y1 + c;
                Phi = point => (a * point.x + b * point.y + c) / denominator;

                var gradient = (1 / denominator) * (new Vector2(a, b));
                GradPhi = point => gradient;
            }
        }

        public ReadOnlyCollection<INode> Nodes { get; }

        public P1Element(Vertex vertex0, Vertex vertex1, Vertex vertex2)
        {
            var node0 = new Node(vertex0, vertex1, vertex2);
            var node1 = new Node(vertex1, vertex2, vertex0);
            var node2 = new Node(vertex2, vertex0, vertex1);
            Nodes = new ReadOnlyCollection<INode>(new[] { node0, node1, node2 });
        }

        public bool Contains(Vector2 point)
            => Nodes.All(node => node.Phi(point) >= 0);

        public class Factory : IFiniteElementFactory
        {
            public IFiniteElement Create(Triangle triangle)
                => new P1Element(triangle.Vertex0, triangle.Vertex1, triangle.Vertex2);
        }
    }
}
