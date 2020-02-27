using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
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
        /// W property of the Quaternion (scalar)
        /// </summary>
        public double W { get; internal set; }

        /// <summary>
        /// X property of the Quaternion (i part)
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Y property of the Quaternion (j part)
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Z property of the Quaternion (k part)
        /// </summary>
        public double Z { get; internal set; }

        /// <summary>
        /// Test if this Quaternion approximately equals another. 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(Quaternion other)
        {
            return Math.Abs(this.W - other.W) < EPSILON2
                && Math.Abs(this.X - other.X) < EPSILON2
                && Math.Abs(this.Y - other.Y) < EPSILON2
                && Math.Abs(this.Z - other.Z) < EPSILON2;
        }


        ///// <summary>
        ///// Equality operator.
        ///// </summary>
        ///// <param name="q1"></param>
        ///// <param name="q2"></param>
        ///// <returns></returns>
        //public static bool operator ==(Quaternion q1, Quaternion q2)
        //{
        //    return Math.Abs(q1.W - q2.W) < EPSILON
        //        && Math.Abs(q1.X - q2.X) < EPSILON
        //        && Math.Abs(q1.Y - q2.Y) < EPSILON
        //        && Math.Abs(q1.Z - q2.Z) < EPSILON;
        //}

        ///// <summary>
        ///// Inequality operator.
        ///// </summary>
        ///// <param name="q1"></param>
        ///// <param name="q2"></param>
        ///// <returns></returns>
        //public static bool operator !=(Quaternion q1, Quaternion q2)
        //{
        //    return Math.Abs(q1.W - q2.W) > EPSILON
        //        || Math.Abs(q1.X - q2.X) > EPSILON
        //        || Math.Abs(q1.Y - q2.Y) > EPSILON
        //        || Math.Abs(q1.Z - q2.Z) > EPSILON;
        //}

        /// <summary>
        /// Quaternion addition
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion operator +(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Addition(q1, q2);
        }

        /// <summary>
        /// Quaternion subtraction
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion operator -(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Subtraction(q1, q2);
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
        /// of the first quaternion by the second.
        /// Remember quaternion multiplication is non-commutative.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Multiplication(q1, q2);
        }

        /// <summary>
        /// Returns the Quaternion reulting of scaling Q by a scalar.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Quaternion operator *(Quaternion q, double factor)
        {
            return Quaternion.Scale(q, factor);
        }
        public static Quaternion operator *(double factor, Quaternion q)
        {
            return Quaternion.Scale(q, factor);
        }
        public static Quaternion operator *(Quaternion q, int factor)
        {
            return Quaternion.Scale(q, factor);
        }
        public static Quaternion operator *(int factor, Quaternion q)
        {
            return Quaternion.Scale(q, factor);
        }

        /// <summary>
        /// Division operator.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion operator /(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Division(q1, q2);
        }




        /// <summary>
        /// Create an identity Quaternion representing no rotation.
        /// </summary>
        public Quaternion()
            : this(1, 0, 0, 0, false) { }

        /// <summary>
        /// Create a Quaternion from its components: w + x * i + y * j + z * k
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
        /// Create a Quaternion as a shallow copy of another. 
        /// This Quaternion will be vector-normalized. 
        /// </summary>
        /// <param name="q"></param>
        public Quaternion(Quaternion q)
            : this(q.W, q.X, q.Y, q.Z, true) { }

        internal Quaternion(Quaternion q, bool normalize) : this(q.W, q.X, q.Y, q.Z, false) { }

        /// <summary>
        /// Creates a Quaternion from a Vector and a scalar. 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="scalar"></param>
        public Quaternion(Vector vec, double scalar) : this(vec.X, vec.Y, vec.Z, scalar, true) { }

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
        /// Shallow-copies the values of specified Quaternion.
        /// </summary>
        /// <param name="q"></param>
        public void Set(Quaternion q)
        {
            this.W = q.W;
            this.X = q.X;
            this.Y = q.Y;
            this.Z = q.Z;
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
        internal void Set(double w, double x, double y, double z, bool normalize)
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
        internal void Identity(bool positive)
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
            return Math.Sqrt(this.W * this.W + this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        /// <summary>
        /// Returns the square length of this Quaternion.
        /// </summary>
        /// <returns></returns>
        public double SqLength()
        {
            return this.W * this.W + this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        /// <summary>
        /// Turns this Quaternion into a <a href="https://en.wikipedia.org/wiki/Versor">Versor</a> (unit length quaternion).
        /// If quaternion couldn't be normalized (zero-length), turns it into identity and return false.
        /// </summary>
        public bool Normalize()
        {
            double len = this.Length();

            if (Math.Abs(len) < EPSILON2)
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
        /// If the scalar is outside the [-1, 1] range, the entire quaternion will be normalized instead.
        /// </summary>
        /// <remarks>Homebrew algorithm, TO REVIEW</remarks>
        /// <returns></returns>
        public bool NormalizeVector()
        {
            if (this.W <= -1 || this.W >= 1)
            {
                return this.Normalize();
            }

            // Can't deal with zero-length quaternions or axis vectors
            if (this.Length() < EPSILON2 || Geometry.Length(this.X, this.Y, this.Z) < EPSILON2)
            {
                this.Identity(this.W >= 0);
                return false;
            }

            // The result of squaring is always positive, and leads to vector reversal. 
            // Account for it. 
            bool pos;

            // Avoid divisions by zero
            if (Math.Abs(this.X) > EPSILON2)
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
            else if (Math.Abs(this.Y) > EPSILON2)
            {
                double xy, zy;

                pos = this.Y >= 0;

                xy = this.X / this.Y;
                zy = this.Z / this.Y;

                this.Y = Math.Sqrt((1 - this.W * this.W) / (xy * xy + 1 + zy * zy)) * (pos ? 1 : -1);
                this.X = xy * this.Y;
                this.Z = zy * this.Y;

                return true;
            }
            else if (Math.Abs(this.Z) > EPSILON2)
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
        /// Turns this Rotation into its conjugate.
        /// See http://mathworld.wolfram.com/QuaternionConjugate.html.
        /// </summary>
        public void Conjugate()
        {
            // W stays the same
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        /// <summary>
        /// Returns the conjugate of given quaternion.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion Conjugate(Quaternion q)
        {
            return new Quaternion(q.W, -q.X, -q.Y, -q.Z);
        }

        /// <summary>
        /// Inverts this quaternion.
        /// </summary>
        public void Invert()
        {
            if (this.IsUnit())
            {
                // The inverse of unit vectors is its conjugate.
                this.X = -this.X;
                this.Y = -this.Y;
                this.Z = -this.Z;
            }
            else if (this.IsZero())
            {
                // The inverse of a zero quat is itself!
            }
            else
            {
                // The inverse of a quaternion is its conjugate divided by the squared norm.
                double sqlen = this.SqLength();

                this.W /= sqlen;
                this.X /= -sqlen;
                this.Y /= -sqlen;
                this.Z /= -sqlen;
            }
        }

        /// <summary>
        /// Returns the inverse of given quaternion.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion Inverse(Quaternion q)
        {
            Quaternion qinv = new Quaternion(q);
            qinv.Invert();
            return qinv;
        }

        /// <summary>
        /// Is this a unit length quaternion?
        /// </summary>
        /// <returns></returns>
        public bool IsUnit()
        {
            return Math.Abs(this.SqLength() - 1) < EPSILON2;
        }

        /// <summary>
        /// Is this a zero length quaternion?
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Math.Abs(this.SqLength()) < EPSILON2;
        }

        /// <summary>
        /// Is this an identity Quaternion? 
        /// The identity Quaternion (1, 0, 0, 0) or (-1, 0, 0, 0) produces no rotation.
        /// </summary>
        /// <returns></returns>
        public bool IsIdentity()
        {
            return (Math.Abs(this.W - 1) < EPSILON2 || Math.Abs(this.W + 1) < EPSILON2)  // check for (1,0,0,0) or (-1,0,0,0)
                && Math.Abs(this.X) < EPSILON2
                && Math.Abs(this.Y) < EPSILON2
                && Math.Abs(this.Z) < EPSILON2;
        }

        /// <summary>
        /// Is the rotation represented by this Quaternion equivalent to another? 
        /// Equivalence is defined as the rotation operation resulting in the same
        /// orientation in three-dimensional space. 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEquivalent(Quaternion other)
        {
            // Assuming this Quaternion is normalized, but the other may not:
            Quaternion qn = new Quaternion(other);  // this constructor already normalizes the q

            // A Quaternion multiplied by any real number represents an equivalent rotation.
            // Numerically, and because both quaternions are unit, they are equivalent if 
            // the addition or subtraction of the sum of their components is zero, i.e.:
            //      q =  q;  // or
            //      q = -q;
            double sumQ = this.Sum();
            double sumQn = qn.Sum();
            return (Math.Abs(sumQ + sumQn) < EPSILON2 || Math.Abs(sumQn - sumQ) < EPSILON2);
        }

        /// <summary>
        /// Return the simple sum of the scalar and vector components.
        /// </summary>
        /// <returns></returns>
        internal double Sum()
        {
            return this.W + this.X + this.Y + this.Z;
        }

        /// <summary>
        /// Add a Quaternion to this one. 
        /// </summary>
        /// <param name="q"></param>
        public void Add(Quaternion q)
        {
            // TODO: the resulting Quaternion may not be normalized. Allow this? 
            this.W += q.W;
            this.X += q.X;
            this.Y += q.Y;
            this.Z += q.Z;
        }

        /// <summary>
        /// Returns the addition of two quaternions.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion Addition(Quaternion q1, Quaternion q2)
        {
            Quaternion q = new Quaternion(q1, false);
            q.Add(q2);
            return q;
        }

        /// <summary>
        /// Subtract a quaternion from this one. 
        /// </summary>
        /// <param name="q"></param>
        public void Subtract(Quaternion q)
        {
            // TODO: the resulting Quaternion may not be normalized. Allow this? 
            this.W -= q.W;
            this.X -= q.X;
            this.Y -= q.Y;
            this.Z -= q.Z;
        }

        /// <summary>
        /// Returns the subtraction of two quaternions.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion Subtraction(Quaternion q1, Quaternion q2)
        {
            Quaternion q = new Quaternion(q1, false);
            q.Subtract(q2);
            return q;
        }

        /// <summary>
        /// Post-Multiply this Quaternion by the specified one, a.k.a. this = this * q. 
        /// Conceptually, this means that a Rotation 'q' in Local (intrinsic) coordinates is applied 
        /// to this Rotation.
        /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
        /// </summary>
        /// <param name="q"></param>
        public void Multiply(Quaternion q)
        {
            // TODO: the resulting Quaternion may not be normalized. Allow this? 
            double w = this.W,
                   x = this.X,
                   y = this.Y,
                   z = this.Z;

            this.W = w * q.W - x * q.X - y * q.Y - z * q.Z;
            this.X = x * q.W + w * q.X + y * q.Z - z * q.Y;
            this.Y = y * q.W + w * q.Y + z * q.X - x * q.Z;
            this.Z = z * q.W + w * q.Z + x * q.Y - y * q.X;
        }

        /// <summary>
        /// Premultiplies this Quaternion by the specified one, a.k.a. this = q * this. 
        /// Conceptually, this means that a Rotation 'q' in Global (extrinsic) coordinates is applied 
        /// to this Rotation.
        /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
        /// </summary>
        /// <param name="q"></param>
        public void PreMultiply(Quaternion q)
        {
            // TODO: the resulting Quaternion may not be normalized. Allow this? 
            double w = this.W,
                   x = this.X,
                   y = this.Y,
                   z = this.Z;

            this.W = q.W * w - q.X * x - q.Y * y - q.Z * z;
            this.X = q.X * w + q.W * x + q.Y * z - q.Z * y;
            this.Y = q.Y * w + q.W * y + q.Z * x - q.X * z;
            this.Z = q.Z * w + q.W * z + q.X * y - q.Y * x;
        }

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
        /// of the first quaternion by the second.
        /// Remember quaternion multiplication is non-commutative.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion Multiplication(Quaternion q1, Quaternion q2)
        {
            Quaternion q = new Quaternion(q1, false);
            q.Multiply(q2);
            return q;
        }

        /// <summary>
        /// Multiply this Quaternion by a scalar.
        /// </summary>
        /// <param name="factor"></param>
        public void Scale(double factor)
        {
            // TODO: the resulting Quaternion may not be normalized. Allow this? 
            this.W *= factor;
            this.X *= factor;
            this.Y *= factor;
            this.Z *= factor;
        }

        /// <summary>
        /// Returns a new Quaternion equal to the original multiplied by a factor.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Quaternion Scale(Quaternion q, double factor)
        {
            Quaternion qScaled = new Quaternion(q, false);
            q.Scale(factor);
            return qScaled;
        }

        /// <summary>
        /// Divide this Quaternion by another one. 
        /// In reality, this quaternion is post-multiplied by the inverse of the provided one.
        /// </summary>
        /// <param name="q"></param>
        public void Divide(Quaternion q)
        {
            Quaternion arg = new Quaternion(q, false);
            arg.Invert();
            this.Multiply(arg);
        }

        /// <summary>
        /// Returns the division of q1 by q2.
        /// Under the hood, q1 is post-multiplied by the inverse of q2.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Quaternion Division(Quaternion q1, Quaternion q2)
        {
            Quaternion a = new Quaternion(q1, false),
                       b = new Quaternion(q2, false);
            b.Invert();
            a.Multiply(b);
            return a;
        }

        /// <summary>
        /// Rotate this Quaternion by specified Rotation around GLOBAL (extrinsic) reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public void RotateGlobal(Quaternion rotation)
        {
            this.PreMultiply(rotation);
        }

        /// <summary>
        /// Rotate this Quaternion by specified Rotation around LOCAL (intrinsic) reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public void RotateLocal(Quaternion rotation)
        {
            this.Multiply(rotation);
        }







        /// <summary>
        /// Returns the AxisAngle rotation represented by this Quaternion. 
        /// Note it will always return the unit vector corresponding to a positive rotation, 
        /// even if the quaternion was created from a negative one (flipped vector).
        /// See http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm
        /// </summary>
        /// <returns></returns>
        public AxisAngle ToAxisAngle()
        {
            double theta2 = 2 * Math.Acos(this.W);

            // If angle == 0, no rotation is performed and this Quat is identity
            if (theta2 < EPSILON2)
            {
                return new AxisAngle(0, 0, 0, 0);
            }

            double s = Math.Sin(0.5 * theta2);
            return new AxisAngle(this.X / s, this.Y / s, this.Z / s, theta2 * TO_DEGS, false);
        }

        /// <summary>
        /// Returns the Rotationvector rotation represented by this Quaternion. 
        /// Note it will always return the unit vector corresponding to a positive rotation, 
        /// even if the quaternion was created from a negative one (flipped vector).
        /// </summary>
        /// <returns></returns>
        public RotationVector ToRotationVector(bool radians)
        {
            //double theta2 = 2 * Math.Acos(this.W);

            //// If angle == 0, no rotation is performed and this Quat is identity
            //if (theta2 < EPSILON)
            //{
            //    return new RotationVector(0, 0, 0, 0);
            //}

            //double s = Math.Sin(0.5 * theta2);
            //return new RotationVector(this.X / s, this.Y / s, this.Z / s, theta2 * TO_DEGS, true);

            // Let's not be reduntant...
            return this.ToAxisAngle().ToRotationVector(radians);
        }


        /// <summary>
        /// Returns a 3x3 Rotation Matrix representing this Quaternion's rotation. 
        /// </summary>
        /// <returns></returns>
        public RotationMatrix ToRotationMatrix()
        {
            // Based on http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/
            double xx2 = 2 * this.X * this.X,
                xy2 = 2 * this.X * this.Y,
                xz2 = 2 * this.X * this.Z,
                xw2 = 2 * this.X * this.W,
                yy2 = 2 * this.Y * this.Y,
                yz2 = 2 * this.Y * this.Z,
                yw2 = 2 * this.Y * this.W,
                zz2 = 2 * this.Z * this.Z,
                zw2 = 2 * this.Z * this.W,
                ww2 = 2 * this.W * this.W;

            return new RotationMatrix(1 - yy2 - zz2, xy2 - zw2, xz2 + yw2,
                                          xy2 + zw2, 1 - xx2 - zz2, yz2 - xw2,
                                          xz2 - yw2, yz2 + xw2, 1 - xx2 - yy2, false);
        }

        /// <summary>
        /// Returns the Euler Angle representation of this rotation in Tait-Bryan representation
        /// (intrinsic ZY'X'' rotations, or Yaw-Pitch-Roll).
        /// </summary>
        /// <returns></returns>
        public YawPitchRoll ToYawPitchRoll()
        {
            /**
             * Adapted from http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/ with:
             *      - Machina conventions: ES uses different heading/pitch/bank axes, fixed here
             *      - More singularity precision: using 0.001 triggers on angles above 86.3 degrees, 
             *        resulting in imprecise sets of conversions in those areas, like:
             *              Quaternion[0.027125, 0.691126, -0.058642, 0.71984]
                            EulerZYX[Z:175.504902, Y:-90, X:0]
                            Quaternion[0.027731, 0.706563, -0.027731, 0.706563]
                            EulerZYX[Z:175.504902, Y:-90, X:0]
                      instead of the following for 0.000001:
                            [Quaternion[0.027125, 0.691126, -0.058642, 0.71984]]
                            [EulerZYX[Z:-135.814015, Y:-86.544758, X:-51.142989]]
                            [Quaternion[-0.027125, -0.691126, 0.058642, -0.71984]]
             *      - Consistency adjustment on singularities: as a result of the 2 * atan2 operation, singularities
             *        may yield Z rotations between [-TAU, TAU], instead of the regular [-PI, PI]. This is not a big deal, 
             *        but for the sake of consistency in the Euler Angle representation, this has been readapted. 
             **/

            double xAng, yAng, zAng;

            // First, test if close to pitch 90deg singularity:
            double test = this.W * this.Y - this.Z * this.X;
            if (test > 0.5 - EPSILON3)  // singularity at north pole
            {
                xAng = 0;
                yAng = 0.5 * Math.PI;
                zAng = 2 * Math.Atan2(this.X, -this.W);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }
            else if (test < -0.5 + EPSILON3)  // singularity at south pole
            {
                xAng = 0;
                yAng = -0.5 * Math.PI;
                zAng = -2 * Math.Atan2(this.X, -this.W);
                if (zAng < -Math.PI) zAng += TAU;  // remap to [-180, 180]
                else if (zAng > Math.PI) zAng -= TAU;
            }
            else
            {
                // From https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#quaternion_to_euler_angles_conversion
                // Roll
                xAng = Math.Atan2(2 * (this.W * this.X + this.Y * this.Z),
                                         1 - 2 * (this.X * this.X + this.Y * this.Y));

                // Pitch
                yAng = Math.Asin(2 * test);

                // Yaw
                zAng = Math.Atan2(2 * (this.W * this.Z + this.X * this.Y),
                                         1 - 2 * (this.Y * this.Y + this.Z * this.Z));
            }

            return new YawPitchRoll(TO_DEGS * xAng, TO_DEGS * yAng, TO_DEGS * zAng);
        }


        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool labels)
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "{0}[{1}{2}, {3}{4}, {5}{6}, {7}{8}]",
                labels ? "Quaternion" : "",
                labels ? "W:" : "",
                Math.Round(W, STRING_ROUND_DECIMALS_QUAT),
                labels ? "X:" : "",
                Math.Round(X, STRING_ROUND_DECIMALS_QUAT),
                labels ? "Y:" : "",
                Math.Round(Y, STRING_ROUND_DECIMALS_QUAT),
                labels ? "Z:" : "",
                Math.Round(Z, STRING_ROUND_DECIMALS_QUAT));
        }

        public string ToArrayString()
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "[{0},{1},{2},{3}]",
                Math.Round(W, STRING_ROUND_DECIMALS_QUAT),
                Math.Round(X, STRING_ROUND_DECIMALS_QUAT),
                Math.Round(Y, STRING_ROUND_DECIMALS_QUAT),
                Math.Round(Z, STRING_ROUND_DECIMALS_QUAT));
        }

    }
}
