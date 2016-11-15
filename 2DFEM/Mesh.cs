using System.Collections.Generic;

namespace _2DFEM
{
    class Mesh
    {
        private readonly Node[] nodes, interiorNodes, boundaryNodes;
        private readonly FiniteElement[] finiteElements;
        
        public IEnumerable<Node> Nodes => GetNodes();

        public IEnumerable<Node> InteriorNodes => GetInteriorNodes();

        public IEnumerable<Node> BoundaryNodes => GetBoundaryNodes();

        public IEnumerable<FiniteElement> FiniteElements => GetFiniteElements();

        public Mesh(int horizontalPointCount, int verticalPointCount, Rectangle rectangle)
        {
            int n = horizontalPointCount,
                m = verticalPointCount;
            double a = rectangle.Left,
                    b = rectangle.Right,
                    c = rectangle.Bottom,
                    d = rectangle.Top,
                    h = (b - a) / (n + 1),
                    k = (d - c) / (m + 1);

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
                    int index = j * (n + 2) + i;
                    nodes[index] = node;
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

        private IEnumerable<Node> GetNodes()
        {
            foreach (var node in nodes)
                yield return node;
        }

        private IEnumerable<Node> GetInteriorNodes()
        {
            foreach (var node in interiorNodes)
                yield return node;
        }

        private IEnumerable<Node> GetBoundaryNodes()
        {
            foreach (var node in boundaryNodes)
                yield return node;
        }

        private IEnumerable<FiniteElement> GetFiniteElements()
        {
            foreach (var finiteElement in finiteElements)
                yield return finiteElement;
        }
    }
}
