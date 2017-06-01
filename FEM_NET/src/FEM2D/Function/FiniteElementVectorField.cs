using System;
using System.Linq;

namespace FEM_NET.FEM2D
{
    public class FiniteElementVectorField : IVectorField
    {
        private readonly FiniteElementScalarField[] components;

        public int Dimension => components.Length;

        internal FiniteElementVectorField(params FiniteElementScalarField[] components)
        // Public class internal constructor?
        // TODO: overload this
        // IFiniteElementSpace finiteElementSpace, IEnumerable<double> values
        {
            if (components.Length == 0)
                throw new ArgumentException("There must be at least one component.");
            this.components = components;
        }

        public double GetValueAt(Vector2 point, int component)
            => components[component].GetValueAt(point);

        public double GetValueAt(Vertex vertex, int component)
            => components[component].GetValueAt(vertex);
    }
}