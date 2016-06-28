using System;

using RobotControl;

namespace EXAMPLE_StreamTargets2
{
    class StreamTargets2
    {

        public static double leadSpeed = 300;
        public static double traceSpeed = 100;

        [MTAThread]
        static void Main(string[] args)
        {
            Robot arm = new Robot();

            // Set connection properties
            arm.ConnectionMode("online");
            arm.OnlineMode("stream");
            arm.Connect();

            // Start real-time streaming
            arm.Start();

            // Set some properties from here on
            arm.SetVelocity(traceSpeed);        // in mm/s
            arm.SetZone(20);             // in mm

            // A set of horizontal squares
            arm.MoveTo("home");          // a 'bbokmarked' target

            arm.SetVelocity(leadSpeed);
            arm.MoveTo(250, 250, 250);   // absolute movement
            arm.SetVelocity(traceSpeed);
            arm.Move(50, 0, 0);          // relative movement
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SetVelocity(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SetVelocity(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SetVelocity(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SetVelocity(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SetVelocity(leadSpeed);
            arm.Move(0, -50, 50);
            arm.SetVelocity(traceSpeed);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);

            arm.SetVelocity(leadSpeed);
            arm.MoveTo("home");

            Console.WriteLine("Press any key to STOP the program...");
            Console.ReadKey();
            arm.Stop();  // this shouldn't be neccessary, should come with Disconnect()
            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
        }
    }
}
