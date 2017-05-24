using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FEM_NET.FEM2D;

namespace FEM_NET
{
    internal static class InOut
    {
        public static void WriteSolutionToFile(string path, Mesh mesh, IFunction[] solution)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine(
@"MeshVersionFormatted 1

Dimension 2

SolAtVertices");
                writer.WriteLine(mesh.Vertices.Count);
                writer.WriteLine($"1 {solution.Length}");
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
