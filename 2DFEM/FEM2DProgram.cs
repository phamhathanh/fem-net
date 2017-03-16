using FEMSharp.FEM2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FEMSharp
{
    internal static class FEM2DProgram
    {
        private static readonly Dictionary<string, Stopwatch> taskTimers = new Dictionary<string, Stopwatch>();

        private const double a0 = 1;
        private const int n = 127, m = n;

        private static double F(Vector2 v)
        {
            double x = v.x,
                   y = v.y;

            return -4 / ((x + y + 1) * (x + y + 1) * (x + y + 1)) + 2 * Math.PI * Math.PI * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y) + a0 * U(v);
        }

        private static double G(Vector2 v)
            => U(v);

        private static double U(Vector2 v)
        {
            double x = v.x,
                   y = v.y;

            return 1 / (x + y + 1) + Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y) + 99;
        }

        public static void Run()
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            StartMeasuringTaskTime("Total");

            StartMeasuringTaskTime("Read mesh");
            var mesh = new MeshFromFile("square.mesh");
            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Read mesh");

            StartMeasuringTaskTime("Calculation");
            var laplaceEquation = new LaplaceEquation(mesh, a0, F, G);
            var solution = laplaceEquation.Solve();
            StopAndShowTaskTime("Calculation");

            WriteSolutionToFile(mesh, solution);

            StopAndShowTaskTime("Total");
            Console.ReadLine();
        }

        private static void WriteSolutionToFile(IMesh mesh, Vector solution)
        {
            using (var solutionFile = new StreamWriter($"square.sol"))
            {
                solutionFile.WriteLine(
$@"MeshVersionFormatted 1

Dimension 2

SolAtVertices
{mesh.Nodes.Count}
2 2 1
");
                int i = 0;
                foreach (var node in mesh.Nodes)
                {
                    solutionFile.WriteLine($"{solution[i]} {node.Position.x} {node.Position.y}");
                    i++;
                }

                solutionFile.WriteLine(
$@"
SolAtTriangles
0
1 2

End");
            }
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
            Console.WriteLine($"Number of interior vertices: {mesh.InteriorNodes.Count}");
            Console.WriteLine($"Number of boundary vertices: {mesh.BoundaryNodes.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count}");
        }
    }
}
