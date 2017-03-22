using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P1MeshFromFile : IMesh
    {
        private List<Vertex> vertices;
        public IReadOnlyCollection<Vertex> Vertices => vertices.AsReadOnly();
        public IReadOnlyCollection<IFiniteElement> FiniteElements { get; }

        public P1MeshFromFile(string path)
        {
            using (var reader = new StreamReader(path))
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
                    int reference = int.Parse(items[2]);
                    // TODO: Format exception.

                    var position = new Vector2(x, y);
                    vertices[i] = new Vertex(position, reference);
                }
                this.vertices = vertices.ToList();
                // TODO: Prevent copying by using own implementation of IReadOnlyCollection.

                do
                    rawString = reader.ReadLine();
                while (rawString != "Triangles");

                rawString = reader.ReadLine();
                int feCount = int.Parse(rawString);

                var fes = new P1Element[feCount];
                for (int i = 0; i < feCount; i++)
                {
                    rawString = reader.ReadLine();
                    if (rawString == "")
                    {
                        i--;
                        continue;
                    }
                    fes[i] = ReadFiniteElement(rawString);
                }
                FiniteElements = new ReadOnlyCollection<P1Element>(fes);
            }
        }

        private P1Element ReadFiniteElement(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1;

            return new P1Element(vertices[i], vertices[j], vertices[k]);
        }
    }
}
