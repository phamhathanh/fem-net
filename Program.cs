using System;

namespace FEM_NET
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            FEM2D.FEM2DProgram.Run();
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
