using System;

namespace FEM_NET.FEM2D
{
    internal interface IScalarField
    {
        double GetValueAt(Vector2 point);
    }
}
