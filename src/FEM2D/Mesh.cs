using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class Mesh : IMesh
    {
        public IReadOnlyCollection<Vertex> Vertices { get; }
        public IReadOnlyCollection<Triangle> Triangles { get; }

        public Mesh(IEnumerable<Triangle> triangles)
        {
            var vertices = new HashSet<Vertex>();
            var triangleSet = new HashSet<Triangle>();
            foreach (var triangle in triangles)
            {
                triangleSet.Add(triangle);
                vertices.Add(triangle.Vertex0);
                vertices.Add(triangle.Vertex1);
                vertices.Add(triangle.Vertex2);
            }
            Vertices = vertices;
            Triangles = triangleSet;
        }
    }
}
