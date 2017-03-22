using System.Collections.Generic;

namespace FEMSharp.FEM2D
{
    internal interface IMesh
    {
        IReadOnlyCollection<Node> Nodes { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }
}