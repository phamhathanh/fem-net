namespace _2DFEM
{
    public class Rectangle
    {
        public double Left { get; }
        public double Right { get; }
        public double Bottom { get; }
        public double Top { get; }

        public Rectangle(double left, double right, double bottom, double top)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }
    }
}