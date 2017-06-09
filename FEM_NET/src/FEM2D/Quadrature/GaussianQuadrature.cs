using FEM_NET.FEM2D;
using System;

namespace FEM_NET.FEM2D
{
    public class GaussianQuadrature : IQuadrature
    {
        // Pre-calculated Gaussian quadrature coefficents.
        private const double w0 = 0.225,
            w1 = 0.125939180544827152595683945500181333657639231912257007644510,
            w2 = 0.132394152788506180737649387833151999675694101421076325688822,
            p0 = 0.333333333333333333333333333333333333333333333333333333333333,
            p1 = 0.101286507323456338800987361915123828055575156890876627305353,
            p2 = 0.797426985353087322398025276169752343888849686218246745389292,
            p3 = 0.470142064105115089770441209513447600515853414537694801266074,
            p4 = 0.059715871789769820459117580973104798968293170924610397467850;
        
        private readonly double[] weights = new[] { w0, w1, w1, w1, w2, w2, w2 };
        private readonly Vector2[] points = new[] { new Vector2(p0, p0),
            new Vector2(p1, p1), new Vector2(p2, p1), new Vector2(p1, p2),
            new Vector2(p3, p3), new Vector2(p3, p4), new Vector2(p4, p3) };

        public double Integrate(Func<Vector2, double> function, Triangle triangle)
        {
            double sum = 0;
            Vector2 x0 = triangle.Vertex0.Position,
                    u = triangle.Vertex1.Position - x0,
                    v = triangle.Vertex2.Position - x0;

            for (int i = 0; i < 7; i++)
            {
                var baricentricPoint = x0 + points[i].x * u + points[i].y * v;
                sum += weights[i] * function(baricentricPoint);
            }
            return sum * triangle.Area;
        }
    }
}
