using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotControl;

namespace TEST_NewAPITests
{
    class Program
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot();
            arm.ControlMode("offline");

            //// Trace a planar square in space
            //TracePlanarRectangle(arm);

            //// Trace a straight line in Linear and Joint movement modes
            //TraceYLine(arm, false);
            //TraceYLine(arm, true);

            //// Test security table check
            //ApproachBaseXYPlane(arm, 300, 25);

            //// Use Push & PopSettings ;)
            //PushAndPopSettingsTest(arm);

            RotationTests(arm);
            
            arm.DebugBuffer();  // read all pending buffered actions

            arm.DebugRobotCursors();
            arm.Export(@"C:\offlineTests.mod");
            arm.DebugRobotCursors();

            arm.DebugBuffer();  // at this point, the buffer should be empty and nothing should show up

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }


        static public void TracePlanarRectangle(Robot arm)
        {
            arm.SetVelocity(200);
            arm.SetZone(20);
            arm.MoveTo(300, 300, 300);

            arm.SetVelocity(50);
            arm.SetZone(2);  // non predef zone
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 50);

            arm.SetVelocity(500);
            arm.SetZone(10);
            arm.MoveTo(300, 0, 500);
        }


        static public void TraceYLine(Robot arm, bool jointMovement)
        {
            if (jointMovement) arm.SetMotionType("joint");

            arm.SetVelocity(100);
            arm.SetZone(1);
            arm.MoveTo(300, 0, 600);
            arm.MoveTo(200, -200, 400);
            arm.Move(0, 378, 0);
            arm.MoveTo(300, 0, 600);

            if (jointMovement) arm.SetMotionType("linear");  // back to where it was... this will improve with arm.PushSettings(); 
        }

        static public void ApproachBaseXYPlane(Robot arm, double height, double zStep)
        {
            double h = height;
            zStep = Math.Abs(zStep);
            arm.MoveTo(300, 0, h);
            while (h > 0)
            {
                arm.Move(0, 0, -zStep);
                h -= zStep;
            }

            // if here, height should be zero, so move back to initial position
            arm.MoveTo(300, 0, height);
        }

        static public void PushAndPopSettingsTest(Robot arm)
        {
            arm.PushSettings();

            arm.SetVelocity(222);
            arm.PushSettings();

            arm.SetZone(4);
            arm.PushSettings();

            arm.SetMotionType("joint");
            arm.PushSettings();

            arm.MoveTo(300, 300, 300);

            arm.DebugSettingsBuffer();

            arm.PopSettings();
            arm.PopSettings();
            arm.PopSettings();
            arm.PopSettings();

            arm.MoveTo(300, 0, 500);

            arm.DebugSettingsBuffer();
        }

        public static void RotationTests(Robot arm)
        {
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);

            // Move to a more dexterous area
            arm.MoveTo(300, -100, 400);

            // Velocity is expresses in both mm/s and °/s
            arm.SetVelocity(45);

            arm.RotateTo(0, 1, 0, 1, 0, 0, 0, 0, -1);   // set coordinate system from XYZ unit vectors (rotate -90° around global Z)
            arm.RotateTo(0, 0, 1, 1, 0, 0, 0, 1, 0);    // 'rotate 90° around global X'
            arm.RotateTo(0, 0, 1, 0, -1, 0, 1, 0, 0);   // 'rotate -90° around global Z'

            arm.RotateTo(0, 0, 1, 0);  // revert back to base flipped Z

            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);

        }

    }
}
