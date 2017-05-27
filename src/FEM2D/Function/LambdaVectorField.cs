using System;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class LambdaVectorField : IVectorField
    {
        private readonly IScalarField[] components;

        public int Dimension => components.Length;

        public LambdaVectorField(params Func<Vector2, double>[] components)
        {
            if (components.Length == 0)
                throw new ArgumentException("There must be at least one component.");
            var scalarFields = from component in components
                               select new LambdaScalarField(component);
            this.components = scalarFields.ToArray();
        }

        public LambdaVectorField(Func<Vector2, double> function)
        {
            this.components = new[] { new LambdaScalarField(function) };
        }

        public LambdaVectorField(Func<double, double, double> function)
            : this(v => function(v.x, v.y))
        { }

        public double GetValueAt(Vector2 point, int component)
            => components[component].GetValueAt(point);
    }
}