using System;
using System.Collections.Generic;
using static System.Math;
using static FEM_NET.Utils;

namespace FEM_NET.FEM2D
{
    internal static class NonhomogeneousStationaryHeatProgram
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
            var conditions = new Dictionary<int, IVectorField>()
            {
                [1] = new LambdaVectorField(v => 25)
            };
            
            var bilinearForm = new BilinearForm(
                (u, v, du, dv) => Vector2.Dot(du, dv));
            var rhs = new LambdaVectorField(v => -4);
            var solver = new ConjugateGradient(accuracy);

            var poisson = new Problem(feSpace, conditions, bilinearForm, rhs, solver);
            var solution = poisson.Solve();
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            Func<double, double, double> uExact = (x, y) => (x-0.5)*(x-0.5) + (y-0.5)*(y-0.5) - 0.25 + 25;
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
