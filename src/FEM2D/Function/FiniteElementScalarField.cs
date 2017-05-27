using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementScalarField : IScalarField
    {
        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Dictionary<Vertex, double> valueByVertex;

        public FiniteElementScalarField(IFiniteElementSpace finiteElementSpace, IEnumerable<double> values)
        {
            this.finiteElementSpace = finiteElementSpace;

            int n = finiteElementSpace.Vertices.Count;
            valueByVertex = new Dictionary<Vertex, double>(n);
            
            int valueCount = 0;
            var valueEnumerator = values.GetEnumerator();
            foreach (var vertex in finiteElementSpace.Vertices)
            {
                valueEnumerator.MoveNext();
                var value = valueEnumerator.Current;
                valueByVertex.Add(vertex, value);
                valueCount++;
            }
            if (valueCount != n)
                throw new ArgumentException($"The number of values ({valueCount}) does not match the number of vertices ({n}).");
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
    }
}
