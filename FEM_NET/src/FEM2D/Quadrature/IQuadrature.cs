using System;

namespace FEM_NET.FEM2D
{
    public interface IQuadrature
    {
        double Integrate(Func<Vector2, double> function, Triangle triangle);
    }
}