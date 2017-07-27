using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;

namespace DataTypesTests
{
    [TestClass]
    public class AxisAngleTests : DataTypesTests
    {

        //   █████╗ ██╗  ██╗██╗███████╗ █████╗ ███╗   ██╗ ██████╗ ██╗     ███████╗
        //  ██╔══██╗╚██╗██╔╝██║██╔════╝██╔══██╗████╗  ██║██╔════╝ ██║     ██╔════╝
        //  ███████║ ╚███╔╝ ██║███████╗███████║██╔██╗ ██║██║  ███╗██║     █████╗  
        //  ██╔══██║ ██╔██╗ ██║╚════██║██╔══██║██║╚██╗██║██║   ██║██║     ██╔══╝  
        //  ██║  ██║██╔╝ ██╗██║███████║██║  ██║██║ ╚████║╚██████╔╝███████╗███████╗
        //  ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝
        //            

        /// <summary>
        /// Are new AxisAngle objects normalized?
        /// </summary>
        [TestMethod]
        public void AxisAngle_NormalizedOnCreation()
        {
            AxisAngle aa;

            double x, y, z, angle;
            double len, len2;

            // Test random axes
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-720, 720);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                aa = new AxisAngle(x, y, z, angle);
                Trace.WriteLine(aa);

                // Raw check
                len = Math.Sqrt(x * x + y * y + z * z);
                len2 = Math.Sqrt(aa.X * aa.X + aa.Y * aa.Y + aa.Z * aa.Z);
                Trace.WriteLine(len);
                Trace.WriteLine(len2);
                Assert.AreNotEqual(len, len2, 0.000001);
                Assert.AreEqual(1, len2, 0.000001);

