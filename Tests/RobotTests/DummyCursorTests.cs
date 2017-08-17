using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Machina;
using SysQuat = System.Numerics.Quaternion;
using SysVec = System.Numerics.Vector3;
using SysMatrix44 = System.Numerics.Matrix4x4;

namespace RobotTests
{

    [TestClass]
    public class DummyCursorTests : RobotTests
    {
        [TestMethod]
        public void DevTests()
        {
            //Robot bot = new Robot("foo", "ABB");
            bool printMatrix = false;

            DummyCursor cursor = new DummyCursor();
            Trace.WriteLine("");

            cursor.MoveGlobal(100, 0, 0);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveGlobal(0, 100, 0);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveGlobal(0, 0, 100);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);

            cursor.MoveLocal(100, 0, 0);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveLocal(0, 100, 0);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveLocal(0, 0, 100);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);

            Assert.IsTrue(cursor.TCP == new SysMatrix44(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 200, 200, 200, 1));

            Trace.WriteLine("");
            for (int i = 0; i < 9; i++)
            {
                cursor.RotateLocal(1, 0, 0, 10);
                Trace.WriteLine(cursor);
                if (printMatrix) Trace.WriteLine(cursor.TCP);
            }
            //Assert.IsTrue(cursor.TCP == new SysMatrix44(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 200, 200, 200, 1));

            Trace.WriteLine("");
            for (int i = 0; i < 9; i++)
            {
                cursor.RotateGlobal(0, 0, 1, 10);
                Trace.WriteLine(cursor);
                if (printMatrix) Trace.WriteLine(cursor.TCP);
            }
            //Assert.IsTrue(cursor.TCP == new SysMatrix44(0, 0, 1, 0, -1, 0, 0, 0, 0, -1, 0, 0, 200, 200, 200, 1));

            Trace.WriteLine("");
            for (int i = 0; i < 9; i++)
            {
                cursor.RotateLocal(1, 0, 0, 10);
                Trace.WriteLine(cursor);
                if (printMatrix) Trace.WriteLine(cursor.TCP);
            }
            //Assert.IsTrue(cursor.TCP == new SysMatrix44(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 200, 200, 200, 1));

            Trace.WriteLine("");
            for (int i = 0; i < 9; i++)
            {
                cursor.RotateGlobal(0, 0, 1, 10);
                Trace.WriteLine(cursor);
                if (printMatrix) Trace.WriteLine(cursor.TCP);
            }
            //Assert.IsTrue(cursor.TCP == new SysMatrix44(0, 0, 1, 0, -1, 0, 0, 0, 0, -1, 0, 0, 200, 200, 200, 1));

            Trace.WriteLine("");
            cursor.MoveGlobal(25, 0, 0);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveLocal(25, 0, 0); ;
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);

            cursor.MoveGlobal(0, 0, 25);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);
            cursor.MoveLocal(0, 0, 25);
            Trace.WriteLine(cursor);
            if (printMatrix) Trace.WriteLine(cursor.TCP);

            // IS THIS NOT WORKING DUE TO USING ROW-MAJOR NOTATION, WHICH MEANS PRE AND POST-MULTIPLICATION ARE REVERSED...???

        }


    }
}
