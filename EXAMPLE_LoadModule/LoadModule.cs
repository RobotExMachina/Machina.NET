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
        static string moduleFilePath = @"D:\temp\simple_line.mod";
        //static string moduleFilePath = @"D:\temp\3d_circles.mod";

        [MTAThread] // "For an application running in a Multi Threaded Apartment (MTA) the Dispose call will remove both managed and native objects"
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing...");
            Robot arm = new Robot();
            arm.Connect();

            Console.WriteLine("Press any key to LOAD module '" + moduleFilePath + "'...");
            Console.ReadKey();
            arm.LoadModule(moduleFilePath);

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
