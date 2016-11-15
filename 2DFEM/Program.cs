using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            var mesh = new Mesh();
            
            var boundaryNodes = mesh.GetBoundaryNodes();
            var finiteElements = mesh.GetFiniteElements();
            
            Matrix A = new Matrix(mesh.InteriorNodesCount, mesh.InteriorNodesCount);
            Matrix Ag = new Matrix(mesh.InteriorNodesCount, mesh.BoundaryNodesCount);

            double[] cg = new double[mesh.BoundaryNodesCount];
            {
                int i = 0;
                foreach (var boundaryNode in boundaryNodes)
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

            double[] rhs = new double[mesh.InteriorNodesCount];

            foreach (FiniteElement fe in finiteElements)
                for (int i = 0; i < 3; i++)
                    if (fe.nodes[i].IsInside)
                    {
                        int I = fe.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (fe.nodes[j].IsInside)
                            {
                                int J = fe.nodes[j].Index;
                                A[I, J] += fe.GetLocalStiffness(i, j) + fe.GetLocalMass(i, j);
                            }

                        rhs[I] += fe.GetLocalRHS(i);
                    }
                    else
                    {
                        int J = fe.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (fe.nodes[j].IsInside)
                            {
                                int I = fe.nodes[j].Index;
                                Ag[I, J] += fe.GetLocalStiffness(i, j) + fe.GetLocalMass(i, j);
                            }
                    }

            Vector F = new Vector(rhs);

            StopAndShowTaskTime("Matrix & RHS calculation");

            // solve

            StartMeasuringTaskTime("Matrix solution");

            Calculator.CGResult result = Calculator.Solve(A, F - Ag * Cg, Input.e);
            Vector C = result.vector;
            
            Console.WriteLine("CG completed successfully: {0} iterations. Residual: {1:0.###e+00}",
                                                                    result.iterations, result.error);
            StopAndShowTaskTime("Matrix solution");
            

            // output error

            var point = new Vector2(0.40594, 0.52323);
            double exactSolution = Input.U(point),
                   approxSolution = 0;

            foreach (FiniteElement fe in finiteElements)
                if (fe.Contains(point))
                {
                    approxSolution = fe.GetSolutionAtPoint(C, Cg, point);
                    break;
                }

            ShowPointError(point, exactSolution, approxSolution);

            double squareError = 0;
            foreach (FiniteElement fe in finiteElements)
                squareError += fe.GetLocalSquareError(C, Cg);

            Console.WriteLine("L2 error in domain: {0}", Math.Sqrt(squareError));

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
            Console.WriteLine(taskName + " time: {0:F3} sec", taskTime.TotalSeconds);
        }

        private static void ShowMeshParameters(Mesh mesh)
        {
            Console.WriteLine("Number of interior vertices: {0}", mesh.InteriorNodesCount);
            Console.WriteLine("Number of boundary vertices: {0}", mesh.BoundaryNodesCount);
            Console.WriteLine("Number of finite elements: {0}", mesh.FiniteElementsCount);
        }

        private static void ShowPointError(Vector2 point, double exact, double approx)
        {
            Console.WriteLine("Exact solution at  {0}: {1}", point, exact);
            Console.WriteLine("Approx solution at {0}: {1}", point, approx);
            Console.WriteLine("The error at point {0}: {1}", point, approx - exact);
        }
    }
}
