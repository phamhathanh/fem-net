using System;
using System.Collections.Generic;
using static System.Math;
using static FEM_NET.Utils;

namespace FEM_NET.FEM2D
{
    internal static class HomogeneousStationaryHeatProgram
    {
        public static void Run(string meshPath, string finiteElementType, double accuracy)
        {
            var totalTimer = StartMeasuringTaskTime("Total");
            var readInputTimer = StartMeasuringTaskTime("Read input files");
            var mesh = Mesh.ReadFromFile($"{meshPath}.mesh");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            var feSpace = CreateFiniteElementSpace(finiteElementType, mesh);
            var g = new LambdaVectorField(v => 0);
            var conditions = new Dictionary<int, IVectorField>()
            {
                [1] = g, [2] = g, [3] = g, [4] = g
            };
            
            var bilinearForm = new BilinearForm(
                (u, v, du, dv) => Vector2.Dot(du, dv) + u*v);
            var rhs = new LambdaVectorField((x, y) => (1 + 2*PI*PI)*Sin(PI*x)*Sin(PI*y));
            
            var solver = new ConjugateGradient(accuracy);

            var poisson = new Problem(feSpace, conditions, bilinearForm, rhs, solver);
            var solution = poisson.Solve();
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            Func<double, double, double> uExact = (x, y) => Sin(PI*x)*Sin(PI*y);
            var error = CalculateError(feSpace, uExact, solution);
            Console.WriteLine($"L2 Error = {error}");

            StopAndShowTaskTime(errorCalculationTimer);
            
            var outputTimer = StartMeasuringTaskTime("Output");
            InOut.WriteSolutionToFile($"{meshPath}.sol", mesh, solution);

            StopAndShowTaskTime(outputTimer);
            StopAndShowTaskTime(totalTimer);
        }
    }
}
