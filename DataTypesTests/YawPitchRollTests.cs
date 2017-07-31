using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;
using System.Collections.Generic;

namespace DataTypesTests
{
    [TestClass]
    public class YawPitchRollTests : DataTypesTests
    {
        [TestMethod]
        public void YawPitchRoll_ToQuaternion_ToYawPitchRoll()
        {
            YawPitchRoll eu1, eu2, eu3;
            Quaternion q1, q2, q3;

            double x, y, z;

            // Amp this up to 100000 cycles to hit singularities
            for (var i = 0; i < 200; i++)
            {
                x = Random(-1440, 1440);
                y = Random(-1440, 1440);
                z = Random(-1440, 1440);

                eu1 = new YawPitchRoll(x, y, z);
                q1 = eu1.ToQuaternion();
                eu2 = q1.ToYawPitchRoll();
                q2 = eu2.ToQuaternion();
                eu3 = q2.ToYawPitchRoll();
                q3 = eu3.ToQuaternion();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1);
                Trace.WriteLine(q1);
                Trace.WriteLine(eu2);
                Trace.WriteLine(q2);
                Trace.WriteLine(eu3);
                Trace.WriteLine(q3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "eus not equiv");
                Assert.IsTrue(eu2.IsSimilar(eu3), "Eulers not equal");
                Assert.IsTrue(q1.IsEquivalent(q2), "Quats not equiv");
                Assert.IsTrue(q2.IsSimilar(q3), "Quats not equal");
            }

