using System;

namespace FEMSharp.FEM2D
{
    public struct Vector2
    {
        public readonly double x, y;

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 vector1, Vector2 vector2)
            => new Vector2(vector1.x + vector2.x, vector1.y + vector2.y);

        public static Vector2 operator -(Vector2 vector1, Vector2 vector2)
            => new Vector2(vector1.x - vector2.x, vector1.y - vector2.y);

        public static Vector2 operator *(double scalar, Vector2 vector)
            => new Vector2(scalar * vector.x, scalar * vector.y);

        public static Vector2 operator /(Vector2 vector, double scalar)
            => new Vector2(vector.x / scalar, vector.y / scalar);

        public static double Dot(Vector2 vector1, Vector2 vector2)
            => vector1.x * vector2.x + vector1.y * vector2.y;

        public static Vector2 Normalize(Vector2 vector)
        {
            double length = Math.Sqrt(vector.x * vector.x + vector.y * vector.y);
            return (1 / length) * vector;
        }

        public override string ToString()
            => $"({x}, {y})";
    }
}
