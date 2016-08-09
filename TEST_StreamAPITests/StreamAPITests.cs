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
        static Robot arm = new Robot();
        static Point dir = new Point(20, 0, 0);
        static int it = 0;
        static int maxTargets = 36;

        static void Main(string[] args)
        {
            // Set control mode
            arm.ControlMode("stream");

            // Connect to a controller
            arm.Connect();
            arm.Start();

            // Subscribe to BufferEmpty events
            arm.BufferEmpty += new BufferEmptyHandler(GenerateMovements);

            // Do some stuff
            arm.Speed(100);
            arm.Zone(5);
            arm.MoveTo(302, 0, 558);
            arm.MoveTo(300, -150, 300);
            arm.Speed(25);

            // From here on, the BufferEmptyHandler should take command ;)

            arm.DebugRobotCursors();
            arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();   

        }

        

        static public void GenerateMovements(object sender, EventArgs args)
        {
            if (it < maxTargets)
            {
                arm.Move(dir);
                dir.Rotate(Point.ZAxis, 10); 
            }
            else if (it == maxTargets)
            {
                arm.Speed(100);
                arm.MoveTo(302, 0, 558);
            }
            it++;
        }
    }
}
