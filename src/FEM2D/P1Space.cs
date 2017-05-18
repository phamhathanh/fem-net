using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal sealed class P1Space : IFiniteElementSpace
    {
        public IReadOnlyCollection<Vertex> Vertices { get; }
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public P1Space(IMesh mesh)
        {
            Vertices = (new List<Vertex>(mesh.Vertices)).AsReadOnly();

            var finiteElements = new List<IFiniteElement>();
            foreach (var triangle in mesh.Triangles)
            {
                var finiteElement = new P1Element(triangle);
                finiteElements.Add(finiteElement);
            }
            FiniteElements = finiteElements.AsReadOnly();
        }

        public static P1Space Create(IMesh mesh)
            => new P1Space(mesh);
    }

    internal sealed class P1Element : IFiniteElement
    {
        public ReadOnlyCollection<INode> Nodes { get; }
        public Triangle Triangle { get; }

        public P1Element(Triangle triangle)
        {
            Triangle = triangle;

            var vertex0 = triangle.Vertex0;
            var vertex1 = triangle.Vertex1;
            var vertex2 = triangle.Vertex2;
            var node0 = new P1Node(vertex0, vertex1, vertex2);
            var node1 = new P1Node(vertex1, vertex2, vertex0);
            var node2 = new P1Node(vertex2, vertex0, vertex1);
            Nodes = new ReadOnlyCollection<INode>(new[] { node0, node1, node2 });
        }

        public bool Contains(Vector2 point)
            => Nodes.All(node => node.Phi(point) >= -1e-5);
    }

    internal class P1Node : INode
    {
        public Vertex Vertex { get; }
        public Func<Vector2, double> Phi { get; }
        public Func<Vector2, Vector2> GradPhi { get; }

        public P1Node(Vertex thisNode, Vertex thatNode, Vertex thatOtherNode)
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
            Phi = p => (a * p.x + b * p.y + c) / denominator;

            var gradient = (new Vector2(a, b)) / denominator;
            GradPhi = point => gradient;
        }
    }
}
