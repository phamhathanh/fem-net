using System;
using System.Collections.ObjectModel;

namespace FEM_NET.FEM3D
{
    internal static class FEM3DProgram
    {
        public static void Run()
        {
            var mesh = new MeshFromFile("mesh3D.mesh");
            var biologicalEquation = new BiologicalEquation(mesh);
            biologicalEquation.SolveAndOutput();
        }
    }
}
