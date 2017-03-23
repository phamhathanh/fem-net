using System;
using System.Collections.Generic;
using System.IO;

namespace FEM_NET.FEM2D
{
    internal static class InOut
    {
        public static IMesh ReadMesh(string path, IFiniteElementFactory factory)
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
                    int reference = int.Parse(items[2]);
                    // TODO: Format exception.

                    var position = new Vector2(x, y);
                    vertices[i] = new Vertex(position, reference);
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
                return new Mesh(vertices, triangles, factory);
            }
        }

        public static Dictionary<int, Func<Vector2, double>> ReadBoundaryConditions(string path)
        {
            Dictionary<int, Func<Vector2, double>> conditions;
            using (var reader = File.OpenText(path))
            {
                string rawString;
                do
                    rawString = reader.ReadLine();
                while (rawString != "dirichlet");

                if (reader.EndOfStream)
                    throw new NotImplementedException();
                // TODO: other types of condition.

                rawString = reader.ReadLine();
                int conditionCount = int.Parse(rawString);
                conditions = new Dictionary<int, Func<Vector2, double>>(conditionCount);
                for (int i = 0; i < conditionCount; i++)
                {
                    rawString = reader.ReadLine();
                    var words = rawString.Split(' ');   // TODO: Use separator.
                    int reference = int.Parse(words[0]);
                    double value = double.Parse(words[3]);
                    // TODO: Parse function instead of constant.

                    conditions.Add(reference, v => value);
                }
            }
            return conditions;
        }

        public static void WriteSolutionToFile(string path, IMesh mesh, Vector solution)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine(
$@"MeshVersionFormatted 1

Dimension 2

SolAtVertices
{mesh.Vertices.Count}
2 2 1
");
                int i = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    writer.WriteLine($"{solution[i]} {vertex.Position.x} {vertex.Position.y}");
                    i++;
                }

                writer.WriteLine(
$@"
SolAtTriangles
0
1 2

End");
            }
        }
    }
}
