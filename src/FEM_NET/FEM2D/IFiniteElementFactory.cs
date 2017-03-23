using System;

namespace FEM_NET.FEM2D
{
    internal interface IFiniteElementFactory
    {
        IFiniteElement Create(Triangle triangle);
    }
}
