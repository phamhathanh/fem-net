using System;

namespace FEMSharp.FEM2D
{
    internal interface IFiniteElementFactory
    {
        IFiniteElement Create(Triangle triangle);
    }
}
