using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using Machina.Types.Geometry;

namespace DataTypesTests
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void Vector_PerpendicularTo()
        {
            Vector vec, perp;
            Direction dir;
            bool success;

            for (var i = 0; i < 100; i++)
            {
                vec = Vector.RandomFromDoubles(-100, 100);
                success = Vector.PerpendicularTo(vec, out perp);

                Trace.WriteLine("");
                Trace.WriteLine(vec + " " + perp);

                dir = Vector.CompareDirections(vec, perp);
                if (success)
                {
                    Assert.IsTrue(dir == Direction.Orthogonal, "Vectors are not perpendicular");
                }
                else
                {
                    Assert.IsTrue(false, "Check this out " + dir);
                }
            }

            for (var i = 0; i < 100; i++)
            {
                vec = Vector.RandomFromInts(-1, 1);
                success = Vector.PerpendicularTo(vec, out perp);

                Trace.WriteLine("");
                Trace.WriteLine(vec + " " + perp);

                dir = Vector.CompareDirections(vec, perp);
                if (success)
                {
                    Assert.IsTrue(dir == Direction.Orthogonal, "Vectors are not perpendicular");
                }
                else
                {
                    Assert.IsTrue(dir == Direction.Invalid, "Could not do perp for valid vectors");
                }
            }
        }
    }
}
