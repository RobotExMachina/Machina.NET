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
    /// A class representing a spatial rotation as an Axis-Angle:
    /// an unit axis vector and the rotation angle.
    /// </summary>
    public class AxisAngle : Geometry
    {
        /// <summary>
        /// X coordinate of the rotation vector.
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Y coordinate of the rotation vector.
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Z coordinate of the rotation vector.
        /// </summary>
        public double Z { get; internal set; }

        /// <summary>
        /// Rotation angle in degrees.
        /// </summary>
        public double Angle { get; internal set; }


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
        /// Implicitn conversion to Vector object.
        /// </summary>
        /// <param name="aa"></param>
        public static implicit operator Point(AxisAngle aa)
        {
            return new BRobot.Point(aa.X, aa.Y, aa.Z);
        }

        /// <summary>
        /// Creates an AxisAngle representing no rotation.
        /// </summary>
        public AxisAngle() : this(0, 0, 0, 0, false) { }

        /// <summary>
        /// Create an AxisAngle representation of a spatial rotation from the XYZ components of the rotation axis, 
        /// and the rotation angle in degrees. 
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
        /// and the rotation angle in degrees. 
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
        internal AxisAngle(double x, double y, double z, double angleDegs, bool normalize)
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
        /// Returns true if this AxisAngle represents no rotation: zero rotation axis or zero (modulated) angle.
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Math.Abs(this.Angle % 360) < EPSILON
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
        /// Is this rotation equivalent to a given one? 
        /// Equivalence is defined as rotations around vectors sharing the same axis (including opposite directions)
        /// and an angle with the same modulated equivalence. This in turn means the same spatial orientation after transformation.
        /// For example, the following rotations are equivalent:
        /// [0, 0, 1, 315]
        /// [0, 0, 1, 675] (one additional turn, same angle)
        /// [0, 0, 1, -45] (negative rotation, same angle)
        /// [0, 0, -1, 45] (flipped axis, same angle)
        /// [0, 0, 10, 315] (same axis and angle, longer vector. note non-unit vectors are not allowed in this AA representation)
        /// 
        /// Also, these are equivalent:
        /// [0, 0, 0, 0]
        /// [0, 0, 1, 720] 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEquivalent(AxisAngle other)
        {
            // Sanity checks
            if (this.IsZero())
            {
                return Math.Abs(other.Angle % 360) < EPSILON;
            } 
            else if (other.IsZero())
            {
                return Math.Abs(this.Angle % 360) < EPSILON;
            }

            //Point v1 = new Point(this.X, this.Y, this.Z),
            //v2 = new Point(axisAngle.X, axisAngle.Y, axisAngle.Z);
            //int directions = Point.CompareDirections(v1, v2);
            int directions = Point.CompareDirections(this, other);
            
            // If axes are not parallel, they are not equivalent
            if (directions == 0 || directions == 2)
            {
                return false;
            }

            // Bring all angles to [0, 360]
            double a1 = this.Angle;
            while (a1 < 0)
            {
                a1 += 360;
            }
            a1 %= 360;
            if (Math.Abs(a1 - 360) < EPSILON) a1 = 0;

            double a2 = other.Angle;
            while (a2 < 0)
            {
                a2 += 360;
            }
            a2 %= 360;
            if (Math.Abs(a2 - 360) < EPSILON) a2 = 0;

            // If the vectors have the same direction, angles should be module of each other.
            if (directions == 1)
            {
                return Math.Abs(a1 - a2) < EPSILON;
            }

            // If opposite directions, they should add up to 360 degs.
            if (directions == 3)
            {
                return Math.Abs(a1 + a2 - 360) < EPSILON;
            }

            return false;  // if here, something went wrong
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

        /// <summary>
        /// Returns a RotationVector representing this AxisAngle rotation.
        /// </summary>
        /// <returns></returns>
        public RotationVector ToRotationVector()
        {
            return new RotationVector(this.X, this.Y, this.Z, this.Angle, false);  // this vector should already be normalized
        }

        /// <summary>
        /// Returns a Rotation Matrix representation of this Axis Angle. 
        /// Please note that rotation matrices represent rotations in orthonormalized coordinates,  
        /// so that additional Axis Angle information such as overturns (angles outside [0, 180])
        /// will get lost, and rotation axis might be flipped. 
        /// If this Axis Angle represents no effective rotation, the identity matrix will be returned. 
        /// </summary>
        /// <returns></returns>
        public RotationMatrix ToRotationMatrix()
        {
            // Some sanity: if this AA represents no rotation, return identity matrix
            if (this.IsZero())
            {
                return new RotationMatrix();
            }

            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToMatrix/index.htm
            // This conversion assumes the rotation vector is normalized.
            double ang = this.Angle * TO_RADS;
            double c = Math.Cos(ang);
            double s = Math.Sin(ang);
            double t = 1 - c;

            RotationMatrix m = new RotationMatrix();
            m.m00 = c + t * this.X * this.X;
            m.m11 = c + t * this.Y * this.Y;
            m.m22 = c + t * this.Z * this.Z;

            double t1 = t * this.X * this.Y;
            double t2 = s * this.Z;
            m.m10 = t1 + t2;
            m.m01 = t1 - t2;

            t1 = t * this.X * this.Z;
            t2 = s * this.Y;
            m.m20 = t1 - t2;
            m.m02 = t1 + t2;

            t1 = t * this.Y * this.Z;
            t2 = s * this.X;
            m.m21 = t1 + t2;
            m.m12 = t1 - t2;

            return m;
        }

        /// <summary>
        /// Returns a YawPitchRoll representation of this rotation.
        /// </summary>
        /// <returns></returns>
        public YawPitchRoll ToYawPitchRoll()
        {
            // Some sanity: if this AA represents no rotation, return identity matrix
            if (this.IsZero())
            {
                return new YawPitchRoll();
            }

            // This is basically transforming it to a RM and then YPR: this.ToRotationMatrix().ToYawPitchRoll()
            // The above may actually be faster, we will see...

            // This conversion assumes the rotation vector is normalized.
            double ang = this.Angle * TO_RADS;
            double c = Math.Cos(ang);
            double s = Math.Sin(ang);
            double t = 1 - c;
            double trace = t * this.X * this.Z - this.Y * s;

            double xAng, yAng, zAng;

            // North pole singularity (yAng ~ 90degs)? Note m02 is -sin(y) = -sin(90) = -1
            if (trace < -1 + EPSILON3)
            {
                xAng = 0;
                yAng = 0.5 * Math.PI;
                zAng = Math.Atan2(t * this.Y * this.Z - s * this.X, t * this.X * this.Z + s * this.Y);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }

            // South pole singularity (yAng ~ -90degs)? Note m02 is -sin(y) = -sin(-90) = 1
            else if (trace > 1 - EPSILON3)
            {
                xAng = 0;
                yAng = -0.5 * Math.PI;
                zAng = Math.Atan2(-t * this.Y * this.Z + s * this.X, -t * this.X * this.Z - s * this.Y);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }

            // Regular derivation
            else
            {
                //xAng = Math.Atan2(this.m21, this.m22);
                //yAng = -Math.Asin(this.m20);
                //zAng = Math.Atan2(this.m10, this.m00);
                xAng = Math.Atan2(t * this.Y * this.Z + s * this.X, c + t * this.Z * this.Z);
                yAng = -Math.Asin(t * this.X * this.Z - s * this.Y);
                zAng = Math.Atan2(t * this.X * this.Y + s * this.Z, c + t * this.X * this.X);

                //m.m00 = c + t * this.X * this.X;
                //m.m11 = c + t * this.Y * this.Y;
                //m.m22 = c + t * this.Z * this.Z;

                //double t1 = t * this.X * this.Y;
                //double t2 = s * this.Z;
                //m.m10 = t1 + t2;
                //m.m01 = t1 - t2;

                //t1 = t * this.X * this.Z;
                //t2 = s * this.Y;
                //m.m20 = t1 - t2;
                //m.m02 = t1 + t2;

                //t1 = t * this.Y * this.Z;
                //t2 = s * this.X;
                //m.m21 = t1 + t2;
                //m.m12 = t1 - t2;
            }

            return new YawPitchRoll(TO_DEGS * xAng, TO_DEGS * yAng, TO_DEGS * zAng);
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
