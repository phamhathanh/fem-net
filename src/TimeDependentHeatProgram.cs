using System;
using System.Collections.Generic;
using static System.Math;
using static FEM_NET.Utils;

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

            var g = new LambdaVectorField(v => 0);
            var conditions = new Dictionary<int, IVectorField>()
            {
                [1] = g, [2] = g, [3] = g, [4] = g
            };
            
            int stepCount = 10;
            double t = 0,
                timeStep = 1.0 / stepCount;
            BilinearForm bilinearForm =
                (u, v, du, dv) => timeStep * Vector2.Dot(du[0], dv[0]) + u[0] * v[0];

            IVectorField previous = new LambdaVectorField((x, y) => Sin(PI*x)*Sin(PI*y));
            
            for (int i = 0; i < stepCount; i++)
            {
                t += timeStep;
                Func<Vector2, double> f = v => (1 + 2*PI*PI)*Exp(t)*Sin(PI*v.x)*Sin(PI*v.y);
                var rhs = new LambdaVectorField(v => previous.GetValueAt(v, 0) + timeStep*f(v));
                var laplaceEquation = new Problem(feSpace, conditions, bilinearForm, rhs, accuracy);
                previous = laplaceEquation.Solve();
            }
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            var solution = (FiniteElementVectorField)previous;
            Func<Vector2, double> uExact = v => Exp(t)*Sin(PI*v.x)*Sin(PI*v.y);
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
