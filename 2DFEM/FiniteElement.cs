using System;

namespace _2DFEM
{
    class FiniteElement
    {
        // cancer
        public readonly Node[] nodes;
        // cancer

        private readonly double[] localRHS;
        private readonly double[,] localStiffness;
        private readonly double[,] localMass;

        private readonly Func<Vector2, double> basisFunction0, basisFunction1, basisFunction2;

        public FiniteElement(Node node1, Node node2, Node node3)
        {
            this.nodes = new Node[] { node1, node2, node3 };

            var gradPhi = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                Node nj = nodes[(i + 1) % 3],
                     nk = nodes[(i + 2) % 3];
                gradPhi[i] = nodes[i].GradPhi(nj, nk);
            }

            Vector2 u1 = node2.Position - node1.Position,
                    u2 = node3.Position - node1.Position;
            double area = Math.Abs(u1.x * u2.y - u1.y * u2.x) / 2;

            basisFunction0 = CreateBasisFunction(node1, node2, node3);
            basisFunction1 = CreateBasisFunction(node2, node3, node1);
            basisFunction2 = CreateBasisFunction(node3, node1, node2);
            var basisFunctions = new[] { basisFunction0, basisFunction1, basisFunction2 };

            this.localRHS = new double[3];
            this.localStiffness = new double[3, 3];
            this.localMass = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    localStiffness[i, j] = Vector2.Dot(gradPhi[i], gradPhi[j]) * area;
                    localMass[i, j] = Calculator.Integrate(v => Input.a0 * basisFunctions[i](v) * basisFunctions[j](v), nodes);
                }

                if (nodes[i].IsInside)
                    localRHS[i] = Calculator.Integrate(v => Input.F(v) * basisFunctions[i](v), nodes);
            }
        }

        private Func<Vector2, double> CreateBasisFunction(Node thisNode, Node thatNode, Node thatOtherNode)
        {
            double x1 = thisNode.Position.x,
                   y1 = thisNode.Position.y,
                   x2 = thatNode.Position.x,
                   y2 = thatNode.Position.y,
                   x3 = thatOtherNode.Position.x,
                   y3 = thatOtherNode.Position.y;

            // Linear interpolation.
            return point => ((y3 - y2) * point.x + (x2 - x3) * point.y + y2 * x3 - x2 * y3)
                    / (x3 * y2 - x2 * y3 + x2 * y1 - x3 * y1 + x1 * y3 - x1 * y2);
        }

        public double Integrate(IFunction<Vector2, double> function)
            => Calculator.Integrate(p => function.GetValueAt(p), nodes);

        public double GetLocalStiffness(int nodeIndex1, int nodeIndex2)
            => localStiffness[nodeIndex1, nodeIndex2];

        public double GetLocalMass(int nodeIndex1, int nodeIndex2)
            => localMass[nodeIndex1, nodeIndex2];


        // TODO: Abstractize this.
        public double GetLocalRHS(int nodeIndex)
            => localRHS[nodeIndex];

        public double GetSolutionAtPoint(Vector C, Vector Cg, Vector2 v)
        {
            if (!this.Contains(v))
                return 0;

            var basisFunctions = new[] { basisFunction0, basisFunction1, basisFunction2 };
            double sum = 0;
            for (int i = 0; i < 3; i++)
                if (nodes[i].IsInside)
                    sum += basisFunctions[i](v) * C[nodes[i].Index];
                else
                    sum += basisFunctions[i](v) * Cg[nodes[i].Index];

            return sum;
        }

        public double GetValueOfFunctionAtPoint(Vector coefficients, Vector2 point)
        {
            if (!this.Contains(point))
                throw new ArgumentException();

            double sum = 0;
            sum += basisFunction0(point) * coefficients[nodes[0].Index];
            sum += basisFunction1(point) * coefficients[nodes[1].Index];
            sum += basisFunction2(point) * coefficients[nodes[2].Index];
            return sum;
        }

        public double GetLocalSquareError(Vector C, Vector Cg)
        {
            var basisFunctions = new[] { basisFunction0, basisFunction1, basisFunction2 };
            Func<Vector2, double> error = v =>
            {
                double u0 = Input.U(v),
                       uh0 = 0;

                for (int i = 0; i < 3; i++)
                {
                    Node node = nodes[i];
                    if (node.IsInside)
                        uh0 += basisFunctions[i](v) * C[node.Index];
                    else
                        uh0 += basisFunctions[i](v) * Cg[node.Index];
                }
                return (u0 - uh0) * (u0 - uh0);
            };

            return Calculator.Integrate(error, nodes);
        }

        public bool Contains(Vector2 v)
        {
            Vector2 p0 = nodes[0].Position,
                    p1 = nodes[1].Position,
                    p2 = nodes[2].Position;

            double signedArea = (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y),
                s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * v.x + (p0.x - p2.x) * v.y) / (2 * signedArea),
                t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * v.x + (p1.x - p0.x) * v.y) / (2 * signedArea);

            return s >= 0 && t >= 0 && (s + t) <= 1;
        }
    }
}
