using System;

namespace _2DFEM
{
    static class Input
    {
        public static readonly double e = 1e-12,
            a = 0, b = 1, c = 0, d = 1,
            a0 = 1;

        public static readonly int n = 63, m = 63;

        public static double F(Vector2 v)
        {
            double x = v.x,
                   y = v.y;

            return -4/((x+y+1)*(x+y+1)*(x+y+1))+2*Math.PI*Math.PI*Math.Sin(Math.PI*x)*Math.Sin(Math.PI*y)+a0*U(v);
            return -12*x*x + a0*U(v);
            return -2*(x*x + y*y - x - y) + a0*U(v);
        }

        public static double G(Vector2 v)
        {
            return U(v);
        }

        public static double U(Vector2 v)
        {
            double x = v.x,
                   y = v.y;

            return 1 / (x + y + 1) + Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y) + 5;
            return x*x*x*x;
            return x*(x - 1)*y*(y - 1) + 1;
        }
    }
}
