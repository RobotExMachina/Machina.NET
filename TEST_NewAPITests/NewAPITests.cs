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
            
            //TracePlanarRectangle(arm);

            TraceYLine(arm, false);
            TraceYLine(arm, true);
            
            arm.DebugBuffer();

            arm.DebugWritePointer();
            arm.Export(@"C:\offlineTests.mod");
            arm.DebugWritePointer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();


        }

        static public void TracePlanarRectangle(Robot arm)
        {
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
        }

        static public void TraceYLine(Robot arm, bool jointMovement)
        {
            if (jointMovement) arm.SetMotionType("joint");

            arm.SetVelocity(100);
            arm.SetZone(1);
            arm.MoveTo(300, -200, 500);
            arm.Move(0, 378, 0);

            if (jointMovement) arm.SetMotionType("linear");  // back to where it was... this will improve with arm.PushSettings(); 
        }


    }
}
