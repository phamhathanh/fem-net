using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FEMSharp.FEM2D
{
    internal class P1MeshFromFile : IMesh
    {
        private List<Node> nodes;
        public IReadOnlyCollection<Node> Nodes => nodes.AsReadOnly();
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
                int nodeCount = int.Parse(rawString);

                var nodes = new Node[nodeCount];
                for (int i = 0; i < nodeCount; i++)
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
                    nodes[i] = new Node(position, reference);
                }
                this.nodes = nodes.ToList();
                // TODO: Prevent copying by using own implementation of IReadOnlyCollection.

                do
                    rawString = reader.ReadLine();
                while (rawString != "Triangles");

                rawString = reader.ReadLine();
                int feCount = int.Parse(rawString);

                var fes = new P1FiniteElement[feCount];
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
                FiniteElements = new ReadOnlyCollection<P1FiniteElement>(fes);
            }
        }

        private P1FiniteElement ReadFiniteElement(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1;

            return new P1FiniteElement(nodes[i], nodes[j], nodes[k]);
        }
    }
}
