using System;
using System.Collections.Generic;

namespace FEMSharp.FEM2D
{
    class LaplaceEquation
    {
        private readonly IMesh mesh;
        private readonly double a0;
        private readonly Func<Vector2, double> f;
        private readonly Dictionary<int, Func<Vector2, double>> boundaryConditions;
        
        private int interiorNodeCount, boundaryNodeCount;

        private Matrix A, Ag;
        private Vector rhs;

        // TODO: Change to FE function.

        // -Laplace(u) + a0 * u = f
        // u = g on boundary
        public LaplaceEquation(IMesh mesh, double a0, Func<Vector2, double> f, Dictionary<int, Func<Vector2, double>> boundaryConditions)
        {
            this.mesh = mesh;
            this.a0 = a0;
            this.f = f;
            this.boundaryConditions = boundaryConditions;
        }

        public Vector Solve()
        {
            var boundary = CalculateBoundaryCondition();
            CalculateMatrixAndRHS();
            return SolveEquation(boundary);
        }

        private Vector CalculateBoundaryCondition()
        {
            var boundary = new List<double>();

            interiorNodeCount = 0;
            boundaryNodeCount = 0;
            foreach (var node in mesh.Nodes)
            {
                if (IsInside(node))
                {
                    node.Index = interiorNodeCount;
                    interiorNodeCount++;
                }
                else
                {
                    var value = boundaryConditions[node.Reference](node.Position);
                    boundary.Add(value);
                    node.Index = boundaryNodeCount;
                    boundaryNodeCount++;
                }
            }
            return new Vector(boundary);
        }

        private bool IsInside(Node node)
            => !boundaryConditions.ContainsKey(node.Reference);

        private bool IsInside(IFENode node)
            => !boundaryConditions.ContainsKey(node.Vertex.Reference);

        private void CalculateMatrixAndRHS()
        {
            A = new Matrix(interiorNodeCount, interiorNodeCount);
            Ag = new Matrix(interiorNodeCount, boundaryNodeCount);
            var rhs = new double[interiorNodeCount];

            foreach (var finiteElement in mesh.FiniteElements)
            {
                foreach (var node in finiteElement.Nodes)
                    if (IsInside(node))
                    {
                        int I = node.Vertex.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (IsInside(otherNode))
                            {
                                int J = otherNode.Vertex.Index;
                                A[I, J] += Calculator.Integrate(v => Vector2.Dot(node.GradPhi(v), otherNode.GradPhi(v)), finiteElement)
                                    + Calculator.Integrate(v => a0 * node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }

                        rhs[I] += Calculator.Integrate(v => f(v) * node.Phi(v), finiteElement);
                    }
                    else
                    {
                        int J = node.Vertex.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (IsInside(otherNode))
                            {
                                int I = otherNode.Vertex.Index;
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
                if (IsInside(node))
                    solution[i] = result[node.Index];
                else
                    solution[i] = boundary[node.Index];
                i++;
            }
            return new Vector(solution);
        }
    }
}
