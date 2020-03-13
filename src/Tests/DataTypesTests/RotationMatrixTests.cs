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
        public void Matrix_Orthogonalization_RandomValues()
        {
            Matrix m;

            double[] r = new double[16];

            for (var i = 0; i < 50; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    r[j] = Random(-100, 100);
                }

                m = new Matrix(r);
                m = m.GetRotationMatrix();

                Trace.WriteLine("");
                for (int j = 0; j < 16; j++)
                {
                    Trace.Write(r[j] + " ");
                }
                Trace.WriteLine("PRE");
                Trace.WriteLine(m);

                
                if (!m.IsOrthogonalRotation)
                {
                    // Since the matrix was created from random double numbers, it is highly unlikely that X and Y will be purely parallel. 
                    // So, let's maintain this as a test.
                    Assert.IsTrue(m.OrthogonalizeRotation(), "Could not orthogonalize matrix.");
                }

                Trace.WriteLine("ORTHO");
                Trace.WriteLine(m);

                Assert.IsTrue(m.IsOrthogonalRotation, "Matrix didn't orthogonalize.");
            }

            for (var i = 0; i < 50; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    r[j] = RandomInt(-1, 1);
                }

                m = new Matrix(r);
                m = m.GetRotationMatrix();

                Trace.WriteLine("");
                for (int j = 0; j < 16; j++)
                {
                    Trace.Write(r[j] + " ");
                }
                Trace.WriteLine("PRE");
                Trace.WriteLine(m);

                // Since using 0 and 1, it is very likely to hit parallel/opposite vectors.
                bool isOrtho = true;
                if (!m.IsOrthogonalRotation)
                {
                    isOrtho = m.OrthogonalizeRotation();
                }
                
                Trace.WriteLine("ORTHO");
                Trace.WriteLine(m);

                if (isOrtho)
                {
                    Assert.IsTrue(m.IsOrthogonalRotation, "Matrix didn't orthogonalize.");
                }
                else
                {
                    Direction dir = Vector.CompareDirections(m.X, m.Y);
                    Assert.IsTrue(dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Opposite, "Matrix didn't orthogonalize vectors with direction " + dir);
                }

            }
        }

        [TestMethod]
        public void Matrix_Orthogonalization_RandomVectors()
        {
            Matrix m;
            Vector vecX, vecY;
            Vector mVecX, mVecY, mVecZ;
            Direction dir;
            bool success;

            for (var i = 0; i < 50; i++)
            {
                vecX = Vector.RandomFromDoubles(-100, 100);
                vecY = Vector.RandomFromDoubles(-100, 100);
                success = Matrix.CreateOrthogonal(vecX, vecY, out m);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY);
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonalRotation, "Matrix isn't orthogonal");

                mVecX = new Vector(m.M11, m.M21, m.M31);
                dir = Vector.CompareDirections(vecX, mVecX);
                Assert.IsTrue(dir == Direction.Parallel, "Original VectorX and orthogonalized one are not parallel");
            }

            for (var i = 0; i < 100; i++)
            {
                vecX = Vector.RandomFromInts(-1, 1);
                vecY = Vector.RandomFromInts(-1, 1);
                dir = Vector.CompareDirections(vecX, vecY);

                success = Matrix.CreateOrthogonal(vecX, vecY, out m);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m);

                if (dir == Direction.Invalid)
                {
                    Assert.IsTrue(vecX.IsZero || vecY.IsZero, "One of the vectors should be zero.");
                }
                else if (dir == Direction.Parallel || dir == Direction.Opposite)
                {
                    Assert.IsTrue(m.IsIdentity, "Parallel vectors should yield an identity matrix");
                }
                else
                {
                    Assert.IsTrue(m.IsOrthogonalRotation, "Matrix isn't orthogonal");

                    mVecX = new Vector(m.M11, m.M21, m.M31);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecX) == Direction.Parallel, "Original VectorX and orthogonalized X should be parallel");

                    mVecY = new Vector(m.M12, m.M22, m.M32);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecY) == Direction.Orthogonal, "Original VectorX and orthogonalized Y should be perpendicular");

                    mVecZ = new Vector(m.M13, m.M23, m.M33);
                    Assert.IsTrue(Vector.CompareDirections(vecX, mVecZ) == Direction.Orthogonal, "Original VectorX and orthogonalized Z should be perpendicular");

                    Assert.IsTrue(Vector.CompareDirections(mVecX, mVecY) == Direction.Orthogonal);
                    Assert.IsTrue(Vector.CompareDirections(mVecX, mVecZ) == Direction.Orthogonal);
                    Assert.IsTrue(Vector.CompareDirections(mVecY, mVecZ) == Direction.Orthogonal);
                }
            }
        }


        [TestMethod]
        public void Matrix_ToQuaternion_VsSystemNumerics()
        {
            Matrix m;
            Quaternion q;
            SysMatrix44 m44, m44bis;
            SysQuat sq;
            Vector vecX, vecY;
            Direction dir;
            bool success;

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
                success = Matrix.CreateOrthogonal(vecX, vecY, out m);
                Assert.IsTrue(m.GetQuaternion(out q), "Could not convert to Quaternion");

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m);
                Trace.WriteLine(q);

                // Use the matrix's orthogonal values to create a M44 matrix with just rotation values:
                m44 = new SysMatrix44((float)m.M11, (float)m.M12, (float)m.M13, 0,
                                        (float)m.M21, (float)m.M22, (float)m.M23, 0,
                                        (float)m.M31, (float)m.M32, (float)m.M33, 0,
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
        public void Matrix_ToQuaternion_LowTrace()
        {
            bool success = Matrix.CreateOrthogonal(new Vector(0, 1, 0), new Vector(-1, 0, 0), out Matrix m);
            Quaternion q;
            Assert.IsTrue(m.GetQuaternion(out q), "Could not convert to Quaternion");
            Matrix m1 = Matrix.CreateFromQuaternion(q);

            Assert.IsTrue(m.IsSimilarTo(m1, MMath.EPSILON3));
        }


        [TestMethod]
        public void Matrix_ToQuaternion_ToMatrix()
        {
            Matrix m1, m2;
            Quaternion q;
            Vector vecX, vecY;
            Direction dir;
            bool success;

            double[] r = new double[9];

            for (var i = 0; i < 100; i++)
            {
                vecX = Vector.RandomFromDoubles(-100, 100);
                vecY = Vector.RandomFromDoubles(-100, 100);
                dir = Vector.CompareDirections(vecX, vecY);

                success = Matrix.CreateOrthogonal(vecX, vecY, out m1);
                Assert.IsTrue(m1.GetQuaternion(out q), "Could not convert to Quaternion");
                m2 = Matrix.CreateFromQuaternion(q);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY + " dir:" + dir);
                Trace.WriteLine(m1);
                Trace.WriteLine(q);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilarTo(m2, MMath.EPSILON3));
            }
        }

        [TestMethod]
        public void Matrix_ToAxisAngle_ToMatrix()
        {

            Matrix m1, m2;
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
                m1 = aa1.ToMatrix();
                aa2 = m1.GetAxisAngle();
                m2 = aa2.ToMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z + " " + angle);
                Trace.WriteLine(aa1);
                Trace.WriteLine(m1);
                Trace.WriteLine(aa2);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilarTo(m2, MMath.EPSILON3));
                Assert.IsTrue(aa1.IsEquivalent(aa2));  // just for the sake of it, not the point of this test ;)
            }


            // Test singularities
            for (var i = 0; i < 1000; i++)
            {
                axis = Vector.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-8, 8);

                aa1 = new AxisAngle(axis, angle);  // a random AA is easier to create than a random RM
                m1 = aa1.ToMatrix();
                aa2 = m1.GetAxisAngle();
                m2 = aa2.ToMatrix();

                Trace.WriteLine("");
                Trace.WriteLine(axis + " " + angle);
                Trace.WriteLine(aa1);
                Trace.WriteLine(m1);
                Trace.WriteLine(aa2);
                Trace.WriteLine(m2);

                Assert.IsTrue(m1.IsSimilarTo(m2, MMath.EPSILON3));
                Assert.IsTrue(aa1.IsEquivalent(aa2));  // just for the sake of it, not the point of this test ;)
            }
        }

        [TestMethod]
        public void RotationMatrix_ToYawPitchRoll_ToRotationMatrix()
        {

            Matrix m1, m2, m3;
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
                m1 = aa.ToMatrix();
                eu1 = m1.ToYawPitchRoll();
                m2 = eu1.ToMatrix();
                eu2 = m2.ToYawPitchRoll();
                m3 = eu2.ToMatrix();
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

                Assert.IsTrue(m1.IsSimilarTo(m2, MMath.EPSILON2));
                Assert.IsTrue(m2.IsSimilarTo(m3, MMath.EPSILON2));
                Assert.IsTrue(eu1.IsEquivalent(eu2));
                Assert.IsTrue(eu2.IsSimilar(eu3));
            }


            // Test singularities
            for (var i = 0; i < 500; i++)
            {
                axis = Vector.RandomFromInts(-1, 1);
                angle = 90 * RandomInt(-16, 16);

                aa = new AxisAngle(axis, angle);  // a random AA is easier to create than a random RM
                m1 = aa.ToMatrix();
                eu1 = m1.ToYawPitchRoll();
                m2 = eu1.ToMatrix();
                eu2 = m2.ToYawPitchRoll();
                m3 = eu2.ToMatrix();
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

                Assert.IsTrue(m1.IsSimilarTo(m2, MMath.EPSILON2));
                Assert.IsTrue(m2.IsSimilarTo(m3, MMath.EPSILON2));
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
            Direction dir;

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
                if (dir == Direction.Invalid || dir == Direction.Parallel || dir == Direction.Opposite)
                {
                    Assert.IsTrue(ori.XAxis.IsSimilarTo(Vector.XAxis, MMath.EPSILON2));
                }
                else
                {
                    Assert.IsTrue(Vector.AreParallel(xAxis, ori.XAxis));
                }
            }
        }
    }
}
