using System;
using System.Collections.Generic;
using FEM_NET.FEM2D;
using static System.Math;
using static FEM_NET.Examples.Utils;

namespace FEM_NET.Examples
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

            var feSpace = CreateFiniteElementSpace(finiteElementType, mesh);
            var g = new LambdaVectorField(v => 0);
            var conditions = new Dictionary<int, IVectorField>()
            {
                [1] = g, [2] = g, [3] = g, [4] = g
            };
            
            int stepCount = 20;
            double t = 0,
                dt = 1.0 / stepCount;
            var bilinearForm = new BilinearForm(
                (u, v, du, dv) => dt * Vector2.Dot(du, dv) + u*v);
            var solver = new ConjugateGradient(accuracy);

            IVectorField previous = new LambdaVectorField((x, y) => Sin(PI*x)*Sin(PI*y));
            
            for (int i = 0; i < stepCount; i++)
            {
                t += dt;
                Func<Vector2, double> f = v => (1 + 2*PI*PI)*Exp(t)*Sin(PI*v.x)*Sin(PI*v.y);
                var rhs = new LambdaVectorField(v => previous.GetValueAt(v, 0) + dt*f(v));
                var laplaceEquation = new Problem(feSpace, conditions, bilinearForm, rhs, solver);
                previous = laplaceEquation.Solve();
            }
            
            StopAndShowTaskTime(calculationTimer);
            var errorCalculationTimer = StartMeasuringTaskTime("Error calculation");

            var solution = (FiniteElementVectorField)previous;
            Func<double, double, double> uExact = (x, y) => Exp(t)*Sin(PI*x)*Sin(PI*y);
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
