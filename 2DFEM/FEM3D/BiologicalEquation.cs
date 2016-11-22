using System;
using System.IO;

namespace FEMSharp.FEM3D
{
    class BiologicalEquation
    {
        private readonly double k, Kb, Km, nnu, zet, c1, c2, dt, r, mmu;
        private readonly IMesh mesh;

        private Matrix A;
        private double T = 0;

        public BiologicalEquation(IMesh mesh)
        {
            this.mesh = mesh;

            k = 17;
            Kb = 0.0005;
            Km = 0.0005;
            r = 0.2;
            mmu = 1.5;
            nnu = 0;
            zet = 0;
            c1 = 0;
            c2 = 0;
            dt = 0.0416667;
            // TODO: Take from args.
        }

        private Vector[] CalculateF(Vector[] ue)
        {
            int n = mesh.Nodes.Count;
            var output = new double[6][];
            for (int i = 0; i < 6; i++)
                output[i] = new double[n];

            for (int i = 0; i < n; i++)
            {
                output[0][i] = k*ue[0][i]*ue[1][i]/(Kb + ue[0][i]) - (mmu + r + nnu)*ue[0][i];
                output[1][i] = k * ue[4][i] / (c1 * ue[2][i] + c2 * ue[3][i]) - k * ue[0][i] * ue[1][i] / (Kb + ue[0][i])
                    + 0.5 * (zet * ue[4][i] + mmu * ue[0][i]);
                output[2][i] = -c1*ue[2][i]*ue[4][i]/(Km + ue[4][i]) + 0.5*(zet*ue[4][i] + mmu*ue[0][i]);
                output[3][i] = -c2*ue[3][i]*ue[4][i]/(Km + ue[4][i]);
                output[4][i] = nnu*ue[0][i] - zet*ue[4][i];
                output[4][i] = r*ue[0][i];
            }

            var vectorArray = new Vector[6];
            for (int i = 0; i < 6; i++)
                vectorArray[i] = new Vector(output[i]);
            return vectorArray;
        }

        private Func<Vector3, double>[] UE0 = new Func<Vector3, double>[]
        {
            v => 1,
            v => 300 * Math.Cos(v.x*v.y) / Math.Cos(v.x*v.y),
            v => 0,
            v => 0,
            v => 0,
            v => 0
        };

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

            foreach (var finiteElement in mesh.FiniteElements)
                for (int i = 0; i < 6; i++)
                    foreach (var node in finiteElement.Nodes)
                    {
                        int I = n*i + node.Index;
                        for (int j = 0; j < 6; j++)
                            foreach (var otherNode in finiteElement.Nodes)
                            {
                                int J = n*j + otherNode.Index;
                                A[I, J] += Calculator.Integrate(v => node.Phi(v) * otherNode.Phi(v), finiteElement);
                            }
                    }

            string name = "6L";
            using (var mbFile = new StreamWriter($"./MB_{name}.dat"))
            using (var domFile = new StreamWriter($"./DOM_{name}.dat"))
            using (var somFile = new StreamWriter($"./SOM_{name}.dat"))
            using (var fomFile = new StreamWriter($"./FOM_{name}.dat"))
            using (var enzFile = new StreamWriter($"./ENZ_{name}.dat"))
            using (var co2File = new StreamWriter($"./CO2_{name}.dat"))
            {
                for (int i = 1; i <= 168; i++)
                {
                    var mb = Integrate(ue[0]);
                    mbFile.WriteLine($"{T:F3} {mb}");
                    var dom = Integrate(ue[1]);
                    domFile.WriteLine($"{T:F3} {dom}");
                    var som = Integrate(ue[0]);
                    somFile.WriteLine($"{T:F3} {som}");
                    var fom = Integrate(ue[0]);
                    fomFile.WriteLine($"{T:F3} {fom}");
                    var enz = Integrate(ue[0]);
                    enzFile.WriteLine($"{T:F3} {enz}");
                    var co2 = Integrate(ue[0]);
                    co2File.WriteLine($"{T:F3} {co2}");

                    ue = Solve(ue);

                    T += dt;
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
                        rhs0[n*i + node.Index] += Calculator.Integrate(v => (finiteElement.GetValueOfFunctionAtPoint(ue[i], v)
                                        + dt * finiteElement.GetValueOfFunctionAtPoint(f[i], v)) * node.Phi(v), finiteElement);
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
