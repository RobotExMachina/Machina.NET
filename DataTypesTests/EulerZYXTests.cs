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
    public class EulerZYXTests : DataTypesTests
    {
        private double TO_RADS = Math.PI / 180.0;

        [TestMethod]
        public void EulerZYX_ToQuaternion_VsSystemNumerics()
        {
            EulerZYX eu;
            Quaternion q;
            SysQuat sq;

            double x, y, z;

            for (var i = 0; i < 200; i++)
            {
                x = Random(-1440, 1440);
                y = Random(-1440, 1440);
                z = Random(-1440, 1440);

                eu = new EulerZYX(x, y, z);
                q = eu.ToQuaternion();

                // What freaking convention is System Quaternion using for YawPitchRoll??
                // Explained in the Rotation section here: https://www.codeproject.com/Articles/17425/A-Vector-Type-for-C
                //sq = SysQuat.CreateFromYawPitchRoll((float) (z * TO_RADS), (float)(y * TO_RADS), (float)(x * TO_RADS));
                sq = SysQuat.CreateFromYawPitchRoll((float)(y * TO_RADS), (float)(x * TO_RADS), -(float)(z * TO_RADS));

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu);
                Trace.WriteLine(q);
                Trace.WriteLine(sq);


            }
        }

    }
}
