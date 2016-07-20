using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

namespace TEST_NewAPITests
{
    class Program
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();
            arm.ControlMode("offline");

            arm.SetVelocity(200);
            arm.SetZone(20);
            arm.MoveTo(300, 300, 300);

            arm.SetVelocity(50);
            arm.SetZone(2);  // non predef zone
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 50);

            arm.SetVelocity(500);
            arm.SetZone(10);
            arm.MoveTo(300, 0, 500);
            
            arm.DebugBuffer();

            arm.DebugWritePointer();
            arm.Export(@"C:\offlineTests.mod");
            arm.DebugWritePointer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();


        }
    }
}
