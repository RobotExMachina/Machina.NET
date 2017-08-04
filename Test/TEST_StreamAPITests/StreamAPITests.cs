using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace TEST_StreamAPITests
{
    class StreamAPITests
    {
        static Robot arm = new Robot();
        static Point dir = new Point(20, 0, 0);
        static int it = 0;
        static int maxTargets = 36;

        static void Main(string[] args)
        {
            // Set control mode
            arm.Mode("stream");

            // Connect to a controller
            arm.Connect();
            arm.Start();

            // arm.BufferEmpty += new BufferEmptyHandler(GenerateMovements);

            // TESTING MULTIPLE STREAM ACTIONS
            TestDifferentActions();

            arm.DebugRobotCursors();
            arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }

        static public void TestDifferentActions()
        {
            // Test MoveAbsJ
            arm.Speed(50);
            arm.JointsTo(0, 0, 15, 0, 90, 0);

            // Test MoveL
            arm.Speed(200);
            arm.TransformTo(new Point(300, 300, 300), Rotation.FlippedAroundY);
            arm.Speed(25);
            arm.Move(50, 0);
            arm.Move(0, 50);
            arm.Move(-50, 0);
            arm.Move(0, -50);
            arm.Move(0, 0, 50);

            // Test Wait + Msg
            
            arm.Wait(1525);
            arm.Message("The quick brown fox jumps over the lazy dog, the quick brown fox jumps over the lazy dog, the quick brown fox jumps over the lazy dog");

            // Test MoveJ
            arm.Motion("joint");
            arm.Speed(100);
            arm.Move(0, -300);

            // MoveAbsJ
            arm.Speed(50);
            arm.JointsTo(0, 0, 0, 0, 90, 0);
        }


        static public void GenerateMovements(object sender, EventArgs args)
        {
            if (it < maxTargets)
            {
                arm.Move(dir);
                dir.Rotate(Point.ZAxis, 10);
            }
            else if (it == maxTargets)
            {
                arm.Speed(100);
                arm.MoveTo(302, 0, 558);
            }
            it++;
        }
    }
}
