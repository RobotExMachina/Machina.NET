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
            Direction dir;
            bool success;

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
        }
    }
}
