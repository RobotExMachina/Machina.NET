using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;
using System.Threading;

namespace TEST_StreamAPITests
{
    class StreamAPITests
    {
        //static Robot arm = new Robot();
        //static Point dir = new Point(20, 0, 0);
        //static int it = 0;
        //static int maxTargets = 36;

        static bool PHYSICAL_ROBOT = false;

        static public void LogEvent(object sender, EventArgs args)
        {
            Console.WriteLine("EVENT RAISED");
            Console.WriteLine(sender);
            Console.WriteLine(args);
        }

        static void Main(string[] args)
        {
            Robot arm = Robot.Create("StreamTests", "ABB");

            arm.BufferEmpty += LogEvent;

            arm.ConnectionManager("machina");
            arm.ControlMode("stream");
            arm.SetUser("BUILD", "password");
            arm.Connect();

            //arm.ControlMode("stream");
            //arm.Connect("127.0.0.1", 7000);

            arm.Message("Hello Robot!");

            //arm.Start();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to START THE VERTICAL SQUARE...");
            Console.ReadKey();
            VerticalSquare(arm);

            //Console.WriteLine(" ");
            //Console.WriteLine("Press any key to START THE SPIRAL...");
            //Console.ReadKey();
            //Spiral(arm, 5);

            //int frame = 0;
            //while(frame < 20 * 1000/30.0)
            //{
            //    Console.WriteLine("Frame: " + (frame++) + " " + arm.GetCurrentPosition());
            //    Thread.Sleep(30);
            //}

            ////arm.DebugRobotCursors();
            ////arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }


        static public void VerticalSquare(Robot bot)
        {
            // Message
            bot.Message("Starting vertical square");

            // A 100 mm long tool with no TCP rotation
            Tool rod = new Tool("rod", new Point(0, 0, 175), new Orientation(1, 0, 0, 0, 1, 0), 1, new Point(0, 0, 50));
            bot.Attach(rod);

            // Home
            bot.SpeedTo((int) (500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(10);
            bot.AxesTo(0, 0, 0, 0, 90, 0);

            // Joint move and rotate to starting point
            bot.PushSettings();
            bot.MotionMode(MotionType.Joint);
            bot.SpeedTo((int)(300 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(5);
            bot.TransformTo(new Point(300, 300, 300), new Orientation(-1, 0, 0, 0, 1, 0));
            bot.Rotate(0, 1, 0, -90);
            bot.PopSettings();
            bot.Wait(500);

            //// Turn on "DO_15"
            //bot.SetIOName("DO_15", 1, true);
            //bot.WriteDigital(1, true);

            // Slow MoveL a square with precision
            bot.SpeedTo((int)(100 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(1);
            bot.Move(0, 50, 0);
            bot.Move(0, 0, 50);
            bot.Move(0, -50, 0);
            bot.Move(0, 0, -50);
            bot.Wait(500);

            //// Turn off "DO_15"
            //bot.WriteDigital(1, false);

            // No tool and back home
            bot.Detach();
            bot.SpeedTo((int)(500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(5);
            bot.AxesTo(0, 0, 0, 0, 90, 0);

        }

        static double x = 400,
            y = 400,
            z = 400;

        static double dx = 50,
            dy = 50,
            dz = 1;

        static int segments = 72;
        static double angle = 0;
        static double da = 2 * Math.PI / segments;


        static private void Spiral(Robot bot, int loops)
        {
            // Home
            bot.SpeedTo((int)(500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(10);
            bot.AxesTo(0, 0, 0, 0, 90, 0);

            // Joint move and rotate to starting point
            bot.PushSettings();
            bot.MotionMode(MotionType.Joint);
            bot.SpeedTo((int)(300 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(5);
            bot.TransformTo(new Point(x, y, z), new Orientation(-1, 0, 0, 0, 1, 0));
            bot.PopSettings();
            bot.Wait(500);

            bot.SpeedTo((int)(100 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            for (var i = 0; i < loops; i++)
            {
                for (var j = 0; j < segments; j++)
                {
                    bot.MoveTo(x + dx * Math.Cos(angle), y + dy * Math.Sin(angle), z + j * dz / segments);
                    angle += da;
                }
                z += dz;
            }

            // Home
            bot.SpeedTo((int)(500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
            bot.PrecisionTo(5);
            bot.AxesTo(0, 0, 0, 0, 90, 0);
        }




        //static public void TestDifferentActions()
        //{
        //    // Test MoveAbsJ
        //    arm.Speed(50);
        //    arm.JointsTo(0, 0, 15, 0, 90, 0);

        //    // Test MoveL
        //    arm.Speed(200);
        //    arm.TransformTo(new Point(300, 300, 300), Rotation.FlippedAroundY);
        //    arm.Speed(25);
        //    arm.Move(50, 0);
        //    arm.Move(0, 50);
        //    arm.Move(-50, 0);
        //    arm.Move(0, -50);
        //    arm.Move(0, 0, 50);

        //    // Test Wait + Msg

        //    arm.Wait(1525);
        //    arm.Message("The quick brown fox jumps over the lazy dog, the quick brown fox jumps over the lazy dog, the quick brown fox jumps over the lazy dog");

        //    // Test MoveJ
        //    arm.Motion("joint");
        //    arm.Speed(100);
        //    arm.Move(0, -300);

        //    // MoveAbsJ
        //    arm.Speed(50);
        //    arm.JointsTo(0, 0, 0, 0, 90, 0);
        //}


        //static public void GenerateMovements(object sender, EventArgs args)
        //{
        //    if (it < maxTargets)
        //    {
        //        arm.Move(dir);
        //        dir.Rotate(Point.ZAxis, 10);
        //    }
        //    else if (it == maxTargets)
        //    {
        //        arm.Speed(100);
        //        arm.MoveTo(302, 0, 558);
        //    }
        //    it++;
        //}
    }
}
