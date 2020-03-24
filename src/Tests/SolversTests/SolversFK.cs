using System;
using System.Collections.Generic;
using System.Linq;
using Machina;
using Machina.Descriptors.Models;
using Machina.Types.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolversTests
{
    [TestClass]
    public class SolversFK
    {
        static Robot bot;
        static RobotModel robotModel;
        static int it = 0;

        /// <summary>
        /// This test connects to a RobotStudio instance (or a real robot), 
        /// streams a bunch of random joint poses, gets FK solutions, 
        /// and uses a solver to check them. 
        /// </summary>
        [TestMethod]
        public void OnlineRobotStudioFKCheck()
        {
            robotModel = RobotModel.CreateABBIRB140();

            bot = Robot.Create("FKTest", "ABB");
            bot.ConnectionManager(ConnectionType.Machina);
            bot.ControlMode(ControlType.Online);
            bot.Connect();

            bot.SolutionFKReceived += Arm_SolutionFKReceived;

            bot.Message("FK TEST STARTING");

            for (int i = 0; i < 50; i++)
            {
                Axes a = Axes.RandomFromDoubles(-400, 400);
                string msg = "20 " + a.ToWhitespacedValues();
                bot.CustomCode(msg);
            }

            for (int i = 0; i < 50; i++)
            {
                Axes a = Axes.RandomFromInts(-400, 400);
                string msg = "20 " + a.ToWhitespacedValues();
                bot.CustomCode(msg);
            }

            // THIS DOESN'T WORK IN A TEST...
            //Console.WriteLine("Press any key to DISCONNECT...");
            //Console.ReadKey();

            //bot.Disconnect();

            //Console.WriteLine("Press any key to EXIT...");
            //Console.ReadKey();

            // Quick and terrible... :sweat_smile:
            while(it < 100) { }

            bot.Disconnect();
        }

        private static void Arm_SolutionFKReceived(object sender, Machina.EventArgs.SolutionFKReceivedArgs args)
        {
            var frames = robotModel.ForwardKinematics(args.Axes.ToList(), Units.Degrees);
            var tcp = frames.Last();
            Console.WriteLine($"Robot: {args.TCP.ToArrayString(6)}\n   FK: {tcp.ToArrayString(6)}");
            Assert.IsTrue(args.TCP.IsSimilarTo(tcp, 0.001), "FK solution differs from the robot's");

            it++;
        }
    }
}
