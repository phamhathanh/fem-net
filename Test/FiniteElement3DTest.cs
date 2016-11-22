using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMSharp.FEM3D;

namespace Test
{
    [TestClass]
    public class FiniteElement3DTest
    {
        [TestMethod]
        public void BasisFunctionShouldReturnSpecificValuesAtVertices()
        {
            var node0 = new Node(new Vector3(0, 0, 0), 0, false);
            var node1 = new Node(new Vector3(1, 1, 1), 1, false);
            var node2 = new Node(new Vector3(1, 1, 0), 2, false);
            var node3 = new Node(new Vector3(0, 1, 0), 3, false);

            var finiteElement = new FiniteElement(node0, node1, node2, node3);

            var phi0 = finiteElement.Nodes[0].Phi;
            Assert.AreEqual(1, phi0(node0.Position));
            Assert.AreEqual(0, phi0(node1.Position));
            Assert.AreEqual(0, phi0(node2.Position));
            Assert.AreEqual(0, phi0(node3.Position));
        }
    }
}
