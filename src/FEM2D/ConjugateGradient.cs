using FEM_NET.FEM2D;
using System;

namespace FEM_NET.FEM2D
{
    internal static class ConjugateGradient
    {
        public static Vector Solve(Matrix A, Vector F, double epsilon)
        {
            int n = F.Length;

            var u = new Vector(new double[n]);
            var r = A * u - F;
            var p = 1 * r;
            // For deep copying.
            
            double oldRho,
                rho = Vector.Dot(r, r);

            for (int i = 0; i < n; i++)
            {
                var s = A * p;
                var alpha = rho / Vector.Dot(s, p);
                u -= alpha * p;
                r -= alpha * s;
                oldRho = rho;
                rho = Vector.Dot(r, r);

                if (rho < epsilon)
                {
                    i++;
                    Console.WriteLine($"CG error = {rho}");
                    return u;
                }

                p = r + (rho / oldRho) * p;
            }
            throw new ArgumentException($"Diverged: Error = {rho}");
        }
    }
}
