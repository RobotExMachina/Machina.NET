using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BRobot;

namespace TEST_OfflineAPITests
{
    class Program
    {
        static void Main(string[] args)
        {
            Robot arm = new Robot("UR");
            arm.Mode("offline");

            // An generic test program
            GeneralTest(arm, 50);

            //// Trace a planar square in space
            //TracePlanarRectangle(arm);

            //// Trace a straight line in Linear and Joint movement modes
            //TraceYLine(arm, false);
            //TraceYLine(arm, true);

            //// Test security table check
            //ApproachBaseXYPlane(arm, 300, 25);

            //// Use Push & PopSettings ;)
            //PushAndPopSettingsTest(arm);

            //// Rotation tests
            //RotationTests(arm);

            //// Advanced rotation tests
            //RotationTestsAdvanced(arm);
            //RotationTests2(arm);

            //// Rel/abs movements
            //TestLocalWorldMovements(arm);

            //// Generative circle movement
            //TestCircle(arm);

            //// Transformations
            //TransformTests(arm);
            //TestCircleTransformAbsolute(arm, 50);
            //TestCircleTransformLocal(arm, 10);
            //TestCircleTransformGlobal(arm, 10);
            //TestZTableLimitations(arm);

            //// Snake
            //TestSnake(arm);

            //// Joint movements
            //TestJointMovementRange(arm);
            //TestRandomJointMovements(arm);

            //// Absolute vs relative movement buffers
            //TestChangesInMovementModes(arm);

            //// Wait and Message
            //TestWaitAndMessage(arm);

            arm.DebugBuffer();  // read all pending buffered actions
            arm.DebugRobotCursors();

            arm.Export(arm.IsBrand("ABB") ? @"C:\offlineTests.mod" : 
                arm.IsBrand("UR") ? @"C:\offlineTests.script" : @"C:\offlineTests.brobot");

            //List<string> code = arm.Export();
            //foreach (string s in code) Console.WriteLine(s);

            arm.DebugRobotCursors();
            arm.DebugBuffer();  // at this point, the buffer should be empty and nothing should show up

            Console.WriteLine(" ");
            Console.WriteLine("Press any key to EXIT...");
            Console.ReadKey();
        }


        // A generic program featuring most of the API calls, good for general base testing
        static public void GeneralTest(Robot arm, double size)
        {
            // Move to 'home' position
            arm.Motion("joint");
            arm.SpeedTo(100);
            arm.ZoneTo(5);  // predef zone
            if (arm.IsBrand("ABB"))
            {
                arm.JointsTo(0, 0, 0, 0, 90, 0);  // 'homie' for ABB
            }
            else if (arm.IsBrand("UR"))
            {
                arm.JointsTo(0, -90, -90, -90, 90, 90);
            }
            else
            {
                arm.JointsTo(0, 0, 0, 0, 0, 0);
            }
            
            // Transform to starting point (needs a transform after a joint movement)
            arm.PushSettings();
            arm.Speed(100);  // relative increase
            arm.Zone(5);    // rel increase
            arm.Motion("joint");
            arm.TransformTo(new Point(200, 300, 400), new Rotation(-1, 0, 0, 0, 1, 0));

            // Align TCP vertically
            //arm.Motion("linear");  // if this is joint mov, UR doesn't do it! XD
            arm.Rotate(0, 1, 0, -90);
            arm.PopSettings();

            // Draw rectangle
            arm.Message("Drawing rectangle");
            arm.Wait(2000);
            arm.PushSettings();
            arm.SpeedTo(25);
            arm.ZoneTo(1);
            arm.Move(0, size, 0);
            arm.Move(0, 0, size);
            arm.Move(0, -size, 0);
            arm.Move(0, 0, -size);

            // Draw half an arc up
            arm.Message("Drawing arc");
            arm.Coordinates("local");
            for (var i = 0; i < 18; i++)
            {
                arm.Move(0, 0.2 * size, 0);
                arm.Rotate(0, 0, 1, 10);
            }
            arm.PopSettings();
            arm.Wait(2000);

            arm.Message("Back home");
            if (arm.IsBrand("ABB"))
            {
                arm.JointsTo(0, 0, 0, 0, 90, 0);  // 'homie' for ABB
            }
            else if (arm.IsBrand("UR"))
            {
                arm.JointsTo(0, -90, -90, -90, 90, 90);
            }
            else
            {
                arm.JointsTo(0, 0, 0, 0, 0, 0);
            }
        }


