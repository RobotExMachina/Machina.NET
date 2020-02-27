using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Machina;
using Machina.Types.Geometry;

namespace DataTypesTests
{
    [TestClass]
    public class PointTests : DataTypesTests
    {
        [TestMethod]
        public void Point_CompareDirections()
        {
            Vector a = new Vector(1, 0, 0);

            Vector b = new Vector(1, 1, 0);
            Assert.AreEqual(0, Vector.CompareDirections(a, b));  // nothing

            b = new Vector(1, 0, 0);
            Assert.AreEqual(1, Vector.CompareDirections(a, b));  // parallel

            b = new Vector(5, 0, 0);
            Assert.AreEqual(1, Vector.CompareDirections(a, b));  // parallel

            b = new Vector(10, 0, 0);
            Assert.AreEqual(1, Vector.CompareDirections(a, b));  // parallel

            b = new Vector(0, 1, 0);
            Assert.AreEqual(2, Vector.CompareDirections(a, b));  // orthogonal

            b = new Vector(0, 0, 1);
            Assert.AreEqual(2, Vector.CompareDirections(a, b));  // orthogonal

            b = new Vector(0, -1, 0);
            Assert.AreEqual(2, Vector.CompareDirections(a, b));  // orthogonal

            b = new Vector(0, 0, -1);
            Assert.AreEqual(2, Vector.CompareDirections(a, b));  // orthogonal

            b = new Vector(-1, 0, 0);
            Assert.AreEqual(3, Vector.CompareDirections(a, b));  // opposed

            b = new Vector(-5, 0, 0);
            Assert.AreEqual(3, Vector.CompareDirections(a, b));  // opposed

            b = new Vector(-10, 0, 0);
            Assert.AreEqual(3, Vector.CompareDirections(a, b));  // opposed

            a = new Vector(Random(-100, 100), Random(-100, 100), Random(-100, 100));
            b = new Vector(5 * a.X, 5 * a.Y, 5 * a.Z);
            Trace.WriteLine(a);
            Trace.WriteLine(b);
            Assert.AreEqual(1, Vector.CompareDirections(a, b));  // parallel

            b = new Vector(-a.X, -a.Y, -a.Z);
            Trace.WriteLine(a);
            Trace.WriteLine(b);
            Assert.AreEqual(3, Vector.CompareDirections(a, b));  // opposed

        }

    }
}
