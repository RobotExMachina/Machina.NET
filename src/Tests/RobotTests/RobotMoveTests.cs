using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using Machina.Types.Geometry;
using Machina.Types.Data;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;
using Machina.Types;

namespace RobotTests
{
    [TestClass]
    public class RobotMoveTests : RobotTests
    {
        [TestMethod]
        public void Robot_Move_Simple()
        {
            // Movement should be platform-agnostic.
            Robot bot = Robot.Create();

            List<SysVec> sysvecs = new List<System.Numerics.Vector3>();

            // Init some virtual cursors
            Vector botpos = bot.GetCurrentPosition();
            SysVec syspos = new SysVec((float)botpos.X, (float)botpos.Y, (float)botpos.Z);

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
            bot = Robot.Create();
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


        [TestMethod]
        public void Robot_Rotate_Simple()
        {
            Robot bot = Robot.Create("foo", "ABB");
            bot.MoveTo(300, 300, 300);

            Rotation ror = bot.GetCurrentRotation();
            Trace.WriteLine("");
            Trace.WriteLine(ror);
            Trace.WriteLine(ror.Q);

            bot.Rotate(0, 1, 0, -90);
            ror = bot.GetCurrentRotation();
            Trace.WriteLine(ror);
            Trace.WriteLine(ror.Q);

            Orientation ori = new Orientation(1, 1, 0, 1, -1, 0);
            Point pos = new Point(200, 200, 200);
            bot.TransformTo(ori, pos);
            ror = bot.GetCurrentRotation();
            Trace.WriteLine(ror);
            Trace.WriteLine(ror.Q);

            RobotProgram program = bot.Compile();
            foreach (var line in program.ToStringList())
            {
                Trace.WriteLine(line);
            }
        }
    }
}
