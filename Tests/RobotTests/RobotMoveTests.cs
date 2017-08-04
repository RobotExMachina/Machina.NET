using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;

namespace RobotTests
{
    [TestClass]
    public class RobotMoveTests : RobotTests
    {
        [TestMethod]
        public void Robot_Move_Simple()
        {
            // Movement should be platform-agnostic.
            Robot bot = new Robot();

            List<SysVec> sysvecs = new List<System.Numerics.Vector3>();

            // Init some virtual cursors
            Vector botpos = bot.GetCurrentPosition();
            SysVec syspos = new SysVec((float) botpos.X, (float) botpos.Y, (float) botpos.Z);

            double x, y, z;

            // Test random movements
            for (var i = 0; i < 100; i++)
            {
                x = Random(-100, 100);
                y = Random(-100, 100);
                z = Random(-100, 100);

                bot.Move(x, y, z);
                botpos = bot.GetCurrentPosition();
                syspos += new SysVec((float)x, (float)y, (float)z);

                Trace.WriteLine("");
                Trace.WriteLine("#" + i + " " + x + " " + y + " " + z);
                Trace.WriteLine(botpos);
                Trace.WriteLine(syspos);

                Assert.IsTrue(AreSimilar(botpos, syspos));
                
            }

            // Reset the robot
            bot = new Robot();
            botpos = bot.GetCurrentPosition();
            syspos = new SysVec((float)botpos.X, (float)botpos.Y, (float)botpos.Z);


            // Try orthogonal configurations
            for (var i = 0; i < 100; i++)
            {
                x = 100 * RandomInt(-1, 1);
                y = 100 * RandomInt(-1, 1);
                z = 100 * RandomInt(-1, 1);

                bot.Move(x, y, z);
                botpos = bot.GetCurrentPosition();
                syspos += new SysVec((float)x, (float)y, (float)z);

                Trace.WriteLine("");
                Trace.WriteLine("#" + i + " " + x + " " + y + " " + z);
                Trace.WriteLine(botpos);
                Trace.WriteLine(syspos);

                Assert.IsTrue(AreSimilar(botpos, syspos));
            }

        }
    }
}
