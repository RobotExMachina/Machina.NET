using System;
using System.Collections.Generic;

using ABB.Robotics.Controllers.RapidDomain;


//██████╗  █████╗ ████████╗ █████╗ ████████╗██╗   ██╗██████╗ ███████╗███████╗
//██╔══██╗██╔══██╗╚══██╔══╝██╔══██╗╚══██╔══╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔════╝
//██║  ██║███████║   ██║   ███████║   ██║    ╚████╔╝ ██████╔╝█████╗  ███████╗
//██║  ██║██╔══██║   ██║   ██╔══██║   ██║     ╚██╔╝  ██╔═══╝ ██╔══╝  ╚════██║
//██████╔╝██║  ██║   ██║   ██║  ██║   ██║      ██║   ██║     ███████╗███████║
//╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝     ╚══════╝╚══════╝
/// <summary>
/// A bunch of utility classes mostly representing geometry and robot instructions.
/// </summary>


namespace RobotControl
{


    //██████╗  ██████╗ ██╗███╗   ██╗████████╗
    //██╔══██╗██╔═══██╗██║████╗  ██║╚══██╔══╝
    //██████╔╝██║   ██║██║██╔██╗ ██║   ██║   
    //██╔═══╝ ██║   ██║██║██║╚██╗██║   ██║   
    //██║     ╚██████╔╝██║██║ ╚████║   ██║   
    //╚═╝      ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝                                  
    /// <summary>
    /// Represents three coordinates in space.
    /// </summary>
    public class Point
    {
        public double X, Y, Z;

        public Point(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Point(Pos pos)
        {
            this.X = pos.X;
            this.Y = pos.Y;
            this.Z = pos.Z;
        }

        public Point(RobTarget rt)
        {
            this.X = rt.Trans.X;
            this.Y = rt.Trans.Y;
            this.Z = rt.Trans.Z;
        }

        public void Set(double newX, double newY, double newz)
        {
            this.X = newX;
            this.Y = newY;
            this.Z = newz;
        }

        public void Add(double incX, double incY, double incZ)
        {
            this.X += incX;
            this.Y += incY;
            this.Z += incZ;
        }

        public void Add(Point p)
        {
            this.X += p.X;
            this.Y += p.Y;
            this.Z += p.Z;
        }

        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        public double LengthSq()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        public bool Normalize()
        {
            var len = this.Length();
            if (len == 0) return false;
            this.X /= len;
            this.Y /= len;
            this.Z /= len;
            return true;
        }

        public void Scale(double factor)
        {
            this.X *= factor;
            this.Y *= factor;
            this.Z *= factor;
        }

        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            //if (obj.GetType() != typeof(Point) && obj.GetType() != typeof(Point))
            if (obj.GetType() != typeof(Point))
                return false;
            return Equals(obj as Point);
        }

        /// <summary>
        /// Equality checks.
        /// </summary>
        /// <ref>https://github.com/imshz/simplify-net</ref>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public override string ToString()
        {
            return "[" + this.X + "," + this.Y + "," + this.Z + "]";
        }
    }


    //██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    /// <summary>
    /// Represents a rotation using quaternions.
    /// </summary>
    public class Rotation
    {
        /// <summary>
        /// The orientation of a global XYZ coordinate system.
        /// </summary>
        public static readonly Rotation GlobalXY = new Rotation(1, 0, 0, 0);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its X axis.
        /// </summary>
        public static readonly Rotation FlippedAroundX = new Rotation(0, 1, 0, 0);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its Y axis. 
        /// Recommended as the easiest orientation for the standard robot end effector to reach in positive XY octants.
        /// </summary>
        public static readonly Rotation FlippedAroundY = new Rotation(0, 0, 1, 0);

        /// <summary>
        /// A global XYZ coordinate system rotated 180 degs around its Z axis.
        /// </summary>
        public static readonly Rotation FlippedAroundZ = new Rotation(0, 0, 0, 1);

