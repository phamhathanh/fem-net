using System.Collections.ObjectModel;

namespace FEM_NET.FEM3D
{
    internal interface IMesh
    {
        ReadOnlyCollection<Node> Nodes { get; }
        ReadOnlyCollection<Node> InteriorNodes { get; }
        ReadOnlyCollection<Node> BoundaryNodes { get; }
        ReadOnlyCollection<FiniteElement> FiniteElements { get; }
    }
}