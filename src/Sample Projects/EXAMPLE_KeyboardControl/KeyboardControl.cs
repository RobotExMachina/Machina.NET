using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;

namespace EXAMPLE_KeyboardControl
{
    class KeyboardControl
    {

        [MTAThread]
        static void Main(string[] args)
        {
            // Some program parameters
            int leadSpeed = 100;
            int moveSpeed = 50;
            double inc = 25;
            bool input = true;

            // Create a new instance of a Robot
            Robot arm = Robot.Create("JoggingBot", "ABB");

            // Set connection properties
            arm.ControlMode("online");

            // Let Machina try to connect to a robot on the network.
            arm.ConnectionManager("Machina");
            arm.Connect();

            //// Alternativelly, connect manually with known network parameters (local RobotStudio simulation in this case)
            //arm.Connect("127.0.0.1", 7000);
            
            // Move to the positive XYZ octant
            arm.SpeedTo(leadSpeed);
            arm.PrecisionTo(2);
            arm.TransformTo(400, 0, 400, -1, 0, 0, 0, 1, 0);  // Be careful with these coordinates, tweak according to robot size 

            arm.SpeedTo(moveSpeed);

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


            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();

        }
    }
}
