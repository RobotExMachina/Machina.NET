using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace DataTypesTests
{
    [TestClass]
    public class AxisAngleTests
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
                        for (angle = -720; angle <= 720; angle += 90)
                        {
                            Trace.WriteLine("");
                            Trace.WriteLine(x + " " + y + " " + z + " " + angle);

                            aa = new AxisAngle(x, y, z, angle);
                            Trace.WriteLine(aa);

                            zero = angle == 0 || (aa.X == 0 && aa.Y == 0 && aa.Z == 0);
                            Assert.AreEqual(zero, aa.IsZero());
                        }
                    }
                }
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
            Vector3 normV;
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
                normV = new Vector3((float)x, (float)y, (float)z);
                normV = Vector3.Normalize(normV);
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
                                normV = new Vector3((float)x, (float)y, (float)z);
                                normV = Vector3.Normalize(normV);
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


        // @TODO: Add AA -> Q -> AA



        internal static Random rnd = new System.Random();

        public double Random(double min, double max)
        {
            return Lerp(min, max, rnd.NextDouble());
        }

        public double Random()
        {
            return Random(0, 1);
        }

        public double Random(double max)
        {
            return Random(0, max);
        }

        public double Lerp(double start, double end, double norm)
        {
            return start + (end - start) * norm;
        }

        public double Normalize(double value, double start, double end)
        {
            return (value - start) / (end - start);
        }

        public double Map(double value, double sourceStart, double sourceEnd, double targetStart, double targetEnd)
        {
            //double n = Normalize(value, sourceStart, sourceEnd);
            //return targetStart + n * (targetEnd - targetStart);
            return targetStart + (targetEnd - targetStart) * (value - sourceStart) / (sourceEnd - sourceStart);
        }

    }
}