                // .AxisLength() check
                len = aa.AxisLength();
                Trace.WriteLine(len);
                Assert.AreEqual(1, len, 0.0000001);
            }

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = -720; angle <= 720; angle += 45)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            aa = new AxisAngle(x, y, z, angle);
                            Trace.WriteLine(aa);

                            // Raw check
                            len = Math.Sqrt(x * x + y * y + z * z);
                            len2 = Math.Sqrt(aa.X * aa.X + aa.Y * aa.Y + aa.Z * aa.Z);
                            Trace.WriteLine(len);
                            Trace.WriteLine(len2);
                            Assert.AreEqual(len == 0 ? 0 : 1, len2, 0.000001);

                            // .AxisLength() check
                            len2 = aa.AxisLength();
                            Trace.WriteLine(len);
                            Assert.AreEqual(len == 0 ? 0 : 1, len2, 0.0000001);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// .IsZero() returns true if this AxisAngle represents no rotation: 
        /// zero rotation axis or zero angle.
        /// </summary>
        [TestMethod]
        public void AxisAngle_IsZero()
        {
            AxisAngle aa;

            double x, y, z, angle;
            bool zero;

            // Test all permutations of unitary components (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = -1440; angle <= 1440; angle += 90)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            aa = new AxisAngle(x, y, z, angle);
                            Trace.WriteLine(aa);

                            zero = angle % 360 == 0 || (aa.X == 0 && aa.Y == 0 && aa.Z == 0);
                            Assert.AreEqual(zero, aa.IsZero());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Quick manual equivalence tests
        /// </summary>
        [TestMethod]
        public void AxisAngle_Equivalence()
        {
            AxisAngle a = new AxisAngle(0, 0, 1, 45);
            AxisAngle b = new AxisAngle(0, 0, 1, 45 + 360);
            Trace.WriteLine(a);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));
            Assert.IsTrue(b.IsEquivalent(a));

            b = new AxisAngle(0, 0, 1, 45 - 360);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));

            b = new AxisAngle(0, 0, -1, -45);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));

            b = new AxisAngle(0, 0, -1, -45 - 360);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));

            b = new AxisAngle(0, 0, -1, -45 + 360);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));

            b = new AxisAngle(0, 0, 10, 45);
            Trace.WriteLine(b);
            Assert.IsTrue(a.IsEquivalent(b));

            // Zero vectors
            a = new AxisAngle(0, 0, 0, 0);
            Trace.WriteLine(a);
            for (var i = 0; i < 50; i++) {
                b = new AxisAngle(Random(-10, 10), Random(-10, 10), Random(-10, 10), RandomInt(-3, 3) * 360);
                Trace.WriteLine(b);

                Assert.IsTrue(a.IsEquivalent(b));
                Assert.IsTrue(b.IsEquivalent(a));
            }
            
        }



        /// <summary>
        /// AxisAngle to Quaternion conversion tests.
        /// Using System.Numerics.Quaternion for comparison.
        /// </summary>
        [TestMethod]
        public void AxisAngle_ToQuaternionConversion()
        {
            AxisAngle aa;
            Quaternion q;

            Point v;
            SysVec normV;
            SysQuat sq;

            double x, y, z, angle;
            bool zero;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-720, 720);

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                aa = new AxisAngle(x, y, z, angle);
                q = aa.ToQuaternion();      // this method will normalize the Quaternion, as neccesary for spatial rotation representation
                Trace.WriteLine(aa);
                Trace.WriteLine(q);

                // TEST 1: compare to System.Numeric.Quaternion
                normV = new SysVec((float)x, (float)y, (float)z);
                normV = SysVec.Normalize(normV);
                sq = SysQuat.CreateFromAxisAngle(normV, (float)(angle * Math.PI / 180.0));  // now this Quaternion SHOULD be normalized...
                Trace.WriteLine(sq + " length: " + sq.Length());

                Assert.AreEqual(q.W, sq.W, 0.000001, "Failed W");  // can't go very precise due to float imprecision in sys quat
                Assert.AreEqual(q.X, sq.X, 0.000001, "Failed X");
                Assert.AreEqual(q.Y, sq.Y, 0.000001, "Failed Y");
                Assert.AreEqual(q.Z, sq.Z, 0.000001, "Failed Z");
            }

            // Test all permutations of unitary components quaternions (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = -720; angle <= 720; angle += 22.5)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                            // Normalize
                            v = new Point(x, y, z);
                            v.Normalize();

                            aa = new AxisAngle(v, angle);
                            q = aa.ToQuaternion();
                            Trace.WriteLine(aa);
                            Trace.WriteLine(q);

                            zero = aa.IsZero();

                            if (zero)
                            {
                                Assert.IsTrue(new Quaternion(1, 0, 0, 0) == q, "Failed zero quaternion");
                            }
                            else
                            {
                                // TEST 1: compare to System.Numeric.Quaternion
                                //sq = SysQuat.CreateFromAxisAngle(new Vector3((float)x, (float)y, (float)z), (float)(angle * Math.PI / 180.0));  // this Quaternion is not a versor (not unit)
                                normV = new SysVec((float)x, (float)y, (float)z);
                                normV = SysVec.Normalize(normV);
                                sq = SysQuat.CreateFromAxisAngle(normV, (float)(angle * Math.PI / 180.0));  // now this Quaternion SHOULD be normalized...
                                Trace.WriteLine(sq + " length: " + sq.Length());

                                Assert.AreEqual(q.W, sq.W, 0.000001, "Failed W");  // can't go very precise due to float imprecision in sys quat
                                Assert.AreEqual(q.X, sq.X, 0.000001, "Failed X");
                                Assert.AreEqual(q.Y, sq.Y, 0.000001, "Failed Y");
                                Assert.AreEqual(q.Z, sq.Z, 0.000001, "Failed Z");
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Does AA to Q to AA work correctly?
        /// Test simple case where AA! rotation is [0, 360]
        /// </summary>
        [TestMethod]
        public void AxisAngle_ToQuaternion_ToAxisAngle_Simple()
        {
            AxisAngle aa1, aa2;
            Quaternion q;

            double x, y, z, angle;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(360);  // choose only positive angles, because quaternion coversion will always return a positive rotation in [0, 360]

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                aa1 = new AxisAngle(x, y, z, angle);
                q = aa1.ToQuaternion();      // this method will normalize the Quaternion, as neccesary for spatial rotation representation
                aa2 = q.ToAxisAngle();
                Trace.WriteLine(aa1);
                Trace.WriteLine(q);
                Trace.WriteLine(aa2);

                Assert.IsTrue(aa1 == aa2, "Boooo! :(");
            }


            // Test all permutations of unitary components quaternions (including zero)
            for (x = -1; x <= 1; x++)
            {
                for (y = -1; y <= 1; y++)
                {
                    for (z = -1; z <= 1; z++)
                    {
                        for (angle = 0; angle <= 360; angle += 45)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                            aa1 = new AxisAngle(x, y, z, angle);
                            q = aa1.ToQuaternion();
                            aa2 = q.ToAxisAngle();
                            Trace.WriteLine(aa1);
                            Trace.WriteLine(q);
                            Trace.WriteLine(aa2);

                            // Deal with zero angles
                            if (aa1.IsZero())
                            {
                                Assert.IsTrue(aa1.IsZero());
                                Assert.IsTrue(q.IsIdentity());
                                Assert.IsTrue(aa2.IsZero());
                            }
                            else
                            {
                                Assert.IsTrue(aa1 == aa2, "Booooo! :(");
                            }
                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perform AA > Q > AA conversion from a AA1 with negative angle.
        /// Uses a .Flip() operation for comparison.
        /// </summary>
        [TestMethod]
        public void AxisAngle_ToQuaternion_ToAxisAngle_Negative()
        {
            AxisAngle aa1, aa2;
            Quaternion q;

            double x, y, z, angle;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-360, 0);  // choose only negative angles

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                aa1 = new AxisAngle(x, y, z, angle);
                q = aa1.ToQuaternion();      // this method will normalize the Quaternion, as neccesary for spatial rotation representation
                aa2 = q.ToAxisAngle();
                Trace.WriteLine(aa1);
                Trace.WriteLine(q);
                Trace.WriteLine(aa2);

                aa2.Flip();  // from quaternion conversion, aa2 will be inverted. Flip for comparison
                Trace.WriteLine(aa2 + " (flipped)");

                Assert.IsTrue(aa1 == aa2, "Boooo! :(");
            }
        }

        /// <summary>
        /// Test AA > Q > AA conversion for ANY angle.
        /// A .Modulate() operation is performed if angle is outside [0, 360] for comparison.
        /// </summary>
        [TestMethod]
        public void AxisAngle_ToQuaternion_ToAxisAngle_Modulation()
        {
            AxisAngle aa1, aa2;
            Quaternion q;

            double x, y, z, angle;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1000, 1000);  // test any possible angle

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle + " length: " + Geometry.Length(x, y, z));

                aa1 = new AxisAngle(x, y, z, angle);
                q = aa1.ToQuaternion();      // this method will normalize the Quaternion, as necessary for spatial rotation representation
                aa2 = q.ToAxisAngle();
                Trace.WriteLine(aa1);
                Trace.WriteLine(q);
                Trace.WriteLine(aa2);

                // If angle wasn't within 'module', modulate it
                if (angle < 0 || angle > 360)
                {
                    aa1.Modulate();
                    q = aa1.ToQuaternion();
                    aa2 = q.ToAxisAngle();
                    Trace.WriteLine("After modulation:");
                    Trace.WriteLine(aa1);
                    Trace.WriteLine(q);
                    Trace.WriteLine(aa2);
                }

                // Now this should work
                Assert.IsTrue(aa1 == aa2);
            }
        }

        [TestMethod]
        public void AxisAngle_ToRotationMatrix_ToAxisAngle()
        {
            AxisAngle aa, aabis;
            RotationMatrix m;

            double x, y, z, angle;
            Point axis;

            // Test random permutations
            for (var i = 0; i < 50; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);  // test any possible angle

                aa = new AxisAngle(x, y, z, angle);
                m = aa.ToRotationMatrix();
                aabis = m.ToAxisAngle();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine(aa);
                Trace.WriteLine(m);
                Trace.WriteLine(aabis);

                Assert.IsTrue(aa.IsEquivalent(aabis));
            }

            // Test singularities
            for (var i = 0; i < 1000; i++)
            {
                axis = Point.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-8, 8);

                aa = new AxisAngle(axis, angle);
                m = aa.ToRotationMatrix();
                aabis = m.ToAxisAngle();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine(aa);
                Trace.WriteLine(m);
                Trace.WriteLine(aabis);

                Assert.IsTrue(aa.IsEquivalent(aabis));
            }

        }

        [TestMethod]
        public void AxisAngle_ToYawPitchRoll_ComparisonThroughRotationMatrix()
        {
            AxisAngle aa;
            RotationMatrix m;
            YawPitchRoll eu1, eu2;

            double x, y, z, angle;
            Point axis;

            // Test random permutations
            for (var i = 0; i < 200; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);  // test any possible angle

                aa = new AxisAngle(x, y, z, angle);
                eu1 = aa.ToYawPitchRoll();
                m = aa.ToRotationMatrix();
                eu2 = m.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine(aa + " --> " + eu1);
                Trace.WriteLine(aa + " --> " + m + " --> " + eu2);

                Assert.IsTrue(eu1 == eu2);
            }

            // Test singularities
            for (var i = 0; i < 200; i++)
            {
                axis = Point.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-8, 8);

                aa = new AxisAngle(axis, angle);
                eu1 = aa.ToYawPitchRoll();
                m = aa.ToRotationMatrix();
                eu2 = m.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine(aa + " --> " + eu1);
                Trace.WriteLine(aa + " --> " + m + " --> " + eu2);

                Assert.IsTrue(eu1 == eu2);
            }
        }

        [TestMethod]
        public void AxisAngle_ToYawPitchRoll_ToAxisAngle()
        {
            AxisAngle aa1, aa2, aa3;
            YawPitchRoll eu1, eu2, eu3;

            double x, y, z, angle;
            Point axis;

            // Test random permutations
            for (var i = 0; i < 200; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);  // test any possible angle

                aa1 = new AxisAngle(x, y, z, angle);
                eu1 = aa1.ToYawPitchRoll();
                aa2 = eu1.ToAxisAngle();
                eu2 = aa2.ToYawPitchRoll();
                aa3 = eu2.ToAxisAngle();
                eu3 = aa3.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine("    " + aa1 + " --> " + eu1);
                Trace.WriteLine("--> " + aa2 + " --> " + eu2);
                Trace.WriteLine("--> " + aa3 + " --> " + eu3);

                Assert.IsTrue(aa1.IsEquivalent(aa2));
                Assert.IsTrue(aa2 == aa3);
                Assert.IsTrue(eu1 == eu2);
                Assert.IsTrue(eu2 == eu3);
            }

            // Test singularities
            for (var i = 0; i < 200; i++)
            {
                axis = Point.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-8, 8);

                aa1 = new AxisAngle(axis, angle);
                eu1 = aa1.ToYawPitchRoll();
                aa2 = eu1.ToAxisAngle();
                eu2 = aa2.ToYawPitchRoll();
                aa3 = eu2.ToAxisAngle();
                eu3 = aa3.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine("    " + aa1 + " --> " + eu1);
                Trace.WriteLine("--> " + aa2 + " --> " + eu2);
                Trace.WriteLine("--> " + aa3 + " --> " + eu3);

                Assert.IsTrue(aa1.IsEquivalent(aa2));
                Assert.IsTrue(aa2 == aa3);
                Assert.IsTrue(eu1 == eu2);
                Assert.IsTrue(eu2 == eu3);
            }
        }
    }
}
