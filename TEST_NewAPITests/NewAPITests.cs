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

            //// Rotation tests
            //RotationTests(arm);

            //// Advanced rotation tests
            //RotationTestsAdvanced(arm);
            //RotationTests2(arm);

            //// Rel/abs movements
            //MovementTest(arm);

            //// Generative circle movement
            //TestCircle(arm);

            // Transformations
            //TransformTests(arm);
            //TestCircleTransformAbsolute(arm, 50);
            //TestCircleTransformLocal(arm, 10);
            //TestCircleTransformGlobal(arm, 10);
            TestZTableLimitations(arm);

            //// Snake
            //TestSnake(arm);

            //// Joint movements
            //TestJointMovementRange(arm);
            //TestRandomJointMovements(arm);

            //// Absolute vs relative movement buffers
            //TestChangesInMovementModes(arm);


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
            arm.MoveGlobal(50, 0, 0);
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(-50, 0, 0);
            arm.MoveGlobal(0, -50, 50);

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
            arm.MoveGlobal(0, 378, 0);
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
                arm.MoveGlobal(0, 0, -zStep);
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

            arm.RotateTo(0, 1, 0, 1, 0, 0);     // set coordinate system from XYZ unit vectors (rotate -90° around global Z)
            arm.RotateTo(0, 0, 1, 1, 0, 0);     // 'rotate 90° around global X'
            arm.RotateTo(0, 0, 1, 0, -1, 0);    // 'rotate -90° around global Z'

            //arm.RotateTo(0, 0, 1, 0);  // revert back to base flipped Z
            arm.RotateTo(1, 0, 0, 0, -1, 0);

            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
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
            Rotation ry180 = new Rotation(new Point(0, 1, 0), 180);

            Rotation rz45 = new Rotation(new Point(0, 0, 1), 45);
            Rotation rz90 = new Rotation(new Point(0, 0, 1), 90);
            Rotation rz180 = new Rotation(new Point(0, 0, 1), 180);

            Rotation rzn45 = new Rotation(new Point(0, 0, 1), -45);
            Rotation rzn90 = new Rotation(new Point(0, 0, 1), -90);

            Point xAxis = rx45.RotationVector();
            Point yAxis = ry45.RotationVector();
            Point zAxis = rz45.RotationVector();

            double val45 = rx45.RotationAngle();
            double val90 = rx90.RotationAngle();

            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);

            // Move to a more dexterous area
            arm.MoveTo(300, -100, 400);

            // Velocity is expresses in both mm/s and °/s
            arm.SetVelocity(45);

            arm.RotateTo(ry180);

            arm.RotateTo(r5);

            arm.SetVelocity(100);
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
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            // Move to a more dexterous area
            arm.MoveTo(300, 100, 400);

            // Make the TCP face the user
            arm.SetVelocity(30);
            arm.RotateTo(0, 0, -1, 0, 1, 0);

            //// Rotate it around its local Z (joint 6)
            //arm.RotateLocal(0, 0, 1, 45);
            //arm.RotateLocal(0, 0, 1, -90);
            //arm.RotateLocal(0, 0, 1, 45);

            // Now lets try world Z
            arm.RotateGlobal(0, 0, 1, -90);
            //arm.RotateGlobal(0, 0, 1, -90);
            //arm.RotateGlobal(0, 0, 1, 45);

            for (int i = 0; i < 10; i++)
            {
                arm.MoveGlobal(0, -40, 0);
                arm.RotateLocal(Point.YAxis, 9);
            }

            // Back home
            arm.SetVelocity(100);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);
            arm.MoveTo(300, 0, 500);

        }

        public static void MovementTest(Robot arm)
        {
            // Reset
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            arm.SetVelocity(25);

            // Rotate TCP
            arm.RotateLocal(Point.XAxis, 45);

            // Do global movement
            arm.MoveGlobal(50, 0, 0);
            arm.MoveGlobal(0, 50, 0);
            arm.MoveGlobal(0, 0, 50);
            arm.MoveGlobal(-50, -50, -50);

            // Do local movement
            arm.MoveLocal(50, 0, 0);
            arm.MoveLocal(0, 50, 0);
            arm.MoveLocal(0, 0, 50);
            arm.MoveLocal(-50, -50, -50);

            // Back home
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);
        }

        public static void TestCircle(Robot arm)
        {
            // Reset
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);

            arm.MoveGlobal(150, 0, -100);

            for (var i = 0; i < 36; i++)
            {
                arm.MoveLocal(5 * Point.YAxis);
                arm.RotateLocal(Point.ZAxis, 10);
                arm.RotateLocal(Point.YAxis, -2);  // add some off plane movement ;)
            }

            // Back home
            arm.SetVelocity(100);
            arm.MoveTo(300, 0, 500);
            arm.RotateTo(-1, 0, 0, 0, 1, 0);
        }


        public static void TransformTests(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);

            arm.MoveGlobal(150, 0, -100);
            arm.SetVelocity(25);

            Rotation z30 = new Rotation(Point.ZAxis, 30);

            // Local tests, TR vs RT:
            arm.TransformLocal(50 * Point.XAxis, z30);                          // Note the T+R action order
            arm.TransformLocal(-50 * Point.XAxis, Rotation.Conjugate(z30));
            arm.TransformLocal(z30, 50 * Point.XAxis);                          // Note the R+T action order
            arm.TransformLocal(Rotation.Conjugate(z30), -50 * Point.XAxis);

            // Global tests, TR vs RT:
            // Action order is irrelevant in relative global mode (since translations are applied based on immutable world XYZ)
            arm.TransformGlobal(50 * Point.XAxis, z30);
            arm.TransformGlobal(-50 * Point.XAxis, Rotation.Conjugate(z30));
            arm.TransformGlobal(z30, 50 * Point.XAxis);
            arm.TransformGlobal(Rotation.Conjugate(z30), -50 * Point.XAxis);

            // Back home
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);
        }


        public static void TestCircleTransformAbsolute(Robot arm, double r)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SetVelocity(100);
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
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);

            double x = 450;
            double y = 0;
            double z = 400;

            arm.MoveTo(x, y, z);

            Point forward = side * Point.YAxis;
            Rotation zn10 = new Rotation(Point.ZAxis, -10);
            // Trace a circle with local coordinates and rotations
            for (var i = 0; i < 36; i++)
            {
                arm.TransformLocal(forward, zn10);
            }

            // Trace it back (and 'disentagle' Axis 6...)
            Point backward = -side * Point.YAxis;
            Rotation z10 = new Rotation(Point.ZAxis, 10);
            for (var i = 0; i < 36; i++)
            {
                arm.TransformLocal(backward, z10);
            }

            // Back home
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);
        }

        public static void TestCircleTransformGlobal(Robot arm, double side)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SetVelocity(100);
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
                arm.TransformGlobal(forward, z10);
            }

            // Trace it back (and 'disentagle' Axis 6...)
            Rotation zn10 = new Rotation(Point.ZAxis, -10);
            for (var i = 0; i > -36; i--)
            {
                Point forward = new Point(
                    side * Math.Sin(2 * Math.PI * i / 36.0),
                    -side * Math.Cos(2 * Math.PI * i / 36.0),
                    0);
                arm.TransformGlobal(forward, zn10);
            }

            // Back home
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);

        }

        static public void TestZTableLimitations(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            Rotation noRot = new Rotation();

            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);

            // Bring close to limit (100 by default)
            arm.TransformTo(new Point(300, 0, 101), homeXYZ);

            // Try to go trough (should not work)
            arm.TransformTo(new Point(300, 0, 99), homeXYZ);
            arm.TransformLocal(new Point(0, 0, 2), noRot);
            arm.TransformGlobal(new Point(0, 0, -2), noRot);

            // Back home
            arm.TransformTo(home, homeXYZ);
        }


        public static void TestSnake(Robot arm)
        {
            // Reset
            Point home = new Point(300, 0, 500);
            Rotation homeXYZ = new Rotation(-1, 0, 0, 0, 1, 0);
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);
            
            arm.MoveTo(300, -300, 250);

            arm.SetVelocity(200);
            for (int i = 0; i < 100; i++)
            {
                Rotation rz = new Rotation(Point.ZAxis, 15 * Math.Cos(2.0 * Math.PI * i / 25.0));
                arm.TransformLocal(rz, 8  * Point.YAxis);
            }

            // Back home
            arm.SetVelocity(100);
            arm.TransformTo(home, homeXYZ);
        }

        // DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF
        public static void TestJointMovementRange(Robot arm)
        {
            Console.WriteLine("WARNING: DO NOT RUN THIS PROGRAM ON A REAL ROBOT, YOU WILL MOST LIKELY HIT SOMETHING OR ITSELF");

            // Reset
            arm.SetVelocity(500);
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
            arm.SetVelocity(300);
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
            arm.SetVelocity(100);
            arm.JointsTo(0, 0, 0, 0, 90, 0);

            // Issue an absolute RT movement (should work)
            arm.TransformTo(new Point(200, 200, 200), Rotation.FlippedAroundY);

            // Issue a relative one (should work)
            arm.TransformGlobal(new Point(50, 0, 0), new Rotation(Point.XAxis, 45));

            // Issue a relative Joints one (SHOULDN'T WORK)
            arm.Joints(-45, 0, 0, 0, 0, 0);

            // Issue abs joints (should work)
            arm.JointsTo(45, 0, 0, 0, 90, 0);

            // Issue rel Joints (should work)
            arm.Joints(30, 0, 0, 0, 0, 0);

            // Issue a bunch of relative ones (NONE SHOULD WORK)
            arm.MoveLocal(100, 0, 0);
            arm.MoveGlobal(100, 0, 0);
            arm.RotateLocal(Point.XAxis, 45);
            arm.RotateGlobal(Point.XAxis, 45);
            arm.TransformLocal(new Point(100, 0, 0), new Rotation(Point.XAxis, 45));
            arm.TransformLocal(new Rotation(Point.XAxis, 45), new Point(100, 0, 0));
            arm.TransformGlobal(new Point(100, 0, 0), new Rotation(Point.XAxis, 45));
            arm.TransformGlobal(new Rotation(Point.XAxis, 45), new Point(100, 0, 0));

            // Rel joints (should work)
            arm.Joints(0, 30, 0, 0, 0, 0);

            // Issue absolute movement (SHOULD NOT WORK)
            arm.MoveTo(400, 0, 200);                // missing rotation information
            arm.RotateTo(Rotation.FlippedAroundY);  // missing point information

            // Back home (should work)
            arm.JointsTo(0, 0, 0, 0, 90, 0);
        }




    }
}
