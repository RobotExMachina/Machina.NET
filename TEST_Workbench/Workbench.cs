using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace TEST_Workbench
{
    class Workbench
    {
        static void Main(string[] args)
        {
            //int inc = 50;
            Robot arm = new Robot();
            //arm.Mode("stream");
            //arm.Connect();
            //arm.Start();

            arm.Mode("offline");

            arm.SpeedTo(100);
            arm.MoveTo(300, 300, 200);

            arm.ZoneTo(2);
            arm.Move(100, 0);

            arm.Motion("joint");
            arm.Move(0, -200);
            arm.Rotate(0, 1, 0, -90);

            arm.Coordinates("local");
            arm.Move(0, 0, 50);     // doesnt workl
            arm.Move(0, 0, -50);
            arm.Move(50, 0);        // doesnt either
            arm.Move(-50, 0);
            arm.Move(0, 50);        // does work!
            arm.Move(0, -50);

            arm.Export(@"C:\test.mod");

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            //arm.Disconnect();
        }
    }
}
