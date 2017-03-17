using System;

namespace FEMSharp.FEM2D
{/*
    class FiniteElementFunction : IFunction<Vector2, double>
    {
        private readonly Mesh mesh;

        public Vector Coefficients { get; }

        public FiniteElementFunction(Mesh mesh, double[] coefficients)
        {
            this.mesh = mesh;
            Coefficients = new Vector(coefficients);
        }

        public FiniteElementFunction(Mesh mesh, Vector coefficients)
        {
            this.mesh = mesh;
            Coefficients = coefficients;
            // Deep enough?
        }

        public double GetValueAt(Vector2 point)
        {
            bool pointIsInsideTheMesh = mesh.Contains(point);
            if (!pointIsInsideTheMesh)
                return 0;

            foreach (var finiteElement in mesh.FiniteElements)
                if (finiteElement.Contains(point))
                    return finiteElement.GetValueOfFunctionAtPoint(Coefficients, point);

            throw new Exception("Programming error.");
        }
    }*/
}
