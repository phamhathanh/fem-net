using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FEM_NET.FEM2D
{
    public delegate double BilinearForm(double u, double v, Vector2 du, Vector2 dv);
}