            // Try orthogonal configurations
            for (var i = 0; i < 500; i++)
            {
                x = 90 * RandomInt(-16, 16);
                y = 90 * RandomInt(-16, 16);
                z = 90 * RandomInt(-16, 16);

                eu1 = new YawPitchRoll(x, y, z);
                q1 = eu1.ToQuaternion();
                eu2 = q1.ToYawPitchRoll();
                q2 = eu2.ToQuaternion();
                eu3 = q2.ToYawPitchRoll();
                q3 = eu3.ToQuaternion();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1);
                Trace.WriteLine(q1);
                Trace.WriteLine(eu2);
                Trace.WriteLine(q2);
                Trace.WriteLine(eu3);
                Trace.WriteLine(q3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "eus not equiv");
                Assert.IsTrue(eu2.IsSimilar(eu3), "Eulers not equal");
                Assert.IsTrue(q1.IsEquivalent(q2), "Quats not equiv");
                Assert.IsTrue(q2.IsSimilar(q3), "Quats not equal");
            }


        }


        [TestMethod]
        public void YawPitchRoll_ToQuaternion_ToYawPitchRoll_Singularities()
        {
            YawPitchRoll eu1, eu2;
            Quaternion q1, q2;

            double x, y, z;
            int cycles = 200;

            for (var i = 0; i < cycles; i++)
            {
                x = Random(-180, 180);
                y = i > 0.5 * cycles ? Random(89.98, 90) : Random(-90, -89.98);
                z = Random(-180, 180);

                eu1 = new YawPitchRoll(x, y, z);
                q1 = eu1.ToQuaternion();
                eu2 = q1.ToYawPitchRoll();
                q2 = eu2.ToQuaternion();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1);
                Trace.WriteLine(q1);
                Trace.WriteLine(eu2);
                Trace.WriteLine(q2);

                /**
                 * When Y angle is very close to +-90, is close to singularities and snaps:
                    7.57141764628301 -89.9993071518835 58.6819062189581
                    EulerZYX[Z:58.681906, Y:-89.999307, X:7.571418]
                    Quaternion[0.592181, 0.386426, -0.592173, 0.38643]
                    EulerZYX[Z:66.252732, Y:-90, X:0]
                    Quaternion[0.592179, 0.386425, -0.592179, 0.386425]
                 * If so, check that that this is was the case, and that Quaternion values 
                 * are decently close to each other
                 **/
                if (eu1 != eu2)
                {
                    Trace.WriteLine("SINGULARITY");

                    Assert.IsTrue(y > 90 - 0.03 || y < -90 + 0.03);
                    // Is q == q or q = -q 
                    Assert.IsTrue(
                        (Math.Abs(q1.W - q2.W) < 0.001 && Math.Abs(q1.X - q2.X) < 0.001 && Math.Abs(q1.Y - q2.Y) < 0.001 && Math.Abs(q1.Z - q2.Z) < 0.001)
                        || (Math.Abs(q1.W + q2.W) < 0.001 && Math.Abs(q1.X + q2.X) < 0.001 && Math.Abs(q1.Y + q2.Y) < 0.001 && Math.Abs(q1.Z + q2.Z) < 0.001));
                }
                else
                {
                    Assert.IsTrue(eu1.IsSimilar(eu2), "Eulers not equal");
                    Assert.IsTrue(q1.IsSimilar(q2), "Quats not equal");
                }
            }
        }

        [TestMethod]
        public void YawPitchRoll_ToRotationMatrix_ToYawPitchRoll()
        {
            YawPitchRoll eu1, eu2, eu3;
            RotationMatrix m1, m2, m3;

            double x, y, z;

            // Amp this up to 100000 cycles to hit singularities
            for (var i = 0; i < 500; i++)
            {
                x = Random(-1440, 1440);
                y = Random(-1440, 1440);
                z = Random(-1440, 1440);

                eu1 = new YawPitchRoll(x, y, z);
                m1 = eu1.ToRotationMatrix();
                eu2 = m1.ToYawPitchRoll();
                m2 = eu2.ToRotationMatrix();
                eu3 = m2.ToYawPitchRoll();
                m3 = eu3.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1 + " " + eu1.ToQuaternion());
                Trace.WriteLine(m1);
                Trace.WriteLine(eu2 + " " + eu2.ToQuaternion());
                Trace.WriteLine(m2);
                Trace.WriteLine(eu3);
                Trace.WriteLine(m3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "Eulers1 not equiv");
                Assert.IsTrue(eu2.IsSimilar(eu3), "Eulers2 not equal");
                Assert.IsTrue(m1.IsSimilar(m2), "Matrices1 not equiv");
                Assert.IsTrue(m2.IsSimilar(m3), "Matrices2 not equal");
            }

            // Try orthogonal configurations
            for (var i = 0; i < 500; i++)
            {
                x = 90 * RandomInt(-16, 16);
                y = 90 * RandomInt(-16, 16);
                z = 90 * RandomInt(-16, 16);

                eu1 = new YawPitchRoll(x, y, z);
                m1 = eu1.ToRotationMatrix();
                eu2 = m1.ToYawPitchRoll();
                m2 = eu2.ToRotationMatrix();
                eu3 = m2.ToYawPitchRoll();
                m3 = eu3.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1 + " " + eu1.ToQuaternion());
                Trace.WriteLine(m1);
                Trace.WriteLine(eu2 + " " + eu2.ToQuaternion());
                Trace.WriteLine(m2);
                Trace.WriteLine(eu3 + " " + eu3.ToQuaternion());
                Trace.WriteLine(m3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "Eulers1 not equiv");
                Assert.IsTrue(eu2.IsEquivalent(eu3), "Eulers2 not equal");
                Assert.IsTrue(m1.IsSimilar(m2), "Matrices1 not equiv");
                Assert.IsTrue(m2.IsSimilar(m3), "Matrices2 not equal");
            }
        }


        [TestMethod]
        public void YawPitchRoll_ToRotationMAtrix_ToYawPitchRoll_Singularities()
        {
            YawPitchRoll eu1, eu2;
            RotationMatrix m1, m2;

            double x, y, z;
            int cycles = 500;

            for (var i = 0; i < cycles; i++)
            {
                x = Random(-180, 180);
                y = i > 0.5 * cycles ? Random(89.98, 90) : Random(-90, -89.98);
                z = Random(-180, 180);

                eu1 = new YawPitchRoll(x, y, z);
                m1 = eu1.ToRotationMatrix();
                eu2 = m1.ToYawPitchRoll();
                m2 = eu2.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1 + " " + eu1.ToQuaternion());
                Trace.WriteLine(m1);
                Trace.WriteLine(eu2 + " " + eu2.ToQuaternion());
                Trace.WriteLine(m2);

                if (eu1 != eu2)
                {
                    Trace.WriteLine("SINGULARITY");

                    Assert.IsTrue(y > 90 - 0.03 || y < -90 + 0.03);
                    // Is q == q or q = -q 
                    //Assert.IsTrue(eu1.ToQuaternion().IsEquivalent(eu2.ToQuaternion()));  // too precise of a comparison, yields errors

                    Quaternion q1 = eu1.ToQuaternion();
                    Quaternion q2 = eu2.ToQuaternion();
                    Assert.IsTrue(
                            (Math.Abs(q1.W - q2.W) < 0.001 && Math.Abs(q1.X - q2.X) < 0.001 && Math.Abs(q1.Y - q2.Y) < 0.001 && Math.Abs(q1.Z - q2.Z) < 0.001)
                            || (Math.Abs(q1.W + q2.W) < 0.001 && Math.Abs(q1.X + q2.X) < 0.001 && Math.Abs(q1.Y + q2.Y) < 0.001 && Math.Abs(q1.Z + q2.Z) < 0.001));
                }
                else
                {
                    Assert.IsTrue(eu1 == eu2, "Eulers not equal");
                    Assert.IsTrue(m1 == m2, "Quats not equal");
                }
            }
        }

        [TestMethod]
        public void YawPitchRoll_ToAxisAngle_ComparisonWithQuaternions()
        {
            YawPitchRoll eu1;
            AxisAngle aa1, aa2;
            Quaternion q;

            double x, y, z;

            // Amp this up to 100000 cycles to hit singularities
            for (var i = 0; i < 500; i++)
            {
                x = Random(-1440, 1440);
                y = Random(-1440, 1440);
                z = Random(-1440, 1440);

                eu1 = new YawPitchRoll(x, y, z);
                aa1 = eu1.ToAxisAngle();
                q = eu1.ToQuaternion();
                aa2 = q.ToAxisAngle();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1 + " --> " + aa1);
                Trace.WriteLine(eu1 + " --> " + q + " --> " + aa2);

                Assert.IsTrue(aa1.IsSimilar(aa2));

            }
        }



        [TestMethod]
        public void YawPitchRoll_ToAxisAngle_ToYawPitchRoll()
        {
            YawPitchRoll eu1, eu2, eu3;
            AxisAngle aa1, aa2, aa3;

            double x, y, z;

            // Amp this up to 100000 cycles to hit singularities
            for (var i = 0; i < 500; i++)
            {
                x = Random(-1440, 1440);
                y = Random(-1440, 1440);
                z = Random(-1440, 1440);

                eu1 = new YawPitchRoll(x, y, z);
                aa1 = eu1.ToAxisAngle();
                eu2 = aa1.ToYawPitchRoll();
                aa2 = eu2.ToAxisAngle();
                eu3 = aa2.ToYawPitchRoll();
                aa3 = eu3.ToAxisAngle();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine("    " + eu1 + " --> " + aa1);
                Trace.WriteLine("    " + eu2 + " --> " + aa2);
                Trace.WriteLine("    " + eu3 + " --> " + aa3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "Eulers1 not equiv");
                Assert.IsTrue(eu2.IsSimilar(eu3), "Eulers2 not equal");
                Assert.IsTrue(aa1.IsEquivalent(aa2), "AA1 not equal");
                Assert.IsTrue(aa2.IsSimilar(aa3), "AA2 not equal");
            }

            // Try orthogonal configurations
            for (var i = 0; i < 500; i++)
            {
                x = 90 * RandomInt(-16, 16);
                y = 90 * RandomInt(-16, 16);
                z = 90 * RandomInt(-16, 16);

                eu1 = new YawPitchRoll(x, y, z);
                aa1 = eu1.ToAxisAngle();
                eu2 = aa1.ToYawPitchRoll();
                aa2 = eu2.ToAxisAngle();
                eu3 = aa2.ToYawPitchRoll();
                aa3 = eu3.ToAxisAngle();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine("    " + eu1 + " --> " + aa1);
                Trace.WriteLine("    " + eu2 + " --> " + aa2);
                Trace.WriteLine("    " + eu3 + " --> " + aa3);

                Assert.IsTrue(eu1.IsEquivalent(eu2), "Eulers1 not equiv");
                Assert.IsTrue(eu2.IsEquivalent(eu3), "Eulers2 not equal");
                Assert.IsTrue(aa1.IsEquivalent(aa2), "AA1 not equal");
                Assert.IsTrue(aa2.IsEquivalent(aa3), "AA2 not equal");
            }
        }
    }
}