        static public void TracePlanarRectangle(Robot arm)
        {
            arm.SpeedTo(100); 
            arm.ZoneTo(20);
            arm.MoveTo(300, 300, 300);

            arm.SpeedTo(50);
            arm.ZoneTo(2);  // non predef zone
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 50);

            arm.SpeedTo(100);
            arm.ZoneTo(10);
            arm.MoveTo(300, 0, 500);
        }


        static public void TraceYLine(Robot arm, bool jointMovement)
        {
            if (jointMovement) arm.Motion("joint");

            arm.SpeedTo(100);
            arm.ZoneTo(1);
            arm.MoveTo(300, 0, 600);
            arm.MoveTo(200, -200, 400);
            arm.Move(0, 378, 0);
            arm.MoveTo(300, 0, 600);

            if (jointMovement) arm.Motion("linear");  // back to where it was... this will improve with arm.PushSettings(); 
        }

        static public void ApproachBaseXYPlane(Robot arm, double height, double zStep)
        {
            double h = height;
            zStep = Math.Abs(zStep);
            arm.SpeedTo(100);
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
            arm.MoveTo(300, -200, 300);

            arm.SpeedTo(100);
            arm.ZoneTo(1);

            arm.PushSettings();
            arm.SpeedTo(50);
            arm.Move(0, 200, 0);
            arm.DebugSettingsBuffer();
            arm.PopSettings();

            arm.PushSettings();
            arm.ZoneTo(4);
            arm.Move(0, 200, 0);
            arm.DebugSettingsBuffer();
            arm.PopSettings();

            arm.PushSettings();
            arm.Motion("joint");
            arm.SpeedTo(100);
            arm.Move(0, 0, 200);
            arm.DebugSettingsBuffer();
            arm.PopSettings();

            arm.PushSettings();
            arm.Coordinates("LOCAL");
            arm.Move(0, 0, 200);
            arm.DebugSettingsBuffer();
            arm.PopSettings();
            
            arm.MoveTo(300, 0, 500);

            arm.DebugSettingsBuffer();
        }

        public static void RotationTests(Robot arm)
        {
            //// ABB VERSION
            //arm.SpeedTo(100);
            //arm.MoveTo(300, 0, 500);

            //// Move to a more dexterous area
            //arm.MoveTo(300, -100, 400);

            //// Velocity is expresses in both mm/s and °/s
            //arm.SpeedTo(45);

            //arm.RotateTo(0, 1, 0, 1, 0, 0);     // set coordinate system from XYZ unit vectors (rotate -90° around global Z)
            //arm.RotateTo(0, 0, 1, 1, 0, 0);     // 'rotate 90° around global X'
            //arm.RotateTo(0, 0, 1, 0, -1, 0);    // 'rotate -90° around global Z'

            ////arm.RotateTo(0, 0, 1, 0);  // revert back to base flipped Z
            //arm.RotateTo(1, 0, 0, 0, -1, 0);

            //arm.SpeedTo(100);
            //arm.MoveTo(300, 0, 500);

            //arm.Wait(1000);
            //arm.RotateTo(-1, 0, 0, 0, 1, 0);  // revert back to standard

            //arm.SpeedTo(100);
            //arm.MoveTo(300, 0, 500);





            // UR VERSION
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);  // Set TCP to default orientation

            // Move to a more dexterous area
            arm.MoveTo(300, -100, 400);

            // Velocity is expresses in both mm/s and °/s
            arm.SpeedTo(100);

            //arm.RotateTo(-1, 0, 0, 0, 0, -1);  // rotate 90 degs around local X --> CONVERSION ERROR!
            //arm.RotateTo(0, 0, -1, 1, 0, 0);   // rotate 90 degs around local z
            //arm.RotateTo(0, 1, 0, 1, 0, 0);    // rotate 90 degs around local Y

