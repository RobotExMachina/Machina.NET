using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace TEST_ExecuteAPITests
{
    class ExecuteAPITests
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();   

            // Set control mode
            arm.Mode("execute");     

            // Connect to a controller
            arm.Connect();
            

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            //// Do some stuff
            //arm.Speed(200);
            //arm.Zone(5);
            //arm.MoveTo(300, 0, 500);
            //arm.Move(100, 0);
            //arm.Move(0, 100);
            //arm.Move(-100, 0);
            //arm.Move(0, -100);
            //arm.Move(0, 0, - 100);
            //arm.JointsTo(0, 0, 0, 0, 90, 0);

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            //arm.Execute();  // flushes all the instructions and sends all pending actions to the controller to be run


            //// Do some stuff
            //arm.Speed(200);
            //arm.Zone(10);
            //arm.TransformTo(new Point(200, 200, 200), Rotation.FlippedAroundY);
            //arm.Move(100, 0);
            //arm.JointsTo(0, 0, 0, 0, 90, 0);

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            //arm.Execute();  // flushes all the instructions and sends all pending actions to the controller to be run


            //// Do some stuff
            //arm.Speed(150);
            //arm.Zone(7);
            //arm.TransformTo(new Point(300, 300, 300), Rotation.FlippedAroundY);
            //arm.Move(0, -100);
            //arm.Rotate(0, 1, 0, 45);
            //arm.JointsTo(0, 0, 0, 0, 90, 0);

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            //arm.Execute();  // flushes all the instructions and sends all pending actions to the controller to be run








            //// Anytime the program is running, it can be paused with 
            //arm.Stop();

            //// And resumed with 
            //arm.Start();

            //// And sent to the beginning with
            //arm.Rewind();

            //// And toggle looping execution:
            //arm.Loop();
            //arm.NoLoop();  // default (is this part of settings?)

            //// Events can be attached:
            //arm.Stop += OnStopHandler;

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();
        }
    }
}
