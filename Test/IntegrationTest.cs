using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMSharp.FEM3D;
using FEMSharp;

namespace Test
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void IntegrationOf1ShouldEqualTheVolumn()
        {
            var node0 = new Node(new Vector3(0, 0, 0), 0, false);
            var node1 = new Node(new Vector3(1, 1, 1), 1, false);
            var node2 = new Node(new Vector3(1, 1, 0), 2, false);
            var node3 = new Node(new Vector3(0, 1, 0), 3, false);
            var finiteElement = new FiniteElement(node0, node1, node2, node3);

            Func<Vector3, double> function = v => 1;

            var result = Calculator.Integrate(function, finiteElement);
            Assert.AreEqual(1.0/6, result);
        }

        [TestMethod]
        public void IntegrationShouldBeCorrect()
        {
            var node0 = new Node(new Vector3(0, 0, 0), 0, false);
            var node1 = new Node(new Vector3(0, 0, 1), 1, false);
            var node2 = new Node(new Vector3(1, 0, 0), 2, false);
            var node3 = new Node(new Vector3(0, 1, 0), 3, false);
            var finiteElement = new FiniteElement(node0, node1, node2, node3);

            Func<Vector3, double> function = v => Vector3.Dot(v, v);

            var result = Calculator.Integrate(function, finiteElement);
            var expected = 0.05;
            Assert.IsTrue(IsAlmostEqual(expected, result));
        }

        private bool IsAlmostEqual(double value1, double value2)
            => Math.Abs(value1 - value2) < 1e-6;
    }
}