            //CoordinateSystem cs = new CoordinateSystem(-1, 0, 0, 0, 0, -1);
            //arm.RotateTo(cs);
            arm.Coordinates("local");
            arm.Rotate(1, 0, 0, 45);
            arm.Rotate(1, 0, 0, -90);
            //arm.Rotate(1, 0, 0, 225);  // interesting (and obvious): because internally this only adds a new target, the result is the robot getting there in the shortest way possible (performing a -135deg rotation) rather than the actual 225 rotation over X as would intuitively come from reading he API...
            arm.Rotate(1, 0, 0, 135);

            arm.Rotate(0, 1, 0, -90);
            arm.Rotate(1, 0, 0, -90);
            arm.Rotate(0, 0, 1, 90);


        }

        public static void RotationTestsAdvanced(Robot arm)
        {
            Rotation r1 = new Rotation(new Point(0, 0, 1), 0);
            Rotation r2 = new Rotation(new Point(0, 0, 1), 45);
            Rotation r3 = new Rotation(new Point(0, 0, 1), 90);
            Rotation r4 = new Rotation(new Point(0, 0, 1), 145);
            Rotation r5 = new Rotation(new Point(0, 0, 1), 180);
            Rotation r6 = new Rotation(new Point(0, 0, 1), -45);
            Rotation r7 = new Rotation(new Point(0, 0, 1), -90);
            Rotation r8 = new Rotation(new Point(0, 0, 1), -145);
            Rotation r9 = new Rotation(new Point(0, 0, 1), -180);

            Rotation rx45 = new Rotation(new Point(1, 0, 0), 45);
            Rotation rx90 = new Rotation(new Point(1, 0, 0), 90);
            Rotation rx180 = new Rotation(new Point(1, 0, 0), 180);

            Rotation ry45 = new Rotation(new Point(0, 1, 0), 45);
            Rotation ry90 = new Rotation(new Point(0, 1, 0), 90);
            Rotation ry135 = new Rotation(new Point(0, 1, 0), 135);
            Rotation ry180 = new Rotation(new Point(0, 1, 0), 180);

            Rotation rz45 = new Rotation(new Point(0, 0, 1), 45);
            Rotation rz90 = new Rotation(new Point(0, 0, 1), 90);
            Rotation rz180 = new Rotation(new Point(0, 0, 1), 180);

            Rotation rzn45 = new Rotation(new Point(0, 0, 1), -45);
            Rotation rzn90 = new Rotation(new Point(0, 0, 1), -90);

            Point xAxis = rx45.GetRotationAxis();
            Point yAxis = ry45.GetRotationAxis();
            Point zAxis = rz45.GetRotationAxis();

            double val45 = rx45.GetRotationAngle();
            double val90 = rx90.GetRotationAngle();

            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);

            // Move to a more dexterous area
            arm.MoveTo(300, -100, 400);

            // Velocity is expresses in both mm/s and °/s
            arm.SpeedTo(45);

            
            arm.RotateTo(ry135);
            arm.RotateTo(ry180);
            arm.RotateTo(ry90);
            arm.RotateTo(ry180);

            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
        }

        public static void RotationTests2(Robot arm)
        {
            Rotation xyz = new Rotation();
            Rotation x45 = new Rotation(new Point(1, 0, 0), 45);
            Rotation x90 = new Rotation(new Point(1, 0, 0), 90);
            Rotation x180 = new Rotation(new Point(1, 0, 0), 180);
            Rotation xn45 = new Rotation(new Point(1, 0, 0), -45);
            Rotation xn90 = new Rotation(new Point(1, 0, 0), -90);

            Rotation y45 = new Rotation(new Point(0, 1, 0), 45);
            Rotation y90 = new Rotation(new Point(0, 1, 0), 90);
            Rotation y180 = new Rotation(new Point(0, 1, 0), 180);

            Rotation z45 = new Rotation(new Point(0, 0, 1), 45);
            Rotation z90 = new Rotation(new Point(0, 0, 1), 90);
            Rotation z190 = new Rotation(new Point(0, 0, 1), 180);

            Rotation zn45 = new Rotation(new Point(0, 0, 1), -45);
            Rotation zn90 = new Rotation(new Point(0, 0, 1), -90);

            // Reset
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            // Move to a more dexterous area
            arm.MoveTo(300, 100, 400);

            // Make the TCP face the user
            arm.SpeedTo(30);
            arm.RotateTo(0, 0, -1, 0, 1, 0);

            // Now lets try relative rotation over world Z
            arm.Rotate(0, 0, 1, -90);
            
            // Now a series of relative transforms on world coordinates
            for (int i = 0; i < 10; i++)
            {
                arm.Move(0, -40, 0);
                arm.Rotate(Point.YAxis, 9);
            }

            // Back home
            arm.SpeedTo(100);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);
            arm.MoveTo(300, 0, 500);
        }

        public static void TestLocalWorldMovements(Robot arm)
        {
            // Reset
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            arm.SpeedTo(25);

            // Rotate TCP
            arm.PushSettings();
            arm.Coordinates("local");
            arm.Rotate(Point.XAxis, 45);
            arm.PopSettings();

            // Do global movement
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(0, 0, 50);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 0);
            arm.Move(0, 0, -50);

            // Do local movement
            arm.PushSettings();
            arm.Coordinates("local");
            arm.Move(50, 0, 0);
            arm.Move(0, 50, 0);
            arm.Move(0, 0, 50);
            arm.Move(-50, 0, 0);
            arm.Move(0, -50, 0);
            arm.Move(0, 0, -50);
            arm.PopSettings();

            // Back home
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);
        }

        public static void TestCircle(Robot arm)
        {
            // Reset
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            arm.Move(150, 0, -100);

            arm.PushSettings();
            arm.Coordinates("local");
            //for (var i = 0; i < 36; i++)
            //{
            //    arm.Move(5 * Point.YAxis);
            //    arm.Rotate(Point.ZAxis, 10);
            //    arm.Rotate(Point.YAxis, -2);  // add some off plane movement ;)
            //}
            // Half circle
            for (var i = 0; i < 18; i++)
            {
                arm.Move(5 * Point.YAxis);
                arm.Rotate(Point.ZAxis, 10);
            }
            // 'Unscrew' TCP to avoid wrist out of rotation limits.
            // Must do sequentially, otherwise if issue a local rotation of -360, the target stays the same...
            for (var i = 0; i < 4; i++)
            {
                arm.Rotate(0, 0, 1, -90);
            }
            // Keep going with the circle.
            for (var i = 0; i < 18; i++)
            {
                arm.Move(5 * Point.YAxis);
                arm.Rotate(Point.ZAxis, 10);
            }

            arm.PopSettings();

            // Back home
            arm.SpeedTo(100);
            arm.MoveTo(300, 0, 500);

            // 'Disentangle' axis 6
            //arm.RotateTo(-1, 0, 0, 0, 1, 0);
            arm.JointsTo(0, 0, 0, 0, 90, 0);
            //arm.TransformTo(new Point(300, 0, 500), new Rotation(new Point(0, 1, 0), 180));
        }


        public static void TransformTests(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

            arm.Move(150, 0, -100);
            arm.SpeedTo(25);

            Rotation z30 = new Rotation(Point.ZAxis, 30);

            // Local tests, TR vs RT:
            arm.PushSettings();
            arm.Coordinates("local");
            arm.Transform(50 * Point.XAxis, z30);                          // Note the T+R action order
            arm.Transform(-50 * Point.XAxis, Rotation.Conjugate(z30));
            arm.Transform(z30, 50 * Point.XAxis);                          // Note the R+T action order
            arm.Transform(Rotation.Conjugate(z30), -50 * Point.XAxis);
            arm.PopSettings();

            // Global tests, TR vs RT:
            // Action order is irrelevant in relative global mode (since translations are applied based on immutable world XYZ)
            arm.Transform(50 * Point.XAxis, z30);
            arm.Transform(-50 * Point.XAxis, Rotation.Conjugate(z30));
            arm.Transform(z30, 50 * Point.XAxis);
            arm.Transform(Rotation.Conjugate(z30), -50 * Point.XAxis);

            // Back home
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);
        }


        public static void TestCircleTransformAbsolute(Robot arm, double r)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

            double x = 450;
            double y = 0;
            double z = 400;
            Rotation y180 = new Rotation(new Point(0, 1, 0), 180);

            arm.MoveTo(x, y, z);
            // Trace a circle with absolute coordinates and rotations
            for (var i = 0; i < 36; i++)
            {
                Point target = new Point(
                    x + r * Math.Cos(2 * Math.PI * i / 36.0),
                    y + r * Math.Sin(2 * Math.PI * i / 36.0),
                    z);
                Rotation rot = Rotation.Multiply(new Rotation(Point.ZAxis, 360 * i / 36.0), y180);
                arm.TransformTo(target, rot);
            }

            // Trace it back (and 'disentagle' Axis 6...)
            for (var i = 0; i > -36; i--)
            {
                Point target = new Point(
                    x + r * Math.Cos(2 * Math.PI * i / 36.0),
                    y + r * Math.Sin(2 * Math.PI * i / 36.0),
                    z);
                Rotation rot = Rotation.Multiply(new Rotation(Point.ZAxis, 360 * i / 36.0), y180);
                arm.TransformTo(target, rot);
            }

            // Back home
            arm.TransformTo(home, homeXYZ);
        }

        public static void TestCircleTransformLocal(Robot arm, double side)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

            double x = 450;
            double y = 0;
            double z = 400;

            arm.MoveTo(x, y, z);

            arm.PushSettings();
            arm.Coordinates("local");

            Point forward = side * Point.YAxis;
            Rotation zn10 = new Rotation(Point.ZAxis, -10);
            // Trace a circle with local coordinates and rotations
            for (var i = 0; i < 36; i++)
            {
                arm.Transform(forward, zn10);
            }

            // Trace it back (and 'disentagle' Axis 6...)
            Point backward = -side * Point.YAxis;
            Rotation z10 = new Rotation(Point.ZAxis, 10);
            for (var i = 0; i < 36; i++)
            {
                arm.Transform(backward, z10);
            }

            arm.PopSettings();

            // Back home
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);
        }

        public static void TestCircleTransformGlobal(Robot arm, double side)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

            double x = 450;
            double y = 0;
            double z = 400;

            arm.MoveTo(x, y, z);

            Rotation noRot = new Rotation();

            Rotation z10 = new Rotation(Point.ZAxis, 10);  // note the rotation sign here is inverted from the 'local' example because Zaxis is flipped in local coordinates
            // Trace a circle with relative global coordinates and rotations
            for (var i = 0; i < 36; i++)
            {
                // A 'rotating' direction vector of length 'side'
                Point forward = new Point(
                    -side * Math.Sin(2 * Math.PI * i / 36.0),
                    side * Math.Cos(2 * Math.PI * i / 36.0),
                    0);
                arm.Transform(forward, z10);
            }

            // Trace it back (and 'disentagle' Axis 6...)
            Rotation zn10 = new Rotation(Point.ZAxis, -10);
            for (var i = 0; i > -36; i--)
            {
                Point forward = new Point(
                    side * Math.Sin(2 * Math.PI * i / 36.0),
                    -side * Math.Cos(2 * Math.PI * i / 36.0),
                    0);
                arm.Transform(forward, zn10);
            }

            // Back home
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

        }

        static public void TestZTableLimitations(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            Rotation noRot = new Rotation();

            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);

            // Bring close to limit (100 by default)
            arm.TransformTo(new Point(300, 0, 101), homeXYZ);

            // Try to go trough (should not work)
            arm.TransformTo(new Point(300, 0, 99), homeXYZ);
            arm.Coordinates("local");
            arm.Transform(new Point(0, 0, 2), noRot);
            arm.Coordinates("global");
            arm.Transform(new Point(0, 0, -2), noRot);
            
            // Back home
            arm.TransformTo(home, homeXYZ);
        }


        public static void TestSnake(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);
            
            arm.MoveTo(300, -300, 250);

            arm.SpeedTo(200);
            arm.Coordinates("local");
            for (int i = 0; i < 100; i++)
            {
                Rotation rz = new Rotation(Point.ZAxis, 15 * Math.Cos(2.0 * Math.PI * i / 25.0));
                arm.Transform(rz, 8  * Point.YAxis);
            }

            // Back home
            arm.SpeedTo(100);
            arm.TransformTo(home, homeXYZ);
        }

        // DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF
        public static void TestJointMovementRange(Robot arm)
        {
            Console.WriteLine("WARNING: DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF");

            // Reset
            arm.SpeedTo(500);
            arm.JointsTo(0, 0, 0, 0, 90, 0);

            // Go from lower to higher configuration space (ABB IRB120)
            arm.JointsTo(-164, -109, -109, -159, -119, -399);
            arm.JointsTo(164, 109, 69, 159, 119, 399);

            // Now sweep all spectrum for each joint
            arm.JointsTo(-164, -109, -109, -159, -119, -399);
            arm.Joints(328, 0, 0, 0, 0, 0);
            arm.Joints(0, 218, 0, 0, 0, 0);
            arm.Joints(0, 0, 178, 0, 0, 0);
            arm.Joints(0, 0, 0, 318, 0, 0);
            arm.Joints(0, 0, 0, 0, 238, 0);
            arm.Joints(0, 0, 0, 0, 0, 798);

            // Back home
            arm.JointsTo(0, 0, 0, 0, 90, 0);
        }

        // DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF
        public static void TestRandomJointMovements(Robot arm)
        {
            Console.WriteLine("WARNING: DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF");
         
            // Reset
            arm.SpeedTo(300);
            arm.JointsTo(0, 0, 0, 0, 90, 0);

            // DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF
            Random rnd = new Random();
            for (var i = 0; i < 10; i++)
            {
                arm.JointsTo(
                    rnd.Next(-164, 164),
                    rnd.Next(-109, 109),
                    rnd.Next(-109, 69),
                    rnd.Next(-159, 159),
                    rnd.Next(-119, 119),
                    rnd.Next(-399, 399));
            }

            // Back home
            arm.JointsTo(0, 0, 0, 0, 90, 0);
        }

        public static void TestChangesInMovementModes(Robot arm)
        {
            // Go home
            arm.SpeedTo(100);
            //arm.JointsTo(0, 0, 0, 0, 90, 0);  // ABB
            arm.JointsTo(0, -90, -90, -90, 90, 90);  // UR

            // Issue an absolute RT movement (should work)
            arm.TransformTo(new Point(200, 200, 200), Rotation.FlippedAroundY);

            // Issue a relative one (should work)
            arm.Transform(new Point(50, 0, 0), new Rotation(Point.XAxis, 45));

            // Issue a relative Joints one (SHOULDN'T WORK)
            arm.Joints(-45, 0, 0, 0, 0, 0);

            // Issue abs joints (should work)
            arm.JointsTo(45, 0, 0, 0, 90, 0);

            // Issue rel Joints (should work)
            arm.Joints(30, 0, 0, 0, 0, 0);

            // Issue a bunch of relative ones (NONE SHOULD WORK)
            arm.Coordinates("local");
            arm.Move(100, 0, 0);
            arm.Rotate(Point.XAxis, 45);
            arm.Transform(new Point(100, 0, 0), new Rotation(Point.XAxis, 45));
            arm.Transform(new Rotation(Point.XAxis, 45), new Point(100, 0, 0));
            arm.Coordinates("world");
            arm.Move(100, 0, 0);
            arm.Rotate(Point.XAxis, 45);
            arm.Transform(new Point(100, 0, 0), new Rotation(Point.XAxis, 45));
            arm.Transform(new Rotation(Point.XAxis, 45), new Point(100, 0, 0));

            // Rel joints (should work)
            arm.Joints(0, 30, 0, 0, 0, 0);

            // Issue absolute movement (SHOULD NOT WORK)
            arm.MoveTo(400, 0, 200);                // missing rotation information
            arm.RotateTo(Rotation.FlippedAroundY);  // missing point information

            // Back home (should work)
            //arm.JointsTo(0, 0, 0, 0, 90, 0);    // ABB
            arm.JointsTo(0, -90, -90, -90, 90, 90);  // UR
        }

        static public void TestWaitAndMessage(Robot arm)
        {
            // Go home
            arm.SpeedTo(100);
            arm.Message("Going home");
            arm.MoveTo(300, 0, 500);

            arm.Message("Waiting 1.5s to go start");
            arm.Wait(1500); 

            arm.Message("Starting first path");
            arm.Move(0, 200, -100);

            arm.Message("Waiting 5s to go back");
            arm.Wait(5000);

            arm.Message("Going home");
            //arm.JointsTo(0, 0, 0, 0, 90, 0);
            arm.JointsTo(0, -90, -90, -90, 90, 90);
        }



    }
}
