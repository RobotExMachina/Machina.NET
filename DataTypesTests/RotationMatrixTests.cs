using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
using SysMatrix44 = System.Numerics.Matrix4x4;
using SysQuat = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

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
            Point vecX, vecY;
            Point mVecX, mVecY, mVecZ;
            int dir;

            for (var i = 0; i < 50; i++)
            {
                vecX = Point.RandomFromDoubles(-100, 100);
                vecY = Point.RandomFromDoubles(-100, 100);
                m = new RotationMatrix(vecX, vecY);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY);
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");

                mVecX = new Point(m.m00, m.m10, m.m20);
                Assert.IsTrue(Point.CompareDirections(vecX, mVecX) == 1, "Original VectorX and orthogonalized one are not parallel");
            }

            for (var i = 0; i < 100; i++)
            {
                vecX = Point.RandomFromInts(-1, 1);
                vecY = Point.RandomFromInts(-1, 1);
                dir = Point.CompareDirections(vecX, vecY);

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

                    mVecX = new Point(m.m00, m.m10, m.m20);
                    Assert.IsTrue(Point.CompareDirections(vecX, mVecX) == 1, "Original VectorX and orthogonalized X should be parallel");

                    mVecY = new Point(m.m01, m.m11, m.m21);
                    Assert.IsTrue(Point.CompareDirections(vecX, mVecY) == 2, "Original VectorX and orthogonalized Y should be perpendicular");

                    mVecZ = new Point(m.m02, m.m12, m.m22);
                    Assert.IsTrue(Point.CompareDirections(vecX, mVecZ) == 2, "Original VectorX and orthogonalized Z should be perpendicular");

                    Assert.IsTrue(Point.CompareDirections(mVecX, mVecY) == 2);
                    Assert.IsTrue(Point.CompareDirections(mVecX, mVecZ) == 2);
                    Assert.IsTrue(Point.CompareDirections(mVecY, mVecZ) == 2);
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
        public void RotationMatrix_ToQuaternionConversion_CompareToNumericsLibrary()
        {
            RotationMatrix m;
            Quaternion q;
            SysMatrix44 m44;
            SysQuat sq;
            Point vecX, vecY;

            for (var i = 0; i < 50; i++)
            {
                vecX = Point.RandomFromDoubles(-100, 100);
                vecY = Point.RandomFromDoubles(-100, 100);
                m = new RotationMatrix(vecX, vecY);
                q = m.ToQuaternion();



            }
        }

        //public void RotationMatrix_ToQuaternion_ToRotationMatrix()
        //{
        //    RotationMatrix m1, m2;
        //    Quaternion q;

        //    double[] r = new double[9];

        //    for (var i = 0; i < 50; i++)
        //    {
        //        for (int j = 0; j < 9; j++)
        //        {
        //            r[j] = Random(-100, 100);
        //        }

        //        m1 = new RotationMatrix(r);


        //    }

        //}
    }
}
