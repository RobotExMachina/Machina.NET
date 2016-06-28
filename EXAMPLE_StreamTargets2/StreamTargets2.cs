using System;

using RobotControl;

namespace EXAMPLE_StreamTargets2
{
    class StreamTargets2
    {


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
            arm.SetVelocity(100);        // in mm/s
            arm.SetZone(5);             // in mm

            //// A horizontal rectangle
            //arm.MoveTo("home");         // internally, all targets are being stream on the fly to the controller whenever they get priority
            //arm.MoveTo(200, 200, 150);  // absolute movement
            //arm.Move(0, 50, 0);         // relative movement
            //arm.Move(50, 0, 0);
            //arm.Move(0, -50, 0);
            //arm.MoveTo("home");

            //arm.Stop();                 // this shouldn't be necessary, should come with Disconnect()

            arm.MoveTo("home");
            arm.MoveTo(200, 200, 150);  // absolute movement
            arm.MoveTo(250, 200, 150);
            arm.MoveTo(250, 250, 150);
            arm.MoveTo(200, 250, 150);
            arm.MoveTo(200, 200, 200);
            arm.MoveTo(250, 200, 200);
            arm.MoveTo(250, 250, 200);
            arm.MoveTo(200, 250, 200);
            arm.MoveTo(200, 200, 300);
            arm.MoveTo(250, 200, 300);
            arm.MoveTo(250, 250, 300);
            arm.MoveTo(200, 250, 300);

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
            arm.Stop();

            //Console.WriteLine("Press any key to EXIT the program...");
            //Console.ReadKey();
            //arm.Start();

            //Console.WriteLine("Press any key to EXIT the program...");
            //Console.ReadKey();
            //arm.Stop();

            //Console.WriteLine("Press any key to EXIT the program...");
            //Console.ReadKey();
            //arm.Start();

            //Console.WriteLine("Press any key to EXIT the program...");
            //Console.ReadKey();
            //arm.Stop();


            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
        }
    }
}