        public double Q1, Q2, Q3, Q4;

        public Rotation(double q1, double q2, double q3, double q4)
        {
            this.Q1 = q1;
            this.Q2 = q2;
            this.Q3 = q3;
            this.Q4 = q4;
        }

        public Rotation(Orient or)
        {
            this.Q1 = or.Q1;
            this.Q2 = or.Q2;
            this.Q3 = or.Q3;
            this.Q4 = or.Q4;
        }

        public Rotation(RobTarget rt)
        {
            this.Q1 = rt.Rot.Q1;
            this.Q2 = rt.Rot.Q2;
            this.Q3 = rt.Rot.Q3;
            this.Q4 = rt.Rot.Q4;
        }

        public void Set(double q1, double q2, double q3, double q4)
        {
            this.Q1 = q1;
            this.Q2 = q2;
            this.Q3 = q3;
            this.Q4 = q4;
        }

        public void Set(List<double> q)
        {
            this.Q1 = q[0];
            this.Q2 = q[1];
            this.Q3 = q[2];
            this.Q4 = q[3];
        }

        /// <summary>
        /// Borrowed from DynamoTORO
        /// </summary>
        /// <param name="XAxis"></param>
        /// <param name="YAxis"></param>
        /// <param name="Normal"></param>
        /// <returns></returns>
        public static List<double> PlaneToQuaternions(double x1, double x2, double x3, double y1, double y2, double y3, double z1, double z2, double z3)
        {

            //Point origin = plane.Origin;
            //Vector xVect = plane.XAxis;
            //Vector yVect = plane.YAxis;
            //Vector zVect = plane.Normal;

            double s, trace;
            //double x1, x2, x3, y1, y2, y3, z1, z2, z3;
            double q1, q2, q3, q4;

            //x1 = xVect.X;
            //x2 = xVect.Y;
            //x3 = xVect.Z;
            //y1 = yVect.X;
            //y2 = yVect.Y;
            //y3 = yVect.Z;
            //z1 = zVect.X;
            //z2 = zVect.Y;
            //z3 = zVect.Z;

            trace = x1 + y2 + z3 + 1;

            if (trace > 0.00001)
            {
                // s = (trace) ^ (1 / 2) * 2
                s = Math.Sqrt(trace) * 2;
                q1 = s / 4;
                q2 = (-z2 + y3) / s;
                q3 = (-x3 + z1) / s;
                q4 = (-y1 + x2) / s;

            }
            else if (x1 > y2 && x1 > z3)
            {
                //s = (x1 - y2 - z3 + 1) ^ (1 / 2) * 2
                s = Math.Sqrt(x1 - y2 - z3 + 1) * 2;
                q1 = (z2 - y3) / s;
                q2 = s / 4;
                q3 = (y1 + x2) / s;
                q4 = (x3 + z1) / s;

            }
            else if (y2 > z3)
            {
                //s = (-x1 + y2 - z3 + 1) ^ (1 / 2) * 2
                s = Math.Sqrt(-x1 + y2 - z3 + 1) * 2;
                q1 = (x3 - z1) / s;
                q2 = (y1 + x2) / s;
                q3 = s / 4;
                q4 = (z2 + y3) / s;
            }

            else
            {
                //s = (-x1 - y2 + z3 + 1) ^ (1 / 2) * 2
                s = Math.Sqrt(-x1 - y2 + z3 + 1) * 2;
                q1 = (y1 - x2) / s;
                q2 = (x3 + z1) / s;
                q3 = (z2 + y3) / s;
                q4 = s / 4;

            }

            List<double> quatDoubles = new List<double>();
            quatDoubles.Add(q1);
            quatDoubles.Add(q2);
            quatDoubles.Add(q3);
            quatDoubles.Add(q4);
            return quatDoubles;

            //return new Rotation(q1, q2, q3, q4);
        }

