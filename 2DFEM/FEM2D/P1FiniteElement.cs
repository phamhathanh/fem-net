using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P1FiniteElement : IFiniteElement
    {
        public class FENode : IFENode
        {
            public Vector2 Position { get; }
            public int Index { get; }
            public bool IsInside { get; }

            public Func<Vector2, double> Phi { get; }
            public Func<Vector2, Vector2> GradPhi { get; }

            public FENode(Vector2 thisNode, Vector2 thatNode, Vector2 thatOtherNode,
                        int index, bool isInside)
            {
                Position = thisNode;
                Index = index;
                IsInside = isInside;

                // Linear interpolation, using Cramer's rule.
                double x1 = thisNode.x, y1 = thisNode.y,
                    x2 = thatNode.x, y2 = thatNode.y,
                    x3 = thatOtherNode.x, y3 = thatOtherNode.y,
                    a = y3 - y2,
                    b = x2 - x3,
                    c = y2 * x3 - x2 * y3,
                    denominator = a * x1 + b * y1 + c;
                Phi = point => (a * point.x + b * point.y + c) / denominator;

                var gradient = (1 / denominator) * (new Vector2(a, b));
                GradPhi = point => gradient;
            }
        }

        public ReadOnlyCollection<IFENode> Nodes { get; }

        public P1FiniteElement(Node node0, Node node1, Node node2)
        {
            var feNode0 = new FENode(node0.Position, node1.Position, node2.Position, node0.Index, node0.IsInside);
            var feNode1 = new FENode(node1.Position, node2.Position, node0.Position, node1.Index, node1.IsInside);
            var feNode2 = new FENode(node2.Position, node0.Position, node1.Position, node2.Index, node2.IsInside);
            Nodes = new ReadOnlyCollection<IFENode>(new[] { feNode0, feNode1, feNode2 });
        }

        public bool Contains(Vector2 point)
            => Nodes.All(node => node.Phi(point) >= 0);
    }
}
