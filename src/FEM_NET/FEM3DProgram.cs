using FEMSharp.FEM3D;
using System;
using System.Collections.ObjectModel;

namespace FEMSharp
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
