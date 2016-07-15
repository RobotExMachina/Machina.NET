using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

namespace EXAMPLE_LoadModule
{

    class LoadModule
    {

        [MTAThread] // "For an application running in a Multi Threaded Apartment (MTA) the Dispose call will remove both managed and native objects"
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Load Module Program...");

            string moduleFilepath = @"D:\temp\simple_line.mod";

            Robot arm = new Robot();

            arm.ControlMode("execute");
            arm.Connect();

            arm.DebugDump();

            Console.WriteLine("Press any key to LOAD module '" + moduleFilepath + "'...");
            Console.ReadKey();
            //arm.LoadModule(moduleFilename, moduleFilepath);
            arm.LoadProgram(moduleFilepath);

            Console.WriteLine("Press any key to START program execution...");
            Console.ReadKey();
            arm.Start();

            Console.WriteLine("Press any key to STOP program execution...");
            Console.ReadKey();
            arm.Stop();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
            arm.Disconnect();
        }

    }
}
