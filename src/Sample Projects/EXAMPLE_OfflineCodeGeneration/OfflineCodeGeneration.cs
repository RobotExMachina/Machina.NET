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
            // Create a new instance of a Robot
            Robot arm = Robot.Create("OfflineBOT", "ABB");

            // Log stuff to the console
            arm.DebugMode(true);

            // Set mode to "offline" (default)
            arm.ControlMode("offline");

            // Let's trace a 100x100 square! 

            // From wherever the robot was, go back to homish position
            arm.SpeedTo(25);
            arm.PrecisionTo(5);
            arm.AxesTo(0, 0, 0, 0, 90, 0);  // This is specific to ABB, check

            // Lead into first corner of square
            arm.MotionMode("joint");
            arm.SpeedTo(100);
            arm.PrecisionTo(1);
            arm.TransformTo(400, 0, 400, -1, 0, 0, 0, 1, 0);

            // Draw a 100 side square with linear movements
            arm.MotionMode("linear");
            arm.SpeedTo(50);
            arm.Move(0, -100, 0);
            arm.Move(-100, 0, 0);
            arm.Move(0, 100, 0);
            arm.Move(100, 0, 0);

            // Go back home
            arm.SpeedTo(25);
            arm.PrecisionTo(5);
            arm.AxesTo(0, 0, 0, 0, 90, 0);

            // Check all pending Actions in the buffer
            arm.DebugBuffers();

            // Export buffered program to local file
            var program = arm.Compile();
            arm.SaveProgram(program, @"C:\square.prg");

            // Exit
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }
    }
}
