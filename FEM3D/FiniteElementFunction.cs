using System;
using System.Linq;

namespace FEM_NET.FEM3D
{
    internal class FiniteElementFunction
    {
        private readonly IMesh mesh;

        public Vector Coefficients { get; }

        public FiniteElementFunction(IMesh mesh, double[] coefficients)
        {
            this.mesh = mesh;
            Coefficients = new Vector(coefficients);
        }

        public FiniteElementFunction(IMesh mesh, Vector coefficients)
        {
            this.mesh = mesh;
            Coefficients = coefficients;
            // Shallow.
        }

        public double GetValueAt(Vector3 point)
        {
            foreach (var finiteElement in mesh.FiniteElements)
                if (finiteElement.Contains(point))
                    return finiteElement.GetValueOfFunctionAtPoint(Coefficients, point);
            return 0;
        }
    }
}
