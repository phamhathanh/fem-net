using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace _2DFEM
{
    class LaplaceEquation
    {
        private readonly double a0;
        private readonly Func<Vector2, double> f, g, u;

        private readonly Dictionary<string, Stopwatch> taskTimers = new Dictionary<string, Stopwatch>();

        private Mesh mesh;
        private int interiorNodesCount;
        private Matrix A, Ag;

        private Vector boundary, rhs, result;

        // TODO: Change to a FE function.

        // -Laplace(u) + a0 * u = f
        // u = g on boundary
        public LaplaceEquation(double a0, Func<Vector2, double> f, Func<Vector2, double> g, Func<Vector2, double> u)
        {
            this.a0 = a0;
            this.f = f;
            this.g = g;
            this.u = u;
        }

        public void SolveAndDisplay()
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            StartMeasuringTaskTime("Total");

            Initialize();
            CalculateMatrixAndRHS();
            SolveMatrix();

            StopAndShowTaskTime("Total");
        }

        private void Initialize()
        {
            StartMeasuringTaskTime("Initialization");

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
            boundary = new Vector(cg);

            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Initialization");
        }

        private void CalculateMatrixAndRHS()
        {
            StartMeasuringTaskTime("Matrix & RHS calculation");

            double[] rhs = new double[mesh.InteriorNodes.Count];
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

            StopAndShowTaskTime("Matrix & RHS calculation");
        }

        private void SolveMatrix()
        {
            StartMeasuringTaskTime("Matrix solution");

            var epsilon = 1e-12;
            Calculator.CGResult result = Calculator.Solve(A, rhs - Ag * boundary, epsilon);
            this.result = result.vector;

            Console.WriteLine($"CG completed successfully: {result.iterations} iterations. Residual: {result.error:0.###e+00}");

            StopAndShowTaskTime("Matrix solution");
        }

        private void OutputError()
        {
            var point = new Vector2(0.40594, 0.52323);
            // Some random point inside the mesh.
            double exactSolution = u(point),
                   approxSolution = 0;

            foreach (var finiteElement in mesh.FiniteElements)
                if (finiteElement.Contains(point))
                {
                    foreach (var node in finiteElement.Nodes)
                        if (node.IsInside)
                            approxSolution += node.Phi(point) * result[node.Index];
                        else
                            approxSolution += node.Phi(point) * boundary[node.Index];
                    break;
                }

            ShowPointError(point, exactSolution, approxSolution);

            double squareError = 0;
            foreach (var finiteElement in mesh.FiniteElements)
            {
                Func<Vector2, double> error = v =>
                {
                    double u0 = u(v),
                           uh0 = 0;

                    foreach (var node in finiteElement.Nodes)
                        if (node.IsInside)
                            uh0 += node.Phi(v) * result[node.Index];
                        else
                            uh0 += node.Phi(v) * boundary[node.Index];
                    return (u0 - uh0) * (u0 - uh0);
                };
                squareError += Calculator.Integrate(error, finiteElement);
            }

            Console.WriteLine($"L2 error in domain: {Math.Sqrt(squareError)}");
        }

        private void StartMeasuringTaskTime(string taskName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            taskTimers.Add(taskName, stopwatch);
        }

        private void StopAndShowTaskTime(string taskName)
        {
            Stopwatch stopwatch = taskTimers[taskName];
            stopwatch.Stop();
            taskTimers.Remove(taskName);

            TimeSpan taskTime = stopwatch.Elapsed;
            Console.WriteLine($"{taskName} time: {taskTime.TotalSeconds:F3} sec");
        }

        private void ShowMeshParameters(Mesh mesh)
        {
            Console.WriteLine($"Number of interior vertices: {mesh.InteriorNodes.Count()}");
            Console.WriteLine($"Number of boundary vertices: {mesh.BoundaryNodes.Count()}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count()}");
        }

        private void ShowPointError(Vector2 point, double exact, double approx)
        {
            Console.WriteLine($"Exact solution at  {point}: {exact}");
            Console.WriteLine($"Approx solution at {point}: {approx}");
            Console.WriteLine($"The error at point {point}: {approx - exact}");
        }
    }
}
