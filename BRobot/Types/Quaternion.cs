using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{
    //   ██████╗ ██╗   ██╗ █████╗ ████████╗███████╗██████╗ ███╗   ██╗██╗ ██████╗ ███╗   ██╗
    //  ██╔═══██╗██║   ██║██╔══██╗╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██║██╔═══██╗████╗  ██║
    //  ██║   ██║██║   ██║███████║   ██║   █████╗  ██████╔╝██╔██╗ ██║██║██║   ██║██╔██╗ ██║
    //  ██║▄▄ ██║██║   ██║██╔══██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██║██║   ██║██║╚██╗██║
    //  ╚██████╔╝╚██████╔╝██║  ██║   ██║   ███████╗██║  ██║██║ ╚████║██║╚██████╔╝██║ ╚████║
    //   ╚══▀▀═╝  ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                     
    
    /// <summary>
    /// A class to represent a spatial rotation as a Quaternion.
    /// </summary>
    public class Quaternion : Geometry
    {
        /// <summary>
        /// Quaternion properties.
        /// </summary>
        public double W, X, Y, Z;

        /// <summary>
        /// Quaternion addition
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation operator +(Rotation r1, Rotation r2)
        {
            return Quaternion.Addition(r1, r2);
        }

        /// <summary>
        /// Quaternion subtraction
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation operator -(Rotation r1, Rotation r2)
        {
            return Quaternion.Subtraction(r1, r2);
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static bool operator ==(Quaternion q1, Quaternion q2)
        {
            return Math.Abs(q1.W - q2.W) < EPSILON
                && Math.Abs(q1.X - q2.X) < EPSILON
                && Math.Abs(q1.Y - q2.Y) < EPSILON
                && Math.Abs(q1.Z - q2.Z) < EPSILON;
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
        /// of the first quaternion by the second.
        /// Remember quaternion multiplication is non-commutative.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation operator *(Rotation r1, Rotation r2)
        {
            return Quaternion.Multiply(r1, r2);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static bool operator !=(Quaternion q1, Quaternion q2)
        {
            return Math.Abs(q1.W - q2.W) > EPSILON
                || Math.Abs(q1.X - q2.X) > EPSILON
                || Math.Abs(q1.Y - q2.Y) > EPSILON
                || Math.Abs(q1.Z - q2.Z) > EPSILON;
        }

        /// <summary>
        /// Create a Quaternion from its components. 
        /// For quaternions to be used as valid representations of spatial rotations, 
        /// they need to be versors (unit quaternions). This constructor will automatically
        /// Vector-Normalize the resulting Quaternion.
        /// While this may restrict more general complex algebra, it will be useful
        /// in the context of robotics to keep quaternions tight this way ;)
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Quaternion(double w, double x, double y, double z)
            : this(w, x, y, z, true) { }

        /// <summary>
        /// A private constructor with the option to bypass automatic quaternion normalization.
        /// This saves computation when using conversion algorithms that already yield normalized results.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="normalize"></param>
        internal Quaternion(double w, double x, double y, double z, bool normalize)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;

            if (normalize)
            {
                this.NormalizeVector();
            }
        }

        /// <summary>
        /// Sets the values of this Quaternion's components. 
        /// The result is Vector-Normalized.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double w, double x, double y, double z)
        {
            this.Set(w, x, y, z, true);
        }

        /// <summary>
        /// Sets the values of this Quaternion's components with the option to bypass normalization.
        /// For internal use, when wxyz come from a normalized source.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="normalize"></param>
        protected void Set(double w, double x, double y, double z, bool normalize)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;

            if (normalize)
            {
                this.NormalizeVector();
            }
        }

        /// <summary>
        /// Turns into a positive identity Quaternion (1, 0, 0, 0).
        /// </summary>
        public void Identity()
        {
            this.Identity(true);
        }

        /// <summary>
        /// Turns into an identity Quaternion.
        /// </summary>
        protected void Identity(bool positive)
        {
            this.W = positive ? 1 : -1;
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        /// <summary>
        /// Returns the length (norm) of this Quaternion.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Returns the square length of this Quaternion.
        /// </summary>
        /// <returns></returns>
        public double SqLength()
        {
            return W * W + X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Turns this Quaternion into a <a href="https://en.wikipedia.org/wiki/Versor">Versor</a> (unit length quaternion).
        /// If quaternion couldn't be normalized (zero-length), turns it into identity and return false.
        /// </summary>
        public bool Normalize()
        {
            double len = this.Length();

            if (Math.Abs(len) < EPSILON)
            {
                this.Identity();
                return false;  // check for zero quaternion
            }

            this.W /= len;
            this.X /= len;
            this.Y /= len;
            this.Z /= len;

            return true;
        }

        /// <summary>
        /// Normalizes the complex portion of this Quaternion (the rotation vector)
        /// maintaining the scalar portion (the rotation angle) the same. This is useful when
        /// coming from a rotation specified by non-unit vectors, to maintain angular spin.
        /// If the scalar is outside the [-1, 0] range, the entire quaternion will be normalized.
        /// </summary>
        /// <remarks>Homebrew algorithm, TO REVIEW</remarks>
        /// <returns></returns>
        public bool NormalizeVector()
        {
            if (this.W <= -1 || this.W >= 1)
            {
                return this.Normalize();
            }

            // Can't deal with a zero-length quaternions or axis vectors
            if (this.Length() < EPSILON || Geometry.Length(this.X, this.Y, this.Z) < EPSILON)
            {
                this.Identity(this.W >= 0);
                return false;
            }

            // The result of squaring is always positive, and leads to vector reversal. 
            // Account for it. 
            bool pos;

            // Avoid divisions by zero
            if (Math.Abs(this.X) > EPSILON)
            {
                double yx, zx;

                pos = this.X >= 0;

                yx = this.Y / this.X;
                zx = this.Z / this.X;

                this.X = Math.Sqrt((1 - this.W * this.W) / (1 + yx * yx + zx * zx)) * (pos ? 1 : -1);
                this.Y = yx * this.X;
                this.Z = zx * this.X;

                return true;
            }
            else if (Math.Abs(this.Y) > EPSILON)
            {
                double xy, zy;

                pos = this.Y >= 0;

                xy = this.X / this.Y;
                zy = this.Y / this.Y;

                this.Y = Math.Sqrt((1 - this.W * this.W) / (xy * xy + 1 + zy * zy)) * (pos ? 1 : -1);
                this.X = xy * this.Y;
                this.Z = zy * this.Y;

                return true;
            }
            else if (Math.Abs(this.Z) > EPSILON)
            {
                double xz, yz;

                pos = this.Z >= 0;

                xz = this.X / this.Z;
                yz = this.Y / this.Z;

                this.Z = Math.Sqrt((1 - this.W * this.W) / (xz * xz + yz * yz + 1)) * (pos ? 1 : -1);
                this.X = xz * this.Z;
                this.Y = yz * this.Z;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Is this a unit length quaternion?
        /// </summary>
        /// <returns></returns>
        public bool IsUnit()
        {
            //double zero = Math.Abs(this.SqLength() - 1);
            //return zero < EPSILON;
            return Math.Abs(this.SqLength() - 1) < EPSILON;
        }

        /// <summary>
        /// Is this a zero length quaternion?
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("IsZero is deprecated, should always return false since the class will not allow zero-length quaternions to exist and always fallbak into an identity quaternion")]
        public bool IsZero()
        {
            double sqlen = this.SqLength();
            return Math.Abs(sqlen) < EPSILON;
        }

        /// <summary>
        /// Is this an identity Quaternion? 
        /// The identity Quaternion (1, 0, 0, 0) or (-1, 0, 0, 0) produces no rotation.
        /// </summary>
        /// <returns></returns>
        public bool IsIdentity()
        {
            return (Math.Abs(this.W - 1) < EPSILON || Math.Abs(this.W + 1) < EPSILON)  // check for (1,0,0,0) or (-1,0,0,0)
                && Math.Abs(this.X) < EPSILON
                && Math.Abs(this.Y) < EPSILON
                && Math.Abs(this.Z) < EPSILON;
        }

        /// <summary>
        /// Returns the AxisAngle rotation represented by this Quaternion. 
        /// Note it will always return the unit vector corresponding to a positive rotation, 
        /// even if the quaternion was created from a negative one (flipped vector).
        /// </summary>
        /// <seealso cref="http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm"/>
        /// <returns></returns>
        public AxisAngle ToAxisAngle()
        {
            double theta2 = 2 * Math.Acos(this.W);

            // If angle == 0, no rotation is performed and this Quat is identity
            if (theta2 < EPSILON)
            {
                return new AxisAngle(0, 0, 0, 0);
            }

            double s = Math.Sin(0.5 * theta2);
            return new AxisAngle(this.X / s, this.Y / s, this.Z / s, theta2 * TO_DEGS);
        }

        public override string ToString()
        {
            return string.Format("Quaternion[{0}, {1}, {2}, {3}]",
                Math.Round(W, STRING_ROUND_DECIMALS_RADS),
                Math.Round(X, STRING_ROUND_DECIMALS_RADS),
                Math.Round(Y, STRING_ROUND_DECIMALS_RADS),
                Math.Round(Z, STRING_ROUND_DECIMALS_RADS));
        }


    }
}
