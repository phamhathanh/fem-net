using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DFEM
{
    class Mesh
    {
        private readonly Node[] nodes, interiorNodes, boundaryNodes;
        private readonly FiniteElement[] finiteElements;
        
        public Mesh()
        {
            int n = Input.n,
                m = Input.m;
            double a = Input.a,
                   c = Input.c,
                   h = (Input.b - Input.a) / (n + 1),
                   k = (Input.d - Input.c) / (m + 1);

            nodes = new Node[(n + 2) * (m + 2)];
            interiorNodes = new Node[n * m];
            boundaryNodes = new Node[(n + 2) * (m + 2) - n * m];

            int interiorNodesCount = 0,
                boundaryNodesCount = 0;
            for (int j = 0; j < m + 2; j++)
                for (int i = 0; i < n + 2; i++)
                {
                    Node node;
                    if (j != 0 && j != m + 1 && i != 0 && i != n + 1)
                    {
                        node = new Node(new Vector2(a + i * h, c + j * k), interiorNodesCount, true);
                        nodes[j * (n + 2) + i] = node;
                        interiorNodes[(j - 1) * n + (i - 1)] = node;
                        interiorNodesCount++;
                    }
                    else
                    {
                        node = new Node(new Vector2(a + i * h, c + j * k), boundaryNodesCount, false);
                        nodes[j * (n + 2) + i] = node;
                        boundaryNodes[boundaryNodesCount] = node;
                        boundaryNodesCount++;
                    }
                }


            finiteElements = new FiniteElement[(n + 1) * (m + 1) * 2];

            for (int j = 0; j < m + 1; j++)
                for (int i = 0; i < n + 1; i++)
                {
                    finiteElements[(j * (n + 1) + i) * 2] =
                        new FiniteElement(nodes[j * (n + 2) + i], nodes[(j + 1) * (n + 2) + i + 1], nodes[(j + 1) * (n + 2) + i]);
                    finiteElements[(j * (n + 1) + i) * 2 + 1] =
                        new FiniteElement(nodes[j * (n + 2) + i + 1], nodes[j * (n + 2) + i], nodes[(j + 1) * (n + 2) + i + 1]);
                }
        }

        public Node[] GetNodes()
        {
            Node[] output = new Node[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                output[i] = nodes[i];

            return output;
        }

        public Node[] GetInteriorNodes()
        {
            Node[] output = new Node[interiorNodes.Length];
            for (int i = 0; i < interiorNodes.Length; i++)
                output[i] = interiorNodes[i];

            return output;
        }

        public Node[] GetBoundaryNodes()
        {
            Node[] output = new Node[boundaryNodes.Length];
            for (int i = 0; i < boundaryNodes.Length; i++)
                output[i] = boundaryNodes[i];

            return output;
        }

        public FiniteElement[] GetFiniteElements()
        {
            FiniteElement[] output = new FiniteElement[finiteElements.Length];
            for (int i = 0; i < finiteElements.Length; i++)
                output[i] = finiteElements[i];

            return output;
        }
    }
}
