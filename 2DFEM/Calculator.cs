using FEMSharp.FEM2D;
using FEMSharp.FEM3D;
using System;

namespace FEMSharp
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

                if (newRho < epsilon * rho0)
                {
                    iterations = i + 1;
                    break;
                }

                p = r + (newRho / oldRho) * p;
            }
            double error = (A * u - F).Norm;

            return new CGResult(u, iterations, error);
        }

        public static double Integrate(Func<Vector2, double> function, FEM2D.FiniteElement finiteElement)
        // TODO: Use finite element function.
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
            Vector2 x0 = finiteElement.Nodes[0].Position,
                    u = finiteElement.Nodes[1].Position - x0,
                    v = finiteElement.Nodes[2].Position - x0;

            for (int i = 0; i < 7; i++)
            {
                var baricentricPoint = x0 + t[i].x * u + t[i].y * v;
                sum += w[i] * function(baricentricPoint);
            }

            var area = Math.Abs(u.x*v.y - u.y*v.x) / 2;
            return sum * area;
        }

        public static double Integrate(Func<Vector3, double> function, FEM3D.FiniteElement finiteElement)
        // TODO: Use finite element function.
        // TODO: Abstraction.
        {
            double w0 = -74.0 / 5625,
                    w1 = 343.0 / 45000,
                    w2 = 56.0 / 2250;
            var weights = new[] { w0, w1, w1, w1, w1, w2, w2, w2, w2, w2, w2 };

            double c = 11.0 / 14,
                d = 1.0 / 14,
                a = 0.3994035761667992,
                b = 0.1005964238332008;
            var points = new[] { new Vector3(0.25, 0.25, 0.25),
                                new Vector3(c, d, d),
                                new Vector3(d, c, d),
                                new Vector3(d, d, c),
                                new Vector3(d, d, d),
                                new Vector3(a, a, b),
                                new Vector3(a, b, a),
                                new Vector3(a, b, b),
                                new Vector3(b, a, a),
                                new Vector3(b, a, b),
                                new Vector3(b, b, a) };

            double sum = 0;
            Vector3 x0 = finiteElement.Nodes[0].Position,
                    u = finiteElement.Nodes[1].Position - x0,
                    v = finiteElement.Nodes[2].Position - x0,
                    w = finiteElement.Nodes[3].Position - x0;

            for (int i = 0; i < 11; i++)
            {
                var baricentricPoint = x0 + points[i].x*u + points[i].y*v + points[i].z*w;
                sum += weights[i] * function(baricentricPoint);
            }

            var volumn = Math.Abs(u.x*v.y*w.z + u.y*v.z*w.z + u.z*v.x*w.y - u.z*v.y*w.x - u.y*v.x*w.z - u.x*v.z*w.y) / 6;
            return sum * volumn * 6;
        }
    }
}
