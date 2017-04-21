using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class Problem
    {
        private const double VERY_LARGE_VALUE = 1e30;

        private readonly IMesh mesh;
        private readonly Func<Vector2, double> rightHandSide;
        private readonly Dictionary<int, Func<Vector2, double>> boundaryConditions;
        private readonly BilinearForm bilinearForm;
        private readonly double accuracy;

        private Matrix A;
        private Vector rhs;

        public Problem(IMesh mesh, Dictionary<int, Func<Vector2, double>> boundaryConditions,
                        BilinearForm bilinearForm, Func<Vector2, double> rightHandSide,
                        double accuracy)
        {
            this.mesh = mesh;
            this.boundaryConditions = boundaryConditions;
            this.bilinearForm = bilinearForm;
            this.rightHandSide = rightHandSide;
            this.accuracy = accuracy;
        }

        public IFiniteElementFunction Solve()
        {
            CalculateMatrixAndRHS();
            var solution = Calculator.Solve(A, rhs, accuracy).vector;
            return new FiniteElementFunction(mesh, solution);
        }

        private void CalculateMatrixAndRHS()
        {
            var n = mesh.Vertices.Count;
            A = new Matrix(n, n);
            var rhs = new double[n];

            var indexByVertex = new Dictionary<Vertex, int>(n);
            {
                int i = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    indexByVertex.Add(vertex, i);
                    bool isDirichletNode = boundaryConditions.ContainsKey(vertex.Reference);
                    if (isDirichletNode)
                    {
                        var value = boundaryConditions[vertex.Reference](vertex.Position);
                        rhs[i] += VERY_LARGE_VALUE*value;
                        A[i, i] += VERY_LARGE_VALUE;
                    }
                    i++;
                }
            }

            foreach (var finiteElement in mesh.FiniteElements)
                foreach (var node in finiteElement.Nodes)
                {
                    int i = indexByVertex[node.Vertex];
                    rhs[i] += Calculator.Integrate(v => rightHandSide(v) * node.Phi(v), finiteElement.Triangle);
                    foreach (var otherNode in finiteElement.Nodes)
                    {
                        int j = indexByVertex[otherNode.Vertex];
                        Func<Vector2, double> localBilinearForm =
                            v => bilinearForm(node.Phi(v), otherNode.Phi(v), node.GradPhi(v), otherNode.GradPhi(v));
                        var integral = Calculator.Integrate(localBilinearForm, finiteElement.Triangle);
                        A[i, j] += integral;
                        // Can be cached.
                    }
                }

            this.rhs = new Vector(rhs);
        }
    }
}
