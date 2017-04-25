using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal interface IFiniteElementSpace
    {
        IMesh Mesh { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }
}