using System;

namespace _2DFEM
{
    struct Vector2
    {
        public readonly double x, y;

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator *(double d, Vector2 v)
        {
            return new Vector2(d * v.x, d * v.y);
        }

        public static double Dot(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

        public static Vector2 Normalize(Vector2 v)
        {
            double n = Math.Sqrt(v.x * v.x + v.y * v.y);
            return new Vector2(v.x / n, v.y / n);
        }

        public override string ToString()
        {
            return "(" + x.ToString() + " ," + y.ToString() + ")";
        }
    }
}
