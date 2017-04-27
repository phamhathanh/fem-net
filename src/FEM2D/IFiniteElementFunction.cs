using System;

namespace FEM_NET.FEM2D
{
    internal interface IFiniteElementFunction
    {
        double GetValueAt(Vector2 point);
    }
}
