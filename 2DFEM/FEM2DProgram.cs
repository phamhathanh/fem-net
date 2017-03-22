using FEMSharp.FEM2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Math;

namespace FEMSharp
{
    internal static class FEM2DProgram
    {
        private static readonly Dictionary<string, Stopwatch> taskTimers = new Dictionary<string, Stopwatch>();
        private const double a0 = 0;

        private static double F(Vector2 v)
        {
            return 0;
            double x = v.x,
                   y = v.y;

            return -4 / ((x + y + 1) * (x + y + 1) * (x + y + 1)) + 2 * PI * PI * Sin(PI * x) * Sin(PI * y) + a0 * U(v);
        }

        private static double U(Vector2 v)
        {
            double x = v.x,
                   y = v.y;

            return 1 / (x + y + 1) + Sin(PI * x) * Sin(PI * y) + 99;
        }

        public static void Run()
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            StartMeasuringTaskTime("Total");

            var problemName = "heat2_cs";

            StartMeasuringTaskTime("Read mesh");
            var mesh = InOut.LoadMeshFromFile($"{problemName}.mesh", new P1Element.Factory());
            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Read mesh");

            var boundaryConditions = InOut.ReadBoundaryConditions("DEFAULT.stokes");

            StartMeasuringTaskTime("Calculation");
            var laplaceEquation = new LaplaceEquation(mesh, a0, F, boundaryConditions);
            var solution = laplaceEquation.Solve();
            StopAndShowTaskTime("Calculation");

            InOut.WriteSolutionToFile($"{problemName}.sol", mesh, solution);
            //OutputError(mesh, solution);

            StopAndShowTaskTime("Total");
            Console.ReadLine();
        }

        private static void OutputError(IMesh mesh, Vector solution)
        {
            double squareError = 0;
            foreach (var finiteElement in mesh.FiniteElements)
            {
                Func<Vector2, double> error = v =>
                {
                    double u0 = U(v),
                           uh0 = 0;

                    foreach (var node in finiteElement.Nodes)
                        uh0 += node.Phi(v) * solution[node.Vertex.Index];
                    return (u0 - uh0) * (u0 - uh0);
                };
                squareError += Calculator.Integrate(error, finiteElement);
            }

            Console.WriteLine($"L2 error in domain: {Sqrt(squareError)}");
        }

        private static void StartMeasuringTaskTime(string taskName)
        // TODO: Encapsulate into own timer class.
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            taskTimers.Add(taskName, stopwatch);
        }

        private static void StopAndShowTaskTime(string taskName)
        {
            var stopwatch = taskTimers[taskName];
            stopwatch.Stop();
            taskTimers.Remove(taskName);

            TimeSpan taskTime = stopwatch.Elapsed;
            Console.WriteLine($"{taskName} time: {taskTime.TotalSeconds:F3} sec");
        }

        private static void ShowMeshParameters(IMesh mesh)
        {
            Console.WriteLine($"Number of vertices: {mesh.Vertices.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count}");
        }
    }
}
