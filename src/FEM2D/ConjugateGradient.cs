using FEM_NET.FEM2D;
using System;

namespace FEM_NET.FEM2D
{
    internal static class ConjugateGradient
    {
        public struct Result
        {
            public readonly Vector vector;
            public readonly int iterations;
            public readonly double error;

            public Result(Vector vector, int iterations, double error)
            {
                this.vector = vector;
                this.iterations = iterations;
                this.error = error;
            }
        }

        public static Result Solve(Matrix A, Vector F, double epsilon)
        {
            int M = F.Length;

            Vector r, u, p, s;
            double alpha, rho0, oldRho, newRho;

            u = new Vector(new double[M]);
            r = A * u - F;
            p = 1 * r;
            newRho = rho0 = Vector.Dot(r, r);

            int i = 0;
            for (; i < M; i++)
            {
                s = A * p;
                alpha = newRho / Vector.Dot(s, p);
                u = u - alpha * p;
                r = r - alpha * s;
                oldRho = newRho;
                newRho = Vector.Dot(r, r);

                if (newRho < epsilon)
                {
                    i++;
                    break;
                }

                p = r + (newRho / oldRho) * p;
            }
            double error = (A * u - F).Norm;
            return new Result(u, i, error);
        }
    }
}
