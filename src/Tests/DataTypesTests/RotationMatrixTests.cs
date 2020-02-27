using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using Machina.Types.Geometry;
using SysMatrix44 = System.Numerics.Matrix4x4;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;

namespace DataTypesTests
{

    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗███╗   ███╗ █████╗ ████████╗██████╗ ██╗██╗  ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║████╗ ████║██╔══██╗╚══██╔══╝██╔══██╗██║╚██╗██╔╝
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║██╔████╔██║███████║   ██║   ██████╔╝██║ ╚███╔╝ 
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║██║╚██╔╝██║██╔══██║   ██║   ██╔══██╗██║ ██╔██╗ 
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║██║██╔╝ ██╗
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═╝
    //                                                                                                                  

    [TestClass]
    public class RotationMatrixTests : DataTypesTests
    {


        [TestMethod]
        public void RotationMatrix_OrthogonalOnCreation_RandomValues()
        {
            RotationMatrix m;

            double[] r = new double[9];

            for (var i = 0; i < 50; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    r[j] = Random(-100, 100);
                }

                m = new RotationMatrix(r);

                Trace.WriteLine("");
                for (int j = 0; j < 9; j++)
                {
                    Trace.Write(r[j] + " ");
                }
                Trace.WriteLine("");
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");
            }

            for (var i = 0; i < 50; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    r[j] = RandomInt(-1, 1);
                }

                m = new RotationMatrix(r);

