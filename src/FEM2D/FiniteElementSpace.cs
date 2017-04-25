using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementSpace : IFiniteElementSpace
    {
        public IMesh Mesh { get; }
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public FiniteElementSpace(IMesh mesh, IFiniteElementFactory factory)
        {
            Mesh = mesh;
            var finiteElements = new HashSet<IFiniteElement>();
            foreach (var triangle in Mesh.Triangles)
            {
                var finiteElement = factory(triangle);
                finiteElements.Add(finiteElement);
            }
            FiniteElements = finiteElements;
        }
    }
}