        public override string ToString()
        {
            return "[" + this.Q1 + "," + this.Q2 + "," + this.Q3 + "," + this.Q4 + "]";
        }

    }


    //     ██╗ ██████╗ ██╗███╗   ██╗████████╗███████╗
    //     ██║██╔═══██╗██║████╗  ██║╚══██╔══╝██╔════╝
    //     ██║██║   ██║██║██╔██╗ ██║   ██║   ███████╗
    //██   ██║██║   ██║██║██║╚██╗██║   ██║   ╚════██║
    //╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   ███████║
    // ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
    /// <summary>
    /// Represents the 6 angular rotations of the axes in a 6-axis manipulator
    /// </summary>
    public class Joints
    {
        public double J1, J2, J3, J4, J5, J6;

        /// <summary>
        /// Create a Joints configuration from values.
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <param name="j4"></param>
        /// <param name="j5"></param>
        /// <param name="j6"></param>
        public Joints(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            this.J1 = j1;
            this.J2 = j2;
            this.J3 = j3;
            this.J4 = j4;
            this.J5 = j5;
            this.J6 = j6;
        }

        /// <summary>
        /// Create a Joints configuration from an ABB JointTarget object.
        /// </summary>
        /// <param name="rj"></param>
        public Joints(RobJoint rj)
        {
            this.J1 = rj.Rax_1;
            this.J2 = rj.Rax_2;
            this.J3 = rj.Rax_3;
            this.J4 = rj.Rax_4;
            this.J5 = rj.Rax_5;
            this.J6 = rj.Rax_6;
        }

        public Joints(JointTarget jt)
        {
            this.J1 = jt.RobAx.Rax_1;
            this.J2 = jt.RobAx.Rax_2;
            this.J3 = jt.RobAx.Rax_3;
            this.J4 = jt.RobAx.Rax_4;
            this.J5 = jt.RobAx.Rax_5;
            this.J6 = jt.RobAx.Rax_6;
        }

        /// <summary>
        /// Returns the norm (euclidean length) of this joints as a vector.
        /// </summary>
        /// <returns></returns>
        public double Norm()
        {
            return Math.Sqrt(this.NormSq());
        }

