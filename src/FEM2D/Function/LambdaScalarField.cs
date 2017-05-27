using System;

namespace FEM_NET.FEM2D
{
    internal class LambdaScalarField : IScalarField
    {
        private readonly Func<Vector2, double> function;

        public LambdaScalarField(Func<Vector2, double> function)
        {
            this.function = function;
        }

        public LambdaScalarField(Func<double, double, double> function)
            : this(v => function(v.x, v.y))
        { }

        public double GetValueAt(Vector2 point)
            => function(point);
    }
}
