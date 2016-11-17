namespace _2DFEM
{
    class Node
    {
        public Vector2 Position { get; }
        public int Index { get; }
        public bool IsInside { get; }

        public Node(Vector2 position, int index, bool isInside)
        {
            Position = position;
            Index = index;
            IsInside = isInside;
        }

        public Vector2 GradPhi(Node n2, Node n3)
        {
            double xi = this.Position.x,
                   yi = this.Position.y,
                   xj = n2.Position.x,
                   yj = n2.Position.y,
                   xk = n3.Position.x,
                   yk = n3.Position.y;
            
            return new Vector2((yk - yj) / (xk * yj - xj * yk + xj * yi - xk * yi + xi * yk - xi * yj),
                            (xj - xk) / (xk * yj - xj * yk + xj * yi - xk * yi + xi * yk - xi * yj));
        }
    }
}
