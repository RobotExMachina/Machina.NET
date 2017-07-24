using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using BRobot;
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
                Trace.WriteLine(r);
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");
            }
        }

        [TestMethod]
        public void RotationMatrix_OrthogonalOnCreation_RandomVectors()
        {
            RotationMatrix m;
            Point vecX, vecY;
            Point mVecX;

            for (var i = 0; i < 50; i++)
            {
                vecX = Point.RandomDouble(-100, 100);
                vecY = Point.RandomDouble(-100, 100);
                m = new RotationMatrix(vecX, vecY);

                Trace.WriteLine("");
                Trace.WriteLine(vecX + " " + vecY);
                Trace.WriteLine(m);
                Assert.IsTrue(m.IsOrthogonal(), "RotationMatrix isn't orthogonal");

                mVecX = new Point(m.m00, m.m10, m.m20);

                Assert.IsTrue(Point.CompareDirections(vecX, mVecX) == 1, "Original VectorX and orthogonalized one are not parallel");

            }

        }

        [TestMethod]
        public void RotationMatrix_ToQuaternionConversion()
        {
            RotationMatrix m;
            Quaternion q;


        }
    }
}
