using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class Problem
    {
        private const double VERY_LARGE_VALUE = 1e30;

        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly IFiniteElementFunction rightHandSide;
        private readonly Dictionary<int, IFiniteElementFunction> boundaryConditions;
        private readonly BilinearForm bilinearForm;
        private readonly double accuracy;

        private Matrix A;
        private Vector rhs;

        public Problem(IFiniteElementSpace finiteElementSpace, Dictionary<int, IFiniteElementFunction> boundaryConditions,
                        BilinearForm bilinearForm, IFiniteElementFunction rightHandSide,
                        double accuracy)
        {
            this.finiteElementSpace = finiteElementSpace;
            this.boundaryConditions = boundaryConditions;
            this.bilinearForm = bilinearForm;
            this.rightHandSide = rightHandSide;
            this.accuracy = accuracy;
        }

        public IFiniteElementFunction Solve()
        {
            CalculateMatrixAndRHS();
            var solution = Calculator.Solve(A, rhs, accuracy).vector;
            return new FiniteElementFunction(finiteElementSpace, solution);
        }

        private void CalculateMatrixAndRHS()
        {
            var n = finiteElementSpace.Vertices.Count;
            A = new Matrix(n, n);
            var rhs = new double[n];

            var indexByVertex = new Dictionary<Vertex, int>(n);
            {
                int i = 0;
                foreach (var vertex in finiteElementSpace.Vertices)
                {
                    bool isDirichletNode = boundaryConditions.ContainsKey(vertex.Reference);
                    if (isDirichletNode)
                    {
                        var value = boundaryConditions[vertex.Reference].GetValueAt(vertex.Position);
                        rhs[i] += VERY_LARGE_VALUE*value;
                        A[i, i] += VERY_LARGE_VALUE;
                    }
                    indexByVertex.Add(vertex, i);
                    i++;
                }
            }

            foreach (var finiteElement in finiteElementSpace.FiniteElements)
                foreach (var node in finiteElement.Nodes)
                {
                    int i = indexByVertex[node.Vertex];
                    rhs[i] += Calculator.Integrate(v => rightHandSide.GetValueAt(v) * node.Phi(v), finiteElement.Triangle);
                    foreach (var otherNode in finiteElement.Nodes)
                    {
                        int j = indexByVertex[otherNode.Vertex];
                        Func<Vector2, double> localBilinearForm =
                            v => bilinearForm(node.Phi(v), otherNode.Phi(v), node.GradPhi(v), otherNode.GradPhi(v));
                        var integral = Calculator.Integrate(localBilinearForm, finiteElement.Triangle);
                        A[i, j] += integral;
                        // TODO: cache.
                    }
                }

            this.rhs = new Vector(rhs);
        }
    }
}
