using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FEMSharp.FEM2D
{
    internal interface IMesh
    {
        IReadOnlyCollection<Node> Nodes { get; }
        IReadOnlyCollection<Node> InteriorNodes { get; }
        IReadOnlyCollection<Node> BoundaryNodes { get; }
        IReadOnlyCollection<IFiniteElement> FiniteElements { get; }
    }
}