using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FEM_NET.FEM2D
{
    internal static class InOut
    {
        public static Dictionary<int, IFiniteElementFunction[]> ReadBoundaryConditions(string path)
        {
            Dictionary<int, IFiniteElementFunction[]> conditions;
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
                conditions = new Dictionary<int, IFiniteElementFunction[]>(conditionCount);
                for (int i = 0; i < conditionCount; i++)
                {
                    rawString = reader.ReadLine();
                    var words = rawString.Split(' ');   // TODO: Use separator.
                    int reference = int.Parse(words[0]);
                    var values = from word in words.Skip(3)
                                 select double.Parse(word);
                    var condition = from value in values
                                    select new LambdaFunction(v => value);
                    // TODO: Parse function instead of constant.

                    conditions.Add(reference, condition.ToArray());
                }
            }
            return conditions;
        }

        public static void WriteSolutionToFile(string path, IMesh mesh, IFiniteElementFunction[] solution)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine(
@"MeshVersionFormatted 1

Dimension 2

SolAtVertices");
                writer.WriteLine(mesh.Vertices.Count);
                for (int i = 0; i < solution.Length; i++)
                    writer.Write($"{i + 1} ");
                writer.WriteLine();
                writer.WriteLine();
                foreach (var vertex in mesh.Vertices)
                {
                    for (int i = 0; i < solution.Length; i++)
                        writer.Write($"{solution[i].GetValueAt(vertex.Position)} ");
                    writer.WriteLine();
                }
                writer.WriteLine();
                writer.WriteLine("End");
            }
        }
    }
}
