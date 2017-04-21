using System;
using static System.Math;

namespace FEM_NET.FEM2D
{
    internal static class HeatProgram
    {
        public static void Run(string meshName, string conditionFileName, double timeStep, int timeStepCount, double accuracy)
        {
            Console.WriteLine();
            Console.WriteLine("Solving...");
            var totalTimer = StartMeasuringTaskTime("Total");

            var readInputTimer = StartMeasuringTaskTime("Read input files");
            var mesh = InOut.ReadMesh($"{meshName}.mesh", new P1Element.Factory());
            var boundaryConditions = InOut.ReadBoundaryConditions($"{conditionFileName}");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            double a0 = 0;
            BilinearForm bilinearForm = (u, v, du, dv) => timeStep * Vector2.Dot(du, dv) + (1 + timeStep * a0) * u * v;
            Func<Vector2, double> f = v => 0,
                u0 = v => 10 + 15*v.x;

            IFiniteElementFunction previous = new LambdaFunction(u0);
            // TODO: Initial step from file
            for (int i = 0; i < timeStepCount; i++)
            {
                Func<Vector2, double> rhs = v => previous.GetValueAt(v) + timeStep*f(v);
                var laplaceEquation = new Problem(mesh, boundaryConditions, bilinearForm, rhs, accuracy);
                previous = laplaceEquation.Solve();
            }
            InOut.WriteSolutionToFile($"{meshName}.sol", mesh, previous);

            StopAndShowTaskTime(calculationTimer);
            StopAndShowTaskTime(totalTimer);
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
