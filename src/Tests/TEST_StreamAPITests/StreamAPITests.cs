using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Machina;
using Machina.Types.Geometry;

namespace TEST_StreamAPITests
{
    class StreamAPITests
    {

        static bool PHYSICAL_ROBOT = true;

        static Joints homeJoints = new Joints(0, 0, 0, 0, 90, 0);           // ABB
        //static Joints homeJoints = new Joints(0, -90, -90, -90, 90, 90);      // UR

        static public void LogEvent(object sender, MachinaEventArgs args)
        {
            Console.WriteLine(args.ToJSONString());
        }

        static void Main(string[] args)
        {
            Robot arm = Robot.Create("StreamTests", "ABB");

            //arm.DebugMode(true);

            //arm.ActionExecuted += LogEvent;
            //arm.ActionReleased += LogEvent;
            //arm.ActionIssued += LogEvent;

            Machina.Logger.SetLogLevel(5);
            Machina.Logger.WriteLine += Console.WriteLine;

            //arm.MotionUpdate += LogEvent;

            //arm.ActionExecuted += (sender, e) =>
            //{
            //    if (e.PendingExecutionTotal == 0) Loop(sender as Robot, 100);
            //};


            arm.ControlMode("stream");
            arm.ConnectionManager("machina");
            arm.Connect();
            //arm.ConnectionManager("user");
            //arm.Connect("127.0.0.1", 7000);

            //arm.SetUser("BUILD", "password");
            //arm.Connect("192.168.0.101", 6969);

            //arm.StreamConfiguration(3, 10);

            arm.SpeedTo(100);
            arm.PrecisionTo(10);
            arm.MoveTo(300, 300, 300);
            arm.Move(0, 0, 200);
            arm.Wait(1000);
            arm.AxesTo(homeJoints);
                       

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            //arm.Export(@"C:\spiral.script", true, false);
            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        private static void Logger_CustomLogging(LoggerArgs e)
        {
            throw new NotImplementedException();
        }

        static void WObjTesting(Robot bot)
        {
            bot.Message("Homing...");
            bot.SpeedTo(100);
            bot.AxesTo(0, 0, 0, 0, 90, 0);
            bot.Wait(1000);

            bot.Message("Moving to 300, 0, 300");
            bot.TransformTo(300, 0, 300, -1, 0, 0, 0, 1, 0);
            bot.Wait(2000);

            bot.Message("Setting wobj");
            bot.CustomCode("18 300 0 300 1 0 0 0");

            bot.Message("Moving to 300, 0, 400");
            bot.TransformTo(0, 0, 100, -1, 0, 0, 0, 1, 0);
            bot.Wait(2000);

            bot.Message("Homing...");
            bot.SpeedTo(100);
            bot.AxesTo(0, 0, 0, 0, 90, 0);
        }

        static void ToolTesting(Robot bot)
        {
            bot.SpeedTo(20);
            bot.Move(0, 100, 0);
            bot.Move(0, -100, 0);
            bot.Wait(2000);

            bot.DefineTool("tool_100", new Point(0, 0, 100), Orientation.WorldXY);
            bot.AttachTool("tool_100");
            bot.MoveTo(400, 0, 400);
            bot.Move(0, 100, 0);
            bot.Move(0, -100, 0);
            bot.DetachTool();
            bot.MoveTo(400, 0, 400);
            bot.Wait(2000);

            bot.DefineTool("tool_300", new Point(0, 0, 300), Orientation.WorldXY);
            bot.AttachTool("tool_300");
            bot.MoveTo(400, 0, 400);
            bot.Move(0, 100, 0);
            bot.Move(0, -100, 0);
            bot.DetachTool();
            bot.MoveTo(400, 0, 400);
            bot.Wait(2000);

            bot.MoveTo(400, 0, 400);
            bot.Move(0, 100, 0);
            bot.Move(0, -100, 0);
            bot.MoveTo(400, 0, 400);
            bot.Wait(2000);

            bot.AxesTo(0, 0, 0, 0, 90, 0);
        }

        static void Loop(Robot bot, double size)
        {
            // Repeat three times to compare pending on device vs pending in total
            bot.SpeedTo(100);
            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, size);
            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, -size);

            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, size);
            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, -size);

            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, size);
            bot.Move(size, 0, 0);
            bot.Move(0, size, 0);
            bot.Move(-size, 0, 0);
            bot.Move(0, -size, 0);
            bot.Move(0, 0, -size);
        }

        static public void ExternalAxes(Robot bot)
        {
            // Message
            bot.Message("Testing external axes");

            // setup
            bot.ExternalAxisTo(1, 1800);

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
            //bot.JointSpeedTo(20);
            //bot.JointAccelerationTo(4);
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
            //bot.JointSpeedTo(60);
            //bot.JointAccelerationTo(10);
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




        static private void Spiral(Robot bot, int loops)
        {
            double x = 400,
                   y = 400,
                   z = 400;

            double dx = 50,
                dy = 50,
                dz = 1;

            int segments = 16;
            double angle = 0;
            double da = 2 * Math.PI / segments;

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

        static private void SpiralUR(Robot bot, double sx, double sy, double sz, double radius, double loopHeight, int loopCount, double linearSpeed)
        {
            int segments = 72;
            double angle = 0;
            double da = 2 * Math.PI / segments;

            bot.PushSettings();

            // Home
            //bot.JointSpeedTo(20);
            //bot.JointAccelerationTo(90);
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
            //bot.JointSpeedTo(20);
            //bot.JointAccelerationTo(90);
            bot.PrecisionTo(10);
            bot.AxesTo(0, -90, -90, -90, 90, 90);

            bot.PopSettings();
        }

        static private void SquareSpiralUR(Robot bot, double sx, double sy, double sz, double side, double h, int loopCount, double linearSpeed)
        {
            bot.PushSettings();

            // Home
            //bot.JointSpeedTo(20);
            //bot.JointAccelerationTo(90);
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
            //bot.JointSpeedTo(20);
            //bot.JointAccelerationTo(90);
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
