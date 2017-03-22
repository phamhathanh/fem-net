using System.Collections.ObjectModel;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal abstract class FiniteElement : IFiniteElement
    {
        public abstract ReadOnlyCollection<INode> Nodes { get; }

        public bool Contains(Vector2 point)
            => Nodes.All(node => node.Phi(point) >= 0);
    }
}
