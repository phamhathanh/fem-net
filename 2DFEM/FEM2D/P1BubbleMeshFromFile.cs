using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P1BubbleMeshFromFile : IMesh
    {
        private List<Vertex> nodes;
        private int interiorIndex = 0;
        public IReadOnlyCollection<Vertex> Vertices => nodes.AsReadOnly();
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public P1BubbleMeshFromFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                string rawString;
                do
                    rawString = reader.ReadLine();
                while (rawString != "Vertices");

                rawString = reader.ReadLine();
                int nodeCount = int.Parse(rawString);

                reader.ReadLine();
                var nodes = new Vertex[nodeCount];
                interiorIndex = 0;
                int boundaryIndex = 0;
                for (int i = 0; i < nodeCount; i++)
                {
                    rawString = reader.ReadLine();
                    var position = ReadPosition(rawString);
                    var isInside = !IsOnBoundary(position);
                    int index;
                    if (isInside)
                    {
                        index = interiorIndex;
                        interiorIndex++;
                    }
                    else
                    {
                        index = boundaryIndex;
                        boundaryIndex++;
                    }
                    nodes[i] = new Vertex(position, 0);//isInside);
                    throw new NotImplementedException();
                }
                this.nodes = nodes.ToList();

                do
                    rawString = reader.ReadLine();
                while (rawString != "Triangles");

                rawString = reader.ReadLine();
                int feCount = int.Parse(rawString);

                reader.ReadLine();
                var fes = new IFiniteElement[feCount];
                for (int i = 0; i < feCount; i++)
                    fes[i] = ReadFiniteElement(reader.ReadLine());
                FiniteElements = new ReadOnlyCollection<IFiniteElement>(fes);
                // TODO: Prevent copying by using own implementation of IReadOnlyCollection.
            }
        }

        private Vector2 ReadPosition(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            double x = double.Parse(items[0]),
                y = double.Parse(items[1]);
            return new Vector2(x, y);
        }

        private bool IsOnBoundary(Vector2 node)
            => node.x == 0 || node.x == 1 || node.y == 0 || node.y == 1;
        // TODO: Read from file.

        private P1bElement ReadFiniteElement(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1;

            var centerNode = new Vertex((nodes[i].Position + nodes[j].Position + nodes[k].Position) / 3, 0);//true);
            throw new NotImplementedException();
            interiorIndex++;
            nodes.Add(centerNode);
            return new P1bElement(nodes[i], nodes[j], nodes[k], centerNode);
        }

        public sealed class P1bElement : FiniteElement
        {
            public override ReadOnlyCollection<INode> Nodes { get; }

            public class FENode : INode
            {
                public Vertex Vertex { get; set; }

                public Func<Vector2, double> Phi { get; set; }
                public Func<Vector2, Vector2> GradPhi { get; set; }
            }

            public P1bElement(Vertex vertex0, Vertex vertex1, Vertex vertex2, Vertex center)
            {
                var node0 = new P1Element.Node(vertex0, vertex1, vertex2);
                var node1 = new P1Element.Node(vertex1, vertex2, vertex0);
                var node2 = new P1Element.Node(vertex2, vertex0, vertex1);
                throw new NotImplementedException();
                var feCenter = new FENode()
                {
                    Vertex = center,
                    Phi = v => 27 * node0.Phi(v) * node1.Phi(v) * node2.Phi(v),
                    GradPhi = v =>
                    {
                        Vector2 gradLambda0 = node0.GradPhi(v),
                                gradLambda1 = node1.GradPhi(v),
                                gradLambda2 = node2.GradPhi(v);
                        double lambda0 = node0.Phi(v),
                                lambda1 = node1.Phi(v),
                                lambda2 = node2.Phi(v);
                        return new Vector2(27 * (gradLambda0.x * lambda1 * lambda2 + gradLambda1.x * lambda2 * lambda0 + gradLambda2.x * lambda0 * lambda1),
                                            27 * (gradLambda0.y * lambda1 * lambda2 + gradLambda1.y * lambda2 * lambda0 + gradLambda2.y * lambda0 * lambda1));
                    }
                };
                Nodes = new ReadOnlyCollection<INode>(new[] { node0, node1, node2 });
            }
        }
    }
}
