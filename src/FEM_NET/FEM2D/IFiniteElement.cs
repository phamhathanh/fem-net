using System;
using System.Collections.ObjectModel;

namespace FEMSharp.FEM2D
{
    internal interface IFiniteElement
    {
        ReadOnlyCollection<INode> Nodes { get; }

        bool Contains(Vector2 point);
    }

    internal interface INode
    {
        Vertex Vertex { get; }
        Func<Vector2, double> Phi { get; }
        Func<Vector2, Vector2> GradPhi { get; }
    }
}
