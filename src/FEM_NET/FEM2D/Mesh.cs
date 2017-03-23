using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class Mesh : IMesh
    {
        public IReadOnlyCollection<Vertex> Vertices { get; }
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public Mesh(IEnumerable<Vertex> vertices, IEnumerable<Triangle> triangles, IFiniteElementFactory finiteElementFactory)
        {
            Vertices = new ReadOnlyCollection<Vertex>(vertices.ToArray());

            var finiteElements = new List<IFiniteElement>();
            foreach (var triangle in triangles)
            {
                var finiteElement = finiteElementFactory.Create(triangle);
                finiteElements.Add(finiteElement);
            }
            FiniteElements = finiteElements.AsReadOnly();
        }
    }
}
