using System;
using System.Collections.ObjectModel;

namespace FEMSharp.FEM2D
{
    internal class Triangle
    {
        private double? area;
        public Vertex Vertex0 { get; }
        public Vertex Vertex1 { get; }
        public Vertex Vertex2 { get; }

        public double Area
        {
            get
            {
                if (!area.HasValue)
                {
                    Vector2 u = Vertex1.Position - Vertex0.Position,
                            v = Vertex2.Position - Vertex0.Position;
                    area = Math.Abs(u.x * v.y - u.y * v.x) / 2;
                }
                return area.Value;
            }
        }

        public Triangle(Vertex vertex0, Vertex vertex1, Vertex vertex2)
        {
            Vertex0 = vertex0;
            Vertex1 = vertex1;
            Vertex2 = vertex2;
        }
    }
}
