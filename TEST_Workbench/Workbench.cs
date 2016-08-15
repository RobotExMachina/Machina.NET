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
            int inc = 50;
            Robot arm = new Robot();
            arm.Mode("stream");
            arm.Connect();
            arm.Start();

            arm.SpeedTo(100);
            arm.MoveTo(300, 300, 200);
            arm.Move(50, 0);

            //arm.Execute();

            for (int i = 0; i < 6; i++)
            {
                arm.Speed(inc);
                arm.Move(0, -50);
                arm.Move(0, 0, 10);
            }
            

            //arm.Export(@"C:\test.mod");
            //arm.Execute();

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();
        }
    }
}
