using System.Collections.Generic;

namespace FEMSharp.FEM2D
{
    internal interface IMesh
    {
        IReadOnlyCollection<Vertex> Vertices { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }
}