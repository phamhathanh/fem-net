using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal static class InOut
    {
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

        public static void WriteSolutionToFile(string path, IMesh mesh, IFiniteElementFunction solution)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine(
$@"MeshVersionFormatted 1

Dimension 2

SolAtVertices
{mesh.Vertices.Count}
1 1
");
                var vertices = from vertex in mesh.Vertices
                               orderby vertex.Index
                               select vertex;
                foreach (var vertex in vertices)
                    writer.WriteLine($"{solution.GetValueAt(vertex)}");

                writer.WriteLine(
$@"

End");
            }
        }
    }
}
