using System;

namespace FEM_NET.FEM2D
{
    internal interface IFunction
    {
        double GetValueAt(Vector2 point);
    }
}
