using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FEM_NET.FEM2D
{
    public sealed class BilinearForm
    {
        private readonly Func<double[], double[], Vector2[], Vector2[], double> bilinearForm;

        public BilinearForm(Func<double[], double[], Vector2[], Vector2[], double> bilinearForm)
        {
            this.bilinearForm = bilinearForm;
        }

        public BilinearForm(Func<double, double, Vector2, Vector2, double> bilinearForm)
        {
            this.bilinearForm = (u, v, du, dv) => bilinearForm(u[0], v[0], du[0], dv[0]);
        }

        public double Evaluate(double[] u, double[] v, Vector2[] du, Vector2[] dv)
            => bilinearForm(u, v, du, dv);
    }
}
