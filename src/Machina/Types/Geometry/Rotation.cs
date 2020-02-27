using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types.Geometry
{
    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   

    /// <summary>
    /// Represents a rotation in three-dimensional space.
    /// This rotation is usually defined by its rotation axis and angles, although other
    /// definition inputs are possible, such as Quaternions, Rotation Matrices or
    /// Euler Angles (Yaw-Pitch-Roll).
    /// </summary>
    public class Rotation : Geometry
    {

        // NOTE: this class is basically a wrapper containing both the AxisAngle and Quaternion 
        // representations of a rotation in space. While AA could have been implemented directly, 
        // The purpose of this class is to present a more intuitive API for the user to express
        // rotations in three-dimensional space, and handle the complexities of their mathematical
        // meaning underneath.


        /// <summary>
        /// An empty Rotation representing the orientation of the global XYZ coordinate system.
        /// </summary>
        public static Rotation GlobalXYZ => new Rotation(0, 0, 0, 0, false);

        /// <summary>
        /// A Rotation of 180 degs around the X axis.
        /// </summary>
        public static Rotation FlippedAroundX => new Rotation(1, 0, 0, 180, false);

        /// <summary>
        /// A Rotation of 180 degs around the Y axis. 
        /// This is the most common orientation of the coordiante system of the flange of a robot in 'home' position.
        /// </summary>
        public static Rotation FlippedAroundY => new Rotation(0, 1, 0, 180, false);

        /// <summary>
        /// A Rotation of 180 degs around the Z axis. 
        /// </summary>
        public static Rotation FlippedAroundZ => new Rotation(0, 0, 1, 180, false);




        /// <summary>
        /// Internal AxisAngle representation of this rotation. Used as high-level representation.
        /// </summary>
        public AxisAngle AA = null;

        /// <summary>
        /// Internal Quaternion representation of this rotation. Used for computations.
        /// </summary>
        public Quaternion Q = null;

        /// <summary>
        /// The axis vector around which this rotation revolves.
        /// </summary>
        public Vector Axis { get { return this.AA.Axis; } internal set { this.AA.Axis = value; } }

        /// <summary>
        /// The rotation angle in degrees. 
        /// </summary>
        public double Angle { get { return this.AA.Angle; } internal set { this.AA.Angle = value; } }


        /// <summary>
        /// Implicit conversion from Orientation to Rotation via its Quaternion.
        /// </summary>
        /// <param name="or"></param>
        public static implicit operator Rotation(Orientation or)
        {
            return new Rotation(or.Q);
        }


        /// <summary>
        /// Create an empty rotation object with no initialized fields.
        /// </summary>
        public Rotation()
            : this(0, 0, 0, 0, false) { }

        /// <summary>
        /// Create a Rotation as a shallow copy of another one. 
        /// </summary>
        /// <param name="r"></param>
        public Rotation(Rotation r) 
            : this(r.AA.X, r.AA.Y, r.AA.Z, r.Angle, false) { }  // the previous rotation should already be normalized?

        /// <summary>
        /// Create a rotation of 'angle' degrees around the 'axis' vector.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angleDegrees"></param>
        public Rotation(Vector axis, double angleDegrees)
            : this(axis.X, axis.Y, axis.Z, angleDegrees, true) { }

        /// <summary>
        /// Creates a rotation around a vector axis with XYZ components with the angle
        /// defined in degrees and right-hand rule. The axis will be automatically
        /// normalized. 
        /// </summary>
        /// <param name="axisX"></param>
        /// <param name="axisY"></param>
        /// <param name="axisZ"></param>
        /// <param name="angleDegrees"></param>
        public Rotation(double axisX, double axisY, double axisZ, double angleDegrees)
            : this(axisX, axisY, axisZ, angleDegrees, true) { }

        /// <summary>
        /// Main internal constructor.
        /// </summary>
        /// <param name="axisX"></param>
        /// <param name="axisY"></param>
        /// <param name="axisZ"></param>
        /// <param name="angleDegrees"></param>
        /// <param name="normalize"></param>
        internal Rotation(double axisX, double axisY, double axisZ, double angleDegrees, bool normalize)
        {
            this.AA = new AxisAngle(axisX, axisY, axisZ, angleDegrees, normalize);
            this.UpdateQuaternion();
        }

        /// <summary>
        /// Internal constructor from a normalized Quaternion object.
        /// </summary>
        /// <param name="q"></param>
        internal Rotation(Quaternion q)
        {
            this.Q = new Quaternion(q.W, q.X, q.Y, q.Z, false);
            this.UpdateAxisAngle();
        }

        /// <summary>
        /// Create a Rotation from its Quaternion values. 
        /// This is a static method because it 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Rotation FromQuaternion(double w, double x, double y, double z)
        {
            Rotation r = new Rotation();
            r.Q = new Quaternion(w, x, y, z, true);
            r.UpdateAxisAngle();
            return r;
        }




        /// <summary>
        /// Rotate this rotation using Global (extrinsic) coordinate system. 
        /// Internally, this rotation is pre-multiplied by the given one. 
        /// </summary>
        /// <param name="r"></param>
        public void RotateGlobal(Rotation r)
        {
            this.Q.PreMultiply(r.Q);
            this.UpdateAxisAngle();
        }

        /// <summary>
        /// Rotate this rotation using Local (intrinsic) coordinate system. 
        /// Internally, this rotation is post-multiplied by the given one. 
        /// </summary>
        /// <param name="r"></param>
        public void RotateLocal(Rotation r)
        {
            this.Q.Multiply(r.Q);
            this.UpdateAxisAngle();
        }

        /// <summary>
        /// Invert this rotation to negative angle around the same axis.
        /// </summary>
        public void Invert()
        {
            this.AA.Angle *= -1;
            this.Q.Conjugate();
        }

        /// <summary>
        /// Combine the effect of two Rotations. 
        /// Please note that rotations will be applied in the order specified by the arguments
        /// in intrinsic coordinates (post-multiply), i.e. r1, then r2 over the new LOCAL transformed coordinate system.
        /// This means that, if you want to rotate a Rotation A with another Rotation B in GLOBAL coordinates, 
        /// you will need to pre-multiply the rotations as in: Combine(B, A).
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Combine(Rotation r1, Rotation r2)
        {
            Rotation r = new Rotation(r1.Q);
            r.RotateLocal(r2);
            return r;
        }

        /// <summary>
        /// Rotate r1 over r2 in GLOBAL coordinates. 
        /// This is an alias for Rotation.Combine(r2, r1), see Rotation.Combine() for more details.
        /// @TODO: optimize, can probably do this with a direct Quaternion pre-multiplication.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static Rotation Global(Rotation r1, Rotation r2)
        {
            return Combine(r2, r1);
        }

        /// <summary>
        /// Rotate r1 with r2 in LOCAL coordinates. 
        /// This is an alias for Rotation.Combine(r1, r2), see Rotation.Combine() for more details.
        /// @TODO: optimize, can probably do this with a direct Quaternion post-multiplication.
        /// </summary>
        public static Rotation Local(Rotation r1, Rotation r2)
        {
            return Combine(r1, r2);
        }

        /// <summary>
        /// Return a rotation around the same axis but negative angle.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rotation Inverse(Rotation r)
        {
            Rotation rinv = new Rotation(r);
            rinv.Invert();
            return rinv;
        }



        ///// <summary>
        ///// Combine the effect of several Rotations. Please note that rotations will be successively
        ///// applied in the specified order in LOCAL (intrinsic) coordinates. See <see cref="Combine(Rotation, Rotation)"/>
        ///// for more information.
        ///// </summary>
        ///// <param name="rotations"></param>
        ///// <returns></returns>
        //public static Rotation Combine(params Rotation[] rotations)
        //{
        //    if (rotations.Length == 0)
        //    {
        //        return new Rotation();
        //    }

        //    Quaternion q = new Quaternion(rotations[0].Q);
        //    for (int i = 1; i < rotations.Length; i++)
        //    {
        //        q.Multiply(rotations[i].Q);
        //    }

        //    return Rotation.FromQuaternion(q);
        //}


        /// <summary>
        /// Update the Quaternion from the AxisAngle value.
        /// </summary>
        internal void UpdateQuaternion()
        {
            this.Q = this.AA.ToQuaternion();
        }

        internal void UpdateAxisAngle()
        {
            this.AA = this.Q.ToAxisAngle();
        }


        /// <summary>
        /// Return a RotationVector representation of this Rotation.
        /// </summary>
        /// <returns></returns>
        internal RotationVector GetRotationVector()
        {
            return GetRotationVector(false);
        }

        /// <summary>
        /// Return a RotationVector representation of this Rotation.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        internal RotationVector GetRotationVector(bool radians)
        {
            return this.AA.ToRotationVector(radians);
        }

        /// <summary>
        /// Returns an Orientation representation of this Rotation. 
        /// </summary>
        /// <returns></returns>
        public Orientation ToOrientation()
        {
            return new Orientation(this);
        }


        public override string ToString()
        {
            return this.ToString(false);
        }
        
        public string ToString(bool labels)
        {

            return string.Format(CultureInfo.InvariantCulture,
                "{0}[{1}{2}, {3}{4}, {5}{6}, {7}{8}]",
                labels ? "Rotation" : "",
                labels ? "X:" : "",
                Math.Round(AA.X, STRING_ROUND_DECIMALS_VECTOR),
                labels ? "Y:" : "",
                Math.Round(AA.Y, STRING_ROUND_DECIMALS_VECTOR),
                labels ? "Z:" : "",
                Math.Round(AA.Z, STRING_ROUND_DECIMALS_VECTOR),
                labels ? "A:" : "",
                Math.Round(Angle, STRING_ROUND_DECIMALS_VECTOR));
        }

        public string ToArrayString()
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "[{0},{1},{2},{3}]",
                Math.Round(AA.X, STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(AA.Y, STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(AA.Z, STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(Angle, STRING_ROUND_DECIMALS_DEGS));
        }

    }
}
