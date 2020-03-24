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
        static Robot bot;
        static RobotModel robotModel;

        static void Main(string[] args)
        {
            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;

            //List<double> target = new List<double> { 0, 0, 0, 0, 90, 0 };
            List<double> target = new List<double> { 72.474, 67.689, -126.868, 0, -30.821, 287.526 };

            robotModel = RobotModel.CreateABBIRB140();
            var frames = robotModel.ForwardKinematics(target, Units.Degrees);

            //var it = 0;
            //foreach (var m in frames)
            //{
            //    Console.WriteLine(it);
            //    Console.WriteLine(m);
            //    it++;
            //}

            Console.WriteLine(Plane.CreateFromMatrix(frames[frames.Count - 1]));

            bot = Robot.Create("FKTest", "ABB");
            bot.ConnectionManager(ConnectionType.Machina);
            bot.ControlMode(ControlType.Online);
            bot.Connect();

            bot.SolutionFKReceived += Arm_SolutionFKReceived;

            bot.Message("FK TEST STARTING");

            for (int i = 0; i < 25; i++)
            {
                Axes a = Axes.RandomFromDoubles(-400, 400);
                string msg = "20 " + a.ToWhitespacedValues();
                bot.CustomCode(msg);
            }

            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            bot.Disconnect();

            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        private static void Arm_SolutionFKReceived(object sender, Machina.EventArgs.SolutionFKReceivedArgs args)
        {
            var frames = robotModel.ForwardKinematics(args.Axes.ToList(), Units.Degrees);
            var tcp = frames.Last();
            Console.WriteLine($"Robot: {args.TCP.ToArrayString(6)}\n   FK: {tcp.ToArrayString(6)}");
            Console.WriteLine($"SIMILAR: {args.TCP.IsSimilarTo(tcp, 0.001)}");

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
