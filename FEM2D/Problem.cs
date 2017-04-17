using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    class Problem
    {
        private readonly IMesh mesh;
        private readonly Func<Vector2, double> f;
        private readonly Dictionary<int, Func<Vector2, double>> boundaryConditions;
        private readonly BilinearForm bilinearForm;

        private int interiorVertexCount, boundaryVertexCount;
        private Dictionary<Vertex, int> indexByVertex = new Dictionary<Vertex, int>();

        private Matrix A, Ag;
        private Vector rhs;

        public Problem(IMesh mesh, Dictionary<int, Func<Vector2, double>> boundaryConditions, BilinearForm bilinearForm, Func<Vector2, double> f)
        {
            this.mesh = mesh;
            this.boundaryConditions = boundaryConditions;
            this.bilinearForm = bilinearForm;
            this.f = f;
        }

        public FiniteElementFunction Solve()
        {
            var boundary = CalculateBoundaryCondition();
            CalculateMatrixAndRHS();
            var values = SolveEquation(boundary);
            return new FiniteElementFunction(mesh, values);
        }

        private Vector CalculateBoundaryCondition()
        {
            var boundary = new List<double>();

            interiorVertexCount = 0;
            boundaryVertexCount = 0;
            foreach (var vertex in mesh.Vertices)
                if (IsInside(vertex))
                {
                    indexByVertex.Add(vertex, interiorVertexCount);
                    interiorVertexCount++;
                }
                else
                {
                    indexByVertex.Add(vertex, boundaryVertexCount);
                    boundaryVertexCount++;
                    var value = boundaryConditions[vertex.Reference](vertex.Position);
                    boundary.Add(value);
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
                foreach (var node in finiteElement.Nodes)
                {
                    int I = indexByVertex[node.Vertex];
                    if (IsInside(node))
                        rhs[I] += Calculator.Integrate(v => f(v) * node.Phi(v), finiteElement.Triangle);
                    foreach (var otherNode in finiteElement.Nodes)
                    {
                        int J = indexByVertex[otherNode.Vertex];
                        Func<Vector2, double> localBilinearForm = v => bilinearForm(node.Phi(v), otherNode.Phi(v), node.GradPhi(v), otherNode.GradPhi(v));
                        var integral = Calculator.Integrate(v => bilinearForm(node.Phi(v), otherNode.Phi(v), node.GradPhi(v), otherNode.GradPhi(v)), finiteElement.Triangle);
                        if (!IsInside(otherNode))
                            continue;

                        if (IsInside(node))
                            A[I, J] += integral;
                        else
                            Ag[J, I] += integral;
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
            foreach (var vertex in mesh.Vertices)
            {
                if (IsInside(vertex))
                    solution[i] = result[indexByVertex[vertex]];
                else
                    solution[i] = boundary[indexByVertex[vertex]];
                i++;
            }
            return new Vector(solution);
        }
    }
}
