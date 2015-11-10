namespace _2DFEM
{
    class Node
    {
        private readonly Vector2 position;
        private readonly int index;
        private readonly bool isInside;

        public Vector2 Position
        {
            get
            {
                return position;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public bool IsInside
        {
            get
            {
                return isInside;
            }
        }

        public Node(Vector2 position, int index, bool isInside)
        {
            this.position = position;
            this.index = index;
            this.isInside = isInside;
        }

        public double Phi(Node n2, Node n3, Vector2 v)
        {
            double x1 = this.position.x,
                   y1 = this.position.y,
                   x2 = n2.position.x,
                   y2 = n2.position.y,
                   x3 = n3.position.x,
                   y3 = n3.position.y;

            return ((y3 - y2) * v.x + (x2 - x3) * v.y + y2 * x3 - x2 * y3)
                    / (x3 * y2 - x2 * y3 + x2 * y1 - x3 * y1 + x1 * y3 - x1 * y2);
        }

        public Vector2 GradPhi(Node n2, Node n3)
        {
            double xi = this.position.x,
                   yi = this.position.y,
                   xj = n2.position.x,
                   yj = n2.position.y,
                   xk = n3.position.x,
                   yk = n3.position.y;
            
            return new Vector2((yk - yj) / (xk * yj - xj * yk + xj * yi - xk * yi + xi * yk - xi * yj),
                            (xj - xk) / (xk * yj - xj * yk + xj * yi - xk * yi + xi * yk - xi * yj));
        }
    }
}
