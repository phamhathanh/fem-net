using System;
using System.Collections.ObjectModel;

namespace _2DFEM
{
    internal class FiniteElement
    {
        public class Node
        {
            public Vector2 Position { get; }
            public int Index { get; }
            public bool IsInside { get; }

            public Func<Vector2, double> Phi { get; }
            public Func<Vector2, Vector2> GradPhi { get; }

            public Node(Vector2 thisNode, Vector2 thatNode, Vector2 thatOtherNode,
                        int index, bool isInside)
            {
                Position = thisNode;
                Index = index;
                IsInside = isInside;

                Phi = CreateBasisFunction(thisNode, thatNode, thatOtherNode);
                GradPhi = CreateGradientOfBasisFunction(thisNode, thatNode, thatOtherNode);
            }

            private Func<Vector2, double> CreateBasisFunction(Vector2 thisNode, Vector2 thatNode, Vector2 thatOtherNode)
            {
                double x1 = thisNode.x,
                       y1 = thisNode.y,
                       x2 = thatNode.x,
                       y2 = thatNode.y,
                       x3 = thatOtherNode.x,
                       y3 = thatOtherNode.y;

                // Linear interpolation.
                return point => ((y3 - y2) * point.x + (x2 - x3) * point.y + y2 * x3 - x2 * y3)
                        / (x3 * y2 - x2 * y3 + x2 * y1 - x3 * y1 + x1 * y3 - x1 * y2);
            }

            private Func<Vector2, Vector2> CreateGradientOfBasisFunction(Vector2 thisNode, Vector2 thatNode, Vector2 thatOtherNode)
            {
                double x1 = thisNode.x,
                       y1 = thisNode.y,
                       x2 = thatNode.x,
                       y2 = thatNode.y,
                       x3 = thatOtherNode.x,
                       y3 = thatOtherNode.y;

                var gradient = new Vector2((y3 - y2) / (x3 * y2 - x2 * y3 + x2 * y1 - x3 * y1 + x1 * y3 - x1 * y2),
                                (x2 - x3) / (x3 * y2 - x2 * y3 + x2 * y1 - x3 * y1 + x1 * y3 - x1 * y2));
                return point => gradient;
            }
        }

        public ReadOnlyCollection<Node> Nodes { get; }

        public FiniteElement(_2DFEM.Node node0, _2DFEM.Node node1, _2DFEM.Node node2)
        {
            var feNode0 = new Node(node0.Position, node1.Position, node2.Position, node0.Index, node0.IsInside);
            var feNode1 = new Node(node1.Position, node2.Position, node0.Position, node1.Index, node1.IsInside);
            var feNode2 = new Node(node2.Position, node0.Position, node1.Position, node2.Index, node2.IsInside);
            Nodes = new ReadOnlyCollection<Node>(new[] { feNode0, feNode1, feNode2 });
        }
        public double GetValueOfFunctionAtPoint(Vector coefficients, Vector2 point)
        {
            if (!this.Contains(point))
                throw new ArgumentException();

            double sum = 0;
            foreach (var node in Nodes)
                if (node.IsInside)
                    sum += node.Phi(point) * coefficients[node.Index];
            return sum;
        }

        public bool Contains(Vector2 v)
        {
            Vector2 p0 = Nodes[0].Position,
                    p1 = Nodes[1].Position,
                    p2 = Nodes[2].Position;

            double signedArea = (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y),
                s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * v.x + (p0.x - p2.x) * v.y) / (2 * signedArea),
                t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * v.x + (p1.x - p0.x) * v.y) / (2 * signedArea);

            return s >= 0 && t >= 0 && (s + t) <= 1;
        }
    }
}
