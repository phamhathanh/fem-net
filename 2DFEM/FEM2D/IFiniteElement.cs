using System;
using System.Collections.ObjectModel;

namespace FEMSharp.FEM2D
{
    internal interface IFiniteElement
    {
        ReadOnlyCollection<IFENode> Nodes { get; }

        bool Contains(Vector2 point);
    }

    internal interface IFENode
    {
        Vector2 Position { get; }
        int Index { get; }
        bool IsInside { get; }

        Func<Vector2, double> Phi { get; }
        Func<Vector2, Vector2> GradPhi { get; }
    }
}
