using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    public interface IFiniteElementSpace
    {
        IReadOnlyCollection<Vertex> Vertices { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }

    internal delegate IFiniteElementSpace FiniteElementSpaceFactory(Mesh mesh);
}