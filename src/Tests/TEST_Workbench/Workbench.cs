using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;
using Machina.Types.Geometry;
using Machina.Descriptors.Models;


namespace TEST_Workbench
{
    class Workbench
    {
        static void Main(string[] args)
        {
            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;

            //List<double> target = new List<double> { 0, 0, 0, 0, 90, 0 };
            List<double> target = new List<double> { 72.474, 67.689, -126.868, 0, -30.821, 287.526 };

            RobotModel bot = RobotModel.CreateABBIRB140();
            var frames = bot.ForwardKinematics(target, Units.Degrees);

            //var it = 0;
            //foreach (var m in frames)
            //{
            //    Console.WriteLine(it);
            //    Console.WriteLine(m);
            //    it++;
            //}

            Console.WriteLine(Plane.CreateFromMatrix(frames[frames.Count - 1]));

            Robot arm = Robot.Create("FKTest", "ABB");
            arm.ConnectionManager(ConnectionType.Machina);
            arm.ControlMode(ControlType.Online);
            arm.Connect();

            arm.Message("FK TEST STARTING");

            for (int i = 0; i < 25; i++)
            {
                Axes a = Axes.RandomFromDoubles(-400, 400);
                string msg = "20 " + a.ToWhitespacedValues();
                arm.CustomCode(msg);
            }

            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        static void Scale(List<double> rots, int factor)
        {
            for (int i = 0; i < rots.Count; i++)
            {
                rots[i] *= factor;
            }
        }



    }
}
