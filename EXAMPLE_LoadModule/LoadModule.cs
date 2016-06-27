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
        static string moduleFilepath = @"D:\temp\";
        static string moduleFilename = "simple_line.mod"; 

        [MTAThread] // "For an application running in a Multi Threaded Apartment (MTA) the Dispose call will remove both managed and native objects"
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing...");
            Robot arm = new Robot();
            while (!arm.Connect()) { }
            arm.DebugDump();

            Console.WriteLine("Press any key to LOAD module '" + moduleFilepath + moduleFilename + "'...");
            Console.ReadKey();
            arm.LoadModule(moduleFilename, moduleFilepath);

            Console.WriteLine("Press any key to START program execution...");
            Console.ReadKey();
            arm.Start();

            Console.WriteLine("Press any key to STOP program execution...");
            Console.ReadKey();
            arm.Stop();
            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT the program...");
            Console.ReadKey();
        }

    }
}
