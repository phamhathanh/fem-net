using System;
using System.Linq;

namespace _2DFEM
{
    class LaplaceEquation
    {
        private readonly double a0;
        private readonly Func<Vector2, double> f, g;

        private Mesh mesh;
        private int interiorNodesCount;
        private Matrix A, Ag;
        private FiniteElementFunction boundary, rhs;

        // -Laplace(u) + a0 * u = f
        // u = g on boundary
        public LaplaceEquation(double a0, Func<Vector2, double> f, Func<Vector2, double> g)
        {
            this.a0 = a0;
            this.f = f;
            this.g = g;
        }

        public Vector Solve()
        {
            throw new NotImplementedException();
            Initialize();

            CalculateMatrixAndRHS();

            var epsilon = 1e-12;
            //var result = Calculator.Solve(A, rhs - Ag * boundary, epsilon);
            //return result.vector;
        }

        private void Initialize()
        {
            var rectangle = new Rectangle(0, 1, 0, 1);
            mesh = new Mesh(127, 127, rectangle);
            interiorNodesCount = mesh.InteriorNodes.Count();
            var boundaryNodesCount = mesh.BoundaryNodes.Count();
            A = new Matrix(interiorNodesCount, interiorNodesCount);
            Ag = new Matrix(interiorNodesCount, boundaryNodesCount);

            var cg = new double[boundaryNodesCount];
            {
                int i = 0;
                foreach (var boundaryNode in mesh.BoundaryNodes)
                {
                    cg[i] = g(boundaryNode.Position);
                    i++;
                }
            }
            boundary = new FiniteElementFunction(mesh, cg);
        }

        private void CalculateMatrixAndRHS()
        {
            var rhs = new double[interiorNodesCount];

            foreach (var finiteElement in mesh.FiniteElements)
                for (int i = 0; i < 3; i++)
                    if (finiteElement.nodes[i].IsInside)
                    {
                        int I = finiteElement.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (finiteElement.nodes[j].IsInside)
                            {
                                int J = finiteElement.nodes[j].Index;
                                A[I, J] += finiteElement.GetLocalStiffness(i, j) + finiteElement.GetLocalMass(i, j);
                            }

                        rhs[I] += finiteElement.GetLocalRHS(i);
                    }
                    else
                    {
                        int J = finiteElement.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (finiteElement.nodes[j].IsInside)
                            {
                                int I = finiteElement.nodes[j].Index;
                                Ag[I, J] += finiteElement.GetLocalStiffness(i, j) + finiteElement.GetLocalMass(i, j);
                            }
                    }

            this.rhs = new FiniteElementFunction(mesh, rhs);
        }
    }
}
