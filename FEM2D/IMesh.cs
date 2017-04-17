using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal interface IMesh
    {
        IReadOnlyCollection<Vertex> Vertices { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }
}