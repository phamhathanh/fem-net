using FEM_NET.FEM2D;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Math;

namespace FEM_NET
{
    internal static class FEM2DProgram
    {
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

        private static IMesh mesh;
        private static Dictionary<int, Func<Vector2, double>> boundaryConditions;
        private const string PROBLEM_NAME = "heat2_cs";

        public static void Run()
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            var totalTimer = StartMeasuringTaskTime("Total");

            var readInputTimer = StartMeasuringTaskTime("Read input files");
            bool success = TryReadInputFiles();
            if (!success)
                return;

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");
            var laplaceEquation = new LaplaceEquation(mesh, a0, F, boundaryConditions);
            var solution = laplaceEquation.Solve();
            StopAndShowTaskTime(calculationTimer);

            InOut.WriteSolutionToFile($"example{Path.DirectorySeparatorChar}{PROBLEM_NAME}.sol", mesh, solution);
            //OutputError(mesh, solution);

            StopAndShowTaskTime(totalTimer);
        }

        private static bool TryReadInputFiles()
        {
            try
            {
                mesh = InOut.ReadMesh($"example{Path.DirectorySeparatorChar}{PROBLEM_NAME}.mesh", new P1Element.Factory());
                boundaryConditions = InOut.ReadBoundaryConditions($"example{Path.DirectorySeparatorChar}DEFAULT.stokes");
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine($"FILE NOT FOUND: {exception.Message}");
                return false;
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine($"DIRECTORY NOT FOUND: {exception.Message}");
                return false;
            }
            return true;
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

        private static Timer StartMeasuringTaskTime(string taskName)
        {
            var timer = new Timer(taskName);
            timer.Start();
            return timer;
        }

        private static void StopAndShowTaskTime(Timer timer)
        {
            timer.Stop();
            Console.WriteLine($"{timer.Name} time: {timer.Elapsed.TotalSeconds:F3} sec");
        }

        private static void ShowMeshParameters(IMesh mesh)
        {
            Console.WriteLine($"Number of vertices: {mesh.Vertices.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count}");
        }
    }
}
