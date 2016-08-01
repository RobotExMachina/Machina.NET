using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace TEST_StreamAPITests
{
    class StreamAPITests
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();

            // Set control mode
            arm.ControlMode("stream");

            // Connect to a controller
            arm.Connect();
            arm.Start();

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            // Do some stuff
            arm.Speed(100);
            arm.Zone(5);
            arm.MoveTo(300, 100, 500);

            arm.MoveTo(200, 200, 200);
            for (int i = 0; i < 5; i++)
            {
                arm.Move(50, 0);
                arm.Move(0, 50);
                arm.Move(-50, 0);
                //arm.Move(0, -50);
                arm.Move(0, -50, 50);
            }
            arm.MoveTo(300, 100, 500);

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();
        }
    }
}
