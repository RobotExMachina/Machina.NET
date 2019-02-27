using System;

using Machina;

namespace EXAMPLE_StreamTargets2
{
    class StreamTargets2
    {

        [MTAThread]
        static void Main(string[] args)
        {
            int leadSpeed = 100;
            int traceSpeed = 50;

            Robot arm = Robot.Create("StreamingTest", "ABB");
            arm.DebugMode(true);

            // Set connection properties
            arm.ControlMode("online");
            arm.ConnectionManager(ConnectionType.Machina);
            arm.Connect();
            
            // Set some properties from here on
            arm.SpeedTo(traceSpeed);    // in mm/s
            arm.PrecisionTo(20);                // in mm

            // An ascending spiral of horizontal squares
            arm.MoveTo(400, 0, 400);

            arm.SpeedTo(leadSpeed);
            arm.MoveTo(250, 250, 250);      // absolute movement
            arm.SpeedTo(traceSpeed);
            arm.Move(50, 0, 0);             // relative movement
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SpeedTo(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SpeedTo(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SpeedTo(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SpeedTo(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SpeedTo(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SpeedTo(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SpeedTo(leadSpeed);
            arm.AxesTo(0, 0, 0, 0, 90, 0);

            Console.WriteLine("Press any key to STOP the program...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
        }
    }
}
