using FEMSharp.FEM3D;
using System;
using System.Collections.ObjectModel;

namespace FEMSharp
{
    internal static class FEM3DProgram
    {
        public static void Run()
        {
            var mesh = new MockMesh();
            var biologicalEquation = new BiologicalEquation(mesh);
            biologicalEquation.SolveAndOutput();
        }

        private class MockMesh : IMesh
        {
            public ReadOnlyCollection<Node> BoundaryNodes { get; }
            public ReadOnlyCollection<FiniteElement> FiniteElements { get; }
            public ReadOnlyCollection<Node> InteriorNodes { get; }
            public ReadOnlyCollection<Node> Nodes { get; }

            public MockMesh()
            {
                var nodes = new[] { new Node(new Vector3(0, 0, 0), 0, false),
                                    new Node(new Vector3(0, 0, 1), 1, false),
                                    new Node(new Vector3(1, 0, 0), 2, false),
                                    new Node(new Vector3(1, 0, 1), 3, false),
                                    new Node(new Vector3(0, 1, 0), 4, false),
                                    new Node(new Vector3(0, 1, 1), 5, false),
                                    new Node(new Vector3(1, 1, 0), 6, false),
                                    new Node(new Vector3(1, 1, 1), 7, false) };
                Nodes = new ReadOnlyCollection<Node>(nodes);

                var finiteElements = new[] { new FiniteElement(nodes[0], nodes[7], nodes[2], nodes[6]),
                                            new FiniteElement(nodes[0], nodes[3], nodes[2], nodes[7]),
                                            new FiniteElement(nodes[0], nodes[7], nodes[1], nodes[3]),
                                            new FiniteElement(nodes[0], nodes[7], nodes[6], nodes[4]),
                                            new FiniteElement(nodes[0], nodes[7], nodes[4], nodes[5]),
                                            new FiniteElement(nodes[0], nodes[5], nodes[1], nodes[7]) };
                FiniteElements = new ReadOnlyCollection<FiniteElement>(finiteElements);
            }
        }
    }
}
