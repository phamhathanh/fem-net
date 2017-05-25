using System;
using System.Collections.Generic;
using static System.Math;

namespace FEM_NET.FEM2D
{
    internal static class HeatProgram
    {
        public static void Run(string meshPath, string finiteElementType, double accuracy)
        {
            var totalTimer = StartMeasuringTaskTime("Total");
            var readInputTimer = StartMeasuringTaskTime("Read input files");
            var mesh = Mesh.ReadFromFile($"{meshPath}.mesh");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            var feSpaceFactoryByName = new Dictionary<string, FiniteElementSpaceFactory>() {
                ["p1"] = P1Space.Create,
                ["p1b"] = P1bSpace.Create };
            if (!feSpaceFactoryByName.ContainsKey(finiteElementType))
                throw new ArgumentException("Unknown or unimplemented finite element type.");
            var feSpace = feSpaceFactoryByName[finiteElementType](mesh);

            var conditions = new Dictionary<int, IFunction[]>()
            {
                [1] = new[] { new LambdaFunction(v => 25) }
            };
            
            BilinearForm bilinearForm =
                (u, v, du, dv) => Vector2.Dot(du[0], dv[0]);
            var rhs = new[] { new LambdaFunction(v => -4) };
            var poisson = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
            var solution = poisson.Solve();
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            Func<Vector2, double> uExact = v => (v.x-0.5)*(v.x-0.5) + (v.y-0.5)*(v.y-0.5) - 0.25 + 25;
            var error = CalculateError(feSpace, uExact, (FiniteElementFunction)solution[0]);
            Console.WriteLine($"L2 Error = {error}");

            StopAndShowTaskTime(errorCalculationTimer);
            
            var outputTimer = StartMeasuringTaskTime("Output");
            InOut.WriteSolutionToFile($"{meshPath}.sol", mesh, solution);

            StopAndShowTaskTime(outputTimer);
            StopAndShowTaskTime(totalTimer);
        }

        private static double CalculateError(IFiniteElementSpace feSpace,
                        Func<Vector2, double> exactSolution, FiniteElementFunction solution)
        {
            double squareError = 0;
            foreach (var element in feSpace.FiniteElements)
            {
                Func<Vector2, double> error = v =>
                {
                    double u0 = exactSolution(v),
                        uh0 = 0;
                    foreach (var node in element.Nodes)
                        uh0 += node.Phi(v) * solution.GetValueAt(node.Vertex);
                    return (u0 - uh0) * (u0 - uh0);
                };
                squareError += GaussianQuadrature.Integrate(error, element.Triangle);
            }
            return Sqrt(squareError);
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
