using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementFunction
    {
        private readonly Dictionary<Vertex, int> indexByVertex;
        private readonly Vector values;

        public FiniteElementFunction(IMesh mesh, Vector values)
        {
            this.indexByVertex = new Dictionary<Vertex, int>(mesh.Vertices.Count);
            int i = 0;
            foreach (var vertex in mesh.Vertices)
            {
                indexByVertex.Add(vertex, i);
                i++;
            }

            this.values = values;
        }

        public double GetValueAt(Vertex vertex)
        {
            try
            {
                var index = indexByVertex[vertex];
                return values[index];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Vertex is not from the same mesh.");
            }
        }
    }
}
