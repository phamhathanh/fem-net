using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P2MeshFromFile : IMesh
    {
        private List<Vertex> nodes;
        private int interiorIndex = 0,
                    boundaryIndex = 0;
        public IReadOnlyCollection<Vertex> Vertices => nodes.AsReadOnly();
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public P2MeshFromFile(string path)
        // TODO: Remove FromFile duplication.
        {
            using (var reader = File.OpenText(path))
            {
                string rawString;
                do
                    rawString = reader.ReadLine();
                while (rawString != "Vertices");

                rawString = reader.ReadLine();
                int nodeCount = int.Parse(rawString);

                reader.ReadLine();
                var nodes = new Vertex[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                {
                    rawString = reader.ReadLine();

                    var items = rawString.Split(' ');
                    double x = double.Parse(items[0]),
                        y = double.Parse(items[1]);
                    int reference = int.Parse(items[2]);
                    // TODO: Format exception.

                    var position = new Vector2(x, y);
                    var isInside = reference == 0;
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
                    nodes[i] = new Vertex(position, reference);
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

        private P2Element ReadFiniteElement(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1;

            Vertex node0 = nodes[i],
                node1 = nodes[j],
                node2 = nodes[k];
            Vector2 p0 = node0.Position,
                    p1 = node1.Position,
                    p2 = node2.Position,
                    p3 = (p0 + p1) / 2,
                    p4 = (p1 + p2) / 2,
                    p5 = (p2 + p0) / 2;

            var node01 = AddNode(p3, false);//ode0.IsInside && node1.IsInside);
            var node12 = AddNode(p4, false);//ode1.IsInside && node2.IsInside);
            var node20 = AddNode(p5, false);//node2.IsInside && node0.IsInside);
            throw new NotImplementedException();
            return new P2Element(node0, node1, node2, node01, node12, node20);
        }

        private Vertex AddNode(Vector2 position, bool isInside)
        {
            Vertex node;
            if (isInside)
            {
                node = new Vertex(position, 0);//isInside);
                interiorIndex++;
            }
            else
            {
                node = new Vertex(position, 0);// isInside);
                boundaryIndex++;
            }
            throw new NotImplementedException();
            nodes.Add(node);
            return node;
        }

        public sealed class P2Element : IFiniteElement
        {
            public ReadOnlyCollection<INode> Nodes { get; }

            public class Node : INode
            {
                public Vertex Vertex { get; set; }

                public Func<Vector2, double> Phi { get; set; }
                public Func<Vector2, Vector2> GradPhi { get; set; }

                public Node(Vertex node)
                {
                    Vertex = node;
                }
            }

            public P2Element(Vertex node0, Vertex node1, Vertex node2, Vertex node01, Vertex node12, Vertex node20)
            {
                var temp0 = new P1Element.Node(node0, node1, node2);
                var temp1 = new P1Element.Node(node1, node2, node0);
                var temp2 = new P1Element.Node(node2, node0, node1);
                throw new NotImplementedException();
                Func<Vector2, Vector2> gradLambda0 = temp0.GradPhi,
                                        gradLambda1 = temp1.GradPhi,
                                        gradLambda2 = temp2.GradPhi;
                Func<Vector2, double> lambda0 = temp0.Phi,
                                        lambda1 = temp1.Phi,
                                        lambda2 = temp2.Phi;

                var feNode0 = new Node(node0)
                {
                    Phi = v => lambda0(v) * (2 * lambda0(v) - 1),
                    GradPhi = v => (4 * lambda0(v) - 1) * gradLambda0(v)
                };
                var feNode1 = new Node(node1)
                {
                    Phi = v => lambda1(v) * (2 * lambda1(v) - 1),
                    GradPhi = v => (4 * lambda1(v) - 1) * gradLambda1(v)
                };
                var feNode2 = new Node(node2)
                {
                    Phi = v => lambda2(v) * (2 * lambda2(v) - 1),
                    GradPhi = v => (4 * lambda2(v) - 1) * gradLambda2(v)
                };

                var feNode01 = new Node(node01)
                {
                    Phi = v => 4 * lambda0(v) * lambda1(v),
                    GradPhi = v => 4 * (lambda0(v) * gradLambda1(v) + lambda1(v) * gradLambda0(v))
                };
                var feNode12 = new Node(node12)
                {
                    Phi = v => 4 * lambda1(v) * lambda2(v),
                    GradPhi = v => 4 * (lambda1(v) * gradLambda2(v) + lambda2(v) * gradLambda1(v))
                };
                var feNode20 = new Node(node20)
                {
                    Phi = v => 4 * lambda2(v) * lambda0(v),
                    GradPhi = v => 4 * (lambda2(v) * gradLambda0(v) + lambda0(v) * gradLambda2(v))
                };

                Nodes = new ReadOnlyCollection<INode>(new[] { feNode0, feNode1, feNode2, feNode01, feNode12, feNode20 });
            }

            public bool Contains(Vector2 point)
            {
                throw new NotImplementedException();
            }
        }
    }
}
