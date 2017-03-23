using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    class LaplaceEquation
    {
        private readonly IMesh mesh;
        private readonly double a0;
        private readonly Func<Vector2, double> f;
        private readonly Dictionary<int, Func<Vector2, double>> boundaryConditions;
        
        private int interiorVertexCount, boundaryVertexCount;

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

            interiorVertexCount = 0;
            boundaryVertexCount = 0;
            foreach (var vertex in mesh.Vertices)
            {
                if (IsInside(vertex))
                {
                    vertex.Index = interiorVertexCount;
                    interiorVertexCount++;
                }
                else
                {
                    var value = boundaryConditions[vertex.Reference](vertex.Position);
                    boundary.Add(value);
                    vertex.Index = boundaryVertexCount;
                    boundaryVertexCount++;
                }
            }
            return new Vector(boundary);
        }

        private bool IsInside(Vertex vertex)
            => !boundaryConditions.ContainsKey(vertex.Reference);

        private bool IsInside(INode node)
            => IsInside(node.Vertex);

        private void CalculateMatrixAndRHS()
        {
            A = new Matrix(interiorVertexCount, interiorVertexCount);
            Ag = new Matrix(interiorVertexCount, boundaryVertexCount);
            var rhs = new double[interiorVertexCount];

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
            var solution = new double[mesh.Vertices.Count];
            int i = 0;
            foreach (var node in mesh.Vertices)
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
