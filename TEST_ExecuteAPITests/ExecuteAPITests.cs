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
            arm.ControlMode("execute");

            // Connect to a controller
            arm.Connect();

            arm.DebugRobotCursors();

            // Do some stuff
            arm.MoveTo(300, 0, 500);
            arm.Move(50, 0);
            arm.Move(0, 50);
            arm.Move(-50, 0);
            arm.Move(0, -50);
            arm.Move(0, 0, 50);
            arm.JointsTo(0, 0, 0, 0, 90, 0);

            arm.DebugRobotCursors();

            arm.Execute();  // flushes all the instructions and sends all pending actions to the controller to be run

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
