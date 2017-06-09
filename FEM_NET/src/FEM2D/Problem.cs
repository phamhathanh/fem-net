using System;
using System.Collections.Generic;
using System.Linq;

namespace FEM_NET.FEM2D
{
    public class Problem
    {
        private const double VERY_LARGE_VALUE = 1e30;

        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Dictionary<int, IVectorField> boundaryConditions;
        private readonly BilinearForm bilinearForm;
        private readonly IVectorField rightHandSide;
        private readonly ISolver solver;
        private readonly IQuadrature quadrature;

        private Matrix A;
        private Vector rhs;

        public Problem(IFiniteElementSpace finiteElementSpace,
                        Dictionary<int, IVectorField> boundaryConditions,
                        BilinearForm bilinearForm, IVectorField rightHandSide,
                        ISolver solver = null, IQuadrature quadrature = null)
        {
            this.finiteElementSpace = finiteElementSpace;
            this.boundaryConditions = boundaryConditions;

            this.bilinearForm = bilinearForm;
            this.rightHandSide = rightHandSide;
            var dim = rightHandSide.Dimension;
            var mismatch = boundaryConditions.Values
                            .Any(condition => condition.Dimension != dim);
            if (mismatch)
                throw new ArgumentException("Dimension mismatched.");
            // TODO: Validate bilinear form.

            this.solver = solver ?? new ConjugateGradient(1e-6);
            this.quadrature = quadrature ?? new GaussianQuadrature();
        }

        public IVectorField Solve()
        {
            CalculateMatrixAndRHS();
            var rawSolution = solver.Solve(A, rhs);
            
            int dim = rightHandSide.Dimension;
            var solution = new FiniteElementScalarField[dim];
            var m = finiteElementSpace.Vertices.Count;
            for (int n = 0; n < dim; n++)
            {
                var values = rawSolution.Skip(n*m).Take(m);
                solution[n] = new FiniteElementScalarField(finiteElementSpace, values);
            }
            return new FiniteElementVectorField(solution);
        }

        private void CalculateMatrixAndRHS()
        {
            var vertexCount = finiteElementSpace.Vertices.Count;
            int dim = rightHandSide.Dimension;
            A = new Matrix(vertexCount*dim, vertexCount*dim);
            var rhs = new double[vertexCount*dim];

            var indexByVertex = new Dictionary<Vertex, int>[dim];
            int index = 0;
            for (int n = 0; n < dim; n++)
            {
                indexByVertex[n] = new Dictionary<Vertex, int>(vertexCount);
                foreach (var vertex in finiteElementSpace.Vertices)
                {
                    bool isDirichletNode = boundaryConditions.ContainsKey(vertex.Label);
                    if (isDirichletNode)
                    {
                        var value = boundaryConditions[vertex.Label].GetValueAt(vertex.Position, n);
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
                        rhs[i] += quadrature.Integrate(v => rightHandSide.GetValueAt(v, n) * node.Phi(v), finiteElement.Triangle);
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
                                        return bilinearForm.Evaluate(u, v, du, dv);
                                    };
                                    // TODO: Extract to bilinear form?
                                var integral = quadrature.Integrate(localBilinearForm, finiteElement.Triangle);
                                A[i, j] += integral;
                                // TODO: cache.
                            }
                    }

            this.rhs = new Vector(rhs);
        }
    }
}
