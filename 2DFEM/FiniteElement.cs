using System;

namespace _2DFEM
{
    class FiniteElement
    {
        // precalculated value of local mass matrix, to be replaced
        private static readonly double[,] m0 =
            {{0.083333333333333333333333333333333333333333333333333333333333,
            0.041666666666666666666666666666666666666666666666666666666666,
            0.041666666666666666666666666666666666666666666666666666666666},
            {0.041666666666666666666666666666666666666666666666666666666666,
            0.083333333333333333333333333333333333333333333333333333333333,
            0.041666666666666666666666666666666666666666666666666666666666},
            {0.041666666666666666666666666666666666666666666666666666666666,
            0.041666666666666666666666666666666666666666666666666666666666,
            0.083333333333333333333333333333333333333333333333333333333333}};

        // cancer
        public readonly Node[] nodes;
        // cancer

        private readonly double[] localRHS;
        private readonly double[,] localStiffness;
        private readonly double[,] localMass;

        public FiniteElement(Node node1, Node node2, Node node3)
        {
            this.nodes = new Node[] { node1, node2, node3 };
            
            Vector2[] gradPhi = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                Node nj = nodes[(i + 1) % 3],
                     nk = nodes[(i + 2) % 3];
                gradPhi[i] = nodes[i].GradPhi(nj, nk);
            }

            Vector2 u1 = node2.Position - node1.Position,
                    u2 = node3.Position - node1.Position;

            double area = Math.Abs(u1.x * u2.y - u1.y * u2.x) / 2;

            this.localRHS = new double[3];
            this.localStiffness = new double[3, 3];
            this.localMass = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    localStiffness[i, j] = Vector2.Dot(gradPhi[i], gradPhi[j]) * area;
                    localMass[i, j] = Input.a0 * m0[i, j] * 2 * area;
                }

                if (nodes[i].IsInside)
                    localRHS[i] = Calculator.Integrate((v) => Input.F(v) * Phi(v, i), nodes);
            }
        }

        private double Phi(Vector2 v, int nodeIndex)
        {
            Node node1 = nodes[nodeIndex],
                 node2 = nodes[(nodeIndex + 1) % 3],
                 node3 = nodes[(nodeIndex + 2) % 3];

            return node1.Phi(node2, node3, v);
        }

        public double GetLocalStiffness(int nodeIndex1, int nodeIndex2)
            => localStiffness[nodeIndex1, nodeIndex2];

        public double GetLocalMass(int nodeIndex1, int nodeIndex2)
            => localMass[nodeIndex1, nodeIndex2];

        public double GetLocalRHS(int nodeIndex)
            => localRHS[nodeIndex];

        public double GetSolutionAtPoint(Vector C, Vector Cg, Vector2 v)
        {
            if (!this.Contains(v))
                return 0;

            double sum = 0;
            for (int i = 0; i < 3; i++)
                if (nodes[i].IsInside)
                    sum += Phi(v, i) * C[nodes[i].Index];
                else
                    sum += Phi(v, i) * Cg[nodes[i].Index];

            return sum;
        }

        public double GetLocalSquareError(Vector C, Vector Cg)
        {
            Func<Vector2, double> error = (v) =>
            {
                double u0 = Input.U(v),
                       uh0 = 0;

                for (int i = 0; i < 3; i++)
                {
                    Node node = nodes[i];
                    if (node.IsInside)
                        uh0 += Phi(v, i) * C[node.Index];
                    else
                        uh0 += Phi(v, i) * Cg[node.Index];
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
