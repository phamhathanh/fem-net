using System.Collections.Generic;

namespace _2DFEM
{
    class Mesh
    {
        private readonly Node[] nodes, interiorNodes, boundaryNodes;
        private readonly FiniteElement[] finiteElements;
        
        public int NodesCount
        {
            get
            {
                return nodes.Length;
            }
        }

        public int InteriorNodesCount
        {
            get
            {
                return interiorNodes.Length;
            }
        }

        public int BoundaryNodesCount
        {
            get
            {
                return boundaryNodes.Length;
            }
        }

        public int FiniteElementsCount
        {
            get
            {
                return finiteElements.Length;
            }
        }

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
                    Vector2 nodePosition = new Vector2(a + i * h, c + j * k);
                    bool nodeIsInside = j != 0 && j != m + 1 && i != 0 && i != n + 1;
                    if (nodeIsInside)
                    {
                        node = new Node(nodePosition, interiorNodesCount, true);
                        interiorNodes[interiorNodesCount] = node;
                        interiorNodesCount++;
                    }
                    else
                    {
                        node = new Node(nodePosition, boundaryNodesCount, false);
                        boundaryNodes[boundaryNodesCount] = node;
                        boundaryNodesCount++;
                    }
                    int nodeIndex = j * (n + 2) + i;
                    nodes[nodeIndex] = node;
                }


            finiteElements = new FiniteElement[(n + 1) * (m + 1) * 2];

            for (int j = 0; j < m + 1; j++)
                for (int i = 0; i < n + 1; i++)
                {
                    int leftLower = j * (n + 2) + i,
                        leftUpper = (j + 1) * (n + 2) + i,
                        rightLower = j * (n + 2) + i + 1,
                        rightUpper = (j + 1) * (n + 2) + i + 1,
                        index = (j * (n + 1) + i) * 2;
                    finiteElements[index] = new FiniteElement(nodes[leftLower], nodes[rightUpper], nodes[leftUpper]);
                    finiteElements[index + 1] = new FiniteElement(nodes[rightLower], nodes[leftLower], nodes[rightUpper]);
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

        public IEnumerable<FiniteElement> GetFiniteElements()
        {
            foreach (var finiteElement in finiteElements)
                yield return finiteElement;
        }
    }
}
