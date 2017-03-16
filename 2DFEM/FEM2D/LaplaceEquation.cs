using System;

namespace FEMSharp.FEM2D
{
    class LaplaceEquation
    {
        private readonly IMesh mesh;
        private readonly double a0;
        private readonly Func<Vector2, double> f, g;

        private Matrix A, Ag;

        private Vector rhs;
        // TODO: Change to FE function.

        // -Laplace(u) + a0 * u = f
        // u = g on boundary
        public LaplaceEquation(IMesh mesh, double a0, Func<Vector2, double> f, Func<Vector2, double> g)
        {
            this.mesh = mesh;
            this.a0 = a0;
            this.f = f;
            this.g = g;
        }

        public Vector Solve()
        {
            var boundary = CalculateBoundaryCondition();
            CalculateMatrixAndRHS();
            return SolveEquation(boundary);
        }

        private Vector CalculateBoundaryCondition()
        {
            var m = mesh.BoundaryNodes.Count;
            var boundary = new double[m];
            {
                int i = 0;
                foreach (var boundaryNode in mesh.BoundaryNodes)
                {
                    boundary[i] = g(boundaryNode.Position);
                    i++;
                }
            }
            return new Vector(boundary);
        }

        private void CalculateMatrixAndRHS()
        {
            var n = mesh.InteriorNodes.Count;
            var m = mesh.BoundaryNodes.Count;
            A = new Matrix(n, n);
            Ag = new Matrix(n, m);
            var rhs = new double[n];

            foreach (var finiteElement in mesh.FiniteElements)
            {
                foreach (var node in finiteElement.Nodes)
                    if (node.IsInside)
                    {
                        int I = node.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (otherNode.IsInside)
                            {
                                int J = otherNode.Index;
                                A[I, J] += Calculator.Integrate(v => Vector2.Dot(node.GradPhi(v), otherNode.GradPhi(v)), finiteElement)
                                    + Calculator.Integrate(v => a0 * node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }

                        rhs[I] += Calculator.Integrate(v => f(v) * node.Phi(v), finiteElement);
                    }
                    else
                    {
                        int J = node.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (otherNode.IsInside)
                            {
                                int I = otherNode.Index;
                                Ag[I, J] += Calculator.Integrate(v => Vector2.Dot(node.GradPhi(v), otherNode.GradPhi(v)), finiteElement)
                                    + Calculator.Integrate(v => a0 * node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }
                    }
            }

            this.rhs = new Vector(rhs);
        }

        private Vector SolveEquation(Vector boundary)
        {
            var epsilon = 1e-12;
            var result = Calculator.Solve(A, rhs - Ag * boundary, epsilon).vector;
            var solution = new double[mesh.Nodes.Count];
            int i = 0;
            foreach (var node in mesh.Nodes)
            {
                if (node.IsInside)
                    solution[i] = result[node.Index];
                else
                    solution[i] = boundary[node.Index];
                i++;
            }
            return new Vector(solution);
        }
    }
}
