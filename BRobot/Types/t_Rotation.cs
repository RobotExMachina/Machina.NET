using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{

    // REVIEW THIS CLASS AND INCORPORATE IT WITH ALL THE OTHER ROTATIONS...



    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   


    ///// <summary>
    ///// Represents a rotation using quaternions.
    ///// </summary>
    //public class Rotation : Geometry
    //{
    //    /// <summary>
    //    /// The orientation of a global XYZ coordinate system (aka an identity rotation).
    //    /// </summary>
    //    public static readonly Rotation GlobalXY = new Rotation(1, 0, 0, 0, true);

    //    /// <summary>
    //    /// A global XYZ coordinate system rotated 180 degs around its X axis.
    //    /// </summary>
    //    public static readonly Rotation FlippedAroundX = new Rotation(0, 1, 0, 0, true);

    //    /// <summary>
    //    /// A global XYZ coordinate system rotated 180 degs around its Y axis. 
    //    /// Recommended as the easiest orientation for the standard robot end effector to reach in positive XY octants.
    //    /// </summary>
    //    public static readonly Rotation FlippedAroundY = new Rotation(0, 0, 1, 0, true);

    //    /// <summary>
    //    /// A global XYZ coordinate system rotated 180 degs around its Z axis.
    //    /// </summary>
    //    public static readonly Rotation FlippedAroundZ = new Rotation(0, 0, 0, 1, true);

    //    public double W, X, Y, Z;


    //    public static Rotation operator +(Rotation r1, Rotation r2)
    //    {
    //        return Rotation.Addition(r1, r2);
    //    }

    //    public static Rotation operator -(Rotation r1, Rotation r2)
    //    {
    //        return Rotation.Subtraction(r1, r2);
    //    }

    //    /// <summary>
    //    /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
    //    /// of the first quaternion by the second.
    //    /// Remember quaternion multiplication is non-commutative.
    //    /// </summary>
    //    /// <param name="r1"></param>
    //    /// <param name="r2"></param>
    //    /// <returns></returns>
    //    public static Rotation operator *(Rotation r1, Rotation r2)
    //    {
    //        return Rotation.Multiply(r1, r2);
    //    }


    //    /// <summary>
    //    /// Create a Rotation object from its Quaternion parameters: 
    //    /// w + x * i + y * j + z * k. 
    //    /// NOTE: it is very unlikely that any public user will input 
    //    /// direct quaternion values when specifying rotations, and this
    //    /// signature could be better used for vector + angle value.
    //    /// Changed this to internal, and adding a public vectorXYZ + ang signature.
    //    /// A public static Rotation.FromQuaterion() is added for 
    //    /// advanced users.
    //    /// </summary>
    //    /// <param name="w"></param>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <param name="z"></param>
    //    internal Rotation(double w, double x, double y, double z, bool quaternionRepresentation)
    //    {
    //        if (quaternionRepresentation)
    //        {
    //            this.W = w;
    //            this.X = x;
    //            this.Y = y;
    //            this.Z = z;
    //        }
    //        else
    //        {
    //            // TODO: this is awful, must be a better way of choosing between two constructors...
    //            Rotation r = new Rotation(w, x, y, z);  // use wxyz as vector + ang representation
    //            this.W = r.W;
    //            this.X = r.X;
    //            this.Y = r.Y;
    //            this.Z = r.Z;
    //        }

    //    }

    //    /// <summary>
    //    /// Creates a unit Quaternion representing a rotation of n degrees around a
    //    /// vector, with right-hand positive convention.
    //    /// </summary>
    //    /// <param name="vecX"></param>
    //    /// <param name="vecY"></param>
    //    /// <param name="vecZ"></param>
    //    /// <param name="angDegs"></param>
    //    public Rotation(double vecX, double vecY, double vecZ, double angDegs)
    //    {
    //        double halfAngRad = 0.5 * TO_RADS * angDegs;   // ang / 2
    //        double s = Math.Sin(halfAngRad);
    //        Vector u = new Vector(vecX, vecY, vecZ);
    //        u.Normalize();  // rotation quaternion must be unit

    //        this.W = Math.Cos(halfAngRad);
    //        this.X = s * u.X;
    //        this.Y = s * u.Y;
    //        this.Z = s * u.Z;
    //    }

    //    /// <summary>
    //    /// Creates a unit Quaternion representing a rotation of n degrees around a 
    //    /// vector, with right-hand positive convention.
    //    /// </summary>
    //    /// <param name="vec"></param>
    //    /// <param name="angDegs"></param>
    //    public Rotation(Vector vec, double angDegs) : this(vec.X, vec.Y, vec.Z, angDegs) { }


    //    /// <summary>
    //    /// A static constructor for Rotation objects from their Quaternion representation.
    //    /// </summary>
    //    /// <param name="w"></param>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <param name="z"></param>
    //    /// <returns></returns>
    //    public static Rotation FromQuaternion(double w, double x, double y, double z)
    //    {
    //        return new Rotation(w, x, y, z, true);
    //    }

    //    /// <summary>
    //    /// Create a Rotation object from a CoordinateSystem defined by
    //    /// the coordinates of its main X vector and the coordiantes of 
    //    /// a guiding Y vector.
    //    /// Vectors don't need to be normalized or orthogonal, the constructor 
    //    /// will generate the best-fitting CoordinateSystem with this information. 
    //    /// </summary>
    //    /// <param name="x0"></param>
    //    /// <param name="x1"></param>
    //    /// <param name="x2"></param>
    //    /// <param name="y0"></param>
    //    /// <param name="y1"></param>
    //    /// <param name="y2"></param>
    //    public Rotation(double x0, double x1, double x2, double y0, double y1, double y2) :
    //        this(new CoordinateSystem(x0, x1, x2, y0, y1, y2).GetQuaternion())
    //    { }

    //    /// <summary>
    //    /// Create a Rotation object from a CoordinateSystem defined by
    //    /// the main Vector X and the guiding Vector Y.
    //    /// Vectors don't need to be normalized or orthogonal, the constructor 
    //    /// will generate the best-fitting CoordinateSystem with this information.
    //    /// </summary>
    //    /// <param name="vecX"></param>
    //    /// <param name="vecY"></param>
    //    public Rotation(Vector vecX, Vector vecY) :
    //        this(new CoordinateSystem(vecX, vecY).GetQuaternion())
    //    { }

    //    /// <summary>
    //    /// Create a Rotation object from a CoordinateSystem defined by
    //    /// the coordinates of its main X vector and the coordiantes of 
    //    /// a guiding Y vector.
    //    /// Vectors don't need to be normalized or orthogonal, the constructor 
    //    /// will generate the best-fitting CoordinateSystem with this information.
    //    /// </summary>
    //    /// <param name="cs"></param>
    //    public Rotation(CoordinateSystem cs) :
    //        this(cs.GetQuaternion())
    //    { }

    //    /// <summary>
    //    /// Creates a new Rotation as a shallow copy of the passed one.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public Rotation(Rotation r)
    //    {
    //        this.W = r.W;
    //        this.X = r.X;
    //        this.Y = r.Y;
    //        this.Z = r.Z;
    //    }

    //    /// <summary>
    //    /// Creates an identity Quaternion (no rotation).
    //    /// </summary>
    //    public Rotation()
    //    {
    //        this.W = 1;
    //        this.X = 0;
    //        this.Y = 0;
    //        this.Z = 0;
    //    }

    //    /// <summary>
    //    /// Sets the values of this Quaternion's components.
    //    /// </summary>
    //    /// <param name="w"></param>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <param name="z"></param>
    //    public void Set(double w, double x, double y, double z)
    //    {
    //        this.W = w;
    //        this.X = x;
    //        this.Y = y;
    //        this.Z = z;
    //    }

    //    /// <summary>
    //    /// Shallow-copies the values of specified Quaternion.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void Set(Rotation r)
    //    {
    //        this.W = r.W;
    //        this.X = r.X;
    //        this.Y = r.Y;
    //        this.Z = r.Z;
    //    }



    //    /// <summary>
    //    /// Returns the length (norm) of this Quaternion.
    //    /// </summary>
    //    /// <returns></returns>
    //    public double Length()
    //    {
    //        return Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
    //    }

    //    /// <summary>
    //    /// Returns the square length of this Quaternion.
    //    /// </summary>
    //    /// <returns></returns>
    //    public double SqLength()
    //    {
    //        return W * W + X * X + Y * Y + Z * Z;
    //    }

    //    /// <summary>
    //    /// Turns this Quaternion into a <a href="https://en.wikipedia.org/wiki/Versor">Versor</a> (unit length quaternion).
    //    /// </summary>
    //    public void Normalize()
    //    {
    //        double len = this.Length();
    //        this.W /= len;
    //        this.X /= len;
    //        this.Y /= len;
    //        this.Z /= len;
    //    }

    //    /// <summary>
    //    /// Is this a unit length quaternion?
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool IsUnit()
    //    {
    //        double zero = Math.Abs(this.SqLength() - 1);
    //        return zero < EPSILON;
    //    }

    //    /// <summary>
    //    /// Is this a zero length quaternion?
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool IsZero()
    //    {
    //        double sqlen = this.SqLength();
    //        return Math.Abs(sqlen) > EPSILON;
    //    }

    //    /// <summary>
    //    /// Add a Quaternion to this one. 
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void Add(Rotation r)
    //    {
    //        this.W += r.W;
    //        this.X += r.X;
    //        this.Y += r.Y;
    //        this.Z += r.Z;
    //    }

    //    /// <summary>
    //    /// Returns the addition of two quaternions.
    //    /// </summary>
    //    /// <param name="r1"></param>
    //    /// <param name="r2"></param>
    //    /// <returns></returns>
    //    public static Rotation Addition(Rotation r1, Rotation r2)
    //    {
    //        return new Rotation(
    //            r1.W + r2.W,
    //            r1.X + r2.X,
    //            r1.Y + r2.Y,
    //            r1.Z + r2.Z, true);
    //    }

    //    /// <summary>
    //    /// Subtract a quaternion from this one. 
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void Subtract(Rotation r)
    //    {
    //        this.W -= r.W;
    //        this.X -= r.X;
    //        this.Y -= r.Y;
    //        this.Z -= r.Z;
    //    }

    //    /// <summary>
    //    /// Returns the subtraction of two quaternions.
    //    /// </summary>
    //    /// <param name="r1"></param>
    //    /// <param name="r2"></param>
    //    /// <returns></returns>
    //    public static Rotation Subtraction(Rotation r1, Rotation r2)
    //    {
    //        return new Rotation(
    //            r1.W - r2.W,
    //            r1.X - r2.X,
    //            r1.Y - r2.Y,
    //            r1.Z - r2.Z, true);
    //    }

    //    /// <summary>
    //    /// Multiply this Quaternion by the specified one, a.k.a. this = this * r. 
    //    /// Conceptually, this means that a Rotation 'r' in Local coordinates is applied 
    //    /// to this Rotation.
    //    /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void Multiply(Rotation r)
    //    {
    //        double w = this.W,
    //               x = this.X,
    //               y = this.Y,
    //               z = this.Z;

    //        this.W = w * r.W - x * r.X - y * r.Y - z * r.Z;
    //        this.X = x * r.W + w * r.X + y * r.Z - z * r.Y;
    //        this.Y = y * r.W + w * r.Y + z * r.X - x * r.Z;
    //        this.Z = z * r.W + w * r.Z + x * r.Y - y * r.X;
    //    }

    //    /// <summary>
    //    /// Premultiplies this Quaternion by the specified one, a.k.a. this = r * this. 
    //    /// Conceptually, this means that a Rotation 'r' in Global coordinates is applied 
    //    /// to this Rotation.
    //    /// See https://en.wikipedia.org/wiki/Quaternion#Hamilton_product
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void PreMultiply(Rotation r)
    //    {
    //        double w = this.W,
    //               x = this.X,
    //               y = this.Y,
    //               z = this.Z;

    //        this.W = r.W * w - r.X * x - r.Y * y - r.Z * z;
    //        this.X = r.X * w + r.W * x + r.Y * z - r.Z * y;
    //        this.Y = r.Y * w + r.W * y + r.Z * x - r.X * z;
    //        this.Z = r.Z * w + r.W * z + r.X * y - r.Y * x;
    //    }

    //    /// <summary>
    //    /// Returns the <a href="https://en.wikipedia.org/wiki/Quaternion#Hamilton_product">Hamilton product</a> 
    //    /// of the first quaternion by the second.
    //    /// Remember quaternion multiplication is non-commutative.
    //    /// </summary>
    //    /// <param name="r1"></param>
    //    /// <param name="r2"></param>
    //    /// <returns></returns>
    //    public static Rotation Multiply(Rotation r1, Rotation r2)
    //    {
    //        double w = r1.W * r2.W - r1.X * r2.X - r1.Y * r2.Y - r1.Z * r2.Z;
    //        double x = r1.X * r2.W + r1.W * r2.X + r1.Y * r2.Z - r1.Z * r2.Y;
    //        double y = r1.Y * r2.W + r1.W * r2.Y + r1.Z * r2.X - r1.X * r2.Z;
    //        double z = r1.Z * r2.W + r1.W * r2.Z + r1.X * r2.Y - r1.Y * r2.X;

    //        return new Rotation(w, x, y, z, true);
    //    }

    //    /// <summary>
    //    /// Divide this Quaternion by another one. 
    //    /// In reality, this quaternion is post-multiplied by the inverse of the provided one.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    public void Divide(Rotation r)
    //    {
    //        Rotation arg = new Rotation(r);
    //        arg.Invert();
    //        this.Multiply(arg);
    //    }

    //    /// <summary>
    //    /// Returns the division of r1 by r2.
    //    /// Under the hood, r1 is post-multiplied by the inverse of r2.
    //    /// </summary>
    //    /// <param name="r1"></param>
    //    /// <param name="r2"></param>
    //    /// <returns></returns>
    //    public static Rotation Division(Rotation r1, Rotation r2)
    //    {
    //        Rotation a = new Rotation(r1),
    //                 b = new Rotation(r2);
    //        b.Invert();
    //        a.Multiply(b);
    //        return a;
    //    }

    //    /// <summary>
    //    /// Turns this Rotation into its conjugate.
    //    /// </summary>
    //    public void Conjugate()
    //    {
    //        // W stays the same
    //        this.X = -this.X;
    //        this.Y = -this.Y;
    //        this.Z = -this.Z;
    //    }

    //    /// <summary>
    //    /// Returns the conjugate of given quaternion.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    /// <returns></returns>
    //    public static Rotation Conjugate(Rotation r)
    //    {
    //        return new Rotation(r.W, -r.X, -r.Y, -r.Z, true);
    //    }

    //    /// <summary>
    //    /// Inverts this quaternion.
    //    /// </summary>
    //    public void Invert()
    //    {
    //        if (this.IsUnit())
    //        {
    //            // The inverse of unit vectors is its conjugate
    //            this.X = -this.X;
    //            this.Y = -this.Y;
    //            this.Z = -this.Z;
    //        }
    //        else if (this.IsZero())
    //        {
    //            // The inverse of a zero quat is itself
    //        }
    //        else
    //        {
    //            // The inverse of a quaternion is its conjugate divided by the sqaured norm.
    //            double sqlen = this.SqLength();

    //            this.W /= sqlen;
    //            this.X /= -sqlen;
    //            this.Y /= -sqlen;
    //            this.Z /= -sqlen;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns a CoordinateSystem representation of current Quaternion
    //    /// (a 3x3 rotation matrix).
    //    /// </summary>
    //    /// <seealso cref="https://en.wikipedia.org/wiki/Rotation_formalisms_in_three_dimensions#Conversion_formulae_between_formalisms"/>
    //    /// <returns></returns>
    //    public CoordinateSystem GetCoordinateSystem()
    //    {
    //        // From gl-matrix.js
    //        double x2 = this.X + this.X,
    //               y2 = this.Y + this.Y,
    //               z2 = this.Z + this.Z;

    //        double xx2 = this.X * x2,
    //               yx2 = this.Y * x2,
    //               yy2 = this.Y * y2,
    //               zx2 = this.Z * x2,
    //               zy2 = this.Z * y2,
    //               zz2 = this.Z * z2,
    //               wx2 = this.W * x2,
    //               wy2 = this.W * y2,
    //               wz2 = this.W * z2;

    //        // @TODO: review the order of these elements, they might be transposed for webgl conventions
    //        return CoordinateSystem.FromComponents(
    //            1 - yy2 - zz2, yx2 + wz2, zx2 - wy2,
    //                yx2 - wz2, 1 - xx2 - zz2, zy2 + wx2,
    //                zx2 + wy2, zy2 - wx2, 1 - xx2 - yy2);
    //    }

    //    /// <summary>
    //    /// Returns the inverse of given quaternion.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    /// <returns></returns>
    //    public static Rotation Inverse(Rotation r)
    //    {
    //        Rotation rot = new Rotation(r);
    //        rot.Invert();
    //        return rot;
    //    }

    //    /// <summary>
    //    /// Returns the rotation vector in axis-angle representation, 
    //    /// i.e. the unit vector defining the rotation axis multiplied by the 
    //    /// angle rotation scalar in degrees.
    //    /// https://en.wikipedia.org/wiki/Axis%E2%80%93angle_representation
    //    /// </summary>
    //    /// <returns></returns>
    //    public Vector GetRotationVector(bool radians)
    //    {
    //        Vector axisAng = GetRotationAxis() * GetRotationAngle();
    //        if (radians)
    //        {
    //            axisAng.Scale(TO_RADS);
    //        }

    //        return axisAng;
    //    }

    //    public Vector GetRotationVector()
    //    {
    //        return GetRotationVector(false);
    //    }


    //    /// <summary>
    //    /// Returns the rotation axis represented by this Quaternion. 
    //    /// Note it will always return the unit vector corresponding to a positive rotation, 
    //    /// even if the quaternion was created from a negative one (flipped vector).
    //    /// </summary>
    //    /// <returns></returns>
    //    public Vector GetRotationAxis()
    //    {
    //        double theta2 = 2 * Math.Acos(W);

    //        // If angle == 0, no rotation is performed and this Quat is identity
    //        if (theta2 < EPSILON)
    //        {
    //            return new Vector();
    //        }

    //        double s = Math.Sin(0.5 * theta2);
    //        return new Vector(this.X / s, this.Y / s, this.Z / s);
    //    }

    //    /// <summary>
    //    /// Returns the rotation angle represented by this Quaternion in degrees.
    //    /// Note it will always yield the positive rotation.
    //    /// </summary>
    //    /// <returns></returns>
    //    public double GetRotationAngle()
    //    {
    //        double theta2 = 2 * Math.Acos(W);
    //        return theta2 < EPSILON ? 0 : Math.Round(theta2 * TO_DEGS, EPSILON_DECIMALS);
    //    }


    //    /// <summary>
    //    /// Rotate this Quaternion by specified Rotation around GLOBAL reference system.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    /// <returns></returns>
    //    public void RotateGlobal(Rotation r)
    //    {
    //        this.PreMultiply(r);
    //    }

    //    /// <summary>
    //    /// Rotate this Quaternion by specified Rotation around LOCAL reference system.
    //    /// </summary>
    //    /// <param name="r"></param>
    //    /// <returns></returns>
    //    public void RotateLocal(Rotation r)
    //    {
    //        this.Multiply(r);
    //    }

    //    /// <summary>
    //    /// Returns the Euler angle representation of this Quaternion in Tait-Bryant convention (ZY'X'' order, aka intrinsic ZYX). Please note each axis rotation is stored in its own XYZ parameter. So for example, to convert this to the ABC representation used by KUKA, ABC maps to the vector's ZYX.
    //    /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Conversion
    //    /// </summary>
    //    /// <returns></returns>
    //    public Vector ToEulerZYX()
    //    {
    //        Vector eu = new Vector();

    //        double y2, t0, t1, t2, t3, t4;

    //        y2 = Y * Y;

    //        t0 = 2 * (W * X + Y * Z);
    //        t1 = 1 - 2 * (X * X + y2);
    //        eu.X = Math.Atan2(t0, t1) * TO_DEGS;

    //        t2 = 2 * (W * Y - Z * X);
    //        t2 = t2 > 1 ? 1 : t2;
    //        t2 = t2 < -1 ? -1 : t2;
    //        eu.Y = Math.Asin(t2) * TO_DEGS;

    //        t3 = 2 * (W * Z + X * Y);
    //        t4 = 1 - 2 * (y2 + Z * Z);
    //        eu.Z = Math.Atan2(t3, t4) * TO_DEGS;

    //        return eu;
    //    }




    //    public override string ToString()
    //    {
    //        return string.Format("[{0},{1},{2},{3}]",
    //            Math.Round(W, STRING_ROUND_DECIMALS_RADS),
    //            Math.Round(X, STRING_ROUND_DECIMALS_RADS),
    //            Math.Round(Y, STRING_ROUND_DECIMALS_RADS),
    //            Math.Round(Z, STRING_ROUND_DECIMALS_RADS));
    //    }

    //}
}
