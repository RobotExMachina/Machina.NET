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

using Machina;


namespace EXAMPLE_ConnectionCheck
{
    class ConnectionCheck
    {
        [MTAThread] // "For an application running in a Multi Threaded Apartment (MTA) the Dispose call will remove both managed and native objects"
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Connection Check Program");

            // Create a new instance of a Robot
            Robot arm = Robot.Create("ConnectionTest", "ABB");

            // Dumps all Log messages to the Console.
            arm.DebugMode(true);

            // Set connection mode to "online"
            arm.ControlMode("online");

            // Let Machina try to figure out the connection parameters. 
            arm.ConnectionManager("Machina");

            // Let Machina try to connect to a robot on the network
            arm.Connect();

            // At this point, a lot of information about the robot should be displayed on the console. 

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            arm.Disconnect();
            
            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }
    }
}
