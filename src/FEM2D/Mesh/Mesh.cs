using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal sealed class Mesh
    {
        public IReadOnlyCollection<Vertex> Vertices { get; }
        public IReadOnlyCollection<Triangle> Triangles { get; }

        public static Mesh ReadFromFile(string path)
        {
            using (var reader = File.OpenText(path))
            {
                string rawString;
                do
                    rawString = reader.ReadLine();
                while (rawString != "Vertices");

                rawString = reader.ReadLine();
                int vertexCount = int.Parse(rawString);

                var vertices = new Vertex[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    rawString = reader.ReadLine();
                    if (rawString == "")
                    {
                        i--;
                        continue;
                    }

                    var items = rawString.Split(' ');
                    double x = double.Parse(items[0]),
                        y = double.Parse(items[1]);
                    int label = int.Parse(items[2]);
                    // TODO: Format exception.

                    var position = new Vector2(x, y);
                    vertices[i] = new Vertex(position, label);
                }

                do
                    rawString = reader.ReadLine();
                while (rawString != "Triangles");

                rawString = reader.ReadLine();
                int triangleCount = int.Parse(rawString);

                var triangles = new Triangle[triangleCount];
                for (int i = 0; i < triangleCount; i++)
                {
                    rawString = reader.ReadLine();
                    if (rawString == "")
                    {
                        i--;
                        continue;
                    }

                    var items = rawString.Split(' ');
                    int index0 = int.Parse(items[0]) - 1,
                        index1 = int.Parse(items[1]) - 1,
                        index2 = int.Parse(items[2]) - 1;
                    // TODO: Exception.

                    triangles[i] = new Triangle(vertices[index0], vertices[index1], vertices[index2]);
                }
                return new Mesh(vertices, triangles);
            }
        }

        private Mesh(Vertex[] vertices, Triangle[] triangles)
        {
            Vertices = new ReadOnlyCollection<Vertex>(vertices);
            Triangles = new ReadOnlyCollection<Triangle>(triangles);
        }
    }
}
