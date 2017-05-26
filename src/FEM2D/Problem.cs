using System;
using System.Collections.Generic;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class Problem
    {
        private const double VERY_LARGE_VALUE = 1e30;

        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Dictionary<int, IFunction[]> boundaryConditions;
        private readonly BilinearForm bilinearForm;
        private readonly IFunction[] rightHandSide;
        private readonly double accuracy;

        private Matrix A;
        private Vector rhs;

        public Problem(IFiniteElementSpace finiteElementSpace, Dictionary<int, IFunction[]> boundaryConditions,
                        BilinearForm bilinearForm, ICollection<IFunction> rightHandSide,
                        double accuracy)
                        // TODO: use vector valued function instead of array of scalar
        {
            this.finiteElementSpace = finiteElementSpace;
            this.boundaryConditions = boundaryConditions;

            this.bilinearForm = bilinearForm;
            this.rightHandSide = rightHandSide.ToArray();
            // TODO: Use ImmutableArray.

            this.accuracy = accuracy;
        }

        public IFunction[] Solve()
        // TODO: Use ImmutableArray.
        {
            CalculateMatrixAndRHS();
            var rawSolution = ConjugateGradient.Solve(A, rhs, accuracy);
            
            int dim = rightHandSide.Length;
            var solution = new IFunction[dim];
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
            int dim = rightHandSide.Length;
            A = new Matrix(vertexCount*dim, vertexCount*dim);
            var rhs = new double[vertexCount*dim];

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
                        rhs[i] += GaussianQuadrature.Integrate(v => rightHandSide[n].GetValueAt(v) * node.Phi(v), finiteElement.Triangle);
                        foreach (var otherNode in finiteElement.Nodes)
                            for (int m = 0; m < dim; m++)
                            {
                                int j = indexByVertex[m][otherNode.Vertex];
                                Func<Vector2, double> localBilinearForm =
                                    p => {
                                        var u = new double[dim];
                                        var v = new double[dim];
                                        u[n] = node.Phi(p);
                                        v[m] = otherNode.Phi(p);
                                        var du = new Vector2[dim];
                                        var dv = new Vector2[dim];
                                        du[n] = node.GradPhi(p);
                                        dv[m] = otherNode.GradPhi(p);
                                        return bilinearForm(u, v, du, dv);
                                    };
                                var integral = GaussianQuadrature.Integrate(localBilinearForm, finiteElement.Triangle);
                                A[i, j] += integral;
                                // TODO: cache.
                            }
                    }

            this.rhs = new Vector(rhs);
        }
    }
}
