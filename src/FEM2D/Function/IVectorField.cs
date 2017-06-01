using System;
using System.Linq;

namespace FEM_NET.FEM2D
{
    public interface IVectorField
    {
        int Dimension { get; }
        double GetValueAt(Vector2 point, int component);
    }
}