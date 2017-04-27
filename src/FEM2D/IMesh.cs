using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal interface IMesh
    {
        IReadOnlyCollection<Vertex> Vertices { get; }
        // mesh.Vertices order is important.
        // Implementation should preserve the order from file.
        IReadOnlyCollection<Triangle> Triangles { get; }
    }
}