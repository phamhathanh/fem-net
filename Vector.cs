using System;

namespace _2DFEM
{
    struct Vector
    {
        public readonly int length;
        private readonly double[] items;

        public double Norm
        {
            get
            {
                double squareNorm = 0;
                for (int i = 0; i < length; i++)
                    squareNorm += this[i] * this[i];

                return Math.Sqrt(squareNorm);
            }
        }

        public double this[int i]
        {
            get
            {
                return items[i];
            }
        }

        public Vector(double[] doubleArray)
        {
            this.length = doubleArray.Length;

            this.items = new double[length];

            for (int i = 0; i < length; i++)
                items[i] = doubleArray[i];
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            int length = v1.length;
            if (v2.length != length)
                throw new ArgumentException("Vector size must match.");

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = v1[i] + v2[i];

            return new Vector(output);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            int length = v1.length;
            if (v2.length != length)
                throw new ArgumentException("Vector size must match.");

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = v1[i] - v2[i];

            return new Vector(output);
        }

        public static Vector operator *(double d, Vector v)
        {
            int length = v.length;

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = d * v[i];

            return new Vector(output);
        }

        public static double Dot(Vector v1, Vector v2)
        {
            int length = v1.length;
            if (v2.length != length)
                throw new ArgumentException("Vector size must match.");

            double output = 0;
            for (int i = 0; i < length; i++)
                output += v1[i] * v2[i];

            return output;
        }
    }
}