        /// <summary>
        /// Returns the square norm of this joints as a vector.
        /// </summary>
        /// <returns></returns>
        public double NormSq()
        {
            return J1 * J1 + J2 * J2 + J3 * J3 + J4 * J4 + J5 * J5 + J6 * J6;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3},{4},{5}]", J1, J2, J3, J4, J5, J6);
        }

    }



    //███████╗██████╗  █████╗ ███╗   ███╗███████╗
    //██╔════╝██╔══██╗██╔══██╗████╗ ████║██╔════╝
    //█████╗  ██████╔╝███████║██╔████╔██║█████╗  
    //██╔══╝  ██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝  
    //██║     ██║  ██║██║  ██║██║ ╚═╝ ██║███████╗
    //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝                                      
    /// <summary>
    /// Represents a location and rotation in 3D space, with some additional
    /// metadata representing velocities, zones, etc.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// This is the default rotation that will be assigned to Frames constructed only with location properties.
        /// </summary>
        public static Rotation DefaultOrientation = Rotation.FlippedAroundY;
        public static double DefaultVelocity = 10;
        public static double DefaultZone = 5;

        public static double DistanceBetween(Frame f1, Frame f2)
        {
            double dx = f2.Position.X - f1.Position.X;
            double dy = f2.Position.Y - f1.Position.Y;
            double dz = f2.Position.Z - f1.Position.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Point Position;
        public Rotation Orientation;
        public double Velocity;
        public double Zone;

        public Frame(double x, double y, double z)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
            this.Velocity = DefaultVelocity;
            this.Zone = DefaultZone;
        }

        public Frame(double x, double y, double z, double vel, double zon)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
            this.Velocity = vel;
            this.Zone = zon;
        }

        public Frame(double x, double y, double z, double q1, double q2, double q3, double q4)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(q1, q2, q3, q4);
            this.Velocity = DefaultVelocity;
            this.Zone = DefaultZone;
        }

        public Frame(double x, double y, double z, double q1, double q2, double q3, double q4, double vel, double zon)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(q1, q2, q3, q4);
            this.Velocity = vel;
            this.Zone = zon;
        }

        public Frame(Point position)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = DefaultOrientation;
            this.Velocity = DefaultVelocity;
            this.Zone = DefaultZone;
        }

        public Frame(Point position, double vel, double zon)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = DefaultOrientation;
            this.Velocity = vel;
            this.Zone = zon;
        }

        public Frame(Point position, Rotation orientation)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = new Rotation(orientation.Q1, orientation.Q2, orientation.Q3, orientation.Q4);  // shallow copy
            this.Velocity = DefaultVelocity;
            this.Zone = DefaultZone;
        }

        public Frame(Point position, Rotation orientation, double vel, double zon)
        {
            this.Position = new Point(position.X, position.Y, position.Z);  // shallow copy
            this.Orientation = new Rotation(orientation.Q1, orientation.Q2, orientation.Q3, orientation.Q4);  // shallow copy
            this.Velocity = vel;
            this.Zone = zon;
        }

        public Frame(RobTarget robTarget)
        {
            this.Position = new Point(robTarget.Trans);
            this.Orientation = new Rotation(robTarget.Rot);
            // temporarily use 'invalid' values to denote this is not getting these values from the robot... 
            this.Velocity = -1;
            this.Zone = -1;
        }
        
        public string GetPositionDeclaration()
        {
            return string.Format("[{0},{1},{2}]", Position.X, Position.Y, Position.Z);
        }

        public string GetOrientationDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", Orientation.Q1, Orientation.Q2, Orientation.Q3, Orientation.Q4);
        }

        /// <summary>
        /// WARNING: This library still doesn't perform IK calculation, and will always use
        /// a default [0,0,0,0] axis configuration.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFEConfigurationDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", 0, 0, 0, 0);
        }

        /// <summary>
        /// WARNING: no external axes are taken into account here... 
        /// </summary>
        /// <returns></returns>
        public string GetExternalAxesDeclaration()
        {
            return "[9E9,9E9,9E9,9E9,9E9,9E9]";
        }

        public string GetSpeedDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", Velocity, Velocity, 5000, 1000);  // default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). using the same value as lin motion
        }

        public string GetZoneDeclaration()
        {
            double high = 1.5 * Zone;
            double low = 0.15 * Zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", Zone, high, high, low, high, low);
        }

        /// <summary>
        /// WARNING: This library still doesn't perform IK calculation, and will always return
        /// a default [0,0,0,0] axis configuration.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFERobTargetDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", 
                GetPositionDeclaration(),
                GetOrientationDeclaration(),
                GetUNSAFEConfigurationDeclaration(),
                GetExternalAxesDeclaration()
            );
        }

        public void FlipXY()
        {
            double x = this.Position.X;
            this.Position.X = this.Position.Y;
            this.Position.Y = x;
        }

        public void FlipYZ()
        {
            double y = this.Position.Y;
            this.Position.Y = this.Position.Z;
            this.Position.Z = y;
        }

        public void FlipXZ()
        {
            double x = this.Position.X;
            this.Position.X = this.Position.Z;
            this.Position.Z = x;
        }

        public void ReverseX()
        {
            this.Position.X = -this.Position.X;
        }

        public void ReverseY()
        {
            this.Position.Y = -this.Position.Y;
        }

        public void ReverseZ()
        {
            this.Position.Z = -this.Position.Z;
        }

        public bool RemapAxis(string axis, double prevMin, double prevMax, double newMin, double newMax)
        {
            string a = axis.ToLower();
            //Some sanity
            if (!a.Equals("x") && !a.Equals("y") && !a.Equals("z"))
            {
                Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
                return false;
            }

            int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

            switch (axid)
            {
                case 0:
                    this.Position.X = Util.Remap(this.Position.X, prevMin, prevMax, newMin, newMax);
                    break;
                case 1:
                    this.Position.Y = Util.Remap(this.Position.Y, prevMin, prevMax, newMin, newMax);
                    break;
                default:
                    this.Position.Z = Util.Remap(this.Position.Z, prevMin, prevMax, newMin, newMax);
                    break;
            }

            return true;
        }


        public override string ToString()
        {
            //return this.Position + "," + this.Orientation;
            return string.Format("{0},{1},{2},{3}", this.Position, this.Orientation, this.Velocity, this.Zone);
        }
    }


    //██████╗  █████╗ ████████╗██╗  ██╗
    //██╔══██╗██╔══██╗╚══██╔══╝██║  ██║
    //██████╔╝███████║   ██║   ███████║
    //██╔═══╝ ██╔══██║   ██║   ██╔══██║
    //██║     ██║  ██║   ██║   ██║  ██║
    //╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝
    /// <summary>
    /// Represents an ordered sequence of target Frames
    /// </summary>
    public class Path
    {
        public string Name;
        private List<Frame> Targets;
        public int Count { get; private set; }

        public Path() : this("defaultPath") { }

        public Path(string name)
        {
            this.Name = name;
            this.Targets = new List<Frame>();
            Count = 0;
        }

        public void Add(Frame target)
        {
            this.Targets.Add(target);
            Count++;
        }

        public void Add(Point position)
        {
            this.Add(new Frame(position));
        }

        public void Add(double x, double y, double z)
        {
            this.Add(new Frame(x, y, z));
        }

        public void Add(Point position, Rotation orientation)
        {
            this.Add(new Frame(position, orientation));
        }

        public Frame GetTarget(int index)
        {
            return this.Targets[index];
        }

        public Frame GetFirstTarget()
        {
            return this.Targets[0];
        }

        public Frame GetLastTarget()
        {
            return this.Targets[this.Targets.Count - 1];
        }

        private void UpadateTargetCount()
        {
            Count = Targets.Count;
        }

        /// <summary>
        /// Flips the XY coordinates of all target frames.
        /// </summary>
        public void FlipXY()
        {
            foreach (Frame f in Targets)
            {
                //double x = f.Position.X;
                //f.Position.X = f.Position.Y;
                //f.Position.Y = x;
                f.FlipXY();
            }
        }

        /// <summary>
        /// Remaps the coordinates of all target frames from a source to a target domain.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="prevMin"></param>
        /// <param name="prevMax"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns></returns>
        public bool RemapAxis(string axis, double prevMin, double prevMax, double newMin, double newMax)
        {
            string a = axis.ToLower();
            //Some sanity
            if (!a.Equals("x") && !a.Equals("y") && !a.Equals("z"))
            {
                Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
                return false;
            }

            int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

            switch (axid)
            {
                case 0:
                    foreach (Frame f in Targets)
                    {
                        f.Position.X = Util.Remap(f.Position.X, prevMin, prevMax, newMin, newMax);
                    }
                    break;
                case 1:
                    foreach (Frame f in Targets)
                    {
                        f.Position.Y = Util.Remap(f.Position.Y, prevMin, prevMax, newMin, newMax);
                    }
                    break;
                default:
                    foreach (Frame f in Targets)
                    {
                        f.Position.Z = Util.Remap(f.Position.Z, prevMin, prevMax, newMin, newMax);
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Simplifies the path using a combination of radial distance and 
        /// Ramer–Douglas–Peucker algorithm. 
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="tolerance"></param>
        /// <param name="highQuality"></param>
        /// <returns></returns>
        public bool Simplify(double tolerance, bool highQuality)
        {

            if (Count < 1)
            {
                Console.WriteLine("Path contains no targets.");
                return false;
            }

            int prev = Count;

            double sqTolerance = tolerance * tolerance;

            if (!highQuality)
            {
                SimplifyRadialDistance(sqTolerance);
            }

            SimplifyDouglasPeucker(sqTolerance);

            Console.WriteLine("Path " + this.Name + " simplified from " + prev + " to " + Count +" targets.");

            return true;
        }

        /// <summary>
        /// The RDP algorithm.
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="tolerance"></param>
        /// <param name="highQuality"></param>
        /// <returns></returns>
        private void SimplifyDouglasPeucker(double sqTolerance)
        {
            var len = Count;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newTargets = new List<Frame>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (int? i = first + 1; i < last; i++)
                {
                    var sqDist = Util.GetSquareSegmentDistance(Targets[i.Value].Position,
                        Targets[first.Value].Position, Targets[last.Value].Position);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    markers[index.Value] = 1;
                    stack.AddRange(new[] { first, index, index, last });
                }

                if (stack.Count > 0)
                {
                    last = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    last = null;
                }

                if (stack.Count > 0)
                {
                    first = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    first = null;
                }
            }


            for (int i = 0; i < len; i++)
            {
                if (markers[i] != null)
                {
                    newTargets.Add(Targets[i]);
                }
            }

            Targets = newTargets;
            UpadateTargetCount();
        }

        /// <summary>
        /// Simple distance-based simplification. Consecutive points under 
        /// threshold distance are removed. 
        /// </summary>
        /// <ref>Adapted from https://github.com/imshz/simplify-net </ref>
        /// <param name="tolerance"></param>
        /// <param name="highQuality"></param>
        /// <returns></returns>
        private void SimplifyRadialDistance(double sqTolerance)
        {
            Frame prevFrame = Targets[0];
            List<Frame> newTargets = new List<Frame> { prevFrame };
            Frame frame = null;

            for (int i = 1; i < Targets.Count; i++)
            {
                frame = Targets[i];

                if (Util.GetSquareDistance(frame.Position, prevFrame.Position) > sqTolerance)
                {
                    newTargets.Add(frame);
                    prevFrame = frame;
                }
            }

            // Add the last frame of the path (?)
            if (frame != null && !prevFrame.Position.Equals(frame.Position))
            {
                newTargets.Add(frame);
            }

            Targets = newTargets;
            UpadateTargetCount();
        }
    }









    //███████╗██████╗  █████╗ ████████╗██╗ █████╗ ██╗         ████████╗██████╗ ██╗ ██████╗  ██████╗ ███████╗██████╗ ███████╗
    //██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔══██╗██║         ╚══██╔══╝██╔══██╗██║██╔════╝ ██╔════╝ ██╔════╝██╔══██╗██╔════╝
    //███████╗██████╔╝███████║   ██║   ██║███████║██║            ██║   ██████╔╝██║██║  ███╗██║  ███╗█████╗  ██████╔╝███████╗
    //╚════██║██╔═══╝ ██╔══██║   ██║   ██║██╔══██║██║            ██║   ██╔══██╗██║██║   ██║██║   ██║██╔══╝  ██╔══██╗╚════██║
    //███████║██║     ██║  ██║   ██║   ██║██║  ██║███████╗       ██║   ██║  ██║██║╚██████╔╝╚██████╔╝███████╗██║  ██║███████║
    //╚══════╝╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═╝╚══════╝       ╚═╝   ╚═╝  ╚═╝╚═╝ ╚═════╝  ╚═════╝ ╚══════╝╚═╝  ╚═╝╚══════╝

    public class SpatialTrigger
    {
        virtual public void Check(Robot robot)
        {

        }
    }

    public class SpatialTriggerBox : SpatialTrigger
    {
        Point p0;
        Point p1;

        public bool isTriggered = false;

        public override void Check(Robot robot)
        {

        }
    }

    public class SpatialTriggerPointProximity : SpatialTrigger
    {
        Point p0;
        double r;
        public override void Check(Robot robot)
        {
        }
    }


}
