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

            arm.DebugVirtualPointer();

            arm.MoveTo(100, 0, 100);
            arm.DebugVirtualPointer();
            
            arm.SetVelocity(100);
            arm.SetZone(1);
            arm.MoveTo(200, 0, 500);
            arm.DebugVirtualPointer();

            arm.Move(50, 0, 0);
            arm.DebugVirtualPointer();

            arm.Move(0, 50, 0);
            arm.DebugVirtualPointer();

            arm.Move(-50, 0, 0);
            arm.DebugVirtualPointer();

            arm.Move(0, -50, 0);
            arm.DebugVirtualPointer();

            arm.SetVelocity(200);
            arm.SetZone(10);
            arm.MoveTo(300, 50, 300);
            arm.DebugVirtualPointer();

            arm.DebugBuffer();


            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();


        }
    }
}
