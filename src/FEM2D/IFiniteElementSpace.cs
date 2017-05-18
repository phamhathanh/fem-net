using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal interface IFiniteElementSpace
    {
        IReadOnlyCollection<Vertex> Vertices { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }

    internal delegate IFiniteElementSpace FiniteElementSpaceFactory(IMesh mesh);
}