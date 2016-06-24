/*
 * ROBOTCONTROL - EXAMPLE: Connection Check
 * A super basic console app that scans the network for controllers,
 * connects to the first one avaliable and dumps a log of all available
 * data for that controller.
 * 
 * USAGE:
 * - Connect the computer to a real (e.g. IRC5) or virtual (e.g. RobotStudio) controller
 * - Run this app
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;


namespace EXAMPLE_ConnectionCheck
{
    class ConnectionCheck
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Connection Check");

            Robot arm = new Robot();
            arm.Connect();
            arm.DebugDump();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }
    }
}
