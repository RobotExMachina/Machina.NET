using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using SysQuat = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace DataTypesTests
{
    [TestClass]
    public class RotationVectorTests : DataTypesTests
    {
        [TestMethod]
        public void RotationVector_Creation_PositiveAngle()
        {
            RotationVector rv;

            double x, y, z, angle;
            double len, len2;
            Vector v1, v2;

            // Test random axes
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(0, 720);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                rv = new RotationVector(x, y, z, angle);
                Trace.WriteLine(rv);

                // Raw check
                len = Math.Sqrt(x * x + y * y + z * z);
                len2 = Math.Sqrt(rv.X * rv.X + rv.Y * rv.Y + rv.Z * rv.Z);
                Trace.WriteLine(len);
                Trace.WriteLine(len2);
                Assert.AreNotEqual(len, len2, 0.000001);
                Assert.AreEqual(angle, len2, 0.000001);
                Assert.AreEqual(angle, rv.GetAngle(), 0.000001);

                v1 = new Vector(x, y, z);
                v1.Normalize();
                v2 = rv.GetVector();

                Assert.IsTrue(v1.IsSimilar(v2));
            }

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = 0; angle <= 720; angle += 45)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            rv = new RotationVector(x, y, z, angle);
                            Trace.WriteLine(rv);

                            // Raw check
                            len = Math.Sqrt(x * x + y * y + z * z);
                            len2 = Math.Sqrt(rv.X * rv.X + rv.Y * rv.Y + rv.Z * rv.Z);
                            Trace.WriteLine(len);
                            Trace.WriteLine(len2);

                            if (angle == 0 || len == 0)
                            {
                                Assert.IsTrue(rv.IsZero());
                                Assert.AreEqual(0, rv.GetAngle(), 0.000001);
                            }
                            else
                            {
                                Assert.AreNotEqual(len, len2, 0.000001);
                                Assert.AreEqual(angle, len2, 0.000001);
                                Assert.AreEqual(angle, rv.GetAngle(), 0.000001);

                                v1 = new Vector(x, y, z);
                                v1.Normalize();
                                v2 = rv.GetVector();

                                Assert.IsTrue(v1.IsSimilar(v2));
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RotationVector_Creation_NegativeAngle()
        {
            RotationVector rv;

            double x, y, z, angle;
            double len, len2;

            Vector v1, v2;

            // Test random axes
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-720, 0);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                rv = new RotationVector(x, y, z, angle);
                Trace.WriteLine(rv);

                v1 = new Vector(x, y, z);
                v1.Normalize();
                v1.Invert();
                v2 = rv.GetVector();
                Assert.IsTrue(v1.IsSimilar(v2));


                // Raw check
                len = Math.Sqrt(x * x + y * y + z * z);
                len2 = Math.Sqrt(rv.X * rv.X + rv.Y * rv.Y + rv.Z * rv.Z);
                Trace.WriteLine(len);
                Trace.WriteLine(len2);
                Assert.AreNotEqual(len, len2, 0.000001);
                Assert.AreEqual(-angle, len2, 0.000001);
                Assert.AreEqual(-angle, rv.GetAngle(), 0.000001);
            }

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = 720; angle <= 0; angle += 45)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            rv = new RotationVector(x, y, z, angle);
                            Trace.WriteLine(rv);

                            // Raw check
                            len = Math.Sqrt(x * x + y * y + z * z);
                            len2 = Math.Sqrt(rv.X * rv.X + rv.Y * rv.Y + rv.Z * rv.Z);
                            Trace.WriteLine(len);
                            Trace.WriteLine(len2);

                            if (angle == 0 || len == 0)
                            {
                                Assert.IsTrue(rv.IsZero());
                                Assert.AreEqual(0, rv.GetAngle(), 0.000001);
                            }
                            else
                            {
                                Assert.AreNotEqual(len, len2, 0.000001);
                                Assert.AreEqual(-angle, len2, 0.000001);
                                Assert.AreEqual(-angle, rv.GetAngle(), 0.000001);

                                v1 = new Vector(x, y, z);
                                v1.Normalize();
                                v2 = rv.GetVector();
                                v2.Invert();

                                Assert.IsTrue(v1.IsSimilar(v2));
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RotationVector_ToAxisAngle_ToRotationVector()
        {
            RotationVector rv1, rv2;
            AxisAngle aa;

            double x, y, z, angle;
            double len;

            // Test random axes
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-720, 720);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                rv1 = new RotationVector(x, y, z, angle);
                aa = rv1.ToAxisAngle();
                rv2 = aa.ToRotationVector();
                Trace.WriteLine(rv1);
                Trace.WriteLine(aa);
                Trace.WriteLine(rv2);

                Assert.IsTrue(rv1.IsSimilar(rv2));
                Assert.AreEqual(angle > 0 ? angle : -angle, aa.Angle, 0.00001);
            }

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = -720; angle <= 0; angle += 45)
                        {

                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            rv1 = new RotationVector(x, y, z, angle);
                            aa = rv1.ToAxisAngle();
                            rv2 = aa.ToRotationVector();
                            Trace.WriteLine(rv1);
                            Trace.WriteLine(aa);
                            Trace.WriteLine(rv2);

                            len = rv1.Length();

                            if (angle == 0 || len == 0)
                            {
                                Assert.IsTrue(aa.IsZero());
                                Assert.IsTrue(rv2.IsZero());
                            }
                            else
                            {
                                Assert.IsTrue(rv1.IsSimilar(rv2));
                                Assert.AreEqual(angle > 0 ? angle : -angle, aa.Angle, 0.00001);
                            }

                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RotationVector_ToQuaternion_ToRotationVector()
        {
            RotationVector rv1, rv2;
            Quaternion q;

            double x, y, z, angle;
            double len;

            // Test random axes
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                rv1 = new RotationVector(x, y, z, angle);
                q = rv1.ToQuaternion();
                rv2 = q.ToRotationVector();
                Trace.WriteLine(rv1 + " (" + rv1.ToAxisAngle() + ")");
                Trace.WriteLine(q + " (" + q.ToAxisAngle() + ")");
                Trace.WriteLine(rv2 + " (" + rv2.ToAxisAngle() + ")");

                //rv1 = new RotationVector(x, y, z, angle);
                //aa1 = rv1.ToAxisAngle();
                //q = aa1.ToQuaternion();
                //rv2 = q.ToRotationVector();
                //Trace.WriteLine("Itemized:");
                //Trace.WriteLine(rv1 + " (" + rv1.ToAxisAngle() + ")");
                //Trace.WriteLine(aa1);
                //Trace.WriteLine(q + " (" + q.ToAxisAngle() + ")");
                //Trace.WriteLine(rv2 + " (" + rv2.ToAxisAngle() + ")");

                ////Assert.IsTrue(rv1 == rv2);
                ////Assert.AreEqual(angle > 0 ? angle : -angle, rv2.GetAngle(), 0.00001);

                // This is not very clean, but I guess does the job...? 
                //Assert.IsTrue(rv1.ToAxisAngle().IsEquivalent(rv2.ToAxisAngle()));
                Assert.IsTrue(rv1.IsEquivalent(rv2));
            }

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = -720; angle <= 0; angle += 45)
                        {

                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            rv1 = new RotationVector(x, y, z, angle);
                            q = rv1.ToQuaternion();
                            rv2 = q.ToRotationVector();
                            Trace.WriteLine(rv1 + " (" + rv1.ToAxisAngle() + ")");
                            Trace.WriteLine(q + " (" + q.ToAxisAngle() + ")");
                            Trace.WriteLine(rv2 + " (" + rv2.ToAxisAngle() + ")");

                            len = rv1.Length();

                            if (angle == 0 || len == 0)
                            {
                                Assert.IsTrue(q.IsIdentity());
                                Assert.IsTrue(rv2.IsZero());
                            }
                            else
                            {
                                //Assert.IsTrue(rv1.ToAxisAngle().IsEquivalent(rv2.ToAxisAngle()), "RV assert failed");
                                if (x == -1 && y == -1 && z == -1 && angle == -360)
                                {
                                    Assert.IsTrue(rv1.IsEquivalent(rv2));
                                }
                                Assert.IsTrue(rv1.IsEquivalent(rv2));
                            }

                        }
                    }
                }
            }

        }
    }
}
