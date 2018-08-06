using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;

namespace EXAMPLE_OfflineCodeGeneration
{
    class OfflineCodeGeneration
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot("OfflineBOT", "HUMAN");
            arm.ControlMode("offline");

            // From wherever the robot was, go back to homish position
            arm.MotionMode("joint");
            arm.SpeedTo(25);
            arm.PrecisionTo(5);
            arm.MoveTo(300, 0, 500);

            // Lead into first corner of square
            arm.SpeedTo(100);
            arm.PrecisionTo(1);
            arm.MoveTo(400, 50, 150);

            // Draw a 100 side square with linear movements
            arm.MotionMode("linear");
            arm.SpeedTo(50);
            arm.Move(0, -100, 0);
            arm.Move(-100, 0, 0);
            arm.Move(0, 100, 0);
            arm.Move(100, 0, 0);

            // Go back to homish
            arm.SpeedTo(25);
            arm.MoveTo(300, 0, 500);

            // Check all pending Actions in the buffer
            arm.DebugBuffers();

            // Export buffered program to local file
            arm.Export(@"C:\square.script");

            // Exit
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }
    }
}
