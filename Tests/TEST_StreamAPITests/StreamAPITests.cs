using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina;

namespace TEST_StreamAPITests
{
    class StreamAPITests
    {
        //static Robot arm = new Robot();
        //static Point dir = new Point(20, 0, 0);
        //static int it = 0;
        //static int maxTargets = 36;

        static void Main(string[] args)
        {

            Robot arm = new Robot("StreamTests", "ABB");

            // arm.BufferEmpty += new BufferEmptyHandler(GenerateMovements);

            OneActionTest(arm);

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to DISCONNECT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        static public void VerticalRectangle(Robot bot)
        {

        }

        static public void OneActionTest(Robot bot) 
        {
            bot.ControlMode(ControlType.Stream);

            bot.Connect();
            bot.Start();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to START THE PROGRAM...");
            Console.ReadKey();

            // Message
            bot.Message("Starting vertical square");
            
            // Home
            bot.SpeedTo(200);
            bot.PrecisionTo(10);
            bot.AxesTo(0, 0, 0, 0, 90, 0);

            // A 100 mm long tool with no TCP rotation
            //r.write('8 0 0 100 1 0 0 0;');
            Tool rod = new Tool("rod", new Point(0, 0, 100), new Orientation(1, 0, 0, 0, 1, 0), 1, new Point(0, 0, 50));
            bot.Attach(rod);

            // Joint move and rotate to starting point
            bot.PushSettings();
            bot.MotionMode(MotionType.Joint);
            bot.SpeedTo(100);
            bot.PrecisionTo(5);
            bot.TransformTo(new Point(300, 300, 300), new Orientation(-1, 0, 0, 0, 1, 0));
            bot.Rotate(0, 1, 0, -90);
            bot.PopSettings();
            bot.Wait(1500);

            // Turn on "DO_15"
            bot.SetIOName("DO_15", 15, true);
            bot.WriteDigital(15, true);

            // Slow MoveL a square with precision
            bot.SpeedTo(20);
            bot.PrecisionTo(1);
            bot.Move(0, 50, 0);
            bot.Move(0, 0, 50);
            bot.Move(0, -50, 0);
            bot.Move(0, 0, -50);
            bot.Wait(1000);

            // Turn off "DO_15"
            bot.WriteDigital(15, false);

            // No tool and back home
            bot.Detach();
            bot.SpeedTo(200);
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