                Trace.WriteLine("");
                for (int j = 0; j < 9; j++)
                {
                    Trace.Write(r[j] + " ");
                }
                Trace.WriteLine("");
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");
            }
        }

        [TestMethod]
        public void RotationMatrix_OrthogonalOnCreation_RandomVectors()
        {
            RotationMatrix m;
            Vector vecX, vecY;
            Vector mVecX, mVecY, mVecZ;
            int dir;

            for (var i = 0; i < 50; i++)
            {
                vecX = Vector.RandomFromDoubles(-100, 100);
                vecY = Vector.RandomFromDoubles(-100, 100);
                m = new RotationMatrix(vecX, vecY);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY);
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");

                mVecX = new Vector(m.m00, m.m10, m.m20);
                Assert.IsTrue(Vector.CompareDirections(vecX, mVecX) == 1, "Original VectorX and orthogonalized one are not parallel");
            }

            for (var i = 0; i < 100; i++)
            {
                vecX = Vector.RandomFromInts(-1, 1);
                vecY = Vector.RandomFromInts(-1, 1);
                dir = Vector.CompareDirections(vecX, vecY);

                m = new RotationMatrix(vecX, vecY);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m);

                if (dir == 1 || dir == 3)
                {
                    Assert.IsTrue(m.IsIdentity(), "Parallel vectors should yield an identity matrix");
                }
                else
                {
                    Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");

                    mVecX = new Vector(m.m00, m.m10, m.m20);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecX) == 1, "Original VectorX and orthogonalized X should be parallel");

                    mVecY = new Vector(m.m01, m.m11, m.m21);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecY) == 2, "Original VectorX and orthogonalized Y should be perpendicular");

                    mVecZ = new Vector(m.m02, m.m12, m.m22);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecZ) == 2, "Original VectorX and orthogonalized Z should be perpendicular");

                    Assert.IsTrue(Vector.CompareDirections(mVecX, mVecY) == 2);
                    Assert.IsTrue(Vector.CompareDirections(mVecX, mVecZ) == 2);
                    Assert.IsTrue(Vector.CompareDirections(mVecY, mVecZ) == 2);
                }
            }
        }

        [TestMethod]
        public void RotationMatrix_OrthogonalOnCreation_ZeroVector()
        {
            RotationMatrix m = new RotationMatrix(0, 0, 0, 0, 0, 0, 0, 0, 0);
            Assert.IsTrue(m.IsIdentity());

        }

        [TestMethod]
        public void RotationMatrix_ToQuaternion_VsSystemNumerics()
        {
            RotationMatrix m;
            Quaternion q;
            SysMatrix44 m44, m44bis;
            SysQuat sq;
            Vector vecX, vecY;
            int dir;

            int runs = 500;
            for (var i = 0; i < runs; i++)
            {
                if (i < 0.5 * runs)
                {
                    vecX = Vector.RandomFromDoubles(-100, 100);
                    vecY = Vector.RandomFromDoubles(-100, 100);
                }
                else
                {
                    vecX = Vector.RandomFromInts(-1, 1);
                    vecY = Vector.RandomFromInts(-1, 1);
                }

                dir = Vector.CompareDirections(vecX, vecY);
                m = new RotationMatrix(vecX, vecY);
                q = m.ToQuaternion();

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m);
                Trace.WriteLine(q);

                // Use the matrix's orthogonal values to create a M44 matrix with just rotation values:
                m44 = new SysMatrix44((float)m.m00, (float)m.m01, (float)m.m02, 0,
                                        (float)m.m10, (float)m.m11, (float)m.m12, 0,
                                        (float)m.m20, (float)m.m21, (float)m.m22, 0,
                                        0, 0, 0, 1);
                m44 = SysMatrix44.Transpose(m44);  // Numerics.Matrix4x4 uses a transposed convention, meaning the translation vector is horizontal in m41-42-43 instead of vertical in m14-24-34
                sq = SysQuat.CreateFromRotationMatrix(m44);
                m44bis = SysMatrix44.CreateFromQuaternion(sq);
                Trace.WriteLine(m44);
                Trace.WriteLine(sq);
                Trace.WriteLine(m44bis);

                Assert.IsTrue(q.IsEquivalent(new Quaternion(sq.W, sq.X, sq.Y, sq.Z)), "Quaternions are not equivalent!");
            }
        }


        // @TODO: design a test with matrices with very low traces...
        [TestMethod]
        public void RotationMatrix_ToQuaternion_LowTrace()
        {
            RotationMatrix m = new RotationMatrix(new Vector(0, 1, 0), new Vector(-1, 0, 0));
            Quaternion q = m.ToQuaternion();
            RotationMatrix m1 = q.ToRotationMatrix();

            Assert.IsTrue(m.IsSimilar(m1));
        }


        [TestMethod]
        public void RotationMatrix_ToQuaternion_ToRotationMatrix()
        {
            RotationMatrix m1, m2;
            Quaternion q;
            Vector vecX, vecY;
            int dir;

            double[] r = new double[9];

            for (var i = 0; i < 100; i++)
            {
                vecX = Vector.RandomFromDoubles(-100, 100);
                vecY = Vector.RandomFromDoubles(-100, 100);
                dir = Vector.CompareDirections(vecX, vecY);

                m1 = new RotationMatrix(vecX, vecY);
                q = m1.ToQuaternion();
                m2 = q.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m1);
                Trace.WriteLine(q);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilar(m2));
            }
        }

        [TestMethod]
        public void RotationMatrix_ToAxisAngle_ToRotationMatrix()
        {

            RotationMatrix m1, m2;
            AxisAngle aa1, aa2;

            double x, y, z, angle;
            Vector axis;

            // Test random permutations
            for (var i = 0; i < 200; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);  // test any possible angle

                aa1 = new AxisAngle(x, y, z, angle);  // a random AA is easier to create than a random RM
                m1 = aa1.ToRotationMatrix();
                aa2 = m1.ToAxisAngle();
                m2 = aa2.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine(aa1);
                Trace.WriteLine(m1);
                Trace.WriteLine(aa2);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilar(m2));
                Assert.IsTrue(aa1.IsEquivalent(aa2));  // just for the sake of it, not the point of this test ;)
            }


            // Test singularities
            for (var i = 0; i < 1000; i++)
            {
                axis = Vector.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-8, 8);

                aa1 = new AxisAngle(axis, angle);  // a random AA is easier to create than a random RM
                m1 = aa1.ToRotationMatrix();
                aa2 = m1.ToAxisAngle();
                m2 = aa2.ToRotationMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine(aa1);
                Trace.WriteLine(m1);
                Trace.WriteLine(aa2);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilar(m2));
                Assert.IsTrue(aa1.IsEquivalent(aa2));  // just for the sake of it, not the point of this test ;)
            }
        }

        [TestMethod]
        public void RotationMatrix_ToYawPitchRoll_ToRotationMatrix()
        {

            RotationMatrix m1, m2, m3;
            YawPitchRoll eu1, eu2, eu3;
            AxisAngle aa;

            double x, y, z, angle;
            Vector axis;

            // Test random permutations
            for (var i = 0; i < 200; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);
                angle = Random(-1440, 1440);  // test any possible angle

                aa = new AxisAngle(x, y, z, angle);  // a random AA is easier to create than a random RM
                m1 = aa.ToRotationMatrix();
                eu1 = m1.ToYawPitchRoll();
                m2 = eu1.ToRotationMatrix();
                eu2 = m2.ToYawPitchRoll();
                m3 = eu2.ToRotationMatrix();
                eu3 = m3.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine(aa);
                Trace.WriteLine(m1);
                Trace.WriteLine(eu1);
                Trace.WriteLine(m2);
                Trace.WriteLine(eu2);
                Trace.WriteLine(m3);
                Trace.WriteLine(eu3);

                Assert.IsTrue(m1.IsSimilar(m2));
                Assert.IsTrue(m2.IsSimilar(m3));
                Assert.IsTrue(eu1.IsEquivalent(eu2));
                Assert.IsTrue(eu2.IsSimilar(eu3));
            }


            // Test singularities
            for (var i = 0; i < 500; i++)
            {
                axis = Vector.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-16, 16);

                aa = new AxisAngle(axis, angle);  // a random AA is easier to create than a random RM
                m1 = aa.ToRotationMatrix();
                eu1 = m1.ToYawPitchRoll();
                m2 = eu1.ToRotationMatrix();
                eu2 = m2.ToYawPitchRoll();
                m3 = eu2.ToRotationMatrix();
                eu3 = m3.ToYawPitchRoll();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine(aa);
                Trace.WriteLine(m1);
                Trace.WriteLine(eu1);
                Trace.WriteLine(m2);
                Trace.WriteLine(eu2);
                Trace.WriteLine(m3);
                Trace.WriteLine(eu3);

                Assert.IsTrue(m1.IsSimilar(m2));
                Assert.IsTrue(m2.IsSimilar(m3));
                Assert.IsTrue(eu1.IsEquivalent(eu2));
                Assert.IsTrue(eu2.IsSimilar(eu3));
            }
        }

        [TestMethod]
        public void RotationMatrix_FromOrientation_MaintainParallelism()
        {
            Orientation ori;

            double x0, y0, z0, x1, y1, z1;
            Vector xAxis, yAxis;
            int dir;

            // Test random permutations
            for (var i = 0; i < 200; i++)
            {
                x0 = Random(-100, 100);
                y0 = Random(-100, 100);
                z0 = Random(-100, 100);
                x1 = Random(-100, 100);
                y1 = Random(-100, 100);
                z1 = Random(-100, 100);

                ori = new Orientation(x0, y0, z0, x1, y1, z1);
                xAxis = new Vector(x0, y0, z0);

                Trace.WriteLine("");
                Trace.WriteLine(x0 + " " + y0 + " " + z0 + " " + x1 + " " + y1 + " " + z1);
                Trace.WriteLine(ori);

                Assert.IsTrue(Vector.AreParallel(xAxis, ori.XAxis));

            }

            // Test orthogonal vectors
            for (var i = 0; i < 200; i++)
            {
                xAxis = Vector.RandomFromInts(-1, 1);
                yAxis = Vector.RandomFromInts(-1, 1);
                ori = new Orientation(xAxis, yAxis);
                dir = Vector.CompareDirections(xAxis, yAxis);

                Trace.WriteLine("");
                Trace.WriteLine(xAxis + " " + yAxis + " dir: " + dir);
                Trace.WriteLine(ori);

                // If parallel or opposite, rotation matrix should be identity...
                if (dir == 1 || dir == 3)
                {
                    Assert.IsTrue(ori.XAxis.IsSimilar(Vector.XAxis));
                }
                else
                {
                    Assert.IsTrue(Vector.AreParallel(xAxis, ori.XAxis));
                }
            }
        }
    }
}
