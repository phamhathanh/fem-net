using FEM_NET.FEM2D;
using System;

namespace FEM_NET.FEM2D
{
    static class Calculator
    {
        public struct CGResult
        {
            public readonly Vector vector;
            public readonly int iterations;
            public readonly double error;

            public CGResult(Vector vector, int iterations, double error)
            {
                this.vector = vector;
                this.iterations = iterations;
                this.error = error;
            }
        }

        public static CGResult Solve(Matrix A, Vector F, double epsilon)
        {
            int M = F.Length;

            Vector r, u, p, s;
            double alpha, rho0, oldRho, newRho;

            u = new Vector(new double[M]);
            r = A * u - F;
            p = 1 * r;
            newRho = rho0 = Vector.Dot(r, r);

            int iterations = 0;
            for (int i = 0; i < M; i++)
            {
                s = A * p;
                alpha = newRho / Vector.Dot(s, p);
                u = u - alpha * p;
                r = r - alpha * s;
                oldRho = newRho;
                newRho = Vector.Dot(r, r);

                if (newRho < epsilon)
                {
                    iterations = i + 1;
                    break;
                }

                p = r + (newRho / oldRho) * p;
            }
            double error = (A * u - F).Norm;
            return new CGResult(u, iterations, error);
        }

        public static double Integrate(Func<Vector2, double> function, Triangle triangle)
        {
            // Gaussian quadrature coefficents
            var w = new[] { 0.225,
                            0.125939180544827152595683945500181333657639231912257007644510,
                            0.125939180544827152595683945500181333657639231912257007644510,
                            0.125939180544827152595683945500181333657639231912257007644510,
                            0.132394152788506180737649387833151999675694101421076325688822,
                            0.132394152788506180737649387833151999675694101421076325688822,
                            0.132394152788506180737649387833151999675694101421076325688822 };

            var t = new[] { new Vector2(0.333333333333333333333333333333333333333333333333333333333333,
                                        0.333333333333333333333333333333333333333333333333333333333333),
                            new Vector2(0.101286507323456338800987361915123828055575156890876627305353,
                                        0.101286507323456338800987361915123828055575156890876627305353),
                            new Vector2(0.797426985353087322398025276169752343888849686218246745389292,
                                        0.101286507323456338800987361915123828055575156890876627305353),
                            new Vector2(0.101286507323456338800987361915123828055575156890876627305353,
                                        0.797426985353087322398025276169752343888849686218246745389292),
                            new Vector2(0.470142064105115089770441209513447600515853414537694801266074,
                                        0.470142064105115089770441209513447600515853414537694801266074),
                            new Vector2(0.470142064105115089770441209513447600515853414537694801266074,
                                        0.059715871789769820459117580973104798968293170924610397467850),
                            new Vector2(0.059715871789769820459117580973104798968293170924610397467850,
                                        0.470142064105115089770441209513447600515853414537694801266074) };

            double sum = 0;
            Vector2 x0 = triangle.Vertex0.Position,
                    u = triangle.Vertex1.Position - x0,
                    v = triangle.Vertex2.Position - x0;

            for (int i = 0; i < 7; i++)
            {
                var baricentricPoint = x0 + t[i].x * u + t[i].y * v;
                sum += w[i] * function(baricentricPoint);
            }

            var area = Math.Abs(u.x*v.y - u.y*v.x) / 2;
            return sum * area;
        }
    }
}
