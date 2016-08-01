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
        static bool dir = true;
        static int it = 0;

        static void Main(string[] args)
        {
            // Set control mode
            arm.ControlMode("stream");

            // Connect to a controller
            arm.Connect();
            arm.Start();

            // Do some stuff
            arm.Speed(50);
            arm.Zone(5);
            arm.MoveTo(300, -200, 300);

            arm.BufferEmpty += new BufferEmptyHandler(GenerateMovements);



            //arm.MoveTo(200, 200, 200);
            //for (int i = 0; i < 3; i++)
            //{
            //    arm.Move(50, 0);
            //    arm.Move(0, 50);
            //    arm.Move(-50, 0);
            //    //arm.Move(0, -50);
            //    arm.Move(0, -50, 50);  
            //}

            //arm.DebugRobotCursors();
            //arm.DebugBuffer();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();

            arm.Disconnect();

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();   

        }

        static public void GenerateMovements(object sender, EventArgs args)
        {
            if (it < 10)
            {
                Console.WriteLine("       ---> SENDING NEW MOVE INSTR: " + dir);
                if (dir)
                {
                    arm.Move(75, 25);
                }
                else
                {
                    arm.Move(-75, 25);
                }
                dir = !dir;
                it++;
            }
            else
            {
                Console.WriteLine("Done sending instructions");
                //arm.moveto(300, 0, 500);
            }

        }
    }
}
