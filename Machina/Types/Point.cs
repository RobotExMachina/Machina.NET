using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //██████╗  ██████╗ ██╗███╗   ██╗████████╗
    //██╔══██╗██╔═══██╗██║████╗  ██║╚══██╔══╝
    //██████╔╝██║   ██║██║██╔██╗ ██║   ██║   
    //██╔═══╝ ██║   ██║██║██║╚██╗██║   ██║   
    //██║     ╚██████╔╝██║██║ ╚████║   ██║   
    //╚═╝      ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   
    //
    /// <summary>
    /// Represents a three dimensional point. 
    /// This is just a cosmetic alias for Vectors in the Public API... Users may have a better
    /// time at first understanding position as Points and direction as Vectors...?
    /// </summary>
    public class Point : Geometry
    {
        /// <summary>
        /// Gets a Point at (0, 0, 0).
        /// </summary>
        public static Point Origin => new Point(0, 0, 0);

        /// <summary>
        /// X property of the Point.
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Y property of the Point.
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Z property of the Point.
        /// </summary>
        public double Z { get; internal set; }

        /// <summary>
        /// Implicit conversion to Vector object.
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator Vector(Point p)
        {
            return p == null ? null : new Machina.Vector(p.X, p.Y, p.Z);
        }

        /// <summary>
        /// Create a Point from its XYZ coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public override string ToString()
        {
            return string.Format("[{0},{1},{2}]",
                Math.Round(X, STRING_ROUND_DECIMALS_MM),
                Math.Round(Y, STRING_ROUND_DECIMALS_MM),
                Math.Round(Z, STRING_ROUND_DECIMALS_MM));
        }

    }
}
