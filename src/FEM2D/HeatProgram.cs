using System;
using System.Collections.Generic;
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
            var conditions = InOut.ReadBoundaryConditions($"{conditionFileName}");
            var mesh = new MeshFromFile($"{meshName}.mesh");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            var feSpace = new P1bSpace(mesh);

            double a0 = 0;
            var bilinearForm = new BilinearForm[] {
                (u, v, du, dv) => timeStep * Vector2.Dot(du, dv) + (1 + timeStep * a0) * u * v
            };

                /* 
                 * Also: P1b vs P1 (variable from multiple finite element space)
                 */

            Func<Vector2, double> f = v => 0,
                u0 = v => 10 + 15*v.x;

            var previous = new IFiniteElementFunction[]
            {
                new LambdaFunction(u0)
            };
            // TODO: Initial step from file
            for (int i = 0; i < timeStepCount; i++)
            {
                var rhs = new IFiniteElementFunction[]
                {
                    new LambdaFunction(v => previous[0].GetValueAt(v) + timeStep*f(v))
                };
                var laplaceEquation = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
                previous = laplaceEquation.Solve();
            }
            StopAndShowTaskTime(calculationTimer);
            
            var outputTimer = StartMeasuringTaskTime("Output");
            InOut.WriteSolutionToFile($"{meshName}.sol", mesh, previous);

            StopAndShowTaskTime(outputTimer);
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
            Console.WriteLine($"Number of finite elements: {mesh.Triangles.Count}");
        }
    }
}
