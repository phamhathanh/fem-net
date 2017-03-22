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
        private const double a0 = 0;

        private static double F(Vector2 v)
        {
            return 0;
            double x = v.x,
                   y = v.y;

            return -4 / ((x + y + 1) * (x + y + 1) * (x + y + 1)) + 2 * Math.PI * Math.PI * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y) + a0 * U(v);
        }

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
            var mesh = new P1MeshFromFile("heat2_cs.mesh");
            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Read mesh");

            var boundaryConditions = ReadBoundaryConditions("DEFAULT.stokes");

            StartMeasuringTaskTime("Calculation");
            var laplaceEquation = new LaplaceEquation(mesh, a0, F, boundaryConditions);
            var solution = laplaceEquation.Solve();
            StopAndShowTaskTime("Calculation");

            WriteSolutionToFile("heat2_cs.sol", mesh, solution);
            //OutputError(mesh, solution);

            StopAndShowTaskTime("Total");
            Console.ReadLine();
        }

        private static Dictionary<int, Func<Vector2, double>> ReadBoundaryConditions(string path)
        {
            Dictionary<int, Func<Vector2, double>> conditions;
            using (var reader = new StreamReader(path))
            {
                string rawString;
                do
                    rawString = reader.ReadLine();
                while (rawString != "dirichlet");

                if (reader.EndOfStream)
                    throw new NotImplementedException();
                // TODO: other types of condition.

                rawString = reader.ReadLine();
                int conditionCount = int.Parse(rawString);
                conditions = new Dictionary<int, Func<Vector2, double>>(conditionCount);
                for (int i = 0; i < conditionCount; i++)
                {
                    rawString = reader.ReadLine();
                    var words = rawString.Split(' ');   // TODO: Use separator.
                    int reference = int.Parse(words[0]);
                    double value = double.Parse(words[3]);
                    // TODO: Parse function instead of constant.

                    conditions.Add(reference, v => value);
                }
            }
            return conditions;
        }

        private static void WriteSolutionToFile(string path, IMesh mesh, Vector solution)
        {
            using (var solutionFile = new StreamWriter(path))
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

            Console.WriteLine($"L2 error in domain: {Math.Sqrt(squareError)}");
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
            Console.WriteLine($"Number of vertices: {mesh.Nodes.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count}");
        }
    }
}
