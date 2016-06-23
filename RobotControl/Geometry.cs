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
        public double x, y, z;

        public Point(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return "[" + this.x + "," + this.y + "," + this.z + "]";
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

        public double q1, q2, q3, q4;

        public Rotation(double q1, double q2, double q3, double q4)
        {
            this.q1 = q1;
            this.q2 = q2;
            this.q3 = q3;
            this.q4 = q4;
        }

        //// From Tait-Bryan angles (rations around X, Y, Z axes)
        // // See here: http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/
        //public Rotation (double roll, double pitch, double yaw)
        //{

        //}

        public override string ToString()
        {
            return "[" + this.q1 + "," + this.q2 + "," + this.q3 + "," + this.q4 + "]";
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

        public Point position;
        public Rotation orientation;

        public Frame(double x, double y, double z)
        {
            this.position = new Point(x, y, z);
            this.orientation = DefaultOrientation;
        }

        public Frame(double x, double y, double z, double q1, double q2, double q3, double q4)
        {
            this.position = new Point(x, y, z);
            this.orientation = new Rotation(q1, q2, q3, q4);
        }

        public Frame(Point position)
        {
            this.position = position;
            this.orientation = DefaultOrientation;
        }

        public Frame(Point position, Rotation orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }

        public override string ToString()
        {
            return this.position + "," + this.orientation;
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
        private List<Frame> targets;
        public int targetCount { get; private set; }

        public Path()
        {
            this.targets = new List<Frame>();
            targetCount = 0;
        }

        public void Add(Frame target)
        {
            this.targets.Add(target);
            targetCount++;
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
            return this.targets[index];
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
            foreach (Frame f in targets)
            {
                double x = f.position.x;
                f.position.x = f.position.y;
                f.position.y = x;
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
            if ( !a.Equals("x") && !a.Equals("y") && !a.Equals("z") )
            {
                Console.WriteLine("Please use 'x', 'y' or 'z' as arguments");
                return false;
            }

            int axid = a.Equals("x") ? 0 : a.Equals("y") ? 1 : 2;

            switch(axid)
            {
                case 0:
                    foreach (Frame f in targets)
                    {
                        f.position.x = Util.Remap(f.position.x, prevMin, prevMax, newMin, newMax);
                    }
                    break;
                case 1:
                    foreach (Frame f in targets)
                    {
                        f.position.y = Util.Remap(f.position.y, prevMin, prevMax, newMin, newMax);
                    }
                    break;
                default:
                    foreach (Frame f in targets)
                    {
                        f.position.z = Util.Remap(f.position.z, prevMin, prevMax, newMin, newMax);
                    }
                    break;
            }
            
            return true;
        }

    }

}
