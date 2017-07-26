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
        private double TO_RADS = Math.PI / 180.0;

        [TestMethod]
        public void YawPitchRoll_ToQuaternion_ToYawPitchRoll()
        {
            YawPitchRoll eu1, eu2;
            Quaternion q;

            double x, y, z;

            for (var i = 0; i < 200; i++)
            {
                x = Random(-180, 180);
                y = Random(-90, 90);
                z = Random(-180, 180);

                eu1 = new YawPitchRoll(x, y, z);
                q = eu1.ToQuaternion();
                eu2 = q.ToEulerZYX();

                Trace.WriteLine("");
                Trace.WriteLine(x + " " + y + " " + z);
                Trace.WriteLine(eu1);
                Trace.WriteLine(q);
                Trace.WriteLine(eu2);
            }


        }

    }
}
