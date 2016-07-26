using System;

using RobotControl;

namespace EXAMPLE_StreamTargets2
{
    class StreamTargets2
    {

        [MTAThread]
        static void Main(string[] args)
        {
            int leadSpeed = 100;
            int traceSpeed = 50;

            Robot arm = new Robot();

            // Set connection properties
            arm.ControlMode("stream");
            arm.Connect();

            arm.DebugDump();

            // Start real-time streaming
            arm.Start();

            // Set some properties from here on
            arm.Speed(traceSpeed);    // in mm/s
            arm.Zone(20);                // in mm

            // A set of horizontal squares
            //arm.MoveTo("home");             // a 'bookmarked' target
            arm.MoveTo(300, 0, 500);

            arm.Speed(leadSpeed);
            arm.MoveTo(250, 250, 250);      // absolute movement
            arm.Speed(traceSpeed);
            arm.MoveGlobal(50, 0, 0);             // relative movement
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(-50, 0, 0);

            arm.Speed(leadSpeed);
            arm.MoveGlobal(0, -50, 50);
            arm.Speed(traceSpeed);
            arm.MoveGlobal(50, 0, 0);
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(-50, 0, 0);

            arm.Speed(leadSpeed);
            arm.MoveGlobal(0, -50, 50);
            arm.Speed(traceSpeed);
            arm.MoveGlobal(50, 0, 0);
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(-50, 0, 0);

            arm.Speed(leadSpeed);
            arm.MoveGlobal(0, -50, 50);
            arm.Speed(traceSpeed);
            arm.MoveGlobal(50, 0, 0);
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(-50, 0, 0);

            arm.Speed(leadSpeed);
            //arm.MoveTo("home");
            arm.MoveTo(300, 0, 500);

            Console.WriteLine("Press any key to STOP the program...");
            Console.ReadKey();
            arm.Stop();  // this shouldn't be neccessary, should come with Disconnect()
            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
        }
    }
}
