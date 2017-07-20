using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{
    //   █████╗ ██╗  ██╗██╗███████╗ █████╗ ███╗   ██╗ ██████╗ ██╗     ███████╗
    //  ██╔══██╗╚██╗██╔╝██║██╔════╝██╔══██╗████╗  ██║██╔════╝ ██║     ██╔════╝
    //  ███████║ ╚███╔╝ ██║███████╗███████║██╔██╗ ██║██║  ███╗██║     █████╗  
    //  ██╔══██║ ██╔██╗ ██║╚════██║██╔══██║██║╚██╗██║██║   ██║██║     ██╔══╝  
    //  ██║  ██║██╔╝ ██╗██║███████║██║  ██║██║ ╚████║╚██████╔╝███████╗███████╗
    //  ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝
    //                                                                        

    /// <summary>
    /// A class representing a spatial otation as an Axis-Angle:
    /// an unit axis vector and the rotation angle.
    /// </summary>
    public class AxisAngle : Geometry
    {
        /// <summary>
        /// AxisAngle properties.
        /// </summary>
        public double X, Y, Z, Angle;

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="aa1"></param>
        /// <param name="aa2"></param>
        /// <returns></returns>
        public static bool operator ==(AxisAngle aa1, AxisAngle aa2)
        {
            return Math.Abs(aa1.X - aa2.X) < EPSILON
                && Math.Abs(aa1.Y - aa2.Y) < EPSILON
                && Math.Abs(aa1.Z - aa2.Z) < EPSILON
                && Math.Abs(aa1.Angle - aa2.Angle) < EPSILON;
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="aa1"></param>
        /// <param name="aa2"></param>
        /// <returns></returns>
        public static bool operator !=(AxisAngle aa1, AxisAngle aa2)
        {
            return Math.Abs(aa1.X - aa2.X) > EPSILON
                || Math.Abs(aa1.Y - aa2.Y) > EPSILON
                || Math.Abs(aa1.Z - aa2.Z) > EPSILON
                || Math.Abs(aa1.Angle - aa2.Angle) > EPSILON;
        }

        /// <summary>
        /// Create an AxisAngle representation of a spatial rotation from the XYZ components of the rotation axis, 
        /// and the rotation agle in degrees. 
        /// The axis vector will be automatically normalized.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="angleDegs"></param>
        public AxisAngle(double x, double y, double z, double angleDegs)
            : this(x, y, z, angleDegs, true) { }

        /// <summary>
        /// Create an AxisAngle representation of a spatial rotation from the XYZ components of the rotation axis, 
        /// and the rotation agle in degrees. 
        /// The axis vector will be automatically normalized.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angleDegs"></param>
        public AxisAngle(Point axis, double angleDegs)
            : this(axis.X, axis.Y, axis.Z, angleDegs, true) { }

        /// <summary>
        /// Private constructor, can bypass normalization if input axis is ensured to be normalized. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="angleDegs"></param>
        /// <param name="normalize"></param>
        protected AxisAngle(double x, double y, double z, double angleDegs, bool normalize)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Angle = angleDegs;

            if (normalize)
            {
                this.Normalize();
            }
        }

        /// <summary>
        /// Make the Axis a vector unit.
        /// </summary>
        public bool Normalize()
        {
            double len = Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);

            if (len < EPSILON)
            {
                this.X = 0;
                this.Y = 0;
                this.Z = 0;

                return false;
            }

            this.X /= len;
            this.Y /= len;
            this.Z /= len;

            return true;
        }

        /// <summary>
        /// Returns true if this AxisAngle represents no rotation: zero rotation axis or zero angle.
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Math.Abs(this.Angle) < EPSILON
                || (Math.Abs(this.X) < EPSILON
                    && Math.Abs(this.Y) < EPSILON
                    && Math.Abs(this.Z) < EPSILON);
        }

        /// <summary>
        /// Returns the length of the rotation axis.
        /// </summary>
        /// <returns></returns>
        public double AxisLength()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        /// <summary>
        /// Transforms this AxisAngle into a rotation around the same axis, but with a rotation angle
        /// between 0 and 360. Note the axis vector will be flipped if the original angle was negative. 
        /// </summary>
        public void Modulate()
        {
            this.Angle %= 360;
            if (this.Angle < 0)
            {
                this.Flip();
            }
        }

        /// <summary>
        /// Flip this AxisAngle to represent the same rotation with inverted vector and opposite angle.
        /// </summary>
        public void Flip()
        {
            this.X *= -1;
            this.Y *= -1;
            this.Z *= -1;
            this.Angle *= -1;
        }

        /// <summary>
        /// Returns a unit Quaternion representing this AxisAngle rotation.
        /// </summary>
        /// <returns></returns>
        public Quaternion ToQuaternion()
        {
            // If this AxisAngle performs no rotation, return an identity Quaternion
            if (this.IsZero())
            {
                return new Quaternion(1, 0, 0, 0);
            }

            double a2 = 0.5 * TO_RADS * this.Angle;
            double s = Math.Sin(a2);

            // Remember that axis vector is already normalized ;)
            return new Quaternion(
                Math.Cos(a2),
                s * this.X,
                s * this.Y,
                s * this.Z,
                false);
        }

        public override string ToString()
        {
            return string.Format("AxisAngle[{0}, {1}, {2}, {3}]",
                Math.Round(X, STRING_ROUND_DECIMALS_MM),
                Math.Round(Y, STRING_ROUND_DECIMALS_MM),
                Math.Round(Z, STRING_ROUND_DECIMALS_MM),
                Math.Round(Angle, STRING_ROUND_DECIMALS_DEGS));
        }
    }
}
