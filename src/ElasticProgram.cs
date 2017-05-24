using System;
using System.Collections.Generic;
using static System.Math;

namespace FEM_NET.FEM2D
{
    internal static class ElasticProgram
    {
        public static void Run(string meshName, string finiteElementType, double accuracy)
        {
            var totalTimer = StartMeasuringTaskTime("Total");

            var readInputTimer = StartMeasuringTaskTime("Read input files");
            var mesh = Mesh.ReadFromFile($"{meshName}.mesh");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");
            
            var feSpaceFactoryByName = new Dictionary<string, FiniteElementSpaceFactory>() {
                ["p1"] = P1Space.Create,
                ["p1b"] = P1bSpace.Create };
            if (!feSpaceFactoryByName.ContainsKey(finiteElementType))
                throw new ArgumentException("Unknown or unimplemented finite element type.");
            var feSpace = feSpaceFactoryByName[finiteElementType](mesh);
            // Some how doesn't work with P1.

            var conditions = new Dictionary<int, IFunction[]>()
            {
                [4] = new[] { new LambdaFunction(v => 0), new LambdaFunction(v => 0) }
            };

            double YOUNG_MODULUS = 21e5, POISSON_RATIO = 0.28,
                MU = YOUNG_MODULUS/(2*(1+POISSON_RATIO)),
                LAMBDA = YOUNG_MODULUS*POISSON_RATIO/((1+POISSON_RATIO)*(1-2*POISSON_RATIO));
            BilinearForm bilinearForm =
                (u, v, du, dv) => LAMBDA*(du[0].x + du[1].y)*(dv[0].x + dv[1].y)
                                + MU*(2*du[0].x*dv[0].x + 2*du[1].y*dv[1].y + (du[0].y+du[1].x)*(dv[0].y+dv[1].x));

            var rhs = new IFunction[] { new LambdaFunction(v => 0), new LambdaFunction(v => -1) };
            var laplaceEquation = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
            var solution = laplaceEquation.Solve();
            StopAndShowTaskTime(calculationTimer);
            
            var outputTimer = StartMeasuringTaskTime("Output");
            InOut.WriteSolutionToFile($"{meshName}.sol", mesh, solution);

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

        private static void ShowMeshParameters(Mesh mesh)
        {
            Console.WriteLine($"Number of vertices: {mesh.Vertices.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.Triangles.Count}");
        }
    }
}