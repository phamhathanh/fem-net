using System;
using System.Collections.Generic;
using static System.Math;

namespace FEM_NET.FEM2D
{
    internal static class TimeDependentHeatProgram
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

            var g = new[] { new LambdaFunction(v => 0) };
            var conditions = new Dictionary<int, IFunction[]>()
            {
                [1] = g, [2] = g, [3] = g, [4] = g
            };
            
            int stepCount = 10;
            double t = 0,
                timeStep = 1.0 / stepCount;
            BilinearForm bilinearForm =
                (u, v, du, dv) => timeStep * Vector2.Dot(du[0], dv[0]) + u[0] * v[0];

            var previous = new IFunction[]
            {
                new LambdaFunction(v => Sin(PI*v.x)*Sin(PI*v.y))
            };
            
            for (int i = 0; i < stepCount; i++)
            {
                t += timeStep;
                Func<Vector2, double> f = v => (1 + 2*PI*PI)*Exp(t)*Sin(PI*v.x)*Sin(PI*v.y);
                var rhs = new IFunction[]
                {
                    new LambdaFunction(v => previous[0].GetValueAt(v) + timeStep*f(v))
                };
                var laplaceEquation = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
                previous = laplaceEquation.Solve();
            }
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            var solution = previous;
            Func<Vector2, double> uExact = v => Exp(t)*Sin(PI*v.x)*Sin(PI*v.y);
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
