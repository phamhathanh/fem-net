using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace _2DFEM
{
    class Program
    {
        private static Dictionary<string, Stopwatch> taskTimers = new Dictionary<string, Stopwatch>();

        private static void Main(string[] args)
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");

            // initialize

            StartMeasuringTaskTime("Total");
            StartMeasuringTaskTime("Initialization");

            var rectangle = new Rectangle(0, 1, 0, 1);
            var mesh = new Mesh(127, 127, rectangle);

            Matrix A = new Matrix(mesh.InteriorNodes.Count, mesh.InteriorNodes.Count);
            Matrix Ag = new Matrix(mesh.InteriorNodes.Count, mesh.BoundaryNodes.Count);

            double[] cg = new double[mesh.BoundaryNodes.Count];

            {
                int i = 0;
                foreach (var boundaryNode in mesh.BoundaryNodes)
                {
                    cg[i] = Input.G(boundaryNode.Position);
                    i++;
                }
            }
            Vector Cg = new Vector(cg);
            
            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Initialization");

            // calculate matrix and RHS

            StartMeasuringTaskTime("Matrix & RHS calculation");

            double[] rhs = new double[mesh.InteriorNodes.Count];

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

            Vector F = new Vector(rhs);

            StopAndShowTaskTime("Matrix & RHS calculation");

            // solve

            StartMeasuringTaskTime("Matrix solution");

            var epsilon = 1e-12;
            Calculator.CGResult result = Calculator.Solve(A, F - Ag * Cg, epsilon);
            Vector C = result.vector;
            
            Console.WriteLine($"CG completed successfully: {result.iterations} iterations. Residual: {result.error:0.###e+00}");
            StopAndShowTaskTime("Matrix solution");
            

            // output error

            var point = new Vector2(0.40594, 0.52323);
            double exactSolution = Input.U(point),
                   approxSolution = 0;

            foreach (var finiteElement in mesh.FiniteElements)
                if (finiteElement.Contains(point))
                {
                    approxSolution = finiteElement.GetSolutionAtPoint(C, Cg, point);
                    break;
                }

            ShowPointError(point, exactSolution, approxSolution);

            double squareError = 0;
            foreach (var finiteElement in mesh.FiniteElements)
                squareError += finiteElement.GetLocalSquareError(C, Cg);

            Console.WriteLine($"L2 error in domain: {Math.Sqrt(squareError)}");

            StopAndShowTaskTime("Total");

            Console.ReadLine();
        }

        private static void StartMeasuringTaskTime(string taskName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            taskTimers.Add(taskName, stopwatch);
        }

        private static void StopAndShowTaskTime(string taskName)
        {
            Stopwatch stopwatch = taskTimers[taskName];
            stopwatch.Stop();
            taskTimers.Remove(taskName);

            TimeSpan taskTime = stopwatch.Elapsed;
            Console.WriteLine($"{taskName} time: {taskTime.TotalSeconds:F3} sec");
        }

        private static void ShowMeshParameters(Mesh mesh)
        {
            Console.WriteLine($"Number of interior vertices: {mesh.InteriorNodes.Count()}");
            Console.WriteLine($"Number of boundary vertices: {mesh.BoundaryNodes.Count()}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count()}");
        }

        private static void ShowPointError(Vector2 point, double exact, double approx)
        {
            Console.WriteLine($"Exact solution at  {point}: {exact}");
            Console.WriteLine($"Approx solution at {point}: {approx}");
            Console.WriteLine($"The error at point {point}: {approx - exact}");
        }
    }
}
