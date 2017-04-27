using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementFunction : IFiniteElementFunction
    {
        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Dictionary<Vertex, double> valueByVertex;

        public FiniteElementFunction(IFiniteElementSpace finiteElementSpace, Vector values)
        {
            this.finiteElementSpace = finiteElementSpace;

            int n = finiteElementSpace.Vertices.Count;
            if (values.Length != n)
                throw new ArgumentException($"Vector size ({values.Length} does not match the number of vertices ({n}).)");
            
            valueByVertex = new Dictionary<Vertex, double>(n);
            int i = 0;
            foreach (var vertex in finiteElementSpace.Vertices)
            {
                valueByVertex.Add(vertex, values[i]);
                i++;
            }
        }

        public double GetValueAt(Vertex vertex)
        {
            try
            {
                return valueByVertex[vertex];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Vertex is not from the same mesh.");
            }
        }

        public double GetValueAt(Vector2 point)
        {
            foreach (var finiteElement in finiteElementSpace.FiniteElements)
                if (finiteElement.Contains(point))
                {
                    double value = 0;
                    foreach (var node in finiteElement.Nodes)
                        value += node.Phi(point) * valueByVertex[node.Vertex];
                    return value;
                }
            throw new ArgumentException("Point is not within the mesh.");
        }
    }
}
