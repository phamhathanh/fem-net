using System;
using System.Collections.Generic;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class Problem
    {
        private const double VERY_LARGE_VALUE = 1e30;

        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Dictionary<int, IFiniteElementFunction[]> boundaryConditions;
        private readonly BilinearForm[] bilinearForm;
        private readonly IFiniteElementFunction[] rightHandSide;
        private readonly int dim;
        private readonly double accuracy;

        private Matrix A;
        private Vector rhs;

        public Problem(IFiniteElementSpace finiteElementSpace, Dictionary<int, IFiniteElementFunction[]> boundaryConditions,
                        ICollection<BilinearForm> bilinearForm, ICollection<IFiniteElementFunction> rightHandSide,
                        double accuracy)
        {
            this.finiteElementSpace = finiteElementSpace;
            this.boundaryConditions = boundaryConditions;

            this.dim = bilinearForm.Count;
            if (dim != rightHandSide.Count)
                throw new ArgumentException("Dimension mismatch.");
            this.bilinearForm = bilinearForm.ToArray();
            this.rightHandSide = rightHandSide.ToArray();
            // TODO: Use ImmutableArray.

            this.accuracy = accuracy;
        }

        public IFiniteElementFunction[] Solve()
        // TODO: Use ImmutableArray.
        {
            CalculateMatrixAndRHS();
            var rawSolution = Calculator.Solve(A, rhs, accuracy).vector;
            var solution = new IFiniteElementFunction[dim];
            var m = finiteElementSpace.Vertices.Count;
            for (int n = 0; n < dim; n++)
            {
                var values = rawSolution.Skip(n*m).Take(m);
                solution[n] = new FiniteElementFunction(finiteElementSpace, values);
            }
            return solution;
        }

        private void CalculateMatrixAndRHS()
        {
            var vertexCount = finiteElementSpace.Vertices.Count;
            A = new Matrix(vertexCount*dim, vertexCount*dim);
            var rhs = new double[vertexCount];

            var indexByVertex = new Dictionary<Vertex, int>[dim];
            int index = 0;
            for (int n = 0; n < dim; n++)
            {
                indexByVertex[n] = new Dictionary<Vertex, int>(vertexCount);
                foreach (var vertex in finiteElementSpace.Vertices)
                {
                    bool isDirichletNode = boundaryConditions.ContainsKey(vertex.Reference);
                    if (isDirichletNode)
                    {
                        var value = boundaryConditions[vertex.Reference][n].GetValueAt(vertex.Position);
                        rhs[index] += VERY_LARGE_VALUE*value;
                        A[index, index] += VERY_LARGE_VALUE;
                    }
                    indexByVertex[n].Add(vertex, index);
                    index++;
                }
            }

            foreach (var finiteElement in finiteElementSpace.FiniteElements)
                foreach (var node in finiteElement.Nodes)
                    for (int n = 0; n < dim; n++)
                    {
                        int i = indexByVertex[n][node.Vertex];
                        rhs[i] += Calculator.Integrate(v => rightHandSide[n].GetValueAt(v) * node.Phi(v), finiteElement.Triangle);
                        foreach (var otherNode in finiteElement.Nodes)
                        {
                            int j = indexByVertex[n][otherNode.Vertex];
                            Func<Vector2, double> localBilinearForm =
                                v => bilinearForm[n](node.Phi(v), otherNode.Phi(v), node.GradPhi(v), otherNode.GradPhi(v));
                            var integral = Calculator.Integrate(localBilinearForm, finiteElement.Triangle);
                            A[i, j] += integral;
                            // TODO: cache.
                        }
                    }

            this.rhs = new Vector(rhs);
        }
    }
}
