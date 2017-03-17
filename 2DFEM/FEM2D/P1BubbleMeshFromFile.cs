using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P1BubbleMeshFromFile : IMesh
    {
        private List<Node> nodes;
        private int interiorIndex = 0;
        public IReadOnlyCollection<Node> Nodes => nodes.AsReadOnly();
        public IReadOnlyCollection<Node> InteriorNodes { get; }
        public IReadOnlyCollection<Node> BoundaryNodes { get; }
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
                var nodes = new Node[nodeCount];
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
                    nodes[i] = new Node(position, index, isInside);
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

                BoundaryNodes = (from node in Nodes
                                 where IsOnBoundary(node.Position)
                                 select node).ToList().AsReadOnly();
                InteriorNodes = (from node in Nodes
                                 where !IsOnBoundary(node.Position)
                                 select node).ToList().AsReadOnly();
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

        private FiniteElement ReadFiniteElement(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1;

            var centerNode = new Node((nodes[i].Position + nodes[j].Position + nodes[k].Position) / 3, interiorIndex, true);
            interiorIndex++;
            nodes.Add(centerNode);
            return new FiniteElement(nodes[i], nodes[j], nodes[k], centerNode);
        }

        public class FiniteElement : IFiniteElement
        {
            public ReadOnlyCollection<IFENode> Nodes { get; }

            public class FENode : IFENode
            {
                public Vector2 Position { get; set; }
                public int Index { get; set; }
                public bool IsInside { get; set; }

                public Func<Vector2, double> Phi { get; set; }
                public Func<Vector2, Vector2> GradPhi { get; set; }
            }

            public FiniteElement(Node node0, Node node1, Node node2, Node center)
            {
                var feNode0 = new P1FiniteElement.FENode(node0.Position, node1.Position, node2.Position, node0.Index, node0.IsInside);
                var feNode1 = new P1FiniteElement.FENode(node1.Position, node2.Position, node0.Position, node1.Index, node1.IsInside);
                var feNode2 = new P1FiniteElement.FENode(node2.Position, node0.Position, node1.Position, node2.Index, node2.IsInside);
                var feCenter = new FENode()
                {
                    Position = center.Position,
                    Index = center.Index,
                    IsInside = true,
                    Phi = v => 27 * feNode0.Phi(v) * feNode1.Phi(v) * feNode2.Phi(v),
                    GradPhi = v =>
                    {
                        Vector2 gradLambda0 = feNode0.GradPhi(v),
                                gradLambda1 = feNode1.GradPhi(v),
                                gradLambda2 = feNode2.GradPhi(v);
                        double lambda0 = feNode0.Phi(v),
                                lambda1 = feNode1.Phi(v),
                                lambda2 = feNode2.Phi(v);
                        return new Vector2(27 * (gradLambda0.x * lambda1 * lambda2 + gradLambda1.x * lambda2 * lambda0 + gradLambda2.x * lambda0 * lambda1),
                                            27 * (gradLambda0.y * lambda1 * lambda2 + gradLambda1.y * lambda2 * lambda0 + gradLambda2.y * lambda0 * lambda1));
                    }
                };
                Nodes = new ReadOnlyCollection<IFENode>(new[] { feNode0, feNode1, feNode2 });
            }

            public bool Contains(Vector2 point)
                => Nodes.All(node => node.Phi(point) >= 0);
        }
    }
}
