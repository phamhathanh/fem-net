using System;
using System.Collections.Generic;
using System.IO;
using static System.Math;

namespace FEM_NET.FEM2D
{
    internal static class FEM2DProgram
    {
        private static IMesh mesh;
        private static Dictionary<int, Func<Vector2, double>> boundaryConditions;
        private const string PROBLEM_NAME = "heat2_cs";

        public static void Run()
        {
            Console.WriteLine("FEM for solving equation: du/dt - Laplace(u) + a0 * u = F");
            var totalTimer = StartMeasuringTaskTime("Total");

            var readInputTimer = StartMeasuringTaskTime("Read input files");
            bool success = TryReadInputFiles();
            if (!success)
                return;

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            // TODO: Time step from file / command.
            double a0 = 0,
                dt = 0.1;
            BilinearForm bilinearForm = (u, v, du, dv) => dt * Vector2.Dot(du, dv) + (1 + dt * a0) * u * v;
            Func<Vector2, double> f = v => 0,
                u0 = v => 10 + 15*v.x;

            IFiniteElementFunction previous = new LambdaFunction(u0);
            // TODO: Initial step from file
            for (int i = 0; i < 30; i++)
            {
                Func<Vector2, double> rhs = v => previous.GetValueAt(v) + dt*f(v);
                var laplaceEquation = new Problem(mesh, boundaryConditions, bilinearForm, rhs);
                previous = laplaceEquation.Solve();
            }
            InOut.WriteSolutionToFile($"example{Path.DirectorySeparatorChar}{PROBLEM_NAME}.sol", mesh, previous);

            StopAndShowTaskTime(calculationTimer);
            StopAndShowTaskTime(totalTimer);
        }

        private static bool TryReadInputFiles()
        {
            try
            {
                mesh = InOut.ReadMesh($"example{Path.DirectorySeparatorChar}{PROBLEM_NAME}.mesh", new P1Element.Factory());
                boundaryConditions = InOut.ReadBoundaryConditions($"example{Path.DirectorySeparatorChar}DEFAULT.heat");
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
