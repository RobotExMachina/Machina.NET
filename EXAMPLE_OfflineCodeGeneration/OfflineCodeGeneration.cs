using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace EXAMPLE_OfflineCodeGeneration
{
    class OfflineCodeGeneration
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();
            arm.ControlMode("offline");

            // From wherever the robot was, go back to homish position
            arm.Motion("joint");
            arm.Speed(50);
            arm.Zone(5);
            arm.MoveTo(300, 0, 500);

            // Lead into first corner of square
            arm.Speed(200);
            arm.Zone(1);
            arm.MoveTo(400, 50, 150);

            // Draw a 100 side square with linear movements
            arm.Motion("linear");
            arm.Speed(100);
            arm.Move(0, -100, 0);
            arm.Move(-100, 0, 0);
            arm.Move(0, 100, 0);
            arm.Move(100, 0, 0);

            // Go back to homish
            arm.Speed(200);
            arm.MoveTo(300, 0, 500);

            // Check all pending Actions in the buffer
            arm.DebugBuffer();

            // Export buffered program to local file
            arm.Export(@"C:\square.mod");

            // Exit
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }
    }
}
