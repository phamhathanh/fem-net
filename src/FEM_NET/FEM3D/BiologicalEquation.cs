using System;
using System.IO;

namespace FEMSharp.FEM3D
{
    class BiologicalEquation
    {
        private const double nnu = 1, zet = 1, c1 = 1, c2 = 1, dt = 0.0416667,
                    //k = 8, Kb = 0.00014, mmu = 0.2, r = 0.3,
                    k = 9, Kb = 0.0005, mmu = 0.22, r = 0.45,
                    Km = Kb;
        private const string name = "6L";//"7R";
        // TODO: Take from args.

        private readonly IMesh mesh;

        private Matrix A;
        private double T = 0;

        private Func<Vector3, double>[] UE0 = new Func<Vector3, double>[]
        {
            v => 1,
            v => 300,
            v => 0,
            v => 0,
            v => 0,
            v => 0
        };

        public BiologicalEquation(IMesh mesh)
        {
            this.mesh = mesh;
        }

        public void SolveAndOutput()
        {
            int n = mesh.Nodes.Count;
            A = new Matrix(n*6, n*6);

            var ue = new Vector[6];
            for (int i = 0; i < 6; i++)
            {
                var uei = new double[n];
                foreach (var finiteElement in mesh.FiniteElements)
                    foreach (var node in finiteElement.Nodes)
                        uei[node.Index] += UE0[i](node.Position);
                ue[i] = new Vector(uei);
            }

            Console.Write("Calculating matrix... ");

            foreach (var finiteElement in mesh.FiniteElements)
                for (int i = 0; i < 6; i++)
                    foreach (var node in finiteElement.Nodes)
                    {
                        int I = n*i + node.Index;
                        foreach (var otherNode in finiteElement.Nodes)
                        {
                            int J = n*i + otherNode.Index;
                            A[I, J] += Calculator.Integrate(v => node.Phi(v) * otherNode.Phi(v), finiteElement);
                        }
                    }

            Console.WriteLine("Done!");

            Console.WriteLine("Solving... ");

            using (var mbFile = File.CreateText($"MB_{name}.dat"))
            using (var domFile = File.CreateText($"DOM_{name}.dat"))
            using (var somFile = File.CreateText($"SOM_{name}.dat"))
            using (var fomFile = File.CreateText($"FOM_{name}.dat"))
            using (var enzFile = File.CreateText($"ENZ_{name}.dat"))
            using (var co2File = File.CreateText($"CO2_{name}.dat"))
            {
                for (int i = 0; i < 168; i++)
                {
                    Console.Write($"Iteration {i+1}... ");

                    var mb = Integrate(ue[0]);
                    mbFile.WriteLine($"{T:F3} {mb}");
                    var dom = Integrate(ue[1]);
                    domFile.WriteLine($"{T:F3} {dom}");
                    var som = Integrate(ue[2]);
                    somFile.WriteLine($"{T:F3} {som}");
                    var fom = Integrate(ue[3]);
                    fomFile.WriteLine($"{T:F3} {fom}");
                    var enz = Integrate(ue[4]);
                    enzFile.WriteLine($"{T:F3} {enz}");
                    var co2 = Integrate(ue[5]);
                    co2File.WriteLine($"{T:F3} {co2}");

                    ue = Solve(ue);

                    T += dt;

                    Console.WriteLine("Done!");
                }
            }
        }

        private Vector[] Solve(Vector[] ue)
        {
            var f = CalculateF(ue);

            int n = mesh.Nodes.Count;
            var rhs0 = new double[n*6];
            foreach (var finiteElement in mesh.FiniteElements)
                for (int i = 0; i < 6; i++)
                    foreach (var node in finiteElement.Nodes)
                        rhs0[n*i + node.Index] += Calculator.Integrate(v => (finiteElement.GetValueOfFunctionAtPoint(ue[i], v) + dt * finiteElement.GetValueOfFunctionAtPoint(f[i], v)) * node.Phi(v), finiteElement);
            var rhs = new Vector(rhs0);

            var epsilon = 1e-12;
            var result = Calculator.Solve(A, rhs, epsilon).vector;
            var output = new Vector[6];
            for (int i = 0; i < 6; i++)
            {
                var ui = new double[n];
                for (int j = 0; j < n; j++)
                    ui[j] = result[i*n + j];
                output[i] = new Vector(ui);
            }
            return output;
        }

        private Vector[] CalculateF(Vector[] ue)
        {
            int n = mesh.Nodes.Count;
            var output = new double[6][];
            for (int i = 0; i < 6; i++)
                output[i] = new double[n];

            for (int i = 0; i < n; i++)
            {
                output[0][i] = k * ue[0][i] * ue[1][i] / (Kb + ue[0][i]) - (mmu + r + nnu) * ue[0][i];
                output[1][i] = k * ue[4][i] * (c1 * ue[2][i] + c2 * ue[3][i]) / (Km + ue[4][i]) - k * ue[0][i] * ue[1][i] / (Kb + ue[0][i])
                    + 0.5 * (zet * ue[4][i] + mmu * ue[0][i]);
                output[2][i] = -c1 * ue[2][i] * ue[4][i] / (Km + ue[4][i]) + (1 - 0.5 * zet) * ue[4][i] + (1 - 0.5 * mmu) * ue[0][i];
                output[3][i] = -c2 * ue[3][i] * ue[4][i] / (Km + ue[4][i]);
                output[4][i] = nnu * ue[0][i] - zet * ue[4][i];
                output[5][i] = r * ue[0][i];
            }

            var vectorArray = new Vector[6];
            for (int i = 0; i < 6; i++)
                vectorArray[i] = new Vector(output[i]);
            return vectorArray;
        }

        // Should belong to IMesh or other static class.
        private double Integrate(Vector function)
        {
            double output = 0;
            foreach (var finiteElement in mesh.FiniteElements)
                output += Calculator.Integrate(v => finiteElement.GetValueOfFunctionAtPoint(function, v), finiteElement);
            return output;
        }
    }
}
