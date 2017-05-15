using System;
using FEM_NET.FEM2D;

internal sealed class BasisFunction
{
    public Func<Vector2, double> Function { get; }
    public Func<Vector2, Vector2> Gradient { get; }

    public BasisFunction(Func<Vector2, double> function, Func<Vector2, Vector2> gradient)
    {
        Function = function;
        Gradient = gradient;
    }
}