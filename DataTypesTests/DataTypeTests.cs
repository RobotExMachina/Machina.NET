using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace DataTypesTests
{
    [TestClass]
    public class DataTypeTests
    {
        //[TestMethod]
        //public void Quaternion_Creation()
        //{
        //    BRobot.Quaternion q1;
        //    System.Numerics.Quaternion q2;
        //    double w, x, y, z;
        //    for (var i = 0; i < 100; i++)
        //    {
        //        w = Random(-10, 10);
        //        x = Random(-10, 10);
        //        y = Random(-10, 10);
        //        z = Random(-10, 10);
        //        q1 = new BRobot.Quaternion(w, x, y, z);
        //        q2 = new System.Numerics.Quaternion((float) x, (float) y, (float) z, (float) w);

        //        Assert.AreEqual(q1.W, q2.W, 0.000001);  // can't go very precise due to float imprecision in sys quat
        //        Assert.AreEqual(q1.X, q2.X, 0.000001);
        //        Assert.AreEqual(q1.Y, q2.Y, 0.000001);
        //        Assert.AreEqual(q1.Z, q2.Z, 0.000001);
        //    }
        //}

        public static double TAU = 2 * Math.PI;

        [TestMethod]
        public void Quaternion_Normalization_Lengths_Units_Zeros()
        {
            Quaternion q;

            double w, x, y, z;
            double len;
            bool norm;

            // Test random quaternions
            for (var i = 0; i < 50; i++)
            {
                w = Random(-100, 100);
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);

                Trace.WriteLine("");
                q = new Quaternion(w, x, y, z);
                Trace.WriteLine(q);

                // Is Length working?
                len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                Trace.WriteLine(q + " " + len);
                Assert.AreEqual(len, q.Length(), 0.0000001);

                // In Normalize working?
                norm = q.Normalize();
                Trace.WriteLine(q + " " + len);
                len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                Assert.AreEqual(len, norm ? 1 : 0, 0.0000001);

                // Is Length working?
                len = q.Length();
                Trace.WriteLine(q + " " + len);
                Assert.AreEqual(len, norm ? 1 : 0, 0.0000001);
            }

            // Test all permutations of unitary components quaternions (including zero)
            for (w = -1; w <= 1; w++)
            {
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        for (z = -1; z <= 1; z++)
                        {
                            Trace.WriteLine("");
                            q = new Quaternion(w, x, y, z);
                            Trace.WriteLine(q);

                            // Is Zero Quaternion properly handled?
                            if (w == 0 && x == 0 && y == 0 && z == 0)
                            {
                                Assert.IsTrue(q.IsZero());
                            }
                            else
                            {
                                Assert.IsFalse(q.IsZero());
                            }

                            // Is Identity Quaternion properly handled?
                            if (w == 1 && x == 0 && y == 0 && z == 0)
                            {
                                Assert.IsTrue(q.IsIdentity());
                            }
                            else
                            {
                                Assert.IsFalse(q.IsIdentity());
                            }

                            // Is Length working?
                            len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                            Trace.WriteLine(q + " " + len);
                            Assert.AreEqual(len, q.Length(), 0.0000001, "Failed Length() for " + q);

                            // Is Unit() working?
                            if (Math.Abs(len - 1) < 0.0000001)
                            {
                                Assert.IsTrue(q.IsUnit());
                            }
                            else
                            {
                                Assert.IsFalse(q.IsUnit());
                            }

                            // In Normalize working?
                            norm = q.Normalize();
                            Trace.WriteLine(q + " " + len);
                            if (w == 0 && x == 0 && y == 0 && z == 0)
                            {
                                Assert.IsFalse(norm, "Zero Quaternion improperly asserted: " + q);
                            }
                            else
                            {
                                Assert.IsTrue(norm, "Non-zero Quaternion properly asserted: " + q);
                            }

                            len = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
                            Assert.AreEqual(norm ? 1 : 0, len, 0.0000001, "Failed Normalize() for " + q);


                            // Is Length working?
                            Trace.WriteLine(q + " " + len);
                            Assert.AreEqual(len, q.Length(), 0.0000001, "Failed normalized Length()");

                        }
                    }
                }
            }
        }

        [TestMethod]
        public void AxisAngle_ToQuaternion()
        {
            AxisAngle aa;
            Quaternion q;

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
                angle = Random(-3 * 360, 3 * 360);

                // Conversion
                aa = new AxisAngle(x, y, z, angle);
                q = aa.ToQuaternion();      // this method will normalize the Quaternion, as neccesary for spatial rotation representation
                Trace.WriteLine("");
                Trace.WriteLine(aa);
                Trace.WriteLine(q);

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

            // Test all permutations of unitary components quaternions (including zero)
            for (angle = -720; angle <= 720; angle += 45)
            {
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        for (z = -1; z <= 1; z++)
                        {
                            // Conversion
                            aa = new AxisAngle(x, y, z, angle);
                            q = aa.ToQuaternion();      // this method will normalize the Quaternion, as neccesary for spatial rotation representation
                            Trace.WriteLine("");
                            Trace.WriteLine(aa);
                            Trace.WriteLine(q);

                            // TEST 0: is the rotation axis valid
                            zero = aa.IsZero();
                            Assert.AreEqual(x == 0 && y == 0 && z == 0, zero);

                            if (!zero)
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



        //[TestMethod]
        //public void AxisAngle_To_Quaternion()
        //{
        //    AxisAngle aa1;
        //    Quaternion q;
        //    //SysQuat sq;
        //    AxisAngle aa2;
        //    Point normV;
        //    AxisAngle normAA;

        //    double x, y, z, angle;
        //    //double len;
        //    //bool norm;

        //    // Test random quaternions
        //    for (var i = 0; i < 50; i++)
        //    {
        //        x = Random(-100, 100);
        //        y = Random(-100, 100);
        //        z = Random(-100, 100);
        //        angle = Random(-3 * 360, 3 * 360);

        //        aa1 = new AxisAngle(x, y, z, angle);
        //        q = aa1.ToQuaternion();
        //        //sq = SysQuat.CreateFromAxisAngle(new Vector3((float)x, (float)y, (float)z), (float) (angle * Math.PI / 180.0));  // This is giving me really strange results
        //        aa2 = q.ToAxisAngle();

        //        Trace.WriteLine("");
        //        Trace.WriteLine(aa1);
        //        Trace.WriteLine(q);
        //        //Trace.WriteLine(sq);
        //        Trace.WriteLine(aa2);


        //        normAA = new AxisAngle(x, y, z, angle);
        //        normAA.Normalize();
        //        Trace.WriteLine(normAA);

        //        //Assert.IsTrue(normAA == aa2);

        //        //Point test = new Point(sq.X, sq.Y, sq.Z);
        //        //test.Normalize();
        //        //Trace.WriteLine(test);

        //        //Assert.AreEqual(q.W, sq.W, 0.000001, "Failed W");  // can't go very precise due to float imprecision in sys quat
        //        //Assert.AreEqual(q.X, sq.X, 0.000001, "Failed X");
        //        //Assert.AreEqual(q.Y, sq.Y, 0.000001, "Failed Y");
        //        //Assert.AreEqual(q.Z, sq.Z, 0.000001, "Failed Z");

        //    }
        //}

        //[TestMethod]
        //public void AxisAngle_To_Quaternion_WithSysComparison()
        //{
        //    AxisAngle aa1;
        //    Quaternion q;
        //    SysQuat sq;
        //    AxisAngle aa2;
        //    Point normV;
        //    AxisAngle normAA;

        //    double x, y, z, angle;
        //    //double len;
        //    //bool norm;

        //    // Test random quaternions
        //    for (var i = 0; i < 50; i++)
        //    {
        //        x = Random(-100, 100);
        //        y = Random(-100, 100);
        //        z = Random(-100, 100);
        //        angle = Random(-3 * 360, 3 * 360);

        //        aa1 = new AxisAngle(x, y, z, angle);
        //        //aa1.Normalize();
        //        q = aa1.ToQuaternion();
        //        sq = SysQuat.CreateFromAxisAngle(new Vector3((float)aa1.X, (float)aa1.Y, (float)aa1.Z), (float)(aa1.Angle * Math.PI / 180.0));  // This is giving me really strange results
        //        //aa2 = q.ToAxisAngle();

        //        Trace.WriteLine("");
        //        Trace.WriteLine(aa1);
        //        Trace.WriteLine(q);
        //        Trace.WriteLine(sq);
        //        //Trace.WriteLine(aa2);


        //        //normAA = new AxisAngle(x, y, z, angle);
        //        //normAA.Normalize();
        //        //Trace.WriteLine(normAA);

        //        //Assert.IsTrue(normAA == aa2);

        //        //Point test = new Point(sq.X, sq.Y, sq.Z);
        //        //test.Normalize();
        //        //Trace.WriteLine(test);

        //        //Assert.AreEqual(q.W, sq.W, 0.000001, "Failed W");  // can't go very precise due to float imprecision in sys quat
        //        //Assert.AreEqual(q.X, sq.X, 0.000001, "Failed X");
        //        //Assert.AreEqual(q.Y, sq.Y, 0.000001, "Failed Y");
        //        //Assert.AreEqual(q.Z, sq.Z, 0.000001, "Failed Z");

        //    }
        //}







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
