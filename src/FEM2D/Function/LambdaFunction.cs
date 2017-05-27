using System;

namespace FEM_NET.FEM2D
{
    internal class LambdaFunction : IFunction
    {
        private readonly Func<Vector2, double> function;

        public LambdaFunction(Func<Vector2, double> function)
        {
            this.function = function;
        }

        public LambdaFunction(Func<double, double, double> function)
        {
            this.function = v => function(v.x, v.y);
        }

        public double GetValueAt(Vector2 point)
            => function(point);
    }
}
