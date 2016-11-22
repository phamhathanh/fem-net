using System;

namespace FEMSharp
{
    class Vector
    {
        private readonly double[] items;
        private double? norm;

        public int Length { get; }
        public double Norm => GetNorm();

        public double this[int i] => items[i];

        public Vector(double[] doubleArray)
        {
            this.Length = doubleArray.Length;
            this.items = new double[Length];
            for (int i = 0; i < Length; i++)
                items[i] = doubleArray[i];

            this.norm = null;
        }

        private double GetNorm()
        {
            if (!norm.HasValue)
            {
                double squareNorm = 0;
                foreach (var item in items)
                    squareNorm += item * item;
                norm =  Math.Sqrt(squareNorm);
            }
            return norm.Value;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            int length = v1.Length;
            if (v2.Length != length)
                throw new ArgumentException("Vector size must match.");

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = v1[i] + v2[i];

            return new Vector(output);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            int length = v1.Length;
            if (v2.Length != length)
                throw new ArgumentException("Vector size must match.");

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = v1[i] - v2[i];

            return new Vector(output);
        }

        public static Vector operator *(double d, Vector v)
        {
            int length = v.Length;

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = d * v[i];

            return new Vector(output);
        }

        public static double Dot(Vector v1, Vector v2)
        {
            int length = v1.Length;
            if (v2.Length != length)
                throw new ArgumentException("Vector size must match.");

            double output = 0;
            for (int i = 0; i < length; i++)
                output += v1[i] * v2[i];

            return output;
        }
    }
}
