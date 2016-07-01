using System;
using System.Collections.Generic;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.FileSystemDomain;

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

        public Point(Pos robotPosition)
        {
            this.X = robotPosition.X;
            this.Y = robotPosition.Y;
            this.Z = robotPosition.Z;
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





        public override string ToString()
        {
            return "[" + this.Q1 + "," + this.Q2 + "," + this.Q3 + "," + this.Q4 + "]";
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

    }



    //███████╗██████╗  █████╗ ███╗   ███╗███████╗
    //██╔════╝██╔══██╗██╔══██╗████╗ ████║██╔════╝
    //█████╗  ██████╔╝███████║██╔████╔██║█████╗  
    //██╔══╝  ██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝  
    //██║     ██║  ██║██║  ██║██║ ╚═╝ ██║███████╗
    //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝                                      
    /// <summary>
    /// Represents a location and rotation in 3D space, with some additional
    /// metadata representing velocities, zones, etc
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// This is the default rotation that will be assigned to Frames constructed only with location properties.
        /// </summary>
        public static Rotation DefaultOrientation = Rotation.FlippedAroundY;
        public static double DefaultVelocity = 10;
        public static double DefaultZone = 5;

        public Point Position;
        public Rotation Orientation;
        public double Velocity;
        public double Zone;

        public static double DistanceBetween(Frame f1, Frame f2)
        {
            double dx = f2.Position.X - f1.Position.X;
            double dy = f2.Position.Y - f1.Position.Y;
            double dz = f2.Position.Z - f1.Position.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Frame(double x, double y, double z)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
            this.Velocity = DefaultVelocity;
            this.Zone = DefaultZone;
        }

        public Frame(double x, double y, double z, double q1, double q2, double q3, double q4)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(q1, q2, q3, q4);
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


        public string GetPositionDeclaration()
        {
            return string.Format("[{0},{1},{2}]", Position.X, Position.Y, Position.Z);
        }

        public string GetOrientationDeclaration()
        {
            return string.Format("[{0},{1},{2},{3}]", Orientation.Q1, Orientation.Q2, Orientation.Q3, Orientation.Q4);
        }

        /// <summary>
        /// WARNING: This method still doesn't perform IK calculation, and will always return
        /// a default declaration for the positive XYZ octant.
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
            return string.Format("[{0},{1},{2},{3}]", Velocity, 500, 5000, 1000);
        }

        public string GetZoneDeclaration()
        {
            double high = 1.5 * Zone;
            double low = 0.15 * Zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", Zone, high, high, low, high, low);
        }

        /// <summary>
        /// WARNING: This method still doesn't perform IK calculation, and will always return
        /// a default configuration for the positive XYZ octant.
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




    ////████████╗ █████╗ ██████╗  ██████╗ ███████╗████████╗
    ////╚══██╔══╝██╔══██╗██╔══██╗██╔════╝ ██╔════╝╚══██╔══╝
    ////   ██║   ███████║██████╔╝██║  ███╗█████╗     ██║   
    ////   ██║   ██╔══██║██╔══██╗██║   ██║██╔══╝     ██║   
    ////   ██║   ██║  ██║██║  ██║╚██████╔╝███████╗   ██║   
    ////   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝   ╚═╝   
    ///// <summary>
    ///// Represents a Position + Orientation with some additional 
    ///// properties such as velocity, zone, etc.
    ///// </summary>
    //public class Target
    //{
    //    public Point position;
    //    public Rotation orientation;
    //    public double velocity;
    //    public double zone;

    //    public static Rotation DefaultOrientation = Rotation.FlippedAroundY;

    //    public Target(Point pos, Rotation orient, double vel, double zone)
    //    {
    //        this.position = pos;
    //        this.orientation = orient;
    //        this.velocity = vel;
    //        this.zone = zone;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("[{0},{1},{2},{3}"])
    //    }
    //}   
    
                                            



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

        public Path() : this("") { }

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

        //public bool Flip(string firstAxis, string secondAxis)
        //{
        //    string a = firstAxis.ToLower();
        //    string b = secondAxis.ToLower();

        //    // Some sanity
        //    if ( (!a.Equals("x") && !a.Equals("y") && !a.Equals("z")) 
        //        || (!b.Equals("x") && !b.Equals("y") && !b.Equals("z")))
        //    {
        //        Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Flips the XY coordinates of all target frames.
        /// </summary>
        public void FlipXY()
        {
            foreach (Frame f in Targets)
            {
                double x = f.Position.X;
                f.Position.X = f.Position.Y;
                f.Position.Y = x;
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
        /// <ref>Adapted from https://github.com/imshz/simplify-net</ref>
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
        /// <ref>Adapted from https://github.com/imshz/simplify-net</ref>
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
        /// <ref>Adapted from https://github.com/imshz/simplify-net</ref>
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


}
