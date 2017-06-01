using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    public sealed class P1bSpace : IFiniteElementSpace
    {
        public IReadOnlyCollection<Vertex> Vertices { get; }
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public P1bSpace(Mesh mesh)
        {
            var vertices = new List<Vertex>(mesh.Vertices);
            var finiteElements = new List<IFiniteElement>();
            foreach (var triangle in mesh.Triangles)
            {
                var centerPos = (triangle.Vertex0.Position+triangle.Vertex1.Position+triangle.Vertex2.Position)/3;
                var center = new Vertex(centerPos, 0);
                // TODO: Use an unused reference label instead of 0.
                vertices.Add(center);

                var finiteElement = new P1bElement(triangle, center);
                finiteElements.Add(finiteElement);
            }
            Vertices = vertices.AsReadOnly();
            FiniteElements = finiteElements.AsReadOnly();
        }
    }

    internal sealed class P1bElement : IFiniteElement
    {
        public ReadOnlyCollection<INode> Nodes { get; }
        public Triangle Triangle { get; }

        public P1bElement(Triangle triangle, Vertex center)
        {
            Triangle = triangle;

            var vertex0 = triangle.Vertex0;
            var vertex1 = triangle.Vertex1;
            var vertex2 = triangle.Vertex2;
            var node0 = new P1Node(vertex0, vertex1, vertex2);
            var node1 = new P1Node(vertex1, vertex2, vertex0);
            var node2 = new P1Node(vertex2, vertex0, vertex1);

            var node3 = new P1bNode(center, node0, node1, node2);
            Nodes = new ReadOnlyCollection<INode>(new[] { node0, node1, node2 });
        }

        public bool Contains(Vector2 point)
            => Nodes.All(node => node.Phi(point) >= -1e-5);
    }

    internal class P1bNode : INode
    {
        public Vertex Vertex { get; }
        public Func<Vector2, double> Phi { get; }
        public Func<Vector2, Vector2> GradPhi { get; }

        public P1bNode(Vertex vertex, INode node0, INode node1, INode node2)
        {
            Vertex = vertex;
            Phi = v => 27*node0.Phi(v)*node1.Phi(v)*node2.Phi(v);
            GradPhi = v =>
            {
                Vector2 gradLambda0 = node0.GradPhi(v),
                        gradLambda1 = node1.GradPhi(v),
                        gradLambda2 = node2.GradPhi(v);
                double lambda0 = node0.Phi(v),
                        lambda1 = node1.Phi(v),
                        lambda2 = node2.Phi(v);
                return new Vector2(
                    27*(gradLambda0.x*lambda1*lambda2 + gradLambda1.x*lambda2*lambda0 + gradLambda2.x*lambda0*lambda1),
                    27*(gradLambda0.y*lambda1*lambda2 + gradLambda1.y*lambda2*lambda0 + gradLambda2.y*lambda0*lambda1));
            };
        }
    }
}
