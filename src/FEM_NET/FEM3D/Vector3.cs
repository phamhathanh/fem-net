using System;

namespace FEM_NET.FEM3D
{
    public struct Vector3
    {
        public readonly double x, y, z;

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator +(Vector3 vector1, Vector3 vector2)
            => new Vector3(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);

        public static Vector3 operator -(Vector3 vector1, Vector3 vector2)
            => new Vector3(vector1.x - vector2.x, vector1.y - vector2.y, vector1.z - vector2.z);

        public static Vector3 operator *(double scalar, Vector3 vector)
            => new Vector3(scalar * vector.x, scalar * vector.y, scalar * vector.z);

        public static double Dot(Vector3 vector1, Vector3 vector2)
            => vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;

        public static Vector3 Normalize(Vector3 vector)
        {
            double length = Math.Sqrt(vector.x * vector.x + vector.y * vector.y);
            return (1 / length) * vector;
        }

        public override string ToString()
            => $"({x}, {y}, {z})";
    }
}
