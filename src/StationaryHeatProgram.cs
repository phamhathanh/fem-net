using System;
using System.Collections.Generic;
using static System.Math;
using static FEM_NET.Utils;

namespace FEM_NET.FEM2D
{
    internal static class StationaryHeatProgram
    {
        public static void Run(string meshPath, string finiteElementType, double accuracy)
        {
            var totalTimer = StartMeasuringTaskTime("Total");
            var readInputTimer = StartMeasuringTaskTime("Read input files");
            var mesh = Mesh.ReadFromFile($"{meshPath}.mesh");

            ShowMeshParameters(mesh);
            StopAndShowTaskTime(readInputTimer);

            var calculationTimer = StartMeasuringTaskTime("Calculation");

            var feSpace = GetFESpaceFactory(finiteElementType)(mesh);
            var conditions = new Dictionary<int, IVectorField>()
            {
                [1] = new LambdaVectorField(v => 25)
            };
            
            BilinearForm bilinearForm =
                (u, v, du, dv) => Vector2.Dot(du[0], dv[0]);
            var rhs = new LambdaVectorField(v => -4);

            var poisson = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
            var solution = (FiniteElementVectorField)poisson.Solve();
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            Func<Vector2, double> uExact = v => (v.x-0.5)*(v.x-0.5) + (v.y-0.5)*(v.y-0.5) - 0.25 + 25;
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
