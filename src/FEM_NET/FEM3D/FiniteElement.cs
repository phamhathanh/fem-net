using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEMSharp.FEM3D
{
    internal class FiniteElement
    {
        public class Node
        {
            public Vector3 Position { get; }
            public double Volume { get; }
            public int Index { get; }
            public bool IsInside { get; }

            public Func<Vector3, double> Phi { get; }
            public Func<Vector3, Vector3> GradPhi { get; }

            public Node(Vector3 thisNode, Vector3 thatNode, Vector3 otherNode, Vector3 thatOtherNode,
                        int index, bool isInside)
            {
                Position = thisNode;
                Index = index;
                IsInside = isInside;
                
                // Linear interpolation, using Cramer's rule.
                double x1 = thisNode.x, y1 = thisNode.y, z1 = thisNode.z,
                    x2 = thatNode.x, y2 = thatNode.y, z2 = thatNode.z,
                    x3 = otherNode.x, y3 = otherNode.y, z3 = otherNode.z,
                    x4 = thatOtherNode.x, y4 = thatOtherNode.y, z4 = thatOtherNode.z,
                    a = -y3*z4 - y4*z2 - y2*z3 + y3*z2 + y2*z4 + y4*z3,
                    b = -x2*z4 - x3*z2 - x4*z3 + x4*z2 + x3*z4 + x2*z3,
                    c = -x2*y3 - x3*y4 - x4*y2 + x4*y3 + x3*y2 + x2*y4,
                    d = x2*y3*z4 + x3*y4*z2 + y2*z3*x4 - x4*y3*z2 - x3*y2*z4 - x2*z3*y4,
                    denominator = a*x1 + b*y1 + c*z1 + d;

                Phi = point => (a * point.x + b * point.y + c * point.z + d) / denominator;

                var gradient = (1 / denominator) * (new Vector3(a, b, c));
                GradPhi = point => gradient;

                Volume = Math.Abs(denominator);
            }
        }

        public ReadOnlyCollection<Node> Nodes { get; }

        public FiniteElement(FEM3D.Node node0, FEM3D.Node node1, FEM3D.Node node2, FEM3D.Node node3)
        {
            var feNode0 = new Node(node0.Position, node1.Position, node2.Position, node3.Position, node0.Index, node0.IsInside);
            var feNode1 = new Node(node1.Position, node2.Position, node3.Position, node0.Position, node1.Index, node1.IsInside);
            var feNode2 = new Node(node2.Position, node3.Position, node0.Position, node1.Position, node2.Index, node2.IsInside);
            var feNode3 = new Node(node3.Position, node0.Position, node1.Position, node2.Position, node3.Index, node3.IsInside);
            Nodes = new ReadOnlyCollection<Node>(new[] { feNode0, feNode1, feNode2, feNode3 });
        }

        public double GetValueOfFunctionAtPoint(Vector coefficients, Vector3 point)
        {
            if (!this.Contains(point))
                throw new ArgumentException();

            double sum = 0;
            foreach (var node in Nodes)
                sum += node.Phi(point) * coefficients[node.Index];
            return sum;
        }

        public bool Contains(Vector3 point)
            => Nodes.All(node => node.Phi(point) >= 0);
    }
}
