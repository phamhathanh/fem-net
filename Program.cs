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
            // initialize

            StartMeasuringTask("Total");
            StartMeasuringTask("Initialization");

            Mesh mesh = new Mesh();
            
            Node[] nodes = mesh.GetNodes(),
                    interiorNodes = mesh.GetInteriorNodes(),
                    boundaryNodes = mesh.GetBoundaryNodes();
            FiniteElement[] finiteElements = mesh.GetFiniteElements();
            
            SparseMatrix A = new SparseMatrix(interiorNodes.Length, interiorNodes.Length);
            SparseMatrix Ag = new SparseMatrix(interiorNodes.Length, boundaryNodes.Length);

            double[] cg = new double[boundaryNodes.Length];
            for (int i = 0; i < boundaryNodes.Length; i++)
                cg[i] = Input.G(boundaryNodes[i].Position);
            Vector Cg = new Vector(cg);
            
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            Console.WriteLine("Number of interior vertices: {0}", interiorNodes.Length);
            Console.WriteLine("Number of boundary vertices: {0}", boundaryNodes.Length);
            Console.WriteLine("Number of finite elements: {0}", finiteElements.Length);
            StopAndShowTaskTime("Initialization");

            // calculate matrix and RHS

            StartMeasuringTask("Matrix & RHS calculation");

            double[] f = new double[interiorNodes.Length];
            foreach (FiniteElement fe in finiteElements)
                for (int i = 0; i < 3; i++)
                    if (fe.nodes[i].IsInside)
                    {
                        int I = fe.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (fe.nodes[j].IsInside)
                            {
                                int J = fe.nodes[j].Index;
                                A[I, J] += fe.GetLocalA(i, j);
                            }

                        f[I] += fe.GetLocalF(i);
                    }
                    else
                    {
                        int J = fe.nodes[i].Index;
                        for (int j = 0; j < 3; j++)
                            if (fe.nodes[j].IsInside)
                            {
                                int I = fe.nodes[j].Index;
                                Ag[I, J] += fe.GetLocalA(i, j);
                            }
                    }

            Vector F = new Vector(f);

            StopAndShowTaskTime("Matrix & RHS calculation");

            // solve

            StartMeasuringTask("Matrix solution");

            Calculator.CGResult result = Calculator.Solve(A, F - Ag * Cg, Input.e);
            Vector C = result.vector;
            
            Console.WriteLine("CG completed successfully: {0} iterations. Residual: {1:0.###e+00}",
                                                                    result.iterations, result.error);
            StopAndShowTaskTime("Matrix solution");
            

            // output error
            
            Func<Vector2, double> Uh = (v) =>
                {
                    double output = 0;
                    foreach (FiniteElement fe in finiteElements)
                        if (fe.Contains(v))
                        {
                            for (int i = 0; i < 3; i++)
                                if (fe.nodes[i].IsInside)
                                    output += fe.Phi(v, i) * C[fe.nodes[i].Index];
                                else
                                    output += fe.Phi(v, i) * Cg[fe.nodes[i].Index];
                            break;
                        }
                    return output;
                };

            Vector2 v0 = new Vector2(0.40594, 0.52323);
            double U0 = Input.U(v0),
                   Uh0 = Uh(v0);

            Console.WriteLine("Exact solution at  {0}: {1}", v0, U0);
            Console.WriteLine("Approx solution at {0}: {1}", v0, Uh0);
            Console.WriteLine("The error at point {0}: {1}", v0, Uh0 - U0);

            double squareError = 0;
            foreach (FiniteElement fe in finiteElements)
                squareError += fe.GetLocalSquareError(C, Cg);

            Console.WriteLine("L2 error in domain: {0}", Math.Sqrt(squareError));

            StopAndShowTaskTime("Total");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void StartMeasuringTask(string taskName)
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
    }
}
