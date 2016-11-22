using System.Collections.ObjectModel;

namespace _2DFEM
{
    internal interface IMesh
    {
        ReadOnlyCollection<Node> Nodes { get; }
        ReadOnlyCollection<Node> InteriorNodes { get; }
        ReadOnlyCollection<Node> BoundaryNodes { get; }
        ReadOnlyCollection<FiniteElement> FiniteElements { get; }
    }
}