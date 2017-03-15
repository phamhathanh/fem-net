using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    class LaplaceEquation
    {
        private readonly double a0;
        private readonly Func<Vector2, double> f, g, u;

        private readonly Dictionary<string, Stopwatch> taskTimers = new Dictionary<string, Stopwatch>();

        private IMesh mesh;
        private int interiorNodesCount;
        private Matrix A, Ag;

        private Vector boundary, rhs, result;
        // TODO: Change to a FE function.

        // -Laplace(u) + a0 * u = f
        // u = g on boundary
        public LaplaceEquation(double a0, Func<Vector2, double> f, Func<Vector2, double> g, Func<Vector2, double> u)
        {
            this.a0 = a0;
            this.f = f;
            this.g = g;
            this.u = u;
        }

        public void SolveAndDisplay()
        {
            Console.WriteLine("FEM for solving equation: -Laplace(u) + a0 * u = F");
            StartMeasuringTaskTime("Total");

            Initialize();
            CalculateMatrixAndRHS();
            SolveMatrix();
            Output();

            StopAndShowTaskTime("Total");
        }

        private void Initialize()
        {
            StartMeasuringTaskTime("Initialization");

            mesh = new MeshFromFile("square.mesh");
            interiorNodesCount = mesh.InteriorNodes.Count;
            var boundaryNodesCount = mesh.BoundaryNodes.Count;
            A = new Matrix(interiorNodesCount, interiorNodesCount);
            Ag = new Matrix(interiorNodesCount, boundaryNodesCount);

            var cg = new double[boundaryNodesCount];
            {
                int i = 0;
                foreach (var boundaryNode in mesh.BoundaryNodes)
                {
                    cg[i] = g(boundaryNode.Position);
                    i++;
                }
            }
            boundary = new Vector(cg);

            ShowMeshParameters(mesh);
            StopAndShowTaskTime("Initialization");
        }

        private void CalculateMatrixAndRHS()
        {
            StartMeasuringTaskTime("Matrix & RHS calculation");

            var rhs = new double[mesh.InteriorNodes.Count];
            foreach (var finiteElement in mesh.FiniteElements)
            {
                foreach (var node in finiteElement.Nodes)
                    if (node.IsInside)
                    {
                        int I = node.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (otherNode.IsInside)
                            {
                                int J = otherNode.Index;
                                A[I, J] += Calculator.Integrate(v => Vector2.Dot(node.GradPhi(v), otherNode.GradPhi(v)), finiteElement)
                                    + Calculator.Integrate(v => a0 * node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }

                        rhs[I] += Calculator.Integrate(v => f(v) * node.Phi(v), finiteElement);
                    }
                    else
                    {
                        int J = node.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                            if (otherNode.IsInside)
                            {
                                int I = otherNode.Index;
                                Ag[I, J] += Calculator.Integrate(v => Vector2.Dot(node.GradPhi(v), otherNode.GradPhi(v)), finiteElement)
                                    + Calculator.Integrate(v => a0 * node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }
                    }
            }

            this.rhs = new Vector(rhs);

            StopAndShowTaskTime("Matrix & RHS calculation");
        }

        private void SolveMatrix()
        {
            StartMeasuringTaskTime("Matrix solution");

            var epsilon = 1e-12;
            Calculator.CGResult result = Calculator.Solve(A, rhs - Ag * boundary, epsilon);
            this.result = result.vector;

            Console.WriteLine($"CG completed successfully: {result.iterations} iterations. Residual: {result.error:0.###e+00}");

            StopAndShowTaskTime("Matrix solution");
        }

        private void Output()
        {
            using (var solutionFile = new StreamWriter($"square.sol"))
            {
                solutionFile.WriteLine(
$@"MeshVersionFormatted 1

Dimension 2

SolAtVertices
{mesh.Nodes.Count}
2 2 1
");
                foreach (var node in mesh.Nodes)
                {
                    var valueAtNode = node.IsInside ? result[node.Index] : boundary[node.Index];
                    solutionFile.WriteLine($"{valueAtNode} {node.Position.x} {node.Position.y}");
                }

                solutionFile.WriteLine(
$@"
SolAtTriangles
0
1 2

End");
            }
        }

        private void StartMeasuringTaskTime(string taskName)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            taskTimers.Add(taskName, stopwatch);
        }

        private void StopAndShowTaskTime(string taskName)
        {
            var stopwatch = taskTimers[taskName];
            stopwatch.Stop();
            taskTimers.Remove(taskName);

            TimeSpan taskTime = stopwatch.Elapsed;
            Console.WriteLine($"{taskName} time: {taskTime.TotalSeconds:F3} sec");
        }

        private void ShowMeshParameters(IMesh mesh)
        {
            Console.WriteLine($"Number of interior vertices: {mesh.InteriorNodes.Count()}");
            Console.WriteLine($"Number of boundary vertices: {mesh.BoundaryNodes.Count()}");
            Console.WriteLine($"Number of finite elements: {mesh.FiniteElements.Count()}");
        }
    }
}
