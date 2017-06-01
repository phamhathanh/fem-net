using System;
using System.Collections.Generic;
using System.Diagnostics;
using FEM_NET.FEM2D;

namespace FEM_NET
{
    internal static class Utils
    {
        public static double CalculateError(IFiniteElementSpace feSpace,
                        Func<Vector2, double> exactSolution, FiniteElementVectorField solution)
        {
            double squareError = 0;
            foreach (var element in feSpace.FiniteElements)
            {
                Func<Vector2, double> error = v =>
                {
                    double u0 = exactSolution(v),
                        uh0 = 0;
                    foreach (var node in element.Nodes)
                        uh0 += node.Phi(v) * solution.GetValueAt(node.Vertex, 0);
                    return (u0 - uh0) * (u0 - uh0);
                };
                squareError += GaussianQuadrature.Integrate(error, element.Triangle);
            }
            return Math.Sqrt(squareError);
        }

        public static Timer StartMeasuringTaskTime(string taskName)
        {
            var timer = new Timer(taskName);
            timer.Start();
            return timer;
        }

        public static void StopAndShowTaskTime(Timer timer)
        {
            timer.Stop();
            Console.WriteLine($"{timer.Name} time: {timer.Elapsed.TotalSeconds:F3} sec");
        }

        public static void ShowMeshParameters(Mesh mesh)
        {
            Console.WriteLine($"Number of vertices: {mesh.Vertices.Count}");
            Console.WriteLine($"Number of finite elements: {mesh.Triangles.Count}");
        }

        public static IFiniteElementSpace CreateFiniteElementSpace(string elementType, Mesh mesh)
        {
            switch (elementType)
            {
                case "p1":
                    return new P1Space(mesh);
                case "p1b":
                    return new P1bSpace(mesh);
                default:
                    throw new ArgumentException("Unknown or unimplemented finite element type.");
            }
        }
    }
}