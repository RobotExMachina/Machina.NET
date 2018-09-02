using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
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
        /// The rotation axis.
        /// </summary>
        public Vector Axis { get; internal set; }

        /// <summary>
        /// Rotation angle in degrees.
        /// </summary>
        public double Angle { get; internal set; }

        /// <summary>
        /// X coordinate of the rotation vector.
        /// </summary>
        public double X { get { return this.Axis.X; } internal set { this.Axis.X = value; } }

        /// <summary>
        /// Y coordinate of the rotation vector.
        /// </summary>
        public double Y { get { return this.Axis.Y; } internal set { this.Axis.Y = value; } }

        /// <summary>
        /// Z coordinate of the rotation vector.
        /// </summary>
        public double Z { get { return this.Axis.Z; } internal set { this.Axis.Z = value; } }

        /// <summary>
        /// Test if this AxisAngle is approximately equal to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(AxisAngle other)
        {
            return Math.Abs(this.Axis.X - other.Axis.X) < EPSILON2
                && Math.Abs(this.Axis.Y - other.Axis.Y) < EPSILON2
                && Math.Abs(this.Axis.Z - other.Axis.Z) < EPSILON2
                && Math.Abs(this.Angle - other.Angle) < EPSILON2;
        }

        ///// <summary>
        ///// Equality operator.
        ///// </summary>
        ///// <param name="aa1"></param>
        ///// <param name="aa2"></param>
        ///// <returns></returns>
        //public static bool operator ==(AxisAngle aa1, AxisAngle aa2)
        //{
        //    return Math.Abs(aa1.Axis.X - aa2.Axis.X) < EPSILON
        //        && Math.Abs(aa1.Axis.Y - aa2.Axis.Y) < EPSILON
        //        && Math.Abs(aa1.Axis.Z - aa2.Axis.Z) < EPSILON
        //        && Math.Abs(aa1.Angle - aa2.Angle) < EPSILON;
        //}

        ///// <summary>
        ///// Inequality operator.
        ///// </summary>
        ///// <param name="aa1"></param>
        ///// <param name="aa2"></param>
        ///// <returns></returns>
        //public static bool operator !=(AxisAngle aa1, AxisAngle aa2)
        //{
        //    return Math.Abs(aa1.Axis.X - aa2.Axis.X) > EPSILON
        //        || Math.Abs(aa1.Axis.Y - aa2.Axis.Y) > EPSILON
        //        || Math.Abs(aa1.Axis.Z - aa2.Axis.Z) > EPSILON
        //        || Math.Abs(aa1.Angle - aa2.Angle) > EPSILON;
        //}

        /// <summary>
        /// Implicit conversion to Vector object.
        /// </summary>
        /// <param name="aa"></param>
        public static implicit operator Vector(AxisAngle aa)
        {
            return new Vector(aa.Axis.X, aa.Axis.Y, aa.Axis.Z);
        }




        /// <summary>
        /// Creates an AxisAngle representing no rotation.
        /// </summary>
        public AxisAngle() : this(0, 0, 0, 0, false) { }

        /// <summary>
        /// Creates an AxisAngle as a shallow copy of another one.
        /// The axis vector will be automatically normalized.
        /// </summary>
        /// <param name="axisAngle"></param>
        public AxisAngle(AxisAngle axisAngle) : this(axisAngle.X, axisAngle.Y, axisAngle.Z, axisAngle.Angle, true) { }

        /// <summary>
        /// Creates an AxisAngle as a shallow copy of another one.
        /// Internal constructor to bypass normalization.
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="normalize"></param>
        internal AxisAngle(AxisAngle aa, bool normalize) : this(aa.X, aa.Y, aa.Z, aa.Angle, normalize) { }

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
        public AxisAngle(Vector axis, double angleDegs)
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
            this.Axis = new Vector(x, y, z);
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
            double len = Axis.Length();

            if (len < EPSILON2)
            {
                this.Axis.X = 0;
                this.Axis.Y = 0;
                this.Axis.Z = 0;

                return false;
            }

            this.Axis.X /= len;
            this.Axis.Y /= len;
            this.Axis.Z /= len;

            return true;
        }

        /// <summary>
        /// Returns true if this AxisAngle represents no rotation: zero rotation axis or zero (modulated) angle.
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            //return Math.Abs(this.Angle % 360) < EPSILON
            //    || (Math.Abs(this.X) < EPSILON
            //        && Math.Abs(this.Y) < EPSILON
            //        && Math.Abs(this.Z) < EPSILON);

            return Math.Abs(this.Angle % 360) < EPSILON2
                || this.Axis.IsZero();
        }

        /// <summary>
        /// Returns the length of the rotation axis.
        /// </summary>
        /// <returns></returns>
        public double AxisLength()
        {
            return this.Axis.Length();
        }

        /// <summary>
        /// Transforms this AxisAngle into a rotation around the same axis, but with an equivalent rotation angle
        /// in the [0, 360]. Note the axis vector will be flipped if the original angle was negative. 
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
            this.Axis.Invert();
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
                return Math.Abs(other.Angle % 360) < EPSILON2;
            } 
            else if (other.IsZero())
            {
                return Math.Abs(this.Angle % 360) < EPSILON2;
            }

            //Vector v1 = new Vector(this.X, this.Y, this.Z),
            //v2 = new Vector(axisAngle.X, axisAngle.Y, axisAngle.Z);
            //int directions = Vector.CompareDirections(v1, v2);
            int directions = Vector.CompareDirections(this, other);
            
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
            if (Math.Abs(a1 - 360) < EPSILON2) a1 = 0;

            double a2 = other.Angle;
            while (a2 < 0)
            {
                a2 += 360;
            }
            a2 %= 360;
            if (Math.Abs(a2 - 360) < EPSILON2) a2 = 0;

            // If the vectors have the same direction, angles should be module of each other.
            if (directions == 1)
            {
                return Math.Abs(a1 - a2) < EPSILON2;
            }

            // If opposite directions, they should add up to 360 degs.
            if (directions == 3)
            {
                return Math.Abs(a1 + a2 - 360) < EPSILON2;
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
                s * this.Axis.X,
                s * this.Axis.Y,
                s * this.Axis.Z, false);
        }

        /// <summary>
        /// Returns a RotationVector representing this AxisAngle rotation.
        /// </summary>
        /// <returns></returns>
        public RotationVector ToRotationVector()
        {
            return this.ToRotationVector(false);
        }

        public RotationVector ToRotationVector(bool radians)
        {
            return new RotationVector(this.Axis.X, 
                this.Axis.Y, 
                this.Axis.Z, 
                radians ? TO_RADS * this.Angle : this.Angle, 
                false);  // this vector should already be normalized
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
            m.m00 = c + t * this.Axis.X * this.Axis.X;
            m.m11 = c + t * this.Axis.Y * this.Axis.Y;
            m.m22 = c + t * this.Axis.Z * this.Axis.Z;

            double t1 = t * this.Axis.X * this.Axis.Y;
            double t2 = s * this.Axis.Z;
            m.m10 = t1 + t2;
            m.m01 = t1 - t2;

            t1 = t * this.Axis.X * this.Axis.Z;
            t2 = s * this.Axis.Y;
            m.m20 = t1 - t2;
            m.m02 = t1 + t2;

            t1 = t * this.Axis.Y * this.Axis.Z;
            t2 = s * this.Axis.X;
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

            // THIS IS BASICALLY TRANSFORMING IT TO A RM AND THEN YPR: this.ToRotationMatrix().ToYawPitchRoll()
            // The above may actually be faster, we will see...

            // This conversion assumes the rotation vector is normalized.
            double ang = this.Angle * TO_RADS;
            double c = Math.Cos(ang);
            double s = Math.Sin(ang);
            double t = 1 - c;
            double trace = t * this.Axis.X * this.Axis.Z - this.Axis.Y * s;

            double xAng, yAng, zAng;

            // North pole singularity (yAng ~ 90degs)? Note m02 is -sin(y) = -sin(90) = -1
            if (trace < -1 + EPSILON3)
            {
                xAng = 0;
                yAng = 0.5 * Math.PI;
                zAng = Math.Atan2(t * this.Axis.Y * this.Axis.Z - s * this.Axis.X, t * this.Axis.X * this.Axis.Z + s * this.Axis.Y);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }

            // South pole singularity (yAng ~ -90degs)? Note m02 is -sin(y) = -sin(-90) = 1
            else if (trace > 1 - EPSILON3)
            {
                xAng = 0;
                yAng = -0.5 * Math.PI;
                zAng = Math.Atan2(-t * this.Axis.Y * this.Axis.Z + s * this.Axis.X, -t * this.Axis.X * this.Axis.Z - s * this.Axis.Y);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }

            // Regular derivation
            else
            {
                xAng = Math.Atan2(t * this.Axis.Y * this.Axis.Z + s * this.Axis.X, c + t * this.Axis.Z * this.Axis.Z);
                yAng = -Math.Asin(t * this.Axis.X * this.Axis.Z - s * this.Axis.Y);
                zAng = Math.Atan2(t * this.Axis.X * this.Axis.Y + s * this.Axis.Z, c + t * this.Axis.X * this.Axis.X);
            }

            return new YawPitchRoll(TO_DEGS * xAng, TO_DEGS * yAng, TO_DEGS * zAng);
        }

        public override string ToString()
        {
            return string.Format("AxisAngle[X:{0}, Y:{1}, Z:{2}, A:{3}]",
                Math.Round(Axis.X, STRING_ROUND_DECIMALS_MM),
                Math.Round(Axis.Y, STRING_ROUND_DECIMALS_MM),
                Math.Round(Axis.Z, STRING_ROUND_DECIMALS_MM),
                Math.Round(Angle, STRING_ROUND_DECIMALS_DEGS));
        }
    }
}
