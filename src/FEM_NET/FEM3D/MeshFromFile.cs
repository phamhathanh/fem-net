using System.Collections.ObjectModel;
using System.IO;

namespace FEM_NET.FEM3D
{
    internal class MeshFromFile : IMesh
    {
        public ReadOnlyCollection<Node> Nodes { get; }
        public ReadOnlyCollection<Node> InteriorNodes { get; }
        public ReadOnlyCollection<Node> BoundaryNodes { get; }
        public ReadOnlyCollection<FiniteElement> FiniteElements { get; }

        public MeshFromFile(string path)
        {
            using (var reader = File.OpenText(path))
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
                    nodes[i] = new Node(PositionFromLine(rawString), i, false);
                }
                Nodes = new ReadOnlyCollection<Node>(nodes);

                do
                    rawString = reader.ReadLine();
                while (rawString != "Tetrahedra");

                rawString = reader.ReadLine();
                int feCount = int.Parse(rawString);
                var fes = new FiniteElement[feCount];
                for (int i = 0; i < feCount; i++)
                    fes[i] = FEFromLine(reader.ReadLine());
                FiniteElements = new ReadOnlyCollection<FiniteElement>(fes);
            }
        }

        private Vector3 PositionFromLine(string line)
        {
            var items = line.Split(' ');
            double x = double.Parse(items[0]),
                y = double.Parse(items[1]),
                z = double.Parse(items[2]);
            return new Vector3(x, y, z);
        }

        private FiniteElement FEFromLine(string line)
        {
            var items = line.Split(' ');
            int i = int.Parse(items[0]) - 1,
                j = int.Parse(items[1]) - 1,
                k = int.Parse(items[2]) - 1,
                l = int.Parse(items[3]) - 1;
            return new FiniteElement(Nodes[i], Nodes[j], Nodes[k], Nodes[l]);
        }
    }
}