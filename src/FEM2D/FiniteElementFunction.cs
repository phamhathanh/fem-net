using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementFunction : IFiniteElementFunction
    {
        private readonly Dictionary<Vertex, int> indexByVertex;
        private readonly IMesh mesh;
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

            this.mesh = mesh;
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

        public double GetValueAt(Vector2 point)
        {
            foreach (var finiteElement in mesh.FiniteElements)
                if (finiteElement.Contains(point))
                {
                    double value = 0;
                    foreach (var node in finiteElement.Nodes)
                        value += node.Phi(point) * values[indexByVertex[node.Vertex]];
                    return value;
                }
            throw new ArgumentException("Point is not within the mesh.");
        }
    }
}
