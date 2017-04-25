using System;
using System.Collections.Generic;

namespace FEM_NET.FEM2D
{
    internal class FiniteElementFunction : IFiniteElementFunction
    {
        private readonly IFiniteElementSpace finiteElementSpace;
        private readonly Vector values;

        public FiniteElementFunction(IFiniteElementSpace finiteElementSpace, Vector values)
        {
            this.finiteElementSpace = finiteElementSpace;
            this.values = values;
        }

        public double GetValueAt(Vertex vertex)
        {
            try
            {
                return values[vertex.Index];
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
                        value += node.Phi(point) * values[node.Vertex.Index];
                    return value;
                }
            throw new ArgumentException("Point is not within the mesh.");
        }
    }
}
