using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

namespace TEST_NewAPITests
{
    class Program
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();
            arm.ControlMode("offline");

            arm.MoveTo(200, 0, 500);
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 0);

            arm.DebugBuffer();


            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();


        }
    }
}
