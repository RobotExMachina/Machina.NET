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

        static bool PHYSICAL_ROBOT = true;

        //static Joints homeJoints = new Joints(0, 0, 0, 0, 90, 0);           // ABB
        static Joints homeJoints = new Joints(0, -90, -90, -90, 90, 90);    // UR

        static public void LogEvent(object sender, EventArgs args)
        {
            Console.WriteLine("EVENT RAISED");
            Console.WriteLine(sender);
            Console.WriteLine(args);
        }

        static void Main(string[] args)
        {
            Robot arm = Robot.Create("StreamTests", "ABB");

            //arm.BufferEmpty += LogEvent;

            arm.ConnectionManager("machina");
            arm.ControlMode("stream");
            arm.Connect();
            //arm.Connect("127.0.0.1", 7000);

            //arm.SetUser("BUILD", "password");
            //arm.Connect("192.168.0.101", 6969);

            //arm.StreamConfiguration(3, 10);

            arm.Message("Hello Robot!");

            //arm.Start();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to START EXTERNAL AXIS TEST...");
            Console.ReadKey();
            ExternalAxes(arm);

            //Console.WriteLine(" ");
            //Console.WriteLine("Press any key to START THE VERTICAL SQUARE...");
            //Console.ReadKey();
            //VerticalSquareUR(arm);

            //Console.WriteLine(" ");
            //Console.WriteLine("Press any key to START THE VERTICAL CIRCLE...");
            //Console.ReadKey();
            //VerticalCircleUR(arm);


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

            //Console.WriteLine(" ");
            //Console.WriteLine("Press any key to START THE SQUARE LOOP...");
            //Console.ReadKey();
            //arm.StreamConfiguration(2, 10);
            //SquareSpiralUR(arm, 400, -400, 100, 25, 10, 10, 50);

            //Console.WriteLine(" ");
            //Console.WriteLine("Press any key to START THE LOOP...");
            //Console.ReadKey();
            //SpiralUR(arm, 400, -400, 200, 50, 10, 1, );

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            //arm.Export(@"C:\spiral.script", true, false);
            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        static public void ExternalAxes(Robot bot)
        {
            // Message
            bot.Message("Testing external axes");

            // setup
            bot.ExternalAxesTo(1800);

            // Home
            bot.SpeedTo(500);
            bot.TransformTo(1800, -1445, 1327.65, 0, 1, 0, 1, 0, 0);

            bot.Wait(2000);

            bot.AxesTo(0, 0, 0, 0, 90, 0);

        }


        static public void VerticalSquare(Robot bot)
        {
            // Message
            bot.Message("Starting vertical square");

            // A 100 mm long tool with no TCP rotation
            //Tool rod = new Tool("rod", new Point(0, 0, 100), Orientation.WorldXY, 1, new Point(0, 0, 50));
            //bot.Attach(rod);

            // Home
            bot.SpeedTo(200 * (PHYSICAL_ROBOT ? 0.2 : 1));
            bot.PrecisionTo(10);
            bot.AxesTo(homeJoints);

            // Joint move and rotate to starting point
            bot.PushSettings();
            bot.MotionMode(MotionType.Joint);
            bot.SpeedTo(300 * (PHYSICAL_ROBOT ? 0.2 : 1));
            bot.PrecisionTo(5);
            bot.TransformTo(new Point(300, 300, 300), new Orientation(-1, 0, 0, 0, 1, 0));
            bot.Rotate(0, 1, 0, -90);
            bot.PopSettings();
            bot.Wait(500);

            //// Turn on "DO_15"
            //bot.SetIOName("DO_15", 1, true);
            //bot.WriteDigital(1, true);

            // Slow MoveL a square with precision
            bot.SpeedTo(100 * (PHYSICAL_ROBOT ? 0.2 : 1));
            bot.PrecisionTo(1);
            bot.Move(0, 50, 0);
            bot.Move(0, 0, 50);
            bot.Move(0, -50, 0);
            bot.Move(0, 0, -50);
            bot.Wait(500);

            //// Turn off "DO_15"
            //bot.WriteDigital(1, false);

            // No tool and back home
            //bot.Detach();
            bot.SpeedTo(500 * (PHYSICAL_ROBOT ? 0.2 : 1));
            bot.PrecisionTo(5);
            bot.AxesTo(homeJoints);

        }

        static public void VerticalSquareUR(Robot bot)
        {
            // Message
            bot.Message("Starting vertical square");

            ////A 100 mm long tool with no TCP rotation
            //Tool rod = new Tool("rod", new Point(0, 0, 100), Orientation.WorldXY, 1, new Point(0, 0, 50));
            //bot.Attach(rod);

            // UR is giving me problems with stupid linear mode...
            bot.MotionMode(MotionType.Joint); 

            // Home
            bot.JointSpeedTo(20);
            bot.JointAccelerationTo(4);
            bot.PrecisionTo(10);
            bot.AxesTo(homeJoints);
            
            // Joint move and rotate to starting point
            bot.PushSettings();
            //bot.SpeedTo(100);
            //bot.AccelerationTo(50);
            bot.PrecisionTo(1);
            bot.TransformTo(new Point(300, 300, 300), new Orientation(1, 0, 0, 0, -1, 0));
            bot.Rotate(0, 1, 0, 90);
            bot.PopSettings();
            bot.Wait(1000);

            //// Turn on "DO_15"
            //bot.SetIOName("DO_15", 1, true);
            bot.WriteDigital(1, true);

            bot.PrecisionTo(1);
            bot.Move(0, 100, 0);
            bot.Move(0, 0, 100);
            bot.Move(0, -100, 0);
            bot.Move(0, 0, -100);
            bot.Wait(1000);

            //// Turn off "DO_15"
            bot.WriteDigital(1, false);

            // No tool and back home
            //bot.Detach();
            //bot.JointSpeedTo(45);
            //bot.JointAccelerationTo(90);
            bot.PrecisionTo(5);
            bot.AxesTo(homeJoints);


        }

        static public void VerticalCircleUR(Robot bot)
        {
            // Message
            bot.Message("Starting vertical square");

            ////A 100 mm long tool with no TCP rotation
            //Tool rod = new Tool("rod", new Point(0, 0, 100), Orientation.WorldXY, 1, new Point(0, 0, 50));
            //bot.Attach(rod);

            // UR is giving me problems with stupid linear mode...
            bot.MotionMode(MotionType.Joint);

            // Home
            bot.JointSpeedTo(60);
            bot.JointAccelerationTo(10);
            bot.PrecisionTo(10);
            bot.AxesTo(homeJoints);

            double x = 300,
                   y = 300,
                   z = 300,
                   r = 50,
                   angle = Math.PI / 2;
            int steps = 32;

            // Joint move and rotate to starting point
            bot.PushSettings();
            //bot.SpeedTo(100);
            //bot.AccelerationTo(50);
            bot.PrecisionTo(1);
            bot.TransformTo(new Point(x, y, z), new Orientation(1, 0, 0, 0, -1, 0));
            bot.Rotate(0, 1, 0, 90);
            bot.PopSettings();
            //bot.Wait(1000);

            bot.WriteDigital(1, true);

            for (int i = 0; i < steps; i++)
            {
                bot.MoveTo(x, y + r * Math.Cos(angle), z + r * Math.Sin(angle));
                angle += 2 * Math.PI / steps;
            }
            //bot.Wait(1000);

            bot.WriteDigital(1, false);

            // No tool and back home
            //bot.Detach();
            //bot.JointSpeedTo(45);
            //bot.JointAccelerationTo(90);
            bot.PrecisionTo(5);
            bot.AxesTo(homeJoints);


        }

        //static double x = 400,
        //    y = 400,
        //    z = 400;

        //static double dx = 50,
        //    dy = 50,
        //    dz = 1;

        //static int segments = 72;
        //static double angle = 0;
        //static double da = 2 * Math.PI / segments;


        //static private void Spiral(Robot bot, int loops)
        //{
        //    // Home
        //    bot.SpeedTo((int)(500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
        //    bot.PrecisionTo(10);
        //    bot.AxesTo(0, 0, 0, 0, 90, 0);

        //    // Joint move and rotate to starting point
        //    bot.PushSettings();
        //    bot.MotionMode(MotionType.Joint);
        //    bot.SpeedTo((int)(300 * (PHYSICAL_ROBOT ? 0.2 : 1)));
        //    bot.PrecisionTo(5);
        //    bot.TransformTo(new Point(x, y, z), new Orientation(-1, 0, 0, 0, 1, 0));
        //    bot.PopSettings();
        //    bot.Wait(500);

        //    bot.SpeedTo((int)(100 * (PHYSICAL_ROBOT ? 0.2 : 1)));
        //    for (var i = 0; i < loops; i++)
        //    {
        //        for (var j = 0; j < segments; j++)
        //        {
        //            bot.MoveTo(x + dx * Math.Cos(angle), y + dy * Math.Sin(angle), z + j * dz / segments);
        //            angle += da;
        //        }
        //        z += dz;
        //    }

        //    // Home
        //    bot.SpeedTo((int)(500 * (PHYSICAL_ROBOT ? 0.2 : 1)));
        //    bot.PrecisionTo(5);
        //    bot.AxesTo(0, 0, 0, 0, 90, 0);
        //}

        static private void SpiralUR(Robot bot, double sx, double sy, double sz, double radius, double loopHeight, int loopCount, double linearSpeed)
        {
            int segments = 72;
            double angle = 0;
            double da = 2 * Math.PI / segments;

            bot.PushSettings();

            // Home
            bot.JointSpeedTo(20);
            bot.JointAccelerationTo(90);
            bot.PrecisionTo(10);
            bot.AxesTo(0, -90, -90, -90, 90, 90);

            // Approach first point
            bot.MotionMode(MotionType.Linear);
            bot.SpeedTo(linearSpeed);
            bot.PrecisionTo(5);
            bot.TransformTo(sx, sy, sz, -1, 0, 0, 0, 1, 0);

            // Start looping
            bot.SpeedTo(linearSpeed);
            bot.PrecisionTo(1);
            //bot.MotionMode(MotionType.Joint);

            for (var i = 0; i < loopCount; i++)
            {
                for (var j = 0; j < segments; j++)
                {
                    bot.MoveTo(sx + radius * Math.Cos(angle), sy + radius * Math.Sin(angle), sz + i * loopHeight + j * loopHeight / segments);
                    angle += da;
                }
            }

            // Home
            bot.JointSpeedTo(20);
            bot.JointAccelerationTo(90);
            bot.PrecisionTo(10);
            bot.AxesTo(0, -90, -90, -90, 90, 90);

            bot.PopSettings();
        }

        static private void SquareSpiralUR(Robot bot, double sx, double sy, double sz, double side, double h, int loopCount, double linearSpeed)
        {
            bot.PushSettings();

            // Home
            bot.JointSpeedTo(20);
            bot.JointAccelerationTo(90);
            bot.PrecisionTo(10);
            bot.AxesTo(0, -90, -90, -90, 90, 90);

            // Approach first point
            bot.MotionMode(MotionType.Linear);
            bot.SpeedTo(5 * linearSpeed);
            bot.PrecisionTo(5);
            bot.TransformTo(sx, sy, sz, -1, 0, 0, 0, 1, 0);

            // Start looping
            bot.SpeedTo(linearSpeed);
            bot.PrecisionTo(1);

            for (int i = 0; i < loopCount; i++)
            {
                bot.Move(side, 0);
                bot.Move(0, side);
                bot.Move(-side, 0);
                bot.Move(0, -side);
                bot.Move(0, 0, h);
            }

            // Home
            bot.JointSpeedTo(20);
            bot.JointAccelerationTo(90);
            bot.PrecisionTo(10);
            bot.AxesTo(0, -90, -90, -90, 90, 90);

            bot.PopSettings();
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
