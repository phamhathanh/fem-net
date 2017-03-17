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
        public IReadOnlyCollection<Node> InteriorNodes { get; }
        public IReadOnlyCollection<Node> BoundaryNodes { get; }
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

                reader.ReadLine();
                var nodes = new Node[nodeCount];
                int interiorIndex = 0,
                    boundaryIndex = 0;
                for (int i = 0; i < nodeCount; i++)
                {
                    rawString = reader.ReadLine();
                    var position = ReadPosition(rawString);
                    var isInside = !IsOnBoundary(position);
                    int index;
                    if (isInside)
                    {
                        index = interiorIndex;
                        interiorIndex++;
                    }
                    else
                    {
                        index = boundaryIndex;
                        boundaryIndex++;
                    }
                    nodes[i] = new Node(position, index, isInside);
                }
                this.nodes = nodes.ToList();
                BoundaryNodes = (from node in Nodes
                                 where IsOnBoundary(node.Position)
                                 select node).ToList().AsReadOnly();
                InteriorNodes = (from node in Nodes
                                 where !IsOnBoundary(node.Position)
                                 select node).ToList().AsReadOnly();
                // TODO: Prevent copying by using own implementation of IReadOnlyCollection.

                do
                    rawString = reader.ReadLine();
                while (rawString != "Triangles");

                rawString = reader.ReadLine();
                int feCount = int.Parse(rawString);

                reader.ReadLine();
                var fes = new P1FiniteElement[feCount];
                for (int i = 0; i < feCount; i++)
                    fes[i] = ReadFiniteElement(reader.ReadLine());
                FiniteElements = new ReadOnlyCollection<P1FiniteElement>(fes);
            }
        }

        private Vector2 ReadPosition(string rawString)
        // TODO: Exception.
        {
            var items = rawString.Split(' ');
            double x = double.Parse(items[0]),
                y = double.Parse(items[1]);
            return new Vector2(x, y);
        }

        private bool IsOnBoundary(Vector2 node)
            => node.x == 0 || node.x == 1 || node.y == 0 || node.y == 1;
        // TODO: Read from file.

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
