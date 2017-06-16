using FEM_NET.FEM2D;
using System;

namespace FEM_NET.FEM2D
{
    public class ConjugateGradient : ISolver
    {
        private readonly double epsilon;

        public ConjugateGradient(double accuracy)
        {
            epsilon = accuracy*accuracy;
        }

        public Vector Solve(Matrix matrix, Vector rightHandSide)
        {
            int n = rightHandSide.Length;

            var u = new Vector(new double[n]);
            var r = matrix * u - rightHandSide;
            var p = 1 * r;
            // For deep copying.
            
            double oldRho,
                rho = r.Norm*r.Norm;

            for (int i = 0; i < n; i++)
            {
                var s = matrix * p;
                var alpha = rho / Vector.Dot(s, p);
                u -= alpha * p;
                r -= alpha * s;
                oldRho = rho;
                rho = r.Norm*r.Norm;

                if (rho < epsilon)
                {
                    i++;
                    Console.WriteLine($"CG error = {Math.Sqrt(rho)}");
                    return u;
                }
                p = r + (rho/oldRho) * p;
            }
            throw new ArgumentException($"Diverged: Error = {Math.Sqrt(rho)}");
        }
    }
}
