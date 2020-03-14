using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using Machina.Types.Geometry;

namespace DataTypesTests
{
    [TestClass]
    public class PlaneTests
    {
        [TestMethod]
        public void Plane_OriginNormal()
        {
            Vector normal, origin = Vector.Zero;
            Plane plane;
            Matrix m;

            for (var i = 0; i < 100; i++)
            {
                normal = Vector.RandomFromDoubles(-100, 100);
                plane = new Plane(origin, normal);

                Trace.WriteLine("");
                Trace.WriteLine(origin + " " + normal);
                Trace.WriteLine(plane);

                if (plane == Plane.Unset)
                {
                    Assert.IsTrue(normal.IsZero, "Something is wrong...");
                }
                else
                {
                    m = Matrix.CreateFromPlane(plane);
                    Trace.WriteLine(m);
                    Assert.IsTrue(m.IsOrthogonalRotation, "Plane is not orthonormal");
                }
            }

            for (var i = 0; i < 100; i++)
            {
                normal = Vector.RandomFromInts(-1, 1);
                plane = new Plane(origin, normal);

                Trace.WriteLine("");
                Trace.WriteLine(origin + " " + normal);
                Trace.WriteLine(plane);

                if (plane == Plane.Unset)
                {
                    Assert.IsTrue(normal.IsZero, "Something is wrong...");
                }
                else
                {
                    m = Matrix.CreateFromPlane(plane);
                    Trace.WriteLine(m);
                    Assert.IsTrue(m.IsOrthogonalRotation, "Plane is not orthonormal");
                }
            }
        }

        [TestMethod]
        public void Plane_OriginTwoVectors()
        {
            Vector x, y;
            Vector origin = Vector.Zero;
            Plane plane;
            Matrix m;

            for (var i = 0; i < 100; i++)
            {
                x = Vector.RandomFromDoubles(-100, 100);
                y = Vector.RandomFromDoubles(-100, 100);
                plane = new Plane(origin, x, y);

                Trace.WriteLine("");
                Trace.WriteLine(origin + " " + x + " " + y);
                Trace.WriteLine(plane);

                if (plane == Plane.Unset)
                {
                    Direction dir = Vector.CompareDirections(x, y);
                    Assert.IsTrue(dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Orthogonal, "Y U NO CREATE PLANE??");
                }
                else
                {
                    m = Matrix.CreateFromPlane(plane);
                    Trace.WriteLine(m);
                    Assert.IsTrue(m.IsOrthogonalRotation, "Plane is not orthonormal");
                }
            }

            for (var i = 0; i < 100; i++)
            {
                x = Vector.RandomFromInts(-1, 1);
                y = Vector.RandomFromInts(-1, 1);
                plane = new Plane(origin, x, y);

                Trace.WriteLine("");
                Trace.WriteLine(origin + " " + x + " " + y);
                Trace.WriteLine(plane);

                if (plane == Plane.Unset)
                {
                    Direction dir = Vector.CompareDirections(x, y);
                    Assert.IsTrue(dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Opposite, "Y U NO CREATE PLANE??");
                }
                else
                {
                    m = Matrix.CreateFromPlane(plane);
                    Trace.WriteLine(m);
                    Assert.IsTrue(m.IsOrthogonalRotation, "Plane is not orthonormal");
                }
            }
        }
    }
}
