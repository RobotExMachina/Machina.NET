using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

namespace EXAMPLE_KeyboardControl
{
    class KeyboardControl
    {
        public static double leadSpeed = 200;
        public static double moveSpeed = 10;
        public static double inc = 10;

        public static bool input = true;

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

            // Move to the positive XYZ octant
            arm.SetVelocity(leadSpeed);
            arm.SetZone(2);
            arm.MoveTo(250, 250, 250);

            arm.SetVelocity(moveSpeed);

            while (input)
            {
                Console.WriteLine("Press ASDW+QE to move the TCP, and ESC to exit...");
                ConsoleKey key = Console.ReadKey(true).Key;

                // Thinking of an orientation corresponding to an user facing the robot frontally
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    arm.Move(-inc, 0, 0);
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    arm.Move(inc, 0, 0);

                }
                else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A)
                {
                    arm.Move(0, -inc, 0);
                }
                else if (key == ConsoleKey.RightArrow || key == ConsoleKey.D)
                {
                    arm.Move(0, inc, 0);
                }
                else if (key == ConsoleKey.Q)
                {
                    arm.Move(0, 0, inc);
                }
                else if (key == ConsoleKey.E)
                {
                    arm.Move(0, 0, -inc);
                }
                else if (key == ConsoleKey.Escape)
                {
                    input = false;
                }

            }

            arm.Stop();
            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();

        }
    }
}
