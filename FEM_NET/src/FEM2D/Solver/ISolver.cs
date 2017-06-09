using System;

namespace FEM_NET.FEM2D
{
    public interface ISolver
    {
        Vector Solve(Matrix matrix, Vector rightHandSide);
    }
}