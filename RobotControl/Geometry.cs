using System;
using System.Collections.Generic;

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
        /// Recommended as the easiest orientation for the standard robot end effector to reach in positive X octants.
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

        //// From Tait-Bryan angles (rations around X, Y, Z axes)
        // // See here: http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/
        //public Rotation (double roll, double pitch, double yaw)
        //{

        //}

        public override string ToString()
        {
            return "[" + this.Q1 + "," + this.Q2 + "," + this.Q3 + "," + this.Q4 + "]";
        }
    }



    //███████╗██████╗  █████╗ ███╗   ███╗███████╗
    //██╔════╝██╔══██╗██╔══██╗████╗ ████║██╔════╝
    //█████╗  ██████╔╝███████║██╔████╔██║█████╗  
    //██╔══╝  ██╔══██╗██╔══██║██║╚██╔╝██║██╔══╝  
    //██║     ██║  ██║██║  ██║██║ ╚═╝ ██║███████╗
    //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝                                      
    /// <summary>
    /// Represents a location and rotation in 3D space
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// This is the default rotation that will be assigned to Frames constructed only with location properties.
        /// </summary>
        public static Rotation DefaultOrientation = Rotation.FlippedAroundY;

        public Point Position;
        public Rotation Orientation;

        public Frame(double x, double y, double z)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = DefaultOrientation;
        }

        public Frame(double x, double y, double z, double q1, double q2, double q3, double q4)
        {
            this.Position = new Point(x, y, z);
            this.Orientation = new Rotation(q1, q2, q3, q4);
        }

        public Frame(Point position)
        {
            this.Position = position;
            this.Orientation = DefaultOrientation;
        }

        public Frame(Point position, Rotation orientation)
        {
            this.Position = position;
            this.Orientation = orientation;
        }

        public override string ToString()
        {
            return this.Position + "," + this.Orientation;
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
            List<Frame> newTargets = new List<Frame>();
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
